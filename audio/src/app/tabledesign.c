#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <string.h>
#include <limits.h>
#include <getopt.h>
#include <errno.h>
#include "debug.h"
#include "machine_config.h"
#include "common.h"
#include "utility.h"
#include "llist.h"
#include "naudio.h"
#include "adpcm_aifc.h"
#include "magic.h"
#include "wav.h"

/**
 * This file contains main entry for tabledesign app.
 * 
 * This generates a codebook for use in .aifc audio.
*/

#define APPNAME "tabledesign"
#define VERSION "1.0"

static int opt_help_flag = 0;
static int opt_input_file = 0;
static int opt_output_file = 0;
static int opt_run_threshold_mode = 0;
static int opt_run_threshold_min = 0;
static int opt_run_threshold_max = 0;
static int opt_run_order = 0;
static int opt_run_predictors = 0;
static enum CODEBOOK_THRESHOLD_MODE threshold_mode = THRESHOLD_MODE_DEFAULT_UNKOWN;
static double run_threshold_min = 0;
static double run_threshold_max = 0;
static int run_order = TABLE_DEFAULT_LAG;
static int run_predictors = TABLE_DEFAULT_PREDICTORS;
static char *input_filename = NULL;
static size_t input_filename_len = 0;
static char *output_filename = NULL;
static size_t output_filename_len = 0;

#define LONG_OPT_DEBUG        1003
#define LONG_OPT_ORDER        2001

#define LONG_OPT_THRESHOLD_MODE        2101
#define LONG_OPT_THRESHOLD_MIN         2102
#define LONG_OPT_THRESHOLD_MAX         2103

#define TABLE_DESIGN_SUPPORTED_EXTENSIONS ".wav .aifc"
#define TABLE_SUPPORTED_THRESHOLD_MODES "a q"

static struct option long_options[] =
{
    {"help",       no_argument,      &opt_help_flag,   1  },
    {"in",         required_argument,              NULL,  'n' },
    {"out",        required_argument,              NULL,  'o' },
    {"order",      required_argument,              NULL,  LONG_OPT_ORDER },
    {"predictors", required_argument,              NULL,  'p' },

    {"threshold-mode", required_argument,      NULL,  LONG_OPT_THRESHOLD_MODE },
    {"threshold-min",  required_argument,      NULL,  LONG_OPT_THRESHOLD_MIN },
    {"threshold-max",  required_argument,      NULL,  LONG_OPT_THRESHOLD_MAX },

    {"quiet",        no_argument,               NULL,  'q' },
    {"verbose",      no_argument,               NULL,  'v' },
    {"debug",        no_argument,               NULL,   LONG_OPT_DEBUG },
    {NULL, 0, NULL, 0}
};

// forward declarations

void print_help(const char * invoke);
void read_opts(int argc, char **argv);

// end forward declarations

void print_help(const char * invoke)
{
    printf("%s %s help\n", APPNAME, VERSION);
    printf("\n");
    printf("Evaluates audio samples to create predictor codebook\n");
    printf("usage:\n");
    printf("\n");
    printf("    %s --in file\n", invoke);
    printf("\n");
    printf("options:\n");
    printf("\n");
    printf("    --help                        print this help\n");
    printf("    -n,--in=FILE                  input audio file to evaluate.\n");
    printf("                                  Supported file formats:\n");
    printf("                                  %s\n", TABLE_DESIGN_SUPPORTED_EXTENSIONS);
    printf("    -o,--out=FILE                 output file. Optional. If not provided, will\n");
    printf("                                  reuse the input file name but change extension.\n");
    printf("    --order=INT                   Number of lag samples. Default=%d, min=%d, max=%d\n", TABLE_DEFAULT_LAG, TABLE_MIN_ORDER, TABLE_MAX_ORDER);
    printf("    -p,--predictors=INT           Number of predictors. Default=%d, min=%d, max=%d\n", TABLE_DEFAULT_PREDICTORS, TABLE_MIN_PREDICTORS, TABLE_MAX_PREDICTORS);
    printf("    --threshold-mode=CHAR         Optional. Audio frame threshold filtering mode.\n");
    printf("                                  If not supplied then no filtering is performed.\n");
    printf("                                  Supported modes: %s.\n", TABLE_SUPPORTED_THRESHOLD_MODES);
    printf("    --threshold-min=DOUBLE        Threshold filter min value. Requries mode be set.\n");
    printf("    --threshold-max=DOUBLE        Threshold filter max value. Requries mode be set.\n");

    printf("    -q,--quiet                    suppress output\n");
    printf("    -v,--verbose                  more output\n");
    printf("\n");

    printf("threshold-mode = a\n");
    printf("\n");
    printf("    'a' = \"absolute\" value filtering.\n");
    printf("    Inner product of audio frame volume with itself is compared against threshold.\n");
    printf("    Only frames with values above min and below max are considered.\n");
    printf("    Frame value is compared directly against min and max.\n");
    printf("    At least one of min or max must be supplied.\n");
    printf("\n");

    printf("threshold-mode = q\n");
    printf("\n");
    printf("    'q' = \"quantile\" value filtering.\n");
    printf("    Inner product of audio frame volume with itself is compared against threshold.\n");
    printf("    All frames are first measured to gather quantile statistics.\n");
    printf("    The \"threshold-min\" parameter is interpreted as a quantile, between 0.0 and 1.0.\n");
    printf("    The \"threshold-max\" parameter is interpreted the same way.\n");
    printf("    Frame value is compared against the value at \"min\" and \"max\" quantile,\n");
    printf("    only frames within this value range are accepted.\n");
    printf("    At least one of min or max must be supplied.\n");
    printf("\n");

    printf("\n");
    fflush(stdout);
}

