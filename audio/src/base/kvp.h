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

/**
 * Key value pair of type <int, int>.
*/
struct KeyValueInt {
    /**
     * Key
     */    
    int key;

    /**
     * Value.
    */
    int value;
};

struct KeyValue *KeyValue_new(void);
struct KeyValue *KeyValue_new_value(char *value);
void KeyValue_free(struct KeyValue *kvp);

struct KeyValuePointer *KeyValuePointer_new(void);
struct KeyValuePointer *KeyValuePointer_new_value(void *value);
void KeyValuePointer_free(struct KeyValuePointer *kvp);

struct KeyValueInt *KeyValueInt_new(void);
struct KeyValueInt *KeyValueInt_new_value(int value);
void KeyValueInt_free(struct KeyValueInt *kvp);

#endif