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
#include <math.h>
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
#include "kvp.h"



int g_test_hashtable_kvpint_compare = 0;

int hashtable_kvpint_callback(void* data)
{
    struct KeyValueInt *kvp = (struct KeyValueInt *)data;

    return g_test_hashtable_kvpint_compare == kvp->value;
}

void hashtable_kvpint_callback_free(void* data)
{
    if (data != NULL)
    {
        struct KeyValueInt *kvp = (struct KeyValueInt *)data;
        KeyValueInt_free(kvp);
    }
}

struct TestKeyValue *TestKeyValue_new()
{
    return (struct TestKeyValue *)malloc_zero(1, sizeof(struct TestKeyValue));
}

void TestKeyValue_free(struct TestKeyValue *tkvp)
{
    if (tkvp != NULL)
    {
        free(tkvp);
    }
}

int LinkedListNode_TestKeyValue_compare_smaller_key(struct LinkedListNode *first, struct LinkedListNode *second)
{
    int ret;

    if (first == NULL && second == NULL)
    {
        ret = 0;
    }
    else if (first == NULL && second != NULL)
    {
        ret = 1;
    }
    else if (first != NULL && second == NULL)
    {
        ret = -1;
    }
    else
    {
        struct TestKeyValue *kvp_first = (struct TestKeyValue *)first->data;
        struct TestKeyValue *kvp_second = (struct TestKeyValue *)second->data;
       
        if (kvp_first == NULL && kvp_second == NULL)
        {
            ret = 0;
        }
        else if (kvp_first == NULL && kvp_second != NULL)
        {
            ret = 1;
        }
        else if (kvp_first != NULL && kvp_second == NULL)
        {
            ret = -1;
        }
        else
        {
            if (kvp_first->key < kvp_second->key)
            {
                ret = -1;
            }
            else if (kvp_first->key > kvp_second->key)
            {
                ret = 1;
            }
            else
            {
                ret = 0;
            }
        }
    }

    return ret;
}

int f64_equal(double d1, double d2, double epsilon)
{
    double diff = d1 - d2;
    return (fabs(diff) < epsilon);
}

void print_expected_vs_actual_arr(uint8_t *expected, size_t expected_len, uint8_t *actual, size_t actual_len)
{
    int color_flag = 0;
    size_t i;
    printf("expected\n");
    for (i=0; i<expected_len; i++)
    {
        if ((i % 16) == 0)
        {
            printf("0x%04lx: ", i);
        }

        color_flag = 0;
        if (g_term_colors && i < actual_len && expected[i] != actual[i])
        {
            printf("\033[32m");
            color_flag = 1;
        }
        printf("0x%02x ", expected[i]);
        if (g_term_colors && color_flag)
        {
            printf("\033[39m");
            color_flag = 0;
        }
        if (((i+1)%16)==0)
        {
            printf("\n");
        }
    }
    printf("\n");
    printf("actual\n");
    for (i=0; i < actual_len; i++)
    {
        if ((i % 16) == 0)
        {
            printf("0x%04lx: ", i);
        }
        
        color_flag = 0;
        if (g_term_colors && i < expected_len && expected[i] != actual[i])
        {
            printf("\033[31m");
            color_flag = 1;
        }
        printf("0x%02x ", actual[i]);
        if (g_term_colors && color_flag)
        {
            printf("\033[39m");
            color_flag = 0;
        }
        if (((i+1)%16)==0)
        {
            printf("\n");
        }
    }
    printf("\n");
}