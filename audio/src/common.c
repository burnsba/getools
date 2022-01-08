#include "common.h"
#include "naudio.h"

int g_verbosity = 1;


int g_output_mode = OUTPUT_MODE_SFX;


char g_write_buffer[WRITE_BUFFER_LEN] = {0};
char g_output_dir[MAX_FILENAME_LEN] = {0};
char g_filename_prefix[MAX_FILENAME_LEN] = {0};