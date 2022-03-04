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
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#include "debug.h"
#include "common.h"
#include "machine_config.h"
#include "utility.h"
#include "string_hash.h"
#include "int_hash.h"
#include "llist.h"
#include "reflection.h"
#include "parse.h"
#include "naudio.h"
#include "kvp.h"

/**
 * This file implements a finite state machine to parse a text .inst file
 * into an ALBankFile. There is a .graphml (and .svg) listing valid
 * state transitions.
*/

/**
 * The .inst file is split into blocks, roughly:
 * 
 *     type instance_name {
 *         property property_value;
 *     }
 * 
 * Each block begins with the type name, followed by instance name. Properties of this instance
 * are then listed between curly braces. The `type` and `property` are described by enums below.
 * 
 * A "newline character" is considered either a '\r' or '\n'.
 * 
 * The parsing algorithm is designed to ignore whitespace (including newlines) whenever possible.
 * 
 * Comments begin with `#` character. All text is then discarded until reading a newline character.
 * 
 * Blocks can be listed in any order; properties can reference a block that has not been declared yet.
 * 
 * Array items can be listed in any order; these will be sorted according to
 * array index listed in .inst file. Array indeces are required. The devkit example seems
 * to make these optional.
 * 
 * Multiple instances can reference the same child object.
*/

/**
 * Max length in bytes to accept as token length.
*/
#define IDENTIFIER_MAX_LEN 50

/**
 * Local singleton containing the parser context.
*/
struct InstParseContext {
    /**
     * Each block is parsed and then stored in the appropiate "orphan"
     * hashtable. Once the file is done parsing, references are resolved.
     * This allows the .inst file to use a reference before it has
     * been declared.
     * 
     * Some of these blocks also abuse the `struct AL-` and store a list
     * of unmet dependencies (LinkedList) on a pointer in the object.
     * This is resolved and removed once parsing is complete.
     * 
     * For objects that don't use a list (only have single child dependency),
     * the context will store a list of items to be resolved, e.g.
     * `sound_missing_envelope`.
    */

    /**
     * Most recently resolved or current type desriptor.
    */
    struct RuntimeTypeInfo *current_type;

    /**
     * Most recently resolved or current property desriptor.
    */
    struct RuntimeTypeInfo *current_property;

    /**
     * Current line from input file.
    */
    int current_line;

    /**
     * Text buffer to store name of container type.
    */
    char *type_name_buffer;

    /**
     * Current position in `type_name_buffer`.
    */
    int type_name_buffer_pos;

    /**
     * Text buffer to store instance name.
    */
    char *instance_name_buffer;

    /**
     * Current position in `instance_name_buffer`.
    */
    int instance_name_buffer_pos;

    /**
     * Text buffer to store property type name.
    */
    char *property_name_buffer;

    /**
     * Current position in `property_name_buffer`.
    */
    int property_name_buffer_pos;

    /**
     * Text buffer to store array index as it's being read.
    */
    char *array_index_value;

    /**
     * Current position in `array_index_value`.
    */
    int array_index_value_pos;

    /**
     * Text buffer to store property value as it's being read.
    */
    char *property_value_buffer;

    /**
     * Current position in `property_value_buffer`.
    */
    int property_value_buffer_pos;

    /**
     * Reference to property after it's instantiated.
     * This will be a struct ALBank, ALEnvelope, etc.
    */
    void *current_instance;

    /**
     * After array index value is finished being read,
     * it's converted to an integer and stored in this
     * property.
    */
    int array_index_int;

    /**
     * After property value is finished being read,
     * if it's an integer it will be converted
     * and stored in this property.
    */
    int current_value_int;

    /**
     * Hash table of ALBank.
    */
    struct StringHashTable *orphaned_banks;

    /**
     * Hash table of ALInstrument.
    */
    struct StringHashTable *orphaned_instruments;

    /**
     * Hash table of ALSound.
    */
    struct StringHashTable *orphaned_sounds;

    /**
     * Hash table of ALWaveTable.
     * This isn't used until parsing is finished.
    */
    struct StringHashTable *orphaned_wavetables;

    /**
     * Hash table of ALKeyMap.
    */
    struct StringHashTable *orphaned_keymaps;

    /**
     * Hash table of ALEnvelope.
    */
    struct StringHashTable *orphaned_envelopes;

    /**
     * Hash table of ALSound that need to have wavetable (aifc_path) reference resolved.
    */
    struct IntHashTable *sound_missing_wavetable;

    /**
     * Hash table of ALSound that need to have envelope reference resolved.
    */
    struct IntHashTable *sound_missing_envelope;

    /**
     * Hash table of ALSound that need to have keymap reference resolved.
    */
    struct IntHashTable *sound_missing_keymap;
};

/**
 * These are the block level types allowed within the .inst file.
*/
enum BANK_CLASS_TYPE {
    /**
     * Default / unset / unknown.
    */
    INST_TYPE_DEFAULT_UNKNOWN = 0,

    /**
     * .inst name: bank
     * class: struct ALBank
    */
    INST_TYPE_BANK = 1,

    /**
     * .inst name: instrument
     * class: struct ALInstrument
    */
    INST_TYPE_INSTRUMENT,

    /**
     * .inst name: sound
     * class: struct ALSound
    */
    INST_TYPE_SOUND,

    /**
     * .inst name: keymap
     * class: struct ALKeyMap
    */
    INST_TYPE_KEYMAP,

    /**
     * .inst name: envelope
     * class: struct ALEnvelope
    */
    INST_TYPE_ENVELOPE
};

/**
 * Properties supported under `bank`.
*/
enum InstBankPropertyId {
    /**
     * Default / unset / unknown.
    */
    INST_BANK_PROPERTY_DEFAULT_UNKNOWN = 0,

    /**
     * .inst name: instrument
     * class: struct ALBank->instruments
    */
    INST_BANK_PROPERTY_INSTRUMENT_ARR_ENTRY = 1,

    /**
     * .inst name: sample_rate
     * class: struct ALBank->sample_rate
    */
    INST_BANK_PROPERTY_SAMPLE_RATE
};

/**
 * Properties supported under `instrument`.
*/
enum InstInstrumentPropertyId {
    /**
     * Default / unset / unknown.
    */
    INST_INSTRUMENT_PROPERTY_DEFAULT_UNKNOWN = 0,

    /**
     * .inst name: volume
     * class: struct ALInstrument->volume
    */
    INST_INSTRUMENT_PROPERTY_VOLUME = 1,

    /**
     * .inst name: pan
     * class: struct ALInstrument->pan
    */
    INST_INSTRUMENT_PROPERTY_PAN,

    /**
     * .inst name: priority
     * class: struct ALInstrument->priority
    */
    INST_INSTRUMENT_PROPERTY_PRIORITY,

    /**
     * (I don't think this needs to be supported ...)
     * .inst name: flags
     * class: struct ALInstrument->flags
    */
    INST_INSTRUMENT_PROPERTY_FLAGS,

    /**
     * .inst name: tremType
     * class: struct ALInstrument->trem_type
    */
    INST_INSTRUMENT_PROPERTY_TREM_TYPE,

    /**
     * .inst name: tremRate
     * class: struct ALInstrument->trem_rate
    */
    INST_INSTRUMENT_PROPERTY_TREM_RATE,

    /**
     * .inst name: tremDepth
     * class: struct ALInstrument->trem_depth
    */
    INST_INSTRUMENT_PROPERTY_TREM_DEPTH,

    /**
     * .inst name: tremDelay
     * class: struct ALInstrument->trem_delay
    */
    INST_INSTRUMENT_PROPERTY_TREM_DELAY,

    /**
     * .inst name: vibType
     * class: struct ALInstrument->vib_type
    */
    INST_INSTRUMENT_PROPERTY_VIB_TYPE,

    /**
     * .inst name: vibRate
     * class: struct ALInstrument->vib_rate
    */
    INST_INSTRUMENT_PROPERTY_VIB_RATE,

    /**
     * .inst name: vibDepth
     * class: struct ALInstrument->vib_depth
    */
    INST_INSTRUMENT_PROPERTY_VIB_DEPTH,

    /**
     * .inst name: vibDelay
     * class: struct ALInstrument->vib_delay
    */
    INST_INSTRUMENT_PROPERTY_VIB_DELAY,

    /**
     * .inst name: bendRange
     * class: struct ALInstrument->bend_range
    */
    INST_INSTRUMENT_PROPERTY_BENDRANGE,

    /**
     * .inst name: sound
     * class: struct ALInstrument->sounds
    */
    INST_INSTRUMENT_PROPERTY_SOUND_ARR_ENTRY
};

/**
 * Properties supported under `sound`.
*/
enum InstSoundPropertyId {
    /**
     * Default / unset / unknown.
    */
    INST_SOUND_PROPERTY_DEFAULT_UNKNOWN = 0,

    /**
     * .inst name: use
     * class: struct ALSound->wavetable->aifc_path
    */
    INST_SOUND_PROPERTY_USE = 1,

    /**
     * .inst name: pan
     * class: struct ALSound->sample_pan
    */
    INST_SOUND_PROPERTY_PAN,

    /**
     * .inst name: volume
     * class: struct ALSound->sample_volume
    */
    INST_SOUND_PROPERTY_VOLUME,

    /**
     * .inst name: envelope
     * class: struct ALSound->envelope
    */
    INST_SOUND_PROPERTY_ENVELOPE,

    /**
     * .inst name: keymap
     * class: struct ALSound->keymap
    */
    INST_SOUND_PROPERTY_KEYMAP,

    /**
     * .inst name: metaCtlWriteOrder
     * class: struct ALSOUND->ctl_write_order
    */
    INST_SOUND_PROPERTY_META_CTL_WRITE_ORDER
};

/**
 * Properties supported under `keymap`.
*/
enum InstKeyMapPropertyId {
    /**
     * Default / unset / unknown.
    */
    INST_KEYMAP_PROPERTY_DEFAULT_UNKNOWN = 0,

    /**
     * .inst name: velocityMin
     * class: struct ALKeyMap->velocity_min
    */
    INST_KEYMAP_PROPERTY_VELOCITY_MIN = 1,

    /**
     * .inst name: velocityMax
     * class: struct ALKeyMap->velocity_max
    */
    INST_KEYMAP_PROPERTY_VELOCITY_MAX,

    /**
     * .inst name: keyMin
     * class: struct ALKeyMap->key_min
    */
    INST_KEYMAP_PROPERTY_KEY_MIN,

    /**
     * .inst name: keyMax
     * class: struct ALKeyMap->key_max
    */
    INST_KEYMAP_PROPERTY_KEY_MAX,

    /**
     * .inst name: keyBase
     * class: struct ALKeyMap->key_base
    */
    INST_KEYMAP_PROPERTY_KEY_BASE,

    /**
     * .inst name: detune
     * class: struct ALKeyMap->detune
    */
    INST_KEYMAP_PROPERTY_DETUNE,

    /**
     * .inst name: metaCtlWriteOrder
     * class: struct ALKeyMap->ctl_write_order
    */
    INST_KEYMAP_PROPERTY_META_CTL_WRITE_ORDER
};

/**
 * Properties supported under `envelope`.
*/
enum InstEnvelopePropertyId {
    /**
     * Default / unset / unknown.
    */
    INST_ENVELOPE_PROPERTY_DEFAULT_UNKNOWN = 0,

    /**
     * .inst name: attackTime
     * class: struct ALEnvelope->attack_time
    */
    INST_ENVELOPE_PROPERTY_ATTACK_TIME = 1,

    /**
     * .inst name: attackVolume
     * class: struct ALEnvelope->attack_volume
    */
    INST_ENVELOPE_PROPERTY_ATTACK_VOLUME,

