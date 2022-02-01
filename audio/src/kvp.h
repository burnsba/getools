#ifndef _GAUDIO_KVP_H_
#define _GAUDIO_KVP_H_

/**
 * Key value pair of type <int, string>.
*/
struct KeyValue {
    /**
     * Key
     */    
    int key;

    /**
     * Value.
    */
    char *value;
};

/**
 * Key value pair of type <int, void*>.
*/
struct KeyValuePointer {
    /**
     * Key
     */    
    int key;

    /**
     * Value.
    */
    void *value;
};

struct KeyValue *KeyValue_new();
struct KeyValue *KeyValue_new_value(char *value);
void KeyValue_free(struct KeyValue *kvp);

struct KeyValuePointer *KeyValuePointer_new();
struct KeyValuePointer *KeyValuePointer_new_value(void *value);
void KeyValuePointer_free(struct KeyValuePointer *kvp);

#endif