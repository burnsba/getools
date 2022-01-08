 /*
 * Copyright 2022 Ben Burns
 * 
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 **/

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

/**
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
#define DEFAULT_EXTENSION       ".aifc" /* Nintendo custom .aiff format */


#define TEXT_INDENT "    "




#define INST_OBJ_ID_STRING_LEN 25

#define BANKFILE_MAGIC_BYTES 0x4231

#define DEFAULT_SAMPLE_SIZE   0x10
#define DEFAULT_NUM_CHANNELS     1



#define  WRITE_BUFFER_LEN      MAX_FILENAME_LEN
static char write_buffer[WRITE_BUFFER_LEN] = {0};



// make system #define if needed...
typedef __float80 f80;





enum {
    AL_ADPCM_WAVE = 0,
    AL_RAW16_WAVE
};

struct ALADPCMBook {
    int32_t order;
    int32_t npredictors;
    int16_t *book;        /* Must be 8-byte aligned */
};

#define ADPCM_STATE_SIZE 0x20 /* size in bytes */

struct ALADPCMloop {
    uint32_t start;
    uint32_t end;
    uint32_t count;
    uint8_t state[ADPCM_STATE_SIZE];
};

struct ALRawLoop {
    uint32_t start;
    uint32_t end;
    uint32_t count;
};

struct ALADPCMWaveInfo {
    int32_t loop_offset;
    struct ALADPCMloop *loop;
    int32_t book_offset;
    struct ALADPCMBook *book;
};

struct ALRAWWaveInfo {
    int32_t loop_offset;
    struct ALRawLoop *loop;
};

struct ALWaveTable {
    int32_t id;
    char text_id[INST_OBJ_ID_STRING_LEN];
    char *aifc_path;
    int32_t base; /* offset into .tbl file, can be zero */
    int32_t len;
    uint8_t type;
    uint8_t flags;
    uint16_t unused_padding;
    union {
        struct ALADPCMWaveInfo adpcm_wave;
        struct ALRAWWaveInfo raw_wave;
    } wave_info;
};

struct ALKeyMap {
    int32_t id;
    char text_id[INST_OBJ_ID_STRING_LEN];
    uint8_t velocity_min;
    uint8_t velocity_max;
    uint8_t key_min;
    uint8_t key_max;
    uint8_t key_base;
    int8_t detune;
};

struct ALEnvelope {
    int32_t id;
    char text_id[INST_OBJ_ID_STRING_LEN];
    int32_t attack_time;
    int32_t decay_time;
    int32_t release_time;
    uint8_t attack_volume;
    uint8_t decay_volume;
};

struct ALSound {
    int32_t id;
    char text_id[INST_OBJ_ID_STRING_LEN];
    int32_t envelope_offset;
    struct ALEnvelope *envelope;
    int32_t key_map_offset;
    struct ALKeyMap *keymap;
    int32_t wavetable_offfset;
    struct ALWaveTable *wavetable;
    uint8_t sample_pan;
    uint8_t sample_volume;
    uint8_t flags;
};

struct ALInstrument {
    int32_t id;
    char text_id[INST_OBJ_ID_STRING_LEN];
    uint8_t volume;
    uint8_t pan;
    uint8_t priority;
    uint8_t flags;
    uint8_t trem_type;
    uint8_t trem_rate;
    uint8_t trem_depth;
    uint8_t trem_delay;
    uint8_t vib_type;
    uint8_t vib_rate;
    uint8_t vib_depth;
    uint8_t vib_delay;
    int16_t bend_range;
    int16_t sound_count;
    int32_t *sound_offsets;
    struct ALSound **sounds;
};

struct ALBank {
    int32_t id;
    char text_id[INST_OBJ_ID_STRING_LEN];
    int16_t inst_count;
    uint8_t flags;
    uint8_t pad;
    int32_t sample_rate;
    int32_t percussion; /* is this a pointer? */
    int32_t *inst_offsets;
    struct ALInstrument **instruments;
};

struct ALBankFile {
    int32_t id;
    char text_id[INST_OBJ_ID_STRING_LEN];
    int16_t revision;
    int16_t bank_count;
    int32_t *bank_offsets;
    struct ALBank **banks;
};

struct AdpcmAifcSoundChunk {
    uint32_t ck_id; /* 0x53534E44 = "SSND" */
    int32_t ck_data_size;
    uint32_t offset; /* should be 0 */
    uint32_t block_size; /* should be 0 */
    uint8_t *sound_data;
};

struct AdpcmAifcApplicationChunk {
    uint32_t ck_id; /* 0x4150504C = "APPL" */
    int32_t ck_data_size;
    uint32_t application_signature; /* 0x73746F63 = "stoc" */
    uint8_t unknown;
    char code_string[11];
};

struct AdpcmAifcInstrumentChunk {
    uint32_t ck_id; /* 0x494E5354 = "INST" */
    int32_t ck_data_size;
    uint32_t application_signature; /* 0x73746F63 = "stoc" */
    uint8_t unknown;
    char code_string[11];
};

#define ADPCM_AIFC_VADPCM_CODES_NAME "VADPCMCODES"
struct AdpcmAifcCodebookChunk {
    struct AdpcmAifcApplicationChunk base;
    /* code_string is "VADPCMCODES", no terminating '\0' */
    uint16_t version; /* should be 1 */
    int16_t order;
    uint16_t nentries;
    uint8_t *table_data; /* length of the tableData field is order*nEntries*16 bytes. */
};

struct AdpcmAifcLoopData {
    int32_t start;
    int32_t end;
    int32_t count;
    uint8_t state[0x20];
};

#define ADPCM_AIFC_VADPCM_LOOPS_NAME "VADPCMLOOPS"
struct AdpcmAifcLoopChunk {
    struct AdpcmAifcApplicationChunk base;
    /* code_string is "VADPCMLOOPS", no terminating '\0' */
    uint16_t version; /* should be 1 */
    int16_t nloops;
    struct AdpcmAifcLoopData *loop_data;
};

#define ADPCM_AIFC_VADPCM_COMPRESSION_NAME "VADPCM ~4-1"
struct AdpcmAifcCommChunk {
    uint32_t ck_id; /* 0x434F4D4D = "COMM" */
    int32_t ck_data_size;
    int16_t num_channels;
    uint32_t num_sample_frames;
    int16_t sample_size;
    uint8_t sample_rate[10];    /* 80 bit float */
    uint32_t compression_type;  /* 0x56415043 = "VAPC" */
    uint8_t unknown;
    char compression_name[11];     /* "VADPCM ~ 4:1", no terminating '\0' */
};

struct AdpcmAifcFile {
    uint32_t ck_id; /* 0x464F524D = "FORM" */
    int32_t ck_data_size;
    int32_t form_type; /* 0x41494643 = "AIFC" */
    int32_t chunk_count;
    void **chunks;

    // convenience pointers
    struct AdpcmAifcCommChunk *comm_chunk;
    struct AdpcmAifcCodebookChunk *codes_chunk;
    struct AdpcmAifcSoundChunk *sound_chunk;
    struct AdpcmAifcLoopChunk *loop_chunk;
};

enum OUTPUT_MODE {
    OUTPUT_MODE_SFX = 0,
    OUTPUT_MODE_MUSIC
};

// forward declarations

struct AdpcmAifcFile *AdpcmAifcFile_new_simple(size_t chunk_count);
struct AdpcmAifcFile *AdpcmAifcFile_new_full(struct ALSound *sound, struct ALBank *bank);
struct AdpcmAifcCommChunk *AdpcmAifcCommChunk_new();
struct AdpcmAifcCodebookChunk *AdpcmAifcCodebookChunk_new(int16_t order, uint16_t nentries);
struct AdpcmAifcSoundChunk *AdpcmAifcSoundChunk_new(size_t sound_data_size_bytes);
struct AdpcmAifcLoopChunk *AdpcmAifcLoopChunk_new();

// end forward declarations

#define SUPPORTTED_OUTPUT_MODE "s m"


