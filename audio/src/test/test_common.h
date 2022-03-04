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
#ifndef _GAUDIO_TEST_COMMON_H_
#define _GAUDIO_TEST_COMMON_H_

#include "llist.h"

struct TestKeyValue {
    int key;
    void *data;
};

extern int g_test_hashtable_kvpint_compare;

// helper and general test functions

int hashtable_kvpint_callback(void* data);
void hashtable_kvpint_callback_free(void* data);

struct TestKeyValue *TestKeyValue_new(void);
void TestKeyValue_free(struct TestKeyValue *tkvp);
int LinkedListNode_TestKeyValue_compare_smaller_key(struct LinkedListNode *first, struct LinkedListNode *second);

int f64_equal(double d1, double d2, double epsilon);
void print_expected_vs_actual_arr(uint8_t *expected, size_t expected_len, uint8_t *actual, size_t actual_len);

void parse_seq_bytes_to_event_list(uint8_t *data, size_t buffer_len, struct LinkedList *event_list);

// top level test entry points.

void test_md5_all(int *run_count, int *pass_count, int *fail_count);
void linked_list_all(int *run_count, int *pass_count, int *fail_count);
void int_hash_all(int *run_count, int *pass_count, int *fail_count);
void string_hash_all(int *run_count, int *pass_count, int *fail_count);
void parse_inst_all(int *run_count, int *pass_count, int *fail_count);
void parse_coef_all(int *run_count, int *pass_count, int *fail_count);
void aifc_all(int *run_count, int *pass_count, int *fail_count);
void magic_all(int *run_count, int *pass_count, int *fail_count);
void midi_all(int *run_count, int *pass_count, int *fail_count);

// child test entry points

void midi_convert_all(int *run_count, int *pass_count, int *fail_count);
void test_midi_convert(int *run_count, int *pass_count, int *fail_count);


#endif