    /**
     * .inst name: decayTime
     * class: struct ALEnvelope->decay_time
    */
    INST_ENVELOPE_PROPERTY_DECAY_TIME,

    /**
     * .inst name: decayVolume
     * class: struct ALEnvelope->decay_volume
    */
    INST_ENVELOPE_PROPERTY_DECAY_VOLUME,

    /**
     * .inst name: releaseTime
     * class: struct ALEnvelope->release_time
    */
    INST_ENVELOPE_PROPERTY_RELEASE_TIME,

    /**
     * .inst name: metaCtlWriteOrder
     * class: struct ALEnvelope->ctl_write_order
    */
    INST_ENVELOPE_PROPERTY_META_CTL_WRITE_ORDER
};

/**
 * Describes top level types.
*/
static struct TypeInfo InstTypes[] = {
    { INST_TYPE_BANK, "bank", TYPE_ID_NONE },
    { INST_TYPE_INSTRUMENT, "instrument", TYPE_ID_NONE },
    { INST_TYPE_SOUND, "sound", TYPE_ID_NONE },
    { INST_TYPE_KEYMAP, "keymap", TYPE_ID_NONE },
    { INST_TYPE_ENVELOPE, "envelope", TYPE_ID_NONE }
};
static const int InstTypes_len = ARRAY_LENGTH(InstTypes);

/**
 * Describes properties of .inst `bank` type.
*/
static struct TypeInfo InstBankProperties[] = {
    { INST_BANK_PROPERTY_INSTRUMENT_ARR_ENTRY, "instrument", TYPE_ID_ARRAY_TEXT_REF_ID },
    { INST_BANK_PROPERTY_SAMPLE_RATE, "sampleRate", TYPE_ID_INT }
};
static const int InstBankProperties_len = ARRAY_LENGTH(InstBankProperties);

/**
 * Describes properties of .inst `instrument` type.
*/
static struct TypeInfo InstInstrumentProperties[] = {
    { INST_INSTRUMENT_PROPERTY_VOLUME, "volume", TYPE_ID_INT },
    { INST_INSTRUMENT_PROPERTY_PAN, "pan", TYPE_ID_INT },
    { INST_INSTRUMENT_PROPERTY_PRIORITY, "priority", TYPE_ID_INT },
    { INST_INSTRUMENT_PROPERTY_FLAGS, "flags", TYPE_ID_INT },
    { INST_INSTRUMENT_PROPERTY_TREM_TYPE, "tremType", TYPE_ID_INT },
    { INST_INSTRUMENT_PROPERTY_TREM_RATE, "tremRate", TYPE_ID_INT },
    { INST_INSTRUMENT_PROPERTY_TREM_DEPTH, "tremDepth", TYPE_ID_INT },
    { INST_INSTRUMENT_PROPERTY_TREM_DELAY, "tremDelay", TYPE_ID_INT },
    { INST_INSTRUMENT_PROPERTY_VIB_TYPE, "vibType", TYPE_ID_INT },
    { INST_INSTRUMENT_PROPERTY_VIB_RATE, "vibRate", TYPE_ID_INT },
    { INST_INSTRUMENT_PROPERTY_VIB_DEPTH, "vibDepth", TYPE_ID_INT },
    { INST_INSTRUMENT_PROPERTY_VIB_DELAY, "vibDelay", TYPE_ID_INT },
    { INST_INSTRUMENT_PROPERTY_BENDRANGE, "bendRange", TYPE_ID_INT },
    { INST_INSTRUMENT_PROPERTY_SOUND_ARR_ENTRY, "sound", TYPE_ID_ARRAY_TEXT_REF_ID }
};
static const int InstInstrumentProperties_len = ARRAY_LENGTH(InstInstrumentProperties);

/**
 * Describes properties of .inst `sound` type.
*/
static struct TypeInfo InstSoundProperties[] = {
    { INST_SOUND_PROPERTY_USE, "use", TYPE_ID_USE_STRING },
    { INST_SOUND_PROPERTY_PAN, "pan", TYPE_ID_INT },
    { INST_SOUND_PROPERTY_VOLUME, "volume", TYPE_ID_INT },
    { INST_SOUND_PROPERTY_ENVELOPE, "envelope", TYPE_ID_TEXT_REF_ID },
    { INST_SOUND_PROPERTY_KEYMAP, "keymap", TYPE_ID_TEXT_REF_ID },
    { INST_SOUND_PROPERTY_META_CTL_WRITE_ORDER, "metaCtlWriteOrder", TYPE_ID_INT }
};
static const int InstSoundProperties_len = ARRAY_LENGTH(InstSoundProperties);

/**
 * Describes properties of .inst `keymap` type.
*/
static struct TypeInfo InstKeyMapProperties[] = {
    { INST_KEYMAP_PROPERTY_VELOCITY_MIN, "velocityMin", TYPE_ID_INT },
    { INST_KEYMAP_PROPERTY_VELOCITY_MAX, "velocityMax", TYPE_ID_INT },
    { INST_KEYMAP_PROPERTY_KEY_MIN, "keyMin", TYPE_ID_INT },
    { INST_KEYMAP_PROPERTY_KEY_MAX, "keyMax", TYPE_ID_INT },
    { INST_KEYMAP_PROPERTY_KEY_BASE, "keyBase", TYPE_ID_INT },
    { INST_KEYMAP_PROPERTY_DETUNE, "detune", TYPE_ID_INT },
    { INST_KEYMAP_PROPERTY_META_CTL_WRITE_ORDER, "metaCtlWriteOrder", TYPE_ID_INT }
};
static const int InstKeyMapProperties_len = ARRAY_LENGTH(InstKeyMapProperties);

/**
 * Describes properties of .inst `envelope` type.
*/
static struct TypeInfo InstEnvelopeProperties[] = {
    { INST_ENVELOPE_PROPERTY_ATTACK_TIME, "attackTime", TYPE_ID_INT },
    { INST_ENVELOPE_PROPERTY_ATTACK_VOLUME, "attackVolume", TYPE_ID_INT },
    { INST_ENVELOPE_PROPERTY_DECAY_TIME, "decayTime", TYPE_ID_INT },
    { INST_ENVELOPE_PROPERTY_DECAY_VOLUME, "decayVolume", TYPE_ID_INT },
    { INST_ENVELOPE_PROPERTY_RELEASE_TIME, "releaseTime", TYPE_ID_INT },
    { INST_ENVELOPE_PROPERTY_META_CTL_WRITE_ORDER, "metaCtlWriteOrder", TYPE_ID_INT }
};
static const int InstEnvelopeProperties_len = ARRAY_LENGTH(InstEnvelopeProperties);

/**
 * Parse states used by finite state machine.
 * See graphml or svg for valid transitions.
*/
enum InstParseState {
    /**
     * Begin state.
    */
    INST_PARSE_STATE_INITIAL = 1,

    /**
     * A '#' character was read.
    */
    INST_PARSE_STATE_COMMENT,

    /**
     * Reading the type name.
    */
    INST_PARSE_STATE_TYPE_NAME,

    /**
     * Type name has ended (whitespace etc), and
     * now searching for a character to begin the
     * instance name.
    */
    INST_PARSE_STATE_INITIAL_INSTANCE_NAME,

    /**
     * Reading the instance name.
    */
    INST_PARSE_STATE_INSTANCE_NAME,

    /**
     * Instance name has ended (whitespace etc),
     * and now searching for '{'.
    */
    INST_PARSE_STATE_SEARCH_OPEN_BRACKET,

    /**
     * Inside the block after '{', searching
     * for text to begin a property name
     * or closing '}'.
    */
    INST_PARSE_STATE_INITIAL_INSTANCE_PROPERTY,

    /**
     * Reading the property name.
    */
    INST_PARSE_STATE_PROPERTY_NAME,

    /**
     * The property requires an equal sign,
     * so search for that.
    */
    INST_PARSE_STATE_EQUAL_SIGN_INT,

    /**
     * The property requires an integer value,
     * search for text of that kind.
    */
    INST_PARSE_STATE_INITIAL_INT_VALUE,

    /**
     * Reading the property value as an integer.
    */
    INST_PARSE_STATE_INT_VALUE,

    /**
     * The property requires `("filename")`,
     * search for open '('.
    */
    INST_PARSE_STATE_INITIAL_FILENAME_VALUE,

    /**
     * The property requires `("filename")`,
     * search for open '"' after open '('.
    */
    INST_PARSE_STATE_SEARCH_OPEN_QUOTE,

    /**
     * Read filename, all text until closing '"'.
    */
    INST_PARSE_STATE_FILENAME,

    /**
     * The property requires `("filename")`,
     * search for closing ')'.
    */
    INST_PARSE_STATE_SEARCH_CLOSE_PAREN,

    /**
     * Done reading property assignment,
     * search for ';'.
    */
    INST_PARSE_STATE_SEARCH_SEMI_COLON,

    /**
     * The property is an array or list,
     * search for opening '['.
    */
    INST_PARSE_STATE_BEGIN_ARRAY_REF,

    /**
     * The property is an array or list,
     * search for character that could be
     * a number.
    */
    INST_PARSE_STATE_INITIAL_ARRAY_INDEX,

    /**
     * Reading the array index value.
    */
    INST_PARSE_STATE_ARRAY_INDEX,

    /**
     * Done reading array index value,
     * search for closing ']'.
    */
    INST_PARSE_STATE_ARRAY_INDEX_SEARCH_CLOSE_BRACKET,

    /**
     * The property value is a text ref, it's read
     * with an equal sign so search for '='.
    */
    INST_PARSE_STATE_EQUAL_SIGN_TEXT_REF_ID,

    /**
     * Search for character that could begin
     * text reference id.
    */
    INST_PARSE_STATE_INITIAL_TEXT_REF_ID,

    /**
     * Read text reference id.
    */
    INST_PARSE_STATE_TEXT_REF_ID
};


// forward declarations.




static inline int StringHash_instrument_unvisited(void *vp);
static inline int StringHash_sound_unvisited(void *vp);
static inline int StringHash_keymap_unvisited(void *vp);
static inline int StringHash_envelope_unvisited(void *vp);
static struct MissingRef *MissingRef_new(int key, void *self, char *ref_id, size_t len);
void MissingRef_free(struct MissingRef *ref);
void MissingRef_hashcallback_free(void *data);
static struct InstParseContext *InstParseContext_new(void);
static void InstParseContext_free(struct InstParseContext *context);

static void buffer_append_inc(char *buffer, int *position, char c);

static void get_type(const char *type_name, struct RuntimeTypeInfo *type);
static void get_property(struct RuntimeTypeInfo *type, const char *property_name, struct RuntimeTypeInfo *property);
static void set_array_index_int(struct InstParseContext *context);
static void set_current_property_value_int(struct InstParseContext *context);
static void create_instance(struct InstParseContext *context);
static void apply_property_on_instance_bank(struct InstParseContext *context);
void apply_property_on_instance_instrument(struct InstParseContext *context);
void apply_property_on_instance_sound(struct InstParseContext *context);
void apply_property_on_instance_keymap(struct InstParseContext *context);
void apply_property_on_instance_envelope(struct InstParseContext *context);
void apply_property_on_instance(struct InstParseContext *context);
static void add_orphaned_instance(struct InstParseContext *context);
static void resolve_references_sound(struct InstParseContext *context, struct ALSound *sound);
static void resolve_references_instrument(struct InstParseContext *context, struct ALInstrument *instrument);
static void resolve_references_bank(struct InstParseContext *context, struct ALBank *bank);
static void resolve_references(struct InstParseContext *context, struct ALBankFile *bank_file);

// end forward declarations

