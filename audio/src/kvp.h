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

struct KeyValue *KeyValue_new();
struct KeyValue *KeyValue_new_value(char *value);
void KeyValue_free(struct KeyValue *kvp);

#endif