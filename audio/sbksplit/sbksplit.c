// Copyright 2022 Ben Burns
// https://github.com/burnsba/getools
//
/*
*  This program is free software: you can redistribute it and/or modify it
* under the terms of the GNU General Public License as published by the Free
* Software Foundation, either version 3 of the License, or (at your option)
* any later version.
* 
* This program is distributed in the hope that it will be useful, but
* WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
* or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License
* for more details.
* 
* You should have received a copy of the GNU General Public License along
* with this program. If not, see <https://www.gnu.org/licenses/>
*/

#include "stdio.h"
#include "stdlib.h"
#include "stdint.h"
#include "string.h"
#include <getopt.h>

/**
 * This program splits a Rare .sbk file into individual .seq.rz files.
 * It does not perform any decompression.
*/

#define APPNAME "sbksplit"
#define VERSION "1.0"

#define DEFAULT_FILENAME_PREFIX "music_"
#define DEFAULT_EXTENSION       ".seq.rz" /* Rare 1172 compressed seq file */

// sanity config
#define MAX_SEQ_FILES           1024 /* arbitrary */
#define MAX_FILENAME_LEN         255
#define MAX_INPUT_FILESIZE  20000000 /* arbitrary, but this should fit on a N64 cart, soooooo */

#ifdef __sgi
// soundbank files are big endian, so don't byte swap 
#  define BSWAP16(x)
#  define BSWAP32(x)
#  define BSWAP16_MANY(x, n)
#else
// else, this is a sane environment, so need to byteswap
#  define BSWAP16(x) x = __builtin_bswap16(x);
#  define BSWAP32(x) x = __builtin_bswap32(x);
#  define BSWAP16_MANY(x, n) { s32 _i; for (_i = 0; _i < n; _i++) BSWAP16((x)[_i]) }
#endif

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

/**
 * Unused, but including for reference:
 * 
 * Metadata for a sequence "file" entry / data content of single sequence.
 * Based on original ALSeqData in n64devkit\ultra\usr\include\PR\libaudio.h.
 */
struct RareALSeqData
{
    // address is offset from the start of .sbk file
    uint8_t *address;

    // seq length after uncompressed.
    uint16_t uncompressed_len;

    // len is data segment length in the ROM. This is the 1172 compressed length.
    uint16_t len;
};

static int verbosity = 1;
static int opt_help_flag = 0;
static int opt_input_file = 0;
static int opt_user_filename_prefix = 0;
static char filename_prefix[MAX_FILENAME_LEN] = {0};
static char input_filename[MAX_FILENAME_LEN] = {0};
static char output_filename[MAX_FILENAME_LEN] = {0};

static struct option long_options[] =
{
    {"help",         no_argument,     &opt_help_flag,    1},
    {"in",     required_argument,               NULL,  'i'},
    {"prefix", required_argument,               NULL,  'p'},
    {"quiet",        no_argument,               NULL,  'q'},
    {"verbose",      no_argument,               NULL,  'v'},
    {"debug",        no_argument,               NULL,  'd'},
    {NULL, 0, NULL, 0}
};

void print_help(const char * invoke)
{
    printf("%s %s help\n", APPNAME, VERSION);
    printf("\n");
    printf("splits a Rare .sbk file into individual .seq files\n");
    printf("usage:\n");
    printf("\n");
    printf("%s -i file\n", invoke);
    printf("\n");
    printf("options:\n");
    printf("\n");
    printf("--help                        print this help\n");
    printf("-i,--in=FILE                  input file (required)\n");
    printf("-p,--prefix=STRING            string to prepend to output files. (optional)\n");
    printf("                              default=%s\n", DEFAULT_FILENAME_PREFIX);
    printf("-q,--quiet                    suppress output\n");
    printf("-v,--verbose                  more output\n");
    printf("\n");
    fflush(stdout);
}

