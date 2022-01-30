#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>
#include "debug.h"
#include "common.h"
#include "utility.h"
#include "naudio.h"

/**
 * This file contains primary code for supporting Rare's audio structs,
 * and Nintendo's (libultra) audio structs.
*/

/**
 * Callback to initialize wavetable object.
 * If not set externally, will be set to @see ALWaveTable_init_default_set_aifc_path.
*/
wavetable_init_callback wavetable_init_callback_ptr = NULL;

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
 * Reads a single {@code struct ALRawLoop} from a .ctl file that has been loaded into memory.
 * No memory allocation performed.
 * @param ctl_file_contents: .ctl file.
 * @param load_from_offset: position in .ctl file to read data from.
 * @returns: pointer to new loop.
*/
struct ALRawLoop *ALRawLoop_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset)
{
    TRACE_ENTER(__func__)

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

    static int32_t envelope_id = 0;

    struct ALEnvelope *envelope = (struct ALEnvelope *)malloc_zero(1, sizeof(struct ALEnvelope));

    envelope->id = envelope_id++;

    TRACE_LEAVE(__func__)

    return envelope;
}

/**
 * Reads a single {@code struct ALEnvelope} from a .ctl file that has been loaded into memory.
 * No memory allocation performed.
 * @param ctl_file_contents: .ctl file.
 * @param load_from_offset: position in .ctl file to read data from.
 * @returns: pointer to new envelope.
*/
struct ALEnvelope *ALEnvelope_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset)
{
    TRACE_ENTER(__func__)

    int32_t input_pos = load_from_offset;

    struct ALEnvelope *envelope = ALEnvelope_new();

    snprintf(envelope->text_id, INST_OBJ_ID_STRING_LEN, "Envelope%04d", envelope->id);

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

    TRACE_LEAVE(__func__)

    return envelope;
}

/**
 * Writes {@code struct ALEnvelope} to .inst file, using current file seek position.
 * Writes any child information as well.
 * @param envelope: object to write.
 * @param fi: file_info.
*/
void ALEnvelope_write_to_fp(struct ALEnvelope *envelope, struct file_info *fi)
{
    TRACE_ENTER(__func__)

    int len;

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "envelope %s", envelope->text_id);
    file_info_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, " {\n");
    file_info_fwrite(fi, g_write_buffer, len, 1);

    if (g_output_mode == OUTPUT_MODE_SFX)
    {
        // the following options are always written, even if zero.

        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"attackTime = %d;\n", envelope->attack_time);
        file_info_fwrite(fi, g_write_buffer, len, 1);

        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"attackVolume = %d;\n", envelope->attack_volume);
        file_info_fwrite(fi, g_write_buffer, len, 1);

        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"decayTime = %d;\n", envelope->decay_time);
        file_info_fwrite(fi, g_write_buffer, len, 1);

        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"decayVolume = %d;\n", envelope->decay_volume);
        file_info_fwrite(fi, g_write_buffer, len, 1);

        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"releaseTime = %d;\n", envelope->release_time);
        file_info_fwrite(fi, g_write_buffer, len, 1);
    }

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "}\n");
    file_info_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "\n");
    file_info_fwrite(fi, g_write_buffer, len, 1);

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

    static int32_t keymap_id = 0;

    struct ALKeyMap *keymap = (struct ALKeyMap *)malloc_zero(1, sizeof(struct ALKeyMap));

    keymap->id = keymap_id++;

    TRACE_LEAVE(__func__)

    return keymap;
}

/**
 * Reads a single {@code struct ALKeyMap} from a .ctl file that has been loaded into memory.
 * No memory allocation performed.
 * @param ctl_file_contents: .ctl file.
 * @param load_from_offset: position in .ctl file to read data from.
 * @returns: pointer to new keymap.
*/
struct ALKeyMap *ALKeyMap_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset)
{
    TRACE_ENTER(__func__)

    int32_t input_pos = load_from_offset;

    struct ALKeyMap *keymap = ALKeyMap_new();
    snprintf(keymap->text_id, INST_OBJ_ID_STRING_LEN, "Keymap%04d", keymap->id);

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

