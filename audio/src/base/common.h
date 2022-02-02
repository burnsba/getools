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

extern int g_verbosity;
extern int g_output_mode;
extern char g_write_buffer[WRITE_BUFFER_LEN];
extern char g_output_dir[MAX_FILENAME_LEN];
extern char g_filename_prefix[MAX_FILENAME_LEN];

#endif