void read_opts(int argc, char **argv)
{
    int option_index = 0;
    int ch;

    while ((ch = getopt_long(argc, argv, "n:o:p:qv", long_options, &option_index)) != -1)
    {
        switch (ch)
        {
            case 'n':
            {
                opt_input_file = 1;

                input_filename_len = snprintf(NULL, 0, "%s", optarg);

                if (input_filename_len < 1)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, input filename not specified\n");
                }

                input_filename = (char *)malloc_zero(input_filename_len + 1, 1);
                input_filename_len = snprintf(input_filename, input_filename_len, "%s", optarg);
            }
            break;

            case 'o':
            {
                opt_output_file = 1;

                output_filename_len = snprintf(NULL, 0, "%s", optarg);

                if (output_filename_len < 1)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, output filename not specified\n");
                }

                output_filename = (char *)malloc_zero(output_filename_len + 1, 1);
                output_filename_len = snprintf(output_filename, output_filename_len, "%s", optarg);
            }
            break;

            case LONG_OPT_THRESHOLD_MODE:
            {
                opt_run_threshold_mode = 1;

                if (optarg != NULL && (optarg[0] == 'a' || optarg[0] == 'A'))
                {
                    threshold_mode = THRESHOLD_MODE_ABSOLUTE;
                }
                else if (optarg != NULL && (optarg[0] == 'q' || optarg[0] == 'Q'))
                {
                    threshold_mode = THRESHOLD_MODE_QUANTILE;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, invalid threshold mode: \"%s\". Supported values: %s\n", optarg, TABLE_SUPPORTED_THRESHOLD_MODES);
                }
            }
            break;

            case LONG_OPT_THRESHOLD_MIN:
            {
                double res;
                char *pend = NULL;

                opt_run_threshold_min = 1;

                res = strtod(optarg, &pend);
                
                if (pend != NULL && *pend == '\0')
                {
                    if (errno == ERANGE)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "error (range), cannot parse threshold as double: %s\n", optarg);
                    }

                    run_threshold_min = res;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, cannot parse threshold as double: %s\n", optarg);
                }
            }
            break;

            case LONG_OPT_THRESHOLD_MAX:
            {
                double res;
                char *pend = NULL;

                opt_run_threshold_max = 1;

                res = strtod(optarg, &pend);
                
                if (pend != NULL && *pend == '\0')
                {
                    if (errno == ERANGE)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "error (range), cannot parse threshold as double: %s\n", optarg);
                    }

                    run_threshold_max = res;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, cannot parse threshold as double: %s\n", optarg);
                }
            }
            break;

            case LONG_OPT_ORDER:
            {
                int res;
                char *pend = NULL;

                opt_run_order = 1;

                res = strtol(optarg, &pend, 0);
                
                if (pend != NULL && *pend == '\0')
                {
                    if (errno == ERANGE)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "error (range), cannot parse order as integer: %s\n", optarg);
                    }

                    run_order = res;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, cannot parse order as integer: %s\n", optarg);
                }
            }
            break;

            case 'p':
            {
                int res;
                char *pend = NULL;

                opt_run_predictors = 1;

                res = strtol(optarg, &pend, 0);
                
                if (pend != NULL && *pend == '\0')
                {
                    if (errno == ERANGE)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "error (range), cannot parse predictors as integer: %s\n", optarg);
                    }

                    run_predictors = res;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, cannot parse predictors as integer: %s\n", optarg);
                }
            }
            break;

            case 'q':
                g_verbosity = 0;
                break;

            case 'v':
                g_verbosity = 2;
                break;

            case LONG_OPT_DEBUG:
                g_verbosity = VERBOSE_DEBUG;
                break;

            case '?':
                print_help(argv[0]);
                exit(0);
                break;
        }
    }
}

