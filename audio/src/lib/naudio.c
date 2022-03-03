#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>
#include "debug.h"
#include "common.h"
#include "utility.h"
#include "naudio.h"
#include "int_hash.h"
#include "string_hash.h"

/**
 * This file contains primary code for supporting Rare's audio structs,
 * and Nintendo's (libultra) audio structs.
*/

struct CtlParseContext {
    struct IntHashTable *seen_sound;
    struct IntHashTable *seen_instrument;
    struct IntHashTable *seen_envelope;
    struct IntHashTable *seen_wavetable;
    struct IntHashTable *seen_keymap;
};

/**
 * Callback to initialize wavetable object.
 * If not set externally, will be set to {@code ALWaveTable_init_default_set_aifc_path}.
*/
wavetable_init_callback wavetable_init_callback_ptr = NULL;

static int32_t g_bank_file_id = 0;
static int32_t g_bank_id = 0;
static int32_t g_instrument_id = 0;
static int32_t g_sound_id = 0;
static int32_t g_envelope_id = 0;
static int32_t g_wavetable_id = 0;
static int32_t g_keymap_id = 0;

// forward declarations

struct ALADPCMLoop *ALADPCMLoop_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset);
struct ALADPCMBook *ALADPCMBook_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset);
struct ALRawLoop *ALRawLoop_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset);
struct ALEnvelope *ALEnvelope_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset, struct ALSound *parent);
struct ALKeyMap *ALKeyMap_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset, struct ALSound *parent);
struct ALWaveTable *ALWaveTable_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset, struct ALSound *parent);
struct ALSound *ALSound_new_from_ctl(struct CtlParseContext *context, uint8_t *ctl_file_contents, int32_t load_from_offset, struct ALInstrument *parent);
struct ALInstrument *ALInstrument_new_from_ctl(struct CtlParseContext *context, uint8_t *ctl_file_contents, int32_t load_from_offset, struct ALBank *parent);
struct ALBank *ALBank_new_from_ctl(struct CtlParseContext *context, uint8_t *ctl_file_contents, int32_t load_from_offset);

void ALEnvelope_write_inst(struct ALEnvelope *envelope, struct FileInfo *fi);
void ALKeyMap_write_inst(struct ALKeyMap *keymap, struct FileInfo *fi);
void ALSound_write_inst(struct ALSound *sound, struct FileInfo *fi);
void ALInstrument_write_inst(struct ALInstrument *instrument, struct FileInfo *fi);
void ALBank_write_inst(struct ALBank *bank, struct FileInfo *fi);

void ALADPCMLoop_free(struct ALADPCMLoop *loop);
void ALRawLoop_free(struct ALRawLoop *loop);
void ALEnvelope_free(struct ALEnvelope *envelope);
void ALKeyMap_free(struct ALKeyMap *keymap);
void ALWaveTable_free(struct ALWaveTable *wavetable);
void ALSound_free(struct ALSound *sound);
void ALInstrument_free(struct ALInstrument *instrument);
void ALBank_free(struct ALBank *bank);

void ALEnvelope_notify_parents_null(struct ALEnvelope *envelope);
void ALKeyMap_notify_parents_null(struct ALKeyMap *keymap);
void ALWaveTable_notify_parents_null(struct ALWaveTable *wavetable);
void ALSound_notify_parents_null(struct ALSound *sound);
void ALInstrument_notify_parents_null(struct ALInstrument *instrument);

static struct CtlParseContext *CtlParseContext_new(void);
static void CtlParseContext_free(struct CtlParseContext *context);
static void ALWaveTable_init_default_set_aifc_path(struct ALWaveTable *wavetable);
static void ALBankFile_set_write_order_by_offset(struct ALBankFile *bank_file);

// end forward declarations

/**
 * Reads a single {@code struct ALADPCMLoop} from a .ctl file that has been loaded into memory.
 * No memory allocation performed.
 * @param ctl_file_contents: .ctl file.
 * @param load_from_offset: position in .ctl file to read data from.
 * @returns: pointer to new loop.
*/
struct ALADPCMLoop *ALADPCMLoop_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset)
{
    TRACE_ENTER(__func__)
    
    if (ctl_file_contents == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> ctl_file_contents is NULL\n", __func__, __LINE__);
    }

    int32_t input_pos = load_from_offset;

    struct ALADPCMLoop *adpcm_loop = (struct ALADPCMLoop *)malloc_zero(1, sizeof(struct ALADPCMLoop));

    adpcm_loop->start = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    adpcm_loop->end = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    adpcm_loop->count = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    // state

    // raw byte copy, no bswap
    memcpy(adpcm_loop->state, &ctl_file_contents[input_pos], ADPCM_STATE_SIZE);

    TRACE_LEAVE(__func__)

    return adpcm_loop;
}

/**
 * Reads a single {@code struct ALADPCMBook} from a .ctl file that has been loaded into memory.
 * {@code adpcm_book->book} is allocated if present.
 * @param ctl_file_contents: .ctl file.
 * @param load_from_offset: position in .ctl file to read data from.
 * @returns: pointer to new book
*/
struct ALADPCMBook *ALADPCMBook_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset)
{
    TRACE_ENTER(__func__)
    
    if (ctl_file_contents == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> ctl_file_contents is NULL\n", __func__, __LINE__);
    }
    
    int32_t input_pos = load_from_offset;
    int book_bytes;

    struct ALADPCMBook *adpcm_book = (struct ALADPCMBook *)malloc_zero(1, sizeof(struct ALADPCMBook));

    adpcm_book->order = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    adpcm_book->npredictors = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    // book, size in bytes = order * npredictors * 16
    book_bytes = adpcm_book->order * adpcm_book->npredictors * 16;

    if (book_bytes > 0)
    {
        adpcm_book->book = (int16_t *)malloc_zero(1, book_bytes);
        // raw byte copy, no bswap
        memcpy(adpcm_book->book, &ctl_file_contents[input_pos], book_bytes);
    }

    TRACE_LEAVE(__func__)

    return adpcm_book;
}

/**
 * Instantiates a new {@code struct ALADPCMBook} and allocates memory for codebook.
 * @param order: order.
 * @param npredictors: number of predictors.
 * @returns: pointer to new book
*/
struct ALADPCMBook *ALADPCMBook_new(int order, int npredictors)
{
    TRACE_ENTER(__func__)
    
    int book_bytes;

    struct ALADPCMBook *adpcm_book = (struct ALADPCMBook *)malloc_zero(1, sizeof(struct ALADPCMBook));

    adpcm_book->order = order;
    adpcm_book->npredictors = npredictors;

    // book, size in bytes = order * npredictors * 16
    book_bytes = adpcm_book->order * adpcm_book->npredictors * 16;

    if (book_bytes > 0)
    {
        adpcm_book->book = (int16_t *)malloc_zero(1, book_bytes);
    }

    TRACE_LEAVE(__func__)

    return adpcm_book;
}

/**
 * Reads a single {@code struct ALRawLoop} from a .ctl file that has been loaded into memory.
 * No memory allocation performed.
 * @param ctl_file_contents: .ctl file.
 * @param load_from_offset: position in .ctl file to read data from.
 * @returns: pointer to new loop.
*/
struct ALRawLoop *ALRawLoop_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset)
{
    TRACE_ENTER(__func__)
    
    if (ctl_file_contents == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> ctl_file_contents is NULL\n", __func__, __LINE__);
    }

    int32_t input_pos = load_from_offset;

    struct ALRawLoop *raw_loop = (struct ALRawLoop *)malloc_zero(1, sizeof(struct ALRawLoop));

    raw_loop->start = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    raw_loop->end = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    raw_loop->count = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    TRACE_LEAVE(__func__)

    return raw_loop;
}

/**
 * Base constructor. Only partially initializes envelope. This should
 * only be called from other constructors.
 * @returns: pointer to new envelope.
*/
struct ALEnvelope *ALEnvelope_new()
{
    TRACE_ENTER(__func__)

    struct ALEnvelope *envelope = (struct ALEnvelope *)malloc_zero(1, sizeof(struct ALEnvelope));

    envelope->id = g_envelope_id++;

    TRACE_LEAVE(__func__)

    return envelope;
}

/**
 * Reads a single {@code struct ALEnvelope} from a .ctl file that has been loaded into memory.
 * No memory allocation performed.
 * @param ctl_file_contents: .ctl file.
 * @param load_from_offset: position in .ctl file to read data from.
 * @param parent: Parent sound this wavetable object belongs to.
 * @returns: pointer to new envelope.
*/
struct ALEnvelope *ALEnvelope_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset, struct ALSound *parent)
{
    TRACE_ENTER(__func__)
    
    if (ctl_file_contents == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> ctl_file_contents is NULL\n", __func__, __LINE__);
    }

    int32_t input_pos = load_from_offset;

    struct ALEnvelope *envelope = ALEnvelope_new();

    snprintf(envelope->text_id, INST_OBJ_ID_STRING_LEN, "Envelope%04d", envelope->id);

    envelope->ctl_write_order = envelope->id;
    envelope->self_offset = load_from_offset;

    envelope->attack_time = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    envelope->decay_time = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    envelope->release_time = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    envelope->attack_volume = ctl_file_contents[input_pos];
    input_pos += 1;

    envelope->decay_volume = ctl_file_contents[input_pos];
    input_pos += 1;

    if (DEBUG_OFFSET_CONSOLE && g_verbosity >= VERBOSE_DEBUG)
    {
        printf("init envelope %d\n", envelope->id);
    }

    if (parent != NULL)
    {
        ALEnvelope_add_parent(envelope, parent);
    }

    TRACE_LEAVE(__func__)

    return envelope;
}