    TRACE_LEAVE(__func__)

    return keymap;
}

/**
 * Writes {@code struct ALKeyMap} to .inst file, using current file seek position.
 * Writes any child information as well.
 * @param keymap: object to write.
 * @param fi: file_info.
*/
void ALKeyMap_write_to_fp(struct ALKeyMap *keymap, struct file_info *fi)
{
    TRACE_ENTER(__func__)

    int len;

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "keymap %s", keymap->text_id);
    file_info_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, " {\n");
    file_info_fwrite(fi, g_write_buffer, len, 1);

    if (g_output_mode == OUTPUT_MODE_SFX)
    {
        // the following options are always written, even if zero.

        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"velocityMin = %d;\n", keymap->velocity_min);
        file_info_fwrite(fi, g_write_buffer, len, 1);

        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"velocityMax = %d;\n", keymap->velocity_max);
        file_info_fwrite(fi, g_write_buffer, len, 1);

        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"keyMin = %d;\n", keymap->key_min);
        file_info_fwrite(fi, g_write_buffer, len, 1);

        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"keyMax = %d;\n", keymap->key_max);
        file_info_fwrite(fi, g_write_buffer, len, 1);

        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"keyBase = %d;\n", keymap->key_base);
        file_info_fwrite(fi, g_write_buffer, len, 1);

        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"detune = %d;\n", keymap->detune);
        file_info_fwrite(fi, g_write_buffer, len, 1);
    }

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "}\n");
    file_info_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "\n");
    file_info_fwrite(fi, g_write_buffer, len, 1);

    TRACE_LEAVE(__func__)
}

/**
 * Default wavetable_init method if not set externally.
 * Sets text id to 4 digit id without any filename.
 * Allocates memory for {@code wavetable->aifc_path}.
 * @param wavetable: wavetable to init.
*/
void ALWaveTable_init_default_set_aifc_path(struct ALWaveTable *wavetable)
{
    TRACE_ENTER(__func__)

    size_t len;
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "%s%s%04d%s", g_output_dir, g_filename_prefix, wavetable->id, NAUDIO_AIFC_OUT_DEFAULT_EXTENSION);

    // g_write_buffer has terminating '\0', but that's not counted in len
    len++;
    wavetable->aifc_path = (char *)malloc_zero(len, 1);
    strncpy(wavetable->aifc_path, g_write_buffer, len);

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

    static int32_t wavetable_id = 0;

    struct ALWaveTable *wavetable = (struct ALWaveTable *)malloc_zero(1, sizeof(struct ALWaveTable));

    wavetable->id = wavetable_id++;

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
 * @returns: pointer to new wavetable.
*/
struct ALWaveTable *ALWaveTable_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset)
{
    TRACE_ENTER(__func__)
    
    int32_t input_pos = load_from_offset;

    struct ALWaveTable *wavetable = ALWaveTable_new();

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);

    if (wavetable_init_callback_ptr == NULL)
    {
        wavetable_init_callback_ptr = ALWaveTable_init_default_set_aifc_path;
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

    static int32_t sound_id = 0;

    struct ALSound *sound = (struct ALSound *)malloc_zero(1, sizeof(struct ALSound));

    sound->id = sound_id++;

    TRACE_LEAVE(__func__)

    return sound;
}

/**
 * Reads a single {@code struct ALSound} from a .ctl file that has been loaded into memory.
 * Allocates memory and calls _init_load for child envelope, keymap, wavetable if present.
 * @param ctl_file_contents: .ctl file.
 * @param load_from_offset: position in .ctl file to read data from.
 * @returns: pointer to new sound.
*/
struct ALSound *ALSound_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset)
{
    TRACE_ENTER(__func__)

    int32_t input_pos = load_from_offset;

    struct ALSound *sound = ALSound_new();
    snprintf(sound->text_id, INST_OBJ_ID_STRING_LEN, "Sound%04d", sound->id);

    sound->envelope_offset = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    sound->key_map_offset = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
    input_pos += 4;

    sound->wavetable_offfset = BSWAP32_INLINE(*(uint32_t*)(&ctl_file_contents[input_pos]));
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

