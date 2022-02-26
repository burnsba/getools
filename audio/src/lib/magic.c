#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <gsl/gsl_linalg.h>
#include <gsl/gsl_sort.h>
#include <gsl/gsl_statistics.h>
#include "debug.h"
#include "common.h"
#include "utility.h"
#include "gaudio_math.h"
#include "magic.h"
#include "adpcm_aifc.h"

/**
 * This file contains code to process audio signals and generate a codebook
 * for use in AL_ADPCM_WAVE .aifc audio.
 * 
 * LU decomposition/solver is implemented via GNU Scientific Library.
*/

struct frame_data {
    size_t origin_frame;
    double norm;
    double *vec;
};

// forward declarations

static int get_bucket_from_frame(int current_frame, int num_buckets, int num_frames);
static struct frame_data* frame_data_new(size_t origin_frame, double norm, double *vec, size_t vec_length);
static void frame_data_free(struct frame_data *fd);

// end forward declarations

/**
 * Evaluates buffer of 16-bit audio samples to generate codebook.
 * @param buffer: Audio data to predict.
 * @param buffer_len: Length in bytes of the audio buffer.
 * @param buffer_encoding: Whether the audio is in little or big endian format.
 * @param threshold: Optional. Filters audio frames based on sum of squares of audio
 * samples within the frame. If not set, all valid frames will be accepted. 
 * @param order: Generate nth order predictors.
 * @param npredictors: The number of predictors to generate.
 * @returns: vorpal codebook materialized from the Hadopelagic zone.
*/
struct ALADPCMBook *estimate_codebook(
    uint8_t *buffer,
    size_t buffer_len,
    enum DATA_ENCODING buffer_encoding,
    struct codebook_threshold_parameters *threshold,
    int order,
    int npredictors)
{
    /**
     * where are you now:
     * 
     * There are three pieces to audio processing: encoding, decoding, resolving codebook.
     * This is the "resolving codebook" piece.
     * 
     * Background:
     * 
     * Predictive Coders are a class of algorithm that estimate future data from previous data.
     * The goal is to minimize the amount of data required to be transmitted, while minimizing
     * signal loss. For the N64, this translates to minimizing the amount of space used on ROM.
     * 
     * I believe the N64 compression algorithm has roots from differential speech processing
     * algorithms (DPCM). Speech audio would be sampled, compared against a previous sample to 
     * generate a predictor, then the predictor error against the model (expected sample) 
     * would be transmitted instead of the audio signal. The receiver would have a
     * predefined model to decode the signal.
     * 
     * The N64 audio algorithm is similar in that the prediction error against the
     * model is what ends up encoded. However, there is no predefined model/codebook.
     * This needs to be generated for each audio file (algorithm below). Industry codecs (e.g., G.728 CELP)
     * calculate / apply codebooks over narrow time slices, but the implementation for
     * N64 uses the same codebook for the entire audio file.
     * 
     * When generating the predictors, a select number are used as feedback into the
     * next audio frame's predictors. The number used as feedback is the "order"
     * (an n'th order predictive coder). 
     * 
     * The "Adaptive" part of Adaptive Differential Pulse Code Modulation (ADPCM)
     * comes from the encode/decode algorith, applying a scale that varies with
     * each audio frame. 
     * 
     * foot note: there are many related kinds of predictive coders: Linear Predictive 
     * Coders (LPC), Differential pulse-code modulation (DPCM), Adaptive Differential
     * Pulse Code Modulation (ADPCM), Code Excited Linear Predictor (CELP).
     * 
     * ---------------------------------------------------------------------------------------------
     * 
     * The goal of this algorithm is to create `npredictor` number of predictors that 
     * minimize square error when encoding the file. Each audio frame will choose
     * whichever predictor results in the smallest square error.
     * 
     * This is my best understanding of the codebook search algorithm:
     * 
     * create a `tally` container of length `order` (one for each npredictor)
     * 
     * Read the sound data 16 samples at a time (one frame). If there aren't 16 samples available then we're done.
     * For each frame read
     *     - Compute the auto-correlation vector (B) between the current frame and
     *       the previous.
     *     - If this is not silence, compute the auto-correlation
     *       coefficients for auto-correlation matrix (A).
     *     - Attempt to solve the auto-correlation matrix for transfer function coefficients x in Ax=B.
     * 
     *     - Calculate the kfroma from x. If these have any poles on or outside the unit
     *       circle, discard this frame. If this were an dynamic codebook, error
     *       correction might take place here (e.g, reflect into the unit circle), but
     *       since the codebook will apply to the entire sound it's ok to drop frames
     *       when solving for an efficient codebook.
     * 
     *     - force stability: Clamp kfroma between 1.0 - epsilon and -1.0 + epsilon
     *     - Compute the afromk from kfroma
     *     - compute the rfroma from afromk
     *     - add result to list of tallies
     * 
     * Compute stats on all collected frames (mean, median, min, max, ...)
     * 
     * Filter collected frames according to user defined threshold parameters.
     * 
     * For the remaining frames, split into buckets (one bucket is one predictor)
     *     - Divide number of frames by number of buckets to get frames_per_bucket
     *     - Fill buckets in order until it has frames_per_bucket number of frames
     *       then move to the next bucket.
     * 
     * Foreach bucket:
     *     - Normalize tally (divide by number of elements in bucket)
     *     - Do the durbin on tally to find reflection (aka predictor)
     *       coefficients, using last rfroma as model
     *     - force stability: Clamp between 1.0 - epsilon and -1.0 + epsilon
     *     - Compute the afromk.
     *     - Send results to codebook untangler (and add to total results codebook)
    */

    TRACE_ENTER(__func__)

    const int frame_buffer_s16_byte_size = FRAME_DECODE_BUFFER_LEN * sizeof(int16_t);
    const int frame_buffer_f64_byte_size = FRAME_DECODE_BUFFER_LEN * sizeof(double);
    const double epsilon = 1e-6;

    // declare
    int main_while_count;
    struct ALADPCMBook *result = NULL;
    int16_t *frame_buffer;
    double *frame_buffer_f64;
    double *previous_frame_buffer_f64;
    double *correlation_arr;
    double **correlation_mat;
    double *transfer_coefficients_arr;
    double *reflection_coefficients;
    double *ar_parameters;
    double *acf_r;
    double *tally;
    double **tally_container;
    int tally_counts[TABLE_MAX_PREDICTORS];
    int tally_index;
    double *predictor_coefficients;
    double norm;
    size_t frame_measure_size;
    size_t frame_measure_pos;
    size_t frame_measure_count;
    double *frame_measures;
    double frame_measures_median;
    enum CODEBOOK_THRESHOLD_MODE threshold_mode;
    double frame_measure_threshold_min = 0.0;
    double frame_measure_threshold_max = 1e300;
    double frame_measure_quantile_min = 0.0;
    double frame_measure_quantile_max = 1.0;
    size_t sound_data_pos;
    int ar_frame_count;
    int axb_solved;
    int stable;
    int i;
    struct LinkedList *ar_frames;
    struct LinkedListNode *node;

    // init
    frame_buffer = (int16_t *)malloc_zero(FRAME_DECODE_BUFFER_LEN, sizeof(int16_t));
    previous_frame_buffer_f64 = (double *)malloc_zero(FRAME_DECODE_BUFFER_LEN, sizeof(double));
    frame_buffer_f64 = (double *)malloc_zero(FRAME_DECODE_BUFFER_LEN, sizeof(double));
    correlation_arr = (double *)malloc_zero(order, sizeof(double));
    transfer_coefficients_arr = (double *)malloc_zero(order, sizeof(double));
    reflection_coefficients = (double *)malloc_zero(order, sizeof(double));
    ar_parameters = (double *)malloc_zero(order, sizeof(double));
    acf_r = (double *)malloc_zero(order, sizeof(double));
    predictor_coefficients = (double *)malloc_zero(order, sizeof(double));

    ar_frames = LinkedList_new();
    
    tally_container = (double **)malloc_zero(TABLE_MAX_PREDICTORS, sizeof(double*));

    for (i=0; i<TABLE_MAX_PREDICTORS; i++)
    {
        tally_container[i] = (double *)malloc_zero(order, sizeof(double));
    }

    memset(tally_counts, 0, TABLE_MAX_PREDICTORS * sizeof(int));

    // size is arbitrary, will be resized at runtime.
    frame_measure_count = 0;
    frame_measure_size = 10 * sizeof(double);
    frame_measures = (double *)malloc_zero(10, sizeof(double));

    correlation_mat = matrix_f64_new((size_t)order, (size_t)order);

    threshold_mode = THRESHOLD_MODE_DEFAULT_UNKOWN;

    // validate configuration

    if (threshold != NULL && threshold->mode != THRESHOLD_MODE_DEFAULT_UNKOWN)
    {
        if (threshold->mode == THRESHOLD_MODE_ABSOLUTE)
        {
            threshold_mode = THRESHOLD_MODE_ABSOLUTE;
            frame_measure_threshold_min = threshold->min;
            frame_measure_threshold_max = threshold->max;
        }
        else if (threshold->mode == THRESHOLD_MODE_QUANTILE)
        {
            threshold_mode = THRESHOLD_MODE_QUANTILE;
            frame_measure_quantile_min = threshold->min;
            frame_measure_quantile_max = threshold->max;

            if (frame_measure_quantile_min < 0 || frame_measure_quantile_min > 1.0)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> invalid threshold->min=%f, valid range is 0.0-1.0\n", threshold->min);
            }

            if (frame_measure_quantile_max < 0 || frame_measure_quantile_max > 1.0)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> invalid threshold->max=%f, valid range is 0.0-1.0\n", threshold->max);
            }
        }
        else
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> threshold->mode=%f not supported\n", threshold->mode);
        }
    }

    // more init

    sound_data_pos = 0;
    ar_frame_count = 0;
    main_while_count = 0;
    tally_index = 0;

    // read all the data.
    while (1)
    {
        main_while_count++;

        // read next frame.
        int sample_bytes_read = fill_16bit_buffer(frame_buffer, FRAME_DECODE_BUFFER_LEN, buffer, &sound_data_pos, buffer_len);

        // if there's not a full frame then exit the while loop.
        if (sample_bytes_read != frame_buffer_s16_byte_size)
        {
            break;
        }

        // convert endianess if needed
        if (buffer_encoding == DATA_ENCODING_MSB)
        {
            bswap16_chunk(frame_buffer, frame_buffer, FRAME_DECODE_BUFFER_LEN);
        }

        // cast int to double
        convert_s16_f64(frame_buffer, FRAME_DECODE_BUFFER_LEN, frame_buffer_f64);

        // measure quanitity of current frame.
        norm = autocorrelation_vector(previous_frame_buffer_f64, frame_buffer_f64, FRAME_DECODE_BUFFER_LEN, order, correlation_arr);

        frame_measure_pos = frame_measure_count * sizeof(double);
        frame_measure_size = dynamic_buffer_memcpy((void*)&norm, sizeof(double), (uint8_t**)&frame_measures, frame_measure_pos, frame_measure_size);
        frame_measure_count++;

        if (!(fabs(norm) > TABLE_SILENCE_THRESHOLD))
        {
            goto continue_while_frame_read;
        }

        scale_f64_array(correlation_arr, (size_t)order, -1.0);

        autocorrelation_matrix(previous_frame_buffer_f64, frame_buffer_f64, FRAME_DECODE_BUFFER_LEN, order, correlation_mat);

        axb_solved = lu_decomp_solve(correlation_mat, correlation_arr, (size_t)order, transfer_coefficients_arr);

        if (!axb_solved)
        {
            goto continue_while_frame_read;
        }

        stable = stable_kfroma(transfer_coefficients_arr, (size_t)order, reflection_coefficients);

        if (!stable)
        {
            goto continue_while_frame_read;
        }

        // force stability (move poles inside unit circle)
        clamp_inclusive_array_f64_epsilon(reflection_coefficients, (size_t)order, -1.0, 1.0, epsilon);

        afromk(reflection_coefficients, (size_t)order, ar_parameters);

        rfroma(ar_parameters, (size_t)order, acf_r);

        struct frame_data* fd = frame_data_new((size_t)main_while_count, norm, acf_r, (size_t)order);
        node = LinkedListNode_new();
        node->data = fd;
        LinkedList_append_node(ar_frames, node);

        ar_frame_count++;

continue_while_frame_read:

        // update previous frame buffer container
        memcpy(previous_frame_buffer_f64, frame_buffer_f64, frame_buffer_f64_byte_size);
    }

    gsl_sort(frame_measures, 1, frame_measure_count);
    frame_measures_median = gsl_stats_median_from_sorted_data(frame_measures, 1, frame_measure_count);

    if (g_verbosity >= 2)
    {
        printf("ar_frame_count: %d\n", ar_frame_count);

        double d;

        printf("frame measure mean, median, variance :\n");
        d = gsl_stats_mean(frame_measures, 1, frame_measure_count);
        printf("%14g, ", d);
        // determined above
        printf("%14g, ", frame_measures_median);
        d = gsl_stats_variance(frame_measures, 1, frame_measure_count);
        printf("%14g ", d);
        printf("\n");

        printf("frame measure min, max :\n");
        d = gsl_stats_min(frame_measures, 1, frame_measure_count);
        printf("%14g, ", d);
        d = gsl_stats_max(frame_measures, 1, frame_measure_count);
        printf("%14g ", d);
        printf("\n");

        printf("frame measure quantile 0.2, 0.4, 0.6, 0.8 :\n");
        d = gsl_stats_quantile_from_sorted_data(frame_measures, 1, frame_measure_count, 0.2);
        printf("%14g, ", d);
        d = gsl_stats_quantile_from_sorted_data(frame_measures, 1, frame_measure_count, 0.4);
        printf("%14g, ", d);
        d = gsl_stats_quantile_from_sorted_data(frame_measures, 1, frame_measure_count, 0.6);
        printf("%14g, ", d);
        d = gsl_stats_quantile_from_sorted_data(frame_measures, 1, frame_measure_count, 0.8);
        printf("%14g ", d);
        printf("\n");
    }

    // Apply threshold filtering
    if (threshold_mode == THRESHOLD_MODE_ABSOLUTE || threshold_mode == THRESHOLD_MODE_QUANTILE)
    {
        size_t filter_remove_count = 0;

        if (threshold_mode == THRESHOLD_MODE_QUANTILE)
        {
            frame_measure_threshold_min = gsl_stats_median_from_sorted_data(frame_measures, 1, frame_measure_quantile_min);
            frame_measure_threshold_max = gsl_stats_median_from_sorted_data(frame_measures, 1, frame_measure_quantile_max);
        }

        if (g_verbosity >= VERBOSE_DEBUG)
        {
            printf("threshold filtering: min=%g max=%g\n", frame_measure_threshold_min, frame_measure_threshold_max);
        }

        node = ar_frames->head;
        while (node != NULL)
        {
            struct LinkedListNode *next_node;
            struct frame_data* fd = node->data;

            next_node = node->next;

            if (fd != NULL)
            {
                if (fd->norm > frame_measure_threshold_max || fd->norm < frame_measure_threshold_min)
                {
                    frame_data_free(fd);
                    LinkedListNode_free(ar_frames, node);
                    filter_remove_count++;
                }
            }

            node = next_node;
        }

        if (g_verbosity >= 2)
        {
            printf("threshold filtering removed %ld frames\n", filter_remove_count);
        }
    }

    /**
     * The main while loop may have filtered out frames due to norm threshold
     * or pole errors, etc. Now that the total number of frames is known,
     * split an (approximately) equal number of frames into each predictor
     * tally. This just splits into buckets sequentially (i.e., first half
     * of frames end up in first bucket, second half in second, etc).
    */
    size_t captures = ar_frames->count;
    size_t capture_index = 0;
    node = ar_frames->head;
    while (node != NULL)
    {
        struct frame_data* fd = node->data;
        if (fd != NULL)
        {
            tally_index = get_bucket_from_frame(capture_index, npredictors, captures);
            tally = tally_container[tally_index];

            for (i=0; i<order; i++)
            {
                tally[i] += fd->vec[i];
            }

            tally_counts[tally_index]++;
        }

        capture_index++;
        node = node->next;
    }

    result = ALADPCMBook_new(order, npredictors);

    // for each predictor, translate coefficients into a codebook entry
    for (tally_index=0; tally_index<npredictors; tally_index++)
    {
        int j;

        tally = tally_container[tally_index];

        for (j=0; j<order; j++)
        {
            tally[j] /= (double)tally_counts[tally_index];
        }

        levinson_durbin_recursion(tally, (size_t)order, predictor_coefficients);

        // force stability (move poles inside unit circle)
        clamp_inclusive_array_f64_epsilon(predictor_coefficients, (size_t)order, -1.0, 1.0, epsilon);

        afromk(predictor_coefficients, (size_t)order, ar_parameters);

        double *row = codebook_row_from_predictors(ar_parameters, (size_t)order);

        ALADPCMBook_set_predictor(result, row, tally_index);

        free(row);
    }

    // cleanup

    node = ar_frames->head;
    while (node != NULL)
    {
        struct frame_data *fd = node->data;
        if (fd != NULL)
        {
            frame_data_free(fd);
        }
        node->data = NULL;

        node = node->next;
    }
    LinkedList_free(ar_frames);

    free(frame_buffer);
    free(previous_frame_buffer_f64);
    free(frame_buffer_f64);
    free(correlation_arr);
    free(transfer_coefficients_arr);
    free(reflection_coefficients);
    free(ar_parameters);
    free(acf_r);
    free(predictor_coefficients);
    free(frame_measures);

    for (i=0; i<TABLE_MAX_PREDICTORS; i++)
    {
        free(tally_container[i]);
    }

    free(tally_container);

    matrix_f64_free(correlation_mat, (size_t)order);

    TRACE_LEAVE(__func__)

    return result;
}

