#ifndef _GAUDIO_UTILITY_
#define _GAUDIO_UTILITY_

#include <stdlib.h>
#include <stdint.h>
#include <stdio.h>

void *malloc_zero(size_t count, size_t item_size);
int mkpath(const char* path);
void reverse_into(uint8_t *dest, uint8_t *src, size_t len);
void reverse_inplace(uint8_t *arr, size_t len);
void fwrite_bswap(void *data, size_t size, size_t n, FILE* fp);
size_t get_file_contents(char *path, uint8_t **buffer);
void fp_write_or_exit(const void* buffer, size_t num_bytes, FILE *fp);
void bswap16_memcpy(void *dest, const void *src, size_t num);
void bswap32_memcpy(void *dest, const void *src, size_t num);


#endif