#include <string.h>
#include <sys/stat.h>   /* mkdir(2) */
#include <errno.h>
#include <stdarg.h>
#include "debug.h"
#include "machine_config.h"
#include "common.h"
#include "utility.h"

void stderr_exit(int exit_code, const char *format, ...)
{
    va_list args;
    va_start(args, format);
    vfprintf(stderr, format, args);
    va_end(args);

    fflush(stderr);
    exit(exit_code);
}

void fflush_printf(FILE *stream, const char *format, ...)
{
    va_list args;
    va_start(args, format);
    vfprintf(stream, format, args);
    va_end(args);

    fflush(stderr);
}


/**
 * malloc (count*item_size) number of bytes.
 * memset result to zero.
 * if malloc fails the program will exit.
*/
void *malloc_zero(size_t count, size_t item_size)
{
    TRACE_ENTER("malloc_zero")

    void *outp;
    size_t malloc_size;

    malloc_size = count * item_size;
    outp = malloc(malloc_size);
    if (outp == NULL)
    {
        perror("malloc");
        exit(1);
    }
    memset(outp, 0, malloc_size);

    TRACE_LEAVE("malloc_zero");

    return outp;
}

int mkpath(const char* path)
{
    TRACE_ENTER("mkpath")

    size_t len = strlen(path);
    char local_path[MAX_FILENAME_LEN];
    char *p;

    if (len > MAX_FILENAME_LEN)
    {
        stderr_exit(1, "error, mkpath name too long.\n");
    }

    memset(local_path, 0, MAX_FILENAME_LEN);
    strcpy(local_path, path);

    for (p = local_path + 1; *p; p++)
    {
        if (*p == PATH_SEPERATOR)
        {
            *p = '\0';

            if (mkdir(local_path, S_IRWXU) != 0)
            {
                if (errno != EEXIST)
                {
                    stderr_exit(1, "mkpath error, could not create dir at step %s.\n", local_path);
                }
            }
            else
            {
                if (g_verbosity >= VERBOSE_DEBUG)
                {
                    printf("create dir %s\n", local_path);
                }
            }

            *p = PATH_SEPERATOR;
        }
    }   

    if (mkdir(local_path, S_IRWXU) != 0)
    {
        if (errno != EEXIST)
        {
            stderr_exit(1, "mkpath error, could not create dir %s.\n", path);
        }
    }
    else
    {
        if (g_verbosity >= VERBOSE_DEBUG)
        {
            printf("create dir %s\n", local_path);
        }
    }

    TRACE_LEAVE("mkpath");

    return 0;
}

void reverse_into(uint8_t *dest, uint8_t *src, size_t len)
{
    if (dest == NULL || src == NULL || len == 0)
    {
        return;
    }

    size_t i;
    for (i=0; i<len; i++)
    {
        dest[len - i - 1] = src[i];
    }
}

void reverse_inplace(uint8_t *arr, size_t len)
{
    if (arr == NULL || len == 0)
    {
        return;
    }

    size_t i;
    for (i=0; i<len/2; i++)
    {
        uint8_t t = arr[len - i - 1];
        arr[len - i - 1] = arr[i];
        arr[i] = t;
    }
}

size_t get_file_contents(char *path, uint8_t **buffer)
{
    TRACE_ENTER("get_file_contents")

    FILE *input;

    size_t f_result;

    // length in bytes of input file
    size_t input_filesize;

    input = fopen(path, "rb");
    if (input == NULL)
    {
        stderr_exit(1, "Cannot open file: %s\n", path);
    }

    if(fseek(input, 0, SEEK_END) != 0)
    {
        fflush_printf(stderr, "error attempting to seek to end of file %s\n", path);
        fclose(input);
        exit(1);
    }

    input_filesize = ftell(input);

    if (g_verbosity > 2)
    {
        printf("file size: %s %ld\n", path, input_filesize);
    }

    if(fseek(input, 0, SEEK_SET) != 0)
    {
        fflush_printf(stderr, "error attempting to seek to beginning of file %s\n", path);
        fclose(input);
        exit(1);
    }

    if (input_filesize > MAX_INPUT_FILESIZE)
    {
        fflush_printf(stderr, "error, filesize=%ld is larger than max supported=%d\n", input_filesize, MAX_INPUT_FILESIZE);
        fclose(input);
        exit(1);
    }

    *buffer = (uint8_t *)malloc(input_filesize);
    if (*buffer == NULL)
    {
        perror("malloc");
		fclose(input);
        exit(1);
    }

    f_result = fread((void *)*buffer, 1, input_filesize, input);
    if(f_result != input_filesize || ferror(input))
    {
        fflush_printf(stderr, "error reading file [%s], expected to read %ld bytes, but read %ld\n", path, input_filesize, f_result);
		fclose(input);
        exit(1);
    }

    // done with input file, it's in memory now.
    fclose(input);

    TRACE_LEAVE("get_file_contents");

    return input_filesize;
}

