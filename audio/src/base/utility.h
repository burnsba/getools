#ifndef _GAUDIO_UTILITY_
#define _GAUDIO_UTILITY_

#include <stdlib.h>
#include <stdint.h>
#include <stdio.h>
#include <stdarg.h>
#include "llist.h"

/**
 * Max bytes supported in varint.
 * Varint struct only has methods for 32-bit int (4 bytes).
 * Internal algorithms add one to this value, but external use should always
 * remain below this.
*/
#define VAR_INT_MAX_BYTES 8

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
 * This implementation supports up to 32 bit varint, as used by MIDI.
*/
struct var_length_int {
    /**
     * Internal varint container.
     * Index zero is least significant byte.
    */
    uint8_t value_bytes[VAR_INT_MAX_BYTES + 1]; // add leading byte to keep `set` algorithm simple.

    /**
     * Standard int value.
    */
    int32_t standard_value;

    /**
     * Number of bytes used in the variable length quantity.
    */
    int num_bytes;
};

void stderr_exit(int exit_code, const char *format, ...) ATTR_NO_RETURN;
void fflush_printf(FILE *stream, const char *format, ...);
void fflush_string(FILE *stream, const char *str);

void *malloc_zero(size_t count, size_t item_size);
void malloc_resize(size_t old_size, void **ref, size_t new_size);

int mkpath(const char* path);

void reverse_into(uint8_t *dest, uint8_t *src, size_t len);
void reverse_inplace(uint8_t *arr, size_t len);
void bswap16_chunk(void *dest, const void *src, size_t num);
void bswap32_chunk(void *dest, const void *src, size_t num);
size_t dynamic_buffer_memcpy(uint8_t *data, size_t data_len, uint8_t **buffer_start, size_t buffer_pos, size_t max_buffer_len);

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
long parse_int(char *buffer);

void int32_to_varint(int32_t in, struct var_length_int *varint);
void varint_value_to_int32(uint8_t *buffer, int max_bytes, struct var_length_int *varint);
void varint_copy(struct var_length_int *dest, struct var_length_int* source);
void varint_write_value_big(uint8_t *dest, struct var_length_int* source);
void varint_write_value_little(uint8_t *dest, struct var_length_int* source);
int32_t varint_get_value_big(struct var_length_int* source);
int32_t varint_get_value_little(struct var_length_int* source);

size_t fill_16bit_buffer(
    int16_t *samples,
    size_t samples_required,
    uint8_t *sound_data,
    size_t *sound_data_pos,
    size_t sound_data_len);

void convert_s16_f64(int16_t *source, size_t len, double *dest);

#endif