/**
 * Write a codebook to disk in gaudio format.
 * @param book: codebook to write.
 * @param fi: file to write to.
*/
void ALADPCMBook_write_coef(struct ALADPCMBook *book, struct FileInfo *fi)
{
    TRACE_ENTER(__func__)

    if (book == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> book is NULL\n", __func__, __LINE__);
    }
    
    if (fi == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> fi is NULL\n", __func__, __LINE__);
    }

    int line_length;
    char line_buffer[WRITE_BUFFER_LEN];
    int num_rows;
    int i;
    int row;
    int pos;
    int val;
    char last;

    memset(line_buffer, 0, WRITE_BUFFER_LEN);
    line_length = sprintf(line_buffer, "order=%d;\n", book->order);
    FileInfo_fwrite(fi, line_buffer, (size_t)line_length, 1);

    memset(line_buffer, 0, WRITE_BUFFER_LEN);
    line_length = sprintf(line_buffer, "npredictors=%d;\n", book->npredictors);
    FileInfo_fwrite(fi, line_buffer, (size_t)line_length, 1);

    memset(line_buffer, 0, WRITE_BUFFER_LEN);
    line_length = sprintf(line_buffer, "book=\n");
    FileInfo_fwrite(fi, line_buffer, (size_t)line_length, 1);

    num_rows = book->order * book->npredictors;
    pos = 0;

    for (row=0; row<num_rows; row++)
    {
        for (i=0; i<7; i++)
        {
            val = book->book[pos];
            pos++;

            memset(line_buffer, 0, WRITE_BUFFER_LEN);
            line_length = sprintf(line_buffer, "%6d, ", val);
            FileInfo_fwrite(fi, line_buffer, (size_t)line_length, 1);
        }

        val = book->book[pos];
        pos++;

        if (row == num_rows - 1)
        {
            last = ';';
        }
        else
        {
            last = ',';
        }

        memset(line_buffer, 0, WRITE_BUFFER_LEN);
        line_length = sprintf(line_buffer, "%6d%c\n", val, last);
        FileInfo_fwrite(fi, line_buffer, (size_t)line_length, 1);
    }

    memset(line_buffer, 0, WRITE_BUFFER_LEN);
    line_length = sprintf(line_buffer, "\n\n");
    FileInfo_fwrite(fi, line_buffer, (size_t)line_length, 1);
    
    TRACE_LEAVE(__func__)
}

/**
 * Writes {@code struct ALEnvelope} to .inst file, using current file seek position.
 * Writes any child information as well.
 * @param envelope: object to write.
 * @param fi: FileInfo.
*/
void ALEnvelope_write_inst(struct ALEnvelope *envelope, struct FileInfo *fi)
{
    TRACE_ENTER(__func__)
    
    if (envelope == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> envelope is NULL\n", __func__, __LINE__);
    }
    
    if (fi == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> fi is NULL\n", __func__, __LINE__);
    }

    int len;

    envelope->visited = 1;

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "envelope %s", envelope->text_id);
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, " {\n");
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"metaCtlWriteOrder = %d;\n", envelope->ctl_write_order);
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    // the following options are always written, even if zero.

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"attackTime = %d;\n", envelope->attack_time);
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"attackVolume = %d;\n", envelope->attack_volume);
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"decayTime = %d;\n", envelope->decay_time);
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"decayVolume = %d;\n", envelope->decay_volume);
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"releaseTime = %d;\n", envelope->release_time);
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "}\n");
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "\n");
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    TRACE_LEAVE(__func__)
}

/**
 * Base constructor. Only partially initializes keymap. This should
 * only be called from other constructors.
 * @returns: pointer to new keymap.
*/
struct ALKeyMap *ALKeyMap_new()
{
    TRACE_ENTER(__func__)

    struct ALKeyMap *keymap = (struct ALKeyMap *)malloc_zero(1, sizeof(struct ALKeyMap));

    keymap->id = g_keymap_id++;

    TRACE_LEAVE(__func__)

    return keymap;
}

/**
 * Reads a single {@code struct ALKeyMap} from a .ctl file that has been loaded into memory.
 * No memory allocation performed.
 * @param ctl_file_contents: .ctl file.
 * @param load_from_offset: position in .ctl file to read data from.
 * @param parent: Parent sound this wavetable object belongs to.
 * @returns: pointer to new keymap.
*/
struct ALKeyMap *ALKeyMap_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset, struct ALSound *parent)
{
    TRACE_ENTER(__func__)
    
    if (ctl_file_contents == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> ctl_file_contents is NULL\n", __func__, __LINE__);
    }

    int32_t input_pos = load_from_offset;

    struct ALKeyMap *keymap = ALKeyMap_new();
    snprintf(keymap->text_id, INST_OBJ_ID_STRING_LEN, "Keymap%04d", keymap->id);

    keymap->ctl_write_order = keymap->id;
    keymap->self_offset = load_from_offset;

    keymap->velocity_min = ctl_file_contents[input_pos];
    input_pos += 1;

    keymap->velocity_max = ctl_file_contents[input_pos];
    input_pos += 1;

    keymap->key_min = ctl_file_contents[input_pos];
    input_pos += 1;

    keymap->key_max = ctl_file_contents[input_pos];
    input_pos += 1;

    keymap->key_base = ctl_file_contents[input_pos];
    input_pos += 1;

    keymap->detune = ctl_file_contents[input_pos];
    input_pos += 1;

    if (DEBUG_OFFSET_CONSOLE && g_verbosity >= VERBOSE_DEBUG)
    {
        printf("init keymap %d\n", keymap->id);
    }

    if (parent != NULL)
    {
        ALKeyMap_add_parent(keymap, parent);
    }

    TRACE_LEAVE(__func__)

    return keymap;
}

/**
 * Writes {@code struct ALKeyMap} to .inst file, using current file seek position.
 * Writes any child information as well.
 * @param keymap: object to write.
 * @param fi: FileInfo.
*/
void ALKeyMap_write_inst(struct ALKeyMap *keymap, struct FileInfo *fi)
{
    TRACE_ENTER(__func__)
    
    if (keymap == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> keymap is NULL\n", __func__, __LINE__);
    }
    
    if (fi == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> fi is NULL\n", __func__, __LINE__);
    }

    int len;

    keymap->visited = 1;

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "keymap %s", keymap->text_id);
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, " {\n");
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"metaCtlWriteOrder = %d;\n", keymap->ctl_write_order);
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    // the following options are always written, even if zero.

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"velocityMin = %d;\n", keymap->velocity_min);
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"velocityMax = %d;\n", keymap->velocity_max);
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"keyMin = %d;\n", keymap->key_min);
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"keyMax = %d;\n", keymap->key_max);
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"keyBase = %d;\n", keymap->key_base);
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"detune = %d;\n", keymap->detune);
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "}\n");
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "\n");
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    TRACE_LEAVE(__func__)
}

/**
 * Adds a reference to an existing parent object.
 * @param keymap: child object.
 * @param parent: reference to add.
*/
void ALKeyMap_add_parent(struct ALKeyMap *keymap, struct ALSound *parent)
{
    TRACE_ENTER(__func__)
    
    if (keymap == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> keymap is NULL\n", __func__, __LINE__);
    }
    
    if (parent == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> parent is NULL\n", __func__, __LINE__);
    }

    if (keymap->parents == NULL)
    {
        keymap->parents = LinkedList_new();
    }

    struct LinkedListNode *node = LinkedListNode_new();
    node->data = parent;

    LinkedList_append_node(keymap->parents, node);

    TRACE_LEAVE(__func__)
}

/**
 * Adds a reference to an existing parent object.
 * @param envelope: child object.
 * @param parent: reference to add.
*/
void ALEnvelope_add_parent(struct ALEnvelope *envelope, struct ALSound *parent)
{
    TRACE_ENTER(__func__)
    
    if (envelope == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> envelope is NULL\n", __func__, __LINE__);
    }
    
    if (parent == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> parent is NULL\n", __func__, __LINE__);
    }

    if (envelope->parents == NULL)
    {
        envelope->parents = LinkedList_new();
    }

    struct LinkedListNode *node = LinkedListNode_new();
    node->data = parent;

    LinkedList_append_node(envelope->parents, node);

    TRACE_LEAVE(__func__)
}

/**
 * Adds a reference to an existing parent object.
 * @param wavetable: child object.
 * @param parent: reference to add.
*/
void ALWaveTable_add_parent(struct ALWaveTable *wavetable, struct ALSound *parent)
{
    TRACE_ENTER(__func__)
    
    if (wavetable == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> wavetable is NULL\n", __func__, __LINE__);
    }
    
    if (parent == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> parent is NULL\n", __func__, __LINE__);
    }

    if (wavetable->parents == NULL)
    {
        wavetable->parents = LinkedList_new();
    }

    struct LinkedListNode *node = LinkedListNode_new();
    node->data = parent;

    LinkedList_append_node(wavetable->parents, node);

    TRACE_LEAVE(__func__)
}

/**
 * Adds a reference to an existing parent object.
 * @param sound: child object.
 * @param parent: reference to add.
*/
void ALSound_add_parent(struct ALSound *sound, struct ALInstrument *parent)
{
    TRACE_ENTER(__func__)
    
    if (sound == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> sound is NULL\n", __func__, __LINE__);
    }
    
    if (parent == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> parent is NULL\n", __func__, __LINE__);
    }

    if (sound->parents == NULL)
    {
        sound->parents = LinkedList_new();
    }

    struct LinkedListNode *node = LinkedListNode_new();
    node->data = parent;

    LinkedList_append_node(sound->parents, node);

    TRACE_LEAVE(__func__)
}

/**
 * Adds a reference to an existing parent object.
 * @param instrument: child object.
 * @param parent: reference to add.
*/
void ALInstrument_add_parent(struct ALInstrument *instrument, struct ALBank *parent)
{
    TRACE_ENTER(__func__)
    
    if (instrument == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> instrument is NULL\n", __func__, __LINE__);
    }
    
    if (parent == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> parent is NULL\n", __func__, __LINE__);
    }

    if (instrument->parents == NULL)
    {
        instrument->parents = LinkedList_new();
    }

    struct LinkedListNode *node = LinkedListNode_new();
    node->data = parent;

    LinkedList_append_node(instrument->parents, node);

    TRACE_LEAVE(__func__)
}

/**
 * Base constructor. Only partially initializes wavetable. This should
 * only be called from other constructors.
 * @returns: pointer to new wavetable.
*/
struct ALWaveTable *ALWaveTable_new()
{
    TRACE_ENTER(__func__)