struct file_info *file_info_fopen(char *filename, const char *mode)
{
    TRACE_ENTER("file_info_fopen")

    size_t filename_len = strlen(filename);

    struct file_info *fi = (struct file_info *)malloc_zero(1, sizeof(struct file_info));

    if (filename_len > 0)
    {
        fi->filename = (char *)malloc_zero(1, filename_len + 1);
        strcpy(fi->filename, filename);
        fi->filename[filename_len] = '\0';
    }

    fi->fp = fopen(filename, mode);
    if (fi->fp == NULL)
    {
        stderr_exit(1, "Cannot open file: %s\n", filename);
    }

    fi->_fp_state = 1;

    if(fseek(fi->fp, 0, SEEK_END) != 0)
    {
        fflush_printf(stderr, "error attempting to seek to end of file %s\n", filename);
        fclose(fi->fp);
        exit(1);
    }

    fi->len = ftell(fi->fp);

    if (g_verbosity > 2)
    {
        printf("filesize: %ld\n", fi->len);
    }

    if(fseek(fi->fp, 0, SEEK_SET) != 0)
    {
        fflush_printf(stderr, "error attempting to seek to beginning of file %s\n", fi->filename);
        fclose(fi->fp);
        exit(1);
    }

    if (fi->len > MAX_INPUT_FILESIZE)
    {
        fflush_printf(stderr, "error, filesize=%ld is larger than max supported=%d\n", fi->len, MAX_INPUT_FILESIZE);
        fclose(fi->fp);
        exit(1);
    }

    TRACE_LEAVE("file_info_fopen")

    return fi;
}

size_t file_info_fread(struct file_info *fi, void *output_buffer, size_t size, size_t n)
{
    TRACE_ENTER("file_info_fread")

    if (fi == NULL)
    {
        stderr_exit(1, "file_info_fread error, fi is NULL\n");
    }

    if (fi->_fp_state != 1)
    {
        stderr_exit(1, "file_info_fread error, fi->fp not valid\n");
    }

    size_t f_result = fread((void *)output_buffer, size, n, fi->fp);
    size_t num_bytes = size*n;
    if(f_result != num_bytes || ferror(fi->fp))
    {
        fflush_printf(stderr, "error reading file [%s], expected to read %ld bytes, but read %ld\n", fi->filename, num_bytes, f_result);
		fclose(fi->fp);
        exit(1);
    }

    TRACE_LEAVE("file_info_fread")

    return f_result;
}

int file_info_fseek(struct file_info *fi, long __off, int __whence)
{
    TRACE_ENTER("file_info_fseek")

    int ret = fseek(fi->fp, __off, __whence);

    if(ret != 0)
    {
        fflush_printf(stderr, "error attempting to seek to beginning of file %s\n", fi->filename);
        fclose(fi->fp);
        exit(1);
    }

    TRACE_LEAVE("file_info_fseek")

    return ret;
}

size_t file_info_fwrite(struct file_info *fi, const void *data, size_t size, size_t n)
{
    TRACE_ENTER("file_info_fwrite")

    size_t ret;
    size_t num_bytes;

    if (fi == NULL)
    {
        stderr_exit(1, "file_info_fwrite error, fi is NULL\n");
    }

    if (fi->_fp_state != 1)
    {
        stderr_exit(1, "file_info_fwrite error, fi->fp not valid\n");
    }

    ret = fwrite(data, size, n, fi->fp);

    num_bytes = size * n;
    if (ret != num_bytes || ferror(fi->fp))
    {
        fflush_printf(stderr, "error writing to file, expected to write %ld bytes, but wrote %ld\n", num_bytes, ret);
		fclose(fi->fp);
        exit(1);
    }

    TRACE_LEAVE("file_info_fwrite");

    return ret;
}