void read_opts(int argc, char **argv)
{
    int ch;
    int str_len;

    while ((ch = getopt_long(argc, argv, "i:p:qvd", long_options, NULL)) != -1)
    {
        switch (ch)
        {
            case 'i':
            {
                opt_input_file = 1;

                str_len = strlen(optarg);
                if (str_len < 1)
                {
                    fprintf(stderr, "error, input filename not specified\n");
                    fflush(stderr);
                    exit(1);
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

            case 'd':
                verbosity = 3;
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
    FILE *input;
    FILE *output;
    // number of .seq files listed in the .sbk header
    int32_t in_seq_count = 0;
    size_t f_result;
    int32_t i;
    // this will be malloc'd, entire file should fit in RAM, should only be few hundred k at most
    uint8_t *input_file_contents;
    // input file current byte position index. only used for header section
    int32_t input_pos = 0;
    // length in bytes of input file
    size_t input_filesize;
    // offset to .seq file, from start of .sbk file
    size_t seq_address;
    // length in bytes of compressed .seq file
    uint16_t seq_len;

    read_opts(argc, argv);

    if (opt_help_flag || argc < 2 || !opt_input_file)
    {
        print_help(argv[0]);
        exit(0);
    }

    // if user didn't supply a prefix use the default
    if (!opt_user_filename_prefix)
    {
        strncpy(filename_prefix, DEFAULT_FILENAME_PREFIX, sizeof(DEFAULT_FILENAME_PREFIX));
    }

    if (verbosity > 2)
    {
        printf("opt_user_filename_prefix: %d\n", opt_user_filename_prefix);
        printf("opt_help_flag: %d\n", opt_help_flag);
        printf("input_filename: %s\n", input_filename);
        printf("filename_prefix: %s\n", filename_prefix);
        fflush(stdout);
    }

    input = fopen(input_filename, "rb");
    if (input == NULL)
    {
        fprintf(stderr, "Cannot open input file: %s\n", input_filename);
        fflush(stderr);
        fclose(input);
        return 1;
    }

    if(fseek(input, 0, SEEK_END) != 0)
    {
        fprintf(stderr, "error attempting to seek to end of input file %s\n", input_filename);
        fflush(stderr);
        fclose(input);
        return 1;
    }

    input_filesize = ftell(input);

    if (verbosity > 2)
    {
        printf("input_filesize: %d\n", input_filesize);
    }

    if(fseek(input, 0, SEEK_SET) != 0)
    {
        fprintf(stderr, "error attempting to seek to beginning of input file %s\n", input_filename);
        fflush(stderr);
        fclose(input);
        return 1;
    }

    if (input_filesize > MAX_INPUT_FILESIZE)
    {
        fprintf(stderr, "error, input_filesize=%d is larger than max supported=%d\n", input_filesize, MAX_INPUT_FILESIZE);
        fflush(stderr);
        fclose(input);
        return 2;
    }

    input_file_contents = (uint8_t *)malloc(input_filesize);
    if (input_file_contents == NULL)
    {
        perror("malloc");
		fclose(input);
        return 3;
    }

    f_result = fread((void *)input_file_contents, 1, input_filesize, input);
    if(f_result != input_filesize || ferror(input))
    {
        fprintf(stderr, "error reading input file [%s], expected to read %d bytes, but read %d\n", input_filename, input_filesize, f_result);
        fflush(stderr);
		fclose(input);
        return 4;
    }

    // done with input file, it's in memory now.
    fclose(input);

    in_seq_count = ((int32_t*)input_file_contents)[0];
    in_seq_count = BSWAP16(in_seq_count);
    if (verbosity > 0)
    {
        printf("soundbank has %d entries\n", in_seq_count);
    }

    if (in_seq_count > MAX_SEQ_FILES)
    {
        fprintf(stderr, "error, input file has too many seq files\n");
        fflush(stderr);
		return 5;
    }

    // 4 is to skip the first 32 bits (seq count + padding)
    input_pos = 4;

    // Read the .sbk header. Read the offset and length,
    // and then use those values to copy the .seq file (in memory) to the output file.
    for (i=0; i<in_seq_count; i++)
    {
        seq_address = *(size_t*)(&input_file_contents[input_pos]);
        BSWAP32(seq_address);
        input_pos +=4; /* sizeof (struct RareALSeqData.address) == 4 */

        // don't care about uncompressed_len
        input_pos +=2; /* sizeof (struct RareALSeqData.uncompressed_len) == 2 */

        seq_len = *(uint16_t*)(&input_file_contents[input_pos]);
        BSWAP16(seq_len);
        input_pos +=2; /* sizeof (struct RareALSeqData.len) == 2 */

        if (verbosity > 2)
        {
            printf("entry %d\n", i);
            printf("seq_address = %p\n", seq_address);
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
        // the getopts method should verify the prefix is within allowed length, including
        // budget for digit characters.
        sprintf(output_filename, "%s%04d%s", filename_prefix, i, DEFAULT_EXTENSION);

        output = fopen(output_filename, "wb");
        if (output == NULL)
        {
            fprintf(stderr, "Cannot open output file: %s\n", output_filename);
            fflush(stderr);
            fclose(output);
            return 6;
        }

        if (verbosity > 1)
        {
            printf("writing entry %d to output file %s\n", i, output_filename);
        }

        // write to output, straight from the input file in memory
        f_result = fwrite(&input_file_contents[(size_t)seq_address], 1, seq_len, output);
        if(f_result != seq_len || ferror(input))
        {
            fprintf(stderr, "error writing output file [%s] (entry %d), expected to write %d bytes, but wrote %d\n", output_filename, i, seq_len, f_result);
            fflush(stderr);
            fclose(input);
            return 7;
        }

        fclose(output);
    }

     return 0;
}