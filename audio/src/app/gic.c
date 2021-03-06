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
 * This file contains main entry for gic app.
 * 
 * This app reads an .inst file, collects all referenced .aifc sources,
 * and creates a .ctl and .tbl output fil.
*/

#define APPNAME "gic"
#define VERSION "1.0"

#define GIC_MAX_SAMPLE_RATE 44100

static int opt_help_flag = 0;
static int opt_input_file = 0;
static int opt_output_file = 0;
static int opt_sort_natural = 0;
static int opt_sort_meta = 0;
static int opt_sample_rate = 0;
static int user_sample_rate = 0;
static char *input_filename = NULL;
static size_t input_filename_len = 0;
static char *output_filename = NULL;
static size_t output_filename_len = 0;

#define LONG_OPT_DEBUG               1003
#define LONG_OPT_SORT_NATURAL        2001
#define LONG_OPT_SORT_META           2002

static struct option long_options[] =
{
    {"help",         no_argument,     &opt_help_flag,   1  },
    {"in",     required_argument,               NULL,  'n' },
    {"out",    required_argument,               NULL,  'o' },
    {"sample-rate",  required_argument,         NULL,  'r' },
    
    {"sort-natural", no_argument,               NULL,  LONG_OPT_SORT_NATURAL },
    {"sort-meta",    no_argument,               NULL,  LONG_OPT_SORT_META },

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
    printf("Reads an .inst file and generates .tbl and .ctl files from specification and referenced .aifc files.\n");
    printf("usage:\n");
    printf("\n");
    printf("    %s --in file\n", invoke);
    printf("\n");
    printf("options:\n");
    printf("\n");
    printf("    --help                        print this help\n");
    printf("    -n,--in=FILE                  input .inst file source\n");
    printf("    -o,--out=FILE                 output file prefix. Optional. If not provided, will\n");
    printf("                                  reuse the input file name but change extension.\n");
    printf("    -r,--sample-rate              overwrite bank sample rate\n");
    printf("    --sort-natural                write envelope and keymap according to parent\n");
    printf("                                  ALSound order. Default value. Incompatible with sort-meta.\n");
    printf("    --sort-meta                   write envelope and keymap according to metaCtlWriteOrder\n");
    printf("                                  property read from .inst file. Incompatible with sort-natural.\n");
    printf("    -q,--quiet                    suppress output\n");
    printf("    -v,--verbose                  more output\n");
    printf("\n");

    fflush(stdout);
}

