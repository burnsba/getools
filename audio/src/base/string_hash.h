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
#ifndef _GAUDIO_STRING_HASH_H_
#define _GAUDIO_STRING_HASH_H_

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

/**
 * Foreach method will iterate every item in the hashtable, this
 * is the definition of the callback method.
*/
typedef void (*StringHash_callback)(void *data);

/**
 * Foreach method will iterate every item in the hashtable, this
 * is the definition of the test method. If the test method
 * returns a positive value iteration stops and that value is
 * returned. Otherwise zero is returned.
*/
typedef int (*StringHash_bool_callback)(void *data);

struct StringHashTable *StringHashTable_new(void);
void StringHashTable_free(struct StringHashTable *root);

void StringHashTable_add(struct StringHashTable *root, char *key, void *data);
int StringHashTable_contains(struct StringHashTable *root, char *key);
uint32_t StringHashTable_count(struct StringHashTable *root);
void *StringHashTable_pop(struct StringHashTable *root, char *key);
void *StringHashTable_get(struct StringHashTable *root, char *key);

void StringHashTable_foreach(struct StringHashTable *root, StringHash_callback action);
int StringHashTable_any(struct StringHashTable *root, StringHash_bool_callback action, void **first);

const char *StringHashTable_peek_next_key(struct StringHashTable *root);

#endif