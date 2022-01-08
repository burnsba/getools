#include <string.h>
#include <sys/stat.h>   /* mkdir(2) */
#include <errno.h>
#include "debug.h"
#include "machine_config.h"
#include "common.h"
#include "utility.h"



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
        exit(3);
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
        fprintf(stderr, "error, mkpath name too long.\n");
        fflush(stderr);
        return 1;
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
                    fprintf(stderr, "mkpath error, could not create dir at step %s.\n", local_path);
                    fflush(stderr);
                    return 1;
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
            fprintf(stderr, "mkpath error, could not create dir %s.\n", path);
            fflush(stderr);
            return 1;
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

void fwrite_bswap(void *data, size_t size, size_t n, FILE* fp)
{
    size_t i;

    if (size == 2)
    {
        for (i=0; i<n; i++)
        {
            uint16_t b16 = BSWAP16_INLINE(((uint16_t*)data)[i]);
            fwrite(&b16, size, n, fp);
        }
    }
    else if (size == 4)
    {
        
        for (i=0; i<n; i++)
        {
            uint32_t b32 = BSWAP32_INLINE(((uint32_t*)data)[i]);
            fwrite(&b32, size, n, fp);
        }
    }
    else
    {
        fwrite(data, size, n, fp);
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
        fprintf(stderr, "Cannot open file: %s\n", path);
        fflush(stderr);
        exit(1);
    }

    if(fseek(input, 0, SEEK_END) != 0)
    {
        fprintf(stderr, "error attempting to seek to end of file %s\n", path);
        fflush(stderr);
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
        fprintf(stderr, "error attempting to seek to beginning of file %s\n", path);
        fflush(stderr);
        fclose(input);
        exit(1);
    }

    if (input_filesize > MAX_INPUT_FILESIZE)
    {
        fprintf(stderr, "error, filesize=%ld is larger than max supported=%d\n", input_filesize, MAX_INPUT_FILESIZE);
        fflush(stderr);
        fclose(input);
        exit(2);
    }

    *buffer = (uint8_t *)malloc(input_filesize);
    if (*buffer == NULL)
    {
        perror("malloc");
		fclose(input);
        exit(3);
    }

    f_result = fread((void *)*buffer, 1, input_filesize, input);
    if(f_result != input_filesize || ferror(input))
    {
        fprintf(stderr, "error reading file [%s], expected to read %ld bytes, but read %ld\n", path, input_filesize, f_result);
        fflush(stderr);
		fclose(input);
        exit(4);
    }

    // done with input file, it's in memory now.
    fclose(input);

    TRACE_LEAVE("get_file_contents");

    return input_filesize;
}



void fp_write_or_exit(const void* buffer, size_t num_bytes, FILE *fp)
{
    TRACE_ENTER("fp_write_or_exit")

    size_t f_result;

    f_result = fwrite(buffer, 1, num_bytes, fp);
    if (f_result != num_bytes || ferror(fp))
    {
        fprintf(stderr, "error writing to file, expected to write %ld bytes, but wrote %ld\n", num_bytes, f_result);
        fflush(stderr);
		fclose(fp);
        exit(4);
    }

    TRACE_LEAVE("fp_write_or_exit");
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
        fprintf(stderr, "bswap16_memcpy error, dest is NULL\n");
        fflush(stderr);
        exit(1);
    }

    if (src == NULL)
    {
        fprintf(stderr, "bswap16_memcpy error, src is NULL\n");
        fflush(stderr);
        exit(1);
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
        fprintf(stderr, "bswap32_memcpy error, dest is NULL\n");
        fflush(stderr);
        exit(1);
    }

    if (src == NULL)
    {
        fprintf(stderr, "bswap32_memcpy error, src is NULL\n");
        fflush(stderr);
        exit(1);
    }

    for (i=0; i<num; i++)
    {
        ((uint32_t*)dest)[i] = BSWAP32_INLINE(((uint32_t*)src)[i]);
    }

    TRACE_LEAVE("bswap32_memcpy");
}