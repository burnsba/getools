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
#include "md5.h"

/**
 * This file contains simple unit tests.
*/

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

    sub_count = 0;
    test_md5_all(&sub_count, &pass_count, &fail_count);
    total_run_count += sub_count;

    sub_count = 0;
    linked_list_all(&sub_count, &pass_count, &fail_count);
    total_run_count += sub_count;

    sub_count = 0;
    string_hash_all(&sub_count, &pass_count, &fail_count);
    total_run_count += sub_count;

    printf("%d tests run, %d pass, %d fail\n", total_run_count, pass_count, fail_count);

    return 0;
}