/**
 * Foreach `any` callback method.
 * @param vp: hashtable value pointer.
 * @returns: true if unvisitied, zero otherwise.
*/
static inline int StringHash_instrument_unvisited(void *vp)
{
    struct ALInstrument *instrument = (struct ALInstrument *)vp;

    if (instrument != NULL)
    {
        return instrument->visited == 0;
    }

    return 0;
}

/**
 * Foreach `any` callback method.
 * @param vp: hashtable value pointer.
 * @returns: true if unvisitied, zero otherwise.
*/
static inline int StringHash_sound_unvisited(void *vp)
{
    struct ALSound *sound = (struct ALSound *)vp;

    if (sound != NULL)
    {
        return sound->visited == 0;
    }

    return 0;
}

/**
 * Foreach `any` callback method.
 * @param vp: hashtable value pointer.
 * @returns: true if unvisitied, zero otherwise.
*/
static inline int StringHash_keymap_unvisited(void *vp)
{
    struct ALKeyMap *keymap = (struct ALKeyMap *)vp;

    if (keymap != NULL)
    {
        return keymap->visited == 0;
    }

    return 0;
}

/**
 * Foreach `any` callback method.
 * @param vp: hashtable value pointer.
 * @returns: true if unvisitied, zero otherwise.
*/
static inline int StringHash_envelope_unvisited(void *vp)
{
    struct ALEnvelope *envelope = (struct ALEnvelope *)vp;

    if (envelope != NULL)
    {
        return envelope->visited == 0;
    }

    return 0;
}

/**
 * Allocates memory for a new object.
 * If {@code ref_id} has a positive length, memory will be allocated for that
 * and the value copied to the new object.
 * @param key: primary key.
 * @param self: object missing reference.
 * @param ref_id: text_id of object to get.
 * @param len: length in bytes of string without terminating zero.
 * @returns: pointer to new object.
*/
static struct MissingRef *MissingRef_new(int key, void *self, char *ref_id, size_t len)
{
    TRACE_ENTER(__func__)

    struct MissingRef *p = (struct MissingRef *)malloc_zero(1, sizeof(struct MissingRef));
    p->key = key;
    p->self = self;

    if (len > 0)
    {
        p->ref_id = (char *)malloc_zero(1, len + 1);
        memcpy(p->ref_id, ref_id, len);
    }

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Frees all memory associated with object.
 * @param ref: object to free.
*/
void MissingRef_free(struct MissingRef *ref)
{
    TRACE_ENTER(__func__)

    if (ref == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (ref->ref_id != NULL)
    {
        free(ref->ref_id);
        ref->ref_id = NULL;
    }

    free(ref);

    TRACE_LEAVE(__func__)
}

/**
 * Foreach iterator callback to free all associated memory.
 * @param data: pointer to {@code struct MissingRef}.
*/
void MissingRef_hashcallback_free(void *data)
{
    TRACE_ENTER(__func__)

    struct MissingRef *ref = (struct MissingRef *)data;

    MissingRef_free(ref);

    TRACE_LEAVE(__func__)
}

/**
 * Allocates memory for a new context.
 * @returns: pointer to new context.
*/
static struct InstParseContext *InstParseContext_new()
{
    TRACE_ENTER(__func__)

    struct InstParseContext *context = (struct InstParseContext*)malloc_zero(1, sizeof(struct InstParseContext));

    context->type_name_buffer = (char *)malloc_zero(1, IDENTIFIER_MAX_LEN);
    context->instance_name_buffer = (char *)malloc_zero(1, IDENTIFIER_MAX_LEN);
    context->property_name_buffer = (char *)malloc_zero(1, IDENTIFIER_MAX_LEN);
    context->array_index_value = (char *)malloc_zero(1, IDENTIFIER_MAX_LEN);
    // can contain filename path
    context->property_value_buffer = (char *)malloc_zero(1, MAX_FILENAME_LEN);

    context->current_property = (struct RuntimeTypeInfo *)malloc_zero(1, sizeof(struct RuntimeTypeInfo));
    context->current_type = (struct RuntimeTypeInfo *)malloc_zero(1, sizeof(struct RuntimeTypeInfo));

    context->orphaned_banks = StringHashTable_new();
    context->orphaned_instruments = StringHashTable_new();
    context->orphaned_sounds = StringHashTable_new();
    context->orphaned_wavetables = StringHashTable_new();
    context->orphaned_keymaps = StringHashTable_new();
    context->orphaned_envelopes = StringHashTable_new();

    context->sound_missing_wavetable = IntHashTable_new();
    context->sound_missing_envelope = IntHashTable_new();
    context->sound_missing_keymap = IntHashTable_new();

    TRACE_LEAVE(__func__)

    return context;
}

/**
 * Frees all memory associated with context.
 * @param context: object to free.
*/
static void InstParseContext_free(struct InstParseContext *context)
{
    TRACE_ENTER(__func__)

    StringHashTable_free(context->orphaned_banks);
    StringHashTable_free(context->orphaned_instruments);
    StringHashTable_free(context->orphaned_sounds);
    StringHashTable_free(context->orphaned_wavetables);
    StringHashTable_free(context->orphaned_keymaps);
    StringHashTable_free(context->orphaned_envelopes);

    if (context->sound_missing_wavetable != NULL)
    {
        IntHashTable_foreach(context->sound_missing_wavetable, MissingRef_hashcallback_free);
        IntHashTable_free(context->sound_missing_wavetable);
    }

    if (context->sound_missing_envelope != NULL)
    {
        IntHashTable_foreach(context->sound_missing_envelope, MissingRef_hashcallback_free);
        IntHashTable_free(context->sound_missing_envelope);
    }

    if (context->sound_missing_keymap != NULL)
    {
        IntHashTable_foreach(context->sound_missing_keymap, MissingRef_hashcallback_free);
        IntHashTable_free(context->sound_missing_keymap);
    }

    free(context->current_type);
    free(context->current_property);

    free(context->property_value_buffer);
    free(context->array_index_value);
    free(context->property_name_buffer);
    free(context->instance_name_buffer);
    free(context->type_name_buffer);
    
    free(context);

    TRACE_LEAVE(__func__)
}

/**
 * Appends character to buffer and increments position.
 * No length / overflow checks are performed.
 * @param buffer: buffer to append to.
 * @param position: out parameter. Current position to place character. Will be incremented.
 * @param c: character to add to bufer.
*/
static void buffer_append_inc(char *buffer, int *position, char c)
{
    buffer[*position] = c;
    *position = *position + 1;
}

/**
 * Resolves text to type.
 * No memory is allocated.
 * @param type_name: text to get type from.
 * @param type: out paramater. Will have properties set explaining type.
*/
static void get_type(const char *type_name, struct RuntimeTypeInfo *type)
{
    TRACE_ENTER(__func__)

    type->key = 0;
    type->type_id = 0;

    int i;
    for (i=0; i<InstTypes_len; i++)
    {
        if (strcmp(type_name, InstTypes[i].value) == 0)
        {
            type->key = InstTypes[i].key;
            type->type_id = InstTypes[i].type_id;

            TRACE_LEAVE(__func__)
            return;
        }
    }

    stderr_exit(EXIT_CODE_GENERAL, "%s %d>: cannot resolve type name \"%s\"\n", __func__, __LINE__, type_name);

    TRACE_LEAVE(__func__)
}

/**
 * For a given type, resolves text to a property info.
 * No memory is allocated.
 * @param type: Type to get property for.
 * @param property_name: text to get property from.
 * @param property: out paramater. Will have properties set explaining type.
*/
static void get_property(struct RuntimeTypeInfo *type, const char *property_name, struct RuntimeTypeInfo *property)
{
    TRACE_ENTER(__func__)

    property->key = 0;
    property->type_id = 0;

    int i;

    switch (type->key)
    {
        case INST_TYPE_BANK:
        {
            for (i=0; i<InstBankProperties_len; i++)
            {
                if (strcmp(property_name, InstBankProperties[i].value) == 0)
                {
                    property->key = InstBankProperties[i].key;
                    property->type_id = InstBankProperties[i].type_id;

                    TRACE_LEAVE(__func__)
                    return;
                }
            }
        }
        break;

        case INST_TYPE_INSTRUMENT:
        {
            for (i=0; i<InstInstrumentProperties_len; i++)
            {
                if (strcmp(property_name, InstInstrumentProperties[i].value) == 0)
                {
                    property->key = InstInstrumentProperties[i].key;
                    property->type_id = InstInstrumentProperties[i].type_id;

                    TRACE_LEAVE(__func__)
                    return;
                }
            }
        }
        break;

        case INST_TYPE_SOUND:
        {
            for (i=0; i<InstSoundProperties_len; i++)
            {
                if (strcmp(property_name, InstSoundProperties[i].value) == 0)
                {
                    property->key = InstSoundProperties[i].key;
                    property->type_id = InstSoundProperties[i].type_id;

                    TRACE_LEAVE(__func__)
                    return;
                }
            }
        }
        break;

        case INST_TYPE_KEYMAP:
        {
            for (i=0; i<InstKeyMapProperties_len; i++)
            {
                if (strcmp(property_name, InstKeyMapProperties[i].value) == 0)
                {
                    property->key = InstKeyMapProperties[i].key;
                    property->type_id = InstKeyMapProperties[i].type_id;

                    TRACE_LEAVE(__func__)
                    return;
                }
            }
        }
        break;

        case INST_TYPE_ENVELOPE:
        {
            for (i=0; i<InstEnvelopeProperties_len; i++)
            {
                if (strcmp(property_name, InstEnvelopeProperties[i].value) == 0)
                {
                    property->key = InstEnvelopeProperties[i].key;
                    property->type_id = InstEnvelopeProperties[i].type_id;

                    TRACE_LEAVE(__func__)
                    return;
                }
            }
        }
        break;
    }

    stderr_exit(EXIT_CODE_GENERAL, "%s %d>: cannot resolve property_name \"%s\", type key=%d\n", __func__, __LINE__, property_name, type->key);

    TRACE_LEAVE(__func__)
}

/**
 * Parses {@code context->array_index_value} as integer and sets
 * {@code context->array_index_int} to the parsed value. If array
 * is empty (position is zero), value is set to zero.
 * @param context: context.
*/
static void set_array_index_int(struct InstParseContext *context)
{
    TRACE_ENTER(__func__)

    int val;
    char *pend = NULL;

    if (context->array_index_value_pos == 0)
    {
        context->array_index_int = 0;

        TRACE_LEAVE(__func__)
        return;
    }

    val = strtol(context->array_index_value, &pend, 0);
                
    if (pend != NULL && *pend == '\0')
    {
        if (errno == ERANGE)
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> error (range), cannot parse context->array_index_value as integer: %s\n", __func__, __LINE__, context->array_index_value);
        }

        context->array_index_int = val;
    }
    else
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> error, cannot parse context->array_index_value as integer: %s\n", __func__, __LINE__, context->array_index_value);
    }

    TRACE_LEAVE(__func__)
}

/**
 * Parses {@code context->property_value_buffer} as integer and sets
 * {@code context->current_value_int} to the parsed value. If array
 * is empty (position is zero), value is set to zero. The original
 * buffer remains unchanged (this can be called multiple times).
 * @param context: context.
*/
static void set_current_property_value_int(struct InstParseContext *context)
{
    TRACE_ENTER(__func__)

    int val;
    char *pend = NULL;

    if (context->property_value_buffer_pos == 0)
    {
        context->current_value_int = 0;

        TRACE_LEAVE(__func__)
        return;
    }

    val = strtol(context->property_value_buffer, &pend, 0);
                
    if (pend != NULL && *pend == '\0')
    {
        if (errno == ERANGE)
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> error (range), cannot parse context->property_value_buffer as integer: %s\n", __func__, __LINE__, context->property_value_buffer);
        }

        context->current_value_int = val;
    }
    else
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> error, cannot parse context->property_value_buffer as integer: %s\n", __func__, __LINE__, context->property_value_buffer);
    }

    TRACE_LEAVE(__func__)
}

