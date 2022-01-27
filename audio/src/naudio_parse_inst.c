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
#include "llist.h"
#include "naudio.h"

#define IDENTIFIER_MAX_LEN 50

inline static int is_whitespace(char c) ATTR_INLINE ;
inline static int is_newline(char c) ATTR_INLINE ;
inline static int is_alpha(char c) ATTR_INLINE ;
inline static int is_alphanumeric(char c) ATTR_INLINE ;
inline static int is_numeric(char c) ATTR_INLINE ;
inline static int is_numeric_int(char c) ATTR_INLINE ;
inline static int is_comment(char c) ATTR_INLINE ;

struct TypeInfo {
    int key;
    char *value;
    int type_id;
};

struct RuntimeTypeInfo {
    int key;
    int type_id;
};

struct KeyValue {
    int key;
    char *value;
};

struct InstParseContext {
    struct RuntimeTypeInfo *current_type;
    struct RuntimeTypeInfo *current_property;

    char *type_name_buffer;
    int type_name_buffer_pos;
    char *instance_name_buffer;
    int instance_name_buffer_pos;
    char *property_name_buffer;
    int property_name_buffer_pos;
    char *array_index_value;
    int array_index_value_pos;
    char *property_value_buffer;
    int property_value_buffer_pos;

    void *current_instance;

    int array_index_int;
    int current_value_int;

    struct StringHashTable *orphaned_banks;
    struct StringHashTable *orphaned_instruments;
    struct StringHashTable *orphaned_sounds;
    struct StringHashTable *orphaned_keymaps;
    struct StringHashTable *orphaned_envelopes;
};

enum TYPE_ID {
    TYPE_ID_NONE = 0,
    TYPE_ID_INT = 1,
    TYPE_ID_USE_STRING,
    TYPE_ID_TEXT_REF_ID,
    TYPE_ID_ARRAY_TEXT_REF_ID,
};

enum BANK_CLASS_TYPE {
    INST_TYPE_DEFAULT_UNKNOWN = 0,
    INST_TYPE_BANK = 1,
    INST_TYPE_INSTRUMENT,
    INST_TYPE_SOUND,
    INST_TYPE_KEYMAP,
    INST_TYPE_ENVELOPE
};

enum InstBankPropertyId {
    INST_BANK_PROPERTY_DEFAULT_UNKNOWN = 0,
    INST_BANK_PROPERTY_INSTRUMENT_ARR_ENTRY = 1
};

enum InstInstrumentPropertyId {
    INST_INSTRUMENT_PROPERTY_DEFAULT_UNKNOWN = 0,
    INST_INSTRUMENT_PROPERTY_VOLUME = 1,
    INST_INSTRUMENT_PROPERTY_PAN,
    INST_INSTRUMENT_PROPERTY_PRIORITY,
    INST_INSTRUMENT_PROPERTY_BENDRANGE,
    INST_INSTRUMENT_PROPERTY_SOUND_ARR_ENTRY
};

enum InstSoundPropertyId {
    INST_SOUND_PROPERTY_DEFAULT_UNKNOWN = 0,
    INST_SOUND_PROPERTY_USE = 1,
    INST_SOUND_PROPERTY_PAN,
    INST_SOUND_PROPERTY_VOLUME,
    INST_SOUND_PROPERTY_ENVELOPE,
    INST_SOUND_PROPERTY_KEYMAP
};

enum InstKeyMapPropertyId {
    INST_KEYMAP_PROPERTY_DEFAULT_UNKNOWN = 0,
    INST_KEYMAP_PROPERTY_VELOCITY_MIN = 1,
    INST_KEYMAP_PROPERTY_VELOCITY_MAX,
    INST_KEYMAP_PROPERTY_KEY_MIN,
    INST_KEYMAP_PROPERTY_KEY_MAX,
    INST_KEYMAP_PROPERTY_KEY_BASE,
    INST_KEYMAP_PROPERTY_DETUNE
};

