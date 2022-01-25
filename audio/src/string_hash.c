#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdint.h>
#include "debug.h"
#include "common.h"
#include "machine_config.h"
#include "utility.h"
#include "string_hash.h"
#include "llist.h"
#include "md5.h"

/**
 * This file contains implementation for a simple hash table designed
 * to use strings as keys. Considered a "bag" rather than dictionary
 * as duplicate keys are allowed.
*/

/**
 * Private struct.
 * Single bucket entry in the hash table bucket.
*/
struct StringHashBucketEntry {
    /**
     * Key of object stored within this bucket.
    */
    char *key;

    /**
     * Pointer to associated data.
    */
    void *data;
};

/**
 * Private struct.
 * Bucket to group hash table entries into a linked list.
*/
struct StringHashBucket {
    /**
     * Array index of this bucket in the parent list of buckets.
    */
    uint32_t table_index;

    /**
     * Linked list of bucket entries.
     * Type: `struct StringHashBucketEntry`.
    */
    struct llist_root *entry_list;
};

/**
 * Private struct.
 * Main hash table object.
*/
struct StringHashTable_internal {

    /**
     * Total number of items stored in the hash table.
    */
    uint32_t num_entries;

    /**
     * Number of buckets in the bucket list.
    */
    uint32_t bucket_count;

    /**
     * List of buckets.
    */
    struct StringHashBucket **buckets;
};

// forward declarations

static struct StringHashTable_internal *StringHashTable_internal_new();
static void StringHashTable_internal_free(struct StringHashTable_internal *root);
static struct StringHashBucket *StringHashBucket_new();
static void StringHashBucket_free(struct StringHashBucket *bucket);
static struct StringHashBucketEntry *StringHashBucketEntry_new();
static void StringHashBucketEntry_free(struct StringHashBucketEntry *entry);

static uint32_t StringHashTable_hash_key(char *key);
static uint32_t StringHashTable_bucket_index_from_hash(struct StringHashTable_internal *internal, uint32_t hash);

// end forward declarations

/**
 * Allocates memory for a new hash table.
 * @returns: pointer to new object.
*/
struct StringHashTable *StringHashTable_new()
{
    TRACE_ENTER(__func__)

    struct StringHashTable *p = (struct StringHashTable *)malloc_zero(1, sizeof(struct StringHashTable));