/**
 * Constructs a new instance of `AL-` type using the {@code context->current_type->key}
 * and sets {@code context->current_instance} to this new object.
 * @param context: context.
*/
static void create_instance(struct InstParseContext *context)
{
    TRACE_ENTER(__func__)

    switch (context->current_type->key)
    {
        case INST_TYPE_BANK:
        {
            struct ALBank *bank = (struct ALBank *)ALBank_new();
            strcpy(bank->text_id, context->instance_name_buffer);
            context->current_instance = bank;

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("create new bank id=%d ref=\"%s\"\n", bank->id, bank->text_id);
            }
        }
        break;

        case INST_TYPE_INSTRUMENT:
        {
            struct ALInstrument *instrument = (struct ALInstrument *)ALInstrument_new();
            strcpy(instrument->text_id, context->instance_name_buffer);
            context->current_instance = instrument;

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("create new instrument id=%d ref=\"%s\"\n", instrument->id, instrument->text_id);
            }
        }
        break;

        case INST_TYPE_SOUND:
        {
            struct ALSound *sound = (struct ALSound *)ALSound_new();
            strcpy(sound->text_id, context->instance_name_buffer);
            context->current_instance = sound;

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("create new sound id=%d ref=\"%s\"\n", sound->id, sound->text_id);
            }
        }
        break;

        case INST_TYPE_KEYMAP:
        {
            struct ALKeyMap *keymap = (struct ALKeyMap *)ALKeyMap_new();
            strcpy(keymap->text_id, context->instance_name_buffer);
            context->current_instance = keymap;

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("create new keymap id=%d ref=\"%s\"\n", keymap->id, keymap->text_id);
            }
        }
        break;

        case INST_TYPE_ENVELOPE:
        {
            struct ALEnvelope *envelope = (struct ALEnvelope *)ALEnvelope_new();
            strcpy(envelope->text_id, context->instance_name_buffer);
            context->current_instance = envelope;

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("create new envelope id=%d ref=\"%s\"\n", envelope->id, envelope->text_id);
            }
        }
        break;
    }

    TRACE_LEAVE(__func__)
}

/**
 * The {@code context->current_instance} has been resolved to {@code struct ALBank}.
 * This resolves the current property info and sets the value from
 * {@code context->property_value_buffer}.
 * 
 * Note: {@code bank->inst_offsets} is abused to store a
 * linked list of dependencies (name ref ids).
 * @param context: context.
*/
static void apply_property_on_instance_bank(struct InstParseContext *context)
{
    TRACE_ENTER(__func__)

    struct ALBank *bank = (struct ALBank *)context->current_instance;

    if (bank == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: context->current_instance is NULL\n", __func__, __LINE__);
    }

    switch (context->current_property->key)
    {
        case INST_BANK_PROPERTY_INSTRUMENT_ARR_ENTRY:
        {
            struct LinkedListNode *node;
            struct KeyValue *kvp;

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("append on bank id=%d [%s]=\"%s\"\n", bank->id, context->array_index_value, context->property_value_buffer);
            }

            // borrow inst_offsets property to store list of references.
            if (bank->inst_offsets == NULL)
            {
                bank->inst_offsets = (void *)LinkedList_new();
            }

            kvp = KeyValue_new_value(context->property_value_buffer);
            kvp->key = context->array_index_int;
            node = LinkedListNode_new();
            node->data = kvp;
            LinkedList_append_node((struct LinkedList*)bank->inst_offsets, node);
        }
        break;

        case INST_BANK_PROPERTY_SAMPLE_RATE:
        {
            set_current_property_value_int(context);

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set bank id=%d sample_rate=%d\n", bank->id, context->current_value_int);
            }
            
            bank->sample_rate = (int)context->current_value_int;
        }
        break;

        default:
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d>: context->current_property->key not supported: %d\n", __func__, __LINE__, context->current_property->key);
        }
        break;
    }

    TRACE_LEAVE(__func__)
}

/**
 * The {@code context->current_instance} has been resolved to {@code struct ALInstrument}.
 * This resolves the current property info and sets the value from
 * {@code context->property_value_buffer}.
 * 
 * Note: {@code instrument->sound_offsets} is abused to store a
 * linked list of dependencies (name ref ids).
 * @param context: context.
*/
void apply_property_on_instance_instrument(struct InstParseContext *context)
{
    TRACE_ENTER(__func__)

    struct ALInstrument *instrument = (struct ALInstrument *)context->current_instance;

    if (instrument == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: context->current_instance is NULL\n", __func__, __LINE__);
    }

    switch (context->current_property->key)
    {
        case INST_INSTRUMENT_PROPERTY_VOLUME:
        {
            set_current_property_value_int(context);

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set instrument id=%d volume=%d\n", instrument->id, context->current_value_int);
            }
            
            instrument->volume = (uint8_t)context->current_value_int;
        }
        break;

        case INST_INSTRUMENT_PROPERTY_PAN:
        {
            set_current_property_value_int(context);

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set instrument id=%d pan=%d\n", instrument->id, context->current_value_int);
            }
            
            instrument->pan = (uint8_t)context->current_value_int;
        }
        break;

        case INST_INSTRUMENT_PROPERTY_PRIORITY:
        {
            set_current_property_value_int(context);

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set instrument id=%d priority=%d\n", instrument->id, context->current_value_int);
            }
            
            instrument->priority = (uint8_t)context->current_value_int;
        }
        break;

        case INST_INSTRUMENT_PROPERTY_FLAGS:
        {
            set_current_property_value_int(context);

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set instrument id=%d flags=%d\n", instrument->id, context->current_value_int);
            }
            
            instrument->flags = (uint8_t)context->current_value_int;
        }
        break;

        case INST_INSTRUMENT_PROPERTY_TREM_TYPE:
        {
            set_current_property_value_int(context);

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set instrument id=%d trem_type=%d\n", instrument->id, context->current_value_int);
            }
            
            instrument->trem_type = (uint8_t)context->current_value_int;
        }
        break;

        case INST_INSTRUMENT_PROPERTY_TREM_RATE:
        {
            set_current_property_value_int(context);

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set instrument id=%d trem_rate=%d\n", instrument->id, context->current_value_int);
            }
            
            instrument->trem_rate = (uint8_t)context->current_value_int;
        }
        break;

        case INST_INSTRUMENT_PROPERTY_TREM_DEPTH:
        {
            set_current_property_value_int(context);

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set instrument id=%d trem_depth=%d\n", instrument->id, context->current_value_int);
            }
            
            instrument->trem_depth = (uint8_t)context->current_value_int;
        }
        break;

        case INST_INSTRUMENT_PROPERTY_TREM_DELAY:
        {
            set_current_property_value_int(context);

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set instrument id=%d trem_delay=%d\n", instrument->id, context->current_value_int);
            }
            
            instrument->trem_delay = (uint8_t)context->current_value_int;
        }
        break;

        case INST_INSTRUMENT_PROPERTY_VIB_TYPE:
        {
            set_current_property_value_int(context);

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set instrument id=%d vib_type=%d\n", instrument->id, context->current_value_int);
            }
            
            instrument->vib_type = (uint8_t)context->current_value_int;
        }
        break;

        case INST_INSTRUMENT_PROPERTY_VIB_RATE:
        {
            set_current_property_value_int(context);

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set instrument id=%d vib_rate=%d\n", instrument->id, context->current_value_int);
            }
            
            instrument->vib_rate = (uint8_t)context->current_value_int;
        }
        break;

        case INST_INSTRUMENT_PROPERTY_VIB_DEPTH:
        {
            set_current_property_value_int(context);

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set instrument id=%d vib_depth=%d\n", instrument->id, context->current_value_int);
            }
            
            instrument->vib_depth = (uint8_t)context->current_value_int;
        }
        break;

        case INST_INSTRUMENT_PROPERTY_VIB_DELAY:
        {
            set_current_property_value_int(context);

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set instrument id=%d vib_delay=%d\n", instrument->id, context->current_value_int);
            }
            
            instrument->vib_delay = (uint8_t)context->current_value_int;
        }
        break;

        case INST_INSTRUMENT_PROPERTY_BENDRANGE:
        {
            set_current_property_value_int(context);

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set instrument id=%d bend_range=%d\n", instrument->id, context->current_value_int);
            }
            
            instrument->bend_range = (int16_t)context->current_value_int;
        }
        break;

        case INST_INSTRUMENT_PROPERTY_SOUND_ARR_ENTRY:
        {
            struct LinkedListNode *node;
            struct KeyValue *kvp;

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("append on instrument id=%d [%s]=\"%s\"\n", instrument->id, context->array_index_value, context->property_value_buffer);
            }
            
            // borrow sound_offsets property to store list of references.
            if (instrument->sound_offsets == NULL)
            {
                instrument->sound_offsets = (void *)LinkedList_new();
            }

            kvp = KeyValue_new_value(context->property_value_buffer);
            kvp->key = context->array_index_int;
            node = LinkedListNode_new();
            node->data = kvp;
            LinkedList_append_node((struct LinkedList*)instrument->sound_offsets, node);
        }
        break;

        default:
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d>: context->current_property->key not supported: %d\n", __func__, __LINE__, context->current_property->key);
        }
        break;
    }

    TRACE_LEAVE(__func__)
}

/**
 * The {@code context->current_instance} has been resolved to {@code struct ALSound}.
 * This resolves the current property info and sets the value from
 * {@code context->property_value_buffer}.
 * 
 * keymap, envelope, wavetable are saved and marked to be resolved once
 * parsing is complete.
 * @param context: context.
*/
void apply_property_on_instance_sound(struct InstParseContext *context)
{
    TRACE_ENTER(__func__)

    struct ALSound *sound = (struct ALSound *)context->current_instance;

    if (sound == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: context->current_instance is NULL\n", __func__, __LINE__);
    }

    switch (context->current_property->key)
    {
        case INST_SOUND_PROPERTY_USE:
        {
            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set sound id=%d wavetable=\"%s\"\n", sound->id, context->property_value_buffer);
            }

            if (sound->wavetable != NULL)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d>: sound->wavetable previously initialized\n", __func__, __LINE__);
            }

            struct MissingRef *ref;

            if (context->sound_missing_wavetable == NULL)
            {
                context->sound_missing_wavetable = IntHashTable_new();
            }

            if (!IntHashTable_contains(context->sound_missing_wavetable, sound->id))
            {
                ref = MissingRef_new(sound->id, (void*)sound, context->property_value_buffer, (size_t)context->property_value_buffer_pos);
                IntHashTable_add(context->sound_missing_wavetable, ref->key, ref);
            }
        }
        break;

        case INST_INSTRUMENT_PROPERTY_PAN:
        {
            set_current_property_value_int(context);

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set sound id=%d sample_pan=%d\n", sound->id, context->current_value_int);
            }

            sound->sample_pan = (uint8_t)context->current_value_int;
        }
        break;

        case INST_SOUND_PROPERTY_VOLUME:
        {
            set_current_property_value_int(context);

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set sound id=%d sample_volume=%d\n", sound->id, context->current_value_int);
            }

            sound->sample_volume = (uint8_t)context->current_value_int;
        }
        break;

        case INST_SOUND_PROPERTY_ENVELOPE:
        {
            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set sound id=%d envelope=\"%s\"\n", sound->id, context->property_value_buffer);
            }

            struct MissingRef *ref;

            if (context->sound_missing_envelope == NULL)
            {
                context->sound_missing_envelope = IntHashTable_new();
            }

            if (!IntHashTable_contains(context->sound_missing_envelope, sound->id))
            {
                ref = MissingRef_new(sound->id, (void*)sound, context->property_value_buffer, (size_t)context->property_value_buffer_pos);
                IntHashTable_add(context->sound_missing_envelope, ref->key, ref);
            }
        }
        break;

        case INST_SOUND_PROPERTY_KEYMAP:
        {
            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set sound id=%d keymap=\"%s\"\n", sound->id, context->property_value_buffer);
            }

            struct MissingRef *ref;

            if (context->sound_missing_keymap == NULL)
            {
                context->sound_missing_keymap = IntHashTable_new();
            }

            if (!IntHashTable_contains(context->sound_missing_keymap, sound->id))
            {
                ref = MissingRef_new(sound->id, (void*)sound, context->property_value_buffer, (size_t)context->property_value_buffer_pos);
                IntHashTable_add(context->sound_missing_keymap, ref->key, ref);
            }
        }
        break;

        case INST_SOUND_PROPERTY_META_CTL_WRITE_ORDER:
        {
            set_current_property_value_int(context);

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set sound id=%d ctl_write_order=%d\n", sound->id, context->current_value_int);
            }

            sound->ctl_write_order = (int)context->current_value_int;
        }
        break;

        default:
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d>: context->current_property->key not supported: %d\n", __func__, __LINE__, context->current_property->key);
        }
        break;
    }

    TRACE_LEAVE(__func__)
}

