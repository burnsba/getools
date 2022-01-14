#ifndef _GAUDIO_MACHINE_CONFIG_H_
#define _GAUDIO_MACHINE_CONFIG_H_

/**
 * This file contains machine specific or OS specific definitions.
 * And some sanity check definitions.
*/

#ifdef __sgi
// files are big endian, so don't byte swap 
#  define BSWAP16(x)
#  define BSWAP16_INLINE(x) x
#  define BSWAP32(x)
#  define BSWAP32_INLINE(x) x
#else
// else, this is a sane environment, so need to byteswap
#  define BSWAP16(x) x = __builtin_bswap16(x);
#  define BSWAP16_INLINE(x) __builtin_bswap16(x)
#  define BSWAP32(x) x = __builtin_bswap32(x);
#  define BSWAP32_INLINE(x) __builtin_bswap32(x)
#endif

/**
 * Sanity check, max number of files that will be handled.
 * Arbitrary.
*/
#define MAX_FILES               1024

/**
 * Sanity check, max filename length.
 * Arbitrary.
*/
#define MAX_FILENAME_LEN         255

/**
 * Sanity check, max input filesize in bytes.
 * Arbitrary.
*/
#define MAX_INPUT_FILESIZE  20000000

/**
 * Default path seperator.
*/
#define PATH_SEPERATOR '/'

#define EXIT_CODE_GENERAL                  1
#define EXIT_CODE_MALLOC                   2
#define EXIT_CODE_IO                       3
#define EXIT_CODE_NULL_REFERENCE_EXCEPTION 4

/**
 * typedef for 80 bit "extended" float.
*/
typedef __float80 f80;

#endif