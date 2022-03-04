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
#include "debug.h"
#include "machine_config.h"
#include "common.h"
#include "utility.h"
#include "llist.h"
#include "naudio.h"
#include "adpcm_aifc.h"
#include "x.h"

/**
 * This file contains main entry for tbl2aifc app.
 * 
 * This program takes as input:
 * - a .ctl file
 * - a .tbl file
 * and generates as output:
 * - a series of .aifc files from the .tbl
 * - an .inst file from the .ctl and .tbl data
 *
 *  ----------------
 * General program flow:
 * 1) load ctl contents into memory ALBankFile
 * 2) walk memory ALBankFile, byteswap and malloc objects
 * 3) walk memory ALBankFile, create .inst file
 * 4) walk memory ALBankFile, extract .aifc from .tbl
 * 
*/

#define APPNAME "tbl2aifc"
#define VERSION "1.0"

#define DEFAULT_OUT_DIR         "snd/"
#define DEFAULT_INST_FILENAME   "snd.inst"
#define DEFAULT_FILENAME_PREFIX "snd_"

#define SUPPORTTED_OUTPUT_MODE "s m"

static int opt_help_flag = 0;
static int opt_ctl_file = 0;
static int opt_tbl_file = 0;
static int opt_dir = 0;
static int use_other_aifc_dir = 1;
static int opt_user_filename_prefix = 0;
static int opt_inst_file = 0;
static int opt_output_mode = 0;
static int opt_names_file = 0;
static char *ctl_filename = NULL;
static size_t ctl_filename_len = 0;
static char *tbl_filename = NULL;
static size_t tbl_filename_len = 0;
static char *inst_filename = NULL;
static size_t inst_filename_len = 0;
static char *names_filename = NULL;
static size_t names_filename_len = 0;
static struct LinkedList user_names = {0};
static int generate_aifc = 1;
static int generate_inst = 1;

#define LONG_OPT_INST    1000
#define LONG_OPT_NO_AIFC 1001
#define LONG_OPT_NO_INST 1002
#define LONG_OPT_DEBUG   1003

static struct option long_options[] =
{
    {"help",         no_argument,     &opt_help_flag,   1  },
    {"ctl",    required_argument,               NULL,  'c' },
    {"tbl",    required_argument,               NULL,  't' },
    {"dir",    required_argument,               NULL,  'd' },
    {"prefix", required_argument,               NULL,  'p' },
    {"inst",   required_argument,               NULL,   LONG_OPT_INST  },
    {"no-aifc",      no_argument,     &generate_aifc,   LONG_OPT_NO_AIFC  },
    {"no-inst",      no_argument,     &generate_inst,   LONG_OPT_NO_INST  },
    {"quiet",        no_argument,               NULL,  'q' },
    {"names",  required_argument,               NULL,  'n' },
    {"verbose",      no_argument,               NULL,  'v' },
    {"debug",        no_argument,               NULL,   LONG_OPT_DEBUG },
    {NULL, 0, NULL, 0}
};

// forward declarations

void print_help(const char * invoke);
void read_opts(int argc, char **argv);
static void wavetable_init_set_aifc_path(struct ALWaveTable *wavetable);

// end forward declarations