    struct ALWaveTable *wavetable = (struct ALWaveTable *)malloc_zero(1, sizeof(struct ALWaveTable));

    wavetable->id = g_wavetable_id++;

    // unlike the other properties with a base constructor, this doesn't have a named
    // reference saved/loaded from .inst file, so go ahead and set the text_id.
    snprintf(wavetable->text_id, INST_OBJ_ID_STRING_LEN, "Wavetable%04d", wavetable->id);

    TRACE_LEAVE(__func__)

    return wavetable;
}

/**
 * Reads a single {@code struct ALWaveTable} from a .ctl file that has been loaded into memory.
 * Allocates memory and calls _init_load for child book and loop if present.
 * @param ctl_file_contents: .ctl file.
 * @param load_from_offset: position in .ctl file to read data from.
 * @param parent: Parent sound this wavetable object belongs to.
 * @returns: pointer to new wavetable.
*/
struct ALWaveTable *ALWaveTable_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset, struct ALSound *parent)
{
    TRACE_ENTER(__func__)
    
    if (ctl_file_contents == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> ctl_file_contents is NULL\n", __func__, __LINE__);
    }
    
    int32_t input_pos = load_from_offset;

    struct ALWaveTable *wavetable = ALWaveTable_new();

    wavetable->self_offset = load_from_offset;

    if (wavetable_init_callback_ptr == NULL)
    {
        wavetable_init_callback_ptr = ALWaveTable_init_default_set_aifc_path;
    }

    // set parent before calling callback method
    if (parent != NULL)
    {
        ALWaveTable_add_parent(wavetable, parent);
    }

    wavetable_init_callback_ptr(wavetable);

    wavetable->base = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    wavetable->len = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    wavetable->type = ctl_file_contents[input_pos];
    input_pos += 1;

    wavetable->flags = ctl_file_contents[input_pos];
    input_pos += 1;

    // padding
    input_pos += 2;

    // it's ok to cast to the wrong pointer type at this time
    wavetable->wave_info.adpcm_wave.loop_offset = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    if (DEBUG_OFFSET_CONSOLE && g_verbosity >= VERBOSE_DEBUG)
    {
        printf("init wavetable %d. tbl base: 0x%04x, length 0x%04x\n", wavetable->id, wavetable->base, wavetable->len);
    }

    if (wavetable->type == AL_ADPCM_WAVE)
    {
        wavetable->wave_info.adpcm_wave.book_offset = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
        input_pos += 4;
    }

    if (wavetable->type == AL_ADPCM_WAVE)
    {
        if (wavetable->wave_info.adpcm_wave.loop_offset > 0)
        {
            wavetable->wave_info.adpcm_wave.loop = ALADPCMLoop_new_from_ctl(ctl_file_contents, wavetable->wave_info.adpcm_wave.loop_offset);
        }

        if (wavetable->wave_info.adpcm_wave.book_offset > 0)
        {
            wavetable->wave_info.adpcm_wave.book = ALADPCMBook_new_from_ctl(ctl_file_contents, wavetable->wave_info.adpcm_wave.book_offset);
        }
    }
    else if (wavetable->type == AL_RAW16_WAVE)
    {
        if (wavetable->wave_info.raw_wave.loop_offset > 0)
        {
            wavetable->wave_info.raw_wave.loop = ALRawLoop_new_from_ctl(ctl_file_contents, wavetable->wave_info.raw_wave.loop_offset);
        }
    }

    TRACE_LEAVE(__func__)

    return wavetable;
}

/**
 * Base constructor. Only partially initializes sound. This should
 * only be called from other constructors.
 * @returns: pointer to new sound.
*/
struct ALSound *ALSound_new()
{
    TRACE_ENTER(__func__)

    struct ALSound *sound = (struct ALSound *)malloc_zero(1, sizeof(struct ALSound));

    sound->id = g_sound_id++;

    TRACE_LEAVE(__func__)

    return sound;
}

/**
 * Reads a single {@code struct ALSound} from a .ctl file that has been loaded into memory.
 * Allocates memory and calls _init_load for child envelope, keymap, wavetable if present.
 * @param context: context
 * @param ctl_file_contents: .ctl file.
 * @param load_from_offset: position in .ctl file to read data from.
 * @param parent: Parent instrument this sound object belongs to.
 * @returns: pointer to new sound.
*/
struct ALSound *ALSound_new_from_ctl(struct CtlParseContext *context, uint8_t *ctl_file_contents, int32_t load_from_offset, struct ALInstrument *parent)
{
    TRACE_ENTER(__func__)
    
    if (context == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> context is NULL\n", __func__, __LINE__);
    }
    
    if (ctl_file_contents == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> ctl_file_contents is NULL\n", __func__, __LINE__);
    }

    int32_t input_pos = load_from_offset;

    struct ALSound *sound = ALSound_new();
    snprintf(sound->text_id, INST_OBJ_ID_STRING_LEN, "Sound%04d", sound->id);

    sound->ctl_write_order = sound->id;
    sound->self_offset = load_from_offset;

    sound->envelope_offset = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    sound->keymap_offset = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    sound->wavetable_offset = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    sound->sample_pan = ctl_file_contents[input_pos];
    input_pos += 1;

    sound->sample_volume = ctl_file_contents[input_pos];
    input_pos += 1;

    sound->flags = ctl_file_contents[input_pos];
    input_pos += 1;

    if (DEBUG_OFFSET_CONSOLE && g_verbosity >= VERBOSE_DEBUG)
    {
        printf("init sound %d\n", sound->id);
    }

    if (parent != NULL)
    {
        ALSound_add_parent(sound, parent);
    }

    if (sound->envelope_offset > 0)
    {
        struct ALEnvelope *envelope;

        if (IntHashTable_contains(context->seen_envelope, sound->envelope_offset))
        {
            envelope = IntHashTable_get(context->seen_envelope, sound->envelope_offset);
            ALEnvelope_add_parent(envelope, sound);
        }
        else
        {
            // parent reference is added in constructor
            envelope = ALEnvelope_new_from_ctl(ctl_file_contents, sound->envelope_offset, sound);
            IntHashTable_add(context->seen_envelope, sound->envelope_offset, envelope);
        }

        sound->envelope = envelope;
    }

    if (sound->keymap_offset > 0)
    {
        struct ALKeyMap *keymap;

        if (IntHashTable_contains(context->seen_keymap, sound->keymap_offset))
        {
            keymap = IntHashTable_get(context->seen_keymap, sound->keymap_offset);
            ALKeyMap_add_parent(keymap, sound);
        }
        else
        {
            // parent reference is added in constructor
            keymap = ALKeyMap_new_from_ctl(ctl_file_contents, sound->keymap_offset, sound);
            IntHashTable_add(context->seen_keymap, sound->keymap_offset, keymap);
        }

        sound->keymap = keymap;
    }

    if (sound->wavetable_offset > 0)
    {
        struct ALWaveTable *wavetable;

        if (IntHashTable_contains(context->seen_wavetable, sound->wavetable_offset))
        {
            wavetable = IntHashTable_get(context->seen_wavetable, sound->wavetable_offset);
            ALWaveTable_add_parent(wavetable, sound);
        }
        else
        {
            // parent reference is added in constructor
            wavetable = ALWaveTable_new_from_ctl(ctl_file_contents, sound->wavetable_offset, sound);
            IntHashTable_add(context->seen_wavetable, sound->wavetable_offset, wavetable);
        }

        sound->wavetable = wavetable;
    }
    
    TRACE_LEAVE(__func__)

    return sound;
}

/**
 * Writes {@code struct ALSound} to .inst file, using current file seek position.
 * Writes any child information as well.
 * @param sound: object to write.
 * @param fi: FileInfo.
*/
void ALSound_write_inst(struct ALSound *sound, struct FileInfo *fi)
{
    TRACE_ENTER(__func__)
    
    if (sound == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> sound is NULL\n", __func__, __LINE__);
    }
    
    if (fi == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> fi is NULL\n", __func__, __LINE__);
    }

    int len;

    sound->visited = 1;

    // don't write declaration more than once.
    if (sound->envelope != NULL && sound->envelope->visited == 0)
    {
        ALEnvelope_write_inst(sound->envelope, fi);
    }

    // don't write declaration more than once.
    if (sound->keymap != NULL && sound->keymap->visited == 0)
    {
        ALKeyMap_write_inst(sound->keymap, fi);
    }

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "sound %s", sound->text_id);
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, " {\n");
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"metaCtlWriteOrder = %d;\n", sound->ctl_write_order);
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    if (sound->wavetable != NULL)
    {
        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"use (\"%s\");\n", sound->wavetable->aifc_path);
        FileInfo_fwrite(fi, g_write_buffer, len, 1);

        if (g_verbosity >= VERBOSE_DEBUG)
        {
            memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"# wavetable_offset = 0x%06x;\n", sound->wavetable_offset);
            FileInfo_fwrite(fi, g_write_buffer, len, 1);
        }
    }

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "\n");
    FileInfo_fwrite(fi, g_write_buffer, len, 1);
    
    if (sound->sample_pan != 0)
    {
        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"pan = %d;\n", sound->sample_pan);
        FileInfo_fwrite(fi, g_write_buffer, len, 1);
    }
    
    if (sound->sample_volume != 0)
    {
        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"volume = %d;\n", sound->sample_volume);
        FileInfo_fwrite(fi, g_write_buffer, len, 1);
    }
    
    if (sound->flags != 0)
    {
        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"flags = %d;\n", sound->flags);
        FileInfo_fwrite(fi, g_write_buffer, len, 1);
    }
    
    if (sound->envelope != NULL)
    {
        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"envelope = %s;\n", sound->envelope->text_id);
        FileInfo_fwrite(fi, g_write_buffer, len, 1);

        if (g_verbosity >= VERBOSE_DEBUG)
        {
            memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"# envelope_offset = 0x%06x;\n", sound->envelope_offset);
            FileInfo_fwrite(fi, g_write_buffer, len, 1);
        }
    }
    
    if (sound->keymap != NULL)
    {
        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"keymap = %s;\n", sound->keymap->text_id);
        FileInfo_fwrite(fi, g_write_buffer, len, 1);

        if (g_verbosity >= VERBOSE_DEBUG)
        {
            memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"# keymap_offset = 0x%06x;\n", sound->keymap_offset);
            FileInfo_fwrite(fi, g_write_buffer, len, 1);
        }
    }

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "}\n");
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "\n");
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    TRACE_LEAVE(__func__)
}

