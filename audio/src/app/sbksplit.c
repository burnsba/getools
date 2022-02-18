#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <string.h>
#include <getopt.h>
#include "debug.h"
#include "machine_config.h"
#include "common.h"
#include "utility.h"

/**
 * This file contains main entry for sbksplit app.
 * 
 * This program splits a Rare .sbk file into individual .seq.rz files.
 * It does not perform any decompression.
*/

#define APPNAME "sbksplit"
#define VERSION "1.0"

#define DEFAULT_FILENAME_PREFIX "music_"
#define DEFAULT_EXTENSION       ".seq.rz" /* Rare 1172 compressed seq file */

#define MAX_SEQ_FILES           1024 /* arbitrary */

/**
 * Rare soundbank file begins with a header section, followed by individual .seq
 * files 1172 compressed.
 * 
 * The header begins with a 16 bit integer giving a count of the number
 * of sequences in the file.
 * The next 16 bits are unused (word padding?)
 * 
 * Following are `RareALSeqData` descriptions of the sequences in the file.
 * 
 * Following the header section are the individual .seq files in 1172
 * compressed format.
*/

static int opt_help_flag = 0;
static int opt_input_file = 0;
static int opt_user_filename_prefix = 0;
static int opt_names_file = 0;
static char input_filename[MAX_FILENAME_LEN] = {0};
static char output_filename[MAX_FILENAME_LEN] = {0};
static char names_filename[MAX_FILENAME_LEN] = {0};
static struct llist_root user_names = {0};

static struct option long_options[] =
{
    {"help",         no_argument,     &opt_help_flag,   1  },
    {"in",     required_argument,               NULL,  'i' },
    {"prefix", required_argument,               NULL,  'p' },
    {"names",  required_argument,               NULL,  'n' },
    {"quiet",        no_argument,               NULL,  'q' },
    {"verbose",      no_argument,               NULL,  'v' },
    {"debug",        no_argument,               NULL,  'd' },
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
    printf("splits a Rare .sbk file into individual .seq files\n");
    printf("usage:\n");
    printf("\n");
    printf("    %s -i file\n", invoke);
    printf("\n");
    printf("options:\n");
    printf("\n");
    printf("    --help                        print this help\n");
    printf("    -i,--in=FILE                  input file (required)\n");
    printf("    -p,--prefix=STRING            string to prepend to output files. (optional)\n");
    printf("                                  default=%s\n", DEFAULT_FILENAME_PREFIX);
    printf("    -n,--names=FILE               sound names. One name per line. Lines starting with # ignored.\n");
    printf("                                  Names applied in order read, if the list is too short\n");
    printf("                                  subsequent items will be given numeric id (0001, 0002, ...).\n");
    printf("                                  Non alphanumeric characters ignored.\n");
    printf("                                  Names listed in file should not include filename extension.\n");
    printf("    -q,--quiet                    suppress output\n");
    printf("    -v,--verbose                  more output\n");
    printf("\n");
    fflush(stdout);
}

