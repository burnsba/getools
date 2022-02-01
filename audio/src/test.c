#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "machine_config.h"
#include "debug.h"
#include "common.h"
#include "math.h"
#include "utility.h"
#include "llist.h"
#include "string_hash.h"
#include "int_hash.h"
#include "md5.h"
#include "naudio.h"

/**
 * This file contains simple unit tests.
*/

struct TestKeyValue {
    int key;
    void *data;
};

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

void print16(char *arr)
{
    int i;
    for (i=0; i<16; i++)
    {
        printf("%02x", (0xff & arr[i]));
    }
    printf("\n");
}

int ascii_to_int(char c)
{
    if (c >= '0' && c <= '9')
    {
        c -= '0';
    }
    else if (c >= 'a' && c <= 'z')
    {
        c = 10 + c - 'a';
    }
    else if (c >= 'A' && c <= 'Z')
    {
        c = 10 + c - 'A';
    }

    return c;
}

/**
 * Compares byte array to array of ASCII characters.
*/
int md5_compare(char *expected, char *actual)
{
    int i;
    for (i=0; i<16; i++)
    {
        int ch;

        ch = 0;
        ch |= ascii_to_int(actual[i*2]) << 4;
        ch |= ascii_to_int(actual[i*2+1]);

        if ((uint8_t)expected[i] != (uint8_t)ch)
        {
            return 1;
        }
    }

    return 0;
}

void test_md5_all(int *run_count, int *pass_count, int *fail_count)
{
    char digest[16];

    {
        printf("md5 test: a\n");
        *run_count = *run_count + 1;
        memset(digest, 0, 16);
        md5_hash("a", 1, digest);
        if (md5_compare(digest, "0cc175b9c0f1b6a831c399e269772661") == 0)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d>fail\n", __func__, __LINE__);
            print16(digest);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("md5 test: empty string\n");
        *run_count = *run_count + 1;
        memset(digest, 0, 16);
        md5_hash("", 0, digest);
        if (md5_compare(digest, "d41d8cd98f00b204e9800998ecf8427e") == 0)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d>fail\n", __func__, __LINE__);
            print16(digest);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("md5 test: The quick brown fox jumps over the lazy dog\n");
        *run_count = *run_count + 1;
        memset(digest, 0, 16);
        md5_hash("The quick brown fox jumps over the lazy dog", 43, digest);
        if (md5_compare(digest, "9e107d9d372bb6826bd81d3542a419d6") == 0)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d>fail\n", __func__, __LINE__);
            print16(digest);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("md5 test: The quick brown fox jumps over the lazy dog\n");
        *run_count = *run_count + 1;
        memset(digest, 0, 16);
        md5_hash("The quick brown fox jumps over the lazy dog", 43, digest);
        if (md5_compare(digest, "9e107d9d372bb6826bd81d3542a419d6") == 0)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d>fail\n", __func__, __LINE__);
            print16(digest);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("md5 test: The quick brown fox jumps over the lazy dog456789012345\n");
        *run_count = *run_count + 1;
        memset(digest, 0, 16);
        md5_hash("The quick brown fox jumps over the lazy dog456789012345", 55, digest);
        if (md5_compare(digest, "e3b93d322b8e9cff911f08d78382a776") == 0)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d>fail\n", __func__, __LINE__);
            print16(digest);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("md5 test: The quick brown fox jumps over the lazy dog4567890123456\n");
        *run_count = *run_count + 1;
        memset(digest, 0, 16);
        md5_hash("The quick brown fox jumps over the lazy dog4567890123456", 56, digest);
        if (md5_compare(digest, "1406f3bf8051d3a9d7ad16506a02be58") == 0)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d>fail\n", __func__, __LINE__);
            print16(digest);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("md5 test: The quick brown fox jumps over the lazy dog45678901234567890123\n");
        *run_count = *run_count + 1;
        memset(digest, 0, 16);
        md5_hash("The quick brown fox jumps over the lazy dog45678901234567890123", 63, digest);
        if (md5_compare(digest, "f5c9aa7ad2289a9e421d0fb460430376") == 0)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d>fail\n", __func__, __LINE__);
            print16(digest);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("md5 test: The quick brown fox jumps over the lazy dog456789012345678901234\n");
        *run_count = *run_count + 1;
        memset(digest, 0, 16);
        md5_hash("The quick brown fox jumps over the lazy dog456789012345678901234", 64, digest);
        if (md5_compare(digest, "95d81dcbdaef4c8ea12b572f51f938a1") == 0)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d>fail\n", __func__, __LINE__);
            print16(digest);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("md5 test: The quick brown fox jumps over the lazy dog4567890123456789012345\n");
        *run_count = *run_count + 1;
        memset(digest, 0, 16);
        md5_hash("The quick brown fox jumps over the lazy dog4567890123456789012345", 65, digest);
        if (md5_compare(digest, "c8620ffef59aa6c3b54e06fb840afc0b") == 0)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d>fail\n", __func__, __LINE__);
            print16(digest);
            *fail_count = *fail_count + 1;
        }
    }
}

