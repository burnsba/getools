#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdint.h>
#include "debug.h"
#include "common.h"
#include "machine_config.h"
#include "utility.h"
#include "int_hash.h"
#include "llist.h"
#include "md5.h"

/**
 * This file contains implementation for a simple hash table designed
 * to use (32-bit) int as keys. Considered a "bag" rather than dictionary
 * as duplicate keys are allowed.
*/

/**
 * Private struct.
 * Single bucket entry in the hash table bucket.
*/
struct IntHashBucketEntry {
    /**
     * Key of object stored within this bucket.
    */
    uint32_t key;

    /**
     * Pointer to associated data.
    */
    void *data;
};

/**
 * Private struct.
 * Bucket to group hash table entries into a linked list.
*/
struct IntHashBucket {
    /**
     * Array index of this bucket in the parent list of buckets.
    */
    uint32_t table_index;

    /**
     * Linked list of bucket entries.
     * Type: `struct IntHashBucketEntry`.
    */
    struct LinkedList *entry_list;
};

/**
 * Private struct.
 * Main hash table object.
*/
struct IntHashTable_internal {

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
    struct IntHashBucket **buckets;
};

// forward declarations

static void *IntHashTable_pop_common(struct IntHashTable *root, uint32_t key, int pop);
static struct IntHashTable_internal *IntHashTable_internal_new(void);
static void IntHashTable_internal_free(struct IntHashTable_internal *root);
static struct IntHashBucket *IntHashBucket_new(void);
static void IntHashBucket_free(struct IntHashBucket *bucket);
static struct IntHashBucketEntry *IntHashBucketEntry_new(uint32_t key);
static void IntHashBucketEntry_free(struct IntHashBucketEntry *entry);

static uint32_t IntHashTable_hash_key(uint32_t key);
static uint32_t IntHashTable_bucket_index_from_hash(struct IntHashTable_internal *internal, uint32_t hash);

// end forward declarations

/**
 * Allocates memory for a new hash table.
 * @returns: pointer to new object.
*/
struct IntHashTable *IntHashTable_new()
{
    TRACE_ENTER(__func__)

    struct IntHashTable *p = (struct IntHashTable *)malloc_zero(1, sizeof(struct IntHashTable));

