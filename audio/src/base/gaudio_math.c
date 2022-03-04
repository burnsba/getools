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
#include <stdlib.h>
#include <string.h>
#include "machine_config.h"
#include "utility.h"
#include "gaudio_math.h"

/**
 * This file contains mathematical methods.
*/

/**
 * Calculates the dot product on two int32_t arrays. Assumes
 * the arrays are atleast {@code len} length long.
 * @param arr1: first int32_t array
 * @param arr2: second int32_t array
 * @param len: length of arrays
 * @returns: int32_t result
*/
int32_t dot_product_i32(int32_t *arr1, int32_t *arr2, size_t len)
{
    size_t i;
    int32_t result = 0;

    if (arr1 == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> arr1 is NULL\n", __func__, __LINE__);
    }

    if (arr2 == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> arr2 is NULL\n", __func__, __LINE__);
    }

    for (i=0; i<len; i++)
    {
        result += arr1[i] * arr2[i];
    }

    return result;
}

/**
 * Divides {@code num} by {@code den}, rounding the result down such that
 * result times {@code den} will be the nearest integral multiple of
 * {@code den} less than or equal to {@code num}. Fatal error if {@code den} is zero.
 * @param num: numerator
 * @param den: denominator
 * @returns: int32_t result
*/
int32_t divide_round_down(int32_t num, int32_t den)
{
    int32_t result;
    int32_t recalc;

    if (den == 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> divide_round_down: divide by zero\n", __func__, __LINE__);
    }

    result = num / den;
    recalc = result * den;

    if (recalc > num)
    {
        return result - 1;
    }
    else
    {
        return result;
    }
}

/**
 * Clamps a value between a range.
 * @param val: input value.
 * @param lt: if {@code val} is less than {@code lt}, then the result is {@code lt}.
 * @param gt: if {@code val} is greater than {@code gt}, then the result is {@code gt}.
 * @returns: {@code val}, or clamped value.
*/
int32_t clamp(int32_t val, int32_t lt, int32_t gt)
{
    if (val < lt)
    {
        return lt;
    }

    if (val > gt)
    {
        return gt;
    }

    return val;
}

/**
 * Clamps a value between a range.
 * @param val: input value.
 * @param lt: if {@code val} is less than {@code lt}, then the result is {@code lt}.
 * @param gt: if {@code val} is greater than {@code gt}, then the result is {@code gt}.
 * @returns: {@code val}, or clamped value.
*/
double clamp_f64(double val, double lt, double gt)
{
    if (val < lt)
    {
        return lt;
    }

    if (val > gt)
    {
        return gt;
    }

    return val;
}

/**
 * Clamps all values in array to a range.
 * The min and max values are inclusive.
 * If value is greater than or equal to {@code max}, then value will be set to {@code max - epsilon}.
 * If value is less than or equal to {@code min}, then value will be set to {@code min + epsilon}.
*/
void clamp_inclusive_array_f64_epsilon(double *arr, size_t len, double min, double max, double epsilon)
{
    if (arr == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> arr is NULL\n", __func__, __LINE__);
    }

    size_t i;

    for (i=0; i<len; i++)
    {
        if (arr[i] <= min)
        {
            arr[i] = min + epsilon;
        }
        else if (arr[i] >= max)
        {
            arr[i] = max - epsilon;
        }
    }
}

/**
 * Multiply every value in an array by amount.
 * @param arr: array to scale.
 * @param len: number of elements in array.
 * @param amount: amount to multiply each element by.
*/
void scale_f64_array(double *arr, size_t len, double amount)
{
    if (arr == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> arr is NULL\n", __func__, __LINE__);
    }

    size_t i;

    for (i=0; i<len; i++)
    {
        arr[i] *= amount;
    }
}

/**
 * Forward quantization step, divide by scale then add one half.
 * Unless the value is negative, then subtract one half.
 * @param x: value to quantize.
 * @param scale: amount to divide by.
 * @returns: 16-bit quantized amount.
*/
int16_t forward_quantize(float x, int32_t scale)
{
    if (x > 0.0f)
    {
        return (int16_t) ((x / scale) + 0.4999999);
    }
    else
    {
        return (int16_t) ((x / scale) - 0.4999999);
    }
}

/**
 * Forward quantization step, divide by scale then add one half.
 * Unless the value is negative, then subtract one half.
 * @param x: value to quantize.
 * @param scale: amount to divide by.
 * @returns: 16-bit quantized amount.
*/
int16_t forward_quantize_f64(double x, int32_t scale)
{
    if (x > 0.0f)
    {
        return (int16_t) ((x / scale) + 0.4999999);
    }
    else
    {
        return (int16_t) ((x / scale) - 0.4999999);
    }
}

/**
 * Allocates memory for a new 2d matrix of type double.
 * @param row_count: number of rows.
 * @param col_count: number of columns.
 * @returns: pointer to new matrix.
*/
double **matrix_f64_new(size_t row_count, size_t col_count)
{
    size_t i;

    double **mat = (double **)malloc_zero(row_count, sizeof(double *));

    for (i=0; i<row_count; i++)
    {
        mat[i] = (double *)malloc_zero(col_count, sizeof(double));
    }

    return mat;
}

/**
 * Allocates memory for a new 2d matrix of type double and copies values
 * from an existing matrix.
 * @param source: existing matrix to copy.
 * @param row_count: number of rows.
 * @param col_count: number of columns.
 * @returns: pointer to new matrix.
*/
double **matrix_f64_new_clone(double** source, size_t row_count, size_t col_count)
{
    if (source == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> source is NULL\n", __func__, __LINE__);
    }

    size_t i;
    double **dest = (double **)malloc_zero(row_count, sizeof(double *));

    for (i=0; i<row_count; i++)
    {
        dest[i] = (double *)malloc_zero(col_count, sizeof(double));
        memcpy(dest[i], source[i], col_count * sizeof(double));
    }

    return dest;
}

/**
 * Copies an existing matrix into another matrix.
 * This must have the same dimensions.
 * @param dest: Destination matrix.
 * @param source: Source matrix.
 * @param row_count: number of rows.
 * @param col_count: number of columns.
*/
void matrix_f64_copy(double **dest, double** source, size_t row_count, size_t col_count)
{
    if (source == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> source is NULL\n", __func__, __LINE__);
    }

    if (dest == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> dest is NULL\n", __func__, __LINE__);
    }

    size_t i;

    for (i=0; i<row_count; i++)
    {
        memcpy(dest[i], source[i], col_count * sizeof(double));
    }
}

/**
 * Frees memory allocated to a matrix and all child rows.
 * @param mat: matrix to free.
 * @param row_count: number of rows in the matrix.
*/
void matrix_f64_free(double **mat, size_t row_count)
{
    if (mat == NULL)
    {
        return;
    }

    size_t i;

    for (i=0; i<row_count; i++)
    {
        if (mat[i] != NULL)
        {
            free(mat[i]);
        }
    }

    free(mat);
}