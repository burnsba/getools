#ifndef _GAUDIO_MD5_H
#define _GAUDIO_MD5_H

#include <stdlib.h>

int ascii_to_int(char c);
int md5_compare(char *expected, char *actual);
void md5_hash(char *str, size_t str_len, char *digest);

#endif