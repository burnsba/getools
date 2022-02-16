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
#include "magic.h"
#include "adpcm_aifc.h"
#include "test_common.h"

#define TEST_EPSILON 0.0001
#define TEST_FAT_EPSILON 0.01
#define TEST_CHONK_EPSILON 0.5
#define TEST_ORDER 2

#ifndef NOGSL

// forward declarations

static void test_autocorrelation_vector(int *run_count, int *pass_count, int *fail_count);
static void test_autocorrelation_matrix(int *run_count, int *pass_count, int *fail_count);
static void test_lu_decomp_solve(int *run_count, int *pass_count, int *fail_count);
static void test_stable_kfroma(int *run_count, int *pass_count, int *fail_count);
static void test_afromk(int *run_count, int *pass_count, int *fail_count);
static void test_rfroma(int *run_count, int *pass_count, int *fail_count);
static void test_levinson_durbin_recursion(int *run_count, int *pass_count, int *fail_count);
static void test_codebook_row_from_predictors(int *run_count, int *pass_count, int *fail_count);
static void test_ALADPCMBook_set_predictor(int *run_count, int *pass_count, int *fail_count);

// end forward declarations

void magic_all(int *run_count, int *pass_count, int *fail_count)
{
    int sub_count;
    int local_run_count = 0;

    sub_count = 0;
    test_autocorrelation_vector(&sub_count, pass_count, fail_count);
    local_run_count += sub_count;

    sub_count = 0;
    test_autocorrelation_matrix(&sub_count, pass_count, fail_count);
    local_run_count += sub_count;

    sub_count = 0;
    test_lu_decomp_solve(&sub_count, pass_count, fail_count);
    local_run_count += sub_count;

    sub_count = 0;
    test_stable_kfroma(&sub_count, pass_count, fail_count);
    local_run_count += sub_count;

    sub_count = 0;
    test_afromk(&sub_count, pass_count, fail_count);
    local_run_count += sub_count;

    sub_count = 0;
    test_rfroma(&sub_count, pass_count, fail_count);
    local_run_count += sub_count;

    sub_count = 0;
    test_levinson_durbin_recursion(&sub_count, pass_count, fail_count);
    local_run_count += sub_count;

    sub_count = 0;
    test_codebook_row_from_predictors(&sub_count, pass_count, fail_count);
    local_run_count += sub_count;

    sub_count = 0;
    test_ALADPCMBook_set_predictor(&sub_count, pass_count, fail_count);
    local_run_count += sub_count;
}

