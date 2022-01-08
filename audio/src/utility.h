#ifndef _GAUDIO_UTILITY_
#define _GAUDIO_UTILITY_

#include <stdlib.h>
#include <stdint.h>
#include <stdio.h>

struct file_info {
    FILE *fp;
    int _fp_state;
    size_t len;
    char *filename;
};

void *malloc_zero(size_t count, size_t item_size);
int mkpath(const char* path);
void reverse_into(uint8_t *dest, uint8_t *src, size_t len);
void reverse_inplace(uint8_t *arr, size_t len);
//void fwrite_bswap(void *data, size_t size, size_t n, FILE* fp);
size_t get_file_contents(char *path, uint8_t **buffer);
//void fwrite_or_exit(const void* buffer, size_t num_bytes, FILE *fp);
void bswap16_memcpy(void *dest, const void *src, size_t num);
void bswap32_memcpy(void *dest, const void *src, size_t num);

struct file_info *file_info_fopen(char *filename, const char *mode);
size_t file_info_fread(struct file_info *fi, void *output_buffer, size_t size, size_t n);
int file_info_fseek(struct file_info *fi, long __off, int __whence);
size_t file_info_fwrite(struct file_info *fi, const void *data, size_t size, size_t n);
size_t file_info_fwrite_bswap(struct file_info *fi, const void *data, size_t size, size_t n);
int file_info_fclose(struct file_info *fi);
void file_info_free(struct file_info *fi);


#endif