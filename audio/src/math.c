
#include <stdint.h>
#include <stdlib.h>

#include "utility.h"

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
        stderr_exit(1, "dot_product_i32: arr1 is NULL\n");
    }

    if (arr2 == NULL)
    {
        stderr_exit(1, "dot_product_i32: arr2 is NULL\n");
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
 * @param den less than or equal to {@code num}. Fatal error if {@code den} is zero.
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
        stderr_exit(1, "divide_round_down: divide by zero\n");
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