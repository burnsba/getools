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
static char input_filename[MAX_FILENAME_LEN] = {0};
static char output_filename[MAX_FILENAME_LEN] = {0};

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
    int str_len;

    while ((ch = getopt_long(argc, argv, "n:o:qv", long_options, &option_index)) != -1)
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
    struct file_info *input_file;
    char *ctl_filename;
    char *tbl_filename;
    struct ALBankFile *bank_file;

    read_opts(argc, argv);

    if (opt_help_flag || !opt_input_file)
    {
        print_help(argv[0]);
        exit(0);
    }

    ctl_filename = (char *)malloc_zero(1, MAX_FILENAME_LEN);
    tbl_filename = (char *)malloc_zero(1, MAX_FILENAME_LEN);

    // if the user didn't provide an output filename, reuse the input filename.
    if (!opt_output_file)
    {
        change_filename_extension(input_filename, ctl_filename, NAUDIO_CTL_DEFAULT_EXTENSION, MAX_FILENAME_LEN);
        change_filename_extension(input_filename, tbl_filename, NAUDIO_TBL_DEFAULT_EXTENSION, MAX_FILENAME_LEN);
    }
    /**
     * Else, the user provided output filename, but still set extenesion.
    */
    else
    {
        change_filename_extension(output_filename, ctl_filename, NAUDIO_CTL_DEFAULT_EXTENSION, MAX_FILENAME_LEN);
        change_filename_extension(output_filename, tbl_filename, NAUDIO_TBL_DEFAULT_EXTENSION, MAX_FILENAME_LEN);
    }

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("g_verbosity: %d\n", g_verbosity);
        printf("opt_help_flag: %d\n", opt_help_flag);
        printf("opt_input_file: %d\n", opt_input_file);
        printf("opt_output_file: %d\n", opt_output_file);
        printf("input_filename: %s\n", input_filename);
        printf("output_filename: %s\n", output_filename);
        printf("ctl_filename: %s\n", ctl_filename);
        printf("tbl_filename: %s\n", tbl_filename);
        printf("opt_sort_meta: %d\n", opt_sort_meta);
        printf("opt_sort_natural: %d\n", opt_sort_natural);
        printf("opt_sample_rate: %d\n", opt_sample_rate);
        fflush(stdout);
    }

    input_file = file_info_fopen(input_filename, "rb");

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
    file_info_free(input_file);
    input_file = NULL;

    ALBankFile_free(bank_file);

    free(tbl_filename);
    free(ctl_filename);
    
    return 0;
}