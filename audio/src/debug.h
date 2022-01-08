#ifndef _GAUDIO_DEBUG_H_
#define _GAUDIO_DEBUG_H_

#include <stdio.h>

#define VERBOSE_DEBUG 3
#define DEBUG_OFFSET_CONSOLE 0
#define DEBUG_TRACE 0
#define THROW_NOT_IMPLEMENTED 1

#if DEBUG_TRACE == 1
static int _trace_depth = 0;
#define TRACE_ENTER(s)  if (DEBUG_TRACE) \
    { \
        printf("[%d] enter " s "\n", _trace_depth); \
        _trace_depth++; \
        fflush(stdout); \
    }
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