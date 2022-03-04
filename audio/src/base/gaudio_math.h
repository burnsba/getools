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
#ifndef _GAUDIO_MATH_H_
#define _GAUDIO_MATH_H_

#include <stdint.h>
#include <stdlib.h>

int32_t dot_product_i32(int32_t *arr1, int32_t *arr2, size_t len);
int32_t divide_round_down(int32_t num, int32_t den);
int32_t clamp(int32_t val, int32_t lt, int32_t gt);
double clamp_f64(double val, double lt, double gt);
void clamp_inclusive_array_f64_epsilon(double *arr, size_t len, double min, double max, double epsilon);
int16_t forward_quantize(float x, int32_t scale);
int16_t forward_quantize_f64(double x, int32_t scale);
void scale_f64_array(double *arr, size_t len, double amount);

double **matrix_f64_new(size_t row_count, size_t col_count);
double **matrix_f64_new_clone(double** source, size_t row_count, size_t col_count);
void matrix_f64_copy(double **dest, double** source, size_t row_count, size_t col_count);
void matrix_f64_free(double **mat, size_t row_count);

#endif