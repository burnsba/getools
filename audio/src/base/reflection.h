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
#ifndef _GAUDIO_REFLECTION_H_
#define _GAUDIO_REFLECTION_H_

/**
 * Local struct to desribe properties or "classes".
 * Set at compile time.
*/
struct TypeInfo {
    /**
     * Unique identifier corresponding to property within "class".
    */
    int key;

    /**
     * Property name.
    */
    char *value;

    /**
     * Property type (another enum, corresponds to "integer" or similar).
    */
    int type_id;
};

/**
 * Container for resolved type info.
*/
struct RuntimeTypeInfo {
    /**
     * Unique identifier corresponding to property within "class".
    */
    int key;

    /**
     * Property type (another enum, corresponds to "integer" or similar).
    */
    int type_id;
};

/**
 * Container for tracking references that need to be resolved.
*/
struct MissingRef {
    /**
     * Unique id of `self`. Will be used as key into hash table.
    */
    int key;

    /**
     * Reference to object that is missing the reference.
    */
    void* self;

    /**
     * Text if that needs to be resolved.
    */
    char *ref_id;
};



/**
 * Describes a "base" type like integer.
 * This is used for the properties, not parent block type.
 * This sort of corresponds to a line in the .inst block.
*/
enum TYPE_ID {
    /**
     * Default / unset / unknown.
    */
    TYPE_ID_NONE = 0,

    /**
     * Integer type, signed/unsigned and bitsize
     * are context dependent (based on property).
    */
    TYPE_ID_INT = 1,

    /**
     * Property such as:
     *     use ("filename.aifc");
    */
    TYPE_ID_USE_STRING,

    /**
     * An unquoted string, gives an id of another block
     * within the instance file.
    */
    TYPE_ID_TEXT_REF_ID,

    /**
     * An unquoted string, gives an id of another block
     * within the instance file. Base property is an array
     * and array index is required.
    */
    TYPE_ID_ARRAY_TEXT_REF_ID,

    /**
     * Use to describe a list of comma seperated integers.
    */
    TYPE_ID_CSV_INT_LIST,
};

#endif