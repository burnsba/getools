#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <string.h>
#include <getopt.h>
#include <errno.h>
#include <unistd.h>
#include "debug.h"
#include "machine_config.h"
#include "common.h"
#include "utility.h"
#include "midi.h"

/**
 * This file contains main entry for miditool app.
 * 
 * This app is a command line tool to adjust various events within a single MIDI track.
 * It does not convert MIDI to other formats.
*/

#define APPNAME "miditool"
#define VERSION "1.0"

enum E_MIDITOOL_MODE {
    MIDITOOL_MODE_DEFAULT_UNKNOWN = 0,
    MIDITOOL_MODE_SET_CHANNEL_INSTRUMENT,
    MIDITOOL_MODE_MAKE_CHANNEL_TRACK,
    MIDITOOL_MODE_REMOVE_LOOP,
    MIDITOOL_MODE_ADD_NOTE_LOOP,
    MIDITOOL_MODE_PARSE,
    MIDITOOL_MODE_PARSE_TRACK
};

static const char *MIDITOOL_MODE_ACTION_NAMES[] = {
    "unknown",
    "set-channel-instrument",
    "make-channel-track",
    "remove-loop",
    "add-note-loop",
    "parse",
    "parse-track",
};

// static const char *MIDITOOL_PARSE_MODE_NAMES[] = {
//     "midi",
//     "seq"
// };

#define MIN_VALID_CHANNEL 0
#define MAX_VALID_CHANNEL 15
#define MIN_VALID_TRACK 0
#define MAX_VALID_TRACK 15
#define MIN_VALID_INSTRUMENT 0
#define MAX_VALID_INSTRUMENT 128

static int opt_help_flag = 0;
static int opt_input_file = 0;
static int opt_output_file = 0;
static int opt_action = 0;
static int opt_channel = 0;
static int opt_track = 0;
static int opt_instrument = 0;
static int opt_loop_number = 0;
//static int opt_parse_mode = 0;
static char *input_filename = NULL;
static size_t input_filename_len = 0;
static char *output_filename = NULL;
static size_t output_filename_len = 0;
static char *temp_filename = NULL;
static size_t temp_filename_len = 0;
static int tool_mode = MIDITOOL_MODE_DEFAULT_UNKNOWN;
static int user_channel = 0;
static int user_track = 0;
static int user_instrument = 0;
static int user_loop_number = 0;
//static int user_implementation = MIDI_IMPLEMENTATION_STANDARD;

#define LONG_OPT_DEBUG   1003

#define LONG_OPT_LOOP_NUMBER    2001
#define LONG_OPT_PARSE_MODE     2002

static struct option long_options[] =
{
    {"help",           no_argument,     &opt_help_flag,   1  },
    {"in",       required_argument,               NULL,  'n' },
    {"out",      required_argument,               NULL,  'o' },
    {"action",        required_argument,          NULL,  'a' },
    {"channel",       required_argument,          NULL,  'c' },
    {"track",         required_argument,          NULL,  't' },
    {"instrument",    required_argument,          NULL,  'i' },
    {"loop-number",   required_argument,          NULL,  LONG_OPT_LOOP_NUMBER },
    //{"parse-mode",    required_argument,          NULL,  LONG_OPT_PARSE_MODE },
    {"quiet",          no_argument,               NULL,  'q' },
    {"verbose",        no_argument,               NULL,  'v' },
    {"debug",          no_argument,               NULL,   LONG_OPT_DEBUG },
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
    printf("Adjust MIDI events within MIDI file\n");
    printf("usage:\n");
    printf("\n");
    printf("    %s --in file --action=ACTION [args]\n", invoke);
    printf("\n");
    printf("options:\n");
    printf("\n");
    printf("    --help                        print this help\n");
    printf("    -n,--in=FILE                  input MIDI file\n");
    printf("    -a,--action=TEXT              Action to perform\n");
    printf("    -c,--channel=INT              Channel value.\n");
    printf("    -t,--track=INT                Track value.\n");
    printf("    -i,--instrument=INT           Instrument value.\n");
    //printf("    --parse-mode=TEXT             Input file format. Supported options: midi, seq\n");
    printf("    --loop-number=INT             Loop number value.\n");
    printf("    -q,--quiet                    suppress output\n");
    printf("    -v,--verbose                  more output\n");
    printf("\n");
    printf("Available actions:\n");
    printf("    parse                         Parse all tracks in file and write parse output to stdout.\n");
    // printf("                                  Parameters used:\n");
    // printf("                                  --parse-mode\n");
    printf("    parse-track                   Parse single track in file and write parse output to stdout.\n");
    printf("                                  Parameters used:\n");
    // printf("                                  --parse-mode\n");
    printf("                                  --track\n");
    printf("    make-channel-track            Set any event channel to the same as the track number.\n");
    printf("    remove-loop                   Remove loop with specified loop number. Parameters used:\n");
    printf("                                  --track\n");
    printf("                                  --loop-number\n");
    printf("    add-note-loop                 Creates a new loop, starting before the first Note On, and\n");
    printf("                                  ending after the last Note Off of the track. Parameters used:\n");
    printf("                                  --track\n");
    printf("                                  --loop-number\n");
    printf("    set-channel-instrument        Iterate all events in the file, and for any Program Change event that sets\n");
    printf("                                  the instrument for a channel, change it instead to the values\n");
    printf("                                  supplied. Parameters used:\n");
    printf("                                  --channel\n");
    printf("                                  --instrument\n");
    printf("\n");
    fflush(stdout);
}

