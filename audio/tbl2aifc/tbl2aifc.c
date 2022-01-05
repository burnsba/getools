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
#include <sys/stat.h>   /* mkdir(2) */
#include <errno.h>

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
 * 3) walk memory ALBankFile, extract .aifc from .tbl
 * 4) walk memory ALBankFile, create .inst file
 * 
*/

#define APPNAME "tbl2aifc"
#define VERSION "1.0"

#define DEFAULT_OUT_DIR         "snd/"
#define DEFAULT_INST_FILENAME   "snd.inst"
#define DEFAULT_FILENAME_PREFIX "snd_"
#define DEFAULT_EXTENSION       ".aifc" /* Nintendo custom .aiff format */

#define PATH_SEPERATOR '/'
#define TEXT_INDENT "    "

#define VERBOSE_DEBUG 3
#define THROW_NOT_IMPLEMENTED 1
#define INST_OBJ_ID_STRING_LEN 25

#define BANKFILE_MAGIC_BYTES 0x4231

// sanity config
#define MAX_FILES               1024 /* arbitrary */
#define MAX_FILENAME_LEN         255
#define MAX_INPUT_FILESIZE  20000000 /* arbitrary, but this should fit on a N64 cart, soooooo */

#define  WRITE_BUFFER_LEN      MAX_FILENAME_LEN
static char write_buffer[WRITE_BUFFER_LEN] = {0};

#ifdef __sgi
// files are big endian, so don't byte swap 
#  define BSWAP16(x)
#  define BSWAP16_INLINE(x) x
#  define BSWAP32(x)
#  define BSWAP32_INLINE(x) x
#  define BSWAP16_MANY(x, n)
#else
// else, this is a sane environment, so need to byteswap
#  define BSWAP16(x) x = __builtin_bswap16(x);
#  define BSWAP16_INLINE(x) __builtin_bswap16(x)
#  define BSWAP32(x) x = __builtin_bswap32(x);
#  define BSWAP32_INLINE(x) __builtin_bswap32(x)
#  define BSWAP16_MANY(x, n) { s32 _i; for (_i = 0; _i < n; _i++) BSWAP16((x)[_i]) }
#endif

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

enum OUTPUT_MODE {
    OUTPUT_MODE_SFX = 0,
    OUTPUT_MODE_MUSIC
};

#define SUPPORTTED_OUTPUT_MODE "s m"

static int verbosity = 1;
static int opt_help_flag = 0;
static int opt_ctl_file = 0;
static int opt_tbl_file = 0;
static int opt_dir = 0;
static int use_other_aifc_dir = 1;
static int opt_user_filename_prefix = 0;
static int opt_inst_file = 0;
static int opt_output_mode = 0;
static int opt_sample_rate = 0;
static char dir[MAX_FILENAME_LEN] = {0};
static char filename_prefix[MAX_FILENAME_LEN] = {0};
static char ctl_filename[MAX_FILENAME_LEN] = {0};
static char tbl_filename[MAX_FILENAME_LEN] = {0};
static char output_filename[MAX_FILENAME_LEN] = {0};
static char inst_filename[MAX_FILENAME_LEN] = {0};
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
    printf("                                  default=s\n");
    printf("    --no-aifc                     don't generate .aifc files\n");
    printf("    --no-inst                     don't generate .inst file\n");
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

            case 'q':
                verbosity = 0;
                break;

            case 'v':
                verbosity = 2;
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
                verbosity = VERBOSE_DEBUG;
                break;

            case '?':
                print_help(argv[0]);
                exit(0);
                break;
        }
    }
}

int mkpath(const char* path)
{
    size_t len = strlen(path);
    char local_path[MAX_FILENAME_LEN];
    char *p;

    if (len > MAX_FILENAME_LEN)
    {
        fprintf(stderr, "error, mkpath name too long.\n");
        fflush(stderr);
        return 1;
    }

    memset(local_path, 0, MAX_FILENAME_LEN);
    strcpy(local_path, path);

    for (p = local_path + 1; *p; p++)
    {
        if (*p == PATH_SEPERATOR)
        {
            *p = '\0';

            if (mkdir(local_path, S_IRWXU) != 0)
            {
                if (errno != EEXIST)
                {
                    fprintf(stderr, "mkpath error, could not create dir at step %s.\n", local_path);
                    fflush(stderr);
                    return 1;
                }
            }
            else
            {
                if (verbosity >= VERBOSE_DEBUG)
                {
                    printf("create dir %s\n", local_path);
                }
            }

            *p = PATH_SEPERATOR;
        }
    }   

    if (mkdir(local_path, S_IRWXU) != 0)
    {
        if (errno != EEXIST)
        {
            fprintf(stderr, "mkpath error, could not create dir %s.\n", path);
            fflush(stderr);
            return 1;
        }
    }
    else
    {
        if (verbosity >= VERBOSE_DEBUG)
        {
            printf("create dir %s\n", local_path);
        }
    }

    return 0;
}