void print_help(const char * invoke)
{
    printf("%s %s help\n", APPNAME, VERSION);
    printf("\n");
    printf("Extracts data from .tbl and .ctl files into .inst and .aifc files\n");
    printf("usage:\n");
    printf("\n");
    printf("    %s --ctl file --tbl file\n", invoke);
    printf("\n");
    printf("options:\n");
    printf("\n");
    printf("    --help                        print this help\n");
    printf("    -c,--ctl=FILE                 .ctl input file (required)\n");
    printf("    -t,--tbl=FILE                 .tbl input file (required)\n");
    printf("    -d,--dir=PATH                 output directory. Default=%s\n", DEFAULT_OUT_DIR);
    printf("    -p,--prefix=STRING            string to prepend to output aifc files.\n");
    printf("                                  Default=%s\n", DEFAULT_FILENAME_PREFIX);
    printf("    --inst=FILE                   output .inst filename. Default=%s\n", DEFAULT_INST_FILENAME);
    printf("    --no-aifc                     don't generate .aifc files\n");
    printf("    --no-inst                     don't generate .inst file\n");
    printf("    -n,--names=FILE               sound names. One name per line. Lines starting with # ignored.\n");
    printf("                                  Names applied in order read, if the list is too short\n");
    printf("                                  subsequent items will be given numeric id (0001, 0002, ...).\n");
    printf("                                  Non alphanumeric characters ignored.\n");
    printf("                                  Do not include filename extension.\n");
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

    while ((ch = getopt_long(argc, argv, "c:t:d:p:n:qv", long_options, &option_index)) != -1)
    {
        switch (ch)
        {
            case 'c':
            {
                opt_ctl_file = 1;

                ctl_filename_len = snprintf(NULL, 0, "%s", optarg) + 1;

                if (ctl_filename_len < 1)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, ctl filename not specified\n");
                }

                ctl_filename = (char *)malloc_zero(ctl_filename_len + 1, 1);
                ctl_filename_len = snprintf(ctl_filename, ctl_filename_len, "%s", optarg);
            }
            break;

            case 't':
            {
                opt_tbl_file = 1;

                tbl_filename_len = snprintf(NULL, 0, "%s", optarg) + 1;

                if (tbl_filename_len < 1)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, tbl filename not specified\n");
                }

                tbl_filename = (char *)malloc_zero(tbl_filename_len + 1, 1);
                tbl_filename_len = snprintf(tbl_filename, tbl_filename_len, "%s", optarg);
            }
            break;

            case 'd':
            {
                opt_dir = 1;
                use_other_aifc_dir = 1;

                str_len = strlen(optarg);
                if (str_len < 1)
                {
                    opt_dir = 0;
                    break;
                }
                else if (str_len == 1)
                {
                    if (optarg[0] == '.')
                    {
                        use_other_aifc_dir = 0;
                        break;
                    }
                }
                else if (str_len == 2)
                {
                    if (optarg[0] == '.' && optarg[1] == '/')
                    {
                        use_other_aifc_dir = 0;
                        break;
                    }
                }

                if (optarg[str_len - 1] != PATH_SEPERATOR)
                {
                    g_output_dir_len = snprintf(NULL, 0, "%s%c", optarg, PATH_SEPERATOR) + 1;
                    g_output_dir = (char *)malloc_zero(g_output_dir_len + 1, 1);
                    g_output_dir_len = snprintf(g_output_dir, g_output_dir_len, "%s%c", optarg, PATH_SEPERATOR);
                }
                else
                {
                    g_output_dir_len = snprintf(NULL, 0, "%s", optarg) + 1;
                    g_output_dir = (char *)malloc_zero(g_output_dir_len + 1, 1);
                    g_output_dir_len = snprintf(g_output_dir, g_output_dir_len, "%s", optarg);
                }
            }
            break;

            case 'p':
            {
                opt_user_filename_prefix = 1;

                g_filename_prefix_len = snprintf(NULL, 0, "%s", optarg) + 1;

                if (g_filename_prefix_len < 1)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, filename prefix not specified\n");
                }

                g_filename_prefix = (char *)malloc_zero(g_filename_prefix_len + 1, 1);
                g_filename_prefix_len = snprintf(g_filename_prefix, g_filename_prefix_len, "%s", optarg);
            }
            break;

            case 'n':
            {
                opt_names_file = 1;

                names_filename_len = snprintf(NULL, 0, "%s", optarg) + 1;

                if (names_filename_len < 1)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, names filename not specified\n");
                }

                names_filename = (char *)malloc_zero(names_filename_len + 1, 1);
                names_filename_len = snprintf(names_filename, names_filename_len, "%s", optarg);
            }
            break;

            case 'q':
                g_verbosity = 0;
                break;

            case 'v':
                g_verbosity = 2;
                break;

            case LONG_OPT_INST:
            {
                opt_inst_file = 1;

                inst_filename_len = snprintf(NULL, 0, "%s", optarg) + 1;

                if (inst_filename_len < 1)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, inst filename not specified\n");
                }

                inst_filename = (char *)malloc_zero(inst_filename_len + 1, 1);
                inst_filename_len = snprintf(inst_filename, inst_filename_len, "%s", optarg);
            }
            break;

            case LONG_OPT_NO_AIFC:
                generate_aifc = 0;
                break;

            case LONG_OPT_NO_INST:
                generate_inst = 0;
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

