#ifndef _GAUDIO_DEBUG_H_
#define _GAUDIO_DEBUG_H_

#include <stdio.h>

/**
 * g_verbosity greater than or equal to this number
 * will be considered debug output.
*/
#define VERBOSE_DEBUG 3

/**
 * naudio debug flag.
 * If enabled, will print file offsets to the console when loading .tbl file.
*/
#define DEBUG_OFFSET_CONSOLE 0

/**
 * Trace debug flag.
 * If enabled, will print when entering and leaving functions.
*/
#define DEBUG_TRACE 0

/**
 * control flow flag, supposed to be used to mark "not implemented"
 * code paths (application should print error and exit(1) ).
*/
#define THROW_NOT_IMPLEMENTED 1

#if DEBUG_TRACE == 1
extern int _trace_depth;

/**
 * prints a message indicating control flow entered a function.
*/
#define TRACE_ENTER(s)  if (DEBUG_TRACE) \
    { \
        _trace_depth++; \
        printf("[%d] enter " s "\n", _trace_depth); \
        fflush(stdout); \
    }
/**
 * prints a message indicating control flow exited a function.
*/
#define TRACE_LEAVE(s)  if (DEBUG_TRACE) \
    { \
        printf("[%d] <- " s "\n", _trace_depth); \
        _trace_depth--; \
        fflush(stdout); \
    }
#else
#define TRACE_ENTER(s)
#define TRACE_LEAVE(s)
#endif

#endif