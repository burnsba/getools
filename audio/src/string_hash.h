#ifndef _GAUDIO_STRING_HASH_H
#define _GAUDIO_STRING_HASH_H

/**
 * Number of buckets to use.
*/
#define STRING_HASH_TABLE_DEFAULT_BUCKET_COUNT 10

/**
 * Hash table container.
*/
struct StringHashTable {
    /**
     * Internal state. Don't touch.
    */
    void *internal;
};

struct StringHashTable *StringHashTable_new();
void StringHashTable_free(struct StringHashTable *root);

void StringHashTable_add(struct StringHashTable *root, char *key, void *data);
int StringHashTable_contains(struct StringHashTable *root, char *key);
uint32_t StringHashTable_count(struct StringHashTable *root);
void *StringHashTable_pop(struct StringHashTable *root, char *key);

#endif