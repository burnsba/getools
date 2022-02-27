#include <string.h>
#include <sys/stat.h>   /* mkdir(2) */
#include <errno.h>
#include <stdarg.h>
#include "debug.h"
#include "machine_config.h"
#include "common.h"
#include "utility.h"
#include "llist.h"

/**
 * This file contains miscellaneous / common / utility functions.
 * Broadly:
 * - writing output
 * - file I/O
 * - memory / bit manipulation
*/

/**
 * Write printf formatted text to stderr then exit with code.
 * @param exit_code: application exit code
 * @param format: printf format string
*/
void stderr_exit(int exit_code, const char *format, ...)
{
    va_list args;
    va_start(args, format);
    vfprintf(stderr, format, args);
    va_end(args);

    fflush(stderr);
    exit(exit_code);
}

/**
 * Write printf formatted text to file stream and flush output.
 * @param stream: file stream to write to.
 * @param format: printf format string.
*/
void fflush_printf(FILE *stream, const char *format, ...)
{
    if (stream == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> stream is NULL\n", __func__, __LINE__);
    }

    va_list args;
    va_start(args, format);
    vfprintf(stream, format, args);
    va_end(args);

    fflush(stream);
}

/**
 * Write text to file stream and flush output.
 * @param stream: stream to write to.
 * @param str: text to write.
*/
void fflush_string(FILE *stream, const char *str)
{
    if (stream == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> stream is NULL\n", __func__, __LINE__);
    }

    fprintf(stream, str);
    fflush(stream);
}

/**
 * Allocates memory for (count*item_size) number of bytes
 * and memset the result to zero.
 * If malloc fails the program will exit.
*/
void *malloc_zero(size_t count, size_t item_size)
{
    TRACE_ENTER(__func__)

    void *outp;
    size_t malloc_size;

    malloc_size = count * item_size;

    if (malloc_size == 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> malloc_size is zero.\n", __func__, __LINE__);
    }

    if (malloc_size > MAX_MALLOC_SIZE)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> malloc_size=%ld exceeds sanity check max malloc size %d.\n", __func__, __LINE__, malloc_size, MAX_MALLOC_SIZE);
    }

    outp = malloc(malloc_size);
    if (outp == NULL)
    {
        perror("malloc");
        exit(EXIT_CODE_MALLOC);
    }
    memset(outp, 0, malloc_size);

    TRACE_LEAVE(__func__)

    return outp;
}

/**
 * Changes allocation for a previously allocated chunk.
 * The lesser of {@code old_size} and {@code new_size} bytes
 * are copied from the old allocation to the new.
 * The previous pointer is automatically freed.
 * @param old_size: number of bytes allocated to old size.
 * @param ref: pointer to pointer to previously allocated memory.
 * @param new_size: size in bytes to change allocation to.
*/
void malloc_resize(size_t old_size, void **ref, size_t new_size)
{
    TRACE_ENTER(__func__)

    void *temp;

    if (new_size == 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> malloc_resize to zero\n", __func__, __LINE__);
    }

    if (ref == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> ref is NULL\n", __func__, __LINE__);
    }

    if (*ref == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> *ref is NULL\n", __func__, __LINE__);
    }

    size_t lesser = old_size;
    if (new_size < old_size)
    {
        lesser = new_size;
    }

    temp = malloc_zero(1, new_size);
    memcpy(temp, *ref, lesser);
    free(*ref);

    *ref = temp;

    TRACE_LEAVE(__func__)
}

/**
 * Writes data into a buffer. If the data to write will exceed the length of the buffer,
 * the buffer is resized and contents are copied to the new buffer.
 * This resizes by a fixed factor of 1.5.
 * @param data: data to write into buffer.
 * @param data_len: number of bytes to write from data into buffer.
 * @param buffer_start: pointer to pointer of allocated memory. If the buffer is resized
 * this will point to the newly allocated memory.
 * @param buffer_pos: byte index to begin writing to in buffer.
 * @param max_buffer_len: length in bytes of the buffer.
 * @returns: the length of the buffer (the original length or resized length).
*/
size_t dynamic_buffer_memcpy(uint8_t *data, size_t data_len, uint8_t **buffer_start, size_t buffer_pos, size_t max_buffer_len)
{
    TRACE_ENTER(__func__)

    if (data == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> data is NULL\n", __func__, __LINE__);
    }

    if (buffer_start == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> buffer_start is NULL\n", __func__, __LINE__);
    }

    if (*buffer_start == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> *buffer_start is NULL\n", __func__, __LINE__);
    }

    if (buffer_pos + data_len <= max_buffer_len)
    {
        memcpy(&(*buffer_start)[buffer_pos], data, data_len);

        TRACE_LEAVE(__func__)
        return max_buffer_len;
    }

    size_t new_size = (size_t)((double)max_buffer_len * 1.5);
    malloc_resize(max_buffer_len, (void**)buffer_start, new_size);

    memcpy(&(*buffer_start)[buffer_pos], data, data_len);

    TRACE_LEAVE(__func__)

    return new_size;
}

