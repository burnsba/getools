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
 * Only format 1 MIDI is supported.
 * It accepts a file path as input and writes to the given output file path.
*/

#define APPNAME "cseq2midi"
#define VERSION "1.0"

static int opt_help_flag = 0;
static int opt_input_file = 0;
static int opt_output_file = 0;
static int opt_write_seq_track = 0;
static int opt_no_pattern_compression = 0;
static int opt_use_pattern_file = 0;
static char *input_filename = NULL;
static size_t input_filename_len = 0;
static char *output_filename = NULL;
static size_t output_filename_len = 0;
static char *pattern_filename = NULL;
static size_t pattern_filename_len = 0;

#define LONG_OPT_DEBUG   1003
#define LONG_OPT_PARSE_DEBUG   1004

#define LONG_OPT_WRITE_SEQ_TRACK   2001
#define LONG_OPT_NO_PATTERN_COMPRESSION   2002
#define LONG_OPT_PATTERN_FILE  2003

static struct option long_options[] =
{
    {"help",         no_argument,     &opt_help_flag,   1  },
    {"in",     required_argument,               NULL,  'n' },
    {"out",    required_argument,               NULL,  'o' },
    {"write-seq-tracks",     no_argument,       NULL,  LONG_OPT_WRITE_SEQ_TRACK },
    {"no-pattern-compression",    no_argument,  NULL,  LONG_OPT_NO_PATTERN_COMPRESSION },
    {"pattern-file",        required_argument,  NULL,  LONG_OPT_PATTERN_FILE },
    {"quiet",        no_argument,               NULL,  'q' },
    {"verbose",      no_argument,               NULL,  'v' },
    {"debug",        no_argument,               NULL,   LONG_OPT_DEBUG },
    {"parsedebug",   no_argument,               NULL,   LONG_OPT_PARSE_DEBUG },
    {NULL, 0, NULL, 0}
};

// forward declarations

void print_help(const char * invoke);
void read_opts(int argc, char **argv);
static void write_seq_track(struct GmidTrack *gtrack);

// end forward declarations

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
    printf("    --write-seq-tracks            Perform pattern substitution (unroll track) then\n");
    printf("                                  write track to disk.\n");
    printf("    --no-pattern-compression      By default, it is assumed the source seq file has\n");
    printf("                                  \"pattern marker\" compression, which escapes bytes\n");
    printf("                                  like 0xfe. This option disables that.\n");
    printf("    --pattern-file=FILE           Saves all pattern markers (with track number) to\n");
    printf("                                  specified file. Only applies when pattern compression\n");
    printf("                                  is not disabled.\n");
    printf("    -q,--quiet                    suppress output\n");
    printf("    -v,--verbose                  more output\n");
    printf("\n");
    fflush(stdout);
}

void read_opts(int argc, char **argv)
{
    int option_index = 0;
    int ch;

    while ((ch = getopt_long(argc, argv, "n:o:qv", long_options, &option_index)) != -1)
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

            case LONG_OPT_PATTERN_FILE:
            {
                opt_use_pattern_file = 1;

                pattern_filename_len = snprintf(NULL, 0, "%s", optarg) + 1;

                if (pattern_filename_len < 1)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, pattern filename not specified\n");
                }

                pattern_filename = (char *)malloc_zero(pattern_filename_len + 1, 1);
                pattern_filename_len = snprintf(pattern_filename, pattern_filename_len, "%s", optarg);
            }
            break;

            case 'q':
                g_verbosity = 0;
                break;

            case 'v':
                g_verbosity = 2;
                break;

            case LONG_OPT_WRITE_SEQ_TRACK:
                opt_write_seq_track = 1;
                break;

            case LONG_OPT_NO_PATTERN_COMPRESSION:
                opt_no_pattern_compression = 1;
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
    struct FileInfo *input_file;
    struct FileInfo *output_file;
    struct MidiConvertOptions *convert_options;
    f_GmidTrack_callback unroll_action = NULL;

    read_opts(argc, argv);

    if (opt_use_pattern_file && opt_no_pattern_compression)
    {
        printf("error, pattern file is not used when pattern compression is disabled\n");
        exit(0);
    }

    if (opt_help_flag || !opt_input_file)
    {
        print_help(argv[0]);
        exit(0);
    }

    // if the user didn't provide an output filename, reuse the input filename.
    if (!opt_output_file)
    {
        output_filename_len = snprintf(NULL, 0, "%s%s", input_filename, MIDI_DEFAULT_EXTENSION) + 1; // overallocate
        output_filename = (char *)malloc_zero(output_filename_len + 1, 1);

        change_filename_extension(input_filename, output_filename, MIDI_DEFAULT_EXTENSION, output_filename_len);
    }

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("g_verbosity: %d\n", g_verbosity);
        printf("opt_help_flag: %d\n", opt_help_flag);
        printf("opt_input_file: %d\n", opt_input_file);
        printf("opt_output_file: %d\n", opt_output_file);
        printf("input_filename: %s\n", input_filename != NULL ? input_filename : "NULL");
        printf("output_filename: %s\n", output_filename != NULL ? output_filename : "NULL");
        printf("opt_write_seq_track: %d\n", opt_write_seq_track);
        printf("opt_no_pattern_compression: %d\n", opt_no_pattern_compression);
        printf("opt_use_pattern_file: %d\n", opt_use_pattern_file);
        printf("pattern_filename: %s\n", pattern_filename != NULL ? pattern_filename : "NULL");
        fflush(stdout);
    }

    if (opt_write_seq_track)
    {
        unroll_action = write_seq_track;
    }

    input_file = FileInfo_fopen(input_filename, "rb");
    cseq_file = CseqFile_new_from_file(input_file);

    // done with input file
    FileInfo_free(input_file);
    input_file = NULL;

    convert_options = MidiConvertOptions_new();
    convert_options->post_unroll_action = unroll_action;
    convert_options->no_pattern_compression = opt_no_pattern_compression;
    if (opt_use_pattern_file)
    {
        convert_options->use_pattern_marker_file = 1;
        convert_options->pattern_marker_filename = pattern_filename;
    }

    midi_file = MidiFile_from_CseqFile(cseq_file, convert_options);

    // done with source cseq file
    CseqFile_free(cseq_file);
    cseq_file = NULL;

    // write to output file
    output_file = FileInfo_fopen(output_filename, "wb");
    MidiFile_fwrite(midi_file, output_file);

    // done with MIDI file
    MidiFile_free(midi_file);
    midi_file = NULL;

    FileInfo_free(output_file);
    MidiConvertOptions_free(convert_options);
    
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

    if (pattern_filename != NULL)
    {
        free(pattern_filename);
        pattern_filename = NULL;
    }
    
    return 0;
}

static void write_seq_track(struct GmidTrack *gtrack)
{
    TRACE_ENTER(__func__)

    struct FileInfo *fi;
    char new_filename[MAX_FILENAME_LEN];
    char new_extension[20];
    
    memset(new_filename, 0, MAX_FILENAME_LEN);
    memset(new_extension, 0, 20);

    sprintf(new_extension, "-track-%03d%s", gtrack->cseq_track_index, MIDI_N64_DEFAULT_EXTENSION);
    change_filename_extension(output_filename, new_filename, new_extension, MAX_FILENAME_LEN);

    fi = FileInfo_fopen(new_filename, "wb");
    FileInfo_fwrite(fi, gtrack->cseq_data, gtrack->cseq_data_len, 1);
    FileInfo_free(fi);

    TRACE_LEAVE(__func__)
}