/**
 * Computes autocorrelation at lag n.
 * Does this for every lag 1-n, and stores result as vector.
 * @param previous: previous frame to compare against.
 * @param current: current frame.
 * @param len: length of frame in number of elements.
 * @param lag: lag (order).
 * @param result: Out. Contains autocorrelations. Result[0] is lag 1, result[1] is lag 2, etc. Must be previously allocated.
 * @returns: lag 0 result (inner product with self).
*/
double autocorrelation_vector(double *previous, double *current, size_t len, int lag, double *result)
{
    TRACE_ENTER(__func__)

    if (previous == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> previous is NULL\n", __func__, __LINE__);
    }

    if (current == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> current is NULL\n", __func__, __LINE__);
    }

    if (result == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> result is NULL\n", __func__, __LINE__);
    }

    if (lag < 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> invalid lag: %d\n", __func__, __LINE__, lag);
    }

    if (lag > TABLE_MAX_ORDER)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> invalid lag: %d, exceeds max value of \n", __func__, __LINE__, lag, TABLE_MAX_ORDER);
    }

    int out_index;
    int s1;
    int s2;
    int int_len = (int)len;
    double self;

    memset(result, 0, lag * sizeof(double));

    self = 0;

    for (s2 = 0; s2 < int_len; s2++)
    {
        self += current[s2] * current[s2];
    }

    for (out_index = 0; out_index < lag; out_index++)
    {
        for (s2 = 0; s2 < int_len; s2++)
        {
            s1 = s2 - out_index - 1;

            if (s1 < 0)
            {
                s1 += int_len;
                result[out_index] += previous[s1] * current[s2];
            }
            else
            {
                result[out_index] += current[s1] * current[s2];
            }
        }
    }

    TRACE_LEAVE(__func__)
    return self;
}