/**
 * Callback function used when creating a new wavetable.
 * This allows setting the aifc filename based on filenames the user provides.
 * Allocates memory for {@code wavetable->aifc_path}.
*/
static void wavetable_init_set_aifc_path(struct ALWaveTable *wavetable)
{
    TRACE_ENTER(__func__)

    static struct LinkedListNode *name_node = NULL;
    size_t filesystem_path_len = 0;
    char *filesystem_path;

    char *local_output_dir = ""; // empty string
    char *local_filename_prefix = ""; // empty string

    if (g_output_dir != NULL)
    {
        local_output_dir = g_output_dir;
    }

    if (g_filename_prefix != NULL)
    {
        local_filename_prefix = g_filename_prefix;
    }

    // only apply user specified filename if there are unclaimed names
    if (opt_names_file && (size_t)wavetable->id < user_names.count)
    {
        struct string_data *sd = NULL;

        if (wavetable->id == 0)
        {
            name_node = user_names.head;
        }

        if (name_node != NULL)
        {
            sd = (struct string_data *)name_node->data;
        }

        // Only use non empty filename
        if (sd != NULL && sd->len > 0)
        {
            filesystem_path_len = snprintf(NULL, 0, "%s%s%s", local_output_dir, sd->text, NAUDIO_AIFC_OUT_DEFAULT_EXTENSION) + 1;
            filesystem_path = (char *)malloc_zero(filesystem_path_len + 1, 1);
            filesystem_path_len = snprintf(filesystem_path, filesystem_path_len, "%s%s%s", local_output_dir, sd->text, NAUDIO_AIFC_OUT_DEFAULT_EXTENSION);
        }
        else
        {
            filesystem_path_len = snprintf(NULL, 0, "%s%s%04d%s", local_output_dir, local_filename_prefix, wavetable->id, NAUDIO_AIFC_OUT_DEFAULT_EXTENSION) + 1;
            filesystem_path = (char *)malloc_zero(filesystem_path_len + 1, 1);
            filesystem_path_len = snprintf(filesystem_path, filesystem_path_len, "%s%s%04d%s", local_output_dir, local_filename_prefix, wavetable->id, NAUDIO_AIFC_OUT_DEFAULT_EXTENSION);
        }

        if (name_node != NULL)
        {
            name_node = name_node->next;
        }
    }
    else
    {
        // same as above.
        filesystem_path_len = snprintf(NULL, 0, "%s%s%04d%s", local_output_dir, local_filename_prefix, wavetable->id, NAUDIO_AIFC_OUT_DEFAULT_EXTENSION) + 1;
        filesystem_path = (char *)malloc_zero(filesystem_path_len + 1, 1);
        filesystem_path_len = snprintf(filesystem_path, filesystem_path_len, "%s%s%04d%s", local_output_dir, local_filename_prefix, wavetable->id, NAUDIO_AIFC_OUT_DEFAULT_EXTENSION);
    }

    wavetable->aifc_path = filesystem_path;

    TRACE_LEAVE(__func__)
}


