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
    TRACE_ENTER(__func__)

    void *outp;
    size_t malloc_size;

    malloc_size = count * item_size;

    if (malloc_size == 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s: malloc_size is zero.\n", __func__);
    }

    if (malloc_size > MAX_MALLOC_SIZE)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s: malloc_size=%ld exceeds sanity check max malloc size %d.\n", __func__, malloc_size, MAX_MALLOC_SIZE);
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
 * Creates directory and any sub directories.
 * Application exits if this fails.
 * @param path: path to final directory, including any sub directories.
 * @returns zero.
*/
int mkpath(const char* path)
{
    TRACE_ENTER(__func__)

    size_t len = strlen(path);
    char local_path[MAX_FILENAME_LEN];
    char *p;

    if (len > MAX_FILENAME_LEN)
    {
        stderr_exit(EXIT_CODE_IO, "error, mkpath name too long.\n");
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
                    stderr_exit(EXIT_CODE_IO, "mkpath error, could not create dir at step %s.\n", local_path);
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
            stderr_exit(EXIT_CODE_IO, "mkpath error, could not create dir %s.\n", path);
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

    FILE *input;

    size_t f_result;

    // length in bytes of input file
    size_t input_filesize;

    input = fopen(path, "rb");
    if (input == NULL)
    {
        stderr_exit(EXIT_CODE_IO, "Cannot open file: %s\n", path);
    }

    if (fseek(input, 0, SEEK_END) != 0)
    {
        fflush_printf(stderr, "error attempting to seek to end of file %s\n", path);
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
        fflush_printf(stderr, "error attempting to seek to beginning of file %s\n", path);
        fclose(input);
        exit(EXIT_CODE_IO);
    }

    if (input_filesize > MAX_INPUT_FILESIZE)
    {
        fflush_printf(stderr, "error, filesize=%ld is larger than max supported=%d\n", input_filesize, MAX_INPUT_FILESIZE);
        fclose(input);
        exit(EXIT_CODE_IO);
    }

    *buffer = (uint8_t *)malloc_zero(1, input_filesize);

    f_result = fread((void *)*buffer, 1, input_filesize, input);
    if(f_result != input_filesize || ferror(input))
    {
        fflush_printf(stderr, "error reading file [%s], expected to read %ld bytes, but read %ld\n", path, input_filesize, f_result);
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
size_t file_info_get_file_contents(struct file_info *fi, uint8_t **buffer)
{
    TRACE_ENTER(__func__)

    size_t f_result;

    file_info_fseek(fi, 0, SEEK_SET);

    *buffer = (uint8_t *)malloc_zero(1, fi->len);

    f_result = file_info_fread(fi, *buffer, fi->len, 1);

    TRACE_LEAVE(__func__)

    return f_result;
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
    TRACE_ENTER(__func__)

    struct stat st;

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
        stderr_exit(EXIT_CODE_IO, "Cannot open file: %s\n", filename);
    }

    stat(filename, &st);
    fi->len = st.st_size;

    fi->_fp_state = 1;

    if(fseek(fi->fp, 0, SEEK_END) != 0)
    {
        fflush_printf(stderr, "error attempting to seek to end of file %s\n", filename);
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
        fflush_printf(stderr, "error attempting to seek to beginning of file %s\n", fi->filename);
        fclose(fi->fp);
        exit(EXIT_CODE_IO);
    }

    if (fi->len > MAX_INPUT_FILESIZE)
    {
        fflush_printf(stderr, "error, filesize=%ld is larger than max supported=%d\n", fi->len, MAX_INPUT_FILESIZE);
        fclose(fi->fp);
        exit(EXIT_CODE_GENERAL);
    }

    TRACE_LEAVE(__func__)

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
    TRACE_ENTER(__func__)

    size_t num_bytes = size*n;

    if (fi == NULL)
    {
        stderr_exit(EXIT_CODE_IO, "file_info_fread error, fi is NULL\n");
    }

    if (fi->_fp_state != 1)
    {
        stderr_exit(EXIT_CODE_IO, "file_info_fread error, fi->fp not valid\n");
    }

    size_t f_result = fread((void *)output_buffer, size, n, fi->fp);
    
    if(f_result != n || ferror(fi->fp))
    {
        fflush_printf(stderr, "error reading file [%s], expected to read %ld elements, but read %ld\n", fi->filename, n, f_result);
		fclose(fi->fp);
        exit(EXIT_CODE_IO);
    }

    TRACE_LEAVE(__func__)

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
    TRACE_ENTER(__func__)

    int ret = fseek(fi->fp, __off, __whence);

    if (ret != 0)
    {
        fflush_printf(stderr, "error attempting to seek to beginning of file %s\n", fi->filename);
        fclose(fi->fp);
        exit(EXIT_CODE_IO);
    }

    TRACE_LEAVE(__func__)

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
    TRACE_ENTER(__func__)

    size_t ret;
    size_t num_bytes;

    num_bytes = size * n;

    if (fi == NULL)
    {
        stderr_exit(EXIT_CODE_IO, "file_info_fwrite error, fi is NULL\n");
    }

    if (fi->_fp_state != 1)
    {
        stderr_exit(EXIT_CODE_IO, "file_info_fwrite error, fi->fp not valid\n");
    }

    if (size == 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "file_info_fwrite: element size is zero.\n");
    }

    ret = fwrite(data, size, n, fi->fp);
    
    if (ret != n || ferror(fi->fp))
    {
        fflush_printf(stderr, "error writing to file, expected to write %ld elements, but wrote %ld\n", n, ret);
		fclose(fi->fp);
        exit(EXIT_CODE_IO);
    }

    TRACE_LEAVE(__func__)

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
    TRACE_ENTER(__func__)
    
    size_t i;
    size_t ret = 0;
    size_t f_result = 0;

    if (fi == NULL)
    {
        stderr_exit(EXIT_CODE_IO, "file_info_fwrite_bswap error, fi is NULL\n");
    }

    if (fi->_fp_state != 1)
    {
        stderr_exit(EXIT_CODE_IO, "file_info_fwrite_bswap error, fi->fp not valid\n");
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
                fflush_printf(stderr, "error writing to file, expected to write 1 element, but wrote %ld\n", 1, f_result);
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
            fflush_printf(stderr, "error writing to file, expected to write %ld elements, but wrote %ld\n", n, f_result);
            fclose(fi->fp);
            exit(EXIT_CODE_IO);
        }

        ret += (size * n);
    }

    TRACE_LEAVE(__func__)

    return ret;
}