void read_opts(int argc, char **argv)
{
    int option_index = 0;
    int ch;

    while ((ch = getopt_long(argc, argv, "n:o:r:qv", long_options, &option_index)) != -1)
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

            case 'r':
            {
                int res;
                char *pend = NULL;

                opt_sample_rate = 1;

                res = strtol(optarg, &pend, 0);
                
                if (pend != NULL && *pend == '\0')
                {
                    if (errno == ERANGE)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "error (range), cannot parse sample rate as integer: %s\n", optarg);
                    }

                    if (res < 0 || res > GIC_MAX_SAMPLE_RATE)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "error, sample rate=%d out of range. Value must be 0-%d.\n", res, GIC_MAX_SAMPLE_RATE);
                    }

                    user_sample_rate = res;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, cannot parse sample rate as integer: %s\n", optarg);
                }
            }
            break;

            case LONG_OPT_DEBUG:
                g_verbosity = VERBOSE_DEBUG;
                break;

            case 'q':
                g_verbosity = 0;
                break;

            case 'v':
                g_verbosity = 2;
                break;

            case LONG_OPT_SORT_NATURAL:
                opt_sort_natural = 1;

                if (opt_sort_meta == 1)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, only one of --sort-natural and --sort-meta can be specified\n");
                }
                break;

            case LONG_OPT_SORT_META:
                opt_sort_meta = 1;

                if (opt_sort_natural == 1)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, only one of --sort-natural and --sort-meta can be specified\n");
                }
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
    char *ctl_filename;
    size_t ctl_filename_len;
    char *tbl_filename;
    size_t tbl_filename_len;
    struct ALBankFile *bank_file;

    read_opts(argc, argv);

    if (opt_help_flag || !opt_input_file)
    {
        print_help(argv[0]);
        exit(0);
    }

    // if the user didn't provide an output filename, reuse the input filename.
    if (!opt_output_file)
    {
        ctl_filename_len = snprintf(NULL, 0, "%s%s", input_filename, NAUDIO_CTL_DEFAULT_EXTENSION) + 1; // overallocate
        ctl_filename = (char *)malloc_zero(ctl_filename_len + 1, 1);

        change_filename_extension(input_filename, ctl_filename, NAUDIO_CTL_DEFAULT_EXTENSION, ctl_filename_len);

        tbl_filename_len = snprintf(NULL, 0, "%s%s", input_filename, NAUDIO_TBL_DEFAULT_EXTENSION) + 1; // overallocate
        tbl_filename = (char *)malloc_zero(tbl_filename_len + 1, 1);

        change_filename_extension(input_filename, tbl_filename, NAUDIO_TBL_DEFAULT_EXTENSION, tbl_filename_len);
    }
    /**
     * Else, the user provided output filename, but still set extenesion.
    */
    else
    {
        ctl_filename_len = snprintf(NULL, 0, "%s%s", output_filename, NAUDIO_CTL_DEFAULT_EXTENSION) + 1; // overallocate
        ctl_filename = (char *)malloc_zero(ctl_filename_len + 1, 1);

        change_filename_extension(output_filename, ctl_filename, NAUDIO_CTL_DEFAULT_EXTENSION, ctl_filename_len);

        tbl_filename_len = snprintf(NULL, 0, "%s%s", output_filename, NAUDIO_TBL_DEFAULT_EXTENSION) + 1; // overallocate
        tbl_filename = (char *)malloc_zero(tbl_filename_len + 1, 1);

        change_filename_extension(output_filename, tbl_filename, NAUDIO_TBL_DEFAULT_EXTENSION, tbl_filename_len);
    }

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("g_verbosity: %d\n", g_verbosity);
        printf("opt_help_flag: %d\n", opt_help_flag);
        printf("opt_input_file: %d\n", opt_input_file);
        printf("opt_output_file: %d\n", opt_output_file);
        printf("input_filename: %s\n", input_filename != NULL ? input_filename : "NULL");
        printf("output_filename: %s\n", output_filename != NULL ? output_filename : "NULL");
        printf("ctl_filename: %s\n", ctl_filename != NULL ? ctl_filename : "NULL");
        printf("tbl_filename: %s\n", tbl_filename != NULL ? tbl_filename : "NULL");
        printf("opt_sort_meta: %d\n", opt_sort_meta);
        printf("opt_sort_natural: %d\n", opt_sort_natural);
        printf("opt_sample_rate: %d\n", opt_sample_rate);
        fflush(stdout);
    }

    input_file = FileInfo_fopen(input_filename, "rb");

    bank_file = ALBankFile_new_from_inst(input_file);

    if (opt_sample_rate == 1)
    {
        int bank_count;

        for (bank_count=0; bank_count<bank_file->bank_count; bank_count++)
        {
            struct ALBank *bank = bank_file->banks[bank_count];

            if (bank != NULL)
            {
                bank->sample_rate = user_sample_rate;
            }
        }
    }

    if (opt_sort_natural == 0 && opt_sort_meta == 0)
    {
        if (g_verbosity >= VERBOSE_DEBUG)
        {
            printf("no sort order specified, set bank_file sort order to CTL_SORT_METHOD_NATURAL\n");
        }

        bank_file->ctl_sort_method = CTL_SORT_METHOD_NATURAL;
    }
    else if (opt_sort_natural == 1)
    {
        if (g_verbosity >= VERBOSE_DEBUG)
        {
            printf("set bank_file sort order to CTL_SORT_METHOD_NATURAL\n");
        }

        bank_file->ctl_sort_method = CTL_SORT_METHOD_NATURAL;
    }
    else if (opt_sort_meta == 1)
    {
        if (g_verbosity >= VERBOSE_DEBUG)
        {
            printf("set bank_file sort order to CTL_SORT_METHOD_META\n");
        }

        bank_file->ctl_sort_method = CTL_SORT_METHOD_META;
    }

    // it's necessary to write the .tbl file first in order to set the wavetable->base
    // offset values.
    ALBankFile_write_tbl(bank_file, tbl_filename);
    ALBankFile_write_ctl(bank_file, ctl_filename);

    // done with input file
    FileInfo_free(input_file);
    input_file = NULL;

    ALBankFile_free(bank_file);

    free(tbl_filename);
    free(ctl_filename);

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