static void test_autocorrelation_vector(int *run_count, int *pass_count, int *fail_count)
{
    {
        printf("autocorrelation_vector (1)\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 2
        #endif

        int i;

        double previous[16] = {
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0
        };

        double current[16] = {
            0, 0, 0, 0,
            1, 0, 0, 0,
            0, 2, -1, 0,
            0, 1, 0, 1
        };

        double expected[TEST_ORDER] = {
            8, -2
        };

        double actual[TEST_ORDER];

        autocorrelation_vector(previous, current, 16, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_EPSILON);
        }

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
        printf("autocorrelation_vector (2)\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 2
        #endif

        int i;

        double previous[16] = {
            0, 0, 0, 0,
            1, 0, 0, 0,
            0, 2, -1, 0,
            0, 1, 0, 1
        };

        double current[16] = {
            -1, 1, 0, 0,
            0, 1, 0, 1,
            0, 0, 0, 0,
            0, 0, 0, 0
        };

        double expected[TEST_ORDER] = {
            4, -2
        };

        double actual[TEST_ORDER];

        autocorrelation_vector(previous, current, 16, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_EPSILON);
        }

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
        printf("autocorrelation_vector (3)\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 2
        #endif

        int i;

        double previous[16] = {
            -1, 1, 0, 0,
            0, 1, 0, 1,
            0, 0, 0, 0,
            0, 0, 0, 0
        };

        double current[16] = {
            2,  -2,    4,   -3,    
            2,   0,   -1,    2,   
            -1,  0,    0,    0,    
            2,  -1,    2,   -2
        };

        double expected[TEST_ORDER] = {
            56, -42
        };

        double actual[TEST_ORDER];

        autocorrelation_vector(previous, current, 16, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_EPSILON);
        }

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
        printf("autocorrelation_vector (4)\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 2
        #endif

        int i;

        double previous[16] = {
            2,  -2,    4,   -3,    
            2,   0,   -1,    2,   
            -1,  0,    0,    0,    
            2,  -1,    2,   -2
        };

        double current[16] = {
            2,    0,    0,    1,
           -1,    1,    0,   -1,
            1,    0,    0,    1,
            0,    0,    0,    1
        };

        double expected[TEST_ORDER] = {
            11, -7
        };

        double actual[TEST_ORDER];

        autocorrelation_vector(previous, current, 16, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_EPSILON);
        }

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

static void test_autocorrelation_matrix(int *run_count, int *pass_count, int *fail_count)
{
    {
        printf("autocorrelation_matrix (1)\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 2
        #endif

        int i, j;

        double previous[16] = {
             -1,    1,    0,    0,
              0,    1,    0,    1,
              0,    0,    0,    0,
              0,    0,    0,    0
        };

        double current[16] = {
            2,   -2,    4,   -3,
            2,    0,   -1,    2,
           -1,    0,    0,    0,
            2,   -1,    2,   -2
        };

        double expected[TEST_ORDER][TEST_ORDER] = {
            { 52, -38 },
            { -38, 48 }
        };

        double **actual = matrix_f64_new(TEST_ORDER,TEST_ORDER);

        autocorrelation_matrix(previous, current, 16, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            for (j=0; j<TEST_ORDER; j++)
            {
                pass &= f64_equal(expected[i][j], actual[i][j], TEST_EPSILON);
                //printf("[%d][%d]: expected %f, actual %f\n", i, j, expected[i][j], actual[i][j]);
            }
        }

        matrix_f64_free(actual, TEST_ORDER);

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
        printf("autocorrelation_matrix (2)\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 2
        #endif

        int i, j;

        double previous[16] = {
            2,   -2,    4,   -3,
            2,    0,   -1,    2,
            -1,    0,    0,    0,
            2,   -1,    2,   -2,
        };

        double current[16] = {
            2,    0,    0,    1,
            -1,    1,    0,   -1,
            1,    0,    0,    1,
            0,    0,    0,    1
        };

        double expected[TEST_ORDER][TEST_ORDER] = {
            { 14, -11 },
            { -11, 18 }
        };

        double **actual = matrix_f64_new(TEST_ORDER,TEST_ORDER);

        autocorrelation_matrix(previous, current, 16, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            for (j=0; j<TEST_ORDER; j++)
            {
                pass &= f64_equal(expected[i][j], actual[i][j], TEST_EPSILON);
            }
        }

        matrix_f64_free(actual, TEST_ORDER);

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
        printf("autocorrelation_matrix (3)\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 2
        #endif

        int i, j;

        double previous[16] = {
            2,    0,    0,    1,
            -1,    1,    0,   -1,
            1,    0,    0,    1,
            0,    0,    0,    1,
        };

        double current[16] = {
            -1,    2,   -1,    0,
            1,    0,    0,    1,
            -2,    3,   -2,    3,
            -2,    3,   -2,    2
        };

        double expected[TEST_ORDER][TEST_ORDER] = {
            { 52, -43 },
            { -43, 48 }
        };

        double **actual = matrix_f64_new(TEST_ORDER,TEST_ORDER);

        autocorrelation_matrix(previous, current, 16, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            for (j=0; j<TEST_ORDER; j++)
            {
                pass &= f64_equal(expected[i][j], actual[i][j], TEST_EPSILON);
            }
        }

        matrix_f64_free(actual, TEST_ORDER);

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
        printf("autocorrelation_matrix (4)\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 2
        #endif

        int i, j;

        double previous[16] = {
            -1,    2,   -1,    0,
            1,    0,    0,    1,
            -2,    3,   -2,    3,
            -2,    3,   -2,    2
        };

        double current[16] = {
            -1,    2,   -1,    1,
            -1,    2,   -2,    3,
            -2,    1,    0,    0,
            2,   -1,    1,   -1
        };

        double expected[TEST_ORDER][TEST_ORDER] = {
            { 40, -35 },
            { -35, 43 }
        };

        double **actual = matrix_f64_new(TEST_ORDER,TEST_ORDER);

        autocorrelation_matrix(previous, current, 16, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            for (j=0; j<TEST_ORDER; j++)
            {
                pass &= f64_equal(expected[i][j], actual[i][j], TEST_EPSILON);
            }
        }

        matrix_f64_free(actual, TEST_ORDER);

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
        printf("autocorrelation_matrix - order 4 (5)\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 4
        #endif

        int i, j;

        double previous[16] = {
            2,    0,    0,    1,
            -1,    1,    0,   -1,
            1,    0,    0,    1,
            0,    0,    0,    1,
        };

        double current[16] = {
            -1,    2,   -1,    0,
            1,    0,    0,    1,
            -2,    3,   -2,    3,
            -2,    3,   -2,    2
        };

        double expected[TEST_ORDER][TEST_ORDER] = {
            { 52,  -43,  35,  -24 }, 
            { -43,  48,  -37,  31 }, 
            { 35,  -37,  39,  -31 }, 
            { -24,  31,  -31,  35 }
        };

        double **actual = matrix_f64_new(TEST_ORDER,TEST_ORDER);

        autocorrelation_matrix(previous, current, 16, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            for (j=0; j<TEST_ORDER; j++)
            {
                pass &= f64_equal(expected[i][j], actual[i][j], TEST_EPSILON);
            }
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                for (j=0; j<TEST_ORDER; j++)
                {
                    printf("%8.04f, ", expected[i][j]);
                }
                printf("\n");
            }

            printf("\n");
            printf("actual:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                for (j=0; j<TEST_ORDER; j++)
                {
                    printf("%8.04f, ", actual[i][j]);
                }
                printf("\n");
            }
        }

        matrix_f64_free(actual, TEST_ORDER);

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

static void test_lu_decomp_solve(int *run_count, int *pass_count, int *fail_count)
{
    {
        printf("lu_decomp_solve (1)\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 4
        #endif

        int i,j;

        double mat_1d[TEST_ORDER * TEST_ORDER] = {
            52.0000, -38.0000, 25.0000, -3.0000,
            -38.0000, 48.0000, -36.0000, 21.0000,
            25.0000, -36.0000, 47.0000, -34.0000,
            -3.0000, 21.0000, -34.0000, 43.0000
        };

        double b[TEST_ORDER] = {
            42, -27,   7, 10
        };

        double expected[TEST_ORDER] = {
            0.8285,  -0.2792,  -0.4601, 0.0629
        };

        double actual[TEST_ORDER] = { 0 };

        double **mat = matrix_f64_new(TEST_ORDER, TEST_ORDER);

        for (i=0; i<TEST_ORDER; i++)
        {
            for (j=0; j<TEST_ORDER; j++)
            {
                mat[i][j] = mat_1d[i*TEST_ORDER + j];
            }
        }

        // printf("b: \n");
        // for (i=0; i<TEST_ORDER; i++)
        // {
        //     printf("%8.04f ", b[i]);
        // }
        // printf("\n");
        // printf("mat: \n");
        // for (i=0; i<TEST_ORDER; i++)
        // {
        //     for (j=0; j<TEST_ORDER; j++)
        //     {
        //         printf("%.04f ", mat[i][j]);
        //     }
        //     printf("\n");
        // }

        lu_decomp_solve((double**)mat, b, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_FAT_EPSILON);
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", expected[i]);
            }
            printf("\n");
            printf("actual:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", actual[i]);
            }
            printf("\n");
        }

        matrix_f64_free(mat, TEST_ORDER);

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
        printf("lu_decomp_solve (2)\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 4
        #endif

        int i,j;

        double mat_1d[TEST_ORDER * TEST_ORDER] = {
            14.0000, -11.0000, 6.0000, -1.0000,
            -11.0000, 18.0000, -13.0000, 10,
            6.0000, -13.0000, 19.0000, -15.0000,
            -1.0000, 10, -15.0000, 23.0000
        };

        double b[TEST_ORDER] = {
            7.0000, -4.0000, -3.0000, 2.0000
        };

        double expected[TEST_ORDER] = {
            0.6138, -0.2070, -0.6856, -0.2435
        };

        double actual[TEST_ORDER] = { 0 };

        double **mat = matrix_f64_new(TEST_ORDER, TEST_ORDER);

        for (i=0; i<TEST_ORDER; i++)
        {
            for (j=0; j<TEST_ORDER; j++)
            {
                mat[i][j] = mat_1d[i*TEST_ORDER + j];
            }
        }

        lu_decomp_solve((double**)mat, b, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_FAT_EPSILON);
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", expected[i]);
            }
            printf("\n");
            printf("actual:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", actual[i]);
            }
            printf("\n");
        }

        matrix_f64_free(mat, TEST_ORDER);

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
        printf("lu_decomp_solve (3)\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 4
        #endif

        int i,j;

        double mat_1d[TEST_ORDER * TEST_ORDER] = {
            52.0000, -43.0000, 35.0000, -24.0000,
            -43.0000, 48.0000, -37.0000, 31.0000,
            35.0000, -37.0000, 39.0000, -31.0000,
            -24.0000, 31.0000, -31.0000, 35.0000
        };

        double b[TEST_ORDER] = {
            47.0000, -41.0000, 28.0000, -23.0000
        };

        double expected[TEST_ORDER] = {
            0.9749, -0.3320, -0.7743, -0.3804
        };

        double actual[TEST_ORDER] = { 0 };

        double **mat = matrix_f64_new(TEST_ORDER, TEST_ORDER);

        for (i=0; i<TEST_ORDER; i++)
        {
            for (j=0; j<TEST_ORDER; j++)
            {
                mat[i][j] = mat_1d[i*TEST_ORDER + j];
            }
        }

        lu_decomp_solve((double**)mat, b, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_FAT_EPSILON);
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", expected[i]);
            }
            printf("\n");
            printf("actual:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", actual[i]);
            }
            printf("\n");
        }

        matrix_f64_free(mat, TEST_ORDER);

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
        printf("lu_decomp_solve (4)\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 4
        #endif

        int i,j;

        double mat_1d[TEST_ORDER * TEST_ORDER] = {
            40, -35.0000, 35.0000, -27.0000,
            -35.0000, 43.0000, -40, 37.0000,
            35.0000, -40, 51.0000, -44.0000,
            -27.0000, 37.0000, -44.0000, 51.0000
        };

        double b[TEST_ORDER] = {
            32.0000, -30, 25.0000, -21.0000
        };

        double expected[TEST_ORDER] = {
            0.7609, -0.3849, -0.3937, -0.0694
        };

        double actual[TEST_ORDER] = { 0 };

        double **mat = matrix_f64_new(TEST_ORDER, TEST_ORDER);

        for (i=0; i<TEST_ORDER; i++)
        {
            for (j=0; j<TEST_ORDER; j++)
            {
                mat[i][j] = mat_1d[i*TEST_ORDER + j];
            }
        }

        lu_decomp_solve((double**)mat, b, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_FAT_EPSILON);
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", expected[i]);
            }
            printf("\n");
            printf("actual:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", actual[i]);
            }
            printf("\n");
        }

        matrix_f64_free(mat, TEST_ORDER);

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

static void test_stable_kfroma(int *run_count, int *pass_count, int *fail_count)
{
    {
        printf("stable_kfroma (1)\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 4
        #endif

        int i;

        double parameters[TEST_ORDER] = {
            0.8285,  -0.2792,  -0.4601,   0.0629
        };

        double expected[TEST_ORDER] = {
            0.7927,   0.2447,  -0.5143,   0.0629
        };

        double actual[TEST_ORDER];

        stable_kfroma(parameters, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_EPSILON);
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", expected[i]);
            }
            printf("\n");
            printf("actual:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", actual[i]);
            }
            printf("\n");
        }

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
        printf("stable_kfroma (2)\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 4
        #endif

        int i;

        double parameters[TEST_ORDER] = {
            0.6138,  -0.2070,  -0.6856,  -0.2435
        };

        double expected[TEST_ORDER] = {
            0.4746,  -0.0043,  -0.5699,  -0.2435
        };

        double actual[TEST_ORDER];

        stable_kfroma(parameters, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_EPSILON);
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", expected[i]);
            }
            printf("\n");
            printf("actual:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", actual[i]);
            }
            printf("\n");
        }

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
        printf("stable_kfroma (3)\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 4
        #endif

        int i;

        double parameters[TEST_ORDER] = {
            0.9749,  -0.3320,  -0.7743,  -0.3804
        };

        double expected[TEST_ORDER] = {
            0.8797,  -0.2066,  -0.4717,  -0.3804
        };

        double actual[TEST_ORDER];

        stable_kfroma(parameters, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_EPSILON);
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", expected[i]);
            }
            printf("\n");
            printf("actual:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", actual[i]);
            }
            printf("\n");
        }

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
        printf("stable_kfroma (4)\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 4
        #endif

        int i;

        double parameters[TEST_ORDER] = {
            0.7609,  -0.3849,  -0.3937,  -0.0694
        };

        double expected[TEST_ORDER] = {
            0.8252,  -0.1825,  -0.3426,  -0.0694
        };

        double actual[TEST_ORDER];

        stable_kfroma(parameters, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_EPSILON);
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", expected[i]);
            }
            printf("\n");
            printf("actual:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", actual[i]);
            }
            printf("\n");
        }

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

