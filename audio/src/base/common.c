#include "common.h"
#include "naudio.h"

/**
 * This file contains shared global variables.
*/

/**
 * Controls level of output messages.
 * 0 = silent.
 * 1 = normal.
 * 2 = some extra stuff.
 * 3 = debug info.
*/
int g_verbosity = 1;

/**
 * Output mode when extracting audio from .tbl
*/
int g_output_mode = OUTPUT_MODE_SFX;

/**
 * Temp write buffer. Can be used in any single function,
 * but state is not guaranteed to persist across
 * function calls.
*/
char g_write_buffer[WRITE_BUFFER_LEN] = {0};

/**
 * Output destination directory.
 * Should have trailing slash.
*/
char g_output_dir[MAX_FILENAME_LEN] = {0};

/**
 * Anything to prepend to output filename.
*/
char g_filename_prefix[MAX_FILENAME_LEN] = {0};

/**
 * When this flag is set, the raw sound about to be encoded into a
 * sound chunk will be byte swapped. This affects:
 * {@code AdpcmAifcFile_encode}
*/
int g_encode_bswap = 0;