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
static int opt_run_threshold = 0;
static int opt_run_order = 0;
static int opt_run_predictors = 0;
static double run_threshold = TABLE_DETAULT_THRESHOLD;
static int run_order = TABLE_DEFAULT_LAG;
static int run_predictors = TABLE_DEFAULT_PREDICTORS;
static char input_filename[MAX_FILENAME_LEN] = {0};
static char output_filename[MAX_FILENAME_LEN] = {0};

#define LONG_OPT_DEBUG        1003
#define LONG_OPT_ORDER        2001

#define TABLE_DESIGN_SUPPORTED_EXTENSIONS ".wav .aifc"

static struct option long_options[] =
{
    {"help",       no_argument,      &opt_help_flag,   1  },
    {"in",         required_argument,              NULL,  'n' },
    {"out",        required_argument,              NULL,  'o' },
    {"threshold",  required_argument,              NULL,  't' },
    {"order",      required_argument,              NULL,  LONG_OPT_ORDER },
    {"predictors", required_argument,              NULL,  'p' },

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
    printf("    -t,--threshold=DOUBLE         Auto correlation threshold for audio frame\n");
    printf("                                  Default=%f, min=0.0, max=inf (double.max_value)\n", TABLE_DETAULT_THRESHOLD);
    printf("    --order=INT                   Number of lag samples. Default=%d, min=%d, max=%d\n", TABLE_DEFAULT_LAG, TABLE_MIN_ORDER, TABLE_MAX_ORDER);
    printf("    -p,--predictors=INT           Number of predictors. Default=%d, min=%d, max=%d\n", TABLE_DEFAULT_PREDICTORS, TABLE_MIN_PREDICTORS, TABLE_MAX_PREDICTORS);

    printf("    -q,--quiet                    suppress output\n");
    printf("    -v,--verbose                  more output\n");
    printf("\n");

    printf("\n");
    fflush(stdout);
}

void read_opts(int argc, char **argv)
{
    int option_index = 0;
    int ch;
    int str_len;

    while ((ch = getopt_long(argc, argv, "qv", long_options, &option_index)) != -1)
    {
        switch (ch)
        {
            case 'n':
            {
                opt_input_file = 1;

                str_len = strlen(optarg);
                if (str_len < 1)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, input filename not specified\n");
                }

                if (str_len > MAX_FILENAME_LEN - 1)
                {
                    str_len = MAX_FILENAME_LEN - 1;
                }

                strncpy(input_filename, optarg, str_len);
            }
            break;

            case 'o':
            {
                opt_output_file = 1;

                str_len = strlen(optarg);
                if (str_len < 1)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, output filename not specified\n");
                }

                if (str_len > MAX_FILENAME_LEN - 1)
                {
                    str_len = MAX_FILENAME_LEN - 1;
                }

                strncpy(output_filename, optarg, str_len);
            }
            break;

            case 't':
            {
                double res;
                char *pend = NULL;

                opt_run_predictors = 1;

                res = strtod(optarg, &pend);
                
                if (pend != NULL && *pend == '\0')
                {
                    if (errno == ERANGE)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "error (range), cannot parse threshold as double: %s\n", optarg);
                    }

                    run_threshold = res;
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
    struct file_info *input_file;
    struct file_info *output_file;
    struct WavFile *wav_file = NULL;
    struct AdpcmAifcFile *aifc_file = NULL;
    uint8_t *audio_data;
    size_t audio_data_len;
    enum DATA_ENCODING encoding = DATA_ENCODING_LSB;
    struct ALADPCMBook *book;

    read_opts(argc, argv);

    if (opt_help_flag || !opt_input_file)
    {
        print_help(argv[0]);
        exit(0);
    }

    // if the user didn't provide an output filename, reuse the input filename.
    if (!opt_output_file)
    {
        change_filename_extension(input_filename, output_filename, TABLE_DEFAULT_EXTENSION, MAX_FILENAME_LEN);
    }

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("g_verbosity: %d\n", g_verbosity);
        printf("opt_help_flag: %d\n", opt_help_flag);
        printf("opt_input_file: %d\n", opt_input_file);
        printf("input_filename: %s\n", input_filename);
        printf("opt_output_file: %d\n", opt_output_file);
        printf("output_filename: %s\n", output_filename);
        printf("opt_run_order: %d\n", opt_run_order);
        printf("run_order: %d\n", run_order);
        printf("opt_run_predictors: %d\n", opt_run_predictors);
        printf("run_predictors: %d\n", run_predictors);
        printf("opt_run_threshold: %d\n", opt_run_threshold);
        printf("run_threshold: %f\n", run_threshold);
        fflush(stdout);
    }

    // setup

    input_file = file_info_fopen(input_filename, "rb");

    if (string_ends_with(input_filename, WAV_DEFAULT_EXTENSION))
    {
        wav_file = WavFile_new_from_file(input_file);

        // didn't exit, so must be valid.
        if (wav_file->data_chunk == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> wav_file->data_chunk is NULL\n", __func__, __LINE__);
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
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> aifc_file->sound_chunk is NULL\n", __func__, __LINE__);
        }

        encoding = DATA_ENCODING_MSB;
        audio_data = aifc_file->sound_chunk->sound_data;
        audio_data_len = aifc_file->sound_chunk->ck_data_size - 8;
    }
    else
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> file (extension) not supported: %s\n", __func__, __LINE__, input_filename);
    }

    // done with setup, execute with parameters.
    book = estimate_codebook(
        audio_data,
        audio_data_len,
        encoding,
        run_threshold,
        run_order,
        run_predictors);

    // done with input file and audio containers
    file_info_free(input_file);

    if (wav_file != NULL)
    {
        WavFile_free(wav_file);
    }

    if (aifc_file != NULL)
    {
        AdpcmAifcFile_free(aifc_file);
    }

    // write result
    output_file = file_info_fopen(output_filename, "wb");
    ALADPCMBook_write_coef(book, output_file);

    // done with output
    file_info_free(output_file);
    ALADPCMBook_free(book);

    return 0;
}