void linked_list_all(int *run_count, int *pass_count, int *fail_count)
{
    {
        printf("llist test: append\n");
        *run_count = *run_count + 1;
        int check = 0;
        struct llist_root *root = llist_root_new();
        struct llist_node *node = llist_node_new();
        llist_root_append_node(root, node);

        check = 1;
        check &= root->count == 1;
        check &= root->root == root->tail;
        check &= root->tail != NULL;
        check &= root->tail == node;

        llist_node_root_free(root);

        if (check == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d>fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("llist test: sort\n");
        *run_count = *run_count + 1;
        int check = 0;
        struct llist_node *node;
        struct TestKeyValue *tkvp;

        struct llist_root *root = llist_root_new();

        node = llist_node_new();
        tkvp = TestKeyValue_new();
        tkvp->key = 2;
        node->data = tkvp;
        llist_root_append_node(root, node);

        node = llist_node_new();
        tkvp = TestKeyValue_new();
        tkvp->key = 0;
        node->data = tkvp;
        llist_root_append_node(root, node);

        node = llist_node_new();
        tkvp = TestKeyValue_new();
        tkvp->key = 1;
        node->data = tkvp;
        llist_root_append_node(root, node);

        llist_root_merge_sort(root, llist_node_TestKeyValue_compare_smaller_key);

        check = 1;
        check &= root->count == 3;
        check &= (((struct TestKeyValue *)root->root->data)->key == 0);
        check &= (((struct TestKeyValue *)root->root->next->data)->key == 1);
        check &= (((struct TestKeyValue *)root->root->next->next->data)->key == 2);

        node = root->root;
        while (node != NULL)
        {
            TestKeyValue_free(node->data);
            node = node->next;
        }

        llist_node_root_free(root);

        if (check == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d>fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }
}

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
            printf("%s %d>fail\n", __func__, __LINE__);
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
            printf("%s %d>fail\n", __func__, __LINE__);
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
            printf("%s %d>fail\n", __func__, __LINE__);
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
            printf("%s %d>fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }
}

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
            printf("%s %d>fail\n", __func__, __LINE__);
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
            printf("%s %d>fail\n", __func__, __LINE__);
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
            printf("%s %d>fail\n", __func__, __LINE__);
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
            printf("%s %d>fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }
}

/**
 * Parses file as first described in `0002.inst`
*/
int parse_inst_default(struct file_info *fi)
{
    struct ALBankFile *bank_file = ALBankFile_new_from_inst(fi);

    struct ALBank *bank;
    struct ALInstrument *instrument;
    struct ALSound *sound;
    struct ALWaveTable *wavetable;
    struct ALKeyMap *keymap;
    struct ALEnvelope *envelope;
    int i;

    int pass = 1;
    if (bank_file == NULL)
    {
        pass = 0;
        printf("%s %d>fail: bank_file is NULL\n", __func__, __LINE__);
    }

    if (bank_file->bank_count != 1)
    {
        pass = 0;
        printf("%s %d>fail: bank_file->bank_count=%d, expected 1\n", __func__, __LINE__, bank_file->bank_count);
    }

    if (bank_file->banks == NULL)
    {
        pass = 0;
        printf("%s %d>fail: bank_file->banks is NULL\n", __func__, __LINE__);
    }

    bank = bank_file->banks[0];

    if (bank == NULL)
    {
        pass = 0;
        printf("%s %d>fail: bank is NULL\n", __func__, __LINE__);
    }

    if (bank->inst_count != 1)
    {
        pass = 0;
        printf("%s %d>fail: bank->inst_count=%d, expected 1\n", __func__, __LINE__, bank->inst_count);
    }

    if (bank->instruments == NULL)
    {
        pass = 0;
        printf("%s %d>fail: bank->instruments is NULL\n", __func__, __LINE__);
    }

    instrument = bank->instruments[0];

    if (instrument == NULL)
    {
        pass = 0;
        printf("%s %d>fail: instrument is NULL\n", __func__, __LINE__);
    }

    if (instrument->sound_count != 3)
    {
        pass = 0;
        printf("%s %d>fail: instrument->sound_count=%d, expected 3\n", __func__, __LINE__, instrument->sound_count);
    }

    if (instrument->volume != 70)
    {
        pass = 0;
        printf("%s %d>fail: instrument->volume=%d, expected 70\n", __func__, __LINE__, instrument->volume);
    }

    if (instrument->pan != 49)
    {
        pass = 0;
        printf("%s %d>fail: instrument->pan=%d, expected 49\n", __func__, __LINE__, instrument->pan);
    }

    if (instrument->sounds == NULL)
    {
        pass = 0;
        printf("%s %d>fail: instrument->sounds is NULL\n", __func__, __LINE__);
    }

    int seen_sound_index_0 = 0;
    int seen_sound_index_1 = 0;
    int seen_sound_index_2 = 0;

    for (i=0; i<instrument->sound_count; i++)
    {
        sound = instrument->sounds[i];

        if (sound == NULL)
        {
            pass = 0;
            printf("%s %d>fail: sound is NULL\n", __func__, __LINE__);
        }

        if (strcmp(sound->text_id, "sound1") == 0)
        {
            seen_sound_index_0++;

            if (sound->sample_volume != 127)
            {
                pass = 0;
                printf("%s %d>fail: sound->sample_volume=%d, expected 127\n", __func__, __LINE__, sound->sample_volume);
            }

            if (sound->sample_pan != 64)
            {
                pass = 0;
                printf("%s %d>fail: sound->sample_pan=%d, expected 64\n", __func__, __LINE__, sound->sample_pan);
            }
            
            wavetable = sound->wavetable;

            if (wavetable == NULL)
            {
                pass = 0;
                printf("%s %d>fail: wavetable is NULL\n", __func__, __LINE__);
            }

            if (strcmp(wavetable->aifc_path, "../sounds/thunk.aifc") != 0)
            {
                pass = 0;
                printf("%s %d>fail: wavetable->aifc_path=\"%s\", expected \"%s\"\n", __func__, __LINE__, wavetable->aifc_path, "../sounds/thunk.aifc");
            }

            envelope = sound->envelope;

            if (envelope == NULL)
            {
                pass = 0;
                printf("%s %d>fail: envelope is NULL\n", __func__, __LINE__);
            }

            if (envelope->attack_time != 5000)
            {
                pass = 0;
                printf("%s %d>fail: envelope->attack_time=%d, expected %d\n", __func__, __LINE__, envelope->attack_time, 5000);
            }

            if (envelope->attack_volume != 127)
            {
                pass = 0;
                printf("%s %d>fail: envelope->attack_volume=%d, expected %d\n", __func__, __LINE__, envelope->attack_volume, 127);
            }

            if (envelope->decay_time != 364920)
            {
                pass = 0;
                printf("%s %d>fail: envelope->decay_time=%d, expected %d\n", __func__, __LINE__, envelope->decay_time, 364920);
            }

            if (envelope->decay_volume != 127)
            {
                pass = 0;
                printf("%s %d>fail: envelope->decay_volume=%d, expected %d\n", __func__, __LINE__, envelope->decay_volume, 127);
            }

            if (envelope->release_time != 1234)
            {
                pass = 0;
                printf("%s %d>fail: envelope->release_time=%d, expected %d\n", __func__, __LINE__, envelope->release_time, 1234);
            }

            keymap = sound->keymap;

            if (keymap == NULL)
            {
                pass = 0;
                printf("%s %d>fail: keymap is NULL\n", __func__, __LINE__);
            }

            if (keymap->velocity_min != 1)
            {
                pass = 0;
                printf("%s %d>fail: keymap->velocity_min=%d, expected %d\n", __func__, __LINE__, keymap->velocity_min, 1);
            }

            if (keymap->velocity_max != 127)
            {
                pass = 0;
                printf("%s %d>fail: keymap->velocity_max=%d, expected %d\n", __func__, __LINE__, keymap->velocity_max, 127);
            }

            if (keymap->key_min != 41)
            {
                pass = 0;
                printf("%s %d>fail: keymap->key_min=%d, expected %d\n", __func__, __LINE__, keymap->key_min, 41);
            }

            if (keymap->key_max != 42)
            {
                pass = 0;
                printf("%s %d>fail: keymap->key_max=%d, expected %d\n", __func__, __LINE__, keymap->key_max, 42);
            }

            if (keymap->key_base != 43)
            {
                pass = 0;
                printf("%s %d>fail: keymap->key_base=%d, expected %d\n", __func__, __LINE__, keymap->key_base, 43);
            }

            if (keymap->detune != 5)
            {
                pass = 0;
                printf("%s %d>fail: keymap->detune=%d, expected %d\n", __func__, __LINE__, keymap->detune, 5);
            }
        }
        else if (strcmp(sound->text_id, "glass_sound") == 0)
        {
            seen_sound_index_1++;
            
            if (sound->sample_volume != 120)
            {
                pass = 0;
                printf("%s %d>fail: sound->sample_volume=%d, expected 120\n", __func__, __LINE__, sound->sample_volume);
            }

            if (sound->sample_pan != 60)
            {
                pass = 0;
                printf("%s %d>fail: sound->sample_pan=%d, expected 60\n", __func__, __LINE__, sound->sample_pan);
            }
            
            wavetable = sound->wavetable;

            if (wavetable == NULL)
            {
                pass = 0;
                printf("%s %d>fail: wavetable is NULL\n", __func__, __LINE__);
            }

            if (strcmp(wavetable->aifc_path, "../sounds/glass.aifc") != 0)
            {
                pass = 0;
                printf("%s %d>fail: wavetable->aifc_path=\"%s\", expected \"%s\"\n", __func__, __LINE__, wavetable->aifc_path, "../sounds/glass.aifc");
            }

            envelope = sound->envelope;

            if (envelope == NULL)
            {
                pass = 0;
                printf("%s %d>fail: envelope is NULL\n", __func__, __LINE__);
            }

            if (envelope->attack_time != 5000)
            {
                pass = 0;
                printf("%s %d>fail: envelope->attack_time=%d, expected %d\n", __func__, __LINE__, envelope->attack_time, 5000);
            }

            if (envelope->attack_volume != 127)
            {
                pass = 0;
                printf("%s %d>fail: envelope->attack_volume=%d, expected %d\n", __func__, __LINE__, envelope->attack_volume, 127);
            }

            if (envelope->decay_time != -1)
            {
                pass = 0;
                printf("%s %d>fail: envelope->decay_time=%d, expected %d\n", __func__, __LINE__, envelope->decay_time, -1);
            }

            if (envelope->decay_volume != 127)
            {
                pass = 0;
                printf("%s %d>fail: envelope->decay_volume=%d, expected %d\n", __func__, __LINE__, envelope->decay_volume, 127);
            }

            if (envelope->release_time != 5000)
            {
                pass = 0;
                printf("%s %d>fail: envelope->release_time=%d, expected %d\n", __func__, __LINE__, envelope->release_time, 5000);
            }

            keymap = sound->keymap;

            if (keymap == NULL)
            {
                pass = 0;
                printf("%s %d>fail: keymap is NULL\n", __func__, __LINE__);
            }

            if (keymap->velocity_min != 1)
            {
                pass = 0;
                printf("%s %d>fail: keymap->velocity_min=%d, expected %d\n", __func__, __LINE__, keymap->velocity_min, 1);
            }

            if (keymap->velocity_max != 127)
            {
                pass = 0;
                printf("%s %d>fail: keymap->velocity_max=%d, expected %d\n", __func__, __LINE__, keymap->velocity_max, 127);
            }

            if (keymap->key_min != 41)
            {
                pass = 0;
                printf("%s %d>fail: keymap->key_min=%d, expected %d\n", __func__, __LINE__, keymap->key_min, 41);
            }

            if (keymap->key_max != 41)
            {
                pass = 0;
                printf("%s %d>fail: keymap->key_max=%d, expected %d\n", __func__, __LINE__, keymap->key_max, 41);
            }

            if (keymap->key_base != 41)
            {
                pass = 0;
                printf("%s %d>fail: keymap->key_base=%d, expected %d\n", __func__, __LINE__, keymap->key_base, 41);
            }

            if (keymap->detune != 5)
            {
                pass = 0;
                printf("%s %d>fail: keymap->detune=%d, expected %d\n", __func__, __LINE__, keymap->detune, 5);
            }
        }
        else if (strcmp(sound->text_id, "Sound0138") == 0)
        {
            seen_sound_index_2++;
            
            if (sound->sample_volume != 110)
            {
                pass = 0;
                printf("%s %d>fail: sound->sample_volume=%d, expected 110\n", __func__, __LINE__, sound->sample_volume);
            }

            if (sound->sample_pan != 72)
            {
                pass = 0;
                printf("%s %d>fail: sound->sample_pan=%d, expected 72\n", __func__, __LINE__, sound->sample_pan);
            }
            
            wavetable = sound->wavetable;

            if (wavetable == NULL)
            {
                pass = 0;
                printf("%s %d>fail: wavetable is NULL\n", __func__, __LINE__);
            }

            if (strcmp(wavetable->aifc_path, "hit.aifc") != 0)
            {
                pass = 0;
                printf("%s %d>fail: wavetable->aifc_path=\"%s\", expected \"%s\"\n", __func__, __LINE__, wavetable->aifc_path, "hit.aifc");
            }

            envelope = sound->envelope;

            if (envelope == NULL)
            {
                pass = 0;
                printf("%s %d>fail: envelope is NULL\n", __func__, __LINE__);
            }

            if (envelope->attack_time != 11)
            {
                pass = 0;
                printf("%s %d>fail: envelope->attack_time=%d, expected %d\n", __func__, __LINE__, envelope->attack_time, 11);
            }

            if (envelope->attack_volume != 127)
            {
                pass = 0;
                printf("%s %d>fail: envelope->attack_volume=%d, expected %d\n", __func__, __LINE__, envelope->attack_volume, 127);
            }

            if (envelope->decay_time != 117913)
            {
                pass = 0;
                printf("%s %d>fail: envelope->decay_time=%d, expected %d\n", __func__, __LINE__, envelope->decay_time, 117913);
            }

            if (envelope->decay_volume != 127)
            {
                pass = 0;
                printf("%s %d>fail: envelope->decay_volume=%d, expected %d\n", __func__, __LINE__, envelope->decay_volume, 127);
            }

            if (envelope->release_time != 2000)
            {
                pass = 0;
                printf("%s %d>fail: envelope->release_time=%d, expected %d\n", __func__, __LINE__, envelope->release_time, 2000);
            }

            keymap = sound->keymap;

            if (keymap == NULL)
            {
                pass = 0;
                printf("%s %d>fail: keymap is NULL\n", __func__, __LINE__);
            }

            if (keymap->velocity_min != 9)
            {
                pass = 0;
                printf("%s %d>fail: keymap->velocity_min=%d, expected %d\n", __func__, __LINE__, keymap->velocity_min, 9);
            }

            if (keymap->velocity_max != 15)
            {
                pass = 0;
                printf("%s %d>fail: keymap->velocity_max=%d, expected %d\n", __func__, __LINE__, keymap->velocity_max, 15);
            }

            if (keymap->key_min != 4)
            {
                pass = 0;
                printf("%s %d>fail: keymap->key_min=%d, expected %d\n", __func__, __LINE__, keymap->key_min, 4);
            }

            if (keymap->key_max != 2)
            {
                pass = 0;
                printf("%s %d>fail: keymap->key_max=%d, expected %d\n", __func__, __LINE__, keymap->key_max, 2);
            }

            if (keymap->key_base != 48)
            {
                pass = 0;
                printf("%s %d>fail: keymap->key_base=%d, expected %d\n", __func__, __LINE__, keymap->key_base, 48);
            }

            if (keymap->detune != 19)
            {
                pass = 0;
                printf("%s %d>fail: keymap->detune=%d, expected %d\n", __func__, __LINE__, keymap->detune, 19);
            }
        }
    }

    if (seen_sound_index_0 != 1)
    {
        pass = 0;
        printf("%s %d>fail: seen_sound_index_0=%d, expected 1\n", __func__, __LINE__, seen_sound_index_0);
    }

    if (seen_sound_index_1 != 1)
    {
        pass = 0;
        printf("%s %d>fail: seen_sound_index_1=%d, expected 1\n", __func__, __LINE__, seen_sound_index_1);
    }

    if (seen_sound_index_2 != 1)
    {
        pass = 0;
        printf("%s %d>fail: seen_sound_index_2=%d, expected 1\n", __func__, __LINE__, seen_sound_index_2);
    }

    ALBankFile_free(bank_file);

    return pass;
}

void parse_inst_all(int *run_count, int *pass_count, int *fail_count)
{
    {
        /**
         * simple parse test
        */
        printf("parse inst test: 0001 - basic read\n");
        int pass;
        *run_count = *run_count + 1;

        struct file_info *fi = file_info_fopen("test_cases/inst_parse/0001.inst", "rb");
        struct ALBankFile *bank_file = ALBankFile_new_from_inst(fi);

        struct ALBank *bank;
        struct ALInstrument *instrument;
        struct ALSound *sound;
        struct ALWaveTable *wavetable;
        struct ALKeyMap *keymap;
        struct ALEnvelope *envelope;

        pass = 1;
        if (bank_file == NULL)
        {
            pass = 0;
            printf("%s %d>fail: bank_file is NULL\n", __func__, __LINE__);
        }

        if (bank_file->bank_count != 1)
        {
            pass = 0;
            printf("%s %d>fail: bank_file->bank_count=%d, expected 1\n", __func__, __LINE__, bank_file->bank_count);
        }

        if (bank_file->banks == NULL)
        {
            pass = 0;
            printf("%s %d>fail: bank_file->banks is NULL\n", __func__, __LINE__);
        }

        bank = bank_file->banks[0];

        if (bank == NULL)
        {
            pass = 0;
            printf("%s %d>fail: bank is NULL\n", __func__, __LINE__);
        }

        if (bank->inst_count != 1)
        {
            pass = 0;
            printf("%s %d>fail: bank->inst_count=%d, expected 1\n", __func__, __LINE__, bank->inst_count);
        }

        if (bank->instruments == NULL)
        {
            pass = 0;
            printf("%s %d>fail: bank->instruments is NULL\n", __func__, __LINE__);
        }

        instrument = bank->instruments[0];

        if (instrument == NULL)
        {
            pass = 0;
            printf("%s %d>fail: instrument is NULL\n", __func__, __LINE__);
        }

        if (instrument->sound_count != 1)
        {
            pass = 0;
            printf("%s %d>fail: instrument->sound_count=%d, expected 1\n", __func__, __LINE__, instrument->sound_count);
        }

        if (instrument->sounds == NULL)
        {
            pass = 0;
            printf("%s %d>fail: instrument->sounds is NULL\n", __func__, __LINE__);
        }

        sound = instrument->sounds[0];

        if (sound == NULL)
        {
            pass = 0;
            printf("%s %d>fail: sound is NULL\n", __func__, __LINE__);
        }

        wavetable = sound->wavetable;

        if (wavetable == NULL)
        {
            pass = 0;
            printf("%s %d>fail: wavetable is NULL\n", __func__, __LINE__);
        }

        if (strcmp(wavetable->aifc_path, "sound_effect_0001.aifc") != 0)
        {
            pass = 0;
            printf("%s %d>fail: wavetable->aifc_path=\"%s\", expected \"%s\"\n", __func__, __LINE__, wavetable->aifc_path, "sound_effect_0001.aifc");
        }

        envelope = sound->envelope;

        if (envelope == NULL)
        {
            pass = 0;
            printf("%s %d>fail: envelope is NULL\n", __func__, __LINE__);
        }

        if (envelope->attack_volume != 127)
        {
            pass = 0;
            printf("%s %d>fail: envelope->attack_volume=%d, expected %d\n", __func__, __LINE__, envelope->attack_volume, 127);
        }

        keymap = sound->keymap;

        if (keymap == NULL)
        {
            pass = 0;
            printf("%s %d>fail: keymap is NULL\n", __func__, __LINE__);
        }

        if (keymap->key_min != 1)
        {
            pass = 0;
            printf("%s %d>fail: keymap->key_min=%d, expected %d\n", __func__, __LINE__, keymap->key_min, 1);
        }

        ALBankFile_free(bank_file);
        file_info_free(fi);

        if (pass == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d>fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    {
        /**
         * test all supported values
        */
        printf("parse inst test: 0002 - read all supported values\n");
        int pass;
        *run_count = *run_count + 1;

        struct file_info *fi = file_info_fopen("test_cases/inst_parse/0002.inst", "rb");
        struct ALBankFile *bank_file = ALBankFile_new_from_inst(fi);

        pass = parse_inst_default(fi);

        ALBankFile_free(bank_file);
        file_info_free(fi);

        if (pass == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d>fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    {
        /**
         * parse test for whitespace and comments
        */
        printf("parse inst test: 0003 - whitespace and comments\n");
        int pass;
        *run_count = *run_count + 1;

        struct file_info *fi = file_info_fopen("test_cases/inst_parse/0003.inst", "rb");
        struct ALBankFile *bank_file = ALBankFile_new_from_inst(fi);

        pass = parse_inst_default(fi);

        ALBankFile_free(bank_file);
        file_info_free(fi);

        if (pass == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d>fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    {
        /**
         * test (class) elements out of order
        */
        printf("parse inst test: 0004 - re-ordered class elements\n");
        int pass;
        *run_count = *run_count + 1;

        struct file_info *fi = file_info_fopen("test_cases/inst_parse/0004.inst", "rb");
        struct ALBankFile *bank_file = ALBankFile_new_from_inst(fi);

        pass = parse_inst_default(fi);

        ALBankFile_free(bank_file);
        file_info_free(fi);

        if (pass == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d>fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    {
        /**
         * test sorting instrument->sound array elements (default)
        */
        printf("parse inst test: 0005 (0002.inst)- sort instrument->sound by .inst array index\n");
        int pass;
        *run_count = *run_count + 1;

        struct file_info *fi = file_info_fopen("test_cases/inst_parse/0002.inst", "rb");
        struct ALBankFile *bank_file = ALBankFile_new_from_inst(fi);

        pass = parse_inst_default(fi);

        if (pass)
        {
            if (strcmp(bank_file->banks[0]->instruments[0]->sounds[0]->text_id, "sound1") != 0)
            {
                pass = 0;
                printf("%s %d>fail: sounds[0]->text_id=\"%s\", expected \"%s\"\n", __func__, __LINE__, bank_file->banks[0]->instruments[0]->sounds[0]->text_id, "sound1");
            }

            if (strcmp(bank_file->banks[0]->instruments[0]->sounds[1]->text_id, "glass_sound") != 0)
            {
                pass = 0;
                printf("%s %d>fail: sounds[1]->text_id=\"%s\", expected \"%s\"\n", __func__, __LINE__, bank_file->banks[0]->instruments[0]->sounds[1]->text_id, "glass_sound");
            }

            if (strcmp(bank_file->banks[0]->instruments[0]->sounds[2]->text_id, "Sound0138") != 0)
            {
                pass = 0;
                printf("%s %d>fail: sounds[2]->text_id=\"%s\", expected \"%s\"\n", __func__, __LINE__, bank_file->banks[0]->instruments[0]->sounds[2]->text_id, "Sound0138");
            }
        }

        ALBankFile_free(bank_file);
        file_info_free(fi);

        if (pass == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d>fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    {
        /**
         * test sorting instrument->sound array elements
        */
        printf("parse inst test: 0006 - sort instrument->sound by .inst array index\n");
        int pass;
        *run_count = *run_count + 1;

        struct file_info *fi = file_info_fopen("test_cases/inst_parse/0006.inst", "rb");
        struct ALBankFile *bank_file = ALBankFile_new_from_inst(fi);

        pass = parse_inst_default(fi);

        if (pass)
        {
            if (strcmp(bank_file->banks[0]->instruments[0]->sounds[0]->text_id, "sound1") != 0)
            {
                pass = 0;
                printf("%s %d>fail: sounds[0]->text_id=\"%s\", expected \"%s\"\n", __func__, __LINE__, bank_file->banks[0]->instruments[0]->sounds[0]->text_id, "sound1");
            }

            if (strcmp(bank_file->banks[0]->instruments[0]->sounds[1]->text_id, "glass_sound") != 0)
            {
                pass = 0;
                printf("%s %d>fail: sounds[1]->text_id=\"%s\", expected \"%s\"\n", __func__, __LINE__, bank_file->banks[0]->instruments[0]->sounds[1]->text_id, "glass_sound");
            }

            if (strcmp(bank_file->banks[0]->instruments[0]->sounds[2]->text_id, "Sound0138") != 0)
            {
                pass = 0;
                printf("%s %d>fail: sounds[2]->text_id=\"%s\", expected \"%s\"\n", __func__, __LINE__, bank_file->banks[0]->instruments[0]->sounds[2]->text_id, "Sound0138");
            }
        }

        ALBankFile_free(bank_file);
        file_info_free(fi);

        if (pass == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d>fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }
}

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

    sub_count = 0;
    test_md5_all(&sub_count, &pass_count, &fail_count);
    total_run_count += sub_count;

    sub_count = 0;
    linked_list_all(&sub_count, &pass_count, &fail_count);
    total_run_count += sub_count;

    sub_count = 0;
    string_hash_all(&sub_count, &pass_count, &fail_count);
    total_run_count += sub_count;

    sub_count = 0;
    int_hash_all(&sub_count, &pass_count, &fail_count);
    total_run_count += sub_count;

    sub_count = 0;
    parse_inst_all(&sub_count, &pass_count, &fail_count);
    total_run_count += sub_count;

    printf("%d tests run, %d pass, %d fail\n", total_run_count, pass_count, fail_count);

    return 0;
}