static void test_afromk(int *run_count, int *pass_count, int *fail_count)
{
    {
        printf("afromk (1)\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 4
        #endif

        int i;

        double parameters[TEST_ORDER] = {
            0.7927,   0.2447,  -0.5143,   0.0629
        };

        double expected[TEST_ORDER] = {
            0.8285,  -0.2104,  -0.4621,   0.0629
        };

        double actual[TEST_ORDER];

        afromk(parameters, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_EPSILON);
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", expected[i]);
            }
            printf("\n");
            printf("actual:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", actual[i]);
            }
            printf("\n");
        }

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
        printf("afromk (2)\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 4
        #endif

        int i;

        double parameters[TEST_ORDER] = {
            0.4746,  -0.0043,  -0.5699,  -0.2435
        };

        double expected[TEST_ORDER] = {
            0.6138,  -0.2081,  -0.7193,  -0.2435
        };

        double actual[TEST_ORDER];

        afromk(parameters, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_EPSILON);
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", expected[i]);
            }
            printf("\n");
            printf("actual:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", actual[i]);
            }
            printf("\n");
        }

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
        printf("afromk (3)\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 4
        #endif

        int i;

        double parameters[TEST_ORDER] = {
            0.8797,  -0.2066,  -0.4717,  -0.3804
        };

        double expected[TEST_ORDER] = {
            0.9749,  -0.3605,  -0.8426,  -0.3804
        };

        double actual[TEST_ORDER];

        afromk(parameters, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_EPSILON);
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", expected[i]);
            }
            printf("\n");
            printf("actual:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", actual[i]);
            }
            printf("\n");
        }

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
        printf("afromk (4)\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 4
        #endif

        int i;

        double parameters[TEST_ORDER] = {
            0.8252,  -0.1825,  -0.3426,  -0.0694
        };

        double expected[TEST_ORDER] = {
            0.7609,  -0.4048,  -0.3954,  -0.0694
        };

        double actual[TEST_ORDER];

        afromk(parameters, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_EPSILON);
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", expected[i]);
            }
            printf("\n");
            printf("actual:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", actual[i]);
            }
            printf("\n");
        }

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