/**
 * Base constructor. Only partially initializes instrument. This should
 * only be called from other constructors.
 * @returns: pointer to new instrument.
*/
struct ALInstrument *ALInstrument_new()
{
    TRACE_ENTER(__func__)

    struct ALInstrument *instrument = (struct ALInstrument *)malloc_zero(1, sizeof(struct ALInstrument));

    instrument->id = g_instrument_id++;

    TRACE_LEAVE(__func__)

    return instrument;
}

/**
 * Reads a single {@code struct ALInstrument} from a .ctl file that has been loaded into memory.
 * Allocates memory and calls _init_load for sounds and related if {@code sound_count} > 0.
 * @param context: context
 * @param instrument: object to write to.
 * @param ctl_file_contents: .ctl file.
 * @param parent: Parent bank this sound instrument belongs to.
 * @param load_from_offset: position in .ctl file to read data from.
*/
struct ALInstrument *ALInstrument_new_from_ctl(struct CtlParseContext *context, uint8_t *ctl_file_contents, int32_t load_from_offset, struct ALBank *parent)
{
    TRACE_ENTER(__func__)
    
    if (context == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> context is NULL\n", __func__, __LINE__);
    }
    
    if (ctl_file_contents == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> ctl_file_contents is NULL\n", __func__, __LINE__);
    }
    
    int32_t input_pos = load_from_offset;
    int i;

    struct ALInstrument *instrument = ALInstrument_new();

    snprintf(instrument->text_id, INST_OBJ_ID_STRING_LEN, "Instrument%04d", instrument->id);

    instrument->self_offset = load_from_offset;

    instrument->volume = ctl_file_contents[input_pos];
    input_pos += 1;

    instrument->pan = ctl_file_contents[input_pos];
    input_pos += 1;

    instrument->priority = ctl_file_contents[input_pos];
    input_pos += 1;

    instrument->flags = ctl_file_contents[input_pos];
    input_pos += 1;

    instrument->trem_type = ctl_file_contents[input_pos];
    input_pos += 1;

    instrument->trem_rate = ctl_file_contents[input_pos];
    input_pos += 1;

    instrument->trem_depth = ctl_file_contents[input_pos];
    input_pos += 1;

    instrument->trem_delay = ctl_file_contents[input_pos];
    input_pos += 1;

    instrument->vib_type = ctl_file_contents[input_pos];
    input_pos += 1;

    instrument->vib_rate = ctl_file_contents[input_pos];
    input_pos += 1;

    instrument->vib_depth = ctl_file_contents[input_pos];
    input_pos += 1;

    instrument->vib_delay = ctl_file_contents[input_pos];
    input_pos += 1;

    instrument->bend_range = BSWAP16_INLINE(*(uint16_t*)(&ctl_file_contents[input_pos]));
    input_pos += 2;

    instrument->sound_count = BSWAP16_INLINE(*(uint16_t*)(&ctl_file_contents[input_pos]));
    input_pos += 2;

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("instrument %d has %d sounds\n", instrument->id, instrument->sound_count);
    }

    if (parent != NULL)
    {
        ALInstrument_add_parent(instrument, parent);
    }

    if (instrument->sound_count < 1)
    {
        TRACE_LEAVE(__func__)

        return instrument;
    }

    instrument->sound_offsets = (int32_t *)malloc_zero(instrument->sound_count, sizeof(void*));
    instrument->sounds = (struct ALSound **)malloc_zero(instrument->sound_count, sizeof(void*));

    bswap32_chunk(instrument->sound_offsets, &ctl_file_contents[input_pos], instrument->sound_count);
    input_pos += instrument->sound_count * 4; /* 4 = sizeof offset */

    for (i=0; i<instrument->sound_count; i++)
    {
        if (instrument->sound_offsets[i] > 0)
        {
            struct ALSound *sound;

            if (IntHashTable_contains(context->seen_sound, instrument->sound_offsets[i]))
            {
                sound = IntHashTable_get(context->seen_sound, instrument->sound_offsets[i]);
                ALSound_add_parent(sound, instrument);
            }
            else
            {
                // parent reference is added in constructor
                sound = ALSound_new_from_ctl(context, ctl_file_contents, instrument->sound_offsets[i], instrument);
                IntHashTable_add(context->seen_sound, instrument->sound_offsets[i], sound);
            }

            instrument->sounds[i] = sound;
        }
    }

    TRACE_LEAVE(__func__)

    return instrument;
}

/**
 * Writes {@code struct ALInstrument} to .inst file, using current file seek position.
 * Writes any child information as well.
 * @param instrument: object to write.
 * @param fi: FileInfo.
*/
void ALInstrument_write_inst(struct ALInstrument *instrument, struct FileInfo *fi)
{
    TRACE_ENTER(__func__)
    
    if (instrument == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> instrument is NULL\n", __func__, __LINE__);
    }
    
    if (fi == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> fi is NULL\n", __func__, __LINE__);
    }

    int len;
    int i;

    instrument->visited = 1;

    for (i=0; i<instrument->sound_count; i++)
    {
        // don't write declaration more than once.
        if (instrument->sounds[i]->visited == 0)
        {
            ALSound_write_inst(instrument->sounds[i], fi);
        }
    }

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "instrument %s", instrument->text_id);
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, " {\n");
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    if (instrument->volume != 0)
    {
        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"volume = %d;\n", instrument->volume);
        FileInfo_fwrite(fi, g_write_buffer, len, 1);
    }

    if (instrument->pan != 0)
    {
        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"pan = %d;\n", instrument->pan);
        FileInfo_fwrite(fi, g_write_buffer, len, 1);
    }

    if (instrument->priority != 0)
    {
        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"priority = %d;\n", instrument->priority);
        FileInfo_fwrite(fi, g_write_buffer, len, 1);
    }

    if (instrument->flags != 0)
    {
        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"flags = %d;\n", instrument->flags);
        FileInfo_fwrite(fi, g_write_buffer, len, 1);
    }

    if (instrument->trem_type != 0)
    {
        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"tremType = %d;\n", instrument->trem_type);
        FileInfo_fwrite(fi, g_write_buffer, len, 1);
    }

    if (instrument->trem_rate != 0)
    {
        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"tremRate = %d;\n", instrument->trem_rate);
        FileInfo_fwrite(fi, g_write_buffer, len, 1);
    }

    if (instrument->trem_depth != 0)
    {
        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"tremDepth = %d;\n", instrument->trem_depth);
        FileInfo_fwrite(fi, g_write_buffer, len, 1);
    }

    if (instrument->trem_delay != 0)
    {
        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"tremDelay = %d;\n", instrument->trem_delay);
        FileInfo_fwrite(fi, g_write_buffer, len, 1);
    }

    if (instrument->vib_type != 0)
    {
        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"vibType = %d;\n", instrument->vib_type);
        FileInfo_fwrite(fi, g_write_buffer, len, 1);
    }

    if (instrument->vib_rate != 0)
    {
        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"vibRate = %d;\n", instrument->vib_rate);
        FileInfo_fwrite(fi, g_write_buffer, len, 1);
    }

    if (instrument->vib_depth != 0)
    {
        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"vibDepth = %d;\n", instrument->vib_depth);
        FileInfo_fwrite(fi, g_write_buffer, len, 1);
    }

    if (instrument->vib_delay != 0)
    {
        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"vibDelay = %d;\n", instrument->vib_delay);
        FileInfo_fwrite(fi, g_write_buffer, len, 1);
    }

    if (instrument->bend_range != 0)
    {
        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"bendRange = %d;\n", instrument->bend_range);
        FileInfo_fwrite(fi, g_write_buffer, len, 1);
    }

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "\n");
    FileInfo_fwrite(fi, g_write_buffer, len, 1);
    
    // Skip writing declaration above, but always write references.
    for (i=0; i<instrument->sound_count; i++)
    {
        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"sound [%d] = %s;\n", i, instrument->sounds[i]->text_id);
        FileInfo_fwrite(fi, g_write_buffer, len, 1);

        if (g_verbosity >= VERBOSE_DEBUG)
        {
            memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"# sound_offset = 0x%06x;\n", instrument->sound_offsets[i]);
            FileInfo_fwrite(fi, g_write_buffer, len, 1);
        }
    }

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "}\n");
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "\n");
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    TRACE_LEAVE(__func__)
}

/**
 * Base constructor. Only partially initializes bank. This should
 * only be called from other constructors.
 * @returns: pointer to new bank.
*/
struct ALBank *ALBank_new()
{
    TRACE_ENTER(__func__)

    struct ALBank *bank = (struct ALBank *)malloc_zero(1, sizeof(struct ALBank));

    bank->id = g_bank_id++;

    TRACE_LEAVE(__func__)

    return bank;
}

/**
 * Reads a single {@code struct ALBank} from a .ctl file that has been loaded into memory.
 * Allocates memory and calls _init_load for instruments and related if {@code inst_count} > 0.
 * @param context: context
 * @param ctl_file_contents: .ctl file.
 * @param load_from_offset: position in .ctl file to read data from.
 * @returns: pointer to new bank.
*/
struct ALBank *ALBank_new_from_ctl(struct CtlParseContext *context, uint8_t *ctl_file_contents, int32_t load_from_offset)
{
    TRACE_ENTER(__func__)
    
    if (context == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> context is NULL\n", __func__, __LINE__);
    }
    
    if (ctl_file_contents == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> ctl_file_contents is NULL\n", __func__, __LINE__);
    }

    int32_t input_pos = load_from_offset;
    int i;

    struct ALBank *bank = ALBank_new();

    snprintf(bank->text_id, INST_OBJ_ID_STRING_LEN, "Bank%04d", bank->id);

