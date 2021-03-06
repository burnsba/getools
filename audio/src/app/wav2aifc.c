/**
 * Copyright 2022 Ben Burns
*/
/**
 * This file is part of Gaudio.
 * 
 * Gaudio is free software: you can redistribute it and/or modify it under the
 * terms of the GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * Gaudio is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
 * without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with Gaudio. If not, see <https://www.gnu.org/licenses/>. 
*/
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
static int opt_input_file = 0;
static int opt_output_file = 0;
static int opt_swap = 0;
static char *input_filename = NULL;
static size_t input_filename_len = 0;
static char *output_filename = NULL;
static size_t output_filename_len = 0;
static char *coef_filename = NULL;
static size_t coef_filename_len = 0;

#define LONG_OPT_DEBUG        1003
#define LONG_OPT_SWAP         2001

static struct option long_options[] =
{
    {"help",         no_argument,     &opt_help_flag,   1  },
    {"in",     required_argument,               NULL,  'n' },
    {"out",    required_argument,               NULL,  'o' },
    {"swap",         no_argument,               NULL,   LONG_OPT_SWAP  },

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
    printf("Converts wav audio (mono 16 bit PCM little endian) to n64 aifc format (mono 16 bit PCM big endian) using codebook\n");
    printf("usage:\n");
    printf("\n");
    printf("    %s --in file [-c coef_file]\n", invoke);
    printf("\n");
    printf("options:\n");
    printf("\n");
    printf("    --help                        print this help\n");
    printf("    -n,--in=FILE                  input .wav file to convert\n");
    printf("    -o,--out=FILE                 output file. Optional. If not provided, will\n");
    printf("                                  reuse the input file name but change extension.\n");
    printf("    -c,--coef=FILE                coef table / codebook previously generated. Optional.\n");
    printf("                                  If no codebook is provided, format will be AL_RAW16_WAVE.\n");
    printf("     --swap                       byte swap audio samples before converting to .aifc\n");
    printf("                                  This is normally determined automatically, but.\n");
    printf("                                  can be forced with this switch.\n");

    printf("    -q,--quiet                    suppress output\n");
    printf("    -v,--verbose                  more output\n");

    printf("\n");
    fflush(stdout);
}

void read_opts(int argc, char **argv)
{
    int option_index = 0;
    int ch;

    while ((ch = getopt_long(argc, argv, "n:o:c:d:qv", long_options, &option_index)) != -1)
    {
        switch (ch)
        {
            case 'n':
            {
                opt_input_file = 1;

                input_filename_len = snprintf(NULL, 0, "%s", optarg) + 1;

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

                output_filename_len = snprintf(NULL, 0, "%s", optarg) + 1;

                if (output_filename_len < 1)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, output filename not specified\n");
                }

                output_filename = (char *)malloc_zero(output_filename_len + 1, 1);
                output_filename_len = snprintf(output_filename, output_filename_len, "%s", optarg);
            }
            break;

            case 'c':
            {
                opt_coef_file = 1;

                coef_filename_len = snprintf(NULL, 0, "%s", optarg) + 1;

                if (coef_filename_len < 1)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, coef filename not specified\n");
                }

                coef_filename = (char *)malloc_zero(coef_filename_len + 1, 1);
                coef_filename_len = snprintf(coef_filename, coef_filename_len, "%s", optarg);
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

            case LONG_OPT_SWAP:
                g_encode_bswap = 1;
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
    struct FileInfo *coef_file;
    struct FileInfo *input_file;
    struct FileInfo *output_file;
    struct AdpcmAifcFile *aifc;
    struct WavFile *wav;

    struct ALADPCMBook *book = NULL;

    read_opts(argc, argv);

    if (opt_help_flag || !opt_input_file)
    {
        print_help(argv[0]);
        exit(0);
    }

    // if the user didn't provide an output filename, reuse the input filename.
    if (!opt_output_file)
    {
        output_filename_len = snprintf(NULL, 0, "%s%s", input_filename, AIFC_DEFAULT_EXTENSION) + 1; // overallocate
        output_filename = (char *)malloc_zero(output_filename_len + 1, 1);

        change_filename_extension(input_filename, output_filename, AIFC_DEFAULT_EXTENSION, output_filename_len);
    }

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("g_verbosity: %d\n", g_verbosity);
        printf("opt_help_flag: %d\n", opt_help_flag);
        printf("opt_coef_file: %d\n", opt_coef_file);
        printf("coef_filename: %s\n", coef_filename != NULL ? coef_filename : "NULL");
        printf("opt_input_file: %d\n", opt_input_file);
        printf("input_filename: %s\n", input_filename != NULL ? input_filename : "NULL");
        printf("opt_output_file: %d\n", opt_output_file);
        printf("output_filename: %s\n", output_filename != NULL ? output_filename : "NULL");
        printf("opt_swap: %d\n", opt_swap);
        printf("g_encode_bswap: %d\n", g_encode_bswap);
        fflush(stdout);
    }

    if (opt_coef_file == 1)
    {
        coef_file = FileInfo_fopen(coef_filename, "rb");
        book = ALADPCMBook_new_from_coef(coef_file);
        FileInfo_free(coef_file);
    }

    input_file = FileInfo_fopen(input_filename, "rb");
    wav = WavFile_new_from_file(input_file);
    FileInfo_free(input_file);

    aifc = AdpcmAifcFile_new_from_wav(wav, book);

    output_file = FileInfo_fopen(output_filename, "wb");
    AdpcmAifcFile_fwrite(aifc, output_file);
    FileInfo_free(output_file);

    if (book != NULL)
    {
        ALADPCMBook_free(book);
    }

    WavFile_free(wav);
    wav = NULL;

    AdpcmAifcFile_free(aifc);
    aifc = NULL;

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

    if (coef_filename != NULL)
    {
        free(coef_filename);
        coef_filename = NULL;
    }

    return 0;
}