    if (sound->envelope_offset > 0)
    {
        sound->envelope = ALEnvelope_new_from_ctl(ctl_file_contents, sound->envelope_offset);
    }

    if (sound->key_map_offset > 0)
    {
        sound->keymap = ALKeyMap_new_from_ctl(ctl_file_contents, sound->key_map_offset);
    }

    if (sound->wavetable_offfset > 0)
    {
        sound->wavetable = ALWaveTable_new_from_ctl(ctl_file_contents, sound->wavetable_offfset);
    }
    
    TRACE_LEAVE(__func__)

    return sound;
}

/**
 * Writes {@code struct ALSound} to .inst file, using current file seek position.
 * Writes any child information as well.
 * @param sound: object to write.
 * @param fi: file_info.
*/
void ALSound_write_to_fp(struct ALSound *sound, struct file_info *fi)
{
    TRACE_ENTER(__func__)

    int len;

    if (sound->envelope_offset > 0)
    {
        ALEnvelope_write_to_fp(sound->envelope, fi);
    }

    if (sound->key_map_offset > 0)
    {
        ALKeyMap_write_to_fp(sound->keymap, fi);
    }

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "sound %s", sound->text_id);
    file_info_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, " {\n");
    file_info_fwrite(fi, g_write_buffer, len, 1);

    if (g_output_mode == OUTPUT_MODE_SFX)
    {
        if (sound->wavetable_offfset != 0)
        {
            memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"use (\"%s\");\n", sound->wavetable->aifc_path);
            file_info_fwrite(fi, g_write_buffer, len, 1);

            if (g_verbosity >= VERBOSE_DEBUG)
            {
                memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
                len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"# wavetable_offfset = 0x%06x;\n", sound->wavetable_offfset);
                file_info_fwrite(fi, g_write_buffer, len, 1);
            }
        }

        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "\n");
        file_info_fwrite(fi, g_write_buffer, len, 1);
        
        if (sound->sample_pan != 0)
        {
            memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"pan = %d;\n", sound->sample_pan);
            file_info_fwrite(fi, g_write_buffer, len, 1);
        }
        
        if (sound->sample_volume != 0)
        {
            memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"volume = %d;\n", sound->sample_volume);
            file_info_fwrite(fi, g_write_buffer, len, 1);
        }
        
        if (sound->flags != 0)
        {
            memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"flags = %d;\n", sound->flags);
            file_info_fwrite(fi, g_write_buffer, len, 1);
        }
        
        if (sound->envelope_offset > 0)
        {
            memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"envelope = %s;\n", sound->envelope->text_id);
            file_info_fwrite(fi, g_write_buffer, len, 1);

            if (g_verbosity >= VERBOSE_DEBUG)
            {
                memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
                len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"# envelope_offset = 0x%06x;\n", sound->envelope_offset);
                file_info_fwrite(fi, g_write_buffer, len, 1);
            }
        }
        
        if (sound->key_map_offset > 0)
        {
            memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"keymap = %s;\n", sound->keymap->text_id);
            file_info_fwrite(fi, g_write_buffer, len, 1);

            if (g_verbosity >= VERBOSE_DEBUG)
            {
                memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
                len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"# key_map_offset = 0x%06x;\n", sound->key_map_offset);
                file_info_fwrite(fi, g_write_buffer, len, 1);
            }
        }
    }

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "}\n");
    file_info_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "\n");
    file_info_fwrite(fi, g_write_buffer, len, 1);

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

    static int32_t instrument_id = 0;

    struct ALInstrument *instrument = (struct ALInstrument *)malloc_zero(1, sizeof(struct ALInstrument));

    instrument->id = instrument_id++;

    TRACE_LEAVE(__func__)

    return instrument;
}

/**
 * Reads a single {@code struct ALInstrument} from a .ctl file that has been loaded into memory.
 * Allocates memory and calls _init_load for sounds and related if {@code sound_count} > 0.
 * @param instrument: object to write to.
 * @param ctl_file_contents: .ctl file.
 * @param load_from_offset: position in .ctl file to read data from.
*/
struct ALInstrument *ALInstrument_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset)
{
    TRACE_ENTER(__func__)
    