    bank->inst_count = BSWAP16_INLINE(*(uint16_t*)(&ctl_file_contents[input_pos]));
    input_pos += 2;

    bank->flags = ctl_file_contents[input_pos];
    input_pos += 1;

    bank->pad = ctl_file_contents[input_pos];
    input_pos += 1;

    bank->sample_rate = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    bank->percussion = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("bank %d has %d instruments\n", bank->id, bank->inst_count);
    }

    if (bank->inst_count < 1)
    {
        TRACE_LEAVE(__func__)

        return bank;
    }

    bank->inst_offsets = (int32_t *)malloc_zero(bank->inst_count, sizeof(void*));
    bank->instruments = (struct ALInstrument **)malloc_zero(bank->inst_count, sizeof(void*));

    bswap32_chunk(bank->inst_offsets, &ctl_file_contents[input_pos], bank->inst_count);
    input_pos += bank->inst_count * 4; /* 4 = sizeof offset */

    for (i=0; i<bank->inst_count; i++)
    {
        if (bank->inst_offsets[i] > 0)
        {
            struct ALInstrument *instrument;

            if (IntHashTable_contains(context->seen_instrument, bank->inst_offsets[i]))
            {
                instrument = IntHashTable_get(context->seen_instrument, bank->inst_offsets[i]);
                ALInstrument_add_parent(instrument, bank);
            }
            else
            {
                // parent reference is added in constructor
                instrument = ALInstrument_new_from_ctl(context, ctl_file_contents, bank->inst_offsets[i], bank);
                IntHashTable_add(context->seen_instrument, bank->inst_offsets[i], instrument);
            }

            bank->instruments[i] = instrument;
        }
    }

    TRACE_LEAVE(__func__)

    return bank;
}

/**
 * Writes {@code struct ALBank} to .inst file, using current file seek position.
 * Writes any child information as well.
 * @param bank: object to write.
 * @param fi: FileInfo.
*/
void ALBank_write_inst(struct ALBank *bank, struct FileInfo *fi)
{
    TRACE_ENTER(__func__)
    
    if (bank == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> bank is NULL\n", __func__, __LINE__);
    }
    
    if (fi == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> fi is NULL\n", __func__, __LINE__);
    }

    int len;
    int i;

    for (i=0; i<bank->inst_count; i++)
    {
        // don't write declaration more than once.
        if (bank->instruments[i]->visited == 0)
        {
            ALInstrument_write_inst(bank->instruments[i], fi);
        }
    }

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "bank %s", bank->text_id);
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, " {\n");
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"sampleRate = %d;\n", bank->sample_rate);
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    // Skip writing declaration above, but always write references.
    for (i=0; i<bank->inst_count; i++)
    {
        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"instrument [%d] = %s;\n", i, bank->instruments[i]->text_id);
        FileInfo_fwrite(fi, g_write_buffer, len, 1);

        if (g_verbosity >= VERBOSE_DEBUG)
        {
            memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"# inst_offset = 0x%06x;\n", bank->inst_offsets[i]);
            FileInfo_fwrite(fi, g_write_buffer, len, 1);
        }
    }

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "}\n");
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "\n");
    FileInfo_fwrite(fi, g_write_buffer, len, 1);

    TRACE_LEAVE(__func__)
}

/**
 * Base constructor. Only partially initializes bankfile. This should
 * only be called from other constructors.
 * @returns: pointer to new bank file.
*/
struct ALBankFile *ALBankFile_new()
{
    TRACE_ENTER(__func__)

    struct ALBankFile *bank_file = (struct ALBankFile *)malloc_zero(1, sizeof(struct ALBankFile));
    bank_file->id = g_bank_file_id++;

    TRACE_LEAVE(__func__)

    return bank_file;
}

/**
 * This is the main entry point for reading a .ctl file.
 * Reads a single {@code struct ALBankFile} from a .ctl file.
 * Allocates memory and calls _init_load for banks and related if {@code bank_count} > 0.
 * @param ctl_file: .ctl file.
 * @returns: pointer to new bank file.
*/
struct ALBankFile *ALBankFile_new_from_ctl(struct FileInfo *ctl_file)
{
    TRACE_ENTER(__func__)
    
    if (ctl_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> ctl_file is NULL\n", __func__, __LINE__);
    }

    int32_t input_pos = 0;
    int i;

    uint8_t *ctl_file_contents;
    FileInfo_get_file_contents(ctl_file, &ctl_file_contents);

    struct ALBankFile *bank_file = ALBankFile_new();
    struct CtlParseContext *context = CtlParseContext_new();

    snprintf(bank_file->text_id, INST_OBJ_ID_STRING_LEN, "BankFile%04d", bank_file->id);

    bank_file->revision = BSWAP16_INLINE(*(uint16_t*)(&ctl_file_contents[input_pos]));
    input_pos += 2;

    if (bank_file->revision != BANKFILE_MAGIC_BYTES)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> Error reading ctl file, revision number does not match. Expected 0x%04x, read 0x%04x.\n", __func__, __LINE__, BANKFILE_MAGIC_BYTES, bank_file->revision);
    }

    bank_file->bank_count = BSWAP16_INLINE(*(uint16_t*)(&ctl_file_contents[input_pos]));
    input_pos += 2;

    if (bank_file->bank_count < 1)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> ctl count=%d, nothing to do.\n", __func__, __LINE__, bank_file->bank_count);
    }

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("bank file %d has %d entries\n", bank_file->id, bank_file->bank_count);
    }

    // hmmmm, malloc should use current system's pointer size.
    bank_file->bank_offsets = (int32_t *)malloc_zero(bank_file->bank_count, sizeof(void*));
    bank_file->banks = (struct ALBank **)malloc_zero(bank_file->bank_count, sizeof(void*));

    bswap32_chunk(bank_file->bank_offsets, &ctl_file_contents[input_pos], bank_file->bank_count);
    input_pos += bank_file->bank_count * 4; /* 4 = sizeof offset */

    for (i=0; i<bank_file->bank_count; i++)
    {
        if (bank_file->bank_offsets[i] > 0)
        {
            bank_file->banks[i] = ALBank_new_from_ctl(context, ctl_file_contents, bank_file->bank_offsets[i]);
        }
    }

    free(ctl_file_contents);
    CtlParseContext_free(context);

    // At this point everything from the .ctl is read and loaded into the bank_file.
    // However, the associated write_order are based on the order read from the top down.
    // In order to correctly write back to .ctl later (if this is read to .inst file
    // and loaded again), then the write_order from the original .ctl needs to be preserved
    // to carry into the .inst file.
    ALBankFile_set_write_order_by_offset(bank_file);

    TRACE_LEAVE(__func__)

    return bank_file;
}

/**
 * This is the main entry point for writing an .inst file.
 * Writes {@code struct ALBankFile} to .inst file.
 * Writes any child information as well.
 * @param bank_file: object to write.
 * @param inst_filename: path to write to.
*/
void ALBankFile_write_inst(struct ALBankFile *bank_file, char* inst_filename)
{
    TRACE_ENTER(__func__)
    
    if (bank_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> bank_file is NULL\n", __func__, __LINE__);
    }
    
    if (inst_filename == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> inst_filename is NULL\n", __func__, __LINE__);
    }

    struct FileInfo *output;
    int i;

    ALBankFile_clear_visited_flags(bank_file);

    output = FileInfo_fopen(inst_filename, "w");

    for (i=0; i<bank_file->bank_count; i++)
    {
        ALBank_write_inst(bank_file->banks[i], output);
    }

    FileInfo_free(output);

    TRACE_LEAVE(__func__)
}

