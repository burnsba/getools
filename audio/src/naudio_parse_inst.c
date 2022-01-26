#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#include "debug.h"
#include "common.h"
#include "machine_config.h"
#include "utility.h"
#include "naudio.h"

/**
 * TODO:
 * add properties to hash tables
 * iterate hash tables into bank
*/

#define IDENTIFIER_MAX_LEN 50

inline static int is_whitespace(char c) ATTR_INLINE ;
inline static int is_newline(char c) ATTR_INLINE ;
inline static int is_alpha(char c) ATTR_INLINE ;
inline static int is_alphanumeric(char c) ATTR_INLINE ;
inline static int is_numeric(char c) ATTR_INLINE ;
inline static int is_numeric_int(char c) ATTR_INLINE ;
inline static int is_comment(char c) ATTR_INLINE ;

static struct TypeInfo {
    int key;
    char *value;
    int type_id;
};

static struct InstParseContext {
    struct TypeInfo *current_type;
    struct TypeInfo *current_property;

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

    int current_value_int;

    struct StringHash *orphaned_banks;
    struct StringHash *orphaned_instruments;
    struct StringHash *orphaned_sounds;
    struct StringHash *orphaned_keymaps;
    struct StringHash *orphaned_envelopes;
};

static enum TYPE_ID {
    TYPE_ID_NONE = 0,
    TYPE_ID_INT = 1,
    TYPE_ID_STRING,
    TYPE_ID_TEXT_REF_ID
};

static enum InstTypeId {
    INST_TYPE_DEFAULT_UNKNOWN = 0,
    INST_TYPE_BANK = 1,
    INST_TYPE_INSTRUMENT,
    INST_TYPE_SOUND,
    INST_TYPE_KEYMAP,
    INST_TYPE_ENVELOPE
};

static enum InstBankPropertyId {
    INST_BANK_PROPERTY_DEFAULT_UNKNOWN = 0,
    INST_BANK_PROPERTY_INSTRUMENT_ARR_ENTRY = 1
};

static enum InstInstrumentPropertyId {
    INST_INSTRUMENT_PROPERTY_DEFAULT_UNKNOWN = 0,
    INST_INSTRUMENT_PROPERTY_VOLUME = 1,
    INST_INSTRUMENT_PROPERTY_PAN,
    INST_INSTRUMENT_PROPERTY_PRIORITY,
    INST_INSTRUMENT_PROPERTY_BENDRANGE,
    INST_INSTRUMENT_PROPERTY_SOUND_ARR_ENTRY
};

static enum InstSoundPropertyId {
    INST_SOUND_PROPERTY_DEFAULT_UNKNOWN = 0,
    INST_SOUND_PROPERTY_USE = 1,
    INST_SOUND_PROPERTY_PAN,
    INST_SOUND_PROPERTY_VOLUME,
    INST_SOUND_PROPERTY_ENVELOPE,
    INST_SOUND_PROPERTY_KEYMAP
};

static enum InstKeyMapPropertyId {
    INST_KEYMAP_PROPERTY_DEFAULT_UNKNOWN = 0,
    INST_KEYMAP_PROPERTY_VELOCITY_MIN = 1,
    INST_KEYMAP_PROPERTY_VELOCITY_MAX,
    INST_KEYMAP_PROPERTY_KEY_MIN,
    INST_KEYMAP_PROPERTY_KEY_MAX,
    INST_KEYMAP_PROPERTY_KEY_BASE,
    INST_KEYMAP_PROPERTY_DETUNE
};

static enum InstEnvelopePropertyId {
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
    { INST_BANK_PROPERTY_INSTRUMENT_ARR_ENTRY, "instrument", TYPE_ID_TEXT_REF_ID }
};
static const int InstBankProperties_len = ARRAY_LENGTH(InstBankProperties);

static struct TypeInfo InstInstrumentProperties[] = {
    { INST_INSTRUMENT_PROPERTY_VOLUME, "volume", TYPE_ID_INT },
    { INST_INSTRUMENT_PROPERTY_PAN, "pan", TYPE_ID_INT },
    { INST_INSTRUMENT_PROPERTY_PRIORITY, "priority", TYPE_ID_INT },
    { INST_INSTRUMENT_PROPERTY_BENDRANGE, "bendRange", TYPE_ID_INT },
    { INST_INSTRUMENT_PROPERTY_SOUND_ARR_ENTRY, "sound", TYPE_ID_TEXT_REF_ID }
};
static const int InstInstrumentProperties_len = ARRAY_LENGTH(InstInstrumentProperties);

