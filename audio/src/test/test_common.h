#ifndef _GAUDIO_TEST_COMMON_H_
#define _GAUDIO_TEST_COMMON_H_

#include "llist.h"

struct TestKeyValue {
    int key;
    void *data;
};

extern int g_test_hashtable_kvpint_compare;

int hashtable_kvpint_callback(void* data);
void hashtable_kvpint_callback_free(void* data);

struct TestKeyValue *TestKeyValue_new(void);
void TestKeyValue_free(struct TestKeyValue *tkvp);
int llist_node_TestKeyValue_compare_smaller_key(struct llist_node *first, struct llist_node *second);

void test_md5_all(int *run_count, int *pass_count, int *fail_count);
void linked_list_all(int *run_count, int *pass_count, int *fail_count);
void int_hash_all(int *run_count, int *pass_count, int *fail_count);
void string_hash_all(int *run_count, int *pass_count, int *fail_count);
void parse_inst_all(int *run_count, int *pass_count, int *fail_count);
void aifc_all(int *run_count, int *pass_count, int *fail_count);

#endif