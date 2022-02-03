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

/**
 * Variable length integer.
 * This implementation only supports 32 bit varint, as used by MIDI.
*/
struct var_length_int {
    /**
     * Variable length byte value.
    */
    uint32_t value;

    /**
     * Standard int value.
    */
    int32_t standard_value;

    /**
     * Number of bytes used in the variable length quantity.
    */
    int num_bytes;
};

void stderr_exit(int exit_code, const char *format, ...);
void fflush_printf(FILE *stream, const char *format, ...);

void *malloc_zero(size_t count, size_t item_size);
int mkpath(const char* path);
void reverse_into(uint8_t *dest, uint8_t *src, size_t len);
void reverse_inplace(uint8_t *arr, size_t len);
void bswap16_memcpy(void *dest, const void *src, size_t num);
void bswap32_memcpy(void *dest, const void *src, size_t num);

size_t get_file_contents(char *path, uint8_t **buffer);

struct file_info *file_info_fopen(char *filename, const char *mode);
size_t file_info_fread(struct file_info *fi, void *output_buffer, size_t size, size_t n);
size_t file_info_get_file_contents(struct file_info *fi, uint8_t **buffer);
int file_info_fseek(struct file_info *fi, long __off, int __whence);
long file_info_ftell(struct file_info *fi);
size_t file_info_fwrite(struct file_info *fi, const void *data, size_t size, size_t n);
size_t file_info_fwrite_bswap(struct file_info *fi, const void *data, size_t size, size_t n);
int file_info_fclose(struct file_info *fi);
void file_info_free(struct file_info *fi);

void parse_names(uint8_t *names_file_contents, size_t file_length, struct llist_root *names);
void get_filename(char *string, char *filename, size_t max_len);
void change_filename_extension(char *input_filename, char *output_filename, char *new_extension, size_t max_len);
int string_ends_with(const char * str, const char * suffix);

void int32_to_varint(int32_t in, struct var_length_int *varint);
void varint_value_to_int32(uint8_t *buffer, int max_bytes, struct var_length_int *varint);
void varint_copy(struct var_length_int *dest, struct var_length_int* source);

#endif