/**
 * Auto correlation matrix.
 * @param previous: previous frame to compare against.
 * @param current: current frame.
 * @param len: length of frame in number of elements.
 * @param lag: lag (order).
 * @param result: Out. Matrix of size {@code lag} x {@code lag}. Must be previously allocated.
*/
void autocorrelation_matrix(double *previous, double *current, size_t len, int lag, double **result)
{
    TRACE_ENTER(__func__)

    if (previous == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> previous is NULL\n", __func__, __LINE__);
    }

    if (current == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> current is NULL\n", __func__, __LINE__);
    }

    if (result == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> result is NULL\n", __func__, __LINE__);
    }

    if (lag < 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> invalid lag: %d\n", __func__, __LINE__, lag);
    }

    int int_len;
    int out_row, out_col;
    int s1;
    int s2;
    int i;

    int_len = (int) len;

    for (out_row=0; out_row<lag; out_row++)
    {
        memset(result[out_row], 0, lag * sizeof(double));
    }

    for (out_row = 0; out_row < lag; out_row++)
    {
        for (out_col = 0; out_col < lag; out_col++)
        {
            // ignore self, start with lag
            for (i = -1; i < int_len - 1; i++)
            {
                double s1_val;
                double s2_val;

                s1 = i - out_row;
                s2 = i - out_col;

                if (s1 < 0)
                {
                    s1_val = previous[int_len + s1];
                }
                else
                {
                    s1_val = current[s1];
                }

                if (s2 < 0)
                {
                    s2_val = previous[int_len + s2];
                }
                else
                {
                    s2_val = current[s2];
                }

                //printf("out[%d][%d] += in[%d] * in[%d]; ---> %f += %f * %f \n", out_row, out_col, s1, s2, result[out_row][out_col], s1_val, s2_val);
                result[out_row][out_col] += s1_val * s2_val;
            }
        }
    }

    TRACE_LEAVE(__func__)
}