void get_file_contents(char *path, uint8_t **buffer)
{
    FILE *input;

    size_t f_result;

    // length in bytes of input file
    size_t input_filesize;

    input = fopen(path, "rb");
    if (input == NULL)
    {
        fprintf(stderr, "Cannot open file: %s\n", path);
        fflush(stderr);
        exit(1);
    }

    if(fseek(input, 0, SEEK_END) != 0)
    {
        fprintf(stderr, "error attempting to seek to end of file %s\n", path);
        fflush(stderr);
        fclose(input);
        exit(1);
    }

    input_filesize = ftell(input);

    if (verbosity > 2)
    {
        printf("file size: %s %d\n", path, input_filesize);
    }

    if(fseek(input, 0, SEEK_SET) != 0)
    {
        fprintf(stderr, "error attempting to seek to beginning of file %s\n", path);
        fflush(stderr);
        fclose(input);
        exit(1);
    }

    if (input_filesize > MAX_INPUT_FILESIZE)
    {
        fprintf(stderr, "error, filesize=%d is larger than max supported=%d\n", input_filesize, MAX_INPUT_FILESIZE);
        fflush(stderr);
        fclose(input);
        exit(2);
    }

    *buffer = (uint8_t *)malloc(input_filesize);
    if (*buffer == NULL)
    {
        perror("malloc");
		fclose(input);
        exit(3);
    }

    f_result = fread((void *)*buffer, 1, input_filesize, input);
    if(f_result != input_filesize || ferror(input))
    {
        fprintf(stderr, "error reading file [%s], expected to read %d bytes, but read %d\n", path, input_filesize, f_result);
        fflush(stderr);
		fclose(input);
        exit(4);
    }

    // done with input file, it's in memory now.
    fclose(input);
}

/**
 * malloc (count*item_size) number of bytes.
 * memset result to zero.
 * if malloc fails the program will exit.
*/
void *malloc_zero(size_t count, size_t item_size)
{
    void *outp;
    size_t malloc_size;

    malloc_size = count * item_size;
    outp = malloc(malloc_size);
    if (outp == NULL)
    {
        perror("malloc");
        exit(3);
    }
    memset(outp, 0, malloc_size);

    return outp;
}

void fp_write_or_exit(const void* buffer, size_t num_bytes, FILE *fp)
{
    int f_result;

    f_result = fwrite(write_buffer, 1, num_bytes, fp);
    if (f_result != num_bytes || ferror(fp))
    {
        fprintf(stderr, "error writing to file, expected to write %d bytes, but wrote %d\n", num_bytes, f_result);
        fflush(stderr);
		fclose(fp);
        exit(4);
    }
}

void bswap16_memcpy(void *dest, const void *src, size_t num)
{
    int i;

    if (num < 1)
    {
        return;
    }

    if (dest == NULL)
    {
        fprintf(stderr, "bswap16_memcpy error, dest is NULL\n");
        fflush(stderr);
        exit(1);
    }

    if (src == NULL)
    {
        fprintf(stderr, "bswap16_memcpy error, src is NULL\n");
        fflush(stderr);
        exit(1);
    }

    for (i=0; i<num; i++)
    {
        ((uint16_t*)dest)[i] = BSWAP16_INLINE(((uint16_t*)src)[i]);
    }
}

void bswap32_memcpy(void *dest, const void *src, size_t num)
{
    int i;
    
    if (num < 1)
    {
        return;
    }

    if (dest == NULL)
    {
        fprintf(stderr, "bswap32_memcpy error, dest is NULL\n");
        fflush(stderr);
        exit(1);
    }

    if (src == NULL)
    {
        fprintf(stderr, "bswap32_memcpy error, src is NULL\n");
        fflush(stderr);
        exit(1);
    }

    for (i=0; i<num; i++)
    {
        ((uint32_t*)dest)[i] = BSWAP32_INLINE(((uint32_t*)src)[i]);
    }
}

void adpcm_loop_init_load(struct ALADPCMloop *adpcm_loop, uint8_t *ctl_file_contents, int32_t load_from_offset)
{
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
}