/**
 * Creates directory and any sub directories.
 * Application exits if this fails.
 * @param path: path to final directory, including any sub directories.
 * @returns zero.
*/
int mkpath(const char* path)
{
    TRACE_ENTER(__func__)

    if (path == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> path is NULL\n", __func__, __LINE__);
    }

    size_t len = strlen(path);
    char local_path[MAX_FILENAME_LEN];
    char *p;

    if (len > MAX_FILENAME_LEN)
    {
        stderr_exit(EXIT_CODE_IO, "%s %d> error, mkpath name too long.\n", __func__, __LINE__);
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
                    stderr_exit(EXIT_CODE_IO, "%s %d> mkpath error, could not create dir at step %s.\n", __func__, __LINE__, local_path);
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
            stderr_exit(EXIT_CODE_IO, "%s %d> mkpath error, could not create dir %s.\n", __func__, __LINE__, path);
        }
    }
    else
    {
        if (g_verbosity >= VERBOSE_DEBUG)
        {
            printf("create dir %s\n", local_path);
        }
    }

    TRACE_LEAVE(__func__)

    return 0;
}

/**
 * Reverses a byte array into another byte array.
 * Both byte arrays must be at least {@code len} bytes long.
 * If either {@code dest} or {@code src} is NULL, nothing happens.
 * If {@code len} is zero, nothing happens.
 * @param dest: destination byte array.
 * @param src: source byte array.
 * @param len: length in bytes to reverse.
*/
void reverse_into(uint8_t *dest, uint8_t *src, size_t len)
{
    TRACE_ENTER(__func__)

    if (dest == NULL || src == NULL || len == 0)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    size_t i;
    for (i=0; i<len; i++)
    {
        dest[len - i - 1] = src[i];
    }

    TRACE_LEAVE(__func__)
}

/**
 * Reverses a byte array in place.
 * Byte array must be at least {@code len} bytes long.
 * If {@code arr} is NULL, nothing happens.
 * If {@code len} is zero, nothing happens.
 * @param arr: array to reverse.
 * @param len: length in bytes to reverse.
*/
void reverse_inplace(uint8_t *arr, size_t len)
{
    TRACE_ENTER(__func__)

    if (arr == NULL || len == 0)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    size_t i;
    for (i=0; i<len/2; i++)
    {
        uint8_t t = arr[len - i - 1];
        arr[len - i - 1] = arr[i];
        arr[i] = t;
    }

    TRACE_LEAVE(__func__)
}

/**
 * Reads all contents of a file into a memory buffer.
 * @param path: path of file to read.
 * @param buffer: pointer to memory array to store file contents. This should
 * not point to any allocated memory; memory will be allocated in function.
 * @returns: number of bytes read from file (also, length of buffer).
*/
size_t get_file_contents(char *path, uint8_t **buffer)
{
    TRACE_ENTER(__func__)

    if (path == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> path is NULL\n", __func__, __LINE__);
    }

    FILE *input;

    size_t f_result;

    // length in bytes of input file
    size_t input_filesize;

    input = fopen(path, "rb");
    if (input == NULL)
    {
        stderr_exit(EXIT_CODE_IO, "%s %d> Cannot open file: %s\n", __func__, __LINE__, path);
    }

    if (fseek(input, 0, SEEK_END) != 0)
    {
        fflush_printf(stderr, "%s %d> error attempting to seek to end of file %s\n", __func__, __LINE__, path);
        fclose(input);
        exit(EXIT_CODE_IO);
    }

    input_filesize = ftell(input);

    if (g_verbosity > 2)
    {
        printf("file size: %s %ld\n", path, input_filesize);
    }

    if(fseek(input, 0, SEEK_SET) != 0)
    {
        fflush_printf(stderr, "%s %d> error attempting to seek to beginning of file %s\n", __func__, __LINE__, path);
        fclose(input);
        exit(EXIT_CODE_IO);
    }

    if (input_filesize > MAX_INPUT_FILESIZE)
    {
        fflush_printf(stderr, "%s %d> error, filesize=%ld is larger than max supported=%d\n", __func__, __LINE__, input_filesize, MAX_INPUT_FILESIZE);
        fclose(input);
        exit(EXIT_CODE_IO);
    }

    *buffer = (uint8_t *)malloc_zero(1, input_filesize);

    f_result = fread((void *)*buffer, 1, input_filesize, input);
    if(f_result != input_filesize || ferror(input))
    {
        fflush_printf(stderr, "%s %d> error reading file [%s], expected to read %ld bytes, but read %ld\n", __func__, __LINE__, path, input_filesize, f_result);
		fclose(input);
        exit(EXIT_CODE_IO);
    }

    // done with input file, it's in memory now.
    fclose(input);

    TRACE_LEAVE(__func__)

    return input_filesize;
}