/**
 * Given matrix A and vector B, solves Ax=B for x using LU decomposition.
 * @param a: matrix A.
 * @param b: vector B.
 * @param n: Size of matrix (nxn), and length of vector B, and length of vector x.
 * @param x: Out parameter. Will contain result.
 * @returns: 1 on success, 0 if system could not be solved.
*/
int lu_decomp_solve(double **a, double *b, size_t n, double *x)
{
    TRACE_ENTER(__func__)

    if (a == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> a is NULL\n", __func__, __LINE__);
    }

    if (b == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> b is NULL\n", __func__, __LINE__);
    }

    if (x == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> x is NULL\n", __func__, __LINE__);
    }

    memset(x, 0, n * sizeof(double));

    int result = 0;
    int sign;
    int status;
    size_t i,j;

    double *a_copy = (double *)malloc_zero(n*n, sizeof(double));

    // unroll matrix into array
    for (i=0; i<n; i++)
    {
        for (j=0; j<n; j++)
        {
            a_copy[i*n + j] = a[i][j];
        }
    }

    gsl_matrix_view gsl_m = gsl_matrix_view_array(a_copy, n, n);
    gsl_vector_view gsl_b = gsl_vector_view_array(b, n);
    gsl_vector *gsl_x = gsl_vector_alloc(n);
    gsl_permutation *gsl_p = gsl_permutation_alloc(n);

    status = gsl_linalg_LU_decomp(&gsl_m.matrix, gsl_p, &sign);

    if (status != GSL_SUCCESS)
    {
        result = 0;
        goto lu_decomp_solve_cleanup_return;
    }

    status = gsl_linalg_LU_solve(&gsl_m.matrix, gsl_p, &gsl_b.vector, gsl_x);

    if (status != GSL_SUCCESS)
    {
        result = 0;
        goto lu_decomp_solve_cleanup_return;
    }

    for (i=0; i<n; i++)
    {
        x[i] = gsl_vector_get(gsl_x, i);
    }

    result = 1;

    // printf ("x = \n");
    // gsl_vector_fprintf (stdout, gsl_x, "%g");

lu_decomp_solve_cleanup_return:
    gsl_permutation_free(gsl_p);
    gsl_vector_free(gsl_x);
    free(a_copy);

    TRACE_LEAVE(__func__)
    return result;
}