/**
 * The {@code context->current_instance} has been resolved to {@code struct ALKeyMap}.
 * This resolves the current property info and sets the value from
 * {@code context->property_value_buffer}.
 * @param context: context.
*/
void apply_property_on_instance_keymap(struct InstParseContext *context)
{
    TRACE_ENTER(__func__)

    struct ALKeyMap *keymap = (struct ALKeyMap *)context->current_instance;

    if (keymap == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: context->current_instance is NULL\n", __func__, __LINE__);
    }

    // all values are integers, can convert outside switch
    set_current_property_value_int(context);

    switch (context->current_property->key)
    {
        case INST_KEYMAP_PROPERTY_VELOCITY_MIN:
        {
            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set keymap id=%d velocity_min=%d\n", keymap->id, context->current_value_int);
            }

            keymap->velocity_min = (uint8_t)context->current_value_int;
        }
        break;

        case INST_KEYMAP_PROPERTY_VELOCITY_MAX:
        {
            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set keymap id=%d velocity_max=%d\n", keymap->id, context->current_value_int);
            }

            keymap->velocity_max = (uint8_t)context->current_value_int;
        }
        break;

        case INST_KEYMAP_PROPERTY_KEY_MIN:
        {
            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set keymap id=%d key_min=%d\n", keymap->id, context->current_value_int);
            }

            keymap->key_min = (uint8_t)context->current_value_int;
        }
        break;

        case INST_KEYMAP_PROPERTY_KEY_MAX:
        {
            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set keymap id=%d key_max=%d\n", keymap->id, context->current_value_int);
            }

            keymap->key_max = (uint8_t)context->current_value_int;
        }
        break;

        case INST_KEYMAP_PROPERTY_KEY_BASE:
        {
            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set keymap id=%d key_base=%d\n", keymap->id, context->current_value_int);
            }

            keymap->key_base = (uint8_t)context->current_value_int;
        }
        break;

        case INST_KEYMAP_PROPERTY_DETUNE:
        {
            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set keymap id=%d detune=%d\n", keymap->id, context->current_value_int);
            }

            keymap->detune = (int8_t)context->current_value_int;
        }
        break;

        case INST_KEYMAP_PROPERTY_META_CTL_WRITE_ORDER:
        {
            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set keymap id=%d ctl_write_order=%d\n", keymap->id, context->current_value_int);
            }

            keymap->ctl_write_order = (int)context->current_value_int;
        }
        break;

        default:
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d>: context->current_property->key not supported: %d\n", __func__, __LINE__, context->current_property->key);
        }
        break;
    }

    TRACE_LEAVE(__func__)
}

/**
 * The {@code context->current_instance} has been resolved to {@code struct ALEnvelope}.
 * This resolves the current property info and sets the value from
 * {@code context->property_value_buffer}.
 * @param context: context.
*/
void apply_property_on_instance_envelope(struct InstParseContext *context)
{
    TRACE_ENTER(__func__)

    struct ALEnvelope *envelope = (struct ALEnvelope *)context->current_instance;

    if (envelope == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: context->current_instance is NULL\n", __func__, __LINE__);
    }

    // all values are integers, can convert outside switch
    set_current_property_value_int(context);

    switch (context->current_property->key)
    {
        case INST_ENVELOPE_PROPERTY_ATTACK_TIME:
        {
            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set envelope id=%d attack_time=%d\n", envelope->id, context->current_value_int);
            }

            envelope->attack_time = (int32_t)context->current_value_int;
        }
        break;

        case INST_ENVELOPE_PROPERTY_ATTACK_VOLUME:
        {
            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set envelope id=%d attack_volume=%d\n", envelope->id, context->current_value_int);
            }

            envelope->attack_volume = (uint8_t)context->current_value_int;
        }
        break;

        case INST_ENVELOPE_PROPERTY_DECAY_TIME:
        {
            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set envelope id=%d decay_time=%d\n", envelope->id, context->current_value_int);
            }

            envelope->decay_time = (int32_t)context->current_value_int;
        }
        break;

        case INST_ENVELOPE_PROPERTY_DECAY_VOLUME:
        {
            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set envelope id=%d decay_volume=%d\n", envelope->id, context->current_value_int);
            }

            envelope->decay_volume = (uint8_t)context->current_value_int;
        }
        break;

        case INST_ENVELOPE_PROPERTY_RELEASE_TIME:
        {
            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set envelope id=%d release_time=%d\n", envelope->id, context->current_value_int);
            }

            envelope->release_time = (int32_t)context->current_value_int;
        }
        break;

        case INST_ENVELOPE_PROPERTY_META_CTL_WRITE_ORDER:
        {
            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set envelope id=%d ctl_write_order=%d\n", envelope->id, context->current_value_int);
            }

            envelope->ctl_write_order = (int32_t)context->current_value_int;
        }
        break;

        default:
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d>: context->current_property->key not supported: %d\n", __func__, __LINE__, context->current_property->key);
        }
        break;
    }

    TRACE_LEAVE(__func__)
}

/**
 * This is a pass through to resolve {@code context->current_type->key} to a known
 * type and call the appropriate handler to set the property value.
 * @param context: context.
*/
void apply_property_on_instance(struct InstParseContext *context)
{
    TRACE_ENTER(__func__)

    switch (context->current_type->key)
    {
        case INST_TYPE_BANK:
        {
            apply_property_on_instance_bank(context);
        }
        break;

        case INST_TYPE_INSTRUMENT:
        {
            apply_property_on_instance_instrument(context);
        }
        break;

        case INST_TYPE_SOUND:
        {
            apply_property_on_instance_sound(context);
        }
        break;

        case INST_TYPE_KEYMAP:
        {
            apply_property_on_instance_keymap(context);
        }
        break;

        case INST_TYPE_ENVELOPE:
        {
            apply_property_on_instance_envelope(context);
        }
        break;
    }

    context->property_value_buffer_pos = 0;
    context->array_index_value_pos = 0;
    context->property_name_buffer_pos = 0;

    context->current_value_int = 0;
    context->array_index_int = 0;

    memset(context->property_value_buffer, 0, MAX_FILENAME_LEN);
    memset(context->array_index_value, 0, IDENTIFIER_MAX_LEN);
    memset(context->property_name_buffer, 0, IDENTIFIER_MAX_LEN);

    context->current_property->key = 0;
    context->current_property->type_id = 0;
    
    TRACE_LEAVE(__func__)
}

/**
 * The block has been terminated and no more properties will be added to
 * {@code context->current_instance}. Resolve this to a type and
 * add to the appropriate "orphaned" hash table.
 * @param context: context.
*/
static void add_orphaned_instance(struct InstParseContext *context)
{
    TRACE_ENTER(__func__)

    if (context->current_instance == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: context->current_instance is NULL\n", __func__, __LINE__);
    }

    switch (context->current_type->key)
    {
        case INST_TYPE_BANK:
        {
            struct ALBank *bank = (struct ALBank *)context->current_instance;

            if (context->orphaned_banks == NULL)
            {
                context->orphaned_banks = StringHashTable_new();
            }

            if (StringHashTable_contains(context->orphaned_banks, bank->text_id))
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d>: error adding bank \"%s\" from line %d, this was previously defined.\n", __func__, __LINE__, bank->text_id, context->current_line);
            }

            StringHashTable_add(context->orphaned_banks, bank->text_id, bank);
        }
        break;

        case INST_TYPE_INSTRUMENT:
        {
            struct ALInstrument *instrument = (struct ALInstrument *)context->current_instance;

            if (context->orphaned_instruments == NULL)
            {
                context->orphaned_instruments = StringHashTable_new();
            }

            if (StringHashTable_contains(context->orphaned_instruments, instrument->text_id))
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d>: error adding isntrument \"%s\" from line %d, this was previously defined.\n", __func__, __LINE__, instrument->text_id, context->current_line);
            }

            StringHashTable_add(context->orphaned_instruments, instrument->text_id, instrument);
        }
        break;

        case INST_TYPE_SOUND:
        {
            struct ALSound *sound = (struct ALSound *)context->current_instance;

            if (context->orphaned_sounds == NULL)
            {
                context->orphaned_sounds = StringHashTable_new();
            }

            if (StringHashTable_contains(context->orphaned_sounds, sound->text_id))
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d>: error adding sound \"%s\" from line %d, this was previously defined.\n", __func__, __LINE__, sound->text_id, context->current_line);
            }

            StringHashTable_add(context->orphaned_sounds, sound->text_id, sound);
        }
        break;

        case INST_TYPE_KEYMAP:
        {
            struct ALKeyMap *keymap = (struct ALKeyMap *)context->current_instance;

            if (context->orphaned_keymaps == NULL)
            {
                context->orphaned_keymaps = StringHashTable_new();
            }

            if (StringHashTable_contains(context->orphaned_keymaps, keymap->text_id))
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d>: error adding keymap \"%s\" from line %d, this was previously defined.\n", __func__, __LINE__, keymap->text_id, context->current_line);
            }

            StringHashTable_add(context->orphaned_keymaps, keymap->text_id, keymap);
        }
        break;

        case INST_TYPE_ENVELOPE:
        {
            struct ALEnvelope *envelope = (struct ALEnvelope *)context->current_instance;

            if (context->orphaned_envelopes == NULL)
            {
                context->orphaned_envelopes = StringHashTable_new();
            }

            if (StringHashTable_contains(context->orphaned_envelopes, envelope->text_id))
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d>: error adding envelope \"%s\" from line %d, this was previously defined.\n", __func__, __LINE__, envelope->text_id, context->current_line);
            }

            StringHashTable_add(context->orphaned_envelopes, envelope->text_id, envelope);
        }
        break;
    }

    context->type_name_buffer_pos = 0;
    context->instance_name_buffer_pos = 0;

    memset(context->type_name_buffer, 0, IDENTIFIER_MAX_LEN);
    memset(context->instance_name_buffer, 0, IDENTIFIER_MAX_LEN);

    context->current_type->key = 0;
    context->current_type->type_id = 0;

    context->current_instance = NULL;
    
    TRACE_LEAVE(__func__)
}