/**
 * Reads all contents of a file into a memory buffer.
 * @param fi: file to read.
 * @param buffer: pointer to memory array to store file contents. This should
 * not point to any allocated memory; memory will be allocated in function.
 * @returns: number of bytes read from file (also, length of buffer).
*/
size_t FileInfo_get_file_contents(struct FileInfo *fi, uint8_t **buffer)
{
    TRACE_ENTER(__func__)

    if (fi == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> fi is NULL\n", __func__, __LINE__);
    }

    size_t f_result;

    FileInfo_fseek(fi, 0, SEEK_SET);

    *buffer = (uint8_t *)malloc_zero(1, fi->len);

    f_result = FileInfo_fread(fi, *buffer, fi->len, 1);

    TRACE_LEAVE(__func__)

    return f_result;
}

/**
 * struct FileInfo wrapper to fopen.
 * Allocates memory for filename, sets internal state, and file length.
 * @param filename: path/filename to open.
 * @param mode: file open mode.
 * @returns: pointer to new {@code struct FileInfo} that was allocated.
*/
struct FileInfo *FileInfo_fopen(char *filename, const char *mode)
{
    TRACE_ENTER(__func__)

    if (filename == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> filename is NULL\n", __func__, __LINE__);
    }

    struct stat st;

    size_t filename_len = strlen(filename);

    struct FileInfo *fi = (struct FileInfo *)malloc_zero(1, sizeof(struct FileInfo));

    if (filename_len > 0)
    {
        fi->filename = (char *)malloc_zero(1, filename_len + 1);
        strcpy(fi->filename, filename);
        fi->filename[filename_len] = '\0';
    }

    fi->fp = fopen(filename, mode);
    if (fi->fp == NULL)
    {
        stderr_exit(EXIT_CODE_IO, "%s %d> Cannot open file: %s\n", __func__, __LINE__, filename);
    }

    stat(filename, &st);
    fi->len = st.st_size;

    fi->_fp_state = 1;

    if(fseek(fi->fp, 0, SEEK_END) != 0)
    {
        fflush_printf(stderr, "%s %d> error attempting to seek to end of file %s\n", __func__, __LINE__, filename);
        fclose(fi->fp);
        exit(EXIT_CODE_IO);
    }

    if (g_verbosity > 2)
    {
        if (strstr(mode, "w") != NULL)
        {
            printf("Open file with truncate \"%s\"\n", filename);
        }
        else
        {
            printf("Open existing file \"%s\", filesize: %ld\n", filename, fi->len);
        }
    }

    if(fseek(fi->fp, 0, SEEK_SET) != 0)
    {
        fflush_printf(stderr, "%s %d> error attempting to seek to beginning of file %s\n", __func__, __LINE__, fi->filename);
        fclose(fi->fp);
        exit(EXIT_CODE_IO);
    }

    if (fi->len > MAX_INPUT_FILESIZE)
    {
        fflush_printf(stderr, "%s %d> error, filesize=%ld is larger than max supported=%d\n", __func__, __LINE__, fi->len, MAX_INPUT_FILESIZE);
        fclose(fi->fp);
        exit(EXIT_CODE_GENERAL);
    }

    TRACE_LEAVE(__func__)

    return fi;
}

/**
 * struct FileInfo wrapper to fread.
 * @param fi: FileInfo.
 * @param output_buffer: buffer to write read result into. Thus must already be allocated.
 * @param size: size of each element to read.
 * @param n: number of elements to read.
 * @returns: number of bytes read (not number of elements read like fread).
*/
size_t FileInfo_fread(struct FileInfo *fi, void *output_buffer, size_t size, size_t n)
{
    TRACE_ENTER(__func__)

    if (fi == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> fi is NULL\n", __func__, __LINE__);
    }

    if (output_buffer == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> output_buffer is NULL\n", __func__, __LINE__);
    }

    size_t num_bytes = size*n;

    if (fi == NULL)
    {
        stderr_exit(EXIT_CODE_IO, "%s %d> fi is NULL\n", __func__, __LINE__);
    }

    if (fi->_fp_state != 1)
    {
        stderr_exit(EXIT_CODE_IO, "%s %d> fi->fp not valid\n", __func__, __LINE__);
    }

    size_t f_result = fread((void *)output_buffer, size, n, fi->fp);
    
    if(f_result != n || ferror(fi->fp))
    {
        fflush_printf(stderr, "%s %d> error reading file [%s], expected to read %ld elements, but read %ld\n", __func__, __LINE__, fi->filename, n, f_result);
		fclose(fi->fp);
        exit(EXIT_CODE_IO);
    }

    TRACE_LEAVE(__func__)

    return num_bytes;
}

/**
 * struct FileInfo wrapper to fseek.
 * @param fi: FileInfo.
 * @param __off: seek amount offset.
 * @param __whence: from where to seek. (SEEK_SET, SEEK_CUR, SEEK_END)
 * @returns: fseek result.
*/
int FileInfo_fseek(struct FileInfo *fi, long __off, int __whence)
{
    TRACE_ENTER(__func__)

    if (fi == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> fi is NULL\n", __func__, __LINE__);
    }

    if (fi->_fp_state != 1)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> error, fi->fp not valid\n", __func__, __LINE__);
    }

    int ret = fseek(fi->fp, __off, __whence);

    if (ret != 0)
    {
        int fseek_errno = errno;
        fflush_printf(stderr, "%s %d> error attempting to seek file %s, offset=%ld, whence=%d, return=%d, errno=%d\n", __func__, __LINE__, fi->filename, __off, __whence, ret, fseek_errno);
        fclose(fi->fp);
        exit(EXIT_CODE_IO);
    }

    TRACE_LEAVE(__func__)

    return ret;
}