/**
 * Frees memory allocated to loop and all child objects.
 * @param loop: object to free.
*/
void ALADPCMLoop_free(struct ALADPCMLoop *loop)
{
    TRACE_ENTER(__func__)

    if (loop == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    free(loop);

    TRACE_LEAVE(__func__)
}

/**
 * Frees memory allocated to book and all child objects.
 * @param book: object to free.
*/
void ALADPCMBook_free(struct ALADPCMBook *book)
{
    TRACE_ENTER(__func__)

    if (book == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (book->book != NULL)
    {
        free(book->book);
    }

    free(book);

    TRACE_LEAVE(__func__)
}

/**
 * Frees memory allocated to loop and all child objects.
 * @param loop: object to free.
*/
void ALRawLoop_free(struct ALRawLoop *loop)
{
    TRACE_ENTER(__func__)

    if (loop == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    free(loop);

    TRACE_LEAVE(__func__)
}

/**
 * Iterates all parent objects and any references pointing to this
 * ALEnvelope are set to NULL.
 * @param envelope: object to remove references to.
*/
void ALEnvelope_notify_parents_null(struct ALEnvelope *envelope)
{
    TRACE_ENTER(__func__)

    struct LinkedListNode *node;

    if (envelope == NULL || envelope->parents == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    node = envelope->parents->head;
    while (node != NULL)
    {
        struct LinkedListNode *next = node->next;
        struct ALSound *sound = node->data;

        if (sound != NULL)
        {
            if (sound->envelope == envelope)
            {
                sound->envelope = NULL;
                LinkedListNode_free(envelope->parents, node);
            }
        }

        node = next;
    }

    TRACE_LEAVE(__func__)
}

/**
 * Frees memory allocated to envelope and all child objects.
 * @param envelope: object to free.
*/
void ALEnvelope_free(struct ALEnvelope *envelope)
{
    TRACE_ENTER(__func__)

    if (envelope == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    ALEnvelope_notify_parents_null(envelope);

    if (envelope->parents != NULL)
    {
        LinkedList_free(envelope->parents);
        envelope->parents= NULL;
    }

    free(envelope);

    TRACE_LEAVE(__func__)
}

/**
 * Iterates all parent objects and any references pointing to this
 * ALKeyMap are set to NULL.
 * @param keymap: object to remove references to.
*/
void ALKeyMap_notify_parents_null(struct ALKeyMap *keymap)
{
    TRACE_ENTER(__func__)

    struct LinkedListNode *node;

    if (keymap == NULL || keymap->parents == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    node = keymap->parents->head;
    while (node != NULL)
    {
        struct LinkedListNode *next = node->next;
        struct ALSound *sound = node->data;

        if (sound != NULL)
        {
            if (sound->keymap == keymap)
            {
                sound->keymap = NULL;
                LinkedListNode_free(keymap->parents, node);
            }
        }

        node = next;
    }

    TRACE_LEAVE(__func__)
}

/**
 * Frees memory allocated to keymap and all child objects.
 * @param keymap: object to free.
*/
void ALKeyMap_free(struct ALKeyMap *keymap)
{
    TRACE_ENTER(__func__)

    if (keymap == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    ALKeyMap_notify_parents_null(keymap);

    if (keymap->parents != NULL)
    {
        LinkedList_free(keymap->parents);
        keymap->parents= NULL;
    }

    free(keymap);

    TRACE_LEAVE(__func__)
}

/**
 * Iterates all parent objects and any references pointing to this
 * ALWaveTable are set to NULL.
 * @param wavetable: object to remove references to.
*/
void ALWaveTable_notify_parents_null(struct ALWaveTable *wavetable)
{
    TRACE_ENTER(__func__)

    struct LinkedListNode *node;

    if (wavetable == NULL || wavetable->parents == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    node = wavetable->parents->head;
    while (node != NULL)
    {
        struct LinkedListNode *next = node->next;
        struct ALSound *sound = node->data;

        if (sound != NULL)
        {
            if (sound->wavetable == wavetable)
            {
                sound->wavetable = NULL;
                LinkedListNode_free(wavetable->parents, node);
            }
        }

        node = next;
    }

    TRACE_LEAVE(__func__)
}

/**
 * Frees memory allocated to wavetable and all child objects.
 * @param wavetable: object to free.
*/
void ALWaveTable_free(struct ALWaveTable *wavetable)
{
    TRACE_ENTER(__func__)

    if (wavetable == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (wavetable->type == AL_ADPCM_WAVE)
    {
        if (wavetable->wave_info.adpcm_wave.loop != NULL)
        {
            ALADPCMLoop_free(wavetable->wave_info.adpcm_wave.loop);
            wavetable->wave_info.adpcm_wave.loop = NULL;
        }

        if (wavetable->wave_info.adpcm_wave.book != NULL)
        {
            ALADPCMBook_free(wavetable->wave_info.adpcm_wave.book);
            wavetable->wave_info.adpcm_wave.book = NULL;
        }
    }
    else if (wavetable->type == AL_RAW16_WAVE)
    {
        if (wavetable->wave_info.raw_wave.loop != NULL)
        {
            ALRawLoop_free(wavetable->wave_info.raw_wave.loop);
            wavetable->wave_info.raw_wave.loop = NULL;
        }
    }

    if (wavetable->aifc_path != NULL)
    {
        free(wavetable->aifc_path);
    }

    ALWaveTable_notify_parents_null(wavetable);

    if (wavetable->parents != NULL)
    {
        LinkedList_free(wavetable->parents);
        wavetable->parents= NULL;
    }

    free(wavetable);

    TRACE_LEAVE(__func__)
}

/**
 * Iterates all parent objects and any references pointing to this
 * ALSound are set to NULL.
 * @param sound: object to remove references to.
*/
void ALSound_notify_parents_null(struct ALSound *sound)
{
    TRACE_ENTER(__func__)

    struct LinkedListNode *node;

    if (sound == NULL || sound->parents == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    // Iterate each parent reference on `this`
    node = sound->parents->head;
    while (node != NULL)
    {
        struct LinkedListNode *next = node->next;
        struct ALInstrument *instrument = node->data;
        int remove_flag = 0;

        // If `this` parent is not null
        if (instrument != NULL && instrument->sounds != NULL)
        {
            // Iterate all the child `ALSound` of the current parent.
            int i;
            for (i=0; i<instrument->sound_count; i++)
            {
                struct ALSound *instrument_sound = instrument->sounds[i];

                // If the current `this->parent->child` points to `this`,
                // set the pointer to NULL. Mark the LinkedListNode for deletion,
                // but it can't be removed yet in case there's another 
                // reference to `this`.
                if (instrument_sound == sound)
                {
                    instrument->sounds[i] = NULL;
                    remove_flag = 1;
                }
            }
        }

        if (remove_flag)
        {
            LinkedListNode_free(sound->parents, node);
        }

        node = next;
    }

    TRACE_LEAVE(__func__)
}

/**
 * Frees memory allocated to sound and all child objects.
 * @param sound: object to free.
*/
void ALSound_free(struct ALSound *sound)
{
    TRACE_ENTER(__func__)

    if (sound == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (sound->envelope != NULL)
    {
        ALEnvelope_free(sound->envelope);
        sound->envelope = NULL;
    }

    if (sound->keymap != NULL)
    {
        ALKeyMap_free(sound->keymap);
        sound->keymap = NULL;
    }

    if (sound->wavetable != NULL)
    {
        ALWaveTable_free(sound->wavetable);
        sound->wavetable = NULL;
    }

    ALSound_notify_parents_null(sound);

    if (sound->parents != NULL)
    {
        LinkedList_free(sound->parents);
        sound->parents= NULL;
    }

    free(sound);

    TRACE_LEAVE(__func__)
}

/**
 * Iterates all parent objects and any references pointing to this
 * ALInstrument are set to NULL.
 * @param instrument: object to remove references to.
*/
void ALInstrument_notify_parents_null(struct ALInstrument *instrument)
{
    TRACE_ENTER(__func__)

    struct LinkedListNode *node;

    if (instrument == NULL || instrument->parents == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    // Iterate each parent reference on `this`
    node = instrument->parents->head;
    while (node != NULL)
    {
        struct LinkedListNode *next = node->next;
        struct ALBank *bank = node->data;
        int remove_flag = 0;

        // If `this` parent is not null
        if (bank != NULL && bank->instruments != NULL)
        {
            // Iterate all the child `ALInstrument` of the current parent.
            int i;
            for (i=0; i<bank->inst_count; i++)
            {
                struct ALInstrument *bank_instrument = bank->instruments[i];

                // If the current `this->parent->child` points to `this`,
                // set the pointer to NULL. Mark the LinkedListNode for deletion,
                // but it can't be removed yet in case there's another 
                // reference to `this`.
                if (bank_instrument == instrument)
                {
                    bank->instruments[i] = NULL;
                    remove_flag = 1;
                }
            }
        }

        if (remove_flag)
        {
            LinkedListNode_free(instrument->parents, node);
        }

        node = next;
    }

    TRACE_LEAVE(__func__)
}

/**
 * Frees memory allocated to instrument and all child objects.
 * @param instrument: object to free.
*/
void ALInstrument_free(struct ALInstrument *instrument)
{
    TRACE_ENTER(__func__)

    int i;

    if (instrument == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (instrument->sound_offsets != NULL)
    {
        free(instrument->sound_offsets);
        instrument->sound_offsets = NULL;
    }

    if (instrument->sounds != NULL)
    {
        for (i=0; i<instrument->sound_count; i++)
        {
            if (instrument->sounds[i] != NULL)
            {
                ALSound_free(instrument->sounds[i]);
                instrument->sounds[i] = NULL;
            }
        }

        free(instrument->sounds);
    }

    ALInstrument_notify_parents_null(instrument);

    if (instrument->parents != NULL)
    {
        LinkedList_free(instrument->parents);
        instrument->parents= NULL;
    }

    free(instrument);

    TRACE_LEAVE(__func__)
}

/**
 * Frees memory allocated to bank and all child objects.
 * @param bank: object to free.
*/
void ALBank_free(struct ALBank *bank)
{
    TRACE_ENTER(__func__)

    int i;

    if (bank == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (bank->inst_offsets != NULL)
    {
        free(bank->inst_offsets);
        bank->inst_offsets = NULL;
    }

    if (bank->instruments != NULL)
    {
        for (i=0; i<bank->inst_count; i++)
        {
            if (bank->instruments[i] != NULL)
            {
                ALInstrument_free(bank->instruments[i]);
                bank->instruments[i] = NULL;
            }
        }

        free(bank->instruments);
    }

    free(bank);

    TRACE_LEAVE(__func__)
}

/**
 * Frees memory allocated to bank file and all child objects.
 * @param bank_file: object to free.
*/
void ALBankFile_free(struct ALBankFile *bank_file)
{
    TRACE_ENTER(__func__)

    int i;

    if (bank_file == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (bank_file->bank_offsets != NULL)
    {
        free(bank_file->bank_offsets);
        bank_file->bank_offsets = NULL;
    }

    if (bank_file->banks != NULL)
    {
        for (i=0; i<bank_file->bank_count; i++)
        {
            if (bank_file->banks[i] != NULL)
            {
                ALBank_free(bank_file->banks[i]);
                bank_file->banks[i] = NULL;
            }
        }

        free(bank_file->banks);
    }

    free(bank_file);

    TRACE_LEAVE(__func__)
}

/**
 * Calculates new frequency based on detune and keybase parameters.
 * @param keybase: keybase (MIDI note).
 * @param detune: value in cents (1200 cents per octave).
 * @returns: adjusted frequency
*/
double detune_frequency(double hw_sample_rate, int keybase, int detune)
{
    TRACE_ENTER(__func__)

    if (detune < -100)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> detune=%d out of range. Valid range: (-100) - 100\n", __func__, __LINE__, detune);
    }

    if (detune > 100)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> detune=%d out of range. Valid range: (-100) - 100\n", __func__, __LINE__, detune);
    }

    if (keybase < 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> keybase=%d out of range. Valid range: 0-127\n", __func__, __LINE__, keybase);
    }

    if (keybase > 127)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> keybase=%d out of range. Valid range: 0-127\n", __func__, __LINE__, keybase);
    }

    if (hw_sample_rate < 1.0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> invalid hw_sample_rate=%f\n", __func__, __LINE__, hw_sample_rate);
    }

    // formula is:
    // hw_sample_rate / 2^( (60 - (keybase + detune/100))/12 )

    // 60 = C4, 12 = notes per octave
    double expon = (60.0 - (keybase + detune/100.0)) / 12.0;
    double denom = pow(2.0, expon);

    TRACE_LEAVE(__func__)

    return hw_sample_rate / denom;
}

/**
 * Searches a {@code ALBankFile}, returns the {@code ALKeyMap} with matching {@code text_id}.
 * @param bank_file: bank file to search.
 * @param keymap_text_id: text id of keymap to find.
 * @returns: keymap or NULL.
*/
struct ALKeyMap *ALBankFile_find_keymap_with_name(struct ALBankFile *bank_file, const char *keymap_text_id)
{
    TRACE_ENTER(__func__)
    
    if (bank_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> bank_file is NULL\n", __func__, __LINE__);
    }
    
    if (keymap_text_id == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> keymap_text_id is NULL\n", __func__, __LINE__);
    }

    int bank_index;
    int instrument_index;
    int sound_index;

    for (bank_index=0; bank_index<bank_file->bank_count; bank_index++)
    {
        struct ALBank *bank = bank_file->banks[bank_index];

        if (bank != NULL)
        {
            for (instrument_index=0; instrument_index<bank->inst_count; instrument_index++)
            {
                struct ALInstrument *instrument = bank->instruments[instrument_index];

                if (instrument != NULL)
                {
                    for (sound_index=0; sound_index<instrument->sound_count; sound_index++)
                    {
                        struct ALSound *sound = instrument->sounds[sound_index];

                        if (sound != NULL)
                        {
                            struct ALKeyMap *keymap = sound->keymap;

                            if (keymap != NULL)
                            {
                                if (strcmp(keymap->text_id, keymap_text_id) == 0)
                                {
                                    TRACE_LEAVE(__func__)
                                    return keymap;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    TRACE_LEAVE(__func__)
    return NULL;
}

/**
 * Searches a {@code ALBankFile}, returns the {@code ALSound} with matching {@code text_id}.
 * @param bank_file: bank file to search.
 * @param sound_text_id: text id of sound to find.
 * @returns: sound or NULL.
*/
struct ALSound *ALBankFile_find_sound_with_name(struct ALBankFile *bank_file, const char *sound_text_id)
{
    TRACE_ENTER(__func__)
    
    if (bank_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> bank_file is NULL\n", __func__, __LINE__);
    }
    
    if (sound_text_id == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> sound_text_id is NULL\n", __func__, __LINE__);
    }

    int bank_index;
    int instrument_index;
    int sound_index;

    for (bank_index=0; bank_index<bank_file->bank_count; bank_index++)
    {
        struct ALBank *bank = bank_file->banks[bank_index];

        if (bank != NULL)
        {
            for (instrument_index=0; instrument_index<bank->inst_count; instrument_index++)
            {
                struct ALInstrument *instrument = bank->instruments[instrument_index];

                if (instrument != NULL)
                {
                    for (sound_index=0; sound_index<instrument->sound_count; sound_index++)
                    {
                        struct ALSound *sound = instrument->sounds[sound_index];

                        if (sound != NULL)
                        {
                            if (strcmp(sound->text_id, sound_text_id) == 0)
                            {
                                TRACE_LEAVE(__func__)
                                return sound;
                            }
                        }
                    }
                }
            }
        }
    }

    TRACE_LEAVE(__func__)
    return NULL;
}

/**
 * Searches a {@code ALBankFile}. Sounds are iterated and child wavetable searched. If
 * the wavetable {@code aifc_path} ends with (or equals) {@code search_filename} then the parent
 * {@code ALSound} is returned.
 * @param bank_file: bank file to search.
 * @param search_filename: filename in aifc_path.
 * @returns: sound or NULL.
*/
struct ALSound *ALBankFile_find_sound_by_aifc_filename(struct ALBankFile *bank_file, const char *search_filename)
{
    TRACE_ENTER(__func__)
    
    if (bank_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> bank_file is NULL\n", __func__, __LINE__);
    }
    
    if (search_filename == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> search_filename is NULL\n", __func__, __LINE__);
    }

    int bank_index;
    int instrument_index;
    int sound_index;

    for (bank_index=0; bank_index<bank_file->bank_count; bank_index++)
    {
        struct ALBank *bank = bank_file->banks[bank_index];

        if (bank != NULL)
        {
            for (instrument_index=0; instrument_index<bank->inst_count; instrument_index++)
            {
                struct ALInstrument *instrument = bank->instruments[instrument_index];

                if (instrument != NULL)
                {
                    for (sound_index=0; sound_index<instrument->sound_count; sound_index++)
                    {
                        struct ALSound *sound = instrument->sounds[sound_index];

                        if (sound != NULL)
                        {
                            struct ALWaveTable *wavetable = sound->wavetable;

                            if (wavetable != NULL)
                            {
                                if (string_ends_with(wavetable->aifc_path, search_filename))
                                {
                                    TRACE_LEAVE(__func__)
                                    return sound;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    TRACE_LEAVE(__func__)
    return NULL;
}

/**
 * Visits all child elements and clears the `visited` flag.
 * @param bank_file: bank file to clear.
*/
void ALBankFile_clear_visited_flags(struct ALBankFile *bank_file)
{
    TRACE_ENTER(__func__)
    
    if (bank_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> bank_file is NULL\n", __func__, __LINE__);
    }

    int bank_count;

    for (bank_count=0; bank_count<bank_file->bank_count; bank_count++)
    {
        struct ALBank *bank = bank_file->banks[bank_count];

        if (bank != NULL)
        {
            int inst_count;
            for (inst_count=0; inst_count<bank->inst_count; inst_count++)
            {
                struct ALInstrument *instrument = bank->instruments[inst_count];

                if (instrument != NULL)
                {
                    int sound_count;

                    instrument->visited = 0;

                    for (sound_count=0; sound_count<instrument->sound_count; sound_count++)
                    {
                        struct ALSound *sound = instrument->sounds[sound_count];

                        if (sound != NULL)
                        {
                            struct ALEnvelope *envelope = sound->envelope;
                            struct ALKeyMap *keymap = sound->keymap;
                            struct ALWaveTable *wavetable = sound->wavetable;

                            sound->visited = 0;

                            if (envelope != NULL)
                            {
                                envelope->visited = 0;
                            }

                            if (keymap != NULL)
                            {
                                keymap->visited = 0;
                            }

                            if (wavetable != NULL)
                            {
                                wavetable->visited = 0;
                            }
                        }
                    }
                }
            }
        }
    }

    TRACE_LEAVE(__func__)
}

/**
 * Attempts to guess the .ctl file size that would result from
 * writing this bank file to disk. This is an over estimate.
 * @param bank_file: bank file to estimate size for.
 * @returns: size in bytes the bank file should fit into.
*/
size_t ALBankFile_estimate_ctl_filesize(struct ALBankFile *bank_file)
{
    TRACE_ENTER(__func__)

    size_t len = 0;
    int bank_count;

    if (bank_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> bank_file is NULL\n", __func__, __LINE__);
    }

    ALBankFile_clear_visited_flags(bank_file);

    len += 4; /* magic bytes, count */
    len += 4 * bank_file->bank_count; /* offsets */

    for (bank_count=0; bank_count<bank_file->bank_count; bank_count++)
    {
        struct ALBank *bank = bank_file->banks[bank_count];

        if (bank != NULL)
        {
            int inst_count;

            len += 12; /* instrument data */
            len += 4 * bank->inst_count; /* offsets */

            // I think there's a final zero word?
            len += 4;

            // I think this is 16-byte aligned? pad the estimate here just in case.
            len += 12;

            for (inst_count=0; inst_count<bank->inst_count; inst_count++)
            {
                struct ALInstrument *instrument = bank->instruments[inst_count];

                if (instrument != NULL)
                {
                    int sound_count;

                    if (instrument->visited == 1)
                    {
                        continue;
                    }

                    instrument->visited = 1;

                    len += 16; /* instrument data */
                    len += 4 * instrument->sound_count; /* offsets */

                    // I think there's a final zero word?
                    len += 4;

                    // I think this is 16-byte aligned? pad the estimate here just in case.
                    len += 12;

                    for (sound_count=0; sound_count<instrument->sound_count; sound_count++)
                    {
                        struct ALSound *sound = instrument->sounds[sound_count];

                        if (sound != NULL)
                        {
                            struct ALEnvelope *envelope = sound->envelope;
                            struct ALKeyMap *keymap = sound->keymap;
                            struct ALWaveTable *wavetable = sound->wavetable;

                            if (sound->visited == 1)
                            {
                                continue;
                            }

                            sound->visited = 1;

                            len += 16; /* sound data */

                            if (envelope != NULL && envelope->visited == 0)
                            {
                                envelope->visited = 1;

                                len += 16; /* envelope data (and padding) */
                            }

                            if (keymap != NULL && keymap->visited == 0)
                            {
                                keymap->visited = 1;

                                len += 8; /* keymap data */
                            }

                            if (wavetable != NULL && wavetable->visited == 0)
                            {
                                wavetable->visited = 1;

                                len += 24; /* wavetable data (and padding) */

                                // always assume there is a ALADPCMWaveInfo loop.
                                // note that the raw wave writes the state (`ADPCM_STATE_SIZE` number of bytes) but all zeros.
                                // loop:
                                len += 12 + 32; /* start, end, count + state */ 

                                if (wavetable->type == AL_ADPCM_WAVE)
                                {
                                    struct ALADPCMWaveInfo *info = &wavetable->wave_info.adpcm_wave;

                                    if (info->book != NULL)
                                    {
                                        len += 8 + (info->book->order * info->book->npredictors * 16); /* order, predictors + state */

                                        // I don't think this is 16-byte align, I think
                                        // it just adds 8 bytes?
                                        len += 8;
                                    }
                                }
                                // else, AL_RAW16_WAVE, but this is less space than AL_ADPCM_WAVE
                            }
                        }
                    }
                }
            }
        }
    }

    TRACE_LEAVE(__func__)

    return len;
}

/**
 * Allocates memory for a new context.
 * @returns: pointer to new context.
*/
static struct CtlParseContext *CtlParseContext_new()
{
    TRACE_ENTER(__func__)

    struct CtlParseContext *context = (struct CtlParseContext*)malloc_zero(1, sizeof(struct CtlParseContext));

    context->seen_sound = IntHashTable_new();
    context->seen_instrument = IntHashTable_new();
    context->seen_envelope = IntHashTable_new();
    context->seen_wavetable = IntHashTable_new();
    context->seen_keymap = IntHashTable_new();

    TRACE_LEAVE(__func__)

    return context;
}

/**
 * Frees all memory associated with context.
 * @param context: object to free.
*/
static void CtlParseContext_free(struct CtlParseContext *context)
{
    TRACE_ENTER(__func__)

    IntHashTable_free(context->seen_sound);
    IntHashTable_free(context->seen_instrument);
    IntHashTable_free(context->seen_envelope);
    IntHashTable_free(context->seen_wavetable);
    IntHashTable_free(context->seen_keymap);

    free(context);

    TRACE_LEAVE(__func__)
}



/**
 * Default wavetable_init method if not set externally.
 * Sets text id to 4 digit id without any filename.
 * Allocates memory for {@code wavetable->aifc_path}.
 * @param wavetable: wavetable to init.
*/
static void ALWaveTable_init_default_set_aifc_path(struct ALWaveTable *wavetable)
{
    TRACE_ENTER(__func__)
    
    if (wavetable == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> wavetable is NULL\n", __func__, __LINE__);
    }

    struct ALSound* sound = NULL;
    struct ALInstrument *instrument = NULL;

    size_t filesystem_path_len = 0;
    char *filesystem_path;

    char *local_output_dir = ""; // empty string
    char *local_filename_prefix = ""; // empty string

    if (g_output_dir != NULL)
    {
        local_output_dir = g_output_dir;
    }

    if (g_filename_prefix != NULL)
    {
        local_filename_prefix = g_filename_prefix;
    }

    if (wavetable->parents != NULL
        && wavetable->parents->head != NULL
        && wavetable->parents->head->data != NULL)
    {
        sound = (struct ALSound*)wavetable->parents->head->data;
    }

    if (sound != NULL
        && sound->parents != NULL
        && sound->parents->head != NULL
        && sound->parents->head->data != NULL)
    {
        instrument = (struct ALInstrument *)sound->parents->head->data;
    }

    if (instrument != NULL)
    {
        filesystem_path_len = snprintf(NULL, 0, "%s%sinst_%04d_%04d%s", local_output_dir, local_filename_prefix, instrument->id, wavetable->id, NAUDIO_AIFC_OUT_DEFAULT_EXTENSION) + 1;
        filesystem_path = (char *)malloc_zero(filesystem_path_len + 1, 1);
        filesystem_path_len = snprintf(filesystem_path, filesystem_path_len, "%s%sinst_%04d_%04d%s", local_output_dir, local_filename_prefix, instrument->id, wavetable->id, NAUDIO_AIFC_OUT_DEFAULT_EXTENSION);
    }
    else
    {
        filesystem_path_len = snprintf(NULL, 0, "%s%s%04d%s", local_output_dir, local_filename_prefix, wavetable->id, NAUDIO_AIFC_OUT_DEFAULT_EXTENSION) + 1;
        filesystem_path = (char *)malloc_zero(filesystem_path_len + 1, 1);
        filesystem_path_len = snprintf(filesystem_path, filesystem_path_len, "%s%s%04d%s", local_output_dir, local_filename_prefix, wavetable->id, NAUDIO_AIFC_OUT_DEFAULT_EXTENSION);
    }

    wavetable->aifc_path = filesystem_path;

    TRACE_LEAVE(__func__)
}

/**
 * Merge sort comparison function.
 * Sorts on sound->keymap->self_offset.
 * Use this to sort smallest to largest.
 * @param first: first node
 * @param second: second node
 * @returns: comparison result
*/
static int LinkedListNode_sound_keymap_offset_compare_smaller(struct LinkedListNode *first, struct LinkedListNode *second)
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
        struct ALSound *sound_first = (struct ALSound *)first->data;
        struct ALSound *sound_second = (struct ALSound *)second->data;
       
        if ((sound_first == NULL || sound_first->keymap == NULL) && (sound_second == NULL || sound_second->keymap == NULL))
        {
            ret = 0;
        }
        else if ((sound_first == NULL || sound_first->keymap == NULL) && sound_second != NULL)
        {
            ret = 1;
        }
        else if (sound_first != NULL && (sound_second == NULL || sound_second->keymap == NULL))
        {
            ret = -1;
        }
        else
        {
            if (sound_first->keymap->self_offset < sound_second->keymap->self_offset)
            {
                ret = -1;
            }
            else if (sound_first->keymap->self_offset > sound_second->keymap->self_offset)
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

/**
 * Merge sort comparison function.
 * Sorts on sound->envelope->self_offset.
 * Use this to sort smallest to largest.
 * @param first: first node
 * @param second: second node
 * @returns: comparison result
*/
static int LinkedListNode_sound_envelope_offset_compare_smaller(struct LinkedListNode *first, struct LinkedListNode *second)
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
        struct ALSound *sound_first = (struct ALSound *)first->data;
        struct ALSound *sound_second = (struct ALSound *)second->data;
       
        if ((sound_first == NULL || sound_first->envelope == NULL) && (sound_second == NULL || sound_second->envelope == NULL))
        {
            ret = 0;
        }
        else if ((sound_first == NULL || sound_first->envelope == NULL) && sound_second != NULL)
        {
            ret = 1;
        }
        else if (sound_first != NULL && (sound_second == NULL || sound_second->envelope == NULL))
        {
            ret = -1;
        }
        else
        {
            if (sound_first->envelope->self_offset < sound_second->envelope->self_offset)
            {
                ret = -1;
            }
            else if (sound_first->envelope->self_offset > sound_second->envelope->self_offset)
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

/**
 * Merge sort comparison function.
 * Sorts on sound->self_offset.
 * Use this to sort smallest to largest.
 * @param first: first node
 * @param second: second node
 * @returns: comparison result
*/
static int LinkedListNode_sound_offset_compare_smaller(struct LinkedListNode *first, struct LinkedListNode *second)
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
        struct ALSound *sound_first = (struct ALSound *)first->data;
        struct ALSound *sound_second = (struct ALSound *)second->data;
       
        if (sound_first == NULL && sound_second == NULL)
        {
            ret = 0;
        }
        else if (sound_first == NULL && sound_second != NULL)
        {
            ret = 1;
        }
        else if (sound_first != NULL && sound_second == NULL)
        {
            ret = -1;
        }
        else
        {
            if (sound_first->self_offset < sound_second->self_offset)
            {
                ret = -1;
            }
            else if (sound_first->self_offset > sound_second->self_offset)
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

/**
 * Iterate the bank file and update envelope and keymap write_order
 * based on their offsets.
 * @param bank_file: bank_file to order.
*/
static void ALBankFile_set_write_order_by_offset(struct ALBankFile *bank_file)
{
    TRACE_ENTER(__func__)

    /**
     * Iterate bank_file and collect all sounds into a list.
     * 
     * sort the sounds list by wavetable offset.
     * iterate sounds list and set wavetable write_order from sort order.
     * 
     * sort the sounds list by envelope offset.
     * iterate sounds list and set envelope write_order from sort order.
    */
    
    if (bank_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> bank_file is NULL\n", __func__, __LINE__);
    }

    struct LinkedList *list_sounds = LinkedList_new();
    struct LinkedListNode *node;
    int bank_count;
    int write_order;

    ALBankFile_clear_visited_flags(bank_file);

    for (bank_count=0; bank_count<bank_file->bank_count; bank_count++)
    {
        struct ALBank *bank = bank_file->banks[bank_count];

        if (bank != NULL)
        {
            int inst_count;

            for (inst_count=0; inst_count<bank->inst_count; inst_count++)
            {
                struct ALInstrument *instrument = bank->instruments[inst_count];

                if (instrument != NULL)
                {
                    int sound_count;

                    if (instrument->visited == 1)
                    {
                        continue;
                    }

                    instrument->visited = 1;

                    for (sound_count=0; sound_count<instrument->sound_count; sound_count++)
                    {
                        struct ALSound *sound = instrument->sounds[sound_count];

                        if (sound != NULL)
                        {
                            if (sound->visited == 1)
                            {
                                continue;
                            }

                            sound->visited = 1;

                            node = LinkedListNode_new();
                            node->data = sound;
                            LinkedList_append_node(list_sounds, node);
                        }
                    }
                }
            }
        }
    }

    LinkedList_merge_sort(list_sounds, LinkedListNode_sound_offset_compare_smaller);
    ALBankFile_clear_visited_flags(bank_file);

    // start at non-zero.
    write_order = 1;

    node = list_sounds->head;
    while (node != NULL)
    {
        struct ALSound *sound = (struct ALSound *)node->data;

        if (sound->visited == 0)
        {
            sound->ctl_write_order = write_order;
            write_order++;
            sound->visited = 1;
        }

        node = node->next;
    }

    LinkedList_merge_sort(list_sounds, LinkedListNode_sound_envelope_offset_compare_smaller);
    ALBankFile_clear_visited_flags(bank_file);

    // start at non-zero.
    write_order = 1;

    node = list_sounds->head;
    while (node != NULL)
    {
        struct ALSound *sound = (struct ALSound *)node->data;

        if (sound->envelope != NULL && sound->envelope->visited == 0)
        {
            sound->envelope->ctl_write_order = write_order;
            write_order++;
            sound->envelope->visited = 1;
        }

        node = node->next;
    }

    LinkedList_merge_sort(list_sounds, LinkedListNode_sound_keymap_offset_compare_smaller);
    ALBankFile_clear_visited_flags(bank_file);

    // start at non-zero.
    write_order = 1;

    node = list_sounds->head;
    while (node != NULL)
    {
        struct ALSound *sound = (struct ALSound *)node->data;

        if (sound->keymap != NULL && sound->keymap->visited == 0)
        {
            sound->keymap->ctl_write_order = write_order;
            write_order++;
            sound->keymap->visited = 1;
        }

        node = node->next;
    }

    LinkedList_free(list_sounds);

    TRACE_LEAVE(__func__)
}