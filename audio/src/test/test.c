#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "machine_config.h"
#include "debug.h"
#include "common.h"
#include "gaudio_math.h"
#include "utility.h"
#include "llist.h"
#include "string_hash.h"
#include "int_hash.h"
#include "md5.h"
#include "naudio.h"
#include "test_common.h"

/**
 * This is the top level entry point to run tests.
 * This calls all other tests and runs them.
*/

int main(int argc, char **argv)
{
    int pass_count = 0;
    int fail_count = 0;
    int total_run_count = 0;
    int sub_count = 0;

    if (argc == 0 || argv == NULL)
    {
        // be quiet gcc
    }

    //g_verbosity = VERBOSE_DEBUG;

    // sub_count = 0;
    // test_md5_all(&sub_count, &pass_count, &fail_count);
    // total_run_count += sub_count;

    // sub_count = 0;
    // linked_list_all(&sub_count, &pass_count, &fail_count);
    // total_run_count += sub_count;

    // sub_count = 0;
    // string_hash_all(&sub_count, &pass_count, &fail_count);
    // total_run_count += sub_count;

    // sub_count = 0;
    // int_hash_all(&sub_count, &pass_count, &fail_count);
    // total_run_count += sub_count;

    // sub_count = 0;
    // parse_inst_all(&sub_count, &pass_count, &fail_count);
    // total_run_count += sub_count;

    // sub_count = 0;
    // parse_coef_all(&sub_count, &pass_count, &fail_count);
    // total_run_count += sub_count;

    // sub_count = 0;
    // aifc_all(&sub_count, &pass_count, &fail_count);
    // total_run_count += sub_count;

    sub_count = 0;
    magic_all(&sub_count, &pass_count, &fail_count);
    total_run_count += sub_count;

    printf("%d tests run, %d pass, %d fail\n", total_run_count, pass_count, fail_count);

    return 0;
}