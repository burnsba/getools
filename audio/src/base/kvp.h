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