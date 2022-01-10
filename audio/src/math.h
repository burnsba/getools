#ifndef _GAUDIO_MATH_H_
#define _GAUDIO_MATH_H_

#include <stdint.h>
#include <stdlib.h>

int32_t dot_product_i32(int32_t *arr1, int32_t *arr2, size_t len);
int32_t divide_round_down(int32_t num, int32_t den);
int32_t clamp(int32_t val, int32_t lt, int32_t gt);

#endif