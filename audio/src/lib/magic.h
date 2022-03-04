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
#ifndef _GAUDIO_MAGIC_H_
#define _GAUDIO_MAGIC_H_

#include <float.h>

/**
 * Max number of feedback states used for prediction.
 * 
 * This is somewhat arbitrary, but above 8 each audio frame will
 * have more feedback state than quantized data.
*/
#define TABLE_MAX_ORDER 8
#define TABLE_MIN_ORDER 1

/**
 * Max number of predictors to generate for a codebook.
*/
#define TABLE_MAX_PREDICTORS 8
#define TABLE_MIN_PREDICTORS 1

/**
 * Default number of lag samples to consider when no user option specified.
 * (n'th order predictor)
*/
#define TABLE_DEFAULT_LAG 2

/**
 * Default number of predictors generate when no user option specified.
*/
#define TABLE_DEFAULT_PREDICTORS 1

/**
 * Minimum norm threshold for an audio frame to be
 * considered. This is a "silence" cutoff, separate from user
 * specified parameters.
*/
#define TABLE_SILENCE_THRESHOLD 10.0

/**
 * Default extension when writing codebook data.
*/
#define TABLE_DEFAULT_EXTENSION ".coef"

#define CODEBOOK_DEFAULT_USER_THRESHOLD_ABSOLUTE_MIN 0.0
#define CODEBOOK_DEFAULT_USER_THRESHOLD_ABSOLUTE_MAX DBL_MAX
#define CODEBOOK_DEFAULT_USER_THRESHOLD_QUANTILE_MIN 0.0
#define CODEBOOK_DEFAULT_USER_THRESHOLD_QUANTILE_MAX 1.0

enum CODEBOOK_THRESHOLD_MODE {
    THRESHOLD_MODE_DEFAULT_UNKOWN = 0,
    THRESHOLD_MODE_ABSOLUTE,
    THRESHOLD_MODE_QUANTILE,
};

struct codebook_threshold_parameters {
    enum CODEBOOK_THRESHOLD_MODE mode;
    double min;
    double max;
};

struct ALADPCMBook *estimate_codebook(
    uint8_t *buffer,
    size_t buffer_len,
    enum DATA_ENCODING buffer_encoding,
    struct codebook_threshold_parameters *threshold,
    int order,
    int npredictors);


// declarations made public for testing

double autocorrelation_vector(double *previous, double *current, size_t len, int lag, double *result);
void autocorrelation_matrix(double *previous, double *current, size_t len, int lag, double **result);
int lu_decomp_solve(double **a, double *b, size_t n, double *x);
int stable_kfroma(double *parameters, size_t lag, double *reflection_coefficients);
void afromk(double *reflection_coefficients, size_t lag, double *ar_parameters);
void rfroma(double *ar_parameters, size_t lag, double *acf_r);
void levinson_durbin_recursion(double *acf_r, size_t lag, double *reflection_coefficients);
double *codebook_row_from_predictors(double *reflection_coefficients, size_t lag);
void ALADPCMBook_set_predictor(struct ALADPCMBook *book, double *row, int predictor_num);

#endif