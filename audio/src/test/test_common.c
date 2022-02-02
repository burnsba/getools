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

int llist_node_TestKeyValue_compare_smaller_key(struct llist_node *first, struct llist_node *second)
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