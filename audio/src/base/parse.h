#ifndef _GAUDIO_PARSE_H_
#define _GAUDIO_PARSE_H_

#include "machine_config.h"

int is_whitespace(char c);
int is_newline(char c);
int is_alpha(char c);
int is_alphanumeric(char c);
int is_numeric(char c);
int is_numeric_int(char c);
int is_comment(char c);

int is_windows_newline(int c, int previous_c);

#endif