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

void int_hash_all(int *run_count, int *pass_count, int *fail_count)
{
    {
        printf("int_hash test: new / count empty\n");
        *run_count = *run_count + 1;
        int check = 0;
        struct IntHashTable *ht = IntHashTable_new();

        check = 1;
        check &= IntHashTable_count(ht) == 0;
        check &= IntHashTable_contains(ht, 1234) == 0;

        IntHashTable_free(ht);

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
        printf("int_hash test: add, contains\n");
        *run_count = *run_count + 1;
        int check = 0;
        int data_start = 0;
        struct IntHashTable *ht = IntHashTable_new();

        IntHashTable_add(ht, 1234, &data_start);

        check = 1;
        check &= IntHashTable_count(ht) == 1;

        if (!check)
        {
            printf("%s %d>fail: IntHashTable_add\n", __func__, __LINE__);
        }
        
        check &= IntHashTable_contains(ht, 1234) == 1;

        if (!check)
        {
            printf("%s %d>fail: IntHashTable_contains\n", __func__, __LINE__);
        }

        IntHashTable_free(ht);

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
        printf("int_hash test: add, pop\n");
        *run_count = *run_count + 1;
        int check = 0;
        int data_start = 0;
        void *data;
        struct IntHashTable *ht = IntHashTable_new();

        IntHashTable_add(ht, 1234, &data_start);

        check = 1;
        check &= IntHashTable_count(ht) == 1;

        if (!check)
        {
            printf("%s %d>fail: IntHashTable_add\n", __func__, __LINE__);
        }

        data = IntHashTable_get(ht, 1234);

        check &= data == &data_start;

        if (!check)
        {
            printf("%s %d>fail: data == &data_start\n", __func__, __LINE__);
        }

        check &= IntHashTable_count(ht) == 1;

        if (!check)
        {
            printf("%s %d>fail: IntHashTable_count\n", __func__, __LINE__);
        }

        data = IntHashTable_pop(ht, 1234);

        check &= data == &data_start;

        if (!check)
        {
            printf("%s %d>fail: data == &data_start\n", __func__, __LINE__);
        }
        
        check &= IntHashTable_contains(ht, 1234) == 0;

        if (!check)
        {
            printf("%s %d>fail: IntHashTable_contains after pop\n", __func__, __LINE__);
        }

        check &= IntHashTable_count(ht) == 0;

        if (!check)
        {
            printf("%s %d>fail: IntHashTable_count\n", __func__, __LINE__);
        }

        IntHashTable_free(ht);

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
        printf("int_hash test: add duplicates\n");
        *run_count = *run_count + 1;
        int check = 0;
        int data_set_1 = 111;
        int data_set_2 = 222;
        void *data;
        struct IntHashTable *ht = IntHashTable_new();

        IntHashTable_add(ht, 1234, &data_set_1);

        check = 1;
        check &= IntHashTable_count(ht) == 1;

        if (!check)
        {
            printf("%s %d>fail: IntHashTable_count 1\n", __func__, __LINE__);
        }

        IntHashTable_add(ht, 1234, &data_set_2);

        check &= IntHashTable_count(ht) == 2;

        if (!check)
        {
            printf("%s %d>fail: IntHashTable_add 2\n", __func__, __LINE__);
        }

        data = IntHashTable_get(ht, 1234);

        check &= data == &data_set_1;

        if (!check)
        {
            printf("%s %d>fail: data == &data_set_1\n", __func__, __LINE__);
        }

        check &= IntHashTable_count(ht) == 2;

        if (!check)
        {
            printf("%s %d>fail: IntHashTable_count 2\n", __func__, __LINE__);
        }

        data = IntHashTable_pop(ht, 1234);

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

        check &= IntHashTable_count(ht) == 1;

        if (!check)
        {
            printf("%s %d>fail: IntHashTable_count 2\n", __func__, __LINE__);
        }

        data = IntHashTable_get(ht, 1234);

        check &= data == &data_set_2;

        if (!check)
        {
            printf("%s %d>fail: data == &data_set_2\n", __func__, __LINE__);
        }

        check &= IntHashTable_count(ht) == 1;

        if (!check)
        {
            printf("%s %d>fail: IntHashTable_count 2\n", __func__, __LINE__);
        }

        data = IntHashTable_pop(ht, 1234);

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

        check &= IntHashTable_count(ht) == 0;

        if (!check)
        {
            printf("%s %d>fail: IntHashTable_count 3\n", __func__, __LINE__);
        }
        
        check &= IntHashTable_contains(ht, 1234) == 0;

        if (!check)
        {
            printf("%s %d>fail: IntHashTable_contains after pop\n", __func__, __LINE__);
        }

        IntHashTable_free(ht);

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
        struct IntHashTable *ht = IntHashTable_new();
        struct KeyValueInt *kvp;

        kvp = KeyValueInt_new();
        kvp->key = 1;
        kvp->value = 100;

        IntHashTable_add(ht, kvp->key, kvp);

        kvp = KeyValueInt_new();
        kvp->key = 2;
        kvp->value = 200;

        IntHashTable_add(ht, kvp->key, kvp);

        kvp = KeyValueInt_new();
        kvp->key = 3;
        kvp->value = 300;

        IntHashTable_add(ht, kvp->key, kvp);

        check = 1;
        check &= IntHashTable_count(ht) == 3;

        if (!check)
        {
            printf("%s %d>fail: IntHashTable_count\n", __func__, __LINE__);
        }

        g_test_hashtable_kvpint_compare = 444;

        check &= (0 == IntHashTable_any(ht, hashtable_kvpint_callback, NULL));
        if (!check)
        {
            printf("%s %d>fail: IntHashTable_any\n", __func__, __LINE__);
        }

        g_test_hashtable_kvpint_compare = 100;

        check &= (1 == IntHashTable_any(ht, hashtable_kvpint_callback, NULL));
        if (!check)
        {
            printf("%s %d>fail: IntHashTable_any -- 100\n", __func__, __LINE__);
        }

        g_test_hashtable_kvpint_compare = 200;

        check &= (1 == IntHashTable_any(ht, hashtable_kvpint_callback, NULL));
        if (!check)
        {
            printf("%s %d>fail: IntHashTable_any -- 200\n", __func__, __LINE__);
        }

        g_test_hashtable_kvpint_compare = 300;

        check &= (1 == IntHashTable_any(ht, hashtable_kvpint_callback, NULL));
        if (!check)
        {
            printf("%s %d>fail: IntHashTable_any -- 300\n", __func__, __LINE__);
        }

        IntHashTable_foreach(ht, hashtable_kvpint_callback_free);

        IntHashTable_free(ht);

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