static void test_rfroma(int *run_count, int *pass_count, int *fail_count)
{
    {
        printf("rfroma (1)\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 4
        #endif

        int i;

        double parameters[TEST_ORDER] = {
            0.828464, -0.210441, -0.462126, 0.062937
        };

        double expected[TEST_ORDER] = {
            -0.774252, 0.464824, -0.037169, -0.292128
        };

        double actual[TEST_ORDER];

        rfroma(parameters, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_EPSILON);
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", expected[i]);
            }
            printf("\n");
            printf("actual:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", actual[i]);
            }
            printf("\n");
        }

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
        printf("rfroma (2)\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 4
        #endif

        int i;

        double parameters[TEST_ORDER] = {
            0.613765, -0.208079, -0.719339, -0.243455
        };

        double expected[TEST_ORDER] = {
            -0.467896, 0.209745, 0.379334, -0.282300
        };

        double actual[TEST_ORDER];

        rfroma(parameters, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_EPSILON);
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", expected[i]);
            }
            printf("\n");
            printf("actual:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", actual[i]);
            }
            printf("\n");
        }

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
        printf("rfroma (3)\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 4
        #endif

        int i;

        double parameters[TEST_ORDER] = {
            0.974905, -0.360493, -0.842567, -0.380386
        };

        double expected[TEST_ORDER] = {
            -0.828797, 0.758819, -0.511249, 0.454038
        };

        double actual[TEST_ORDER];

        rfroma(parameters, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_EPSILON);
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", expected[i]);
            }
            printf("\n");
            printf("actual:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", actual[i]);
            }
            printf("\n");
        }

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
        printf("rfroma (4)\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 4
        #endif

        int i;

        double parameters[TEST_ORDER] = {
            0.760887, -0.404833, -0.395377, -0.069387
        };

        double expected[TEST_ORDER] = {
            -0.838707, 0.764430, -0.584000, 0.491605
        };

        double actual[TEST_ORDER];

        rfroma(parameters, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_EPSILON);
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", expected[i]);
            }
            printf("\n");
            printf("actual:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", actual[i]);
            }
            printf("\n");
        }

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

