#ifndef _GAUDIO_MACHINE_CONFIG_H_
#define _GAUDIO_MACHINE_CONFIG_H_

#ifdef __sgi
// files are big endian, so don't byte swap 
#  define BSWAP16(x)
#  define BSWAP16_INLINE(x) x
#  define BSWAP32(x)
#  define BSWAP32_INLINE(x) x
#  define BSWAP16_MANY(x, n)
#else
// else, this is a sane environment, so need to byteswap
#  define BSWAP16(x) x = __builtin_bswap16(x);
#  define BSWAP16_INLINE(x) __builtin_bswap16(x)
#  define BSWAP32(x) x = __builtin_bswap32(x);
#  define BSWAP32_INLINE(x) __builtin_bswap32(x)
#  define BSWAP16_MANY(x, n) { s32 _i; for (_i = 0; _i < n; _i++) BSWAP16((x)[_i]) }
#endif

// sanity config
#define MAX_FILES               1024 /* arbitrary */
#define MAX_FILENAME_LEN         255
#define MAX_INPUT_FILESIZE  20000000 /* arbitrary, but this should fit on a N64 cart, soooooo */

#define PATH_SEPERATOR '/'

// make system #define if needed...
typedef __float80 f80;

#endif