int main(int argc, char **argv)
{
    struct FileInfo *input_file;
    struct FileInfo *output_file;
    struct WavFile *wav_file = NULL;
    struct AdpcmAifcFile *aifc_file = NULL;
    uint8_t *audio_data = NULL;
    size_t audio_data_len = 0;
    enum DATA_ENCODING encoding = DATA_ENCODING_LSB;
    struct ALADPCMBook *book;
    struct codebook_threshold_parameters threshold_parameters;
    struct codebook_threshold_parameters *threshold_parameters_ptr;

    read_opts(argc, argv);

    if (opt_help_flag || !opt_input_file)
    {
        print_help(argv[0]);
        exit(0);
    }

    // if the user didn't provide an output filename, reuse the input filename.
    if (!opt_output_file)
    {
        output_filename_len = snprintf(NULL, 0, "%s%s", input_filename, TABLE_DEFAULT_EXTENSION); // overallocate
        output_filename = (char *)malloc_zero(output_filename_len + 1, 1);

        change_filename_extension(input_filename, output_filename, TABLE_DEFAULT_EXTENSION, output_filename_len);
    }

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("g_verbosity: %d\n", g_verbosity);
        printf("opt_help_flag: %d\n", opt_help_flag);
        printf("opt_input_file: %d\n", opt_input_file);
        printf("input_filename: %s\n", input_filename != NULL ? input_filename : "NULL");
        printf("opt_output_file: %d\n", opt_output_file);
        printf("output_filename: %s\n", output_filename != NULL ? output_filename : "NULL");
        printf("opt_run_order: %d\n", opt_run_order);
        printf("run_order: %d\n", run_order);
        printf("opt_run_predictors: %d\n", opt_run_predictors);
        printf("run_predictors: %d\n", run_predictors);
        printf("opt_run_threshold_mode: %d\n", opt_run_threshold_mode);
        printf("opt_run_threshold_min: %d\n", opt_run_threshold_min);
        printf("opt_run_threshold_max: %d\n", opt_run_threshold_max);
        printf("threshold_mode: %d\n", threshold_mode);
        printf("run_threshold_min: %g\n", run_threshold_min);
        printf("run_threshold_max: %g\n", run_threshold_max);
        fflush(stdout);
    }

    // setup threshold

    memset(&threshold_parameters, 0, sizeof(struct codebook_threshold_parameters));

    if ((opt_run_threshold_min || opt_run_threshold_max) && threshold_mode == THRESHOLD_MODE_DEFAULT_UNKOWN)
    {
        stderr_exit(EXIT_CODE_GENERAL, "error, mode is required if setting threshold-min or threshold-max\n");
    }

    if (threshold_mode != THRESHOLD_MODE_DEFAULT_UNKOWN
        && !opt_run_threshold_min
        && !opt_run_threshold_max)
    {
        stderr_exit(EXIT_CODE_GENERAL, "error, at least one of threshold-min or threshold-max needs to be set when mode is supplied\n");
    }

    if (threshold_mode != THRESHOLD_MODE_DEFAULT_UNKOWN)
    {
        // if user only set one threshold, supply the default value for the other.
        if (threshold_mode == THRESHOLD_MODE_ABSOLUTE)
        {
            if (!opt_run_threshold_min)
            {
                run_threshold_min = CODEBOOK_DEFAULT_USER_THRESHOLD_ABSOLUTE_MIN;
            }
            if (!opt_run_threshold_max)
            {
                run_threshold_max = CODEBOOK_DEFAULT_USER_THRESHOLD_ABSOLUTE_MAX;
            }

            if (run_threshold_min < CODEBOOK_DEFAULT_USER_THRESHOLD_ABSOLUTE_MIN)
            {
                stderr_exit(EXIT_CODE_GENERAL, "error, threshold-min=%g less than min value=%g\n", run_threshold_min, CODEBOOK_DEFAULT_USER_THRESHOLD_ABSOLUTE_MIN);
            }

            if (run_threshold_max > CODEBOOK_DEFAULT_USER_THRESHOLD_ABSOLUTE_MAX)
            {
                stderr_exit(EXIT_CODE_GENERAL, "error, threshold-min=%g greater than max value=%g\n", run_threshold_max, CODEBOOK_DEFAULT_USER_THRESHOLD_ABSOLUTE_MAX);
            }
        }
        else if (threshold_mode == THRESHOLD_MODE_QUANTILE)
        {
            if (!opt_run_threshold_min)
            {
                run_threshold_min = CODEBOOK_DEFAULT_USER_THRESHOLD_QUANTILE_MIN;
            }
            if (!opt_run_threshold_max)
            {
                run_threshold_max = CODEBOOK_DEFAULT_USER_THRESHOLD_QUANTILE_MAX;
            }

            if (run_threshold_min < CODEBOOK_DEFAULT_USER_THRESHOLD_QUANTILE_MIN)
            {
                stderr_exit(EXIT_CODE_GENERAL, "error, threshold-min=%g less than min value=%g\n", run_threshold_min, CODEBOOK_DEFAULT_USER_THRESHOLD_QUANTILE_MIN);
            }

            if (run_threshold_max > CODEBOOK_DEFAULT_USER_THRESHOLD_QUANTILE_MAX)
            {
                stderr_exit(EXIT_CODE_GENERAL, "error, threshold-min=%g greater than max value=%g\n", run_threshold_max, CODEBOOK_DEFAULT_USER_THRESHOLD_QUANTILE_MAX);
            }
        }
        
        threshold_parameters.mode = threshold_mode;
        threshold_parameters.min = run_threshold_min;
        threshold_parameters.max = run_threshold_max;

        if (g_verbosity >= VERBOSE_DEBUG)
        {
            if (threshold_mode == THRESHOLD_MODE_ABSOLUTE)
            {
                printf("threshold mode: absolute\n");
            }
            else if (threshold_mode == THRESHOLD_MODE_QUANTILE)
            {
                printf("threshold mode: quantile\n");
            }

            printf("run_threshold_min: %g\n", run_threshold_min);
            printf("run_threshold_max: %g\n", run_threshold_max);
        }
    }

    // done with threshold setup,
    // setup audio data

    input_file = FileInfo_fopen(input_filename, "rb");

    if (string_ends_with(input_filename, WAV_DEFAULT_EXTENSION))
    {
        wav_file = WavFile_new_from_file(input_file);

        // didn't exit, so must be valid.
        if (wav_file->data_chunk == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> error, wav_file->data_chunk is NULL\n", __func__, __LINE__);
        }

        encoding = DATA_ENCODING_LSB;
        audio_data = wav_file->data_chunk->data;
        audio_data_len = wav_file->data_chunk->ck_data_size;
    }
    else if (string_ends_with(input_filename, AIFC_DEFAULT_EXTENSION))
    {
        aifc_file = AdpcmAifcFile_new_from_file(input_file);

        // didn't exit, so must be valid.
        if (aifc_file->sound_chunk == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> error, aifc_file->sound_chunk is NULL\n", __func__, __LINE__);
        }

        encoding = DATA_ENCODING_MSB;
        audio_data = aifc_file->sound_chunk->sound_data;
        audio_data_len = aifc_file->sound_chunk->ck_data_size - 8;
    }
    else
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> error, file (extension) not supported: %s\n", __func__, __LINE__, input_filename);
    }

    if (opt_run_threshold_mode != THRESHOLD_MODE_DEFAULT_UNKOWN)
    {
        threshold_parameters_ptr = &threshold_parameters;
    }
    else
    {
        threshold_parameters_ptr = NULL;
    }

    // done with setup, execute with parameters.
    book = estimate_codebook(
        audio_data,
        audio_data_len,
        encoding,
        threshold_parameters_ptr,
        run_order,
        run_predictors);

    // done with input file and audio containers
    FileInfo_free(input_file);

    if (wav_file != NULL)
    {
        WavFile_free(wav_file);
    }

    if (aifc_file != NULL)
    {
        AdpcmAifcFile_free(aifc_file);
    }

    // write result
    output_file = FileInfo_fopen(output_filename, "wb");
    ALADPCMBook_write_coef(book, output_file);

    // done with output
    FileInfo_free(output_file);
    ALADPCMBook_free(book);

    if (input_filename != NULL)
    {
        free(input_filename);
        input_filename = NULL;
    }

    if (output_filename != NULL)
    {
        free(output_filename);
        output_filename = NULL;
    }

    return 0;
}