#include "debug.h"

/**
 * This file contains debugging code and global variables.
*/

#if DEBUG_TRACE == 1
/**
 * Global debug variable. Used to print callstack depth when printing function trace info.
*/
int _trace_depth = 0;
#endif