void read_opts(int argc, char **argv)
{
    int ch;
    int str_len;

    while ((ch = getopt_long(argc, argv, "i:p:n:qvd", long_options, NULL)) != -1)
    {
        switch (ch)
        {
            case 'i':
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

            case 'p':
            {
                opt_user_filename_prefix = 1;

                str_len = strlen(optarg);
                if (str_len < 1)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, filename prefix not specified\n");
                }

                // 4 characters allocated for digits
                if (str_len > MAX_FILENAME_LEN - 5)
                {
                    str_len = MAX_FILENAME_LEN - 5;
                }

                strncpy(g_filename_prefix, optarg, str_len);
            }
            break;

            case 'n':
            {
                opt_names_file = 1;

                str_len = strlen(optarg);
                if (str_len < 1)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, names filename not specified\n");
                }

                if (str_len > MAX_FILENAME_LEN - 1)
                {
                    str_len = MAX_FILENAME_LEN - 1;
                }

                strncpy(names_filename, optarg, str_len);
            }
            break;

            case 'q':
                g_verbosity = 0;
                break;

            case 'v':
                g_verbosity = 2;
                break;

            case 'd':
                g_verbosity = 3;
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
    struct file_info *input;
    struct file_info *output;

    // number of .seq files listed in the .sbk header
    int32_t in_seq_count = 0;
    int32_t i;
    // this will be malloc'd, entire file should fit in RAM, should only be few hundred k at most
    uint8_t *input_file_contents;
    // input file current byte position index. only used for header section
    int32_t input_pos = 0;
    // offset to .seq file, from start of .sbk file
    size_t seq_address;
    // length in bytes of compressed .seq file
    uint16_t seq_len;

    // current name node from the list of user supplied names.
    struct llist_node *name_node = NULL;

    read_opts(argc, argv);

    if (opt_help_flag || argc < 2 || !opt_input_file)
    {
        print_help(argv[0]);
        exit(0);
    }

    // if user didn't supply a prefix use the default
    if (!opt_user_filename_prefix)
    {
        strncpy(g_filename_prefix, DEFAULT_FILENAME_PREFIX, MAX_FILENAME_LEN);
    }

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("opt_user_filename_prefix: %d\n", opt_user_filename_prefix);
        printf("opt_help_flag: %d\n", opt_help_flag);
        printf("input_filename: %s\n", input_filename);
        printf("g_filename_prefix: %s\n", g_filename_prefix);
        fflush(stdout);
    }

    input = file_info_fopen(input_filename, "rb");

    input_file_contents = (uint8_t *)malloc(input->len);
    if (input_file_contents == NULL)
    {
        perror("malloc");
		file_info_free(input);
        exit(EXIT_CODE_MALLOC);
    }

    file_info_fread(input, input_file_contents, input->len, 1);

    // done with input file, it's in memory now.
    file_info_free(input);

    in_seq_count = ((int32_t*)input_file_contents)[0];
    in_seq_count = BSWAP16_INLINE(in_seq_count);
    if (g_verbosity > 0)
    {
        printf("soundbank has %d entries\n", in_seq_count);
    }

    if (in_seq_count > MAX_SEQ_FILES)
    {
        fprintf(stderr, "error, input file has too many seq files\n");
        fflush(stderr);
		exit(EXIT_CODE_GENERAL);
    }

    if (opt_names_file)
    {
        uint8_t *names_file_contents;
        size_t file_length = 0;
        file_length = get_file_contents(names_filename, &names_file_contents);
        parse_names(names_file_contents, file_length, &user_names);
        free(names_file_contents);

        // llist_node_string_data_print(&user_names);
    }

    // 4 is to skip the first 32 bits (seq count + padding)
    input_pos = 4;

    // Read the .sbk header. Read the offset and length,
    // and then use those values to copy the .seq file (in memory) to the output file.
    for (i=0; i<in_seq_count; i++)
    {
        size_t write_len;
        seq_address = *(size_t*)(&input_file_contents[input_pos]);
        BSWAP32(seq_address);
        input_pos +=4; /* sizeof (struct RareALSeqData.address) == 4 */

        // don't care about uncompressed_len
        input_pos +=2; /* sizeof (struct RareALSeqData.uncompressed_len) == 2 */

        seq_len = *(uint16_t*)(&input_file_contents[input_pos]);
        BSWAP16(seq_len);
        input_pos +=2; /* sizeof (struct RareALSeqData.len) == 2 */

        if (g_verbosity > 2)
        {
            printf("entry %d\n", i);
            printf("seq_address = 0x%06x\n", (int32_t)seq_address);
            printf("seq_len = %d\n", seq_len);
        }

        if (seq_len == 0)
        {
            fprintf(stderr, "warning, entry %d (zero base index) has length zero, skipping\n", i);
            fflush(stderr);
            continue;
        }

        // done with .sbk header for this file.
        
        // generate output filename.
        memset(output_filename, 0, MAX_FILENAME_LEN);

        // only apply user specified filename if there are unclaimed names
        if (opt_names_file && (size_t)i < user_names.count)
        {
            struct string_data *sd = NULL;

            if (i == 0)
            {
                name_node = user_names.root;
            }

            if (name_node != NULL)
            {
                sd = (struct string_data *)name_node->data;
            }

            // Only use non empty filename
            if (sd != NULL && sd->len > 0)
            {
                write_len = snprintf(output_filename, MAX_FILENAME_LEN, "%s%s%s", g_filename_prefix, sd->text, DEFAULT_EXTENSION);
            }
            else
            {
                // the getopts method should verify the prefix is within allowed length, including
                // budget for digit characters.
                write_len = snprintf(output_filename, MAX_FILENAME_LEN, "%s%04d%s", g_filename_prefix, i, DEFAULT_EXTENSION);
            }

            if (name_node != NULL)
            {
                name_node = name_node->next;
            }
        }
        else
        {
            // same as above.
            write_len = snprintf(output_filename, MAX_FILENAME_LEN, "%s%04d%s", g_filename_prefix, i, DEFAULT_EXTENSION);
        }

        if (write_len > MAX_FILENAME_LEN)
        {
            // be quiet gcc
        }

        output = file_info_fopen(output_filename, "wb");

        if (g_verbosity > 1)
        {
            printf("writing entry %d to output file %s\n", i, output_filename);
        }

        // write to output, straight from the input file in memory
        file_info_fwrite(output, &input_file_contents[(size_t)seq_address], seq_len, 1);

        file_info_free(output);
        output = NULL;
    }

    free(input_file_contents);

    llist_node_free_string_data(&user_names);
    llist_node_root_free_children(&user_names);

    return 0;
}