    int32_t input_pos = load_from_offset;
    int i;

    struct ALInstrument *instrument = ALInstrument_new();

    snprintf(instrument->text_id, INST_OBJ_ID_STRING_LEN, "Instrument%04d", instrument->id);

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

    if (instrument->sound_count < 1)
    {
        TRACE_LEAVE(__func__)

        return instrument;
    }

    instrument->sound_offsets = (int32_t *)malloc_zero(instrument->sound_count, sizeof(void*));
    instrument->sounds = (struct ALSound **)malloc_zero(instrument->sound_count, sizeof(void*));

    bswap32_memcpy(instrument->sound_offsets, &ctl_file_contents[input_pos], instrument->sound_count);
    input_pos += instrument->sound_count * 4; /* 4 = sizeof offset */

    for (i=0; i<instrument->sound_count; i++)
    {
        if (instrument->sound_offsets[i] > 0)
        {
            instrument->sounds[i] = ALSound_new_from_ctl(ctl_file_contents, instrument->sound_offsets[i]);
        }
    }

    TRACE_LEAVE(__func__)

    return instrument;
}

/**
 * Writes {@code struct ALInstrument} to .inst file, using current file seek position.
 * Writes any child information as well.
 * @param instrument: object to write.
 * @param fi: file_info.
*/
void ALInstrument_write_to_fp(struct ALInstrument *instrument, struct file_info *fi)
{
    TRACE_ENTER(__func__)

    int len;
    int i;

    for (i=0; i<instrument->sound_count; i++)
    {
        ALSound_write_to_fp(instrument->sounds[i], fi);
    }

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "instrument %s", instrument->text_id);
    file_info_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, " {\n");
    file_info_fwrite(fi, g_write_buffer, len, 1);

    if (g_output_mode == OUTPUT_MODE_SFX)
    {
        if (instrument->volume != 0)
        {
            memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"volume = %d;\n", instrument->volume);
            file_info_fwrite(fi, g_write_buffer, len, 1);
        }

        if (instrument->pan != 0)
        {
            memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"pan = %d;\n", instrument->pan);
            file_info_fwrite(fi, g_write_buffer, len, 1);
        }

        if (instrument->priority != 0)
        {
            memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"priority = %d;\n", instrument->priority);
            file_info_fwrite(fi, g_write_buffer, len, 1);
        }

        if (instrument->flags != 0)
        {
            memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"flags = %d;\n", instrument->flags);
            file_info_fwrite(fi, g_write_buffer, len, 1);
        }

        if (instrument->trem_type != 0)
        {
            memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"tremType = %d;\n", instrument->trem_type);
            file_info_fwrite(fi, g_write_buffer, len, 1);
        }

        if (instrument->trem_rate != 0)
        {
            memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"tremRate = %d;\n", instrument->trem_rate);
            file_info_fwrite(fi, g_write_buffer, len, 1);
        }

        if (instrument->trem_depth != 0)
        {
            memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"tremDepth = %d;\n", instrument->trem_depth);
            file_info_fwrite(fi, g_write_buffer, len, 1);
        }

        if (instrument->trem_delay != 0)
        {
            memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"tremDelay = %d;\n", instrument->trem_delay);
            file_info_fwrite(fi, g_write_buffer, len, 1);
        }

        if (instrument->vib_type != 0)
        {
            memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"vibType = %d;\n", instrument->vib_type);
            file_info_fwrite(fi, g_write_buffer, len, 1);
        }

        if (instrument->vib_rate != 0)
        {
            memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"vibRate = %d;\n", instrument->vib_rate);
            file_info_fwrite(fi, g_write_buffer, len, 1);
        }

        if (instrument->vib_depth != 0)
        {
            memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"vibDepth = %d;\n", instrument->vib_depth);
            file_info_fwrite(fi, g_write_buffer, len, 1);
        }

        if (instrument->vib_delay != 0)
        {
            memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"vibDelay = %d;\n", instrument->vib_delay);
            file_info_fwrite(fi, g_write_buffer, len, 1);
        }

        if (instrument->bend_range != 0)
        {
            memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"bendRange = %d;\n", instrument->bend_range);
            file_info_fwrite(fi, g_write_buffer, len, 1);
        }

        memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
        len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "\n");
        file_info_fwrite(fi, g_write_buffer, len, 1);
        
        for (i=0; i<instrument->sound_count; i++)
        {
            memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"sound [%d] = %s;\n", i, instrument->sounds[i]->text_id);
            file_info_fwrite(fi, g_write_buffer, len, 1);

            if (g_verbosity >= VERBOSE_DEBUG)
            {
                memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
                len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"# sound_offset = 0x%06x;\n", instrument->sound_offsets[i]);
                file_info_fwrite(fi, g_write_buffer, len, 1);
            }
        }
    }

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "}\n");
    file_info_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "\n");
    file_info_fwrite(fi, g_write_buffer, len, 1);

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

    static int32_t bank_id = 0;

    struct ALBank *bank = (struct ALBank *)malloc_zero(1, sizeof(struct ALBank));

    bank->id = bank_id++;

    TRACE_LEAVE(__func__)

    return bank;
}

