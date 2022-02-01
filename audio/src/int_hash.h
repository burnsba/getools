#ifndef _GAUDIO_INT_HASH_H_
#define _GAUDIO_INT_HASH_H_

#include <stdint.h>

/**
 * Number of buckets to use.
*/
#define INT_HASH_TABLE_DEFAULT_BUCKET_COUNT 10

/**
 * Hash table container.
*/
struct IntHashTable {
    /**
     * Internal state. Don't touch.
    */
    void *internal;
};

struct IntHashTable *IntHashTable_new();
void IntHashTable_free(struct IntHashTable *root);

void IntHashTable_add(struct IntHashTable *root, uint32_t key, void *data);
int IntHashTable_contains(struct IntHashTable *root, uint32_t key);
uint32_t IntHashTable_count(struct IntHashTable *root);
void *IntHashTable_pop(struct IntHashTable *root, uint32_t key);
void *IntHashTable_get(struct IntHashTable *root, uint32_t key);

int IntHashTable_peek_next_key(struct IntHashTable *root, uint32_t *key);

#endif