/**
 * Returns current position of underlying file stream.
 * @param fi: FileInfo.
 * @returns: ftell result.
*/
long FileInfo_ftell(struct FileInfo *fi)
{
    TRACE_ENTER(__func__)

    if (fi == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> error, fi is NULL\n", __func__, __LINE__);
    }

    if (fi->_fp_state != 1)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> error, fi->fp not valid\n", __func__, __LINE__);
    }

    long ret = ftell(fi->fp);

    TRACE_LEAVE(__func__)

    return ret;
}

/**
 * struct FileInfo wrapper to fwrite.
 * @param fi: FileInfo.
 * @param data: data to write.
 * @param size: size of each element to write.
 * @param n: number of elements to write.
 * @returns: number of bytes written (not number of elements written like fwrite).
*/
size_t FileInfo_fwrite(struct FileInfo *fi, const void *data, size_t size, size_t n)
{
    TRACE_ENTER(__func__)

    size_t ret;
    size_t num_bytes;

    num_bytes = size * n;

    if (fi == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> error, fi is NULL\n", __func__, __LINE__);
    }

    if (fi->_fp_state != 1)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> error, fi->fp not valid\n", __func__, __LINE__);
    }

    if (size == 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> element size is zero.\n", __func__, __LINE__);
    }

    ret = fwrite(data, size, n, fi->fp);
    
    if (ret != n || ferror(fi->fp))
    {
        fflush_printf(stderr, "%s %d> error writing to file, expected to write %ld elements, but wrote %ld\n", __func__, __LINE__, n, ret);
		fclose(fi->fp);
        exit(EXIT_CODE_IO);
    }

    TRACE_LEAVE(__func__)

    return num_bytes;
}

/**
 * struct FileInfo wrapper to fwrite, with possible byte swapping before writing.
 * If the size of the element(s) being written is 2 or 4 then the value will be
 * swapped via 16 bit swap or 32 bit swap respectively. Otherwise the value
 * is written unchanged.
 * @param fi: FileInfo.
 * @param data: data to write.
 * @param size: size of each element to write.
 * @param n: number of elements to write.
 * @returns: number of bytes written (not number of elements written like fwrite).
*/
size_t FileInfo_fwrite_bswap(struct FileInfo *fi, const void *data, size_t size, size_t n)
{
    TRACE_ENTER(__func__)
    
    size_t i;
    size_t ret = 0;
    size_t f_result = 0;

    if (fi == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> error, fi is NULL\n", __func__, __LINE__);
    }

    if (fi->_fp_state != 1)
    {
        stderr_exit(EXIT_CODE_IO, "%s %d> error, fi->fp not valid\n", __func__, __LINE__);
    }

    if (size == 2)
    {
        for (i=0; i<n; i++)
        {
            uint16_t b16 = BSWAP16_INLINE(((uint16_t*)data)[i]);
            f_result = fwrite(&b16, size, 1, fi->fp);

            if (f_result != 1 || ferror(fi->fp))
            {
                fflush_printf(stderr, "%s %d> error writing to file, expected to write 1 element, but wrote %ld\n", __func__, __LINE__, f_result);
                fclose(fi->fp);
                exit(EXIT_CODE_IO);
            }

            ret += size;
        }
    }
    else if (size == 4)
    {
        for (i=0; i<n; i++)
        {
            uint32_t b32 = BSWAP32_INLINE(((uint32_t*)data)[i]);
            f_result = fwrite(&b32, size, 1, fi->fp);

            if (f_result != 1 || ferror(fi->fp))
            {
                fflush_printf(stderr, "%s %d> error writing to file, expected to write 1 element, but wrote %ld\n", __func__, __LINE__, 1, f_result);
                fclose(fi->fp);
                exit(EXIT_CODE_IO);
            }

            ret += size;
        }
    }
    else
    {
        f_result = fwrite(data, size, n, fi->fp);

        if (f_result != n || ferror(fi->fp))
        {
            fflush_printf(stderr, "%s %d> error writing to file, expected to write %ld elements, but wrote %ld\n", __func__, __LINE__, n, f_result);
            fclose(fi->fp);
            exit(EXIT_CODE_IO);
        }

        ret += (size * n);
    }

    TRACE_LEAVE(__func__)

    return ret;
}

/**
 * struct FileInfo wrapper to fclose.
 * @param fi: FileInfo.
 * @returns: fclose result.
*/
int FileInfo_fclose(struct FileInfo *fi)
{
    TRACE_ENTER(__func__)

    if (fi == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> error, fi is NULL\n", __func__, __LINE__);
    }

    int ret = 0;

    if (fi->_fp_state == 1)
    {
        ret = fclose(fi->fp);
        fi->_fp_state = 0;
    }

    TRACE_LEAVE(__func__)

    return ret;
}