    p->internal = StringHashTable_internal_new();

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Frees memory allocated to hash table and all child objects.
 * Any external data pointers still stored within the hash table will
 * remain unmodified.
 * @param root: hash table to free.
*/
void StringHashTable_free(struct StringHashTable *root)
{
    TRACE_ENTER(__func__)

    if (root == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (root->internal != NULL)
    {
        StringHashTable_internal_free(root->internal);
        root->internal = NULL;
    }

    free(root);

    TRACE_LEAVE(__func__)
}

/**
 * Adds a new object to the hash table. Objects with duplicate keys are
 * appended at the end of the bucket; retrieving the duplicate key returns the first
 * object (FIFO / queue).
 * @param root: hash table.
 * @param key: lookup key.
 * @param data: pointer to data.
*/
void StringHashTable_add(struct StringHashTable *root, char *key, void *data)
{
    TRACE_ENTER(__func__)

    struct StringHashTable_internal *ht;
    struct StringHashBucket *bucket;
    struct StringHashBucketEntry *entry;
    struct llist_node *node;
    uint32_t bucket_index;
    uint32_t hash;

    if (root == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: hash table is NULL\n", __func__);
    }

    if (key == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: key is NULL\n", __func__);
    }

    ht = (struct StringHashTable_internal *)root->internal;

    if (ht == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: hash table invalid internal state\n", __func__);
    }

    hash = StringHashTable_hash_key(key);
    bucket_index = StringHashTable_bucket_index_from_hash(ht, hash);
    bucket = ht->buckets[bucket_index];

    if (bucket == NULL)
    {
        bucket = StringHashBucket_new();
        ht->buckets[bucket_index] = bucket;
    }

    entry = StringHashBucketEntry_new(key);
    entry->data = data;

    node = llist_node_new();
    node->data = entry;

    llist_root_append_node(bucket->entry_list, node);

    ht->num_entries++;

    TRACE_LEAVE(__func__)
}

/**
 * Returns a value indicating whether the associated key can be found in the hash table.
 * @param root: hash table.
 * @param key: lookup key.
 * @returns: 1 if key is found, 0 otherwise.
*/
int StringHashTable_contains(struct StringHashTable *root, char *key)
{
    TRACE_ENTER(__func__)

    struct StringHashTable_internal *ht;
    struct StringHashBucket *bucket;
    struct StringHashBucketEntry *entry;
    struct llist_node *node;
    uint32_t bucket_index;
    uint32_t hash;

    if (root == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: hash table is NULL\n", __func__);
    }

    if (key == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: key is NULL\n", __func__);
    }
    
    ht = (struct StringHashTable_internal *)root->internal;

    if (ht == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: hash table invalid internal state\n", __func__);
    }

    // if hashtable is empty, there's nothing to check
    if (ht->num_entries == 0)
    {
        TRACE_LEAVE(__func__)
        return 0;
    }

    hash = StringHashTable_hash_key(key);
    bucket_index = StringHashTable_bucket_index_from_hash(ht, hash);
    bucket = ht->buckets[bucket_index];

    // if bucket has never been setup, then it's not in the bucket
    if (bucket == NULL)
    {
        TRACE_LEAVE(__func__)
        return 0;
    }

    node = bucket->entry_list->root;
    while (node != NULL)
    {
        entry = (struct StringHashBucketEntry *)node->data;

        if (entry == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: hash table invalid internal state, bucket entry is NULL, key=%s\n", __func__, key);
        }

        if (strcmp(key, entry->key) == 0)
        {
            TRACE_LEAVE(__func__)
            return 1;
        }

        node = node->next;
    }

    TRACE_LEAVE(__func__)

    return 0;
}

/**
 * Returns total number of items stored in the hash table.
 * @param root: hash table.
 * @returns: total number of items.
*/
uint32_t StringHashTable_count(struct StringHashTable *root)
{
    TRACE_ENTER(__func__)

    struct StringHashTable_internal *ht;

    if (root == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: hash table is NULL\n", __func__);
    }

    ht = (struct StringHashTable_internal *)root->internal;

    if (ht == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: hash table invalid internal state\n", __func__);
    }

    TRACE_LEAVE(__func__)

    return ht->num_entries;
}

/**
 * Removes an item from the hash table and returns the associated data pointer.
 * Objects with duplicate keys are appended at the end of the bucket;
 * retrieving the duplicate key returns the first object (FIFO / queue).
 * Attempting to remove an item that doesn't exist is a fatal error.
 * @param root: hash table.
 * @param key: lookup key.
 * @returns: pointer to data.
*/
void *StringHashTable_pop(struct StringHashTable *root, char *key)
{
    TRACE_ENTER(__func__)

    struct StringHashTable_internal *ht;
    struct StringHashBucket *bucket;
    struct StringHashBucketEntry *entry;
    struct llist_node *node;
    uint32_t bucket_index;
    uint32_t hash;
    void *result;

    if (root == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: hash table is NULL\n", __func__);
    }

    if (key == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: key is NULL\n", __func__);
    }
    
    ht = (struct StringHashTable_internal *)root->internal;

    if (ht == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: hash table invalid internal state\n", __func__);
    }

    // if hashtable is empty, exit
    if (ht->num_entries == 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s: hash table is empty, key=%s\n", __func__, key);
    }

    hash = StringHashTable_hash_key(key);
    bucket_index = StringHashTable_bucket_index_from_hash(ht, hash);
    bucket = ht->buckets[bucket_index];

    // if bucket has never been setup, exit
    if (bucket == NULL)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s: hash table bucket is empty, key=%s\n", __func__, key);
    }

    node = bucket->entry_list->root;
    while (node != NULL)
    {
        entry = (struct StringHashBucketEntry *)node->data;

        if (entry == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: hash table invalid internal state, bucket entry is NULL, key=%s\n", __func__, key);
        }

        if (strcmp(key, entry->key) == 0)
        {
            result = entry->data;

            StringHashBucketEntry_free(entry);
            llist_node_free(bucket->entry_list, node);

            ht->num_entries--;

            TRACE_LEAVE(__func__)
            return result;
        }

        node = node->next;
    }

    TRACE_LEAVE(__func__)

    stderr_exit(EXIT_CODE_GENERAL, "%s: key not found: %s\n", __func__, key);

    // be quiet gcc
    return 0;
}

/**
 * Allocates memory for a new hash table internal.
 * @returns: pointer to new object.
*/
static struct StringHashTable_internal *StringHashTable_internal_new()
{
    TRACE_ENTER(__func__)