enum InstEnvelopePropertyId {
    INST_ENVELOPE_PROPERTY_DEFAULT_UNKNOWN = 0,
    INST_ENVELOPE_PROPERTY_ATTACK_TIME = 1,
    INST_ENVELOPE_PROPERTY_ATTACK_VOLUME,
    INST_ENVELOPE_PROPERTY_DECAY_TIME,
    INST_ENVELOPE_PROPERTY_DECAY_VOLUME,
    INST_ENVELOPE_PROPERTY_RELEASE_TIME
};

static struct TypeInfo InstTypes[] = {
    { INST_TYPE_BANK, "bank", TYPE_ID_NONE },
    { INST_TYPE_INSTRUMENT, "instrument", TYPE_ID_NONE },
    { INST_TYPE_SOUND, "sound", TYPE_ID_NONE },
    { INST_TYPE_KEYMAP, "keymap", TYPE_ID_NONE },
    { INST_TYPE_ENVELOPE, "envelope", TYPE_ID_NONE }
};
static const int InstTypes_len = ARRAY_LENGTH(InstTypes);

static struct TypeInfo InstBankProperties[] = {
    { INST_BANK_PROPERTY_INSTRUMENT_ARR_ENTRY, "instrument", TYPE_ID_ARRAY_TEXT_REF_ID }
};
static const int InstBankProperties_len = ARRAY_LENGTH(InstBankProperties);

static struct TypeInfo InstInstrumentProperties[] = {
    { INST_INSTRUMENT_PROPERTY_VOLUME, "volume", TYPE_ID_INT },
    { INST_INSTRUMENT_PROPERTY_PAN, "pan", TYPE_ID_INT },
    { INST_INSTRUMENT_PROPERTY_PRIORITY, "priority", TYPE_ID_INT },
    { INST_INSTRUMENT_PROPERTY_BENDRANGE, "bendRange", TYPE_ID_INT },
    { INST_INSTRUMENT_PROPERTY_SOUND_ARR_ENTRY, "sound", TYPE_ID_ARRAY_TEXT_REF_ID }
};
static const int InstInstrumentProperties_len = ARRAY_LENGTH(InstInstrumentProperties);

static struct TypeInfo InstSoundProperties[] = {
    { INST_SOUND_PROPERTY_USE, "use", TYPE_ID_USE_STRING },
    { INST_SOUND_PROPERTY_PAN, "pan", TYPE_ID_INT },
    { INST_SOUND_PROPERTY_VOLUME, "volume", TYPE_ID_INT },
    { INST_SOUND_PROPERTY_ENVELOPE, "envelope", TYPE_ID_TEXT_REF_ID },
    { INST_SOUND_PROPERTY_KEYMAP, "keymap", TYPE_ID_TEXT_REF_ID }
};
static const int InstSoundProperties_len = ARRAY_LENGTH(InstSoundProperties);

static struct TypeInfo InstKeyMapProperties[] = {
    { INST_KEYMAP_PROPERTY_VELOCITY_MIN, "velocityMin", TYPE_ID_INT },
    { INST_KEYMAP_PROPERTY_VELOCITY_MAX, "velocityMax", TYPE_ID_INT },
    { INST_KEYMAP_PROPERTY_KEY_MIN, "keyMin", TYPE_ID_INT },
    { INST_KEYMAP_PROPERTY_KEY_MAX, "keyMax", TYPE_ID_INT },
    { INST_KEYMAP_PROPERTY_KEY_BASE, "keyBase", TYPE_ID_INT },
    { INST_KEYMAP_PROPERTY_DETUNE, "detune", TYPE_ID_INT }
};
static const int InstKeyMapProperties_len = ARRAY_LENGTH(InstKeyMapProperties);