static void test_levinson_durbin_recursion(int *run_count, int *pass_count, int *fail_count)
{
    {
        printf("levinson_durbin_recursion (1) -- order=2\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 2
        #endif

        int i;

        double parameters[TEST_ORDER] = {
            0.950353, 0.866164
        };

        double expected[TEST_ORDER] = {
            -0.950353, 0.382191
        };

        double actual[TEST_ORDER];

        levinson_durbin_recursion(parameters, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_EPSILON);
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", expected[i]);
            }
            printf("\n");
            printf("actual:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", actual[i]);
            }
            printf("\n");
        }

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
        printf("levinson_durbin_recursion (2) -- order=2\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 2
        #endif

        int i;

        double parameters[TEST_ORDER] = {
            0.878123, 0.716890
        };

        double expected[TEST_ORDER] = {
            -0.878123, 0.236829
        };

        double actual[TEST_ORDER];

        levinson_durbin_recursion(parameters, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_EPSILON);
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", expected[i]);
            }
            printf("\n");
            printf("actual:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", actual[i]);
            }
            printf("\n");
        }

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
        printf("levinson_durbin_recursion (3) -- order=2\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 2
        #endif

        int i;

        double parameters[TEST_ORDER] = {
            0.980259, 0.927968
        };

        double expected[TEST_ORDER] = {
            -0.980259, 0.842585
        };

        double actual[TEST_ORDER];

        levinson_durbin_recursion(parameters, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_EPSILON);
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", expected[i]);
            }
            printf("\n");
            printf("actual:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", actual[i]);
            }
            printf("\n");
        }

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
        printf("levinson_durbin_recursion (4) -- order=3 \n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 3
        #endif

        int i;

        double parameters[TEST_ORDER] = {
            0.741880, 0.191422, -0.591202
        };

        double expected[TEST_ORDER] = {
            -0.741880, 0.798382, 1.559842
        };

        double actual[TEST_ORDER];

        levinson_durbin_recursion(parameters, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_EPSILON);
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", expected[i]);
            }
            printf("\n");
            printf("actual:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", actual[i]);
            }
            printf("\n");
        }

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
        printf("levinson_durbin_recursion (5) -- order=3 \n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 3
        #endif

        int i;

        double parameters[TEST_ORDER] = {
            0.472505, 0.323270, 0.425205
        };

        double expected[TEST_ORDER] = {
            -0.472505, -0.128755, -0.302787
        };

        double actual[TEST_ORDER];

        levinson_durbin_recursion(parameters, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_EPSILON);
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", expected[i]);
            }
            printf("\n");
            printf("actual:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", actual[i]);
            }
            printf("\n");
        }

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
        printf("levinson_durbin_recursion (6) -- order=4 \n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 4
        #endif

        int i;

        double parameters[TEST_ORDER] = {
            5.974759, 17.762763, 25.204166, 10.179954
        };

        double expected[TEST_ORDER] = {
            -5.974759, 0, 0, 0
        };

        double actual[TEST_ORDER];

        levinson_durbin_recursion(parameters, TEST_ORDER, actual);

        for (i=0; i<TEST_ORDER; i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_EPSILON);
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", expected[i]);
            }
            printf("\n");
            printf("actual:\n");
            for (i=0; i<TEST_ORDER; i++)
            {
                printf("%8.04f, ", actual[i]);
            }
            printf("\n");
        }

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

