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
#include "kvp.h"

void string_hash_all(int *run_count, int *pass_count, int *fail_count)
{
    {
        printf("string_hash test: new / count empty\n");
        *run_count = *run_count + 1;
        int check = 0;
        struct StringHashTable *ht = StringHashTable_new();

        check = 1;
        check &= StringHashTable_count(ht) == 0;
        check &= StringHashTable_contains(ht, "asdf") == 0;

        StringHashTable_free(ht);

        if (check == 1)
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
        printf("string_hash test: add, contains\n");
        *run_count = *run_count + 1;
        int check = 0;
        int data_start = 0;
        struct StringHashTable *ht = StringHashTable_new();

        StringHashTable_add(ht, "test", &data_start);

        check = 1;
        check &= StringHashTable_count(ht) == 1;

        if (!check)
        {
            printf("%s %d>fail: StringHashTable_add\n", __func__, __LINE__);
        }
        
        check &= StringHashTable_contains(ht, "test") == 1;

        if (!check)
        {
            printf("%s %d>fail: StringHashTable_contains\n", __func__, __LINE__);
        }

        StringHashTable_free(ht);

        if (check == 1)
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
        printf("string_hash test: add, pop\n");
        *run_count = *run_count + 1;
        int check = 0;
        int data_start = 0;
        void *data;
        struct StringHashTable *ht = StringHashTable_new();

        StringHashTable_add(ht, "test", &data_start);

        check = 1;
        check &= StringHashTable_count(ht) == 1;

        if (!check)
        {
            printf("%s %d>fail: StringHashTable_add\n", __func__, __LINE__);
        }

        data = StringHashTable_get(ht, "test");

        check &= data == &data_start;

        if (!check)
        {
            printf("%s %d>fail: data == &data_start\n", __func__, __LINE__);
        }

        check &= StringHashTable_count(ht) == 1;

        if (!check)
        {
            printf("%s %d>fail: StringHashTable_get\n", __func__, __LINE__);
        }

        data = NULL;
        data = StringHashTable_pop(ht, "test");

        check &= data == &data_start;

        if (!check)
        {
            printf("%s %d>fail: data == &data_start\n", __func__, __LINE__);
        }
        
        check &= StringHashTable_contains(ht, "test") == 0;

        if (!check)
        {
            printf("%s %d>fail: StringHashTable_contains after pop\n", __func__, __LINE__);
        }

        check &= StringHashTable_count(ht) == 0;

        if (!check)
        {
            printf("%s %d>fail: StringHashTable_count\n", __func__, __LINE__);
        }

        StringHashTable_free(ht);

        if (check == 1)
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
        printf("string_hash test: add duplicates\n");
        *run_count = *run_count + 1;
        int check = 0;
        int data_set_1 = 111;
        int data_set_2 = 222;
        void *data;
        struct StringHashTable *ht = StringHashTable_new();

        StringHashTable_add(ht, "test", &data_set_1);

        check = 1;
        check &= StringHashTable_count(ht) == 1;

        if (!check)
        {
            printf("%s %d>fail: StringHashTable_count 1\n", __func__, __LINE__);
        }

        StringHashTable_add(ht, "test", &data_set_2);

        check &= StringHashTable_count(ht) == 2;

        if (!check)
        {
            printf("%s %d>fail: StringHashTable_add 2\n", __func__, __LINE__);
        }

        data = StringHashTable_get(ht, "test");

        check &= data == &data_set_1;

        if (!check)
        {
            printf("%s %d>fail: data == &data_set_1\n", __func__, __LINE__);
        }

        check &= StringHashTable_count(ht) == 2;

        if (!check)
        {
            printf("%s %d>fail: StringHashTable_add 2\n", __func__, __LINE__);
        }

        data = StringHashTable_pop(ht, "test");

        check &= data == &data_set_1;

        if (!check)
        {
            printf("%s %d>fail: data == &data_set_1\n", __func__, __LINE__);
        }

        check &= (*(int*)data == 111);

        if (!check)
        {
            printf("%s %d>fail: data == 111\n", __func__, __LINE__);
        }

        check &= StringHashTable_count(ht) == 1;

        if (!check)
        {
            printf("%s %d>fail: StringHashTable_count 2\n", __func__, __LINE__);
        }

        data = StringHashTable_pop(ht, "test");

        check &= data == &data_set_2;

        if (!check)
        {
            printf("%s %d>fail: data == &data_set_2\n", __func__, __LINE__);
        }

        check &= (*(int*)data == 222);

        if (!check)
        {
            printf("%s %d>fail: data == 222\n", __func__, __LINE__);
        }

        check &= StringHashTable_count(ht) == 0;

        if (!check)
        {
            printf("%s %d>fail: StringHashTable_count 3\n", __func__, __LINE__);
        }
        
        check &= StringHashTable_contains(ht, "test") == 0;

        if (!check)
        {
            printf("%s %d>fail: StringHashTable_contains after pop\n", __func__, __LINE__);
        }

        StringHashTable_free(ht);

        if (check == 1)
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
        printf("int_hash test: any,foreach\n");
        *run_count = *run_count + 1;
        int check = 0;
        struct StringHashTable *ht = StringHashTable_new();
        struct KeyValueInt *kvp;

        kvp = KeyValueInt_new();
        kvp->key = 1;
        kvp->value = 100;

        StringHashTable_add(ht, "1", kvp);

        kvp = KeyValueInt_new();
        kvp->key = 2;
        kvp->value = 200;

        StringHashTable_add(ht, "2", kvp);

        kvp = KeyValueInt_new();
        kvp->key = 3;
        kvp->value = 300;

        StringHashTable_add(ht, "3", kvp);

        check = 1;
        check &= StringHashTable_count(ht) == 3;

        if (!check)
        {
            printf("%s %d>fail: StringHashTable_count\n", __func__, __LINE__);
        }

        g_test_hashtable_kvpint_compare = 444;

        check &= (0 == StringHashTable_any(ht, hashtable_kvpint_callback, NULL));
        if (!check)
        {
            printf("%s %d>fail: StringHashTable_any\n", __func__, __LINE__);
        }

        g_test_hashtable_kvpint_compare = 100;

        check &= (1 == StringHashTable_any(ht, hashtable_kvpint_callback, NULL));
        if (!check)
        {
            printf("%s %d>fail: StringHashTable_any -- 100\n", __func__, __LINE__);
        }

        g_test_hashtable_kvpint_compare = 200;

        check &= (1 == StringHashTable_any(ht, hashtable_kvpint_callback, NULL));
        if (!check)
        {
            printf("%s %d>fail: StringHashTable_any -- 200\n", __func__, __LINE__);
        }

        g_test_hashtable_kvpint_compare = 300;

        check &= (1 == StringHashTable_any(ht, hashtable_kvpint_callback, NULL));
        if (!check)
        {
            printf("%s %d>fail: StringHashTable_any -- 300\n", __func__, __LINE__);
        }

        StringHashTable_foreach(ht, hashtable_kvpint_callback_free);

        StringHashTable_free(ht);

        if (check == 1)
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