/**
 * Fixes up the temporary repurposing of properties used during parsing.
 * This resolves all text ref ids to objects from the related "orphaned"
 * hash table.
 * @param context: context.
 * @param sound: sound object to resolve dependencies.
*/
static void resolve_references_sound(struct InstParseContext *context, struct ALSound *sound)
{
    TRACE_ENTER(__func__)

    if (context->sound_missing_wavetable != NULL)
    {
        if (IntHashTable_contains(context->sound_missing_wavetable, sound->id))
        {
            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("resolving wavetable reference for sound. sound id=%d, sound=\"%s\"...\n", sound->id, sound->text_id);
            }
            
            // dependency is satisfied, so remove from "missing" list
            struct MissingRef *ref = IntHashTable_pop(context->sound_missing_wavetable, sound->id);

            if (ref == NULL)
            {
                stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: context->sound_missing_wavetable hash table cannot resolve key %d\n", __func__, __LINE__, sound->id);
            }

            struct ALWaveTable *wavetable;

            // wave table is different from envelope and keymap.
            // The ref_id is for the .aifc_path.
            // The wavetable may not have been created yet, so create and add in that case.

            if (StringHashTable_contains(context->orphaned_wavetables, ref->ref_id))
            {
                // don't remove from hash table
                wavetable = StringHashTable_get(context->orphaned_wavetables, ref->ref_id);

                if (wavetable == NULL)
                {
                    stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: context->orphaned_wavetables hash table cannot resolve key \"%s\"\n", __func__, __LINE__, ref->ref_id);
                }

                if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
                {
                    printf("... found wavetable \"%s\"\n", wavetable->text_id);
                }
            }
            else
            {
                size_t len;
                wavetable = ALWaveTable_new();

                // the MissingRef objects owns the ref_id memory, so allocate space for the path.
                len = strlen(ref->ref_id);
                wavetable->aifc_path = (char *)malloc_zero(1, len + 1);
                memcpy(wavetable->aifc_path, ref->ref_id, len);

                StringHashTable_add(context->orphaned_wavetables, wavetable->aifc_path, wavetable);
            }

            sound->wavetable = wavetable;
            ALWaveTable_add_parent(wavetable, sound);
            wavetable->visited = 1;

            // dependency is satisfied, so free memory
            MissingRef_free(ref);
        }
    }

    if (context->sound_missing_envelope != NULL)
    {
        if (IntHashTable_contains(context->sound_missing_envelope, sound->id))
        {
            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("resolving envelope reference for sound. sound id=%d, sound=\"%s\"...\n", sound->id, sound->text_id);
            }
            
            // dependency is satisfied, so remove from "missing" list
            struct MissingRef *ref = IntHashTable_pop(context->sound_missing_envelope, sound->id);

            if (ref == NULL)
            {
                stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: context->sound_missing_envelope hash table cannot resolve key %d\n", __func__, __LINE__, sound->id);
            }

            // don't remove from hash table
            struct ALEnvelope *envelope = StringHashTable_get(context->orphaned_envelopes, ref->ref_id);

            if (envelope == NULL)
            {
                stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: context->orphaned_envelopes hash table cannot resolve key \"%s\"\n", __func__, __LINE__, ref->ref_id);
            }

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("... found envelope \"%s\"\n", envelope->text_id);
            }

            sound->envelope = envelope;
            ALEnvelope_add_parent(envelope, sound);
            envelope->visited = 1;

            // dependency is satisfied, so free memory
            MissingRef_free(ref);
        }
    }

    if (context->sound_missing_keymap != NULL)
    {
        if (IntHashTable_contains(context->sound_missing_keymap, sound->id))
        {
            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("resolving keymap reference for sound. sound id=%d, sound=\"%s\"...\n", sound->id, sound->text_id);
            }
            
            // dependency is satisfied, so remove from "missing" list
            struct MissingRef *ref = IntHashTable_pop(context->sound_missing_keymap, sound->id);

            if (ref == NULL)
            {
                stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: context->sound_missing_keymap hash table cannot resolve key %d\n", __func__, __LINE__, sound->id);
            }

            struct ALKeyMap *keymap = StringHashTable_get(context->orphaned_keymaps, ref->ref_id);

            if (keymap == NULL)
            {
                stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: context->orphaned_keymaps hash table cannot resolve key \"%s\"\n", __func__, __LINE__, ref->ref_id);
            }

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("... found keymap \"%s\"\n", keymap->text_id);
            }

            sound->keymap = keymap;
            ALKeyMap_add_parent(keymap, sound);
            keymap->visited = 1;

            // dependency is satisfied, so free memory
            MissingRef_free(ref);
        }
    }

    TRACE_LEAVE(__func__)
}

/**
 * Fixes up the temporary repurposing of properties used during parsing.
 * This resolves all text ref ids to objects from the related "orphaned"
 * hash table.
 * @param context: context.
 * @param instrument: instrument object to resolve dependencies.
*/
static void resolve_references_instrument(struct InstParseContext *context, struct ALInstrument *instrument)
{
    TRACE_ENTER(__func__)

    char *htkey;
    int count;
    int i;
    struct LinkedList *need_names;
    struct LinkedListNode *node;
    struct KeyValue *kvp;
    struct ALSound *sound;

    // sound_offsets was borrowed as list container.
    need_names = (struct LinkedList *)instrument->sound_offsets;

    if (need_names == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    count = need_names->count;
    
    instrument->sound_count = count;

    if (count == 0)
    {
        LinkedList_free(need_names);

        TRACE_LEAVE(__func__)
        return;
    }

    // sort nodes by array index ("key" property, not "value") read from .inst file, smallest to largest.
    LinkedList_merge_sort(need_names, LinkedListNode_KeyValue_compare_smaller_key);

    instrument->sounds = (struct ALSound **)malloc_zero(instrument->sound_count, sizeof(void*));

    node = need_names->head;
    for (i=0; node != NULL; i++, node = node->next)
    {
        kvp = node->data;

        if (kvp == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: instrument \"%s\" need_names invalid state, node->data is NULL\n", __func__, __LINE__, instrument->text_id);
        }

        htkey = kvp->value;

        if (htkey == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: instrument \"%s\" need_names invalid state, kvp->value is NULL, kvp->key=%d\n", __func__, __LINE__, instrument->text_id, kvp->key);
        }

        sound = StringHashTable_get(context->orphaned_sounds, htkey);

        if (sound == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: context->orphaned_sounds hash table cannot resolve key \"%s\"\n", __func__, __LINE__, htkey);
        }

        instrument->sounds[i] = sound;
        ALSound_add_parent(sound, instrument);
        sound->visited = 1;

        resolve_references_sound(context, sound);

        KeyValue_free(kvp);

        node->data = NULL;
    }

    LinkedList_free(need_names);

    instrument->sound_offsets = NULL;

    // allocate offsets array in case writing to .ctl
    // this can't be done until it's done being borrowed above.
    instrument->sound_offsets = (int32_t *)malloc_zero(instrument->sound_count, sizeof(void*));

    TRACE_LEAVE(__func__)
}

/**
 * Fixes up the temporary repurposing of properties used during parsing.
 * This resolves all text ref ids to objects from the related "orphaned"
 * hash table.
 * @param context: context.
 * @param bank: bank object to resolve dependencies.
*/
static void resolve_references_bank(struct InstParseContext *context, struct ALBank *bank)
{
    TRACE_ENTER(__func__)

    char *htkey;
    int count;
    int i;
    struct LinkedList *need_names;
    struct LinkedListNode *node;
    struct KeyValue *kvp;
    struct ALInstrument *instrument;

    // inst_offsets was borrowed as list container.
    need_names = (struct LinkedList *)bank->inst_offsets;

    if (need_names == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    count = need_names->count;
    
    bank->inst_count = count;

    if (count == 0)
    {
        LinkedList_free(need_names);

        TRACE_LEAVE(__func__)
        return;
    }

    // sort nodes by array index ("key" property, not "value") read from .inst file, smallest to largest.
    LinkedList_merge_sort(need_names, LinkedListNode_KeyValue_compare_smaller_key);

    bank->instruments = (struct ALInstrument **)malloc_zero(bank->inst_count, sizeof(void*));

    node = need_names->head;
    for (i=0; node != NULL; i++, node = node->next)
    {
        kvp = node->data;

        if (kvp == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: bank \"%s\" need_names invalid state, node->data is NULL\n", __func__, __LINE__, bank->text_id);
        }

        htkey = kvp->value;

        if (htkey == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: bank \"%s\" need_names invalid state, kvp->value is NULL, kvp->key=%d\n", __func__, __LINE__, bank->text_id, kvp->key);
        }

        instrument = StringHashTable_get(context->orphaned_instruments, htkey);

        if (instrument == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: context->orphaned_instruments hash table cannot resolve key \"%s\"\n", __func__, __LINE__, htkey);
        }

        bank->instruments[i] = instrument;
        ALInstrument_add_parent(instrument, bank);
        instrument->visited = 1;

        resolve_references_instrument(context, instrument);

        KeyValue_free(kvp);

        node->data = NULL;
    }

    LinkedList_free(need_names);

    bank->inst_offsets = NULL;
    
    // allocate offsets array in case writing to .ctl
    // this can't be done until it's done being borrowed above.
    bank->inst_offsets = (int32_t *)malloc_zero(bank->inst_count, sizeof(void*));

    TRACE_LEAVE(__func__)
}

/**
 * Fixes up the temporary repurposing of properties used during parsing.
 * This iterates all child objects recursively and fixes them as well.
 * This resolves all text ref ids to objects from the related "orphaned"
 * hash table.
 * @param context: context.
 * @param bank_file: bank file object to resolve dependencies.
*/
static void resolve_references(struct InstParseContext *context, struct ALBankFile *bank_file)
{
    TRACE_ENTER(__func__)

    char *htkey;
    int count;
    int i;

    count = StringHashTable_count(context->orphaned_banks);
    
    bank_file->bank_count = count;

    bank_file->banks = (struct ALBank **)malloc_zero(bank_file->bank_count, sizeof(void*));

    for (i=0; i<count; i++)
    {
        struct ALBank *bank;

        htkey = (char *)StringHashTable_peek_next_key(context->orphaned_banks);

        if (htkey == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: context->orphaned_banks hash table key count mismatch, htkey is NULL\n", __func__, __LINE__);
        }

        bank = StringHashTable_pop(context->orphaned_banks, htkey);

        if (bank == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: context->orphaned_banks hash table cannot resolve key \"%s\"\n", __func__, __LINE__, htkey);
        }

        bank_file->banks[i] = bank;

        resolve_references_bank(context, bank);
    }

    // hash table memory will be freed when context is freed.

    // allocate offsets array in case writing to .ctl
    bank_file->bank_offsets = (int32_t *)malloc_zero(bank_file->bank_count, sizeof(void*));

    TRACE_LEAVE(__func__)
}

/**
 * Reads a .inst file and parses into a bank file.
 * This allocates memory.
 * This is the public parse entry point.
 * @param fi: file info object of file to parse.
 * @returns: new bank file parsed from .inst file.
*/
struct ALBankFile *ALBankFile_new_from_inst(struct FileInfo *fi)
{
    TRACE_ENTER(__func__)

    /**
     * Debug helper, contains text of current line.
    */
    char line_buffer[MAX_FILENAME_LEN];

    /**
     * Debug helper, position in line_buffer.
    */
    int line_buffer_pos;

    /**
     * Current character read or being processed.
    */
    char c;

    /**
     * Character `c` as integer. Promoted to int to status as valid
     * or invalid (equal to int32_t -1).
    */
    int c_int;

    /**
     * Second most recent charactere read or being processed.
     * Promoted to int to status as valid
     * or invalid (equal to int32_t -1).
    */
    int previous_c;