void read_opts(int argc, char **argv)
{
    int option_index = 0;
    int ch;
    size_t str_len;

    while ((ch = getopt_long(argc, argv, "a:c:t:i:n:o:qv", long_options, &option_index)) != -1)
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

            case 't':
            {
                int res;
                char *pend = NULL;

                opt_track = 1;

                res = strtol(optarg, &pend, 0);
                
                if (pend != NULL && *pend == '\0')
                {
                    if (errno == ERANGE)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "error (range), cannot parse track as integer: %s\n", optarg);
                    }

                    if (res < MIN_VALID_TRACK || res > MAX_VALID_TRACK)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "error track value %s out of range: %d-%d\n", optarg, MIN_VALID_CHANNEL, MAX_VALID_CHANNEL);
                    }

                    user_track = res;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, cannot parse track as integer: %s\n", optarg);
                }
            }
            break;

            case 'c':
            {
                int res;
                char *pend = NULL;

                opt_channel = 1;

                res = strtol(optarg, &pend, 0);
                
                if (pend != NULL && *pend == '\0')
                {
                    if (errno == ERANGE)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "error (range), cannot parse channel as integer: %s\n", optarg);
                    }

                    if (res < MIN_VALID_CHANNEL || res > MAX_VALID_CHANNEL)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "error channel value %s out of range: %d-%d\n", optarg, MIN_VALID_CHANNEL, MAX_VALID_CHANNEL);
                    }

                    user_channel = res;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, cannot parse channel as integer: %s\n", optarg);
                }
            }
            break;

            case 'i':
            {
                int res;
                char *pend = NULL;

                opt_instrument = 1;

                res = strtol(optarg, &pend, 0);
                
                if (pend != NULL && *pend == '\0')
                {
                    if (errno == ERANGE)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "error (range), cannot parse instrument as integer: %s\n", optarg);
                    }

                    if (res < MIN_VALID_INSTRUMENT || res > MAX_VALID_INSTRUMENT)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "error instrument value %s out of range: %d-%d\n", optarg, MIN_VALID_INSTRUMENT, MAX_VALID_INSTRUMENT);
                    }

                    user_instrument = res;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, cannot parse instrument as integer: %s\n", optarg);
                }
            }
            break;

            case LONG_OPT_LOOP_NUMBER:
            {
                int res;
                char *pend = NULL;

                opt_loop_number = 1;

                res = strtol(optarg, &pend, 0);
                
                if (pend != NULL && *pend == '\0')
                {
                    if (errno == ERANGE)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "error (range), cannot parse loop as integer: %s\n", optarg);
                    }

                    user_loop_number = res;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, cannot parse loop as integer: %s\n", optarg);
                }
            }
            break;

            case 'a':
            {
                opt_action = 1;

                str_len = strlen(optarg);
                if (str_len < 1)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, action not specified\n");
                }

                if (strncasecmp(optarg, MIDITOOL_MODE_ACTION_NAMES[MIDITOOL_MODE_SET_CHANNEL_INSTRUMENT], str_len) == 0)
                {
                    tool_mode = MIDITOOL_MODE_SET_CHANNEL_INSTRUMENT;
                }
                else if (strncasecmp(optarg, MIDITOOL_MODE_ACTION_NAMES[MIDITOOL_MODE_MAKE_CHANNEL_TRACK], str_len) == 0)
                {
                    tool_mode = MIDITOOL_MODE_MAKE_CHANNEL_TRACK;
                }
                else if (strncasecmp(optarg, MIDITOOL_MODE_ACTION_NAMES[MIDITOOL_MODE_REMOVE_LOOP], str_len) == 0)
                {
                    tool_mode = MIDITOOL_MODE_REMOVE_LOOP;
                }
                else if (strncasecmp(optarg, MIDITOOL_MODE_ACTION_NAMES[MIDITOOL_MODE_ADD_NOTE_LOOP], str_len) == 0)
                {
                    tool_mode = MIDITOOL_MODE_ADD_NOTE_LOOP;
                }
                else if (strncasecmp(optarg, MIDITOOL_MODE_ACTION_NAMES[MIDITOOL_MODE_PARSE], str_len) == 0)
                {
                    tool_mode = MIDITOOL_MODE_PARSE;
                }
                else if (strncasecmp(optarg, MIDITOOL_MODE_ACTION_NAMES[MIDITOOL_MODE_PARSE_TRACK], str_len) == 0)
                {
                    tool_mode = MIDITOOL_MODE_PARSE_TRACK;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, action value not recognized:%s\n", optarg);
                }
            }
            break;

            // case LONG_OPT_PARSE_MODE:
            // {
            //     opt_parse_mode = 1;

            //     str_len = strlen(optarg);
            //     if (str_len < 1)
            //     {
            //         stderr_exit(EXIT_CODE_GENERAL, "error, parse mode not specified\n");
            //     }

            //     if (strncasecmp(optarg, MIDITOOL_PARSE_MODE_NAMES[MIDI_IMPLEMENTATION_STANDARD], str_len) == 0)
            //     {
            //         user_implementation = MIDI_IMPLEMENTATION_STANDARD;
            //     }
            //     else if (strncasecmp(optarg, MIDITOOL_PARSE_MODE_NAMES[MIDI_IMPLEMENTATION_STANDARD], str_len) == 0)
            //     {
            //         user_implementation = MIDI_IMPLEMENTATION_STANDARD;
            //     }
            //     else
            //     {
            //         stderr_exit(EXIT_CODE_GENERAL, "error, parse mode value not recognized:%s\n", optarg);
            //     }
            // }
            // break;

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
    struct MidiFile *midi_file;
    //struct CseqFile *cseq_file;
    struct FileInfo *input_file;
    struct FileInfo *output_file;
    int fs_result;

    read_opts(argc, argv);

    if (opt_help_flag || !opt_input_file || tool_mode == MIDITOOL_MODE_DEFAULT_UNKNOWN)
    {
        print_help(argv[0]);
        exit(0);
    }

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("g_verbosity: %d\n", g_verbosity);
        printf("opt_help_flag: %d\n", opt_help_flag);
        printf("opt_input_file: %d\n", opt_input_file);
        printf("opt_output_file: %d\n", opt_output_file);
        printf("input_filename: %s\n", input_filename != NULL ? input_filename : "NULL");
        printf("output_filename: %s\n", output_filename != NULL ? output_filename : "NULL");
        printf("opt_action: %d\n", opt_action);
        printf("opt_channel: %d\n", opt_channel);
        printf("opt_track: %d\n", opt_track);
        printf("opt_instrument: %d\n", opt_instrument);
        printf("opt_loop_number: %d\n", opt_loop_number);
        printf("tool_mode: %d\n", tool_mode);
        printf("user_channel: %d\n", user_channel);
        printf("user_track: %d\n", user_track);
        printf("user_instrument: %d\n", user_instrument);
        printf("user_loop_number: %d\n", user_loop_number);
        //printf("user_implementation: %d\n", user_implementation);
        fflush(stdout);
    }

    if (tool_mode == MIDITOOL_MODE_SET_CHANNEL_INSTRUMENT)
    {
        if (opt_channel == 0)
        {
            stderr_exit(EXIT_CODE_GENERAL, "Error, channel required for action %s\n", MIDITOOL_MODE_ACTION_NAMES[tool_mode]);
        }

        if (opt_instrument == 0)
        {
            stderr_exit(EXIT_CODE_GENERAL, "Error, instrument required for action %s\n", MIDITOOL_MODE_ACTION_NAMES[tool_mode]);
        }
    }

    if (tool_mode == MIDITOOL_MODE_REMOVE_LOOP || tool_mode == MIDITOOL_MODE_ADD_NOTE_LOOP)
    {
        if (opt_loop_number == 0)
        {
            stderr_exit(EXIT_CODE_GENERAL, "Error, loop required for action %s\n", MIDITOOL_MODE_ACTION_NAMES[tool_mode]);
        }
    }

    if (tool_mode == MIDITOOL_MODE_REMOVE_LOOP || tool_mode == MIDITOOL_MODE_ADD_NOTE_LOOP || tool_mode == MIDITOOL_MODE_PARSE_TRACK)
    {
        if (opt_track == 0)
        {
            stderr_exit(EXIT_CODE_GENERAL, "Error, track required for action %s\n", MIDITOOL_MODE_ACTION_NAMES[tool_mode]);
        }
    }

    // if (tool_mode == MIDITOOL_MODE_ADD_NOTE_LOOP || tool_mode == MIDITOOL_MODE_PARSE_TRACK)
    // {
    //     if (!opt_parse_mode)
    //     {
    //         user_implementation = MIDI_IMPLEMENTATION_STANDARD;
    //     }
    // }

    int transform = 0;

    output_filename_len = snprintf(NULL, 0, "%s.~", input_filename) + 1;
    output_filename = (char *)malloc_zero(output_filename_len + 1, 1);
    output_filename_len = snprintf(output_filename, output_filename_len, "%s.~", input_filename);

    temp_filename_len = snprintf(NULL, 0, "%s.T~", input_filename) + 1;
    temp_filename = (char *)malloc_zero(temp_filename_len + 1, 1);
    temp_filename_len = snprintf(temp_filename, temp_filename_len, "%s.T~", input_filename);

    input_file = FileInfo_fopen(input_filename, "rb");

    // if (user_implementation == MIDI_IMPLEMENTATION_STANDARD)
    // {
    midi_file = MidiFile_new_from_file(input_file);
    // }
    // else
    // {
    //     cseq_file = CseqFile_new_from_file(input_file);
    // }

    // done with input file
    FileInfo_free(input_file);

    if (tool_mode == MIDITOOL_MODE_SET_CHANNEL_INSTRUMENT)
    {
        transform = 1;

        struct MidiFile *new_midi_file = MidiFile_transform_set_channel_instrument(midi_file, user_channel, user_instrument);
        MidiFile_free(midi_file);
        midi_file = new_midi_file;
    }
    else if (tool_mode == MIDITOOL_MODE_MAKE_CHANNEL_TRACK)
    {
        transform = 1;
        
        struct MidiFile *new_midi_file = MidiFile_transform_make_channel_track(midi_file);
        MidiFile_free(midi_file);
        midi_file = new_midi_file;
    }
    else if (tool_mode == MIDITOOL_MODE_REMOVE_LOOP)
    {
        transform = 1;
        
        struct MidiFile *new_midi_file = MidiFile_transform_remove_loop(midi_file, user_loop_number, user_track);
        MidiFile_free(midi_file);
        midi_file = new_midi_file;
    }
    else if (tool_mode == MIDITOOL_MODE_ADD_NOTE_LOOP)
    {
        transform = 1;
        
        struct MidiFile *new_midi_file = MidiFile_transform_add_note_loop(midi_file, user_loop_number, user_track);
        MidiFile_free(midi_file);
        midi_file = new_midi_file;
    }
    else if (tool_mode == MIDITOOL_MODE_PARSE || tool_mode == MIDITOOL_MODE_PARSE_TRACK)
    {
        int parse_track_arg = -1;
        if (opt_track)
        {
            parse_track_arg = user_track;
        }

        // if (user_implementation == MIDI_IMPLEMENTATION_STANDARD)
        // {
        MidiFile_parse(midi_file, parse_track_arg);
        // }
        // else
        // {
        //     CseqFile_parse(midi_file, parse_track_arg);
        // }
    }

    if (transform)
    {
        output_file = FileInfo_fopen(output_filename, "wb");

        // if (user_implementation == MIDI_IMPLEMENTATION_STANDARD)
        // {
        MidiFile_fwrite(midi_file, output_file);
        // }
        // else
        // {
        //     CseqFile_fwrite(cseq_file, output_file);
        // }

        // done with output file
        FileInfo_free(output_file);
    }

    if (midi_file != NULL)
    {
        MidiFile_free(midi_file);
    }

    // if (cseq_file != NULL)
    // {
    //     CseqFile_free(cseq_file);
    // }

    if (transform)
    {
        // need to release file handles before renaming
        
        fs_result = rename(input_filename, temp_filename);
        if (fs_result == -1)
        {
            stderr_exit(EXIT_CODE_IO, "Error attempting to rename original input file to prepare for replacement with output result.\n");
        }

        fs_result = rename(output_filename, input_filename);
        if (fs_result == -1)
        {
            stderr_exit(EXIT_CODE_IO, "Error attempting to rename output file to update input file.\n");
        }

        fs_result = unlink(temp_filename);
        if (fs_result == -1)
        {
            stderr_exit(EXIT_CODE_IO, "Error attempting to delete temporary file.\n");
        }
    }
    
    return 0;
}