/**
 * struct file_info wrapper to fclose.
 * @param fi: file_info.
 * @returns: fclose result.
*/
int file_info_fclose(struct file_info *fi)
{
    TRACE_ENTER(__func__)

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
 * @param fi: file_info.
*/
void file_info_free(struct file_info *fi)
{
    TRACE_ENTER(__func__)

    if (fi == NULL)
    {
        TRACE_LEAVE(__func__)
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

    TRACE_LEAVE(__func__)
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
    TRACE_ENTER(__func__)

    size_t i;

    if (num < 1)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (dest == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s error, dest is NULL\n", __func__);
    }

    if (src == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s error, src is NULL\n", __func__);
    }

    for (i=0; i<num; i++)
    {
        ((uint16_t*)dest)[i] = BSWAP16_INLINE(((uint16_t*)src)[i]);
    }

    TRACE_LEAVE(__func__)
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
    TRACE_ENTER(__func__)

    size_t i;
    
    if (num < 1)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (dest == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s error, dest is NULL\n", __func__);
    }

    if (src == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s error, src is NULL\n", __func__);
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
 * If the processed line is non-empty, a new {@code struct string_data struct llist_node}
 * is allocated and appended to the list of names.
 * @param names_file_contents: buffer containing file content.
 * @param file_length: size in bytes of file content array.
 * @param names: linked list to append names to.
*/
void parse_names(uint8_t *names_file_contents, size_t file_length, struct llist_root *names)
{
    TRACE_ENTER(__func__)

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
        stderr_exit(EXIT_CODE_GENERAL, "%s: can't change extension, output_filename buffer too short.\n", __func__);
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
*/
void int32_to_varint(int32_t in, struct var_length_int *varint)
{
    TRACE_ENTER(__func__)

    int32_t out_result = 0;
    int num_bytes = 0;

    varint->standard_value = in;

    do {
        int var7bits = in & 0x7f;
        in >>= 7;
        out_result |= var7bits;

        if (in > 0)
        {
            out_result <<= 8;
            out_result |= 0x80;
        }

        num_bytes++;
    } while (in > 0);
    
    varint->value = out_result;
    varint->num_bytes = num_bytes;

    TRACE_LEAVE(__func__)
}

/**
 * Reads an input buffer as variable length integer and parses
 * into regular integer.
 * @param buffer: byte buffer to read.
 * @param max_bytes: max number of bytes to read from buffer.
 * @param varint: out parameter. Will set standard_value and number bytes read.
*/
void varint_value_to_int32(uint8_t *buffer, int max_bytes, struct var_length_int *varint)
{
    TRACE_ENTER(__func__)

    int32_t ret = 0;
    int done = 0;
    int bytes_read = 0;

    if (buffer == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: buffer is NULL.\n", __func__);
    }

    if (varint == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: varint is NULL.\n", __func__);
    }

    varint->value = 0;

    if (max_bytes < 1 || max_bytes > 4)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s parameter error, max_bytes=%d out of range.\n", __func__, max_bytes);
    }

    while (!done && bytes_read <= max_bytes)
    {
        int var7bits = buffer[bytes_read] & 0x7f;

        if ((buffer[bytes_read] & 0x80) == 0)
        {
            done = 1;
        }

        varint->value <<= 8;
        varint->value |= buffer[bytes_read];

        ret <<= 7;
        ret |= var7bits;

        bytes_read++;
    }

    if (!done)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s parse error.\n", __func__);
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
void varint_copy(struct var_length_int *dest, struct var_length_int* source)
{
    TRACE_ENTER(__func__)

    if (dest == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: dest is NULL.\n", __func__);
    }

    if (source == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: source is NULL.\n", __func__);
    }

    dest->standard_value = source->standard_value;
    dest->value = source->value;
    dest->num_bytes = source->num_bytes;

    TRACE_LEAVE(__func__)
}