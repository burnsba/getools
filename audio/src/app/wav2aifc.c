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
#include "wav.h"
#include "x.h"

/**
 * This file contains main entry for wav2aifc app.
 * 
 * This app converts an .wav file to .aifc file.
 * It uses a codebook that has previously been generated.
 * It accepts a file path as input and writes to the given output file path.
*/

#define APPNAME "wav2aifc"
#define VERSION "1.0"

static int opt_help_flag = 0;
static int opt_coef_file = 0;
static char coef_filename[MAX_FILENAME_LEN] = {0};

#define LONG_OPT_DEBUG        1003

static struct option long_options[] =
{
    {"help",         no_argument,     &opt_help_flag,   1  },

    {"coef",    required_argument,              NULL,  'c' },

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
    printf("Converts n64 wav audio to aifc format using codebook\n");
    printf("usage:\n");
    printf("\n");
    printf("    %s --in file -c coef_file\n", invoke);
    printf("\n");
    printf("options:\n");
    printf("\n");
    printf("    --help                        print this help\n");
    printf("    -n,--in=FILE                  input .wav file to convert\n");
    printf("    -o,--out=FILE                 output file. Optional. If not provided, will\n");
    printf("                                  reuse the input file name but change extension.\n");
    printf("    -c,--coef=FILE                coef table / codebook previously generated.\n");

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

    while ((ch = getopt_long(argc, argv, "n:o:l:k:d:qv", long_options, &option_index)) != -1)
    {
        switch (ch)
        {
            case 'c':
            {
                opt_coef_file = 1;

                str_len = strlen(optarg);
                if (str_len < 1)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, coef filename not specified\n");
                }

                if (str_len > MAX_FILENAME_LEN - 1)
                {
                    str_len = MAX_FILENAME_LEN - 1;
                }

                strncpy(coef_filename, optarg, str_len);
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
    struct file_info *coef_file;
    struct ALADPCMBook *book;

    read_opts(argc, argv);

    if (opt_help_flag || !opt_coef_file)
    {
        print_help(argv[0]);
        exit(0);
    }

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("g_verbosity: %d\n", g_verbosity);
        printf("opt_help_flag: %d\n", opt_help_flag);
        printf("opt_coef_file: %d\n", opt_coef_file);
        printf("coef_filename: %s\n", coef_filename);
        fflush(stdout);
    }

    // TODO: add !opt_in_File in initial check

    coef_file = file_info_fopen(coef_filename, "rb");
    book = AdpcmAifcFile_new_from_file(coef_file);

    ALADPCMBook_free(book);
    file_info_free(coef_file);
}