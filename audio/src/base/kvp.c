#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#include "debug.h"
#include "common.h"
#include "machine_config.h"
#include "utility.h"
#include "kvp.h"

/**
 * This file is the implementation of a simple key value pair
 * of type <int, string>.
*/

/**
 * Allocates memory for a new {@code struct KeyValue}.
 * No memory is allocated for the value.
 * @returns: pointer to new object.
*/
struct KeyValue *KeyValue_new()
{
    TRACE_ENTER(__func__)

    struct KeyValue *p = (struct KeyValue *)malloc_zero(1, sizeof(struct KeyValue));

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Allocates memory for a new {@code struct KeyValue} and value and
 * copies string into value.
 * @param value: string to copy.
 * @returns: pointer to new object.
*/
struct KeyValue *KeyValue_new_value(char *value)
{
    TRACE_ENTER(__func__)

    size_t len = strlen(value);

    struct KeyValue *p = (struct KeyValue *)malloc_zero(1, sizeof(struct KeyValue));

    if (len > 0)
    {
        p->value = (char *)malloc_zero(1, len + 1);
        strncpy(p->value, value, len);
    }

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Frees memory associated with {@code struct KeyValue}.
 * If value is non-NULL, that is freed too.
 * @param kvp: object to free.
*/
void KeyValue_free(struct KeyValue *kvp)
{
    TRACE_ENTER(__func__)

    if (kvp == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (kvp->value != NULL)
    {
        free(kvp->value);
        kvp->value = NULL;
    }

    free(kvp);

    TRACE_LEAVE(__func__)
}

/**
 * Allocates memory for a new {@code struct KeyValuePointer}.
 * No memory is allocated for the value.
 * @returns: pointer to new object.
*/
struct KeyValuePointer *KeyValuePointer_new()
{
    TRACE_ENTER(__func__)

    struct KeyValuePointer *p = (struct KeyValuePointer *)malloc_zero(1, sizeof(struct KeyValuePointer));

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Allocates memory for a new {@code struct KeyValuePointer} and
 * copies pointer into value.
 * @param value: pointer to copy.
 * @returns: pointer to new object.
*/
struct KeyValuePointer *KeyValuePointer_new_value(void *value)
{
    TRACE_ENTER(__func__)

    struct KeyValuePointer *p = (struct KeyValuePointer *)malloc_zero(1, sizeof(struct KeyValuePointer));
    p->value = value;

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Frees memory associated with {@code struct KeyValuePointer}.
 * The `value` is not freed.
 * @param kvp: object to free.
*/
void KeyValuePointer_free(struct KeyValuePointer *kvp)
{
    TRACE_ENTER(__func__)

    if (kvp == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    free(kvp);

    TRACE_LEAVE(__func__)
}

/**
 * Allocates memory for a new {@code struct KeyValueInt}.
 * @returns: pointer to new object.
*/
struct KeyValueInt *KeyValueInt_new()
{
    TRACE_ENTER(__func__)

    struct KeyValueInt *p = (struct KeyValueInt *)malloc_zero(1, sizeof(struct KeyValueInt));

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Allocates memory for a new {@code struct KeyValueInt} and copies value.
 * @param value: value.
 * @returns: pointer to new object.
*/
struct KeyValueInt *KeyValueInt_new_value(int value)
{
    TRACE_ENTER(__func__)

    struct KeyValueInt *p = (struct KeyValueInt *)malloc_zero(1, sizeof(struct KeyValueInt));
    p->value = value;

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Frees memory associated with {@code struct KeyValueInt}.
 * @param kvp: object to free.
*/
void KeyValueInt_free(struct KeyValueInt *kvp)
{
    TRACE_ENTER(__func__)

    if (kvp == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    free(kvp);

    TRACE_LEAVE(__func__)
}