    /**
     * Previous state of parsing finite state machine.
     * This is mostly used after finishing reading a comment
     * to know which state to return to.
    */
    int previous_state;

    /**
     * Current state of parsing finite state machine.
    */
    int state;

    /**
     * Current line number as read from .inst file.
    */
    int current_line_number;

    /**
     * Current position of .inst file in bytes.
    */
    size_t pos;

    /**
     * Total length of .inst file in bytes.
    */
    size_t len;

    /**
     * The entire .inst file is read into memory and stored
     * in this buffer.
    */
    char *file_contents;

    /**
     * Flag used in finite state machine to indicate the current state
     * should process the current context in some way.
    */
    int terminate;

    /**
     * Sanity check after parsing, counts entries in the "orphaned"
     * hash tables.
    */
    uint32_t hash_count;

    // used in sanity check error message.
    void *any_first;

    /**
     * Sometimes the current state ends but needs to understand context or delay processing
     * until the next state. For example, applying the property should happen on reading
     * a semi colon, but reading-the-value-state can abruptly end when a semi colon is read.
     * In that case, transition to the new state, but mark the character as needing to be
     * processed again.
    */
    int replay;

    // begin initial setup.

    struct InstParseContext *context = InstParseContext_new();

    memset(line_buffer, 0, MAX_FILENAME_LEN);

    // read .inst file into memory
    FileInfo_fseek(fi, 0, SEEK_SET);
    file_contents = (char *)malloc_zero(1, fi->len);
    len = FileInfo_fread(fi, file_contents, fi->len, 1);

    struct ALBankFile *bank_file = ALBankFile_new();

    c_int = -1;
    previous_c = -1;
    current_line_number = 1;
    pos = 0;
    line_buffer_pos = 0;
    state = INST_PARSE_STATE_INITIAL;
    len = fi->len;
    replay = 0;

    // done with initial setup.