/**
 * Frees all memory associated with the struct, including itself.
 * If internal state indicates the file handle is still open
 * the {@code flcose} is called on the file handle.
 * @param fi: FileInfo.
*/
void FileInfo_free(struct FileInfo *fi)
{
    TRACE_ENTER(__func__)

    if (fi == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (fi->filename != NULL)
    {
        // Filename was malloc'd and copied when FileInfo first created.
        free(fi->filename);
    }

    if (fi->_fp_state == 1)
    {
        fclose(fi->fp);
        fi->_fp_state = 0;
    }

    free(fi);

    TRACE_LEAVE(__func__)
}

/**
 * Copies 16 bit elements from source to destination, performing
 * byte swap on each element.
 * It is safe for dest to be the same as source (probably don't overlap otherwise though).
 * @param dest: destination array.
 * @param src: source array.
 * @param num: number of elements to copy and swap. (not number of bytes!)
*/
void bswap16_chunk(void *dest, const void *src, size_t num)
{
    TRACE_ENTER(__func__)

    size_t i;

    if (num < 1)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (dest == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>error, dest is NULL\n", __func__, __LINE__);
    }

    if (src == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>error, src is NULL\n", __func__, __LINE__);
    }

    if (dest == src)
    {
        for (i=0; i<num; i++)
        {
            ((uint16_t*)dest)[i] = BSWAP16_INLINE(((uint16_t*)dest)[i]);
        }
    }
    else
    {
        for (i=0; i<num; i++)
        {
            ((uint16_t*)dest)[i] = BSWAP16_INLINE(((uint16_t*)src)[i]);
        }
    }


    TRACE_LEAVE(__func__)
}

/**
 * Copies 32 bit elements from source to destination, performing
 * byte swap on each element.
 * @param dest: destination array.
 * @param src: source array.
 * @param num: number of elements to copy and swap. (not number of bytes!)
*/
void bswap32_chunk(void *dest, const void *src, size_t num)
{
    TRACE_ENTER(__func__)

    size_t i;
    
    if (num < 1)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (dest == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>error, dest is NULL\n", __func__, __LINE__);
    }

    if (src == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>error, src is NULL\n", __func__, __LINE__);
    }

    for (i=0; i<num; i++)
    {
        ((uint32_t*)dest)[i] = BSWAP32_INLINE(((uint32_t*)src)[i]);
    }

    TRACE_LEAVE(__func__)
}

/**
 * Parses contents of a file that has been loaded into memory.
 * Each line of the file is read as a single entry; the last
 * entry in the file does not need to end with newline.
 * Leading and trailing whitespace are ignored.
 * Lines that begin with a '#' (after zero or more whitespace) are ignored.
 * Non alphanumeric characters are ignored except: -_,.()[]
 * Space characters are accepted within.
 * If the processed line is non-empty, a new {@code struct string_data} (list node)
 * is allocated and appended to the list of names.
 * @param names_file_contents: buffer containing file content.
 * @param file_length: size in bytes of file content array.
 * @param names: linked list to append names to. Must be previously allocated.
*/
void parse_names(uint8_t *names_file_contents, size_t file_length, struct LinkedList *names)
{
    TRACE_ENTER(__func__)

    if (names_file_contents == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> names_file_contents is NULL.\n", __func__, __LINE__);
    }

    if (names == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> names is NULL.\n", __func__, __LINE__);
    }

    size_t i;
    int current_len = 0;
    int trailing_space = 0;

    /**
     * states:
     * 1 - reading line, waiting for comment indicator or text
     * 2 - appending to current buffer
     * 3 - ignoring input until newline
    */
    int state = 1;

    char name_buffer[MAX_FILENAME_LEN];

    memset(name_buffer, 0, MAX_FILENAME_LEN);

    for (i=0; i<file_length; i++)
    {
        char c = (char)names_file_contents[i];

        if (state == 1)
        {
            if (
                (c >= '0' && c <= '9')
                || (c >= 'a' && c <= 'z')
                || (c >= 'A' && c <= 'Z')
                // || c == ' ' // can't start with whitespace
                || c == '-'
                || c == '_'
                || c == ','
                || c == '.'
                || c == '('
                || c == ')'
                || c == '['
                || c == ']'
                )
            {
                name_buffer[current_len] = c;
                current_len++;
                state = 2;
            }
            else if (c == '#')
            {
                state = 3;
            }
        }
        else if (state == 2)
        {
            if (
                (c >= '0' && c <= '9')
                || (c >= 'a' && c <= 'z')
                || (c >= 'A' && c <= 'Z')
                || c == '-'
                || c == '_'
                || c == ','
                || c == '.'
                || c == '('
                || c == ')'
                || c == '['
                || c == ']'
                )
            {
                name_buffer[current_len] = c;
                current_len++;
                trailing_space = 0;
            }
            else if (c == ' ')
            {
                name_buffer[current_len] = c;
                current_len++;
                trailing_space++;
            }
        }

        if (c == '\n' || c == '\r')
        {
            if (trailing_space > 0)
            {
                current_len -= trailing_space;
                name_buffer[current_len + 1] = '\0';
            }

            if (current_len > 0)
            {
                struct LinkedListNode *node = LinkedListNode_string_data_new();
                set_string_data((struct string_data *)node->data, name_buffer, current_len);
                LinkedList_append_node(names, node);
                memset(name_buffer, 0, MAX_FILENAME_LEN);
            }

            current_len = 0;
            trailing_space = 0;
            state = 1;
        }
    }

    // last entry might not end with newline
    if (trailing_space > 0)
    {
        current_len -= trailing_space;
        name_buffer[current_len + 1] = '\0';
    }

    if (current_len > 0)
    {
        struct LinkedListNode *node = LinkedListNode_string_data_new();
        set_string_data((struct string_data *)node->data, name_buffer, current_len);
        LinkedList_append_node(names, node);
        memset(name_buffer, 0, MAX_FILENAME_LEN);
    }

    TRACE_LEAVE(__func__)
}

/**
 * Copies path form source, taking only filename.
 * @param string: filename, may contain one or more directories as prefix.
 * @param filename: out parameter. Must already be allocated. Will contain just filename.
 * @param max_len: max length of output filename. 
*/
void get_filename(char *string, char *filename, size_t max_len)
{
    TRACE_ENTER(__func__)

    int i = 0;
    int last_pos = -1;
    char c;
    size_t len;
    int new_len;

    if (string == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    // find last occurrence of PATH_SEPERATOR in the input filename.
    while (1)
    {
        c = string[i];

        if (c == PATH_SEPERATOR)
        {
            last_pos = i;
        }

        i++;
        if (c == '\0' || i >= MAX_FILENAME_LEN)
        {
            break;
        }
    }

    last_pos++;
    len = strlen(string);

    // make sure last_pos is within allowed limit for input.
    if ((size_t)last_pos > len)
    {
        last_pos = len;
    }

    new_len = (int)len - (int)last_pos;

    // now restrict new length to specified parameter
    if ((size_t)new_len > max_len)
    {
        new_len = max_len;
    }

    strncpy(filename, &string[last_pos], new_len);

    TRACE_LEAVE(__func__)
}

/**
 * Copies filename, changing the extension to the one specified.
 * @param input_filename: source filename.
 * @param output_filename: out parameter. Will contain new filename. Must be previously allocated.
 * @param new_extension: extension with leading '.' to replace in source filename.
 * @param max_len: max length of output filename.
*/
void change_filename_extension(char *input_filename, char *output_filename, char *new_extension, size_t max_len)
{
    TRACE_ENTER(__func__)

    int i = 0;
    int last_pos = -1;
    char c;
    size_t len;
    size_t ext_len;

    // find last occurrence of '.' in the input filename.
    while (1)
    {
        c = input_filename[i];

        if (c == '.')
        {
            last_pos = i;
        }

        i++;
        if (c == '\0' || i >= MAX_FILENAME_LEN)
        {
            break;
        }
    }

    if (last_pos > -1)
    {
        len = last_pos;
    }
    else
    {
        // if there's no '.' then just use the entire input filename
        len = strlen(input_filename);
    }
    
    ext_len = strlen(new_extension);

    // truncate filename so there's room for the extension
    if (len >= 1 + (MAX_FILENAME_LEN - ext_len))
    {
        len = MAX_FILENAME_LEN - ext_len;
    }

    if (len + ext_len > max_len)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> can't change extension, output_filename buffer too short.\n", __func__, __LINE__);
    }

    strncpy(output_filename, input_filename, len);
    strncpy(&output_filename[len], new_extension, ext_len);
    output_filename[len+ext_len] = '\0';

    TRACE_LEAVE(__func__)
}

/**
 * Checks whether a string ends in a suffix.
 * @param str: base string.
 * @param suffix: suffix to find at end of string.
 * @returns: 1 if `str` ends with `suffix`, zero otherwise.
*/
int string_ends_with(const char * str, const char * suffix)
{
    TRACE_ENTER(__func__)

    if (str == NULL || suffix == NULL)
    {
        return 0;
    }
    
    int str_len = strlen(str);
    int suffix_len = strlen(suffix);

    TRACE_LEAVE(__func__)
    return (str_len >= suffix_len) && (0 == strcmp(str + (str_len - suffix_len), suffix));
}

/**
 * Converts a 32 bit integer to a variable length quantity integer.
 * @param in: int value to convert.
 * @param varint: out parameter. Must be previously allocated. Will set value and num_bytes.
*/
void int32_to_VarLengthInt(int32_t in, struct VarLengthInt *varint)
{
    TRACE_ENTER(__func__)

    if (varint == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> varint is NULL.\n", __func__, __LINE__);
    }

    int num_bytes = 0;

    memset(varint->value_bytes, 0, VAR_INT_MAX_BYTES+1);

    varint->standard_value = in;

    do {
        int var7bits = in & 0x7f;
        in >>= 7;
        // out_result |= var7bits;
        varint->value_bytes[num_bytes] |= var7bits;

        if (in > 0)
        {
            varint->value_bytes[num_bytes + 1] = 0x80;
        }

        num_bytes++;

        if (num_bytes > VAR_INT_MAX_BYTES)
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> varint exceed available number bytes\n", __func__, __LINE__);
        }
    } while (in > 0);
    
    varint->num_bytes = num_bytes;

    TRACE_LEAVE(__func__)
}