// I think this is part of Levinson-Durbin algorithm to compute reflection coefficients
/**
 * @returns: 1 if coefficients are stable (inside unit circle), zero otherwise.
*/
int stable_kfroma(double *parameters, size_t lag, double *reflection_coefficients)
{
    /**
    example run:

out[3] = copy[3];

i=2
temp[0] = (copy[-1] - copy[3] * out[3]) / (1 - out[3]^2);
temp[1] = (copy[0] - copy[2] * out[3]) / (1 - out[3]^2);
temp[2] = (copy[1] - copy[1] * out[3]) / (1 - out[3]^2);
temp[3] = (copy[2] - copy[0] * out[3]) / (1 - out[3]^2);
copy[0] = temp[0];
copy[1] = temp[1];
copy[2] = temp[2];
out[2] = temp[2];

i=1
temp[0] = (copy[-1] - copy[2] * out[2]) / (1 - out[2]^2);
temp[1] = (copy[0] - copy[1] * out[2]) / (1 - out[2]^2);
temp[2] = (copy[1] - copy[0] * out[2]) / (1 - out[2]^2);
copy[0] = temp[0];
copy[1] = temp[1];
out[1] = temp[1];

i=0
temp[0] = (copy[-1] - copy[1] * out[1]) / (1 - out[1]^2);
temp[1] = (copy[0] - copy[0] * out[1]) / (1 - out[1]^2);
copy[0] = temp[0];
out[0] = temp[0];
    */

    TRACE_ENTER(__func__)

    if (parameters == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> parameters is NULL\n", __func__, __LINE__);
    }

    if (reflection_coefficients == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> reflection_coefficients is NULL\n", __func__, __LINE__);
    }

    int result = 1;
    int int_len = (int)lag;
    int i, j, s1, s2;

    double *copy = (double *)malloc_zero(lag, sizeof(double));
    double *temp = (double *)malloc_zero(lag, sizeof(double));

    memset(reflection_coefficients, 0, lag * sizeof(double));

    for (i=0; i<int_len; i++)
    {
        copy[i] = parameters[i];
    }

    reflection_coefficients[int_len - 1] = copy[int_len - 1];

    for (i=int_len - 2; i >= 0; i--)
    {
        //printf("i=%d\n", i);

        s2 = i + 1;

        for (j=0; j<i+2; j++)
        {
            double d;
            double den;

            s1 = j - 1;

            if (s1 < 0)
            {
                d = 1.0;
            }
            else
            {
                d = copy[s1];
            }

            den = 1.0 - (reflection_coefficients[s2] * reflection_coefficients[s2]);
            if (den == 0.0)
            {
                //printf("exit early\n");
                result = 0;
                goto cleanup_return;
            } 
            
            //printf("temp[%d] = (d - (copy[%d] * reflection_coefficients[%d])) / den --> %f = (%f - (%f * %f)) / %f\n", j, s2-j,s2,temp[j],d,copy[s2 - j],reflection_coefficients[s2],den);
            temp[j] = (d - (copy[s2 - j] * reflection_coefficients[s2])) / den;
        }

        for (j=0; j<=i; j++)
        {
            //printf("copy[%d] = temp[%d]; --> %f = %f\n", j, j+1, copy[j],temp[j+1]);
            copy[j] = temp[j+1];
        }

        //printf("reflection_coefficients[%d] = copy[%d]; --> %f = %f\n", i, i, reflection_coefficients[i],copy[i]);
        reflection_coefficients[i] = copy[i];
        if (fabs(reflection_coefficients[i]) > 1.0)
        {
            //printf("exit early\n");
            result = 0;
            goto cleanup_return;
        }

        //printf("\n");
    }

cleanup_return:

    free(temp);
    free(copy);

    TRACE_LEAVE(__func__)
    return result;
}

