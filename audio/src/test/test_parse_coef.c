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

// forward declarations

int parse_coef_default(struct FileInfo *fi);

// end forward declarations

/**
 * Parses file as first described in `0001.inst`
*/
int parse_coef_default(struct FileInfo *fi)
{
    int pass = 1;

    struct ALADPCMBook *book = ALADPCMBook_new_from_coef(fi);
    int i;
    int count;

    if (book == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: book is NULL\n", __func__, __LINE__);
    }

    if (book->book == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: book->book is NULL\n", __func__, __LINE__);
    }

    pass &= (book->order == 2);
    pass &= (book->npredictors == 1);

    count = book->order * book->npredictors * 8;

    int16_t expected[] = {99,2,3,4,5,6,7,8,9,(int16_t)0xffff,11,12,13,-100,15,16};

    for (i=0; i<count; i++)
    {
        // values are stored big endian
        int16_t expected_val = BSWAP16_INLINE((uint16_t)expected[i]);

        pass &= (book->book[i] == expected_val);
    }

    ALADPCMBook_free(book);

    return pass;
}

void parse_coef_all(int *run_count, int *pass_count, int *fail_count)
{
    {
        /**
         * test all supported values
        */
        printf("parse coef test: 0001\n");
        int pass = 1;
        *run_count = *run_count + 1;

        struct FileInfo *fi = FileInfo_fopen("test_cases/coef_parse/0001.coef", "rb");
        
        pass = parse_coef_default(fi);

        FileInfo_free(fi);

        if (pass == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d> fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    {
        /**
         * parse test for whitespace and comments
        */
        printf("parse coef test: 0002 - whitespace and comments\n");
        int pass = 1;
        *run_count = *run_count + 1;

        struct FileInfo *fi = FileInfo_fopen("test_cases/coef_parse/0001.coef", "rb");
        
        pass = parse_coef_default(fi);

        FileInfo_free(fi);

        if (pass == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d> fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }
}