/**
 * Reads an input buffer as variable length integer and parses
 * into regular integer.
 * @param buffer: byte buffer to read.
 * @param max_bytes: max number of bytes to read from buffer.
 * @param varint: out parameter. Will set varint values according to values read from buffer.
*/
void VarLengthInt_value_to_int32(uint8_t *buffer, int max_bytes, struct VarLengthInt *varint)
{
    TRACE_ENTER(__func__)

    int32_t ret = 0;
    int done = 0;
    int bytes_read = 0;
    int i;

    if (buffer == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> buffer is NULL.\n", __func__, __LINE__);
    }

    if (varint == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> varint is NULL.\n", __func__, __LINE__);
    }

    if (max_bytes < 1 || max_bytes > VAR_INT_MAX_BYTES)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d>parameter error, max_bytes=%d out of range. Max supported=%d\n", __func__, __LINE__, max_bytes, VAR_INT_MAX_BYTES);
    }

    while (!done && bytes_read <= max_bytes)
    {
        int var7bits = buffer[bytes_read] & 0x7f;

        if ((buffer[bytes_read] & 0x80) == 0)
        {
            done = 1;
        }

        ret <<= 7;
        ret |= var7bits;

        bytes_read++;
    }

    if (!done)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d>parse error.\n", __func__, __LINE__);
    }

    memset(varint->value_bytes, 0, VAR_INT_MAX_BYTES+1);
    for (i=0; i<bytes_read; i++)
    {
        varint->value_bytes[bytes_read - 1 - i] = buffer[i];
    }

    varint->num_bytes = bytes_read;
    varint->standard_value = ret;

    TRACE_LEAVE(__func__)
}