/**
 * I think this is part of Levinson-Durbin algorithm resolving the j-th parameters of m-th order model
 * a_{j,k} = a_{j,m-1} + k_m * a_{m-j,m-1}
*/
void afromk(double *reflection_coefficients, size_t lag, double *ar_parameters)
{
    /**
     * example run:
out[0] = in[0]
out[1] = in[1]
out[2] = in[2]
out[3] = in[3]

out[0] += out[0] * out[1];

out[0] += out[1] * out[2];
out[1] += out[0] * out[2];

out[0] += out[2] * out[3];
out[1] += out[1] * out[3];
out[2] += out[0] * out[3];
    */
    TRACE_ENTER(__func__)

    if (reflection_coefficients == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> reflection_coefficients is NULL\n", __func__, __LINE__);
    }

    if (ar_parameters == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> ar_parameters is NULL\n", __func__, __LINE__);
    }

    memset(ar_parameters, 0, lag * sizeof(double));

    int int_len = (int)lag;
    int i,j;
    int s2;

    for (i=0; i<int_len; i++)
    {
        ar_parameters[i] = reflection_coefficients[i];
    }

    for (i=0; i<int_len - 1; i++)
    {
        s2 = i + 1;

        for (j = 0; j <= i; j++)
        {
            ar_parameters[j] += ar_parameters[i - j] * ar_parameters[s2];
        }
    }

    TRACE_LEAVE(__func__)
}

