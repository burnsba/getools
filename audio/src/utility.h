#ifndef _GAUDIO_UTILITY_
#define _GAUDIO_UTILITY_

#include <stdlib.h>
#include <stdint.h>
#include <stdio.h>
#include <stdarg.h>
#include "llist.h"

/**
 * Container for file information.
*/
struct file_info {
    /**
     * file pointer.
    */
    FILE *fp;

    /**
     * Internal state, don't touch.
    */
    int _fp_state;

    /**
     * Size in bytes of the file.
    */
    size_t len;

    /**
     * Filename with extension.
     * Copied when opened, internal malloc.
     * Memory will be released once file_info_free is called.
    */
    char *filename;
};

void stderr_exit(int exit_code, const char *format, ...);
void fflush_printf(FILE *stream, const char *format, ...);

void *malloc_zero(size_t count, size_t item_size);
int mkpath(const char* path);
void reverse_into(uint8_t *dest, uint8_t *src, size_t len);
void reverse_inplace(uint8_t *arr, size_t len);
size_t get_file_contents(char *path, uint8_t **buffer);
void bswap16_memcpy(void *dest, const void *src, size_t num);
void bswap32_memcpy(void *dest, const void *src, size_t num);

struct file_info *file_info_fopen(char *filename, const char *mode);
size_t file_info_fread(struct file_info *fi, void *output_buffer, size_t size, size_t n);
int file_info_fseek(struct file_info *fi, long __off, int __whence);
size_t file_info_fwrite(struct file_info *fi, const void *data, size_t size, size_t n);
size_t file_info_fwrite_bswap(struct file_info *fi, const void *data, size_t size, size_t n);
int file_info_fclose(struct file_info *fi);
void file_info_free(struct file_info *fi);

void parse_names(uint8_t *names_file_contents, size_t file_length, struct llist_root *names);


#endif