/**
 * Reads a single {@code struct ALBank} from a .ctl file that has been loaded into memory.
 * Allocates memory and calls _init_load for instruments and related if {@code inst_count} > 0.
 * @param ctl_file_contents: .ctl file.
 * @param load_from_offset: position in .ctl file to read data from.
 * @returns: pointer to new bank.
*/
struct ALBank *ALBank_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset)
{
    TRACE_ENTER(__func__)

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

    bswap32_memcpy(bank->inst_offsets, &ctl_file_contents[input_pos], bank->inst_count);
    input_pos += bank->inst_count * 4; /* 4 = sizeof offset */

    for (i=0; i<bank->inst_count; i++)
    {
        if (bank->inst_offsets[i] > 0)
        {
            bank->instruments[i] = ALInstrument_new_from_ctl(ctl_file_contents, bank->inst_offsets[i]);
        }
    }

    TRACE_LEAVE(__func__)

    return bank;
}

/**
 * Writes {@code struct ALBank} to .inst file, using current file seek position.
 * Writes any child information as well.
 * @param bank: object to write.
 * @param fi: file_info.
*/
void ALBank_write_to_fp(struct ALBank *bank, struct file_info *fi)
{
    TRACE_ENTER(__func__)

    int len;
    int i;

    for (i=0; i<bank->inst_count; i++)
    {
        ALInstrument_write_to_fp(bank->instruments[i], fi);
    }

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "bank %s", bank->text_id);
    file_info_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, " {\n");
    file_info_fwrite(fi, g_write_buffer, len, 1);

    if (g_output_mode == OUTPUT_MODE_SFX)
    {
        for (i=0; i<bank->inst_count; i++)
        {
            memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
            len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"instrument [%d] = %s;\n", i, bank->instruments[i]->text_id);
            file_info_fwrite(fi, g_write_buffer, len, 1);

            if (g_verbosity >= VERBOSE_DEBUG)
            {
                memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
                len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, TEXT_INDENT"# inst_offset = 0x%06x;\n", bank->inst_offsets[i]);
                file_info_fwrite(fi, g_write_buffer, len, 1);
            }
        }
    }

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "}\n");
    file_info_fwrite(fi, g_write_buffer, len, 1);

    memset(g_write_buffer, 0, WRITE_BUFFER_LEN);
    len = snprintf(g_write_buffer, WRITE_BUFFER_LEN, "\n");
    file_info_fwrite(fi, g_write_buffer, len, 1);

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

    static int32_t bank_file_id = 0;

    struct ALBankFile *bank_file = (struct ALBankFile *)malloc_zero(1, sizeof(struct ALBankFile));
    bank_file->id = bank_file_id++;

    TRACE_LEAVE(__func__)

    return bank_file;
}

