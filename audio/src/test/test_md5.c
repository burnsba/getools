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

// forward declarations.

void print16(char *arr);

// end forward declarations

void print16(char *arr)
{
    int i;
    for (i=0; i<16; i++)
    {
        printf("%02x", (0xff & arr[i]));
    }
    printf("\n");
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