/**
 * Copies values from one varint to another.
 * @param dest: destination varint.
 * @param source: source varint.
*/
void VarLengthInt_copy(struct VarLengthInt *dest, struct VarLengthInt* source)
{
    TRACE_ENTER(__func__)

    if (dest == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> dest is NULL.\n", __func__, __LINE__);
    }

    if (source == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> source is NULL.\n", __func__, __LINE__);
    }

    if (source->num_bytes == 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> varint has zero bytes set.\n", __func__, __LINE__);
    }

    int i;

    dest->standard_value = source->standard_value;
    dest->num_bytes = source->num_bytes;

    for (i=0; i<VAR_INT_MAX_BYTES+1; i++)
    {
        dest->value_bytes[i] = source->value_bytes[i];
    }

    TRACE_LEAVE(__func__)
}

/**
 * Writes internal value of varint to output buffer in litte endian format
 * (least significant byte first).
 * @param dest: buffer to write to.
 * @param source: varint source.
*/
void VarLengthInt_write_value_little(uint8_t *dest, struct VarLengthInt* source)
{
    TRACE_ENTER(__func__)

    if (dest == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> dest is NULL.\n", __func__, __LINE__);
    }

    if (source == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> source is NULL.\n", __func__, __LINE__);
    }

    if (source->num_bytes == 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> varint has zero bytes set.\n", __func__, __LINE__);
    }

    int i;

    for (i=0; i<source->num_bytes; i++)
    {
        // value_bytes index zero is least significant byte
        dest[i] = source->value_bytes[i];
    }

    TRACE_LEAVE(__func__)
}

/**
 * Writes internal value of varint to output buffer in big endian format
 * (most significant byte first).
 * @param dest: buffer to write to.
 * @param source: varint source.
*/
void VarLengthInt_write_value_big(uint8_t *dest, struct VarLengthInt* source)
{
    TRACE_ENTER(__func__)

    if (dest == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> dest is NULL.\n", __func__, __LINE__);
    }

    if (source == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> source is NULL.\n", __func__, __LINE__);
    }

    if (source->num_bytes == 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> varint has zero bytes set.\n", __func__, __LINE__);
    }

    int i;

    for (i=0; i<source->num_bytes; i++)
    {
        // value_bytes index zero is least significant byte
        dest[i] = source->value_bytes[source->num_bytes - 1 - i];
    }

    TRACE_LEAVE(__func__)
}

/**
 * retrieves internal value of varint in litte endian format
 * (least significant byte first).
 * @param source: varint source.
 * @returns: value as 32 bit signed integer.
*/
int32_t VarLengthInt_get_value_little(struct VarLengthInt* source)
{
    TRACE_ENTER(__func__)

    if (source == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> source is NULL.\n", __func__, __LINE__);
    }

    if (source->num_bytes > 4)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> varint larger than int32.\n", __func__, __LINE__);
    }

    if (source->num_bytes == 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> varint has zero bytes set.\n", __func__, __LINE__);
    }

    int i;
    int32_t result = 0;

    for (i=0; i<source->num_bytes; i++)
    {
        // value_bytes index zero is least significant byte
        result |= ((int)source->value_bytes[i]) << (8*i);
    }

    TRACE_LEAVE(__func__)
    return result;
}