int main(int argc, char **argv)
{
    struct ALBankFile *bank_file;
    struct FileInfo *ctl_file;
    
    read_opts(argc, argv);

    if (opt_help_flag || !opt_ctl_file || !opt_tbl_file)
    {
        print_help(argv[0]);
        exit(0);
    }

    if (!generate_aifc && !generate_inst)
    {
        fprintf(stderr, "both .aifc and .inst generation disabled, nothing to do.\n");
        fflush(stderr);
        exit(EXIT_CODE_GENERAL);
    }

    if (use_other_aifc_dir)
    {
        // if user didn't supply output directory use the default
        if (!opt_dir)
        {
            g_output_dir_len = snprintf(NULL, 0, "%s", DEFAULT_OUT_DIR) + 1;
            g_output_dir = (char *)malloc_zero(g_output_dir_len + 1, 1);
            g_output_dir_len = snprintf(g_output_dir, g_output_dir_len, "%s", DEFAULT_OUT_DIR);
        }
    }

    // if user didn't supply filename_prefix use the default
    if (!opt_user_filename_prefix)
    {
        g_filename_prefix_len = snprintf(NULL, 0, "%s", DEFAULT_FILENAME_PREFIX) + 1;
        g_filename_prefix = (char *)malloc_zero(g_filename_prefix_len + 1, 1);
        g_filename_prefix_len = snprintf(g_filename_prefix, g_filename_prefix_len, "%s", DEFAULT_FILENAME_PREFIX);
    }

    // if user didn't supply inst filename use the default
    if (!opt_inst_file)
    {
        inst_filename_len = snprintf(NULL, 0, "%s", DEFAULT_INST_FILENAME) + 1;
        inst_filename = (char *)malloc_zero(inst_filename_len + 1, 1);
        inst_filename_len = snprintf(inst_filename, inst_filename_len, "%s", DEFAULT_INST_FILENAME);
    }

    if (opt_names_file)
    {
        uint8_t *names_file_contents;
        size_t file_length = 0;
        file_length = get_file_contents(names_filename, &names_file_contents);
        parse_names(names_file_contents, file_length, &user_names);
        free(names_file_contents);

        // LinkedListNode_string_data_print(&user_names);
    }

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("g_verbosity: %d\n", g_verbosity);
        printf("opt_help_flag: %d\n", opt_help_flag);
        printf("opt_ctl_file: %d\n", opt_ctl_file);
        printf("opt_tbl_file: %d\n", opt_tbl_file);
        printf("opt_dir: %d\n", opt_dir);
        printf("use_other_aifc_dir: %d\n", use_other_aifc_dir);
        printf("opt_user_filename_prefix: %d\n", opt_user_filename_prefix);
        printf("opt_inst_file: %d\n", opt_inst_file);
        printf("opt_output_mode: %d\n", opt_output_mode);
        printf("generate_aifc: %d\n", generate_aifc);
        printf("generate_inst: %d\n", generate_inst);
        printf("opt_names_file: %d\n", opt_names_file);
        printf("user_names_count: %ld\n", user_names.count);
        printf("g_output_dir: %s\n", g_output_dir != NULL ? g_output_dir : "NULL");
        printf("g_filename_prefix: %s\n", g_filename_prefix != NULL ? g_filename_prefix : "NULL");
        printf("ctl_filename: %s\n", ctl_filename != NULL ? ctl_filename : "NULL");
        printf("tbl_filename: %s\n", tbl_filename != NULL ? tbl_filename : "NULL");
        printf("inst_filename: %s\n", inst_filename != NULL ? inst_filename : "NULL");
        printf("names_filename: %s\n", names_filename != NULL ? names_filename : "NULL");
    }

    if (use_other_aifc_dir)
    {
        mkpath(g_output_dir);
    }

    ctl_file = FileInfo_fopen(ctl_filename, "rb");

    if (opt_names_file)
    {
        // need to set the callback before any wavetable objects are instantiated.
        wavetable_init_callback_ptr = wavetable_init_set_aifc_path;
    }

    bank_file = ALBankFile_new_from_ctl(ctl_file);

    if (generate_inst)
    {
        ALBankFile_write_inst(bank_file, inst_filename);
    }

    if (generate_aifc)
    {
        uint8_t *tbl_file_contents;

        get_file_contents(tbl_filename, &tbl_file_contents);
        write_bank_to_aifc(bank_file, tbl_file_contents);
        free(tbl_file_contents);
    }

    LinkedListNode_free_string_data(&user_names);
    LinkedList_free_children(&user_names);
    
    ALBankFile_free(bank_file);

    FileInfo_free(ctl_file);

    if (ctl_filename != NULL)
    {
        free(ctl_filename);
        ctl_filename = NULL;
    }

    if (tbl_filename != NULL)
    {
        free(tbl_filename);
        tbl_filename = NULL;
    }

    if (inst_filename != NULL)
    {
        free(inst_filename);
        inst_filename = NULL;
    }

    if (names_filename != NULL)
    {
        free(names_filename);
        names_filename = NULL;
    }

    if (g_filename_prefix != NULL)
    {
        free(g_filename_prefix);
        g_filename_prefix = NULL;
        g_filename_prefix_len = 0;
    }

    if (g_output_dir != NULL)
    {
        free(g_output_dir);
        g_output_dir = NULL;
        g_output_dir_len = 0;
    }

    return 0;
}