static struct TypeInfo InstEnvelopeProperties[] = {
    { INST_ENVELOPE_PROPERTY_ATTACK_TIME, "attackTime", TYPE_ID_INT },
    { INST_ENVELOPE_PROPERTY_ATTACK_VOLUME, "attackVolume", TYPE_ID_INT },
    { INST_ENVELOPE_PROPERTY_DECAY_TIME, "decayTime", TYPE_ID_INT },
    { INST_ENVELOPE_PROPERTY_DECAY_VOLUME, "decayVolume", TYPE_ID_INT },
    { INST_ENVELOPE_PROPERTY_RELEASE_TIME, "releaseTime", TYPE_ID_INT }
};
static const int InstEnvelopeProperties_len = ARRAY_LENGTH(InstEnvelopeProperties);

enum InstParseState {
    INST_PARSE_STATE_INITIAL = 1,
    INST_PARSE_STATE_COMMENT,
    INST_PARSE_STATE_TYPE_NAME,
    INST_PARSE_STATE_INITIAL_INSTANCE_NAME,
    INST_PARSE_STATE_INSTANCE_NAME,
    INST_PARSE_STATE_SEARCH_OPEN_BRACKET,
    INST_PARSE_STATE_INITIAL_INSTANCE_PROPERTY,
    INST_PARSE_STATE_PROPERTY_NAME,
    INST_PARSE_STATE_EQUAL_SIGN_INT,
    INST_PARSE_STATE_INITIAL_INT_VALUE,
    INST_PARSE_STATE_INT_VALUE,
    INST_PARSE_STATE_INITIAL_FILENAME_VALUE,
    INST_PARSE_STATE_SEARCH_OPEN_QUOTE,
    INST_PARSE_STATE_FILENAME,
    INST_PARSE_STATE_SEARCH_CLOSE_PAREN,
    INST_PARSE_STATE_SEARCH_SEMI_COLON,
    INST_PARSE_STATE_BEGIN_ARRAY_REF,
    INST_PARSE_STATE_INITIAL_ARRAY_INDEX,
    INST_PARSE_STATE_ARRAY_INDEX,
    INST_PARSE_STATE_ARRAY_INDEX_SEARCH_CLOSE_BRACKET,
    INST_PARSE_STATE_EQUAL_SIGN_TEXT_REF_ID,
    INST_PARSE_STATE_INITIAL_TEXT_REF_ID,
    INST_PARSE_STATE_TEXT_REF_ID,

    INST_PARSE_STATE_EOF,

    INST_PARSE_STATE_ERROR,
};

inline static int is_whitespace(char c)
{
    return c == ' ' || c == '\t';
}

inline static int is_newline(char c)
{
    return c == '\r' || c == '\n';
}

static int is_windows_newline(int c, int previous_c)
{
    if (c == '\n' && previous_c == '\r')
    {
        return 1;
    }

    return 0;
}