    /**
     * This is the parsing finite state machine.
     * Iterate until reaching end of file.
    */
    while (pos < len)
    {
        if (replay)
        {
            replay = 0;

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("state=%d, source line=%d pos=%ld> replay previous character\n", state, current_line_number, pos);
            }
        }
        else
        {
            previous_c = c_int;

            c = file_contents[pos];

            if ((line_buffer_pos + 1) > MAX_FILENAME_LEN)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> buffer overflow (line position %d) readline line, pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, line_buffer_pos, pos, current_line_number, state);
            }

            line_buffer[line_buffer_pos] = c;
            c_int = 0xff & (int)c;

            pos++;
            line_buffer_pos++;

            // any '\n' or '\r' increments the count.
            if (is_newline(c))
            {
                current_line_number++;
                memset(line_buffer, 0, MAX_FILENAME_LEN);
                line_buffer_pos = 0;
            }
            
            // but if this is windows, adjust for "\r\n" double counting.
            if (is_windows_newline(c_int, previous_c))
            {
                current_line_number--;
            }

            context->current_line = current_line_number;
        }

        /**
         * See the graphml or svg for state transitions.
        */
        switch (state)
        {
            case INST_PARSE_STATE_INITIAL:
            {
                if (is_whitespace(c) || is_newline(c))
                {
                    // nothing to do
                }
                else if (is_comment(c))
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_COMMENT;
                }
                else if (is_alpha(c))
                {
                    buffer_append_inc(context->type_name_buffer, &context->type_name_buffer_pos, c);
                    previous_state = state;
                    state = INST_PARSE_STATE_TYPE_NAME;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }
            }
            break;

            case INST_PARSE_STATE_COMMENT:
            {
                if (is_newline(c))
                {
                    state = previous_state;
                }
                else
                {
                    // accept all.
                }
            }
            break;

            case INST_PARSE_STATE_TYPE_NAME:
            {
                terminate = 0;

                if (is_whitespace(c) || is_newline(c))
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_INITIAL_INSTANCE_NAME;

                    terminate = 1;
                }
                else if (is_alpha(c))
                {
                    buffer_append_inc(context->type_name_buffer, &context->type_name_buffer_pos, c);
                }
                else if (is_comment(c))
                {
                    // return to next state after comment
                    previous_state = INST_PARSE_STATE_INITIAL_INSTANCE_NAME;
                    state = INST_PARSE_STATE_COMMENT;

                    terminate = 1;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }

                if (terminate)
                {
                    buffer_append_inc(context->type_name_buffer, &context->type_name_buffer_pos, '\0');
                    get_type(context->type_name_buffer, context->current_type);

                    if (context->current_type->key == 0)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "%s %d> cannot determine type for \"%s\", pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, context->type_name_buffer, pos, current_line_number, state);
                    }

                    if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
                    {
                        printf("state=%d, source line=%d pos=%ld> terminate type name=\"%s\"\n", state, current_line_number, pos, context->type_name_buffer);
                    }

                    context->type_name_buffer_pos = 0;
                    memset(context->type_name_buffer, 0, IDENTIFIER_MAX_LEN);
                }
            }
            break;

            case INST_PARSE_STATE_INITIAL_INSTANCE_NAME:
            {
                if (is_whitespace(c) || is_newline(c))
                {
                    // nothing to do
                }
                else if (is_comment(c))
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_COMMENT;
                }
                else if (is_alpha(c))
                {
                    if ((context->instance_name_buffer_pos + 1) == INST_OBJ_ID_STRING_LEN)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "%s %d> buffer overflow reading instance name \"%s\", pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, context->instance_name_buffer, pos, current_line_number, state);
                    }

                    buffer_append_inc(context->instance_name_buffer, &context->instance_name_buffer_pos, c);
                    previous_state = state;
                    state = INST_PARSE_STATE_INSTANCE_NAME;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }
            }
            break;

            case INST_PARSE_STATE_INSTANCE_NAME:
            {
                terminate = 0;

                if (is_whitespace(c) || is_newline(c))
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_SEARCH_OPEN_BRACKET;

                    terminate = 1;
                }
                else if (is_alphanumeric(c))
                {
                    if ((context->instance_name_buffer_pos + 1) == INST_OBJ_ID_STRING_LEN)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "%s %d> buffer overflow reading instance name \"%s\", pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, context->instance_name_buffer, pos, current_line_number, state);
                    }

                    buffer_append_inc(context->instance_name_buffer, &context->instance_name_buffer_pos, c);
                }
                else if (c == '{')
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_INITIAL_INSTANCE_PROPERTY;

                    terminate = 1;
                }
                else if (is_comment(c))
                {
                    // return to next state after comment
                    previous_state = INST_PARSE_STATE_SEARCH_OPEN_BRACKET;
                    state = INST_PARSE_STATE_COMMENT;

                    terminate = 1;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }

                if (terminate)
                {
                    if ((context->instance_name_buffer_pos + 1) == INST_OBJ_ID_STRING_LEN)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "%s %d> buffer overflow reading instance name \"%s\", pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, context->instance_name_buffer, pos, current_line_number, state);
                    }

                    buffer_append_inc(context->instance_name_buffer, &context->instance_name_buffer_pos, '\0');

                    if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
                    {
                        printf("state=%d, source line=%d pos=%ld> terminate create instance name=\"%s\"\n", state, current_line_number, pos, context->instance_name_buffer);
                    }

                    create_instance(context);
                }
            }
            break;

            case INST_PARSE_STATE_SEARCH_OPEN_BRACKET:
            {
                if (is_whitespace(c) || is_newline(c))
                {
                    // nothing to do
                }
                else if (is_comment(c))
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_COMMENT;
                }
                else if (c == '{')
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_INITIAL_INSTANCE_PROPERTY;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }
            }
            break;

            case INST_PARSE_STATE_INITIAL_INSTANCE_PROPERTY:
            {
                terminate = 0;

                if (is_whitespace(c) || is_newline(c))
                {
                    // nothing to do
                }
                else if (is_alpha(c))
                {
                    buffer_append_inc(context->property_name_buffer, &context->property_name_buffer_pos, c);
                    previous_state = state;
                    state = INST_PARSE_STATE_PROPERTY_NAME;
                }
                else if (is_comment(c))
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_COMMENT;
                }
                else if (c == '}')
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_INITIAL;

                    terminate = 1;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }

                if (terminate)
                {
                    if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
                    {
                        printf("state=%d, source line=%d pos=%ld> terminate add instance\n", state, current_line_number, pos);
                    }

                    add_orphaned_instance(context);
                }
            }
            break;

            case INST_PARSE_STATE_PROPERTY_NAME:
            {
                /**
                 * Flag for comment state. This will swap current and previous to
                 * return to the correct state.
                */
                int terminate_is_comment = 0;
                terminate = 0;

                if (is_whitespace(c) || is_newline(c))
                {
                    terminate = 1;
                }
                else if (is_alphanumeric(c))
                {
                    buffer_append_inc(context->property_name_buffer, &context->property_name_buffer_pos, c);
                }
                else if (c == '=')
                {
                    terminate_is_comment = 0;

                    terminate = 1;
                    replay = 1;
                }
                else if (c == '(')
                {
                    terminate_is_comment = 0;

                    terminate = 1;
                    replay = 1;
                }
                else if (c == '[')
                {
                    terminate_is_comment = 0;

                    terminate = 1;
                    replay = 1;
                }
                else if (is_comment(c))
                {
                    terminate_is_comment = 1;
                    
                    terminate = 1;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }

                if (terminate)
                {
                    /**
                     * This will be the next state to transition to (or second next after comment).
                     * The challenge is that this is context dependent and can't be decided
                     * until after the current property type is known (which is what is
                     * currently being parsed).
                     * 
                     * Note that the `replay` flag simplifies the number of states to consider,
                     * otherwise (e.g.), would have to skip '=' and jump to state after that, etc.
                    */
                    int terminate_state;

                    buffer_append_inc(context->property_name_buffer, &context->property_name_buffer_pos, '\0');
                    get_property(context->current_type, context->property_name_buffer, context->current_property);

                    if (context->current_property->key == 0)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "%s %d> cannot determine property for \"%s\", pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, context->property_name_buffer, pos, current_line_number, state);
                    }

                    if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
                    {
                        printf("state=%d, source line=%d pos=%ld> terminate property name=\"%s\"\n", state, current_line_number, pos, context->property_name_buffer);
                    }

                    context->property_name_buffer_pos = 0;
                    memset(context->property_name_buffer, 0, IDENTIFIER_MAX_LEN);

                    // now that it's known what needs to happen next, set the next state.
                    if (context->current_property->type_id == TYPE_ID_INT)
                    {
                        terminate_state = INST_PARSE_STATE_EQUAL_SIGN_INT;
                    }
                    else if (context->current_property->type_id == TYPE_ID_USE_STRING)
                    {
                        terminate_state = INST_PARSE_STATE_INITIAL_FILENAME_VALUE;
                    }
                    else if (context->current_property->type_id == TYPE_ID_TEXT_REF_ID)
                    {
                        terminate_state = INST_PARSE_STATE_EQUAL_SIGN_TEXT_REF_ID;
                    }
                    else if (context->current_property->type_id == TYPE_ID_ARRAY_TEXT_REF_ID)
                    {
                        terminate_state = INST_PARSE_STATE_BEGIN_ARRAY_REF;
                    }
                    else
                    {
                        stderr_exit(
                            EXIT_CODE_GENERAL,
                            "%s %d> cannot resolve next state for property, type key=%d, property key=%d, property type_id=%d, pos=%ld, source line=%d, state=%d\n",
                            __func__,
                            __LINE__,
                            context->current_type->key,
                            context->current_property->key,
                            context->current_property->type_id,
                            pos,
                            current_line_number,
                            state);
                    }

                    if (terminate_is_comment)
                    {
                        state = INST_PARSE_STATE_COMMENT;
                        previous_state = terminate_state;
                    }
                    else
                    {
                        previous_state = state;
                        state = terminate_state;
                    }
                }
            }
            break;

            case INST_PARSE_STATE_EQUAL_SIGN_INT:
            {
                if (is_whitespace(c) || is_newline(c))
                {
                    // nothing to do
                }
                else if (c == '=')
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_INITIAL_INT_VALUE;
                }
                else if (is_comment(c))
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_COMMENT;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }
            }
            break;

            case INST_PARSE_STATE_INITIAL_INT_VALUE:
            {
                if (is_whitespace(c) || is_newline(c))
                {
                    // nothing to do
                }
                else if (is_numeric_int(c))
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_INT_VALUE;

                    buffer_append_inc(context->property_value_buffer, &context->property_value_buffer_pos, c);
                }
                else if (is_comment(c))
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_COMMENT;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }
            }
            break;

            case INST_PARSE_STATE_INT_VALUE:
            {
                terminate = 0;

                if (is_whitespace(c) || is_newline(c))
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_SEARCH_SEMI_COLON;

                    terminate = 1;
                }
                else if (is_numeric_int(c))
                {
                    buffer_append_inc(context->property_value_buffer, &context->property_value_buffer_pos, c);
                }
                else if (c == ';')
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_SEARCH_SEMI_COLON;

                    terminate = 1;
                    replay = 1;
                }
                else if (is_comment(c))
                {
                    // return to next state after comment
                    previous_state = INST_PARSE_STATE_SEARCH_SEMI_COLON;
                    state = INST_PARSE_STATE_COMMENT;

                    terminate = 1;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }

                if (terminate)
                {
                    buffer_append_inc(context->property_value_buffer, &context->property_value_buffer_pos, '\0');

                    if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
                    {
                        printf("state=%d, source line=%d pos=%ld> terminate int value=\"%s\"\n", state, current_line_number, pos, context->property_value_buffer);
                    }

                    // apply_property_on_instance will be called at end of line, once ';' is found
                }
            }
            break;

            case INST_PARSE_STATE_INITIAL_FILENAME_VALUE:
            {
                if (is_whitespace(c) || is_newline(c))
                {
                    // nothing to do
                }
                else if (is_comment(c))
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_COMMENT;
                }
                else if (c == '(')
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_SEARCH_OPEN_QUOTE;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }
            }
            break;

            case INST_PARSE_STATE_SEARCH_OPEN_QUOTE:
            {
                if (is_whitespace(c) || is_newline(c))
                {
                    // nothing to do
                }
                else if (is_comment(c))
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_COMMENT;
                }
                else if (c == '"')
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_FILENAME;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }
            }
            break;

            case INST_PARSE_STATE_FILENAME:
            {
                if (c == '"')
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_SEARCH_CLOSE_PAREN;

                    buffer_append_inc(context->property_value_buffer, &context->property_value_buffer_pos, '\0');

                    // apply_property_on_instance will be called at end of line, once ';' is found
                }
                else
                {
                    // accept all.
                    buffer_append_inc(context->property_value_buffer, &context->property_value_buffer_pos, c);
                }
            }
            break;

            case INST_PARSE_STATE_SEARCH_CLOSE_PAREN:
            {
                if (is_whitespace(c) || is_newline(c))
                {
                    // nothing to do
                }
                else if (is_comment(c))
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_COMMENT;
                }
                else if (c == ')')
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_SEARCH_SEMI_COLON;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }

                // apply_property_on_instance will be called at end of line, once ';' is found
            }
            break;

            case INST_PARSE_STATE_BEGIN_ARRAY_REF:
            {
                if (is_whitespace(c) || is_newline(c))
                {
                    // nothing to do
                }
                else if (c == '[')
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_INITIAL_ARRAY_INDEX;
                }
                else if (is_comment(c))
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_COMMENT;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }
            }
            break;

            case INST_PARSE_STATE_INITIAL_ARRAY_INDEX:
            {
                if (is_whitespace(c) || is_newline(c))
                {
                    // nothing to do
                }
                else if (is_numeric(c))
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_ARRAY_INDEX;

                    buffer_append_inc(context->array_index_value, &context->array_index_value_pos, c);
                }
                else if (is_comment(c))
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_COMMENT;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }
            }
            break;

            case INST_PARSE_STATE_ARRAY_INDEX:
            {
                terminate = 0;

                if (is_whitespace(c) || is_newline(c))
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_ARRAY_INDEX_SEARCH_CLOSE_BRACKET;

                    terminate = 1;
                }
                else if (is_numeric(c))
                {
                    buffer_append_inc(context->array_index_value, &context->array_index_value_pos, c);
                }
                else if (c == ']')
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_ARRAY_INDEX_SEARCH_CLOSE_BRACKET;

                    terminate = 1;
                    replay = 1;
                }
                else if (is_comment(c))
                {
                    // advance to search state
                    previous_state = INST_PARSE_STATE_ARRAY_INDEX_SEARCH_CLOSE_BRACKET;
                    state = INST_PARSE_STATE_COMMENT;

                    terminate = 1;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }

                if (terminate)
                {
                    buffer_append_inc(context->array_index_value, &context->array_index_value_pos, '\0');

                    set_array_index_int(context);

                    if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
                    {
                        printf("state=%d, source line=%d pos=%ld> array_index_value=\"%s\"\n", state, current_line_number, pos, context->array_index_value);
                    }
                }
            }
            break;

            case INST_PARSE_STATE_ARRAY_INDEX_SEARCH_CLOSE_BRACKET:
            {
                if (is_whitespace(c) || is_newline(c))
                {
                    // nothing to do
                }
                else if (c == ']')
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_EQUAL_SIGN_TEXT_REF_ID;
                }
                else if (is_comment(c))
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_COMMENT;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }
            }
            break;

            case INST_PARSE_STATE_EQUAL_SIGN_TEXT_REF_ID:
            {
                if (is_whitespace(c) || is_newline(c))
                {
                    // nothing to do
                }
                else if (c == '=')
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_INITIAL_TEXT_REF_ID;
                }
                else if (is_comment(c))
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_COMMENT;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }
            }
            break;

            case INST_PARSE_STATE_INITIAL_TEXT_REF_ID:
            {
                if (is_whitespace(c) || is_newline(c))
                {
                    // nothing to do
                }
                else if (is_alpha(c))
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_TEXT_REF_ID;

                    buffer_append_inc(context->property_value_buffer, &context->property_value_buffer_pos, c);
                }
                else if (is_comment(c))
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_COMMENT;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }
            }
            break;

            case INST_PARSE_STATE_TEXT_REF_ID:
            {
                terminate = 0;

                if (is_whitespace(c) || is_newline(c))
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_SEARCH_SEMI_COLON;

                    terminate = 1;
                }
                else if (is_alphanumeric(c))
                {
                    buffer_append_inc(context->property_value_buffer, &context->property_value_buffer_pos, c);
                }
                else if (is_comment(c))
                {
                    // return to next state
                    previous_state = INST_PARSE_STATE_SEARCH_SEMI_COLON;
                    state = INST_PARSE_STATE_COMMENT;

                    terminate = 1;
                }
                else if (c == ';')
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_SEARCH_SEMI_COLON;

                    terminate = 1;
                    replay = 1;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }

                if (terminate)
                {
                    buffer_append_inc(context->property_value_buffer, &context->property_value_buffer_pos, '\0');

                    if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
                    {
                        printf("state=%d, source line=%d pos=%ld> terminate property ref id=\"%s\"\n", state, current_line_number, pos, context->property_value_buffer);
                    }

                    // apply_property_on_instance will be called at end of line, once ';' is found
                }
            }
            break;

            case INST_PARSE_STATE_SEARCH_SEMI_COLON:
            {
                terminate = 0;

                if (is_whitespace(c) || is_newline(c))
                {
                    // nothing to do
                }
                else if (is_comment(c))
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_COMMENT;
                }
                else if (c == ';')
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_INITIAL_INSTANCE_PROPERTY;

                    terminate = 1;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }

                if (terminate)
                {
                    if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
                    {
                        printf("state=%d, source line=%d pos=%ld> terminate (;)\n", state, current_line_number, pos);
                    }

                    apply_property_on_instance(context);
                }
            }
            break;

            default:
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> Invalid parse state: %d\n", __func__, __LINE__, state);
        }
    }

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("exit parse state machine\n");
    }

    // Done with reading file, can release memory.
    free(file_contents);

    /**
     * Iterate all the "orphaned" hash tables and resolve text ref id
     * to actual instances. This also fixes up properties that were
     * borrowed during parsing.
    */
    resolve_references(context, bank_file);

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("Finish resolving references.\n");
    }

    /**
     * sanity check. Make sure all orphaned instances were used.
    */

    // banks are _pop from hashtable, so this should be empty.
    hash_count = StringHashTable_count(context->orphaned_banks);
    if (hash_count > 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> error, finished parsing file but there are %d unclaimed banks\n", __func__, __LINE__, hash_count);
    }

    /**
     * instruments, sounds, envelope, and keymaps are only _get from hashtable,
     * as these can be referenced more than once.
     * Check that each item in these hash tables is marked as `visited` (used by something).
    */

    if (StringHashTable_any(context->orphaned_instruments, StringHash_instrument_unvisited, &any_first))
    {
        struct ALInstrument *instrument = (struct ALInstrument *)any_first;

        stderr_exit(EXIT_CODE_GENERAL, "%s %d> error, finished parsing file but there is at least one unclaimed instrument, %s\n", __func__, __LINE__, instrument->text_id);
    }

    if (StringHashTable_any(context->orphaned_sounds, StringHash_sound_unvisited, &any_first))
    {
        struct ALSound *sound = (struct ALSound *)any_first;

        stderr_exit(EXIT_CODE_GENERAL, "%s %d>, finished parsing file but there is at least one unclaimed sound, %s\n", __func__, __LINE__, sound->text_id);
    }

    if (StringHashTable_any(context->orphaned_keymaps, StringHash_keymap_unvisited, &any_first))
    {
        struct ALKeyMap *keymap = (struct ALKeyMap *)any_first;

        stderr_exit(EXIT_CODE_GENERAL, "%s %d> error, finished parsing file but there is at least one unclaimed keymap, %s\n", __func__, __LINE__, keymap->text_id);
    }

    if (StringHashTable_any(context->orphaned_envelopes, StringHash_envelope_unvisited, &any_first))
    {
        struct ALEnvelope *envelope = (struct ALEnvelope *)any_first;

        stderr_exit(EXIT_CODE_GENERAL, "%s %d> error, finished parsing file but there is at least one unclaimed envelope, %s\n", __func__, __LINE__, envelope->text_id);
    }

    /**
     * When an item is resolved from these `missing` hash tables it gets
     * removed, so these should all be empty.
    */

    hash_count = IntHashTable_count(context->sound_missing_wavetable);
    if (hash_count > 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> error, there are %d sounds with unresolved references to wavetable\n", __func__, __LINE__, hash_count);
    }

    hash_count = IntHashTable_count(context->sound_missing_envelope);
    if (hash_count > 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> error, there are %d sounds with unresolved references to envelope\n", __func__, __LINE__, hash_count);
    }

    hash_count = IntHashTable_count(context->sound_missing_keymap);
    if (hash_count > 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> error, there are %d sounds with unresolved references to keymap\n", __func__, __LINE__, hash_count);
    }

    // done with final sanity check

    /**
     * Done parsing, release context.
    */
    InstParseContext_free(context);

    TRACE_LEAVE(__func__)

    return bank_file;
}