/**
 * This is the main entry point for reading a .ctl file.
 * Reads a single {@code struct ALBankFile} from a .ctl file that has been loaded into memory.
 * Allocates memory and calls _init_load for banks and related if {@code bank_count} > 0.
 * @param ctl_file_contents: .ctl file.
 * @param load_from_offset: position in .ctl file to read data from.
 * @returns: pointer to new bank file.
*/
struct ALBankFile *ALBankFile_new_from_ctl(uint8_t *ctl_file_contents)
{
    TRACE_ENTER(__func__)

    int32_t input_pos = 0;
    int i;

    struct ALBankFile *bank_file = ALBankFile_new();

    snprintf(bank_file->text_id, INST_OBJ_ID_STRING_LEN, "BankFile%04d", bank_file->id);

    bank_file->revision = BSWAP16_INLINE(*(uint16_t*)(&ctl_file_contents[input_pos]));
    input_pos += 2;

    if (bank_file->revision != BANKFILE_MAGIC_BYTES)
    {
        stderr_exit(EXIT_CODE_GENERAL, "Error reading ctl file, revision number does not match. Expected 0x%04x, read 0x%04x.\n", BANKFILE_MAGIC_BYTES, bank_file->revision);
    }

    bank_file->bank_count = BSWAP16_INLINE(*(uint16_t*)(&ctl_file_contents[input_pos]));
    input_pos += 2;

    if (bank_file->bank_count < 1)
    {
        stderr_exit(EXIT_CODE_GENERAL, "ctl count=%d, nothing to do.\n", bank_file->bank_count);
    }

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("bank file %d has %d entries\n", bank_file->id, bank_file->bank_count);
    }

    // hmmmm, malloc should use current system's pointer size.
    bank_file->bank_offsets = (int32_t *)malloc_zero(bank_file->bank_count, sizeof(void*));
    bank_file->banks = (struct ALBank **)malloc_zero(bank_file->bank_count, sizeof(void*));

    bswap32_memcpy(bank_file->bank_offsets, &ctl_file_contents[input_pos], bank_file->bank_count);
    input_pos += bank_file->bank_count * 4; /* 4 = sizeof offset */

    for (i=0; i<bank_file->bank_count; i++)
    {
        if (bank_file->bank_offsets[i] > 0)
        {
            bank_file->banks[i] = ALBank_new_from_ctl(ctl_file_contents, bank_file->bank_offsets[i]);
        }
    }

    TRACE_LEAVE(__func__)

    return bank_file;
}

/**
 * This is the main entry point for writing an .inst file.
 * Writes {@code struct ALEnvelope} to .inst file, using current file seek position.
 * Writes any child information as well.
 * @param bank_file: object to write.
 * @param inst_filename: path to write to.
*/
void write_inst(struct ALBankFile *bank_file, char* inst_filename)
{
    TRACE_ENTER(__func__)

    struct file_info *output;
    int i;

    output = file_info_fopen(inst_filename, "w");

    for (i=0; i<bank_file->bank_count; i++)
    {
        ALBank_write_to_fp(bank_file->banks[i], output);
    }

    file_info_free(output);

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

    free(envelope);

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

    free(keymap);

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

    free(wavetable);

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

    free(sound);

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
        stderr_exit(EXIT_CODE_GENERAL, "%s: detune=%d out of range. Valid range: (-100) - 100\n", __func__, detune);
    }

    if (detune > 100)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s: detune=%d out of range. Valid range: (-100) - 100\n", __func__, detune);
    }

    if (keybase < 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s: keybase=%d out of range. Valid range: 0-127\n", __func__, keybase);
    }

    if (keybase > 127)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s: keybase=%d out of range. Valid range: 0-127\n", __func__, keybase);
    }

    if (hw_sample_rate < 1.0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s: invalid hw_sample_rate=%f\n", __func__, hw_sample_rate);
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

    int bank_index;
    int instrument_index;
    int sound_index;

    if (bank_file == NULL)
    {
        TRACE_LEAVE(__func__)
        return NULL;
    }

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

    int bank_index;
    int instrument_index;
    int sound_index;

    if (bank_file == NULL)
    {
        TRACE_LEAVE(__func__)
        return NULL;
    }

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

    int bank_index;
    int instrument_index;
    int sound_index;

    if (bank_file == NULL)
    {
        TRACE_LEAVE(__func__)
        return NULL;
    }

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