inline static int is_alpha(char c)
{
    return  (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
}

inline static int is_alphanumeric(char c)
{
    return  (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_';
}

inline static int is_numeric(char c)
{
    return  (c >= '0' && c <= '9');
}

inline static int is_numeric_int(char c)
{
    return  (c >= '0' && c <= '9') || c == 'x' || c == 'X' || c == '-';
}

inline static int is_comment(char c)
{
    return c == '#';
}

struct KeyValue *KeyValue_new()
{
    TRACE_ENTER(__func__)

    struct KeyValue *p = (struct KeyValue *)malloc_zero(1, sizeof(struct KeyValue));

    TRACE_LEAVE(__func__)

    return p;
}

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
 * Merge sort comparison function.
 * Use this to sort smallest to largest.
 * @param first: first node
 * @param second: second node
 * @returns: comparison result
*/
int llist_node_KeyValue_compare_smaller_key(struct llist_node *first, struct llist_node *second)
{
    TRACE_ENTER(__func__)

    int ret;

    if (first == NULL && second == NULL)
    {
        ret = 0;
    }
    else if (first == NULL && second != NULL)
    {
        ret = 1;
    }
    else if (first != NULL && second == NULL)
    {
        ret = -1;
    }
    else
    {
        struct KeyValue *kvp_first = (struct KeyValue *)first->data;
        struct KeyValue *kvp_second = (struct KeyValue *)second->data;
       
        if (kvp_first == NULL && kvp_second == NULL)
        {
            ret = 0;
        }
        else if (kvp_first == NULL && kvp_second != NULL)
        {
            ret = 1;
        }
        else if (kvp_first != NULL && kvp_second == NULL)
        {
            ret = -1;
        }
        else
        {
            if (kvp_first->key < kvp_second->key)
            {
                ret = -1;
            }
            else if (kvp_first->key > kvp_second->key)
            {
                ret = 1;
            }
            else
            {
                ret = 0;
            }
        }
    }

    TRACE_LEAVE(__func__)

    return ret;
}

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

    TRACE_LEAVE(__func__)

    return context;
}

static void InstParseContext_free(struct InstParseContext *context)
{
    TRACE_ENTER(__func__)

    StringHashTable_free(context->orphaned_banks);
    StringHashTable_free(context->orphaned_instruments);
    StringHashTable_free(context->orphaned_sounds);
    StringHashTable_free(context->orphaned_keymaps);
    StringHashTable_free(context->orphaned_envelopes);

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

static void buffer_append_inc(char *buffer, int *position, char c)
{
    buffer[*position] = c;
    *position = *position + 1;
}

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

void set_array_index_int(struct InstParseContext *context)
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
            stderr_exit(EXIT_CODE_GENERAL, "error (range), cannot parse context->array_index_value as integer: %s\n", context->array_index_value);
        }

        context->array_index_int = val;
    }
    else
    {
        stderr_exit(EXIT_CODE_GENERAL, "error, cannot parse context->array_index_value as integer: %s\n", context->array_index_value);
    }

    TRACE_LEAVE(__func__)
}

void set_current_property_value_int(struct InstParseContext *context)
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
            stderr_exit(EXIT_CODE_GENERAL, "error (range), cannot parse context->property_value_buffer as integer: %s\n", context->property_value_buffer);
        }

        context->current_value_int = val;
    }
    else
    {
        stderr_exit(EXIT_CODE_GENERAL, "error, cannot parse context->property_value_buffer as integer: %s\n", context->property_value_buffer);
    }

    TRACE_LEAVE(__func__)
}

void create_instance(struct InstParseContext *context)
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

void apply_property_on_instance_bank(struct InstParseContext *context)
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
            struct llist_node *node;
            struct KeyValue *kvp;

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("append on bank id=%d [%s]=\"%s\"\n", bank->id, context->array_index_value, context->property_value_buffer);
            }

            // borrow inst_offsets property to store list of references.
            if (bank->inst_offsets == NULL)
            {
                bank->inst_offsets = (void *)llist_root_new();
            }

            kvp = KeyValue_new_value(context->property_value_buffer);
            kvp->key = context->array_index_int;
            node = llist_node_new();
            node->data = kvp;
            llist_root_append_node((struct llist_root*)bank->inst_offsets, node);
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
            struct llist_node *node;
            struct KeyValue *kvp;

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("append on instrument id=%d [%s]=\"%s\"\n", instrument->id, context->array_index_value, context->property_value_buffer);
            }
            
            // borrow sound_offsets property to store list of references.
            if (instrument->sound_offsets == NULL)
            {
                instrument->sound_offsets = (void *)llist_root_new();
            }

            kvp = KeyValue_new_value(context->property_value_buffer);
            kvp->key = context->array_index_int;
            node = llist_node_new();
            node->data = kvp;
            llist_root_append_node((struct llist_root*)instrument->sound_offsets, node);
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

            struct ALWaveTable *wavetable = ALWaveTable_new();
            
            // includes space for terminating zero
            wavetable->aifc_path = (char*)malloc_zero(1, context->property_value_buffer_pos);
            memcpy(wavetable->aifc_path, context->property_value_buffer, (size_t)context->property_value_buffer_pos);

            sound->wavetable = wavetable;
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

            // includes space for terminating zero
            char *ref_id = (char*)malloc_zero(1, context->property_value_buffer_pos);
            memcpy(ref_id, context->property_value_buffer, (size_t)context->property_value_buffer_pos);

            // borrow envelope pointer property to store text ref string
            sound->envelope = (struct ALEnvelope *)(void*)ref_id;
        }
        break;

        case INST_SOUND_PROPERTY_KEYMAP:
        {
            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set sound id=%d keymap=\"%s\"\n", sound->id, context->property_value_buffer);
            }

            // includes space for terminating zero
            char *ref_id = (char*)malloc_zero(1, context->property_value_buffer_pos);
            memcpy(ref_id, context->property_value_buffer, (size_t)context->property_value_buffer_pos);

            // borrow keymap pointer property to store text ref string
            sound->keymap = (struct ALKeyMap *)(void*)ref_id;
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

        default:
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d>: context->current_property->key not supported: %d\n", __func__, __LINE__, context->current_property->key);
        }
        break;
    }

    TRACE_LEAVE(__func__)
}