static struct TypeInfo InstSoundProperties[] = {
    { INST_SOUND_PROPERTY_USE, "use", TYPE_ID_STRING },
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

static enum InstParseState {
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
    INST_PARSE_STATE_EQUAL_SIGN_ARRAY_REF,
    INST_PARSE_STATE_ARRAY_INITIAL_TEXT_REF_ID,
    INST_PARSE_STATE_ARRAY_TEXT_REF_ID,

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
    if (c == '\r' && previous_c == '\n')
    {
        return 1;
    }

    return 0;
}

inline static int is_alpha(char c)
{
    return  (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
}

inline static int is_alphanumeric(char c)
{
    return  (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9');
}

inline static int is_numeric(char c)
{
    return  (c >= '0' && c <= '9');
}

inline static int is_numeric_int(char c)
{
    return  (c >= '0' && c <= '9') || c == 'x' || c == 'X';
}

inline static int is_comment(char c)
{
    return c == '#';
}

static void buffer_append_inc(char *buffer, int *position, char c)
{
    buffer[*position] = c;
    *position = *position + 1;
}

static void get_type(const char *type_name, struct TypeInfo *type)
{
    TRACE_ENTER(__func__)

    type->key = 0;
    memset(type->value, 0, MAX_FILENAME_LEN);

    int i;
    for (i=0; i<InstTypes_len; i++)
    {
        if (strcmp(type_name, InstTypes[i].value) == 0)
        {
            type->key = InstTypes[i].key;
            strncpy(type->value, InstTypes[i].value, MAX_FILENAME_LEN);

            TRACE_LEAVE(__func__)
            return;
        }
    }

    TRACE_LEAVE(__func__)
}

static void get_property(struct TypeInfo *type, const char *property_name, struct TypeInfo *property)
{
    TRACE_ENTER(__func__)

    property->key = 0;
    memset(property->value, 0, MAX_FILENAME_LEN);

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
                    strncpy(property->value, InstBankProperties[i].value, MAX_FILENAME_LEN);

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
                    strncpy(property->value, InstInstrumentProperties[i].value, MAX_FILENAME_LEN);

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
                    strncpy(property->value, InstSoundProperties[i].value, MAX_FILENAME_LEN);

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
                    strncpy(property->value, InstKeyMapProperties[i].value, MAX_FILENAME_LEN);

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
                    strncpy(property->value, InstEnvelopeProperties[i].value, MAX_FILENAME_LEN);

                    TRACE_LEAVE(__func__)
                    return;
                }
            }
        }
        break;
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
            context->current_instance = (struct ALBank *)ALBank_new();
        }
        break;

        case INST_TYPE_INSTRUMENT:
        {
            context->current_instance = (struct ALInstrument *)ALInstrument_new();
        }
        break;

        case INST_TYPE_SOUND:
        {
            context->current_instance = (struct ALSound *)ALSound_new();
        }
        break;

        case INST_TYPE_KEYMAP:
        {
            context->current_instance = (struct ALKeyMap *)ALKeyMap_new();
        }
        break;

        case INST_TYPE_ENVELOPE:
        {
            context->current_instance = (struct ALEnvelope *)ALEnvelope_new();
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

    set_current_property_value_int(context);

    switch (context->current_property->key)
    {
        case INST_INSTRUMENT_PROPERTY_VOLUME:
        {
            instrument->volume = (uint8_t)context->current_value_int;
        }
        break;

        case INST_INSTRUMENT_PROPERTY_PAN:
        {
            instrument->pan = (uint8_t)context->current_value_int;
        }
        break;

        case INST_INSTRUMENT_PROPERTY_PRIORITY:
        {
            instrument->priority = (uint8_t)context->current_value_int;
        }
        break;

        case INST_INSTRUMENT_PROPERTY_BENDRANGE:
        {
            instrument->bend_range = (int16_t)context->current_value_int;
        }
        break;

        case INST_INSTRUMENT_PROPERTY_SOUND_ARR_ENTRY:
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

    set_current_property_value_int(context);

    switch (context->current_property->key)
    {
        case INST_SOUND_PROPERTY_USE:
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d>: context->current_property->key not supported: %d\n", __func__, __LINE__, context->current_property->key);
        }
        break;

        case INST_INSTRUMENT_PROPERTY_PAN:
        {
            sound->sample_pan = (uint8_t)context->current_value_int;
        }
        break;

        case INST_SOUND_PROPERTY_VOLUME:
        {
            sound->sample_volume = (uint8_t)context->current_value_int;
        }
        break;

        case INST_SOUND_PROPERTY_ENVELOPE:
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d>: context->current_property->key not supported: %d\n", __func__, __LINE__, context->current_property->key);
        }
        break;

        case INST_SOUND_PROPERTY_KEYMAP:
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d>: context->current_property->key not supported: %d\n", __func__, __LINE__, context->current_property->key);
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

    set_current_property_value_int(context);

    switch (context->current_property->key)
    {
        case INST_KEYMAP_PROPERTY_VELOCITY_MIN:
        {
            keymap->velocity_min = (uint8_t)context->current_value_int;
        }
        break;

        case INST_KEYMAP_PROPERTY_VELOCITY_MAX:
        {
            keymap->velocity_max = (uint8_t)context->current_value_int;
        }
        break;

        case INST_KEYMAP_PROPERTY_KEY_MIN:
        {
            keymap->key_min = (uint8_t)context->current_value_int;
        }
        break;

        case INST_KEYMAP_PROPERTY_KEY_MAX:
        {
            keymap->key_max = (uint8_t)context->current_value_int;
        }
        break;

        case INST_KEYMAP_PROPERTY_KEY_BASE:
        {
            keymap->key_base = (uint8_t)context->current_value_int;stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: context->current_property->key not supported: %d\n", __func__, __LINE__, context->current_property->key);
        }
        break;

        case INST_KEYMAP_PROPERTY_DETUNE:
        {
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
            envelope->attack_time = (int32_t)context->current_value_int;
        }
        break;

        case INST_ENVELOPE_PROPERTY_ATTACK_VOLUME:
        {
            envelope->attack_volume = (uint8_t)context->current_value_int;
        }
        break;

        case INST_ENVELOPE_PROPERTY_DECAY_TIME:
        {
            envelope->decay_time = (int32_t)context->current_value_int;
        }
        break;

        case INST_ENVELOPE_PROPERTY_DECAY_VOLUME:
        {
            envelope->decay_volume = (uint8_t)context->current_value_int;
        }
        break;

        case INST_ENVELOPE_PROPERTY_RELEASE_TIME:
        {
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

    memset(context->property_value_buffer, 0, MAX_FILENAME_LEN);
    memset(context->array_index_value, 0, IDENTIFIER_MAX_LEN);
    memset(context->property_name_buffer, 0, IDENTIFIER_MAX_LEN);

    context->current_property->key = 0;
    context->current_property->type_id = 0;
    memset(context->current_property->value, 0, MAX_FILENAME_LEN);
    
    TRACE_LEAVE(__func__)
}

void add_orphaned_instance(struct InstParseContext *context)
{
    TRACE_ENTER(__func__)

    context->type_name_buffer_pos = 0;
    context->instance_name_buffer_pos = 0;

    memset(context->type_name_buffer, 0, IDENTIFIER_MAX_LEN);
    memset(context->instance_name_buffer, 0, IDENTIFIER_MAX_LEN);

    context->current_type->key = 0;
    context->current_type->type_id = 0;
    memset(context->current_type->value, 0, IDENTIFIER_MAX_LEN);

    context->current_instance = NULL;
    
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

    struct InstParseContext *context = (struct InstParseContext*)malloc_zero(1, sizeof(struct InstParseContext));

    context->type_name_buffer = (char *)malloc_zero(1, IDENTIFIER_MAX_LEN);
    context->instance_name_buffer = (char *)malloc_zero(1, IDENTIFIER_MAX_LEN);
    context->property_name_buffer = (char *)malloc_zero(1, IDENTIFIER_MAX_LEN);
    context->array_index_value = (char *)malloc_zero(1, IDENTIFIER_MAX_LEN);
    // can contain filename path
    context->property_value_buffer = (char *)malloc_zero(1, MAX_FILENAME_LEN);

    context->current_property = (struct TypeInfo *)malloc_zero(1, sizeof(struct TypeInfo));
    context->current_type = (struct TypeInfo *)malloc_zero(1, sizeof(struct TypeInfo));
    context->current_type->value = (char *)malloc_zero(1, IDENTIFIER_MAX_LEN);
    context->current_property->value = (char *)malloc_zero(1, MAX_FILENAME_LEN);

    memset(line_buffer, 0, MAX_FILENAME_LEN);

    file_info_fseek(fi, 0, SEEK_SET);

    file_contents = (char *)malloc_zero(1, fi->len);
    len = file_info_fread(fi, file_contents, fi->len, 1);

    struct ALBankFile *bank_file = ALBankFile_new();

    c_int = -1;
    previous_c = -1;
    current_line_number = 0;
    pos = 0;
    state = INST_PARSE_STATE_INITIAL;
    len = fi->len;

    while (pos < len)
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
                    buffer_append_inc(context->type_name_buffer, context->type_name_buffer_pos, c);
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
                    buffer_append_inc(context->type_name_buffer, context->type_name_buffer_pos, c);
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
                    buffer_append_inc(context->type_name_buffer, context->type_name_buffer_pos, '\0');
                    get_type(context->type_name_buffer, context->current_type);

                    if (context->current_type->key == 0)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "%s %d> cannot determine type for \"%s\", pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, context->type_name_buffer, pos, current_line_number, state);
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
                    buffer_append_inc(context->instance_name_buffer, context->instance_name_buffer_pos, c);
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
                else if (is_alpha(c))
                {
                    buffer_append_inc(context->instance_name_buffer, context->instance_name_buffer_pos, c);
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
                    buffer_append_inc(context->instance_name_buffer, context->instance_name_buffer_pos, '\0');

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
                    buffer_append_inc(context->property_name_buffer, context->property_name_buffer_pos, c);
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
                    add_orphaned_instance(context);
                }
            }
            break;

            case INST_PARSE_STATE_PROPERTY_NAME:
            {
                terminate = 0;

                if (is_whitespace(c) || is_newline(c))
                {
                    previous_state = state;

                    if (context->current_property->type_id == TYPE_ID_INT)
                    {
                        state = INST_PARSE_STATE_EQUAL_SIGN_INT;
                    }
                    else if (context->current_property->type_id == TYPE_ID_STRING)
                    {
                        state = INST_PARSE_STATE_INITIAL_FILENAME_VALUE;
                    }
                    else if (context->current_property->type_id == TYPE_ID_TEXT_REF_ID)
                    {
                        state = INST_PARSE_STATE_BEGIN_ARRAY_REF;
                    }
                    else
                    {
                        stderr_exit(
                            EXIT_CODE_GENERAL,
                            "%s %d> cannot resolve next state for property, id=%d, name=\"%s\", type_id=%d, pos=%ld, source line=%d, state=%d\n",
                            __func__,
                            __LINE__,
                            context->current_property->key,
                            context->current_property->value,
                            context->current_property->type_id,
                            pos,
                            current_line_number,
                            state);
                    }

                    terminate = 1;
                }
                else if (is_alpha(c))
                {
                    buffer_append_inc(context->property_name_buffer, context->property_name_buffer_pos, c);
                }
                else if (is_comment(c))
                {
                    // return to next state after comment
                    if (context->current_property->type_id == TYPE_ID_INT)
                    {
                        previous_state = INST_PARSE_STATE_EQUAL_SIGN_INT;
                    }
                    else if (context->current_property->type_id == TYPE_ID_STRING)
                    {
                        previous_state = INST_PARSE_STATE_INITIAL_FILENAME_VALUE;
                    }
                    else if (context->current_property->type_id == TYPE_ID_TEXT_REF_ID)
                    {
                        state = INST_PARSE_STATE_BEGIN_ARRAY_REF;
                    }
                    else
                    {
                        stderr_exit(
                            EXIT_CODE_GENERAL,
                            "%s %d> cannot resolve next state for property, id=%d, name=\"%s\", type_id=%d, pos=%ld, source line=%d, state=%d\n",
                            __func__,
                            __LINE__,
                            context->current_property->key,
                            context->current_property->value,
                            context->current_property->type_id,
                            pos,
                            current_line_number,
                            state);
                    }

                    state = INST_PARSE_STATE_COMMENT;
                    
                    terminate = 1;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }

                if (terminate)
                {
                    buffer_append_inc(context->property_name_buffer, context->property_name_buffer_pos, '\0');
                    get_property(context->current_type, context->property_name_buffer, context->current_property);

                    if (context->current_property->key == 0)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "%s %d> cannot determine property for \"%s\", pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, context->property_name_buffer, pos, current_line_number, state);
                    }

                    context->property_name_buffer_pos = 0;
                    memset(context->property_name_buffer, 0, IDENTIFIER_MAX_LEN);
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
                    buffer_append_inc(context->property_value_buffer, context->property_value_buffer_pos, c);
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
                    buffer_append_inc(context->property_value_buffer, context->property_value_buffer_pos, c);
                }
                else if (c == ';')
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_INITIAL_INSTANCE_PROPERTY;

                    terminate = 1;
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
                    buffer_append_inc(context->property_value_buffer, context->property_value_buffer_pos, '\0');

                    apply_property_on_instance(context);
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
                    state = INST_PARSE_STATE_SEARCH_OPEN_QUOTE;
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

                    buffer_append_inc(context->property_value_buffer, context->property_value_buffer_pos, '\0');
                }
                else
                {
                    // accept all.
                    buffer_append_inc(context->property_value_buffer, context->property_value_buffer_pos, c);
                }
            }
            break;

            case INST_PARSE_STATE_SEARCH_CLOSE_PAREN:
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
                else if (c == ')')
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_SEARCH_SEMI_COLON;

                    terminate = 1;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }

                if (terminate)
                {
                    apply_property_on_instance(context);
                }
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

                    buffer_append_inc(context->array_index_value, context->array_index_value_pos, c);
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
                    previous_state = state;
                    state = INST_PARSE_STATE_ARRAY_INDEX_SEARCH_CLOSE_BRACKET;

                    buffer_append_inc(context->array_index_value, context->array_index_value_pos, '\0');
                }
                else if (is_numeric(c))
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_ARRAY_INDEX;

                    buffer_append_inc(context->array_index_value, context->array_index_value_pos, c);
                }
                else if (c == ']')
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_EQUAL_SIGN_ARRAY_REF;

                    buffer_append_inc(context->array_index_value, context->array_index_value_pos, '\0');
                }
                else if (is_comment(c))
                {
                    // advance to search state
                    previous_state = INST_PARSE_STATE_ARRAY_INDEX_SEARCH_CLOSE_BRACKET;
                    state = INST_PARSE_STATE_COMMENT;

                    buffer_append_inc(context->array_index_value, context->array_index_value_pos, '\0');
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
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
                    state = INST_PARSE_STATE_EQUAL_SIGN_ARRAY_REF;
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

            case INST_PARSE_STATE_EQUAL_SIGN_ARRAY_REF:
            {
                if (is_whitespace(c) || is_newline(c))
                {
                    // nothing to do
                }
                else if (c == '=')
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_ARRAY_INITIAL_TEXT_REF_ID;
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

            case INST_PARSE_STATE_ARRAY_INITIAL_TEXT_REF_ID:
            {
                if (is_whitespace(c) || is_newline(c))
                {
                    // nothing to do
                }
                else if (is_alpha(c))
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_ARRAY_TEXT_REF_ID;

                    buffer_append_inc(context->property_value_buffer, context->property_value_buffer_pos, c);
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

            case INST_PARSE_STATE_ARRAY_TEXT_REF_ID:
            {
                if (is_whitespace(c) || is_newline(c))
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_SEARCH_SEMI_COLON;

                    buffer_append_inc(context->property_value_buffer, context->property_value_buffer_pos, '\0');
                }
                else if (is_alphanumeric(c))
                {
                    buffer_append_inc(context->property_value_buffer, context->property_value_buffer_pos, c);
                }
                else if (is_comment(c))
                {
                    // return to next state
                    previous_state = INST_PARSE_STATE_SEARCH_SEMI_COLON;
                    state = INST_PARSE_STATE_COMMENT;

                    buffer_append_inc(context->property_value_buffer, context->property_value_buffer_pos, '\0');
                }
                else if (c == ';')
                {
                    previous_state = state;
                    state = INST_PARSE_STATE_INITIAL_INSTANCE_PROPERTY;

                    buffer_append_inc(context->property_value_buffer, context->property_value_buffer_pos, '\0');
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
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
                    state = INST_PARSE_STATE_INITIAL;

                    terminate = 1;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }

                if (terminate)
                {
                    apply_property_on_instance(context);
                }
            }
            break;

            default:
            stderr_exit(EXIT_CODE_GENERAL, "Invalid parse state: %d\n", state);
        }
    }

    free(context->current_type->value);
    free(context->current_property->value);
    free(context->current_type);
    free(context->current_property);
    free(context->property_value_buffer);
    free(context->array_index_value);
    free(context->property_name_buffer);
    free(context->instance_name_buffer);
    free(context->type_name_buffer);
    free(context);

    TRACE_LEAVE(__func__)

    return bank_file;
}