// I think this is related to the Levinson-Durbin algorithm, and it recovers the autocorrelation function (R) from the "parameters"
void rfroma(double *ar_parameters, size_t lag, double *acf_r)
{
    /**
     * quote: 
     *     The Levinson-Durbin algorithm transforms an autocorrelation function to the autoregressive
     *     parameters. The reflection coefficients show up as an intermediate set of coefficients. The
     *     parameters, the reflection coefficients and the autocorrelation function (normalised with
     *     respect to 
     * - "Autoregressive Modelling for Speech Coding: Estimation, Interpolation and Quantisation"
     * J. S. Erkelens
     * page 36
    */
    /**
     * example run:
     * 
temp[3][0] = -in[0];
temp[3][1] = -in[1];
temp[3][2] = -in[2];
temp[3][3] = -in[3];

temp[2][0] = (temp[3][2] * temp[3][3] + temp[3][0]) / (1.0 - temp[3][3] * temp[3][3]);
temp[2][1] = (temp[3][1] * temp[3][3] + temp[3][1]) / (1.0 - temp[3][3] * temp[3][3]);
temp[2][2] = (temp[3][0] * temp[3][3] + temp[3][2]) / (1.0 - temp[3][3] * temp[3][3]);
temp[1][0] = (temp[2][1] * temp[2][2] + temp[2][0]) / (1.0 - temp[2][2] * temp[2][2]);
temp[1][1] = (temp[2][0] * temp[2][2] + temp[2][1]) / (1.0 - temp[2][2] * temp[2][2]);
temp[0][0] = (temp[1][0] * temp[1][1] + temp[1][0]) / (1.0 - temp[1][1] * temp[1][1]);

out[0] += temp[0][0] *    1.0;
out[1] += temp[1][0] * out[0];
out[1] += temp[1][1] *    1.0;
out[2] += temp[2][0] * out[1];
out[2] += temp[2][1] * out[0];
out[2] += temp[2][2] *    1.0;
out[3] += temp[3][0] * out[2];
out[3] += temp[3][1] * out[1];
out[3] += temp[3][2] * out[0];
out[3] += temp[3][3] *    1.0;
    */
    TRACE_ENTER(__func__)

    if (ar_parameters == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> ar_parameters is NULL\n", __func__, __LINE__);
    }

    if (acf_r == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> acf_r is NULL\n", __func__, __LINE__);
    }

    memset(acf_r, 0, lag * sizeof(double));

    double **temp = matrix_f64_new(lag, lag);

    int int_len = (int)lag;
    int i,j;

    for (i=0; i<int_len; i++)
    {
        //printf("temp[%d][%d] = -ar_parameters[%d]; --> = %f\n", int_len - 1, i, i, -ar_parameters[i]);
        temp[int_len - 1][i] = -ar_parameters[i];
    }
    
    for (i = int_len - 1; i> 0; i--)
    {
        double den = 1.0 - (temp[i][i] * temp[i][i]);
        //printf("den = %f\n", den);

        for (j = 0; j < i; j++)
        {
            if (den != 0)
            {
                //printf("temp[%d][%d] = (temp[%d][%d] * temp[%d][%d] + temp[%d][%d]) / den; --> = (%f * %f + %f) / %f\n", i-1,j,i,i-j-1,i,i,i,j,temp[i][i-j-1],temp[i][i],temp[i][j],den);
                temp[i-1][j] = (temp[i][i-j-1] * temp[i][i] + temp[i][j]) / den;
            }
            else
            {
                temp[i-1][j] = 0;
            }
        }
    }

    // printf("rfroma mat: \n");
    // for (i=0; i<int_len; i++)
    // {
    //     for (j=0; j<int_len; j++)
    //     {
    //         printf("%8.04f ", temp[i][j]);
    //     }
    //     printf("\n");
    // }
    // printf("\n");

    for (i=0; i<int_len; i++)
    {
        for (j=0; j<=i; j++)
        {
            double d;

            int s2 = i - j - 1;
            if (s2 < 0)
            {
                d = 1.0;
            }
            else
            {
                d = acf_r[s2];
            }

            //printf("acf_r[%d] = temp[%d][%d] * d; --> = %f * %f\n", i,i,j,temp[i][j],d);
            //printf("acf_r[%d]: %f --> ", i, acf_r[i]);
            acf_r[i] += temp[i][j] * d;
            //printf("%f\n", acf_r[i]);
        }
    }

    matrix_f64_free(temp, lag);
    
    TRACE_LEAVE(__func__)
}

/**
 * Levnion-Durbin recursion to obtain reflection coefficients from autocorrelation function (R) values.
 * Potentially unstable (ignores poles outside unit circle).
*/
void levinson_durbin_recursion(double *acf_r, size_t lag, double *reflection_coefficients)
{
    /**
     * example run:
div = 1.0;

sum = 0.0
if (div > 0.0)
    temp[0] = -(in[0] + sum) / div;
out[0] = temp[0];
div *= 1.0 - temp[0] * temp[0];

sum = 0.0
sum += temp[0] * in[0];
if (div > 0.0)
    temp[0] = -(in[0] + sum) / div;
out[1] = temp[1];
temp[0] += temp[0] * temp[1]
div *= 1.0 - temp[1] * temp[1];

sum = 0.0
sum += temp[0] * in[1];
sum += temp[1] * in[0];
if (div > 0.0)
    temp[0] = -(in[0] + sum) / div;
out[2] = temp[2];
temp[0] += temp[1] * temp[2]
temp[1] += temp[0] * temp[2]
div *= 1.0 - temp[2] * temp[2];

sum = 0.0
sum += temp[0] * in[2];
sum += temp[1] * in[1];
sum += temp[2] * in[0];
if (div > 0.0)
    temp[0] = -(in[0] + sum) / div;
out[3] = temp[3];
temp[0] += temp[2] * temp[3]
temp[1] += temp[1] * temp[3]
temp[2] += temp[0] * temp[3]
div *= 1.0 - temp[3] * temp[3];
    */
    TRACE_ENTER(__func__)

    if (acf_r == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> acf_r is NULL\n", __func__, __LINE__);
    }

    if (reflection_coefficients == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> reflection_coefficients is NULL\n", __func__, __LINE__);
    }

    int int_len = (int)lag;
    int i, j;
    double residual_variance;
    double sum;
    double *temp;

    temp = (double*)malloc_zero(lag, sizeof(double));
    memset(reflection_coefficients, 0, lag * sizeof(double));

    residual_variance = 1.0;

    for (i=0; i<int_len; i++)
    {
        sum = 0.0;

        for (j=0; j<i; j++)
        {
            sum += temp[j] * acf_r[i-j-1];
        }

        if (residual_variance > 0.0)
        {
            temp[i] = -(acf_r[i] + sum) / residual_variance;
        }
        else
        {
            temp[i] = 0.0;
        }

        for (j=0; j<i; j++)
        {
            temp[j] += temp[i-j-1] * temp[i];
        }

        residual_variance *= 1.0 - temp[i] * temp[i];

        reflection_coefficients[i] = temp[i];
    }

    free(temp);

    TRACE_LEAVE(__func__)
}