    struct StringHashTable_internal *p = (struct StringHashTable_internal *)malloc_zero(1, sizeof(struct StringHashTable_internal));

    p->bucket_count = STRING_HASH_TABLE_DEFAULT_BUCKET_COUNT;

    p->buckets = (struct StringHashBucket **)malloc_zero(p->bucket_count, sizeof(struct StringHashBucket *));

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Frees memory from the internal hash table and all related child
 * objects.
 * @param root: hash table internal to free.
*/
static void StringHashTable_internal_free(struct StringHashTable_internal *root)
{
    TRACE_ENTER(__func__)

    uint32_t i;

    if (root == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (root->buckets != NULL)
    {
        for (i=0; i<root->bucket_count; i++)
        {
            struct StringHashBucket *bucket = root->buckets[i];
            if (bucket != NULL)
            {
                StringHashBucket_free(bucket);
                root->buckets[i] = NULL;
            }
        }

        free(root->buckets);
        root->buckets = NULL;
    }

    free(root);

    TRACE_LEAVE(__func__)
}

/**
 * Allocates memory for a new bucket.
 * @returns: pointer to new object.
*/
static struct StringHashBucket *StringHashBucket_new()
{
    TRACE_ENTER(__func__)

    struct StringHashBucket *p = (struct StringHashBucket *)malloc_zero(1, sizeof(struct StringHashBucket));

    p->entry_list = llist_root_new();

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Frees memory associated with a bucket and all child entries.
 * @param bucket: object to free.
*/
static void StringHashBucket_free(struct StringHashBucket *bucket)
{
    TRACE_ENTER(__func__)

    if (bucket == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (bucket->entry_list != NULL)
    {
        struct StringHashBucketEntry *data;
        struct llist_node *node;

        node = bucket->entry_list->root;

        while (node != NULL)
        {
            data = (struct StringHashBucketEntry *)node->data;
            if (data != NULL)
            {
                StringHashBucketEntry_free(data);
                node->data = NULL;
            }

            node = node->next;
        }

        llist_node_root_free(bucket->entry_list);
        bucket->entry_list = NULL;
    }

    free(bucket);

    TRACE_LEAVE(__func__)
}

/**
 * Allocates memory for a new bucket entry.
 * @returns: pointer to new object.
*/
static struct StringHashBucketEntry *StringHashBucketEntry_new(char *key)
{
    TRACE_ENTER(__func__)

    if (key == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: key is NULL\n", __func__);
    }

    size_t len;

    struct StringHashBucketEntry *p = (struct StringHashBucketEntry*)malloc_zero(1, sizeof(struct StringHashBucketEntry));

    // Allocate bytes for string, add one more byte for '\0'
    len = strlen(key);
    p->key = (char *)malloc_zero(len + 1, 1);

    // copy key value. Terminating zero was set in malloc_zero call above.
    memcpy(p->key, key, len);

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Frees memory associated with an entry.
 * @param bucket: object to free.
*/
static void StringHashBucketEntry_free(struct StringHashBucketEntry *entry)
{
    TRACE_ENTER(__func__)

    if (entry == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (entry->key != NULL)
    {
        free(entry->key);
        entry->key = NULL;
    }

    free(entry);

    TRACE_LEAVE(__func__)
}

/**
 * The hashing algorithm.
 * @param key: string to hash.
 * @returns: hash result.
*/
static uint32_t StringHashTable_hash_key(char *key)
{
    TRACE_ENTER(__func__)

    char digest[16];
    uint32_t hash;
    md5_hash(key, strlen(key), digest);
    // probably endianess
    memcpy(&hash, digest, 4);

    TRACE_LEAVE(__func__)

    return hash;
}

/**
 * Gets bucket index from a given hash.
 * @param internal: hash table
 * @param hash: hash to resolve to bucket
 * @returns: zero based index into bucket list.
*/
static uint32_t StringHashTable_bucket_index_from_hash(struct StringHashTable_internal *internal, uint32_t hash)
{
    TRACE_ENTER(__func__)

    TRACE_LEAVE(__func__)

    return hash % internal->bucket_count;
}