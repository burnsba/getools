#ifndef _GAUDIO_COMMON_H_
#define _GAUDIO_COMMON_H_

#include "machine_config.h"
#include "debug.h"

#define TEXT_INDENT "    "

#define DEFAULT_SAMPLE_SIZE   0x10
#define DEFAULT_NUM_CHANNELS     1

#define  WRITE_BUFFER_LEN      MAX_FILENAME_LEN

extern int g_verbosity;
extern int g_output_mode;
extern char g_write_buffer[WRITE_BUFFER_LEN];
extern char g_output_dir[MAX_FILENAME_LEN];
extern char g_filename_prefix[MAX_FILENAME_LEN];

#endif