size_t file_info_fwrite_bswap(struct file_info *fi, const void *data, size_t size, size_t n)
{
    TRACE_ENTER("file_info_fwrite_bswap")
    
    size_t i;
    size_t ret = 0;
    size_t f_result = 0;
    size_t num_bytes = size;

    if (fi == NULL)
    {
        stderr_exit(1, "file_info_fwrite_bswap error, fi is NULL\n");
    }

    if (fi->_fp_state != 1)
    {
        stderr_exit(1, "file_info_fwrite_bswap error, fi->fp not valid\n");
    }

    if (size == 2)
    {
        for (i=0; i<n; i++)
        {
            uint16_t b16 = BSWAP16_INLINE(((uint16_t*)data)[i]);
            f_result = fwrite(&b16, size, 1, fi->fp);

            // num_bytes *= 1;
            if (f_result != num_bytes || ferror(fi->fp))
            {
                fflush_printf(stderr, "error writing to file, expected to write %ld bytes, but wrote %ld\n", num_bytes, f_result);
                fclose(fi->fp);
                exit(1);
            }

            ret += f_result;
        }
    }
    else if (size == 4)
    {
        for (i=0; i<n; i++)
        {
            uint32_t b32 = BSWAP32_INLINE(((uint32_t*)data)[i]);
            f_result = fwrite(&b32, size, 1, fi->fp);

            // num_bytes *= 1;
            if (ret != num_bytes || ferror(fi->fp))
            {
                fflush_printf(stderr, "error writing to file, expected to write %ld bytes, but wrote %ld\n", num_bytes, f_result);
                fclose(fi->fp);
                exit(1);
            }

            ret += f_result;
        }
    }
    else
    {
        f_result = fwrite(data, size, n, fi->fp);

        num_bytes *= n;
        if (f_result != num_bytes || ferror(fi->fp))
        {
            fflush_printf(stderr, "error writing to file, expected to write %ld bytes, but wrote %ld\n", num_bytes, f_result);
            fclose(fi->fp);
            exit(1);
        }

        ret += f_result;
    }

    TRACE_LEAVE("file_info_fwrite_bswap");

    return ret;
}

int file_info_fclose(struct file_info *fi)
{
    TRACE_ENTER("file_info_fclose")

    int ret = 0;

    if (fi->_fp_state == 1)
    {
        ret = fclose(fi->fp);
        fi->_fp_state = 0;
    }

    TRACE_LEAVE("file_info_fclose");

    return ret;
}

void file_info_free(struct file_info *fi)
{
    TRACE_ENTER("file_info_free")

    if (fi == NULL)
    {
        return;
    }

    if (fi->filename != NULL)
    {
        free(fi->filename);
    }

    if (fi->_fp_state == 1)
    {
        fclose(fi->fp);
        fi->_fp_state = 0;
    }

    free(fi);

    TRACE_LEAVE("file_info_free");
}

void bswap16_memcpy(void *dest, const void *src, size_t num)
{
    TRACE_ENTER("bswap16_memcpy")

    size_t i;

    if (num < 1)
    {
        return;
    }

    if (dest == NULL)
    {
        stderr_exit(1, "bswap16_memcpy error, dest is NULL\n");
    }

    if (src == NULL)
    {
        stderr_exit(1, "bswap16_memcpy error, src is NULL\n");
    }

    for (i=0; i<num; i++)
    {
        ((uint16_t*)dest)[i] = BSWAP16_INLINE(((uint16_t*)src)[i]);
    }

    TRACE_LEAVE("bswap16_memcpy");
}

void bswap32_memcpy(void *dest, const void *src, size_t num)
{
    TRACE_ENTER("bswap32_memcpy")

    size_t i;
    
    if (num < 1)
    {
        return;
    }

    if (dest == NULL)
    {
        stderr_exit(1, "bswap32_memcpy error, dest is NULL\n");
    }

    if (src == NULL)
    {
        stderr_exit(1, "bswap32_memcpy error, src is NULL\n");
    }

    for (i=0; i<num; i++)
    {
        ((uint32_t*)dest)[i] = BSWAP32_INLINE(((uint32_t*)src)[i]);
    }

    TRACE_LEAVE("bswap32_memcpy");
}