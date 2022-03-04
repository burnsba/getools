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
#ifndef _GAUDIO_COMMON_H_
#define _GAUDIO_COMMON_H_

#include "machine_config.h"
#include "debug.h"

/**
 * One indentation level.
*/
#define TEXT_INDENT "    "

/**
 * wav and aifc defaut sample size in bits
*/
#define DEFAULT_SAMPLE_SIZE   0x10

/**
 * wav and aifc default number of channels.
*/
#define DEFAULT_NUM_CHANNELS     1

/**
 * temp write buffer length.
*/
#define  WRITE_BUFFER_LEN      MAX_FILENAME_LEN

/**
 * Preprocessor macro to get length of array.
*/
#define ARRAY_LENGTH(array) (sizeof((array))/sizeof((array)[0]))

enum DATA_ENCODING {
    DATA_ENCODING_NONE = 0,

    /**
     * Little endian.
    */
    DATA_ENCODING_LSB,

    /**
     * Big endian.
    */
    DATA_ENCODING_MSB
};

extern int g_verbosity;
extern int g_term_colors;
extern int g_encode_bswap;
extern char g_write_buffer[WRITE_BUFFER_LEN];
extern char *g_output_dir;
extern size_t g_output_dir_len;
extern char *g_filename_prefix;
extern size_t g_filename_prefix_len;

#endif