/**
 * Converts reflection coefficients of length {@code lag} into
 * codebook row of length {@code 8 * lag}.
 * @param reflection_coefficients: values to convert.
 * @param lag: (order).
 * @returns: newly allocated array containing converted values. 
*/
double *codebook_row_from_predictors(double *reflection_coefficients, size_t lag)
{
    TRACE_ENTER(__func__)

    if (reflection_coefficients == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> reflection_coefficients is NULL\n", __func__, __LINE__);
    }

    int i,j,k;
    int int_len = (int)lag;
    int pos;

    double **temp = matrix_f64_new(FRAME_DECODE_ROW_LEN, FRAME_DECODE_ROW_LEN);

    // element count = order * npredictors * 16
    double *result = (double *)malloc_zero(lag * 1 * 16, sizeof(double));

    for (i = 0; i < int_len; i++)
    {
        for (j = i; j < int_len; j++)
        {
            //printf("temp[%d][%d] = -reflection_coefficients[%d]; --> = -(%f)\n", i,j,int_len - j + i-1,reflection_coefficients[int_len - j + i-1]);
            temp[i][j] = -reflection_coefficients[int_len - j + i - 1];
        }
    }

    for (i = 1; i < FRAME_DECODE_ROW_LEN; i++)
    {
        for (j = 0; j < int_len; j++)
        {
            if (i - j - 1 >= 0)
            {
                for (k = 0; k < int_len; k++)
                {
                    //printf("temp[%d][%d] -= reflection_coefficients[%d] * temp[%d][%d]; --> %f * %f\n", i,k,j,i-j- 1,k,reflection_coefficients[j],temp[i - j- 1][k]);
                    //printf("temp[%d][%d]: %f --> ", i, k, temp[i][k]);
                    temp[i][k] -= reflection_coefficients[j] * temp[i - j - 1][k];
                    //printf("%f\n", temp[i][k]);
                }
            }
        }
    }

    pos = 0;
    for (i = 0; i < int_len; i++)
    {
        for (j = 0; j < FRAME_DECODE_ROW_LEN; j++)
        {
            result[pos] = temp[j][i] * FRAME_DECODE_SCALE;
            pos++;
        }
    }

    matrix_f64_free(temp, FRAME_DECODE_ROW_LEN);

    TRACE_LEAVE(__func__)
    return result;
}

/**
 * Sets the n'th predictor in a codebook.
 * @param book: Container to set predictor.
 * @param row: Codebook row of values.
 * @param predictor_num: Zero based index of predictor to set.
*/
void ALADPCMBook_set_predictor(struct ALADPCMBook *book, double *row, int predictor_num)
{
    TRACE_ENTER(__func__)

    if (book == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> book is NULL\n", __func__, __LINE__);
    }

    int i;
    int book_pos;

    book_pos = book->order * FRAME_DECODE_ROW_LEN * predictor_num;

    for (i=0; i<(book->order * FRAME_DECODE_ROW_LEN); i++, book_pos++)
    {
        double d = clamp_f64(row[i], (double)(INT16_MIN), (double)(INT16_MAX));
        book->book[book_pos] = forward_quantize_f64(d, 1);
    }

    TRACE_LEAVE(__func__)
}

/**
 * For a given frame out of a number of frames and a number of buckets,
 * resolves which bucket this frame should go into.
 * @param current_frame: zero based index of current frame to resolve.
 * @param num_buckets: number of buckets.
 * @param num_frames: number of frames.
 * @returns: zero based index of bucket.
*/
static int get_bucket_from_frame(int current_frame, int num_buckets, int num_frames)
{
    TRACE_ENTER(__func__)

    if (num_buckets > TABLE_MAX_PREDICTORS)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> invalid num_buckets\n", __func__, __LINE__);
    }

    if (num_buckets <= 1)
    {
        return 0;
    }

    int step_size = num_frames / num_buckets;

    int bucket = 0;

    while (current_frame > step_size)
    {
        current_frame -= step_size;
        bucket++;
    }

    if (bucket >= num_buckets)
    {
        return num_buckets - 1;
    }

    return bucket;

    TRACE_LEAVE(__func__)
}

/**
 * Allocates new memory for a data container, and allocates memory for vector values.
 * @param origin_frame: nth frame from main loop.
 * @param norm: autocorrelation[0] value (inner product of frame with self).
 * @param vec: Copy of vector. Memory will be allocated and these will be copied
 * into the new container.
 * @param vec_length: Number of elements in {@code vec}.
 * @returns: pointer to new memory.
*/
static struct frame_data* frame_data_new(size_t origin_frame, double norm, double *vec, size_t vec_length)
{
    TRACE_ENTER(__func__)

    if (vec == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> vec is NULL\n", __func__, __LINE__);
    }

    size_t i;

    struct frame_data* fd = (struct frame_data*)malloc_zero(1, sizeof(struct frame_data));
    fd->vec = (double *)malloc_zero(vec_length, sizeof(double));

    fd->origin_frame = origin_frame;
    fd->norm = norm;

    for (i=0; i<vec_length; i++)
    {
        fd->vec[i] = vec[i];
    }

    TRACE_LEAVE(__func__)
    return fd;
}

/**
 * Frees memory associated with object.
 * @param fd: object to free.
*/
static void frame_data_free(struct frame_data *fd)
{
    TRACE_ENTER(__func__)

    if (fd == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (fd->vec != NULL)
    {
        free(fd->vec);
        fd->vec = NULL;
    }

    free(fd);
    
    TRACE_LEAVE(__func__)
}