void adpcm_book_init_load(struct ALADPCMBook *adpcm_book, uint8_t *ctl_file_contents, int32_t load_from_offset)
{
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
}

void raw_loop_init_load(struct ALRawLoop *raw_loop, uint8_t *ctl_file_contents, int32_t load_from_offset)
{
    int32_t input_pos = load_from_offset;

    raw_loop->start = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    raw_loop->end = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    raw_loop->count = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;
}

void envelope_init_load(struct ALEnvelope *envelope, uint8_t *ctl_file_contents, int32_t load_from_offset)
{
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

    if (verbosity >= VERBOSE_DEBUG)
    {
        printf("init envelope %d\n", envelope->id);
    }
}

void envelope_write_to_fp(struct ALEnvelope *envelope, FILE *fp)
{
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
}

void keymap_init_load(struct ALKeyMap *keymap, uint8_t *ctl_file_contents, int32_t load_from_offset)
{
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

    if (verbosity >= VERBOSE_DEBUG)
    {
        printf("init keymap %d\n", keymap->id);
    }
}

void keymap_write_to_fp(struct ALKeyMap *keymap, FILE *fp)
{
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
}

void wavetable_init_load(struct ALWaveTable *wavetable, uint8_t *ctl_file_contents, int32_t load_from_offset)
{
    static int32_t wavetable_id = 0;
    int32_t input_pos = load_from_offset;
    int len;

    wavetable->id = wavetable_id++;
    snprintf(wavetable->text_id, INST_OBJ_ID_STRING_LEN, "Wavetable%04d", wavetable->id);

    memset(write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(write_buffer, WRITE_BUFFER_LEN, "%s%s%04d%s", dir, filename_prefix, wavetable->id, DEFAULT_EXTENSION);
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

    // this can cast to the wrong pointer type at this time
    wavetable->wave_info.adpcm_wave.loop_offset = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    if (verbosity >= VERBOSE_DEBUG)
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
}

void sound_init_load(struct ALSound *sound, uint8_t *ctl_file_contents, int32_t load_from_offset)
{
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

    if (verbosity >= VERBOSE_DEBUG)
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
}

void sound_write_to_fp(struct ALSound *sound, FILE *fp)
{
    int len;
    int i;

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

            if (verbosity >= VERBOSE_DEBUG)
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

            if (verbosity >= VERBOSE_DEBUG)
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

            if (verbosity >= VERBOSE_DEBUG)
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
}

void instrument_init_load(struct ALInstrument *instrument, uint8_t *ctl_file_contents, int32_t load_from_offset)
{
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

    if (verbosity >= VERBOSE_DEBUG)
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
}

void instrument_write_to_fp(struct ALInstrument *instrument, FILE *fp)
{
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

            if (verbosity >= VERBOSE_DEBUG)
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
}

void bank_init_load(struct ALBank *bank, uint8_t *ctl_file_contents, int32_t load_from_offset)
{
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

    if (verbosity >= VERBOSE_DEBUG)
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
}

void bank_write_to_fp(struct ALBank *bank, FILE *fp)
{
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

            if (verbosity >= VERBOSE_DEBUG)
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
}

/**
 * Initializes bank file.
 * Reads ctl contents into bank_file, malloc as necessary.
*/
void bank_file_init_load(struct ALBankFile *bank_file, uint8_t *ctl_file_contents)
{
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

    if (verbosity >= VERBOSE_DEBUG)
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
}

void write_inst(struct ALBankFile *bank_file)
{
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
            strncpy(dir, DEFAULT_OUT_DIR, sizeof(DEFAULT_OUT_DIR));
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
        strncpy(filename_prefix, DEFAULT_FILENAME_PREFIX, sizeof(DEFAULT_FILENAME_PREFIX));
    }

    // if user didn't supply inst filename use the default
    if (!opt_inst_file)
    {
        strncpy(inst_filename, DEFAULT_INST_FILENAME, sizeof(DEFAULT_INST_FILENAME));
    }

    if (verbosity >= VERBOSE_DEBUG)
    {
        printf("verbosity: %d\n", verbosity);
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
        printf("dir: %s\n", dir);
        printf("filename_prefix: %s\n", filename_prefix);
        printf("ctl_filename: %s\n", ctl_filename);
        printf("tbl_filename: %s\n", tbl_filename);
        printf("inst_filename: %s\n", inst_filename);
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

    free(ctl_file_contents);
    free(tbl_file_contents);
}