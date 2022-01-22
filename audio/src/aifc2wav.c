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

/**
 * This file contains main entry for aifc2wav app.
 * 
 * This app converts an .aifc file to .wav file.
 * It accepts a file path as input and writes to the given output file path.
*/

#define APPNAME "aifc2wav"
#define VERSION "1.0"

enum E_FREQ_ADJUST_MODE {
    FREQ_ADJUST_DEFAULT_UNKNOWN = 0,
    FREQ_ADJUST_NONE,
    FREQ_ADJUST_EXPLICIT,
    FREQ_ADJUST_SEARCH
};

static const char *FREQ_ADJUST_MODE_NAMES[] = {
    "unknown",
    "none",
    "explicit",
    "search"
};

enum E_INST_FILE_SEARCH_MODE {
    INST_FILE_SEARCH_DEFAULT_UNKNOWN = 0,
    INST_FILE_SEARCH_USE,
    INST_FILE_SEARCH_SOUND,
    INST_FILE_SEARCH_KEYMAP
};

static const char *INST_FILE_SEARCH_NAMES[] = {
    "unknown",
    "use",
    "sound",
    "keymap"
};

static int opt_help_flag = 0;
static int opt_input_file = 0;
static int opt_output_file = 0;
static int opt_loop_count = 0;
static int opt_keybase = 0;
static int opt_detune = 0;
static int opt_inst_file = 0;
static int opt_inst_search = 0;
static int opt_inst_val = 0;
static char input_filename[MAX_FILENAME_LEN] = {0};
static char output_filename[MAX_FILENAME_LEN] = {0};
static char inst_filename[MAX_FILENAME_LEN] = {0};
static char inst_val[MAX_FILENAME_LEN] = {0};
static int freq_adjust_mode = FREQ_ADJUST_NONE;
static int inst_search_mode = INST_FILE_SEARCH_DEFAULT_UNKNOWN;
static int keybase = 0;
static int detune = 0;

#define LONG_OPT_DEBUG        1003
#define LONG_OPT_INST_FILE    1200
#define LONG_OPT_INST_SEARCH  1210
#define LONG_OPT_INST_VAL     1220

#define AIFC2WAV_DEFAULT_INF_LOOP_TIMES 0

#define AIFC2WAV_DEFAULT_KEYBASE 60 /* MIDI note C4 */
#define AIFC2WAV_DEFAULT_KEYBASE_HELP_TEXT "60 (MIDI note C4)"
#define AIFC2WAV_DEFAULT_DETUNE 0



static struct option long_options[] =
{
    {"help",         no_argument,     &opt_help_flag,   1  },
    {"in",     required_argument,               NULL,  'n' },
    {"out",    required_argument,               NULL,  'o' },
    {"loop",   required_argument,               NULL,  'l' },

    /* freq_adjust_mode = explicit */
    {"keybase", required_argument,               NULL,  'k' },
    {"detune",  required_argument,               NULL,  'd' },

    /* freq_adjust_mode = search */
    {"inst-file",   required_argument,            NULL, LONG_OPT_INST_FILE },
    {"inst-search", required_argument,            NULL, LONG_OPT_INST_SEARCH },
    {"inst-val",    required_argument,            NULL, LONG_OPT_INST_VAL },

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
    printf("    -l,--loop=NUMBER              This specifies the number of times an ADPCM Loop\n");
    printf("                                  should be repeated. Only applies to infinite loops.\n");
    printf("                                  Default=%d.\n", AIFC2WAV_DEFAULT_INF_LOOP_TIMES);
    printf("    -q,--quiet                    suppress output\n");
    printf("    -v,--verbose                  more output\n");
    printf("\n");

    printf("freq_adjust_mode = explicit\n");
    printf("\n");
    printf("    Keybase and detune parameters are explicitly set. Setting either value\n");
    printf("    implicitly toggles this mode. These options are incompatible with `search` mode.\n");
    printf("\n");
    printf("    -k,--keybase=NOTE             Keybase sound was recorded in. MIDI note range from\n");
    printf("                                  0-127. Refer to N64 Programming manual for more info.\n");
    printf("                                  Default=%s\n", AIFC2WAV_DEFAULT_KEYBASE_HELP_TEXT);
    printf("    -d,--detune=CENTS             Additional detune value in cents (1200 cents per octave).\n");
    printf("                                  Refer to N64 Programming manual for more info.\n");
    printf("                                  Default=0\n");
    printf("\n");

    printf("freq_adjust_mode = search\n");
    printf("\n");
    printf("    Keybase and detune parameters loaded from .inst file. Setting any value\n");
    printf("    implicitly toggles this mode. These options are incompatible with `explicit` mode.\n");
    printf("\n");
    printf("    --inst-file=FILE              Input .inst file to search\n");
    printf("    --inst-search=MODE            Search method to use. Available options are:\n");
    printf("                                  \"%s\"\n", INST_FILE_SEARCH_NAMES[INST_FILE_SEARCH_USE]);
    printf("                                  Finds `sound` based on trailing text of \"use\" value.\n");
    printf("                                  \"%s\"\n", INST_FILE_SEARCH_NAMES[INST_FILE_SEARCH_SOUND]);
    printf("                                  Finds `sound` with same name\n");
    printf("                                  \"%s\"\n", INST_FILE_SEARCH_NAMES[INST_FILE_SEARCH_KEYMAP]);
    printf("                                  Finds `keymap` with same name\n");
    printf("    --inst-val=TEXT               Text parameter of search.\n");