    p->internal = IntHashTable_internal_new();

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Frees memory allocated to hash table and all child objects.
 * Any external data pointers still stored within the hash table will
 * remain unmodified.
 * @param root: hash table to free.
*/
void IntHashTable_free(struct IntHashTable *root)
{
    TRACE_ENTER(__func__)

    if (root == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (root->internal != NULL)
    {
        IntHashTable_internal_free(root->internal);
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
void IntHashTable_add(struct IntHashTable *root, uint32_t key, void *data)
{
    TRACE_ENTER(__func__)

    struct IntHashTable_internal *ht;
    struct IntHashBucket *bucket;
    struct IntHashBucketEntry *entry;
    struct LinkedListNode *node;
    uint32_t bucket_index;
    uint32_t hash;

    if (root == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: hash table is NULL\n", __func__, __LINE__);
    }

    ht = (struct IntHashTable_internal *)root->internal;

    if (ht == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: hash table invalid internal state\n", __func__, __LINE__);
    }

    hash = IntHashTable_hash_key(key);
    bucket_index = IntHashTable_bucket_index_from_hash(ht, hash);
    bucket = ht->buckets[bucket_index];

    if (bucket == NULL)
    {
        bucket = IntHashBucket_new();
        ht->buckets[bucket_index] = bucket;
    }

    entry = IntHashBucketEntry_new(key);
    entry->data = data;

    node = LinkedListNode_new();
    node->data = entry;

    LinkedList_append_node(bucket->entry_list, node);

    ht->num_entries++;

    TRACE_LEAVE(__func__)
}

/**
 * Returns a value indicating whether the associated key can be found in the hash table.
 * @param root: hash table.
 * @param key: lookup key.
 * @returns: 1 if key is found, 0 otherwise.
*/
int IntHashTable_contains(struct IntHashTable *root, uint32_t key)
{
    TRACE_ENTER(__func__)

    struct IntHashTable_internal *ht;
    struct IntHashBucket *bucket;
    struct IntHashBucketEntry *entry;
    struct LinkedListNode *node;
    uint32_t bucket_index;
    uint32_t hash;

    if (root == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: hash table is NULL\n", __func__, __LINE__);
    }
    
    ht = (struct IntHashTable_internal *)root->internal;

    if (ht == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: hash table invalid internal state\n", __func__, __LINE__);
    }

    // if hashtable is empty, there's nothing to check
    if (ht->num_entries == 0)
    {
        TRACE_LEAVE(__func__)
        return 0;
    }

    hash = IntHashTable_hash_key(key);
    bucket_index = IntHashTable_bucket_index_from_hash(ht, hash);
    bucket = ht->buckets[bucket_index];

    // if bucket has never been setup, then it's not in the bucket
    if (bucket == NULL)
    {
        TRACE_LEAVE(__func__)
        return 0;
    }

    node = bucket->entry_list->head;
    while (node != NULL)
    {
        entry = (struct IntHashBucketEntry *)node->data;

        if (entry == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: hash table invalid internal state, bucket entry is NULL, key=%s\n", __func__, __LINE__, key);
        }

        if (key == entry->key)
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
uint32_t IntHashTable_count(struct IntHashTable *root)
{
    TRACE_ENTER(__func__)

    struct IntHashTable_internal *ht;

    if (root == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: hash table is NULL\n", __func__, __LINE__);
    }

    ht = (struct IntHashTable_internal *)root->internal;

    if (ht == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: hash table invalid internal state\n", __func__, __LINE__);
    }

    TRACE_LEAVE(__func__)

    return ht->num_entries;
}

/**
 * Removes an item from the hash table and returns the associated data pointer.
 * (Item cannot be retrieved again.)
 * Objects with duplicate keys are appended at the end of the bucket;
 * retrieving the duplicate key returns the first object (FIFO / queue).
 * Attempting to remove an item that doesn't exist is a fatal error.
 * @param root: hash table.
 * @param key: lookup key.
 * @returns: pointer to data.
*/
void *IntHashTable_pop(struct IntHashTable *root, uint32_t key)
{
    TRACE_ENTER(__func__)

    TRACE_LEAVE(__func__)

    return IntHashTable_pop_common(root, key, 1);
}

/**
 * Retrieves an item from the hash table and returns the associated data pointer.
 * (Item can be retrieved again.)
 * Objects with duplicate keys are appended at the end of the bucket;
 * retrieving the duplicate key returns the first object (FIFO / queue).
 * Attempting to remove an item that doesn't exist is a fatal error.
 * @param root: hash table.
 * @param key: lookup key.
 * @returns: pointer to data.
*/
void *IntHashTable_get(struct IntHashTable *root, uint32_t key)
{
    TRACE_ENTER(__func__)

    TRACE_LEAVE(__func__)

    return IntHashTable_pop_common(root, key, 0);
}

/**
 * Iterates hash table and returns key of first entry found.
 * No memory is allocated.
 * @param root: hash table to iterate.
 * @param key: out parameter. First found key, or NULL.
 * @returns: 1 if key found, zero otherwise.
*/
int IntHashTable_peek_next_key(struct IntHashTable *root, uint32_t *key)
{
    TRACE_ENTER(__func__)

    struct IntHashTable_internal *ht;
    struct IntHashBucket *bucket;
    struct IntHashBucketEntry *entry;
    struct LinkedListNode *node;
    uint32_t i;

    *key = 0;

    if (root == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: hash table is NULL\n", __func__, __LINE__);
    }

    ht = (struct IntHashTable_internal *)root->internal;

    if (ht == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: hash table invalid internal state\n", __func__, __LINE__);
    }

    // if hashtable is empty, exit
    if (ht->num_entries == 0)
    {
        TRACE_LEAVE(__func__)
        return 0;
    }

    for (i=0; i<ht->bucket_count; i++)
    {
        bucket = ht->buckets[i];

        if (bucket != NULL)
        {
            node = bucket->entry_list->head;
            if (node != NULL)
            {
                entry = (struct IntHashBucketEntry *)node->data;

                if (entry == NULL)
                {
                    stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: hash table invalid internal state, bucket entry is NULL\n", __func__, __LINE__);
                }

                *key = entry->key;
                
                TRACE_LEAVE(__func__)
                return 1;
            }
        }
    }

    TRACE_LEAVE(__func__)

    return 0;
}

/**
 * Iterate every item in the hash table and perform an action on it.
 * @param root: hash table.
 * @param action: action to perform.
*/
void IntHashTable_foreach(struct IntHashTable *root, IntHash_callback action)
{
    TRACE_ENTER(__func__)

    struct IntHashTable_internal *ht;
    struct IntHashBucket *bucket;
    struct IntHashBucketEntry *entry;
    struct LinkedList *list;
    struct LinkedListNode *node;
    uint32_t i;

    if (root == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: hash table is NULL\n", __func__, __LINE__);
    }

    ht = (struct IntHashTable_internal *)root->internal;

    if (ht == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: hash table invalid internal state\n", __func__, __LINE__);
    }

    for (i=0; i<ht->bucket_count; i++)
    {
        bucket = ht->buckets[i];

        if (bucket != NULL)
        {
            list = bucket->entry_list;
            if (list != NULL)
            {
                node = list->head;
                while (node != NULL)
                {
                    entry = node->data;

                    if (entry != NULL)
                    {
                        action(entry->data);
                    }

                    node = node->next;
                }
            }
        }
    }

    TRACE_LEAVE(__func__)
}

/**
 * Iterate every item in the hash table and test it.
 * @param root: hash table.
 * @param action: action to perform. Should return 0 for false, and any
 * positive value for true.
 * @param first: Out parameter. Optional. If the action ever returns a truthy value, this will
 * point to the data pointer of that entry. Otherewise NULL.
 * @returns: If callback ever returns a positive value, that value is
 * returned and iteration stops, otherwise zero.
*/
int IntHashTable_any(struct IntHashTable *root, IntHash_bool_callback action, void **first)
{
    TRACE_ENTER(__func__)

    struct IntHashTable_internal *ht;
    struct IntHashBucket *bucket;
    struct IntHashBucketEntry *entry;
    struct LinkedListNode *node;
    struct LinkedList *list;
    uint32_t i;

    if (first != NULL)
    {
        *first = NULL;
    }

    if (root == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: hash table is NULL\n", __func__, __LINE__);
    }

    ht = (struct IntHashTable_internal *)root->internal;

    if (ht == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: hash table invalid internal state\n", __func__, __LINE__);
    }

    for (i=0; i<ht->bucket_count; i++)
    {
        bucket = ht->buckets[i];

        if (bucket != NULL)
        {
            list = bucket->entry_list;
            if (list != NULL)
            {
                node = list->head;
                while (node != NULL)
                {
                    entry = node->data;

                    if (entry != NULL)
                    {
                        int result = action(entry->data);

                        if (result)
                        {
                            if (first != NULL)
                            {
                                *first = entry->data;
                            }

                            TRACE_LEAVE(__func__)
                            return result;
                        }
                    }

                    node = node->next;
                }
            }
        }
    }

    TRACE_LEAVE(__func__)

    return 0;
}

/**
 * Shared code for _get and _pop.
 * @param root: hash table.
 * @param key: lookup key.
 * @param pop: flag to indicate whether item should be removed from the hash table or not.
 * @returns: pointer to data.
*/
static void *IntHashTable_pop_common(struct IntHashTable *root, uint32_t key, int pop)
{
    TRACE_ENTER(__func__)

    struct IntHashTable_internal *ht;
    struct IntHashBucket *bucket;
    struct IntHashBucketEntry *entry;
    struct LinkedListNode *node;
    uint32_t bucket_index;
    uint32_t hash;
    void *result;

    if (root == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> (flag=%s): hash table is NULL\n", __func__, __LINE__, pop?"pop":"get");
    }

    ht = (struct IntHashTable_internal *)root->internal;

    if (ht == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> (flag=%s): hash table invalid internal state\n", __func__, __LINE__, pop?"pop":"get");
    }

    // if hashtable is empty, exit
    if (ht->num_entries == 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> (flag=%s): hash table is empty, key=%s\n", __func__, __LINE__, pop?"pop":"get", key);
    }

    hash = IntHashTable_hash_key(key);
    bucket_index = IntHashTable_bucket_index_from_hash(ht, hash);
    bucket = ht->buckets[bucket_index];

    // if bucket has never been setup, exit
    if (bucket == NULL)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> (flag=%s): hash table bucket is empty, key=%s\n", __func__, __LINE__, pop?"pop":"get", key);
    }

    node = bucket->entry_list->head;
    while (node != NULL)
    {
        entry = (struct IntHashBucketEntry *)node->data;

        if (entry == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> (flag=%s): hash table invalid internal state, bucket entry is NULL, key=%s\n", __func__, __LINE__, pop?"pop":"get", key);
        }

        if (key == entry->key)
        {
            result = entry->data;

            if (pop)
            {
                IntHashBucketEntry_free(entry);
                LinkedListNode_free(bucket->entry_list, node);

                ht->num_entries--;
            }

            TRACE_LEAVE(__func__)
            return result;
        }

        node = node->next;
    }

    TRACE_LEAVE(__func__)

    stderr_exit(EXIT_CODE_GENERAL, "%s %d> (flag=%s): key not found: %s\n", __func__, __LINE__, pop?"pop":"get", key);

    // be quiet gcc
    return 0;
}

/**
 * Allocates memory for a new hash table internal.
 * @returns: pointer to new object.
*/
static struct IntHashTable_internal *IntHashTable_internal_new()
{
    TRACE_ENTER(__func__)

    struct IntHashTable_internal *p = (struct IntHashTable_internal *)malloc_zero(1, sizeof(struct IntHashTable_internal));

    p->bucket_count = INT_HASH_TABLE_DEFAULT_BUCKET_COUNT;

    p->buckets = (struct IntHashBucket **)malloc_zero(p->bucket_count, sizeof(struct IntHashBucket *));

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Frees memory from the internal hash table and all related child
 * objects.
 * @param root: hash table internal to free.
*/
static void IntHashTable_internal_free(struct IntHashTable_internal *root)
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
            struct IntHashBucket *bucket = root->buckets[i];
            if (bucket != NULL)
            {
                IntHashBucket_free(bucket);
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
static struct IntHashBucket *IntHashBucket_new()
{
    TRACE_ENTER(__func__)

    struct IntHashBucket *p = (struct IntHashBucket *)malloc_zero(1, sizeof(struct IntHashBucket));

    p->entry_list = LinkedList_new();

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Frees memory associated with a bucket and all child entries.
 * @param bucket: object to free.
*/
static void IntHashBucket_free(struct IntHashBucket *bucket)
{
    TRACE_ENTER(__func__)

    if (bucket == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (bucket->entry_list != NULL)
    {
        struct IntHashBucketEntry *data;
        struct LinkedListNode *node;

        node = bucket->entry_list->head;

        while (node != NULL)
        {
            data = (struct IntHashBucketEntry *)node->data;
            if (data != NULL)
            {
                IntHashBucketEntry_free(data);
                node->data = NULL;
            }

            node = node->next;
        }

        LinkedList_free(bucket->entry_list);
        bucket->entry_list = NULL;
    }

    free(bucket);

    TRACE_LEAVE(__func__)
}

/**
 * Allocates memory for a new bucket entry.
 * @returns: pointer to new object.
*/
static struct IntHashBucketEntry *IntHashBucketEntry_new(uint32_t key)
{
    TRACE_ENTER(__func__)

    struct IntHashBucketEntry *p = (struct IntHashBucketEntry*)malloc_zero(1, sizeof(struct IntHashBucketEntry));
    p->key = key;

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Frees memory associated with an entry.
 * @param bucket: object to free.
*/
static void IntHashBucketEntry_free(struct IntHashBucketEntry *entry)
{
    TRACE_ENTER(__func__)

    if (entry == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    free(entry);

    TRACE_LEAVE(__func__)
}

/**
 * The hashing algorithm.
 * @param key: string to hash.
 * @returns: hash result.
*/
static uint32_t IntHashTable_hash_key(uint32_t key)
{
    TRACE_ENTER(__func__)

    char digest[16];
    uint32_t hash;

    char key_string[5];
    memset(key_string, 0, 5);
    memcpy(key_string, &key, 4);

    md5_hash(key_string, 4, digest);
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
static uint32_t IntHashTable_bucket_index_from_hash(struct IntHashTable_internal *internal, uint32_t hash)
{
    TRACE_ENTER(__func__)

    TRACE_LEAVE(__func__)

    return hash % internal->bucket_count;
}