void apply_property_on_instance_envelope(struct InstParseContext *context)
{
    TRACE_ENTER(__func__)

    struct ALEnvelope *envelope = (struct ALEnvelope *)context->current_instance;

    if (envelope == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: context->current_instance is NULL\n", __func__, __LINE__);
    }

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

        default:
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d>: context->current_property->key not supported: %d\n", __func__, __LINE__, context->current_property->key);
        }
        break;
    }

    TRACE_LEAVE(__func__)
}

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

void add_orphaned_instance(struct InstParseContext *context)
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

static void resolve_references_sound(struct InstParseContext *context, struct ALSound *sound)
{
    TRACE_ENTER(__func__)

    char *htkey;

    // borrow envelope pointer property to store text ref string
    htkey = (char *)(void*)sound->envelope;

    if (htkey != NULL)
    {
        struct ALEnvelope *envelope = StringHashTable_pop(context->orphaned_envelopes, htkey);

        if (envelope == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: context->orphaned_envelopes hash table cannot resolve key \"%s\"\n", __func__, __LINE__, htkey);
        }

        // free temp ref string
        free(sound->envelope);

        sound->envelope = envelope;
    }

    // borrow keymap pointer property to store text ref string
    htkey = (char *)(void*)sound->keymap;

    if (htkey != NULL)
    {
        struct ALKeyMap *keymap = StringHashTable_pop(context->orphaned_keymaps, htkey);

        if (keymap == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: context->orphaned_keymaps hash table cannot resolve key \"%s\"\n", __func__, __LINE__, htkey);
        }

        // free temp ref string
        free(sound->keymap);

        sound->keymap = keymap;
    }

    TRACE_LEAVE(__func__)
}

static void resolve_references_instrument(struct InstParseContext *context, struct ALInstrument *instrument)
{
    TRACE_ENTER(__func__)

    char *htkey;
    int count;
    int i;
    struct llist_root *need_names;
    struct llist_node *node;
    struct KeyValue *kvp;
    struct ALSound *sound;

    // sound_offsets was borrowed as list container.
    need_names = (struct llist_root *)instrument->sound_offsets;

    if (need_names == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    count = need_names->count;
    
    instrument->sound_count = count;

    if (count == 0)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    // sort nodes by array index read from .inst file, smallest to largest.
    llist_root_merge_sort(need_names, llist_node_KeyValue_compare_smaller_key);

    instrument->sounds = (struct ALSound **)malloc_zero(instrument->sound_count, sizeof(void*));

    node = need_names->root;
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

        sound = StringHashTable_pop(context->orphaned_sounds, htkey);

        if (sound == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: context->orphaned_sounds hash table cannot resolve key \"%s\"\n", __func__, __LINE__, htkey);
        }

        instrument->sounds[i] = sound;

        resolve_references_sound(context, sound);

        KeyValue_free(kvp);

        node->data = NULL;
    }

    llist_node_root_free(need_names);

    instrument->sound_offsets = NULL;

    TRACE_LEAVE(__func__)
}