    printf("\n");
    fflush(stdout);
}

void read_opts(int argc, char **argv)
{
    int option_index = 0;
    int ch;
    int str_len;

    while ((ch = getopt_long(argc, argv, "c:t:d:p:k:d:qv", long_options, &option_index)) != -1)
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

            case 'l':
            {
                int res;
                char *pend = NULL;

                opt_loop_count = 1;

                res = strtol(optarg, &pend, 0);
                
                if (pend != NULL && *pend == '\0')
                {
                    if (errno == ERANGE)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "error (range), cannot parse loop count as integer: %s\n", optarg);
                    }

                    g_AdpcmLoopInfiniteExportCount = res;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, cannot parse loop count as integer: %s\n", optarg);
                }
            }
            break;

            case 'k':
            {
                int res;
                char *pend = NULL;

                opt_keybase = 1;

                res = strtol(optarg, &pend, 0);
                
                if (pend != NULL && *pend == '\0')
                {
                    if (errno == ERANGE)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "error (range), cannot parse keybase as integer: %s\n", optarg);
                    }

                    if (res < 0 || res > 127)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "error, keybase=%d out of range. Value must be 0-127.\n", res);
                    }

                    keybase = res;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, cannot parse keybase as integer: %s\n", optarg);
                }

                if (freq_adjust_mode == FREQ_ADJUST_DEFAULT_UNKNOWN || freq_adjust_mode == FREQ_ADJUST_NONE)
                {
                    freq_adjust_mode = FREQ_ADJUST_EXPLICIT;
                }
                else if (freq_adjust_mode != FREQ_ADJUST_EXPLICIT)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, keybase can't be specified when freq_adjust_mode=%s\n", FREQ_ADJUST_MODE_NAMES[freq_adjust_mode]);
                }
            }
            break;

            case 'd':
            {
                int res;
                char *pend = NULL;

                opt_detune = 1;

                res = strtol(optarg, &pend, 0);
                
                if (pend != NULL && *pend == '\0')
                {
                    if (errno == ERANGE)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "error (range), cannot parse detune as integer: %s\n", optarg);
                    }

                    if (res < 0 || res > 100)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "error, detune=%d out of range. Value must be 0-100.\n", res);
                    }

                    detune = res;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, cannot parse detune as integer: %s\n", optarg);
                }

                if (freq_adjust_mode == FREQ_ADJUST_DEFAULT_UNKNOWN || freq_adjust_mode == FREQ_ADJUST_NONE)
                {
                    freq_adjust_mode = FREQ_ADJUST_EXPLICIT;
                }
                else if (freq_adjust_mode != FREQ_ADJUST_EXPLICIT)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, keybase can't be specified when freq_adjust_mode=%s\n", FREQ_ADJUST_MODE_NAMES[freq_adjust_mode]);
                }
            }
            break;

            case LONG_OPT_INST_FILE:
            {
                opt_inst_file = 1;

                str_len = strlen(optarg);
                if (str_len < 1)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, inst filename not specified\n");
                }

                if (str_len > MAX_FILENAME_LEN - 1)
                {
                    str_len = MAX_FILENAME_LEN - 1;
                }

                strncpy(inst_filename, optarg, str_len);

                if (freq_adjust_mode == FREQ_ADJUST_DEFAULT_UNKNOWN || freq_adjust_mode == FREQ_ADJUST_NONE)
                {
                    freq_adjust_mode = FREQ_ADJUST_SEARCH;
                }
                else if (freq_adjust_mode != FREQ_ADJUST_SEARCH)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, inst filename can't be specified when freq_adjust_mode=%s\n", FREQ_ADJUST_MODE_NAMES[freq_adjust_mode]);
                }
            }
            break;

            case LONG_OPT_INST_SEARCH:
            {
                opt_inst_search = 1;

                str_len = strlen(optarg);
                if (str_len < 1)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, inst search not specified\n");
                }

                if (strncasecmp(optarg, INST_FILE_SEARCH_NAMES[INST_FILE_SEARCH_USE], str_len) == 0)
                {
                    inst_search_mode = INST_FILE_SEARCH_USE;
                }
                else if (strncasecmp(optarg, INST_FILE_SEARCH_NAMES[INST_FILE_SEARCH_SOUND], str_len) == 0)
                {
                    inst_search_mode = INST_FILE_SEARCH_SOUND;
                }
                else if (strncasecmp(optarg, INST_FILE_SEARCH_NAMES[INST_FILE_SEARCH_KEYMAP], str_len) == 0)
                {
                    inst_search_mode = INST_FILE_SEARCH_KEYMAP;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, inst search value not recognized:%s\n", optarg);
                }

                if (freq_adjust_mode == FREQ_ADJUST_DEFAULT_UNKNOWN || freq_adjust_mode == FREQ_ADJUST_NONE)
                {
                    freq_adjust_mode = FREQ_ADJUST_SEARCH;
                }
                else if (freq_adjust_mode != FREQ_ADJUST_SEARCH)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, inst search can't be specified when freq_adjust_mode=%s\n", FREQ_ADJUST_MODE_NAMES[freq_adjust_mode]);
                }
            }
            break;

            case LONG_OPT_INST_VAL:
            {
                opt_inst_val = 1;

                str_len = strlen(optarg);
                if (str_len < 1)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, inst val not specified\n");
                }

                if (str_len > MAX_FILENAME_LEN - 1)
                {
                    str_len = MAX_FILENAME_LEN - 1;
                }

                strncpy(inst_val, optarg, str_len);

                if (freq_adjust_mode == FREQ_ADJUST_DEFAULT_UNKNOWN || freq_adjust_mode == FREQ_ADJUST_NONE)
                {
                    freq_adjust_mode = FREQ_ADJUST_SEARCH;
                }
                else if (freq_adjust_mode != FREQ_ADJUST_SEARCH)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "error, inst val can't be specified when freq_adjust_mode=%s\n", FREQ_ADJUST_MODE_NAMES[freq_adjust_mode]);
                }
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

    if (freq_adjust_mode == FREQ_ADJUST_SEARCH)
    {
        if (!opt_inst_file)
        {
            stderr_exit(EXIT_CODE_GENERAL, "error, inst file must be specified in freq_adjust_mode=%s\n", FREQ_ADJUST_MODE_NAMES[freq_adjust_mode]);
        }

        if (!opt_inst_search)
        {
            stderr_exit(EXIT_CODE_GENERAL, "error, inst search mode be specified in freq_adjust_mode=%s\n", FREQ_ADJUST_MODE_NAMES[freq_adjust_mode]);
        }

        if (!opt_inst_val)
        {
            stderr_exit(EXIT_CODE_GENERAL, "error, inst val be specified in freq_adjust_mode=%s\n", FREQ_ADJUST_MODE_NAMES[freq_adjust_mode]);
        }
    }

    // if the user didn't provide an output filename, reuse the input filename.
    if (!opt_output_file)
    {
        change_filename_extension(input_filename, output_filename, WAV_DEFAULT_EXTENSION, MAX_FILENAME_LEN);
    }

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("g_verbosity: %d\n", g_verbosity);
        printf("opt_help_flag: %d\n", opt_help_flag);
        printf("opt_input_file: %d\n", opt_input_file);
        printf("opt_output_file: %d\n", opt_output_file);
        printf("input_filename: %s\n", input_filename);
        printf("output_filename: %s\n", output_filename);
        printf("opt_loop_count: %d\n", opt_loop_count);
        printf("g_AdpcmLoopInfiniteExportCount: %d\n", g_AdpcmLoopInfiniteExportCount);
        printf("opt_inst_file: %d\n", opt_inst_file);
        printf("opt_inst_search: %d\n", opt_inst_search);
        printf("opt_inst_val: %d\n", opt_inst_val);
        printf("inst_filename: %s\n", inst_filename);
        printf("inst_val: %s\n", inst_val);
        printf("freq_adjust_mode: %d\n", freq_adjust_mode);
        printf("inst_search_mode: %d\n", inst_search_mode);
        printf("opt_keybase: %d\n", opt_keybase);
        printf("opt_detune: %d\n", opt_detune);
        printf("keybase: %d\n", keybase);
        printf("detune: %d\n", detune);
        fflush(stdout);
    }

    if (freq_adjust_mode == FREQ_ADJUST_SEARCH)
    {
        // parse .inst file.
    }

    input_file = file_info_fopen(input_filename, "rb");
    aifc_file = AdpcmAifcFile_new_from_file(input_file);

    // done with input file
    file_info_free(input_file);
    input_file = NULL;

    wav_file = WavFile_load_from_aifc(aifc_file);

    // done with input aifc file
    AdpcmAifcFile_free(aifc_file);
    aifc_file = NULL;

    // check if freq adjustment is necessary
    if (freq_adjust_mode == FREQ_ADJUST_SEARCH || freq_adjust_mode == FREQ_ADJUST_EXPLICIT)
    {
        double freq = WavFile_get_frequency(wav_file);
        double new_freq = detune_frequency(freq, keybase, detune);

        if (g_verbosity >= VERBOSE_DEBUG)
        {
            printf("adjust wav freq to %f\n", new_freq);
        }

        WavFile_set_frequency(wav_file, new_freq);
    }

    output_file = file_info_fopen(output_filename, "wb");

    WavFile_fwrite(wav_file, output_file);

    // done with wav file
    WavFile_free(wav_file);
    wav_file = NULL;

    file_info_free(output_file);
    
    return 0;
}