static void test_codebook_row_from_predictors(int *run_count, int *pass_count, int *fail_count)
{
    {
        printf("codebook_row_from_predictors (1) -- order=2 \n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 2
        #endif

        int i;

        double parameters[TEST_ORDER] = {
            0.757515, -0.023203
        };

        double expected[(TEST_ORDER*FRAME_DECODE_ROW_LEN)] = {
            47.519819, -35.996987, 28.370871, -22.326606,
            17.571035, -13.828372, 10.882904, -8.564826,
            -1551.391216, 1222.722313, -962.227776, 757.273078,
            -595.972506, 469.029292, -369.125210, 290.500877
        };

        double *actual = codebook_row_from_predictors(parameters, TEST_ORDER);

        for (i=0; i<(TEST_ORDER*FRAME_DECODE_ROW_LEN); i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_FAT_EPSILON);
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<(TEST_ORDER*FRAME_DECODE_ROW_LEN); i++)
            {
                printf("%8.04f, ", expected[i]);
            }
            printf("\n");
            printf("actual:\n");
            for (i=0; i<(TEST_ORDER*FRAME_DECODE_ROW_LEN); i++)
            {
                printf("%8.04f, ", actual[i]);
            }
            printf("\n");
        }

        free(actual);

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
        printf("codebook_row_from_predictors (2) -- order=2 \n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 2
        #endif

        int i;

        double parameters[TEST_ORDER] = {
            -1.701975, 0.840607
        };

        double expected[(TEST_ORDER*FRAME_DECODE_ROW_LEN)] = {
            -1721.562242, -2930.055826, -3539.725123, -3561.499361,
            -3086.066555, -2258.588259, -1249.892856, -228.702227,
            3485.644715, 4210.917778, 4236.820785, 3671.237757,
            2686.855369, 1486.894000, 272.068096, -786.839770
        };

        double *actual = codebook_row_from_predictors(parameters, TEST_ORDER);

        for (i=0; i<(TEST_ORDER*FRAME_DECODE_ROW_LEN); i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_CHONK_EPSILON);
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<(TEST_ORDER*FRAME_DECODE_ROW_LEN); i++)
            {
                printf("%8.04f, ", expected[i]);
            }
            printf("\n");
            printf("actual:\n");
            for (i=0; i<(TEST_ORDER*FRAME_DECODE_ROW_LEN); i++)
            {
                printf("%8.04f, ", actual[i]);
            }
            printf("\n");
        }

        free(actual);

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
        printf("codebook_row_from_predictors (3) -- order=3 \n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 3
        #endif

        int i;

        double parameters[TEST_ORDER] = {
            -2.731960, 3.723956, -1.000000
        };

        double expected[(TEST_ORDER*FRAME_DECODE_ROW_LEN)] = {
            2048.000000, 5595.053692, 7658.800366, 2135.802200,
            -17091.054508, -46986.906646, -62584.206979, -13091.429651,
            -7626.661460, -18787.732599, -22925.980285, -294.832608,
            65782.133617, 157886.108618, 186073.913383, -13832.301713,
            5595.053692, 7658.800366, 2135.802201, -17091.054510,
            -46986.906651, -62584.206985, -13091.429652, 150308.653709
        };

        double *actual = codebook_row_from_predictors(parameters, TEST_ORDER);

        for (i=0; i<(TEST_ORDER*FRAME_DECODE_ROW_LEN); i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_CHONK_EPSILON);
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<(TEST_ORDER*FRAME_DECODE_ROW_LEN); i++)
            {
                printf("%8.04f, ", expected[i]);
            }
            printf("\n");
            printf("actual:\n");
            for (i=0; i<(TEST_ORDER*FRAME_DECODE_ROW_LEN); i++)
            {
                printf("%8.04f, ", actual[i]);
            }
            printf("\n");
        }

        free(actual);

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
        printf("codebook_row_from_predictors (4) -- order=3 \n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 3
        #endif

        int i;

        double parameters[TEST_ORDER] = {
            0.047930, -0.791675, -0.212466
        };

        double expected[(TEST_ORDER*FRAME_DECODE_ROW_LEN)] = {
            435.130933, -20.855950, 345.481818, 59.380477,
            266.231934, 107.652689, 218.225634, 131.331610,
            1621.349955, 357.419167, 1266.450671, 566.740586,
            1051.392542, 667.358744, 920.787529, 707.582937,
            -98.161224, 1626.054851, 279.481894, 1253.055024,
            506.681302, 1027.107162, 618.129205, 891.160420
        };

        double *actual = codebook_row_from_predictors(parameters, TEST_ORDER);

        for (i=0; i<(TEST_ORDER*FRAME_DECODE_ROW_LEN); i++)
        {
            pass &= f64_equal(expected[i], actual[i], TEST_CHONK_EPSILON);
        }

        if (!pass)
        {
            printf("expected:\n");
            for (i=0; i<(TEST_ORDER*FRAME_DECODE_ROW_LEN); i++)
            {
                printf("%8.04f, ", expected[i]);
            }
            printf("\n");
            printf("actual:\n");
            for (i=0; i<(TEST_ORDER*FRAME_DECODE_ROW_LEN); i++)
            {
                printf("%8.04f, ", actual[i]);
            }
            printf("\n");
        }

        free(actual);

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

static void test_ALADPCMBook_set_predictor(int *run_count, int *pass_count, int *fail_count)
{
    {
        printf("ALADPCMBook_set_predictor (1) -- order=2, npredictors=1, predictor=0\n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 2
        #endif

        int i;
        int book_predictor = 0;
        struct ALADPCMBook *book;

        double parameters[(TEST_ORDER*FRAME_DECODE_ROW_LEN)] = {
            47.519819, -35.996987, 28.370871, -22.326606,
            17.571035, -13.828372, 10.882904, -8.564826,
            -1551.391216, 1222.722313, -962.227776, 757.273078,
            -595.972506, 469.029292, -369.125210, 290.500877
        };

        int16_t expected[(TEST_ORDER*FRAME_DECODE_ROW_LEN)] = {
            48,    -36,     28,    -22,
            18,    -14,     11,     -9,
            -1551,   1223,   -962,    757,
            -596,    469,   -369,    291
        };

        book = ALADPCMBook_new(TEST_ORDER, 1);

        ALADPCMBook_set_predictor(book, parameters, book_predictor);

        for (i=0; i<(TEST_ORDER*FRAME_DECODE_ROW_LEN); i++)
        {
            pass &= (expected[i] == book->book[(book_predictor * (TEST_ORDER*FRAME_DECODE_ROW_LEN)) + i]);
        }

        ALADPCMBook_free(book);

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
        printf("ALADPCMBook_set_predictor (2) -- order=2, npredictors=2, predictor=1 \n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 2
        #endif

        int i;
        int book_predictor = 1;
        struct ALADPCMBook *book;

        double parameters[(TEST_ORDER*FRAME_DECODE_ROW_LEN)] = {
            47.519819, -35.996987, 28.370871, -22.326606,
            17.571035, -13.828372, 10.882904, -8.564826,
            -1551.391216, 1222.722313, -962.227776, 757.273078,
            -595.972506, 469.029292, -369.125210, 290.500877
        };

        int16_t expected[(TEST_ORDER*FRAME_DECODE_ROW_LEN)] = {
            48,    -36,     28,    -22,
            18,    -14,     11,     -9,
            -1551,   1223,   -962,    757,
            -596,    469,   -369,    291
        };

        book = ALADPCMBook_new(TEST_ORDER, 2);

        ALADPCMBook_set_predictor(book, parameters, book_predictor);

        for (i=0; i<(TEST_ORDER*FRAME_DECODE_ROW_LEN); i++)
        {
            pass &= (0 == book->book[i]);
            pass &= (expected[i] == book->book[(book_predictor * (TEST_ORDER*FRAME_DECODE_ROW_LEN)) + i]);
        }

        if (!pass)
        {
            int j;
            int count = 0;

            printf("expected:\n");
            for (j=0; j<TEST_ORDER; j++)
            {
                for (i=0; i<FRAME_DECODE_ROW_LEN; i++)
                {
                    printf("%d ", expected[count]);
                    count++;
                }
                printf("\n");
            }
            printf("\n");

            count = 0;
            printf("actual:\n");
            for (j=0; j<TEST_ORDER; j++)
            {
                for (i=0; i<FRAME_DECODE_ROW_LEN; i++)
                {
                    printf("%d ", book->book[(book_predictor * (TEST_ORDER*FRAME_DECODE_ROW_LEN)) + count]);
                    count++;
                }
                printf("\n");
            }
        }

        ALADPCMBook_free(book);

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
        printf("ALADPCMBook_set_predictor (3) -- order=2, npredictors=3, predictor=1 \n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 2
        #endif

        int i;
        int book_predictor = 1;
        struct ALADPCMBook *book;

        double parameters[(TEST_ORDER*FRAME_DECODE_ROW_LEN)] = {
            47.519819, -35.996987, 28.370871, -22.326606,
            17.571035, -13.828372, 10.882904, -8.564826,
            -1551.391216, 1222.722313, -962.227776, 757.273078,
            -595.972506, 469.029292, -369.125210, 290.500877
        };

        int16_t expected[(TEST_ORDER*FRAME_DECODE_ROW_LEN)] = {
            48,    -36,     28,    -22,
            18,    -14,     11,     -9,
            -1551,   1223,   -962,    757,
            -596,    469,   -369,    291
        };

        book = ALADPCMBook_new(TEST_ORDER, 3);

        ALADPCMBook_set_predictor(book, parameters, book_predictor);

        for (i=0; i<(TEST_ORDER*FRAME_DECODE_ROW_LEN); i++)
        {
            pass &= (0 == book->book[i]);
            pass &= (expected[i] == book->book[(book_predictor * (TEST_ORDER*FRAME_DECODE_ROW_LEN)) + i]);
        }

        if (!pass)
        {
            int j;
            int count = 0;

            printf("expected:\n");
            for (j=0; j<TEST_ORDER; j++)
            {
                for (i=0; i<FRAME_DECODE_ROW_LEN; i++)
                {
                    printf("%d ", expected[count]);
                    count++;
                }
                printf("\n");
            }
            printf("\n");

            count = 0;
            printf("actual:\n");
            for (j=0; j<TEST_ORDER; j++)
            {
                for (i=0; i<FRAME_DECODE_ROW_LEN; i++)
                {
                    printf("%d ", book->book[(book_predictor * (TEST_ORDER*FRAME_DECODE_ROW_LEN)) + count]);
                    count++;
                }
                printf("\n");
            }
        }

        ALADPCMBook_free(book);

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
        printf("ALADPCMBook_set_predictor (3) -- order=3, npredictors=3, predictor=1 \n");
        int pass = 1;
        *run_count = *run_count + 1;
        #if defined(TEST_ORDER)
            #undef TEST_ORDER
            #define TEST_ORDER 3
        #endif

        int i;
        int book_predictor = 1;
        struct ALADPCMBook *book;

        double parameters[(TEST_ORDER*FRAME_DECODE_ROW_LEN)] = {
            435.130933, -20.855950, 345.481818, 59.380477,
            266.231934, 107.652689, 218.225634, 131.331610,
            1621.349955, 357.419167, 1266.450671, 566.740586,
            1051.392542, 667.358744, 920.787529, 707.582937,
            -98.161224, 1626.054851, 279.481894, 1253.055024,
            506.681302, 1027.107162, 618.129205, 891.160420
        };

        int16_t expected[(TEST_ORDER*FRAME_DECODE_ROW_LEN)] = {
            435,    -21,    345,     59,
            266,    108,    218,    131,
            1621,    357,   1266,    567,
            1051,    667,    921,    708,
            -98,   1626,    279,   1253,
            507,   1027,    618,    891
        };

        book = ALADPCMBook_new(TEST_ORDER, 3);

        ALADPCMBook_set_predictor(book, parameters, book_predictor);

        for (i=0; i<(TEST_ORDER*FRAME_DECODE_ROW_LEN); i++)
        {
            pass &= (0 == book->book[i]);
            pass &= (expected[i] == book->book[(book_predictor * (TEST_ORDER*FRAME_DECODE_ROW_LEN)) + i]);
        }

        if (!pass)
        {
            int j;
            int count = 0;

            printf("expected:\n");
            for (j=0; j<TEST_ORDER; j++)
            {
                for (i=0; i<FRAME_DECODE_ROW_LEN; i++)
                {
                    printf("%d ", expected[count]);
                    count++;
                }
                printf("\n");
            }
            printf("\n");

            count = 0;
            printf("actual:\n");
            for (j=0; j<TEST_ORDER; j++)
            {
                for (i=0; i<FRAME_DECODE_ROW_LEN; i++)
                {
                    printf("%d ", book->book[(book_predictor * (TEST_ORDER*FRAME_DECODE_ROW_LEN)) + count]);
                    count++;
                }
                printf("\n");
            }
        }

        ALADPCMBook_free(book);

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

#else

// GNU Scientific library is not available, can't build coefficient table.
void magic_all(int *run_count, int *pass_count, int *fail_count)
{
    // nothing to do.
}

#endif

#undef TEST_EPSILON
#undef TEST_ORDER