static int opt_help_flag = 0;
static int opt_ctl_file = 0;
static int opt_tbl_file = 0;
static int opt_dir = 0;
static int use_other_aifc_dir = 1;
static int opt_user_filename_prefix = 0;
static int opt_inst_file = 0;
static int opt_output_mode = 0;
static int opt_sample_rate = 0;
static int opt_names_file = 0;
static char dir[MAX_FILENAME_LEN] = {0};
static char filename_prefix[MAX_FILENAME_LEN] = {0};
static char ctl_filename[MAX_FILENAME_LEN] = {0};
static char tbl_filename[MAX_FILENAME_LEN] = {0};
static char inst_filename[MAX_FILENAME_LEN] = {0};
static char names_filename[MAX_FILENAME_LEN] = {0};
static struct llist_root user_names = {0};
static int output_mode = 0;
static int sample_rate = 0;
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
    {"mode",   required_argument,               NULL,   'm'  },
    {"sample-rate",required_argument,           NULL,   's'  },
    {"no-aifc",      no_argument,     &generate_aifc,   LONG_OPT_NO_AIFC  },
    {"no-inst",      no_argument,     &generate_inst,   LONG_OPT_NO_INST  },
    {"quiet",        no_argument,               NULL,  'q' },
    {"names",  required_argument,               NULL,  'n' },
    {"verbose",      no_argument,               NULL,  'v' },
    {"debug",        no_argument,               NULL,   LONG_OPT_DEBUG },
    {NULL, 0, NULL, 0}
};

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
    printf("    -m,--mode=X                   .inst file output mode. Supported options: %s\n", SUPPORTTED_OUTPUT_MODE);
    printf("                                  Default=s\n");
    printf("    --no-aifc                     don't generate .aifc files\n");
    printf("    --no-inst                     don't generate .inst file\n");
    printf("    -n,--names=FILE               sound names. One name per line. Lines starting with # ignored.\n");
    printf("                                  Names applied in order read, if the list is too short\n");
    printf("                                  subsequent sounds will be given numeric id (0001, 0002, ...).\n");
    printf("                                  Non alphanumeric characters ignored.\n");
    printf("                                  Do not include filename extension.\n");
    printf("    -q,--quiet                    suppress output\n");
    printf("    -v,--verbose                  more output\n");
    printf("\n");
    printf("\n");
    printf(".inst mode options:\n");
    printf("\n");
    printf("    sound output mode: --mode=s\n");
    printf("        no additional options\n");
    printf("\n");
    printf("    music output mode: --mode=m\n");
    printf("        -s,--sample-rate=NUM      sample rate, between 0 and 65536 HZ\n");

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
            case 'c':
            {
                opt_ctl_file = 1;

                str_len = strlen(optarg);
                if (str_len < 1)
                {
                    fprintf(stderr, "error, ctl filename not specified\n");
                    fflush(stderr);
                    exit(1);
                }

                if (str_len > MAX_FILENAME_LEN - 1)
                {
                    str_len = MAX_FILENAME_LEN - 1;
                }

                strncpy(ctl_filename, optarg, str_len);
            }
            break;

            case 't':
            {
                opt_tbl_file = 1;

                str_len = strlen(optarg);
                if (str_len < 1)
                {
                    fprintf(stderr, "error, tbl filename not specified\n");
                    fflush(stderr);
                    exit(1);
                }

                if (str_len > MAX_FILENAME_LEN - 1)
                {
                    str_len = MAX_FILENAME_LEN - 1;
                }

                strncpy(tbl_filename, optarg, str_len);
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

                if (str_len > MAX_FILENAME_LEN - 1)
                {
                    str_len = MAX_FILENAME_LEN - 1;
                }

                strncpy(dir, optarg, str_len);
            }
            break;

            case 'p':
            {
                opt_user_filename_prefix = 1;

                str_len = strlen(optarg);
                if (str_len < 1)
                {
                    fprintf(stderr, "error, filename prefix not specified\n");
                    fflush(stderr);
                    exit(1);
                }

                // 4 characters allocated for digits
                if (str_len > MAX_FILENAME_LEN - 5)
                {
                    str_len = MAX_FILENAME_LEN - 5;
                }

                strncpy(filename_prefix, optarg, str_len);
            }
            break;

            case 'n':
            {
                opt_names_file = 1;

                str_len = strlen(optarg);
                if (str_len < 1)
                {
                    fprintf(stderr, "error, names filename not specified\n");
                    fflush(stderr);
                    exit(1);
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

            case 's':
            {
                opt_sample_rate = 1;
                sample_rate = atoi(optarg);
                if (sample_rate > USHRT_MAX)
                {
                    sample_rate = USHRT_MAX;
                }

                if (sample_rate < 0)
                {
                    sample_rate = 0;
                }
            }
            break;

            case 'm':
            {
                opt_output_mode = 1;

                output_mode = -1;

                if (optarg != NULL)
                {
                    if (optarg[0] == 's' || optarg[0] == 'S')
                    {
                        output_mode = OUTPUT_MODE_SFX;
                    }
                    else if (optarg[0] == 'm' || optarg[0] == 'M')
                    {
                        output_mode = OUTPUT_MODE_MUSIC;
                    }
                }

                if (output_mode == -1)
                {
                    fprintf(stderr, "invalid output mode %s\n", optarg);
                    fflush(stderr);
                    print_help(argv[0]);
                    exit(0);
                }
            }
            break;

            case LONG_OPT_INST:
            {
                opt_inst_file = 1;

                str_len = strlen(optarg);
                if (str_len < 1)
                {
                    fprintf(stderr, "error, inst filename not specified\n");
                    fflush(stderr);
                    exit(1);
                }

                if (str_len > MAX_FILENAME_LEN - 1)
                {
                    str_len = MAX_FILENAME_LEN - 1;
                }

                strncpy(inst_filename, optarg, str_len);
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



void adpcm_loop_init_load(struct ALADPCMloop *adpcm_loop, uint8_t *ctl_file_contents, int32_t load_from_offset)
{
    TRACE_ENTER("adpcm_loop_init_load")

    int32_t input_pos = load_from_offset;

    adpcm_loop->start = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    adpcm_loop->end = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    adpcm_loop->count = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    // state

    // raw byte copy, no bswap
    memcpy(adpcm_loop->state, &ctl_file_contents[input_pos], ADPCM_STATE_SIZE);

    TRACE_LEAVE("adpcm_loop_init_load");
}

void adpcm_book_init_load(struct ALADPCMBook *adpcm_book, uint8_t *ctl_file_contents, int32_t load_from_offset)
{
    TRACE_ENTER("adpcm_book_init_load")
    
    int32_t input_pos = load_from_offset;
    int book_bytes;

    adpcm_book->order = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    adpcm_book->npredictors = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    // book, size in bytes = order * npredictors * 16
    book_bytes = adpcm_book->order * adpcm_book->npredictors * 16;

    if (book_bytes > 0)
    {
        adpcm_book->book = (int16_t *)malloc_zero(1, book_bytes);
        // raw byte copy, no bswap
        memcpy(adpcm_book->book, &ctl_file_contents[input_pos], book_bytes);
    }

    TRACE_LEAVE("adpcm_book_init_load");
}

void raw_loop_init_load(struct ALRawLoop *raw_loop, uint8_t *ctl_file_contents, int32_t load_from_offset)
{
    TRACE_ENTER("raw_loop_init_load")

    int32_t input_pos = load_from_offset;

    raw_loop->start = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    raw_loop->end = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    raw_loop->count = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    TRACE_LEAVE("raw_loop_init_load");
}

void envelope_init_load(struct ALEnvelope *envelope, uint8_t *ctl_file_contents, int32_t load_from_offset)
{
    TRACE_ENTER("envelope_init_load")

    static int32_t envelope_id = 0;
    int32_t input_pos = load_from_offset;

    envelope->id = envelope_id++;
    snprintf(envelope->text_id, INST_OBJ_ID_STRING_LEN, "Envelope%04d", envelope->id);

    envelope->attack_time = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    envelope->decay_time = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    envelope->release_time = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    envelope->attack_volume = ctl_file_contents[input_pos];
    input_pos += 1;

    envelope->decay_volume = ctl_file_contents[input_pos];
    input_pos += 1;

    if (DEBUG_OFFSET_CONSOLE && g_verbosity >= VERBOSE_DEBUG)
    {
        printf("init envelope %d\n", envelope->id);
    }

    TRACE_LEAVE("envelope_init_load");
}

void envelope_write_to_fp(struct ALEnvelope *envelope, FILE *fp)
{
    TRACE_ENTER("envelope_write_to_fp")

    int len;

    memset(write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(write_buffer, WRITE_BUFFER_LEN, "envelope %s\n", envelope->text_id);
    fp_write_or_exit(write_buffer, len, fp);

    memset(write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(write_buffer, WRITE_BUFFER_LEN, "{\n");
    fp_write_or_exit(write_buffer, len, fp);

    if (output_mode == OUTPUT_MODE_SFX)
    {
        // the following options are always written, even if zero.

        memset(write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"attackTime = %d;\n", envelope->attack_time);
        fp_write_or_exit(write_buffer, len, fp);

        memset(write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"attackVolume = %d;\n", envelope->attack_volume);
        fp_write_or_exit(write_buffer, len, fp);

        memset(write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"decayTime = %d;\n", envelope->decay_time);
        fp_write_or_exit(write_buffer, len, fp);

        memset(write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"decayVolume = %d;\n", envelope->decay_volume);
        fp_write_or_exit(write_buffer, len, fp);

        memset(write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"releaseTime = %d;\n", envelope->release_time);
        fp_write_or_exit(write_buffer, len, fp);
    }

    memset(write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(write_buffer, WRITE_BUFFER_LEN, "}\n");
    fp_write_or_exit(write_buffer, len, fp);

    memset(write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(write_buffer, WRITE_BUFFER_LEN, "\n");
    fp_write_or_exit(write_buffer, len, fp);

    TRACE_LEAVE("envelope_write_to_fp");
}

void keymap_init_load(struct ALKeyMap *keymap, uint8_t *ctl_file_contents, int32_t load_from_offset)
{
    TRACE_ENTER("keymap_init_load")

    static int32_t keymap_id = 0;
    int32_t input_pos = load_from_offset;

    keymap->id = keymap_id++;
    snprintf(keymap->text_id, INST_OBJ_ID_STRING_LEN, "Keymap%04d", keymap->id);

    keymap->velocity_min = ctl_file_contents[input_pos];
    input_pos += 1;

    keymap->velocity_max = ctl_file_contents[input_pos];
    input_pos += 1;

    keymap->key_min = ctl_file_contents[input_pos];
    input_pos += 1;

    keymap->key_max = ctl_file_contents[input_pos];
    input_pos += 1;

    keymap->key_base = ctl_file_contents[input_pos];
    input_pos += 1;

    keymap->detune = ctl_file_contents[input_pos];
    input_pos += 1;

    if (DEBUG_OFFSET_CONSOLE && g_verbosity >= VERBOSE_DEBUG)
    {
        printf("init keymap %d\n", keymap->id);
    }

    TRACE_LEAVE("keymap_init_load");
}

void keymap_write_to_fp(struct ALKeyMap *keymap, FILE *fp)
{
    TRACE_ENTER("keymap_write_to_fp")

    int len;

    memset(write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(write_buffer, WRITE_BUFFER_LEN, "keymap %s\n", keymap->text_id);
    fp_write_or_exit(write_buffer, len, fp);

    memset(write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(write_buffer, WRITE_BUFFER_LEN, "{\n");
    fp_write_or_exit(write_buffer, len, fp);

    if (output_mode == OUTPUT_MODE_SFX)
    {
        // the following options are always written, even if zero.

        memset(write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"velocityMin = %d;\n", keymap->velocity_min);
        fp_write_or_exit(write_buffer, len, fp);

        memset(write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"velocityMax = %d;\n", keymap->velocity_max);
        fp_write_or_exit(write_buffer, len, fp);

        memset(write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"keyMin = %d;\n", keymap->key_min);
        fp_write_or_exit(write_buffer, len, fp);

        memset(write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"keyMax = %d;\n", keymap->key_max);
        fp_write_or_exit(write_buffer, len, fp);

        memset(write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"keyBase = %d;\n", keymap->key_base);
        fp_write_or_exit(write_buffer, len, fp);

        memset(write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"detune = %d;\n", keymap->detune);
        fp_write_or_exit(write_buffer, len, fp);
    }

    memset(write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(write_buffer, WRITE_BUFFER_LEN, "}\n");
    fp_write_or_exit(write_buffer, len, fp);

    memset(write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(write_buffer, WRITE_BUFFER_LEN, "\n");
    fp_write_or_exit(write_buffer, len, fp);

    TRACE_LEAVE("keymap_write_to_fp");
}

void wavetable_init_load(struct ALWaveTable *wavetable, uint8_t *ctl_file_contents, int32_t load_from_offset)
{
    TRACE_ENTER("wavetable_init_load")

    static int32_t wavetable_id = 0;
    static struct llist_node *name_node = NULL;
    int32_t input_pos = load_from_offset;
    int len;

    wavetable->id = wavetable_id++;
    snprintf(wavetable->text_id, INST_OBJ_ID_STRING_LEN, "Wavetable%04d", wavetable->id);

    memset(write_buffer, 0, WRITE_BUFFER_LEN);

    // want to compare against wavetable->id before increment
    if (opt_names_file && (size_t)wavetable->id < user_names.count)
    {
        struct string_data *sd = NULL;

        if (wavetable->id == 0)
        {
            name_node = user_names.root;
        }

        if (name_node != NULL)
        {
            sd = (struct string_data *)name_node->data;
        }

        if (sd != NULL && sd->len > 0)
        {
            len = snprintf(write_buffer, WRITE_BUFFER_LEN, "%s%s%s", dir, sd->text, DEFAULT_EXTENSION);
        }
        else
        {
            len = snprintf(write_buffer, WRITE_BUFFER_LEN, "%s%s%04d%s", dir, filename_prefix, wavetable->id, DEFAULT_EXTENSION);
        }

        if (name_node != NULL)
        {
            name_node = name_node->next;
        }
    }
    else
    {
        len = snprintf(write_buffer, WRITE_BUFFER_LEN, "%s%s%04d%s", dir, filename_prefix, wavetable->id, DEFAULT_EXTENSION);
        
    }

    // write_buffer has terminating '\0', but that's not counted in len
    len++;
    wavetable->aifc_path = (char *)malloc_zero(len, 1);
    strncpy(wavetable->aifc_path, write_buffer, len);

    wavetable->base = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    wavetable->len = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    wavetable->type = ctl_file_contents[input_pos];
    input_pos += 1;

    wavetable->flags = ctl_file_contents[input_pos];
    input_pos += 1;

    // padding
    input_pos += 2;

    // it's ok to cast to the wrong pointer type at this time
    wavetable->wave_info.adpcm_wave.loop_offset = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    if (DEBUG_OFFSET_CONSOLE && g_verbosity >= VERBOSE_DEBUG)
    {
        printf("init wavetable %d. tbl base: 0x%04x, length 0x%04x\n", wavetable->id, wavetable->base, wavetable->len);
    }

    if (wavetable->type == AL_ADPCM_WAVE)
    {
        wavetable->wave_info.adpcm_wave.book_offset = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
        input_pos += 4;
    }

    if (wavetable->type == AL_ADPCM_WAVE)
    {
        if (wavetable->wave_info.adpcm_wave.loop_offset > 0)
        {
            wavetable->wave_info.adpcm_wave.loop = (struct ALADPCMloop *)malloc_zero(1, sizeof(struct ALADPCMloop));
            adpcm_loop_init_load(wavetable->wave_info.adpcm_wave.loop, ctl_file_contents, wavetable->wave_info.adpcm_wave.loop_offset);
        }

        if (wavetable->wave_info.adpcm_wave.book_offset > 0)
        {
            wavetable->wave_info.adpcm_wave.book = (struct ALADPCMBook *)malloc_zero(1, sizeof(struct ALADPCMBook));
            adpcm_book_init_load(wavetable->wave_info.adpcm_wave.book, ctl_file_contents, wavetable->wave_info.adpcm_wave.book_offset);
        }
    }
    else if (wavetable->type == AL_RAW16_WAVE)
    {
        if (wavetable->wave_info.raw_wave.loop_offset > 0)
        {
            wavetable->wave_info.raw_wave.loop = (struct ALRawLoop *)malloc_zero(1, sizeof(struct ALRawLoop));
            raw_loop_init_load(wavetable->wave_info.raw_wave.loop, ctl_file_contents, wavetable->wave_info.raw_wave.loop_offset);
        }
    }

    TRACE_LEAVE("wavetable_init_load");
}

void sound_init_load(struct ALSound *sound, uint8_t *ctl_file_contents, int32_t load_from_offset)
{
    TRACE_ENTER("sound_init_load")

    static int32_t sound_id = 0;
    int32_t input_pos = load_from_offset;

    sound->id = sound_id++;
    snprintf(sound->text_id, INST_OBJ_ID_STRING_LEN, "Sound%04d", sound->id);

    sound->envelope_offset = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    sound->key_map_offset = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    sound->wavetable_offfset = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    sound->sample_pan = ctl_file_contents[input_pos];
    input_pos += 1;

    sound->sample_volume = ctl_file_contents[input_pos];
    input_pos += 1;

    sound->flags = ctl_file_contents[input_pos];
    input_pos += 1;

    if (DEBUG_OFFSET_CONSOLE && g_verbosity >= VERBOSE_DEBUG)
    {
        printf("init sound %d\n", sound->id);
    }

    if (sound->envelope_offset > 0)
    {
        sound->envelope = (struct ALEnvelope *)malloc_zero(1, sizeof(struct ALEnvelope));
        envelope_init_load(sound->envelope, ctl_file_contents, sound->envelope_offset);
    }

    if (sound->key_map_offset > 0)
    {
        sound->keymap = (struct ALKeyMap *)malloc_zero(1, sizeof(struct ALKeyMap));
        keymap_init_load(sound->keymap, ctl_file_contents, sound->key_map_offset);
    }

    if (sound->wavetable_offfset > 0)
    {
        sound->wavetable = (struct ALWaveTable *)malloc_zero(1, sizeof(struct ALWaveTable));
        wavetable_init_load(sound->wavetable, ctl_file_contents, sound->wavetable_offfset);
    }
    
    TRACE_LEAVE("sound_init_load");
}

void sound_write_to_fp(struct ALSound *sound, FILE *fp)
{
    TRACE_ENTER("sound_write_to_fp")

    int len;

    if (sound->envelope_offset > 0)
    {
        envelope_write_to_fp(sound->envelope, fp);
    }

    if (sound->key_map_offset > 0)
    {
        keymap_write_to_fp(sound->keymap, fp);
    }

    memset(write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(write_buffer, WRITE_BUFFER_LEN, "sound %s\n", sound->text_id);
    fp_write_or_exit(write_buffer, len, fp);

    memset(write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(write_buffer, WRITE_BUFFER_LEN, "{\n");
    fp_write_or_exit(write_buffer, len, fp);

    if (output_mode == OUTPUT_MODE_SFX)
    {
        if (sound->wavetable_offfset != 0)
        {
            memset(write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"use (\"%s\");\n", sound->wavetable->aifc_path);
            fp_write_or_exit(write_buffer, len, fp);

            if (g_verbosity >= VERBOSE_DEBUG)
            {
                memset(write_buffer, 0, WRITE_BUFFER_LEN);
                len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"# wavetable_offfset = 0x%06x;\n", sound->wavetable_offfset);
                fp_write_or_exit(write_buffer, len, fp);
            }
        }

        memset(write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(write_buffer, WRITE_BUFFER_LEN, "\n");
        fp_write_or_exit(write_buffer, len, fp);
        
        if (sound->sample_pan != 0)
        {
            memset(write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"pan = %d;\n", sound->sample_pan);
            fp_write_or_exit(write_buffer, len, fp);
        }
        
        if (sound->sample_volume != 0)
        {
            memset(write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"volume = %d;\n", sound->sample_volume);
            fp_write_or_exit(write_buffer, len, fp);
        }
        
        if (sound->flags != 0)
        {
            memset(write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"flags = %d;\n", sound->flags);
            fp_write_or_exit(write_buffer, len, fp);
        }
        
        if (sound->envelope_offset > 0)
        {
            memset(write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"envelope = %s;\n", sound->envelope->text_id);
            fp_write_or_exit(write_buffer, len, fp);

            if (g_verbosity >= VERBOSE_DEBUG)
            {
                memset(write_buffer, 0, WRITE_BUFFER_LEN);
                len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"# envelope_offset = 0x%06x;\n", sound->envelope_offset);
                fp_write_or_exit(write_buffer, len, fp);
            }
        }
        
        if (sound->key_map_offset > 0)
        {
            memset(write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"keymap = %s;\n", sound->keymap->text_id);
            fp_write_or_exit(write_buffer, len, fp);

            if (g_verbosity >= VERBOSE_DEBUG)
            {
                memset(write_buffer, 0, WRITE_BUFFER_LEN);
                len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"# key_map_offset = 0x%06x;\n", sound->key_map_offset);
                fp_write_or_exit(write_buffer, len, fp);
            }
        }
    }

    memset(write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(write_buffer, WRITE_BUFFER_LEN, "}\n");
    fp_write_or_exit(write_buffer, len, fp);

    memset(write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(write_buffer, WRITE_BUFFER_LEN, "\n");
    fp_write_or_exit(write_buffer, len, fp);

    TRACE_LEAVE("sound_write_to_fp");
}

void instrument_init_load(struct ALInstrument *instrument, uint8_t *ctl_file_contents, int32_t load_from_offset)
{
    TRACE_ENTER("instrument_init_load")
    
    static int32_t instrument_id = 0;
    int32_t input_pos = load_from_offset;
    int i;

    instrument->id = instrument_id++;
    snprintf(instrument->text_id, INST_OBJ_ID_STRING_LEN, "Instrument%04d", instrument->id);

    instrument->volume = ctl_file_contents[input_pos];
    input_pos += 1;

    instrument->pan = ctl_file_contents[input_pos];
    input_pos += 1;

    instrument->priority = ctl_file_contents[input_pos];
    input_pos += 1;

    instrument->flags = ctl_file_contents[input_pos];
    input_pos += 1;

    instrument->trem_type = ctl_file_contents[input_pos];
    input_pos += 1;

    instrument->trem_rate = ctl_file_contents[input_pos];
    input_pos += 1;

    instrument->trem_depth = ctl_file_contents[input_pos];
    input_pos += 1;

    instrument->trem_delay = ctl_file_contents[input_pos];
    input_pos += 1;

    instrument->vib_type = ctl_file_contents[input_pos];
    input_pos += 1;

    instrument->vib_rate = ctl_file_contents[input_pos];
    input_pos += 1;

    instrument->vib_depth = ctl_file_contents[input_pos];
    input_pos += 1;

    instrument->vib_delay = ctl_file_contents[input_pos];
    input_pos += 1;

    instrument->bend_range = BSWAP16_INLINE(*(uint16_t*)(&ctl_file_contents[input_pos]));
    input_pos += 2;

    instrument->sound_count = BSWAP16_INLINE(*(uint16_t*)(&ctl_file_contents[input_pos]));
    input_pos += 2;

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("instrument %d has %d sounds\n", instrument->id, instrument->sound_count);
    }

    if (instrument->sound_count < 1)
    {
        return;
    }

    instrument->sound_offsets = (int32_t *)malloc_zero(instrument->sound_count, sizeof(void*));
    instrument->sounds = (struct ALSound **)malloc_zero(instrument->sound_count, sizeof(void*));

    bswap32_memcpy(instrument->sound_offsets, &ctl_file_contents[input_pos], instrument->sound_count);
    input_pos += instrument->sound_count * 4; /* 4 = sizeof offset */

    for (i=0; i<instrument->sound_count; i++)
    {
        if (instrument->sound_offsets[i] > 0)
        {
            instrument->sounds[i] = (struct ALSound *)malloc_zero(1, sizeof(struct ALSound));
            sound_init_load(instrument->sounds[i], ctl_file_contents, instrument->sound_offsets[i]);
        }
    }

    TRACE_LEAVE("instrument_init_load");
}

void instrument_write_to_fp(struct ALInstrument *instrument, FILE *fp)
{
    TRACE_ENTER("instrument_write_to_fp")

    int len;
    int i;

    for (i=0; i<instrument->sound_count; i++)
    {
        sound_write_to_fp(instrument->sounds[i], fp);
    }

    memset(write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(write_buffer, WRITE_BUFFER_LEN, "instrument %s\n", instrument->text_id);
    fp_write_or_exit(write_buffer, len, fp);

    memset(write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(write_buffer, WRITE_BUFFER_LEN, "{\n");
    fp_write_or_exit(write_buffer, len, fp);

    if (output_mode == OUTPUT_MODE_SFX)
    {
        if (instrument->volume != 0)
        {
            memset(write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"volume = %d;\n", instrument->volume);
            fp_write_or_exit(write_buffer, len, fp);
        }

        if (instrument->pan != 0)
        {
            memset(write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"pan = %d;\n", instrument->pan);
            fp_write_or_exit(write_buffer, len, fp);
        }

        if (instrument->priority != 0)
        {
            memset(write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"priority = %d;\n", instrument->priority);
            fp_write_or_exit(write_buffer, len, fp);
        }

        if (instrument->flags != 0)
        {
            memset(write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"flags = %d;\n", instrument->flags);
            fp_write_or_exit(write_buffer, len, fp);
        }

        if (instrument->trem_type != 0)
        {
            memset(write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"tremType = %d;\n", instrument->trem_type);
            fp_write_or_exit(write_buffer, len, fp);
        }

        if (instrument->trem_rate != 0)
        {
            memset(write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"tremRate = %d;\n", instrument->trem_rate);
            fp_write_or_exit(write_buffer, len, fp);
        }

        if (instrument->trem_depth != 0)
        {
            memset(write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"tremDepth = %d;\n", instrument->trem_depth);
            fp_write_or_exit(write_buffer, len, fp);
        }

        if (instrument->trem_delay != 0)
        {
            memset(write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"tremDelay = %d;\n", instrument->trem_delay);
            fp_write_or_exit(write_buffer, len, fp);
        }

        if (instrument->vib_type != 0)
        {
            memset(write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"vibType = %d;\n", instrument->vib_type);
            fp_write_or_exit(write_buffer, len, fp);
        }

        if (instrument->vib_rate != 0)
        {
            memset(write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"vibRate = %d;\n", instrument->vib_rate);
            fp_write_or_exit(write_buffer, len, fp);
        }

        if (instrument->vib_depth != 0)
        {
            memset(write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"vibDepth = %d;\n", instrument->vib_depth);
            fp_write_or_exit(write_buffer, len, fp);
        }

        if (instrument->vib_delay != 0)
        {
            memset(write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"vibDelay = %d;\n", instrument->vib_delay);
            fp_write_or_exit(write_buffer, len, fp);
        }

        if (instrument->bend_range != 0)
        {
            memset(write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"bendRange = %d;\n", instrument->bend_range);
            fp_write_or_exit(write_buffer, len, fp);
        }

        memset(write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(write_buffer, WRITE_BUFFER_LEN, "\n");
        fp_write_or_exit(write_buffer, len, fp);
        
        for (i=0; i<instrument->sound_count; i++)
        {
            memset(write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"sound [%d] = %s;\n", i, instrument->sounds[i]->text_id);
            fp_write_or_exit(write_buffer, len, fp);

            if (g_verbosity >= VERBOSE_DEBUG)
            {
                memset(write_buffer, 0, WRITE_BUFFER_LEN);
                len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"# sound_offset = 0x%06x;\n", instrument->sound_offsets[i]);
                fp_write_or_exit(write_buffer, len, fp);
            }
        }
    }

    memset(write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(write_buffer, WRITE_BUFFER_LEN, "}\n");
    fp_write_or_exit(write_buffer, len, fp);

    memset(write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(write_buffer, WRITE_BUFFER_LEN, "\n");
    fp_write_or_exit(write_buffer, len, fp);

    TRACE_LEAVE("instrument_write_to_fp");
}

void bank_init_load(struct ALBank *bank, uint8_t *ctl_file_contents, int32_t load_from_offset)
{
    TRACE_ENTER("bank_init_load")

    static int32_t bank_id = 0;
    int32_t input_pos = load_from_offset;
    int i;

    bank->id = bank_id++;
    snprintf(bank->text_id, INST_OBJ_ID_STRING_LEN, "Bank%04d", bank->id);

    bank->inst_count = BSWAP16_INLINE(*(uint16_t*)(&ctl_file_contents[input_pos]));
    input_pos += 2;

    bank->flags = ctl_file_contents[input_pos];
    input_pos += 1;

    bank->pad = ctl_file_contents[input_pos];
    input_pos += 1;

    bank->sample_rate = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    bank->percussion = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("bank %d has %d instruments\n", bank->id, bank->inst_count);
    }

    if (bank->inst_count < 1)
    {
        return;
    }

    bank->inst_offsets = (int32_t *)malloc_zero(bank->inst_count, sizeof(void*));
    bank->instruments = (struct ALInstrument **)malloc_zero(bank->inst_count, sizeof(void*));

    bswap32_memcpy(bank->inst_offsets, &ctl_file_contents[input_pos], bank->inst_count);
    input_pos += bank->inst_count * 4; /* 4 = sizeof offset */

    for (i=0; i<bank->inst_count; i++)
    {
        if (bank->inst_offsets[i] > 0)
        {
            bank->instruments[i] = (struct ALInstrument *)malloc_zero(1, sizeof(struct ALInstrument));
            instrument_init_load(bank->instruments[i], ctl_file_contents, bank->inst_offsets[i]);
        }
    }

    TRACE_LEAVE("bank_init_load");
}

void bank_write_to_fp(struct ALBank *bank, FILE *fp)
{
    TRACE_ENTER("bank_write_to_fp")

    int len;
    int i;

    for (i=0; i<bank->inst_count; i++)
    {
        instrument_write_to_fp(bank->instruments[i], fp);
    }

    memset(write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(write_buffer, WRITE_BUFFER_LEN, "bank %s\n", bank->text_id);
    fp_write_or_exit(write_buffer, len, fp);

    memset(write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(write_buffer, WRITE_BUFFER_LEN, "{\n");
    fp_write_or_exit(write_buffer, len, fp);

    if (output_mode == OUTPUT_MODE_SFX)
    {
        for (i=0; i<bank->inst_count; i++)
        {
            memset(write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"instrument [%d] = %s;\n", i, bank->instruments[i]->text_id);
            fp_write_or_exit(write_buffer, len, fp);

            if (g_verbosity >= VERBOSE_DEBUG)
            {
                memset(write_buffer, 0, WRITE_BUFFER_LEN);
                len = snprintf(write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"# inst_offset = 0x%06x;\n", bank->inst_offsets[i]);
                fp_write_or_exit(write_buffer, len, fp);
            }
        }
    }

    memset(write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(write_buffer, WRITE_BUFFER_LEN, "}\n");
    fp_write_or_exit(write_buffer, len, fp);

    memset(write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(write_buffer, WRITE_BUFFER_LEN, "\n");
    fp_write_or_exit(write_buffer, len, fp);

    TRACE_LEAVE("bank_write_to_fp");
}

/**
 * Initializes bank file.
 * Reads ctl contents into bank_file, malloc as necessary.
*/
void bank_file_init_load(struct ALBankFile *bank_file, uint8_t *ctl_file_contents)
{
    TRACE_ENTER("bank_file_init_load")

    static int32_t bank_file_id = 0;

    int32_t input_pos = 0;
    int i;

    memset(bank_file, 0, sizeof(struct ALBankFile));

    bank_file->id = bank_file_id++;
    snprintf(bank_file->text_id, INST_OBJ_ID_STRING_LEN, "BankFile%04d", bank_file->id);

    bank_file->revision = BSWAP16_INLINE(*(uint16_t*)(&ctl_file_contents[input_pos]));
    input_pos += 2;

    if (bank_file->revision != BANKFILE_MAGIC_BYTES)
    {
        fprintf(stderr, "Error reading ctl file, revision number does not match. Expected 0x%04x, read 0x%04x.\n", BANKFILE_MAGIC_BYTES, bank_file->revision);
        fflush(stderr);
        exit(1);
    }

    bank_file->bank_count = BSWAP16_INLINE(*(uint16_t*)(&ctl_file_contents[input_pos]));
    input_pos += 2;

    if (bank_file->bank_count < 1)
    {
        fprintf(stderr, "ctl count=%d, nothing to do.\n", bank_file->bank_count);
        fflush(stderr);
        exit(1);
    }

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("bank file %d has %d entries\n", bank_file->id, bank_file->bank_count);
    }

    // hmmmm, malloc should use current system's pointer size.
    bank_file->bank_offsets = (int32_t *)malloc_zero(bank_file->bank_count, sizeof(void*));
    bank_file->banks = (struct ALBank **)malloc_zero(bank_file->bank_count, sizeof(void*));

    bswap32_memcpy(bank_file->bank_offsets, &ctl_file_contents[input_pos], bank_file->bank_count);
    input_pos += bank_file->bank_count * 4; /* 4 = sizeof offset */

    for (i=0; i<bank_file->bank_count; i++)
    {
        if (bank_file->bank_offsets[i] > 0)
        {
            bank_file->banks[i] = (struct ALBank *)malloc_zero(1, sizeof(struct ALBank));
            bank_init_load(bank_file->banks[i], ctl_file_contents, bank_file->bank_offsets[i]);
        }
    }

    TRACE_LEAVE("bank_file_init_load");
}

void write_inst(struct ALBankFile *bank_file)
{
    TRACE_ENTER("write_inst")

    FILE *output;
    int i;

    output = fopen(inst_filename, "w");
    if (output == NULL)
    {
        fprintf(stderr, "Cannot open file: %s\n", inst_filename);
        fflush(stderr);
        exit(1);
    }

    for (i=0; i<bank_file->bank_count; i++)
    {
        bank_write_to_fp(bank_file->banks[i], output);
    }

    fclose(output);

    TRACE_LEAVE("write_inst");
}

struct AdpcmAifcFile *AdpcmAifcFile_new_simple(size_t chunk_count)
{
    TRACE_ENTER("AdpcmAifcFile_new_simple")

    struct AdpcmAifcFile *p = (struct AdpcmAifcFile *)malloc_zero(1, sizeof(struct AdpcmAifcFile));
    p->ck_id = 0x464F524D; /* 0x464F524D = "FORM" */
    p->form_type = 0x41494643; /* 0x41494643 = "AIFC" */

    p->chunk_count = chunk_count;
    p->chunks = (void*)malloc_zero(chunk_count, sizeof(void*));

    TRACE_LEAVE("AdpcmAifcFile_new_simple");

    return p;
}

struct AdpcmAifcFile *AdpcmAifcFile_new_full(struct ALSound *sound, struct ALBank *bank)
{
    TRACE_ENTER("AdpcmAifcFile_new_full")

    int chunk_count = 3; // COMM, APPL VADPCMCODES, SSND.

    if (sound->wavetable != NULL
        && sound->wavetable->type == AL_ADPCM_WAVE
        // it doesn't matter which wave_info we dereference here
        && sound->wavetable->wave_info.adpcm_wave.loop != NULL)
    {
        chunk_count++; // APPL VADPCMLOOPS
    }

    struct AdpcmAifcFile *aaf = AdpcmAifcFile_new_simple(chunk_count);

    aaf->chunks[0] = AdpcmAifcCommChunk_new();
    aaf->comm_chunk = aaf->chunks[0];

    if (sound->wavetable != NULL
        && sound->wavetable->type == AL_ADPCM_WAVE
        && sound->wavetable->wave_info.adpcm_wave.book != NULL)
    {
        struct ALADPCMBook *book = sound->wavetable->wave_info.adpcm_wave.book;
        aaf->chunks[1] = AdpcmAifcCodebookChunk_new(book->order, book->npredictors);
        aaf->codes_chunk = aaf->chunks[1];
    }
    else
    {
        fprintf(stderr, "Cannot find ALADPCMBook to resolve sound data, sound %s, bank %s\n", sound->text_id, bank->text_id);
        fflush(stderr);
        exit(1);
    }

    if (sound->wavetable != NULL)
    {
        if (sound->wavetable->len > 0)
        {
            aaf->chunks[2] = AdpcmAifcSoundChunk_new(sound->wavetable->len);
            aaf->sound_chunk = aaf->chunks[2];
        }
        else
        {
            fprintf(stderr, "wavetable->len is zero, sound %s, bank %s\n", sound->text_id, bank->text_id);
            fflush(stderr);
            exit(1);
        }
    }
    else
    {
        fprintf(stderr, "wavetable is NULL, sound %s, bank %s\n", sound->text_id, bank->text_id);
        fflush(stderr);
        exit(1);
    }

    if (chunk_count == 4)
    {
        aaf->chunks[3] = AdpcmAifcLoopChunk_new();
        aaf->loop_chunk = aaf->chunks[3];
    }

    TRACE_LEAVE("AdpcmAifcFile_new_full");

    return aaf;
}

struct AdpcmAifcCommChunk *AdpcmAifcCommChunk_new()
{
    TRACE_ENTER("AdpcmAifcCommChunk_new")

    struct AdpcmAifcCommChunk *p = (struct AdpcmAifcCommChunk *)malloc_zero(1, sizeof(struct AdpcmAifcCommChunk));
    p->ck_id = 0x434F4D4D; /* 0x434F4D4D = "COMM" */
    p->num_channels = DEFAULT_NUM_CHANNELS;
    p->ck_data_size = 2 + 4 + 2 + 10 + 4 + 1 + 11;
    p->unknown = 0xb;
    p->compression_type = 0x56415043; /* 0x56415043 = "VAPC" */
    
    // no terminating zero
    memcpy(p->compression_name, ADPCM_AIFC_VADPCM_COMPRESSION_NAME, 11);
    
    TRACE_LEAVE("AdpcmAifcCommChunk_new");
    
    return p;
}

struct AdpcmAifcCodebookChunk *AdpcmAifcCodebookChunk_new(int16_t order, uint16_t nentries)
{
    TRACE_ENTER("AdpcmAifcCodebookChunk_new")

    size_t table_data_size_bytes = order * nentries * 16;
    struct AdpcmAifcCodebookChunk *p = (struct AdpcmAifcCodebookChunk *)malloc_zero(1, sizeof(struct AdpcmAifcCommChunk));
    p->base.ck_id = 0x4150504C; /* 0x4150504C = "APPL" */
    p->base.ck_data_size = 4 + 1 + 11 + 2 + 2 + 2 + table_data_size_bytes;
    p->base.application_signature = 0x73746F63; /* 0x73746F63 = "stoc" */
    
    // no terminating zero
    memcpy(p->base.code_string, ADPCM_AIFC_VADPCM_CODES_NAME, 11);

    p->nentries = nentries;

    p->table_data = (uint8_t *)malloc_zero(1, table_data_size_bytes);

    TRACE_LEAVE("AdpcmAifcCodebookChunk_new");

    return p;
}

struct AdpcmAifcSoundChunk *AdpcmAifcSoundChunk_new(size_t sound_data_size_bytes)
{
    TRACE_ENTER("AdpcmAifcSoundChunk_new")

    struct AdpcmAifcSoundChunk *p = (struct AdpcmAifcSoundChunk *)malloc_zero(1, sizeof(struct AdpcmAifcSoundChunk));
    p->ck_id = 0x53534E44; /* 0x53534E44 = "SSND" */
    p->ck_data_size = 4 + 4 + sound_data_size_bytes;

    p->sound_data = (uint8_t *)malloc_zero(1, sound_data_size_bytes);

    TRACE_LEAVE("AdpcmAifcSoundChunk_new");

    return p;
}

struct AdpcmAifcLoopChunk *AdpcmAifcLoopChunk_new()
{
    TRACE_ENTER("AdpcmAifcLoopChunk_new")

    // nloops * sizeof(struct AdpcmAifcLoopData)
    size_t loop_data_size_bytes = 4 + 4 + 4 + 0x20;
    struct AdpcmAifcLoopChunk *p = (struct AdpcmAifcLoopChunk *)malloc_zero(1, sizeof(struct AdpcmAifcLoopChunk));
    p->base.ck_id = 0x4150504C; /* 0x4150504C = "APPL" */
    p->base.ck_data_size = 4 + 1 + 11 + 2 + 2 + loop_data_size_bytes;
    p->base.application_signature = 0x73746F63; /* 0x73746F63 = "stoc" */
    
    // no terminating zero
    memcpy(p->base.code_string, ADPCM_AIFC_VADPCM_LOOPS_NAME, 11);

    p->nloops = 1;

    p->loop_data = (struct AdpcmAifcLoopData *)malloc_zero(1, loop_data_size_bytes);

    TRACE_LEAVE("AdpcmAifcLoopChunk_new");

    return p;
}

void load_aifc_from_sound(struct AdpcmAifcFile *aaf, struct ALSound *sound, uint8_t *tbl_file_contents, struct ALBank *bank)
{
    TRACE_ENTER("load_aifc_from_sound")

    aaf->ck_data_size = 0;

    // COMM chunk
    aaf->comm_chunk->num_channels = DEFAULT_NUM_CHANNELS;
    aaf->comm_chunk->sample_size = DEFAULT_SAMPLE_SIZE;

    f80 float_rate = (f80)(bank->sample_rate);
    reverse_into(aaf->comm_chunk->sample_rate, (uint8_t *)&float_rate, 10);

    aaf->ck_data_size += aaf->comm_chunk->ck_data_size;

    // code book chunk
    aaf->codes_chunk->base.unknown = 0xb; // ??
    aaf->codes_chunk->version = 1; // ??
    if (sound->wavetable != NULL
        && sound->wavetable->type == AL_ADPCM_WAVE
        && sound->wavetable->wave_info.adpcm_wave.book != NULL)
    {
        int code_len;
        struct ALADPCMBook *book = sound->wavetable->wave_info.adpcm_wave.book;
        aaf->codes_chunk->order = book->order;
        aaf->codes_chunk->nentries = book->npredictors;
        code_len = book->order * book->npredictors * 16;
        memcpy(aaf->codes_chunk->table_data, book->book, code_len);

        aaf->ck_data_size += aaf->codes_chunk->base.ck_data_size;
    }

    // sound chunk
    if (sound->wavetable != NULL
        && sound->wavetable->len > 0)
    {
        if (g_verbosity >= VERBOSE_DEBUG)
        {
            printf("copying tbl data from offset 0x%06x len 0x%06x\n", sound->wavetable->base, sound->wavetable->len);
            fflush(stdout);
        }
    
        memcpy(
            aaf->sound_chunk->sound_data,
            &tbl_file_contents[sound->wavetable->base],
            sound->wavetable->len);

        // from the programming manual:
        // "The numSampleFrames field should be set to the number of bytes represented by the compressed data, not the the number of bytes used."
        // Well, the programming manual is wrong.
        // vadpcm_dec counts by 16, in how much it reads 9 bytes at a time.
        aaf->comm_chunk->num_sample_frames = (sound->wavetable->len / 9) * 16;

        aaf->ck_data_size += aaf->sound_chunk->ck_data_size;
    }

    // loop chunk
    if (aaf->loop_chunk != NULL
        && sound->wavetable != NULL
        && sound->wavetable->type == AL_ADPCM_WAVE
        && sound->wavetable->wave_info.adpcm_wave.loop != NULL)
    {
        struct ALADPCMloop *loop = sound->wavetable->wave_info.adpcm_wave.loop;

        aaf->loop_chunk->nloops = 1;
        aaf->loop_chunk->loop_data->start = loop->start;
        aaf->loop_chunk->loop_data->end = loop->end;
        aaf->loop_chunk->loop_data->count = loop->count;

        memcpy(
            aaf->loop_chunk->loop_data->state,
            loop->state,
            ADPCM_STATE_SIZE);

        aaf->ck_data_size += aaf->loop_chunk->base.ck_data_size;
    }

    TRACE_LEAVE("load_aifc_from_sound");
}

void AdpcmAifcCommChunk_frwrite(struct AdpcmAifcCommChunk *chunk, FILE *fp)
{
    TRACE_ENTER("AdpcmAifcCommChunk_frwrite")

    fwrite_bswap(&chunk->ck_id, 4, 1, fp);
    fwrite_bswap(&chunk->ck_data_size, 4, 1, fp);
    fwrite_bswap(&chunk->num_channels, 2, 1, fp);
    fwrite_bswap(&chunk->num_sample_frames, 4, 1, fp);
    fwrite_bswap(&chunk->sample_size, 2, 1, fp);
    fwrite_bswap(chunk->sample_rate, 10, 1, fp);
    fwrite_bswap(&chunk->compression_type, 4, 1, fp);
    fwrite_bswap(&chunk->unknown, 1, 1, fp);
    fwrite_bswap(chunk->compression_name, 11, 1, fp);
    
    TRACE_LEAVE("AdpcmAifcCommChunk_frwrite");
}

void AdpcmAifcApplicationChunk_frwrite(struct AdpcmAifcApplicationChunk *chunk, FILE *fp)
{
    TRACE_ENTER("AdpcmAifcApplicationChunk_frwrite")

    fwrite_bswap(&chunk->ck_id, 4, 1, fp);
    fwrite_bswap(&chunk->ck_data_size, 4, 1, fp);
    fwrite_bswap(&chunk->application_signature, 4, 1, fp);
    fwrite_bswap(&chunk->unknown, 1, 1, fp);
    fwrite_bswap(chunk->code_string, 11, 1, fp);
    
    TRACE_LEAVE("AdpcmAifcApplicationChunk_frwrite");
}

void AdpcmAifcCodebookChunk_frwrite(struct AdpcmAifcCodebookChunk *chunk, FILE *fp)
{
    TRACE_ENTER("AdpcmAifcCodebookChunk_frwrite")

    size_t table_size;

    AdpcmAifcApplicationChunk_frwrite(&chunk->base, fp);

    fwrite_bswap(&chunk->version, 2, 1, fp);
    fwrite_bswap(&chunk->order, 2, 1, fp);
    fwrite_bswap(&chunk->nentries, 2, 1, fp);

    table_size = chunk->order * chunk->nentries * 16;

    fwrite_bswap(chunk->table_data, table_size, 1, fp);

    TRACE_LEAVE("AdpcmAifcCodebookChunk_frwrite");
}

void AdpcmAifcSoundChunk_frwrite(struct AdpcmAifcSoundChunk *chunk, FILE *fp)
{
    TRACE_ENTER("AdpcmAifcSoundChunk_frwrite")

    size_t table_size;

    fwrite_bswap(&chunk->ck_id, 4, 1, fp);
    fwrite_bswap(&chunk->ck_data_size, 4, 1, fp);
    fwrite_bswap(&chunk->offset, 4, 1, fp);
    fwrite_bswap(&chunk->block_size, 4, 1, fp);
    
    table_size = chunk->ck_data_size - 8;
    if (table_size > 0 && table_size < INT32_MAX)
    {
        fwrite(chunk->sound_data, table_size, 1, fp);
    }

    TRACE_LEAVE("AdpcmAifcSoundChunk_frwrite");
}

void AdpcmAifcLoopData_frwrite(struct AdpcmAifcLoopData *loop, FILE *fp)
{
    TRACE_ENTER("AdpcmAifcLoopData_frwrite")

    fwrite_bswap(&loop->start, 4, 1, fp);
    fwrite_bswap(&loop->end, 4, 1, fp);
    fwrite_bswap(&loop->count, 4, 1, fp);
    fwrite_bswap(&loop->state, 0x20, 1, fp);

    TRACE_LEAVE("AdpcmAifcLoopData_frwrite");
}

void AdpcmAifcLoopChunk_frwrite(struct AdpcmAifcLoopChunk *chunk, FILE *fp)
{
    TRACE_ENTER("AdpcmAifcLoopChunk_frwrite")

    int i;

    AdpcmAifcApplicationChunk_frwrite(&chunk->base, fp);

    fwrite_bswap(&chunk->version, 2, 1, fp);
    fwrite_bswap(&chunk->nloops, 2, 1, fp);

    for (i=0; i<chunk->nloops; i++)
    {
        AdpcmAifcLoopData_frwrite(&chunk->loop_data[i], fp);
    }

    TRACE_LEAVE("AdpcmAifcLoopChunk_frwrite");
}

void AdpcmAifcFile_frwrite(struct AdpcmAifcFile *aaf, FILE *fp)
{
    TRACE_ENTER("AdpcmAifcFile_frwrite")

    int i;

    fwrite_bswap(&aaf->ck_id, 4, 1, fp);
    fwrite_bswap(&aaf->ck_data_size, 4, 1, fp);
    fwrite_bswap(&aaf->form_type, 4, 1, fp);

    for (i=0; i<aaf->chunk_count; i++)
    {
        uint32_t ck_id = *(uint32_t*)aaf->chunks[i];
        switch (ck_id)
        {
            case 0x434F4D4D: // COMM
            {
                struct AdpcmAifcCommChunk *chunk = (struct AdpcmAifcCommChunk *)aaf->chunks[i];
                AdpcmAifcCommChunk_frwrite(chunk, fp);
            }
            break;

            case 0x53534E44: // SSND
            {
                struct AdpcmAifcSoundChunk *chunk = (struct AdpcmAifcSoundChunk *)aaf->chunks[i];
                AdpcmAifcSoundChunk_frwrite(chunk, fp);
            }
            break;

            case 0x4150504C: // APPL
            {
                struct AdpcmAifcApplicationChunk *basechunk = (struct AdpcmAifcApplicationChunk *)aaf->chunks[i];
                if (strncmp(basechunk->code_string, ADPCM_AIFC_VADPCM_CODES_NAME, 10) == 0)
                {
                    struct AdpcmAifcCodebookChunk *chunk = (struct AdpcmAifcCodebookChunk *)basechunk;
                    AdpcmAifcCodebookChunk_frwrite(chunk, fp);
                }
                else if (strncmp(basechunk->code_string, ADPCM_AIFC_VADPCM_LOOPS_NAME, 10) == 0)
                {
                    struct AdpcmAifcLoopChunk *chunk = (struct AdpcmAifcLoopChunk *)basechunk;
                    AdpcmAifcLoopChunk_frwrite(chunk, fp);
                }
                // else, ignore unsupported
                else
                {
                    if (g_verbosity >= 2)
                    {
                        printf("AdpcmAifcFile_frwrite: APPL ignore code_string '%s'\n", basechunk->code_string);
                    }
                }
            }
            break;

            default:
                // ignore unsupported
            {
                if (g_verbosity >= 2)
                {
                    printf("AdpcmAifcFile_frwrite: ignore ck_id 0x%08x\n", ck_id);
                }
            }
            break;
        }
    }

    TRACE_LEAVE("AdpcmAifcFile_frwrite");
}

void write_sound_to_aifc(struct ALSound *sound, struct ALBank *bank, uint8_t *tbl_file_contents, FILE *fp)
{
    TRACE_ENTER("write_sound_to_aifc")

    struct AdpcmAifcFile *aaf = AdpcmAifcFile_new_full(sound, bank);

    load_aifc_from_sound(aaf, sound, tbl_file_contents, bank);

    AdpcmAifcFile_frwrite(aaf, fp);

    TRACE_LEAVE("write_sound_to_aifc");
}

void write_bank_to_aifc(struct ALBankFile *bank_file, uint8_t *tbl_file_contents)
{
    TRACE_ENTER("write_bank_to_aifc")

    FILE *output;
    int i,j,k;

    for (i=0; i<bank_file->bank_count; i++)
    {
        struct ALBank *bank = bank_file->banks[i];
        for (j=0; j<bank->inst_count; j++)
        {
            struct ALInstrument *inst = bank->instruments[j];
            for (k=0; k<inst->sound_count; k++)
            {
                struct ALSound *sound = inst->sounds[k];

                if (g_verbosity >= VERBOSE_DEBUG)
                {
                    printf("opening sound file for output aifc: \"%s\"\n", sound->wavetable->aifc_path);
                }

                output = fopen(sound->wavetable->aifc_path, "w");
                if (output == NULL)
                {
                    fprintf(stderr, "Cannot open file: %s\n", sound->wavetable->aifc_path);
                    fflush(stderr);
                    exit(1);
                }

                write_sound_to_aifc(sound, bank, tbl_file_contents, output);

                fclose(output);
            }
        }
    }

    TRACE_LEAVE("write_bank_to_aifc");
}

void parse_user_names(uint8_t *names_file_contents, size_t file_length)
{
    TRACE_ENTER("parse_user_names");

    size_t i;
    int current_len = 0;
    int trailing_space = 0;
    struct llist_root *names = &user_names;

    /**
     * states:
     * 1 - reading line, waiting for comment indicator or text
     * 2 - appending to current buffer
     * 3 - ignoring input until newline
    */
    int state = 1;

    char name_buffer[MAX_FILENAME_LEN];

    memset(name_buffer, 0, MAX_FILENAME_LEN);

    for (i=0; i<file_length; i++)
    {
        char c = (char)names_file_contents[i];

        if (state == 1)
        {
            if (
                (c >= '0' && c <= '9')
                || (c >= 'a' && c <= 'z')
                || (c >= 'A' && c <= 'Z')
                // || c == ' ' // can't start with whitespace
                || c == '-'
                || c == '_'
                || c == ','
                || c == '.'
                || c == '('
                || c == ')'
                || c == '['
                || c == ']'
                )
            {
                name_buffer[current_len] = c;
                current_len++;
                state = 2;
            }
            else if (c == '#')
            {
                state = 3;
            }
        }
        else if (state == 2)
        {
            if (
                (c >= '0' && c <= '9')
                || (c >= 'a' && c <= 'z')
                || (c >= 'A' && c <= 'Z')
                || c == '-'
                || c == '_'
                || c == ','
                || c == '.'
                || c == '('
                || c == ')'
                || c == '['
                || c == ']'
                )
            {
                name_buffer[current_len] = c;
                current_len++;
                trailing_space = 0;
            }
            else if (c == ' ')
            {
                name_buffer[current_len] = c;
                current_len++;
                trailing_space++;
            }
        }

        if (c == '\n' || c == '\r')
        {
            if (trailing_space > 0)
            {
                current_len -= trailing_space;
                name_buffer[current_len + 1] = '\0';
            }

            if (current_len > 0)
            {
                struct llist_node *node = llist_node_string_data_new();
                set_string_data((struct string_data *)node->data, name_buffer, current_len);
                llist_root_append_node(names, node);
                memset(name_buffer, 0, MAX_FILENAME_LEN);
            }

            current_len = 0;
            trailing_space = 0;
            state = 1;
        }
    }

    // last entry might not end with newline
    if (trailing_space > 0)
    {
        current_len -= trailing_space;
        name_buffer[current_len + 1] = '\0';
    }

    if (current_len > 0)
    {
        struct llist_node *node = llist_node_string_data_new();
        set_string_data((struct string_data *)node->data, name_buffer, current_len);
        llist_root_append_node(names, node);
        memset(name_buffer, 0, MAX_FILENAME_LEN);
    }

    current_len = 0;
    trailing_space = 0;
    state = 1;

    TRACE_LEAVE("parse_user_names");
}

int main(int argc, char **argv)
{
    struct ALBankFile bank_file;

    // ctl and tbl contents will be malloc'd, these should fit in RAM, should only be < 1MB at most
    uint8_t *ctl_file_contents;
    uint8_t *tbl_file_contents;
    
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
        return 1;
    }

    if (use_other_aifc_dir)
    {
        // if user didn't supply output directory use the default
        if (!opt_dir)
        {
            strncpy(dir, DEFAULT_OUT_DIR, MAX_FILENAME_LEN);
        }
        else
        {
            // else, make sure it ends witih a trailing slash
            int len = strlen(dir);
            if (len > (MAX_FILENAME_LEN - 2))
            {
                len = MAX_FILENAME_LEN - 2;
            }

            if (dir[len - 1] != PATH_SEPERATOR)
            {
                dir[len] = PATH_SEPERATOR;
                dir[len+1] = '\0';
            }
        }
    }

    // if user didn't supply filename_prefix use the default
    if (!opt_user_filename_prefix)
    {
        strncpy(filename_prefix, DEFAULT_FILENAME_PREFIX, MAX_FILENAME_LEN);
    }

    // if user didn't supply inst filename use the default
    if (!opt_inst_file)
    {
        strncpy(inst_filename, DEFAULT_INST_FILENAME, MAX_FILENAME_LEN);
    }

    if (opt_names_file)
    {
        uint8_t *names_file_contents;
        size_t file_length = 0;
        file_length = get_file_contents(names_filename, &names_file_contents);
        parse_user_names(names_file_contents, file_length);
        free(names_file_contents);

        // llist_node_string_data_print(&user_names);
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
        printf("output_mode: %d\n", output_mode);
        printf("opt_sample_rate: %d\n", opt_sample_rate);
        printf("sample_rate: %d\n", sample_rate);
        printf("generate_aifc: %d\n", generate_aifc);
        printf("generate_inst: %d\n", generate_inst);
        printf("opt_names_file: %d\n", opt_names_file);
        printf("user_names_count: %ld\n", user_names.count);
        printf("dir: %s\n", dir);
        printf("filename_prefix: %s\n", filename_prefix);
        printf("ctl_filename: %s\n", ctl_filename);
        printf("tbl_filename: %s\n", tbl_filename);
        printf("inst_filename: %s\n", inst_filename);
        printf("names_filename: %s\n", names_filename);
    }

    if (use_other_aifc_dir)
    {
        mkpath(dir);
    }

    get_file_contents(ctl_filename, &ctl_file_contents);
    get_file_contents(tbl_filename, &tbl_file_contents);

    bank_file_init_load(&bank_file, ctl_file_contents);

    if (generate_inst)
    {
        write_inst(&bank_file);
    }

    if (generate_aifc)
    {
        write_bank_to_aifc(&bank_file, tbl_file_contents);
    }

    free(ctl_file_contents);
    free(tbl_file_contents);
}

/**
 * todo:
 * - is sample rate required input option?
 * - call free()
 * - differences for mode=m
 * - documentation
 * 
*/