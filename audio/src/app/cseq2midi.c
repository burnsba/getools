#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <string.h>
#include <getopt.h>
#include "debug.h"
#include "machine_config.h"
#include "common.h"
#include "utility.h"
#include "midi.h"
#include "x.h"

/**
 * This file contains main entry for cseq2midi app.
 * 
 * This app converts a compressed MIDI for N64 playback to a regular MIDI.
 * Only format 0 MIDI is supported.
 * It accepts a file path as input and writes to the given output file path.
*/

#define APPNAME "cseq2midi"
#define VERSION "1.0"

static int opt_help_flag = 0;
static int opt_input_file = 0;
static int opt_output_file = 0;
static char input_filename[MAX_FILENAME_LEN] = {0};
static char output_filename[MAX_FILENAME_LEN] = {0};

#define LONG_OPT_DEBUG   1003
#define LONG_OPT_PARSE_DEBUG   1004

static struct option long_options[] =
{
    {"help",         no_argument,     &opt_help_flag,   1  },
    {"in",     required_argument,               NULL,  'n' },
    {"out",    required_argument,               NULL,  'o' },
    {"quiet",        no_argument,               NULL,  'q' },
    {"verbose",      no_argument,               NULL,  'v' },
    {"debug",        no_argument,               NULL,   LONG_OPT_DEBUG },
    {"parsedebug",   no_argument,               NULL,   LONG_OPT_PARSE_DEBUG },
    {NULL, 0, NULL, 0}
};

void print_help(const char * invoke)
{
    printf("%s %s help\n", APPNAME, VERSION);
    printf("\n");
    printf("Converts n64 compressed format MIDI to regular MIDI\n");
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

            case 'q':
                g_verbosity = 0;
                break;

            case 'v':
                g_verbosity = 2;
                break;

            case LONG_OPT_DEBUG:
                g_verbosity = VERBOSE_DEBUG;
                break;

            case LONG_OPT_PARSE_DEBUG:
                g_midi_parse_debug = 1;
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
    struct MidiFile *midi_file;
    struct CseqFile *cseq_file;
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
        change_filename_extension(input_filename, output_filename, MIDI_DEFAULT_EXTENSION, MAX_FILENAME_LEN);
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
    cseq_file = CseqFile_new_from_file(input_file);

    // done with input file
    file_info_free(input_file);
    input_file = NULL;

    midi_file = MidiFile_from_CseqFile(cseq_file);

    // done with input cseq file
    CseqFile_free(cseq_file);
    cseq_file = NULL;

    output_file = file_info_fopen(output_filename, "wb");

    MidiFile_fwrite(midi_file, output_file);

    // done with MIDI file
    MidiFile_free(midi_file);
    midi_file = NULL;

    file_info_free(output_file);
    
    return 0;
}
