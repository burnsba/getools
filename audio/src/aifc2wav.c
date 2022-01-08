#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <string.h>
#include <limits.h>
#include <getopt.h>
#include "debug.h"
#include "machine_config.h"
#include "common.h"
#include "utility.h"
#include "llist.h"
#include "naudio.h"
#include "adpcm_aifc.h"
#include "wav.h"

#define APPNAME "aifc2wav"
#define VERSION "1.0"

static int opt_help_flag = 0;
static int opt_input_file = 0;
static int opt_output_file = 0;
static char input_filename[MAX_FILENAME_LEN] = {0};
static char output_filename[MAX_FILENAME_LEN] = {0};

#define LONG_OPT_DEBUG   1003

static struct option long_options[] =
{
    {"help",         no_argument,     &opt_help_flag,   1  },
    {"in",     required_argument,               NULL,  'n' },
    {"out",    required_argument,               NULL,  'o' },
    {"quiet",        no_argument,               NULL,  'q' },
    {"verbose",      no_argument,               NULL,  'v' },
    {"debug",        no_argument,               NULL,   LONG_OPT_DEBUG },
    {NULL, 0, NULL, 0}
};

void print_help(const char * invoke)
{
    printf("%s %s help\n", APPNAME, VERSION);
    printf("\n");
    printf("Converts n64 aifc audio to wav format\n");
    printf("usage:\n");
    printf("\n");
    printf("    %s --in file\n", invoke);
    printf("\n");
    printf("options:\n");
    printf("\n");
    printf("    --help                        print this help\n");
    printf("    -n,--in=FILE                  input .aifc file to convert\n");
    printf("    -o,--out=FILE                 output file. Optional. If not provided, will\n");
    printf("                                  reuse the input file name but change extension.\n");
    printf("    -q,--quiet                    suppress output\n");
    printf("    -v,--verbose                  more output\n");
    printf("\n");
    fflush(stdout);
}

void read_opts(int argc, char **argv)
{
    int option_index = 0;
    int ch;
    int str_len;

    while ((ch = getopt_long(argc, argv, "c:t:d:p:qv", long_options, &option_index)) != -1)
    {
        switch (ch)
        {
            case 'n':
            {
                opt_input_file = 1;

                str_len = strlen(optarg);
                if (str_len < 1)
                {
                    stderr_exit(1, "error, input filename not specified\n");
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
                    stderr_exit(1, "error, output filename not specified\n");
                }

                if (str_len > MAX_FILENAME_LEN - 1)
                {
                    str_len = MAX_FILENAME_LEN - 1;
                }

                strncpy(output_filename, optarg, str_len);
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
    struct WavFile *wav_file;
    struct AdpcmAifcFile *aifc_file;
    struct file_info *input_file;
    struct file_info *output_file;

    read_opts(argc, argv);

    if (opt_help_flag || !opt_input_file)
    {
        print_help(argv[0]);
        exit(0);
    }

    // if the user didn't provide an output filename, reuse the input filename.
    if (!opt_output_file)
    {
        int i = 0;
        int last_pos = -1;
        char c;
        size_t len;
        size_t ext_len;

        // find last occurrence of '.' in the input filename.
        while (1)
        {
            c = input_filename[i];

            if (c == '.')
            {
                last_pos = i;
            }

            i++;
            if (c == '\0' || i >= MAX_FILENAME_LEN)
            {
                break;
            }
        }

        if (last_pos > -1)
        {
            len = last_pos;
        }
        else
        {
            // if there's no '.' then just use the entire input filename
            len = strlen(input_filename);
        }

        // truncate filename so there's room for the extension
        if (len >= 1 + (MAX_FILENAME_LEN - sizeof(WAV_DEFAULT_EXTENSION)))
        {
            len = MAX_FILENAME_LEN - sizeof(WAV_DEFAULT_EXTENSION);
        }

        strncpy(output_filename, input_filename, len);
        ext_len = sizeof(WAV_DEFAULT_EXTENSION);
        strncpy(&output_filename[len], WAV_DEFAULT_EXTENSION, ext_len);
        output_filename[len+ext_len] = '\0';
    }

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("g_verbosity: %d\n", g_verbosity);
        printf("opt_help_flag: %d\n", opt_help_flag);
        printf("opt_input_file: %d\n", opt_input_file);
        printf("opt_output_file: %d\n", opt_output_file);
        printf("input_filename: %s\n", input_filename);
        printf("output_filename: %s\n", output_filename);
        fflush(stdout);
    }

    input_file = file_info_fopen(input_filename, "rb");

    aifc_file = AdpcmAifcFile_new_from_file(input_file);

    // done with input file
    file_info_free(input_file);

    wav_file = WavFile_load_from_aifc(aifc_file);

    output_file = file_info_fopen(output_filename, "wb");

    WavFile_frwrite(wav_file, output_file);

    file_info_free(output_file);
    
    return 0;
}