static void resolve_references_bank(struct InstParseContext *context, struct ALBank *bank)
{
    TRACE_ENTER(__func__)

    char *htkey;
    int count;
    int i;
    struct llist_root *need_names;
    struct llist_node *node;
    struct KeyValue *kvp;
    struct ALInstrument *instrument;

    // inst_offsets was borrowed as list container.
    need_names = (struct llist_root *)bank->inst_offsets;

    if (need_names == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    count = need_names->count;
    
    bank->inst_count = count;

    if (count == 0)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    // sort nodes by array index read from .inst file, smallest to largest.
    llist_root_merge_sort(need_names, llist_node_KeyValue_compare_smaller_key);

    bank->instruments = (struct ALInstrument **)malloc_zero(bank->inst_count, sizeof(void*));

    node = need_names->root;
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

        instrument = StringHashTable_pop(context->orphaned_instruments, htkey);

        if (instrument == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: context->orphaned_instruments hash table cannot resolve key \"%s\"\n", __func__, __LINE__, htkey);
        }

        bank->instruments[i] = instrument;

        resolve_references_instrument(context, instrument);

        KeyValue_free(kvp);

        node->data = NULL;
    }

    llist_node_root_free(need_names);

    bank->inst_offsets = NULL;

    TRACE_LEAVE(__func__)
}

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

    TRACE_LEAVE(__func__)
}

struct ALBankFile *ALBankFile_new_from_inst(struct file_info *fi)
{
    TRACE_ENTER(__func__)

    char line_buffer[MAX_FILENAME_LEN];
    int line_buffer_pos;
    char c;
    int c_int;
    int previous_c;
    int previous_state;
    int state;
    int current_line_number;
    size_t pos;
    size_t len;
    char *file_contents;
    int terminate;
    uint32_t hash_count;

    /**
     * Sometimes the current state ends but needs to understand context or delay processing
     * until the next state. For example, applying the property should happen on reading
     * a semi colon, but reading-the-value-state can abruptly end when a semi colon is read.
     * In that case, transition to the new state, but mark the character as needing to be
     * processed again.
    */
    int replay;

    struct InstParseContext *context = InstParseContext_new();

    memset(line_buffer, 0, MAX_FILENAME_LEN);

    file_info_fseek(fi, 0, SEEK_SET);

    file_contents = (char *)malloc_zero(1, fi->len);
    len = file_info_fread(fi, file_contents, fi->len, 1);

    struct ALBankFile *bank_file = ALBankFile_new();

    c_int = -1;
    previous_c = -1;
    current_line_number = 0;
    pos = 0;
    line_buffer_pos = 0;
    state = INST_PARSE_STATE_INITIAL;
    len = fi->len;
    replay = 0;

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
        }

        // if (pos == 277)
        // {
        //     // debug breakpoint
        //     int aaa = 123;
        // }

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
                // flag for comment state
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

                    // now that the it's known what needs to happen next, set the next state.
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
            stderr_exit(EXIT_CODE_GENERAL, "Invalid parse state: %d\n", state);
        }
    }

    free(file_contents);

    resolve_references(context, bank_file);

    // sanity check

    hash_count = StringHashTable_count(context->orphaned_banks);

    if (hash_count > 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "error, finished parsing file but there are %d unclaimed banks\n", hash_count);
    }

    hash_count = StringHashTable_count(context->orphaned_instruments);

    if (hash_count > 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "error, finished parsing file but there are %d unclaimed instruments\n", hash_count);
    }

    hash_count = StringHashTable_count(context->orphaned_sounds);

    if (hash_count > 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "error, finished parsing file but there are %d unclaimed sounds\n", hash_count);
    }

    hash_count = StringHashTable_count(context->orphaned_keymaps);

    if (hash_count > 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "error, finished parsing file but there are %d unclaimed keymaps\n", hash_count);
    }

    hash_count = StringHashTable_count(context->orphaned_envelopes);

    if (hash_count > 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "error, finished parsing file but there are %d unclaimed envelopes\n", hash_count);
    }

    // done with final sanity check

    InstParseContext_free(context);

    TRACE_LEAVE(__func__)

    return bank_file;
}