/**
 * retrieves internal value of varint in big endian format
 * (most significant byte first).
 * @param source: varint source.
 * @returns: value as 32 bit signed integer.
*/
int32_t VarLengthInt_get_value_big(struct VarLengthInt* source)
{
    TRACE_ENTER(__func__)

    if (source == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> source is NULL.\n", __func__, __LINE__);
    }

    if (source->num_bytes > 4)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> varint larger than int32.\n", __func__, __LINE__);
    }

    if (source->num_bytes == 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> varint has zero bytes set.\n", __func__, __LINE__);
    }

    int i;
    int32_t result = 0;
    int shift = 8 * (source->num_bytes - 1);

    for (i=0; i<source->num_bytes; i++)
    {
        // value_bytes index zero is least significant byte
        result |= ((int)source->value_bytes[source->num_bytes - 1 - i]) << shift;
        shift -= 8;
    }

    TRACE_LEAVE(__func__)
    return result;
}

/**
 * Reads a little endian byte buffer of sound data and puts it
 * into a buffer of 16 bit samples. If the incoming data is not long
 * enough to provide the required number of samples then values
 * of zeros are used to pad the results.
 * @param samples: Output buffer, must be previously allocated and large
 * enough to hold {@code samples_required} number of 16-bit samples.
 * @param samples_required: The number of 16 bit samples to acquire.
 * @param sound_data: Byte buffer of litte endian sound data.
 * @param sound_data_pos: In/out parmater. Current position in sound buffer
 * to begin reading sound data. Will contain the last position read.
 * @param sound_data_len: Length in bytes of incoming sound buffer.
 * @returns: number of bytes read from buffer.
*/
size_t fill_16bit_buffer(
    int16_t *samples,
    size_t samples_required,
    uint8_t *sound_data,
    size_t *sound_data_pos,
    size_t sound_data_len)
{
    TRACE_ENTER(__func__)

    if (samples == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> samples is NULL\n", __func__, __LINE__);
    }

    if (sound_data == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> sound_data is NULL\n", __func__, __LINE__);
    }

    if (sound_data_pos == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> sound_data_pos is NULL\n", __func__, __LINE__);
    }

    int i, sample_index;
    size_t bytes_read = 0;
    size_t max_i = 2 * samples_required; // 16 bit samples

    /**
     * Read bytes and convert to 16-bit samples, as long as data is
     * available in incoming buffer.
    */
    i=0;
    sample_index = 0;
    while (*sound_data_pos < sound_data_len && (size_t)i<max_i && (size_t)sample_index<samples_required)
    {
        if ((i & 0x1) == 0)
        {
            samples[sample_index] = sound_data[*sound_data_pos];
        }
        else
        {
            samples[sample_index] |= sound_data[*sound_data_pos] << 8;
            sample_index++;
        }
        
        *sound_data_pos = *sound_data_pos + 1;
        i++;
        bytes_read++;
    }

    // if the above ended on half a sample, advance to next index
    if ((i & 0x1) == 1)
    {
        sample_index++;
    }

    // pad any remaining samples with zero.
    for (i=sample_index; (size_t)i<samples_required; i++)
    {
        samples[i] = 0;
    }

    TRACE_LEAVE(__func__)
    return bytes_read;
}

/**
 * Converts signed 16 bit int array to array of double.
 * @param source: source array.
 * @param len: number of elements in array.
 * @param dest: destination array.
*/
void convert_s16_f64(int16_t *source, size_t len, double *dest)
{
    TRACE_ENTER(__func__)

    if (source == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> source is NULL\n", __func__, __LINE__);
    }

    if (dest == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> dest is NULL\n", __func__, __LINE__);
    }

    size_t i;

    for (i=0; i<len; i++)
    {
        dest[i] = (double)source[i];
    }

    TRACE_LEAVE(__func__)
}

/**
 * Convert string buffer of unspecified length to integer.
 * Passthrough to {@code strtol} with generic error message.
 * @param buffer: string to parse.
 * @returns: parsed value cast to int.
*/
long parse_int(char *buffer)
{
    TRACE_ENTER(__func__)

    if (buffer == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> buffer is NULL\n", __func__, __LINE__);
    }

    int res;
    char *pend = NULL;

    res = (int)strtol(buffer, &pend, 0);
    
    if (pend != NULL && *pend == '\0')
    {
        if (errno == ERANGE)
        {
            stderr_exit(EXIT_CODE_GENERAL, "error (range), cannot parse integer: %s\n", buffer);
        }
    }
    else
    {
        stderr_exit(EXIT_CODE_GENERAL, "error, cannot parse integer: %s\n", buffer);
    }

    TRACE_LEAVE(__func__)
    return res;
}