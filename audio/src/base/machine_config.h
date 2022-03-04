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
#define MAX_MALLOC_SIZE     50000000

/**
 * Default path seperator.
*/
#define PATH_SEPERATOR '/'

#define EXIT_CODE_GENERAL                  1
#define EXIT_CODE_MALLOC                   2
#define EXIT_CODE_IO                       3
#define EXIT_CODE_NULL_REFERENCE_EXCEPTION 4

#if defined(__llvm__)

    /**
     * typedef for 80 bit "extended" float.
    */
    typedef long double f80;
    #define ATTR_NO_RETURN __attribute__((analyzer_noreturn))

#else

    /**
     * typedef for 80 bit "extended" float.
    */
    typedef __float80 f80;
    #define ATTR_NO_RETURN

#endif

// gcc
#define ATTR_INLINE __attribute__((always_inline))

#endif