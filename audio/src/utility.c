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
    va_list args;
    va_start(args, format);
    vfprintf(stream, format, args);
    va_end(args);

    fflush(stderr);
}

/**
 * Allocates memory for (count*item_size) number of bytes
 * and memset the result to zero.
 * If malloc fails the program will exit.
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

/**
 * Creates directory and any sub directories.
 * Application exits if this fails.
 * @param path: path to final directory, including any sub directories.
 * @returns zero.
*/
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

/**
 * Reads all contents of a file into a memory buffer.
 * @param path: path of file to read.
 * @param buffer: pointer to memory array to store file contents. This should
 * not point to any allocated memory; memory will be allocated in function.
 * @returns: number of bytes read from file (also, length of buffer).
*/
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

    if (fseek(input, 0, SEEK_END) != 0)
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

/**
 * struct file_info wrapper to fopen.
 * Allocates memory for filename, sets internal state, and file length.
 * @param filename: path/filename to open.
 * @param mode: file open mode.
 * @returns: pointer to new {@code struct file_info} that was allocated.
*/
struct file_info *file_info_fopen(char *filename, const char *mode)
{
    TRACE_ENTER("file_info_fopen")

    size_t filename_len = strlen(filename);

    struct file_info *fi = (struct file_info *)malloc_zero(1, sizeof(struct file_info));

    if (filename_len > 0)
    {
        // TODO: this should check for path seperator and only use
        // the actual filename.
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

/**
 * struct file_info wrapper to fread.
 * @param fi: file_info.
 * @param output_buffer: buffer to write read result into. Thus must already be allocated.
 * @param size: size of each element to read.
 * @param n: number of elements to read.
 * @returns: number of bytes read (not number of elements read like fread).
*/
size_t file_info_fread(struct file_info *fi, void *output_buffer, size_t size, size_t n)
{
    TRACE_ENTER("file_info_fread")

    size_t num_bytes = size*n;

    if (fi == NULL)
    {
        stderr_exit(1, "file_info_fread error, fi is NULL\n");
    }

    if (fi->_fp_state != 1)
    {
        stderr_exit(1, "file_info_fread error, fi->fp not valid\n");
    }

    size_t f_result = fread((void *)output_buffer, size, n, fi->fp);
    
    if(f_result != n || ferror(fi->fp))
    {
        fflush_printf(stderr, "error reading file [%s], expected to read %ld elements, but read %ld\n", fi->filename, n, f_result);
		fclose(fi->fp);
        exit(1);
    }

    TRACE_LEAVE("file_info_fread")

    return num_bytes;
}

/**
 * struct file_info wrapper to fseek.
 * @param fi: file_info.
 * @param __off: seek amount offset.
 * @param __whence: from where to seek.
 * @returns: fseek result.
*/
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

/**
 * struct file_info wrapper to fwrite.
 * @param fi: file_info.
 * @param data: data to write.
 * @param size: size of each element to write.
 * @param n: number of elements to write.
 * @returns: number of bytes written (not number of elements written like fwrite).
*/
size_t file_info_fwrite(struct file_info *fi, const void *data, size_t size, size_t n)
{
    TRACE_ENTER("file_info_fwrite")

    size_t ret;
    size_t num_bytes;

    num_bytes = size * n;

    if (fi == NULL)
    {
        stderr_exit(1, "file_info_fwrite error, fi is NULL\n");
    }

    if (fi->_fp_state != 1)
    {
        stderr_exit(1, "file_info_fwrite error, fi->fp not valid\n");
    }

    ret = fwrite(data, size, n, fi->fp);
    
    if (ret != n || ferror(fi->fp))
    {
        fflush_printf(stderr, "error writing to file, expected to write %ld elements, but wrote %ld\n", n, ret);
		fclose(fi->fp);
        exit(1);
    }

    TRACE_LEAVE("file_info_fwrite");

    return num_bytes;
}

/**
 * struct file_info wrapper to fwrite, with possible byte swapping before writing.
 * If the size of the element(s) being written is 2 or 4 then the value will be
 * swapped via 16 bit swap or 32 bit swap respectively. Otherwise the value
 * is written unchanged.
 * @param fi: file_info.
 * @param data: data to write.
 * @param size: size of each element to write.
 * @param n: number of elements to write.
 * @returns: number of bytes written (not number of elements written like fwrite).
*/
size_t file_info_fwrite_bswap(struct file_info *fi, const void *data, size_t size, size_t n)
{
    TRACE_ENTER("file_info_fwrite_bswap")
    
    size_t i;
    size_t ret = 0;
    size_t f_result = 0;

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

            if (f_result != 1 || ferror(fi->fp))
            {
                fflush_printf(stderr, "error writing to file, expected to write 1 element, but wrote %ld\n", f_result);
                fclose(fi->fp);
                exit(1);
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
                fflush_printf(stderr, "error writing to file, expected to write 1 element, but wrote %ld\n", 1, f_result);
                fclose(fi->fp);
                exit(1);
            }

            ret += size;
        }
    }
    else
    {
        f_result = fwrite(data, size, n, fi->fp);

        if (f_result != n || ferror(fi->fp))
        {
            fflush_printf(stderr, "error writing to file, expected to write %ld elements, but wrote %ld\n", n, f_result);
            fclose(fi->fp);
            exit(1);
        }

        ret += (size * n);
    }

    TRACE_LEAVE("file_info_fwrite_bswap");

    return ret;
}

/**
 * struct file_info wrapper to fclose.
 * @param fi: file_info.
 * @returns: fclose result.
*/
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

/**
 * Frees all memory associated with the struct, including itself.
 * If internal state indicates the file handle is still open
 * the {@code flcose} is called on the file handle.
 * @param fi: file_info.
*/
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

/**
 * Copies 16 bit elements from source to destination, performing
 * byte swap on each element.
 * @param dest: destination array.
 * @param src: source array.
 * @param num: number of elements to copy and swap.
*/
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

/**
 * Copies 32 bit elements from source to destination, performing
 * byte swap on each element.
 * @param dest: destination array.
 * @param src: source array.
 * @param num: number of elements to copy and swap.
*/
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

/**
 * Parses contents of a file that has been loaded into memory.
 * Each line of the file is read as a single entry; the last
 * entry in the file does not need to end with newline.
 * Leading and trailing whitespace are ignored.
 * Lines that begin with a '#' (after zero or more whitespace) are ignored.
 * Non alphanumeric characters are ignored except: -_,.()[]
 * Space characters are accepted within.
 * If the processed line is non-empty, a new {@code struct string_data struct llist_node}
 * is allocated and appended to the list of names.
 * @param names_file_contents: buffer containing file content.
 * @param file_length: size in bytes of file content array.
 * @param names: linked list to append names to.
*/
void parse_names(uint8_t *names_file_contents, size_t file_length, struct llist_root *names)
{
    TRACE_ENTER("parse_names");

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
                struct llist_node *node = llist_node_string_data_new();
                set_string_data((struct string_data *)node->data, name_buffer, current_len);
                llist_root_append_node(names, node);
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
        struct llist_node *node = llist_node_string_data_new();
        set_string_data((struct string_data *)node->data, name_buffer, current_len);
        llist_root_append_node(names, node);
        memset(name_buffer, 0, MAX_FILENAME_LEN);
    }

    current_len = 0;
    trailing_space = 0;
    state = 1;

    TRACE_LEAVE("parse_names");
}