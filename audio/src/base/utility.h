/**
 * Copyright 2022 Ben Burns
*/
/**
 * This file is part of Gaudio.
 * 
 * Gaudio is free software: you can redistribute it and/or modify it under the
 * terms of the GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * Gaudio is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
 * without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with Gaudio. If not, see <https://www.gnu.org/licenses/>. 
*/
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
struct FileInfo {
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
     * Filename with extension. This may include a path, depending on
     * how this object was created / fopen was called.
     * Copied when opened, internal malloc.
     * Memory will be released once FileInfo_free is called.
    */
    char *filename;
};

/**
 * Variable length integer.
 * This implementation supports up to 32 bit varint, as used by MIDI.
*/
struct VarLengthInt {
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

struct FileInfo *FileInfo_fopen(char *filename, const char *mode);
size_t FileInfo_fread(struct FileInfo *fi, void *output_buffer, size_t size, size_t n);
size_t FileInfo_get_file_contents(struct FileInfo *fi, uint8_t **buffer);
int FileInfo_fseek(struct FileInfo *fi, long __off, int __whence);
long FileInfo_ftell(struct FileInfo *fi);
size_t FileInfo_fwrite(struct FileInfo *fi, const void *data, size_t size, size_t n);
size_t FileInfo_fwrite_bswap(struct FileInfo *fi, const void *data, size_t size, size_t n);
int FileInfo_fclose(struct FileInfo *fi);
void FileInfo_free(struct FileInfo *fi);

void parse_names(uint8_t *names_file_contents, size_t file_length, struct LinkedList *names);
void get_filename(char *string, char *filename, size_t max_len);
void change_filename_extension(char *input_filename, char *output_filename, char *new_extension, size_t max_len);
int string_ends_with(const char * str, const char * suffix);
long parse_int(char *buffer);

void int32_to_VarLengthInt(int32_t in, struct VarLengthInt *varint);
void VarLengthInt_value_to_int32(uint8_t *buffer, int max_bytes, struct VarLengthInt *varint);
void VarLengthInt_copy(struct VarLengthInt *dest, struct VarLengthInt* source);
void VarLengthInt_write_value_big(uint8_t *dest, struct VarLengthInt* source);
void VarLengthInt_write_value_little(uint8_t *dest, struct VarLengthInt* source);
int32_t VarLengthInt_get_value_big(struct VarLengthInt* source);
int32_t VarLengthInt_get_value_little(struct VarLengthInt* source);

size_t fill_16bit_buffer(
    int16_t *samples,
    size_t samples_required,
    uint8_t *sound_data,
    size_t *sound_data_pos,
    size_t sound_data_len);

void convert_s16_f64(int16_t *source, size_t len, double *dest);

#endif