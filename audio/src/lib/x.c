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
#include <math.h>
#include "debug.h"
#include "common.h"
#include "utility.h"
#include "naudio.h"
#include "adpcm_aifc.h"
#include "wav.h"
#include "int_hash.h"
#include "string_hash.h"
#include "x.h"

/**
 * This contains code that converts between audio formats.
*/

// forward declarations

static void ALBankFile_write_natural_order_envelope_ctl(struct ALBankFile *bank_file, uint8_t *buffer, size_t buffer_size, int *pos_ptr);
static void ALBankFile_write_natural_order_keymap_ctl(struct ALBankFile *bank_file, uint8_t *buffer, size_t buffer_size, int *pos_ptr);
static void ALBankFile_write_natural_order_sound_ctl(struct ALBankFile *bank_file, uint8_t *buffer, size_t buffer_size, int *pos_ptr);
static void ALBankFile_write_instrument_ctl(struct ALBankFile *bank_file, uint8_t *buffer, size_t buffer_size, int *pos_ptr);
static void ALBankFile_write_bank_ctl(struct ALBankFile *bank_file, uint8_t *buffer, size_t buffer_size, int *pos_ptr);

static void ALBankFile_populate_wavetables_from_aifc(struct ALBankFile *bank_file);
static void ALWaveTable_populate_from_aifc(struct ALWaveTable *wavetable, struct AdpcmAifcFile *aifc_file);

static void ALBankFile_write_meta_order_envelope_ctl(struct ALBankFile *bank_file, uint8_t *buffer, size_t buffer_size, int *pos_ptr);
static void ALBankFile_write_meta_order_keymap_ctl(struct ALBankFile *bank_file, uint8_t *buffer, size_t buffer_size, int *pos_ptr);
static void ALBankFile_write_meta_order_sound_ctl(struct ALBankFile *bank_file, uint8_t *buffer, size_t buffer_size, int *pos_ptr);

static int LinkedListNode_sound_keymap_write_order_compare_smaller(struct LinkedListNode *first, struct LinkedListNode *second);
static int LinkedListNode_sound_envelope_write_order_compare_smaller(struct LinkedListNode *first, struct LinkedListNode *second);
static int LinkedListNode_sound_write_order_compare_smaller(struct LinkedListNode *first, struct LinkedListNode *second);

// end forward declarations

/**
 * Allocates a new {@code struct AdpcmAifcFile} and initializes default values.
 * No sound data is loaded/converted, the parameters are simply to know what
 * needs to be initialized.
 * @param sound: reference {@code struct ALSound} that will be loaded
 * @param bank: reference {@code struct ALBank} that is the parent of {@code sound}
 * @returns pointer to new {@code struct AdpcmAifcFile}
*/
struct AdpcmAifcFile *AdpcmAifcFile_new_full(struct ALSound *sound, struct ALBank *bank)
{
    TRACE_ENTER(__func__)
    
    if (sound == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> sound is NULL\n", __func__, __LINE__);
    }
    
    if (bank == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> bank is NULL\n", __func__, __LINE__);
    }

    int expected_chunk_count = 2; // COMM, SSND.
    int alloc_chunk_count = 0;
    int need_loop = 0;

    uint32_t compression_type = 0;

    if (sound->wavetable != NULL)
    {
        if (sound->wavetable->type == AL_ADPCM_WAVE)
        {
            compression_type = ADPCM_AIFC_VAPC_COMPRESSION_TYPE_ID;

            if (sound->wavetable->wave_info.adpcm_wave.book != NULL)
            {
                expected_chunk_count++;
            }

            if (sound->wavetable->wave_info.adpcm_wave.loop != NULL)
            {
                need_loop = 1;
                expected_chunk_count++;
            }
        }
        else if (sound->wavetable->type == AL_RAW16_WAVE)
        {
            compression_type = ADPCM_AIFC_NONE_COMPRESSION_TYPE_ID;

            if (sound->wavetable->wave_info.raw_wave.loop != NULL)
            {
                need_loop = 1;
                expected_chunk_count++;
            }
        }
    }

    struct AdpcmAifcFile *aaf = AdpcmAifcFile_new_simple(expected_chunk_count);

    aaf->chunks[alloc_chunk_count] = AdpcmAifcCommChunk_new(compression_type);
    aaf->comm_chunk = aaf->chunks[alloc_chunk_count];
    alloc_chunk_count++;

    if (sound->wavetable != NULL)
    {
        if (sound->wavetable->type == AL_ADPCM_WAVE)
        {
            if (sound->wavetable->wave_info.adpcm_wave.book != NULL)
            {
                struct ALADPCMBook *book = sound->wavetable->wave_info.adpcm_wave.book;
                aaf->chunks[alloc_chunk_count] = AdpcmAifcCodebookChunk_new(book->order, book->npredictors);
                aaf->codes_chunk = aaf->chunks[alloc_chunk_count];
                alloc_chunk_count++;
            }
            else
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> Cannot find ALADPCMBook to resolve codebook data, sound %s, bank %s\n", __func__, __LINE__, sound->text_id, bank->text_id);
            }
        }

        if (sound->wavetable->len > 0)
        {
            aaf->chunks[alloc_chunk_count] = AdpcmAifcSoundChunk_new(sound->wavetable->len);
            aaf->sound_chunk = aaf->chunks[alloc_chunk_count];
            alloc_chunk_count++;
        }
        else
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> wavetable->len is zero, sound %s, bank %s\n", __func__, __LINE__, sound->text_id, bank->text_id);
        }
    }
    else
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> wavetable is NULL, sound %s, bank %s\n", __func__, __LINE__, sound->text_id, bank->text_id);
    }

    if (need_loop)
    {
        aaf->chunks[alloc_chunk_count] = AdpcmAifcLoopChunk_new();
        aaf->loop_chunk = aaf->chunks[alloc_chunk_count];
        alloc_chunk_count++;
    }

    if (alloc_chunk_count != expected_chunk_count)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> Expected to allocate %d chunks, but only allocated %d. Sound %s, bank %s\n", __func__, __LINE__, expected_chunk_count, alloc_chunk_count, sound->text_id, bank->text_id);
    }

    TRACE_LEAVE(__func__)

    return aaf;
}

/**
 * Reads a {@code struct ALSound} and converts to .aifc format.
 * @param aaf: destination container
 * @param sound: object to convert
 * @param tbl_file_contents: .tbl file contents
 * @param bank: parent bank of {@code sound}
*/
void load_aifc_from_sound(struct AdpcmAifcFile *aaf, struct ALSound *sound, uint8_t *tbl_file_contents, struct ALBank *bank)
{
    TRACE_ENTER(__func__)
    
    if (aaf == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> aaf is NULL\n", __func__, __LINE__);
    }
    
    if (sound == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> sound is NULL\n", __func__, __LINE__);
    }
    
    if (tbl_file_contents == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> tbl_file_contents is NULL\n", __func__, __LINE__);
    }
    
    if (bank == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> bank is NULL\n", __func__, __LINE__);
    }

    aaf->ck_data_size = 0;

    if (aaf->comm_chunk == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> error loading aifc from sound, common chunk is NULL, sound %s, bank %s\n", __func__, __LINE__, sound->text_id, bank->text_id);
    }

    // COMM chunk
    aaf->comm_chunk->num_channels = DEFAULT_NUM_CHANNELS;
    aaf->comm_chunk->sample_size = DEFAULT_SAMPLE_SIZE;

    f80 float_rate = (f80)(bank->sample_rate);
    reverse_into(aaf->comm_chunk->sample_rate, (uint8_t *)&float_rate, 10);

    aaf->ck_data_size += aaf->comm_chunk->ck_data_size;

    if (sound->wavetable == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> wavetable is NULL, sound %s, bank %s\n", __func__, __LINE__, sound->text_id, bank->text_id);
    }

    if (sound->wavetable->wave_info.adpcm_wave.book != NULL)
    {
        if (aaf->codes_chunk == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> error loading aifc from sound, codebook chunk is NULL, sound %s, bank %s\n", __func__, __LINE__, sound->text_id, bank->text_id);
        }
        
        int code_len;
        struct ALADPCMBook *book = sound->wavetable->wave_info.adpcm_wave.book;

        // code book chunk
        aaf->codes_chunk->base.unknown = 0xb; // ??
        aaf->codes_chunk->version = 1; // ??

        aaf->codes_chunk->order = book->order;
        aaf->codes_chunk->nentries = book->npredictors;
        code_len = book->order * book->npredictors * 16;
        memcpy(aaf->codes_chunk->table_data, book->book, code_len);

        AdpcmAifcCodebookChunk_decode_aifc_codebook(aaf->codes_chunk);

        aaf->ck_data_size += aaf->codes_chunk->base.ck_data_size;
    }

    // sound chunk
    if (sound->wavetable->len > 0)
    {
        if (g_verbosity >= VERBOSE_DEBUG)
        {
            printf("copying tbl data from offset 0x%06x len 0x%06x\n", sound->wavetable->base, sound->wavetable->len);
            fflush(stdout);
        }

        if (aaf->sound_chunk == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> error loading aifc from sound, sound chunk is NULL, sound %s, bank %s\n", __func__, __LINE__, sound->text_id, bank->text_id);
        }
    
        memcpy(
            aaf->sound_chunk->sound_data,
            &tbl_file_contents[sound->wavetable->base],
            sound->wavetable->len);

        // from the programming manual:
        // "The numSampleFrames field should be set to the number of bytes represented by the compressed data, not the the number of bytes used."
        // Well, the programming manual is wrong.
        // vadpcm_dec counts by 16, in how much it reads 9 bytes at a time.
        aaf->comm_chunk->num_sample_frames = (sound->wavetable->len / 9) * 16;

        aaf->ck_data_size += aaf->sound_chunk->ck_data_size;
    }

    // loop chunk
    if (sound->wavetable->wave_info.adpcm_wave.loop != NULL
        || sound->wavetable->wave_info.raw_wave.loop != NULL)
    {
        if (aaf->loop_chunk == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> error loading aifc from sound, loop chunk is NULL, sound %s, bank %s\n", __func__, __LINE__, sound->text_id, bank->text_id);
        }

        if (sound->wavetable->type == AL_ADPCM_WAVE)
        {
            struct ALADPCMLoop *loop = sound->wavetable->wave_info.adpcm_wave.loop;

            aaf->loop_chunk->nloops = 1;
            aaf->loop_chunk->loop_data->start = loop->start;
            aaf->loop_chunk->loop_data->end = loop->end;
            aaf->loop_chunk->loop_data->count = loop->count;

            memcpy(
                aaf->loop_chunk->loop_data->state,
                loop->state,
                ADPCM_AIFC_LOOP_STATE_LEN);
        }
        else if (sound->wavetable->type == AL_RAW16_WAVE)
        {
            struct ALRawLoop *loop = sound->wavetable->wave_info.raw_wave.loop;

            aaf->loop_chunk->nloops = 1;
            aaf->loop_chunk->loop_data->start = loop->start;
            aaf->loop_chunk->loop_data->end = loop->end;
            aaf->loop_chunk->loop_data->count = loop->count;
        }
        else
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> Unsupported loop type: %d. Sound %s, bank %s\n", __func__, __LINE__, sound->wavetable->type, sound->text_id, bank->text_id);
        }

        aaf->ck_data_size += aaf->loop_chunk->base.ck_data_size;
    }

    TRACE_LEAVE(__func__)
}

/**
 * Converts {@code struct ALSound} wavetable sound data to .aifc file.
 * @param sound: sound object holding wavetable data.
 * @param bank: sound object parent bank
 * @param tbl_file_contents: .tbl file contents
 * @param fi: FileInfo to write to. Uses current seek position.
*/
void write_sound_to_aifc(struct ALSound *sound, struct ALBank *bank, uint8_t *tbl_file_contents, struct FileInfo *fi)
{
    TRACE_ENTER(__func__)
    
    if (sound == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> sound is NULL\n", __func__, __LINE__);
    }
    
    if (bank == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> bank is NULL\n", __func__, __LINE__);
    }
    
    if (tbl_file_contents == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> tbl_file_contents is NULL\n", __func__, __LINE__);
    }
    
    if (fi == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> fi is NULL\n", __func__, __LINE__);
    }

    struct AdpcmAifcFile *aaf = AdpcmAifcFile_new_full(sound, bank);

    load_aifc_from_sound(aaf, sound, tbl_file_contents, bank);

    AdpcmAifcFile_fwrite(aaf, fi);
    AdpcmAifcFile_free(aaf);

    TRACE_LEAVE(__func__)
}

/**
 * This is the main entry point for converting {@code struct ALBankFile} to .aifc format.
 * Converts bank file and .tbl information, writing all wavetable sound data to .aifc files.
 * @param bank_file: bank file.
 * @param tbl_file_contents: .tbl file contents
*/
void write_bank_to_aifc(struct ALBankFile *bank_file, uint8_t *tbl_file_contents)
{
    TRACE_ENTER(__func__)
    
    if (bank_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> bank_file is NULL\n", __func__, __LINE__);
    }
    
    if (tbl_file_contents == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> tbl_file_contents is NULL\n", __func__, __LINE__);
    }

    struct FileInfo *output;
    int i,j,k;

    ALBankFile_clear_visited_flags(bank_file);

    for (i=0; i<bank_file->bank_count; i++)
    {
        struct ALBank *bank = bank_file->banks[i];
        for (j=0; j<bank->inst_count; j++)
        {
            struct ALInstrument *inst = bank->instruments[j];
            for (k=0; k<inst->sound_count; k++)
            {
                struct ALSound *sound = inst->sounds[k];

                if (sound == NULL)
                {
                    stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> sound is NULL\n", __func__, __LINE__);
                }

                if (sound->wavetable == NULL)
                {
                    stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> sound->wavetable is NULL\n", __func__, __LINE__);
                }

                if (sound->wavetable->visited == 0)
                {
                    sound->wavetable->visited = 1;

                    if (g_verbosity >= VERBOSE_DEBUG)
                    {
                        printf("opening sound file for output aifc: \"%s\"\n", sound->wavetable->aifc_path);
                    }

                    output = FileInfo_fopen(sound->wavetable->aifc_path, "w");

                    write_sound_to_aifc(sound, bank, tbl_file_contents, output);

                    FileInfo_free(output);
                }
            }
        }
    }

    TRACE_LEAVE(__func__)
}

/**
 * Helper method.
 * If there is loop information in the .wav file then an application loop chunk
 * is appended to the .aifc file. This only partially sets the loop data in the .aifc file
 * because decode state information is not available here.
 * @param aaf: aifc file to append chunk to.
 * @param wav: wav file to get loop from.
*/
void AdpcmAifcFile_add_partial_loop_from_wav(struct AdpcmAifcFile *aaf, struct WavFile *wav)
{
    TRACE_ENTER(__func__)

    if (wav == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: wav is NULL\n", __func__, __LINE__);
    }

    if (aaf == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: aaf is NULL\n", __func__, __LINE__);
    }

    if (wav->smpl_chunk == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (wav->smpl_chunk->num_sample_loops == 0)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    struct WavSampleChunk *smpl_chunk = wav->smpl_chunk;

    // Should only be one loop.
    // Checked for zero loops above.
    if (smpl_chunk->num_sample_loops != 1)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d>: only 1 loop is supported. smpl_chunk->num_sample_loops=%d\n", __func__, __LINE__, smpl_chunk->num_sample_loops);
    }

    struct WavSampleLoop *wav_loop = smpl_chunk->loops[0];

    if (wav_loop == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: wav_loop is NULL\n", __func__, __LINE__);
    }

    // ok, made it this far, try to add this to the aifc file.
    struct AdpcmAifcLoopChunk *aifc_loop = AdpcmAifcLoopChunk_new(); // sets nloops to 1
    aifc_loop->loop_data->start = wav_loop->start;
    aifc_loop->loop_data->end = wav_loop->end;

    // .aifc infinite loop is 0xffffffff but wav infinite loop is zero.
    // Other values map the same.
    if ((uint32_t)wav_loop->play_count == 0)
    {
        aifc_loop->loop_data->count = (uint32_t)0xffffffff;
    }
    else
    {
        aifc_loop->loop_data->count = wav_loop->play_count;
    }

    // done with conversion. Add to aifc.
    AdpcmAifcFile_append_chunk(aaf, aifc_loop);
    aaf->loop_chunk = aifc_loop;

    TRACE_LEAVE(__func__)
}

/**
 * Helper method.
 * If there is loop information in the .aifc file then a "smpl"
 * chunk is appended to the wav file.
 * @param wav: wav file to append chunk to.
 * @param aaf: aifc file to get loop information from.
*/
void WavFile_check_append_aifc_loop(struct WavFile *wav, struct AdpcmAifcFile *aaf)
{
    TRACE_ENTER(__func__)

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("enter append \"smpl\" to wav\n");
    }

    if (wav == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: wav is NULL\n", __func__, __LINE__);
    }

    if (aaf == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: aaf is NULL\n", __func__, __LINE__);
    }

    if (aaf->loop_chunk == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (aaf->loop_chunk->nloops == 0)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    struct AdpcmAifcLoopChunk *aifc_loop_chunk = aaf->loop_chunk;

    // Should only be one loop.
    // Checked for zero loops above.
    if (aifc_loop_chunk->nloops != 1)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d>: only 1 loop is supported. aaf->loop_chunk->nloops=%d\n", __func__, __LINE__, aaf->loop_chunk->nloops);
    }

    struct AdpcmAifcLoopData *aifc_loop_data = aifc_loop_chunk->loop_data;

    if (aifc_loop_data == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: aifc_loop_data is NULL\n", __func__, __LINE__);
    }

    // ok, made it this far, try to add this to the wav file.
    struct WavSampleChunk *wav_sample_chunk = WavSampleChunk_new(1);
    struct WavSampleLoop *wav_loop = wav_sample_chunk->loops[0];

    // wav loop data is in samples, .aifc loop data is in samples
    wav_loop->start = aifc_loop_data->start;
    wav_loop->end = aifc_loop_data->end;

    // this will already be set to correct value (zero), but making it explicit
    wav_loop->loop_type = SAMPLE_LOOP_FORWARD;

    // .aifc infinite loop is 0xffffffff but wav infinite loop is zero.
    // Other values map the same.
    if ((uint32_t)aifc_loop_data->count == (uint32_t)0xffffffff)
    {
        wav_loop->play_count = 0;
    }
    else
    {
        wav_loop->play_count = aifc_loop_data->count;
    }

    // done with conversion. Add to wav.
    WavFile_append_smpl_chunk(wav, wav_sample_chunk);

    TRACE_LEAVE(__func__)
}

/**
 * Translates from .aifc to .wav.
 * The .aifc must be loaded into memory.
 * Allocates memory for resulting wav file, including uncompressed audio.
 * The exact .aifc uncompressed audio size is not known, so slightly more space
 * is allocated than is needed.
 * Loops ("smpl" chunk) are not copied in this method.
 * @param aifc_file: aifc file to convert to wav
 * @returns: pointer to new wav file.
*/
struct WavFile *WavFile_new_from_aifc(struct AdpcmAifcFile *aifc_file)
{
    TRACE_ENTER(__func__)

    if (aifc_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: aifc_file is NULL\n", __func__, __LINE__);
    }

    struct WavFile *wav = WavFile_new(WAV_DEFAULT_NUM_CHUNKS);

    // "fmt " chunk
    wav->fmt_chunk = WavFmtChunk_new();
    wav->chunks[0] = wav->fmt_chunk;

    wav->fmt_chunk->audio_format = WAV_AUDIO_FORMAT;
    wav->fmt_chunk->num_channels = 1;
    wav->fmt_chunk->sample_rate = AdpcmAifcFile_get_int_sample_rate(aifc_file);
    wav->fmt_chunk->bits_per_sample = aifc_file->comm_chunk->sample_size;
    wav->fmt_chunk->byte_rate = wav->fmt_chunk->sample_rate * wav->fmt_chunk->num_channels * wav->fmt_chunk->bits_per_sample/8;
    wav->fmt_chunk->block_align = wav->fmt_chunk->num_channels * wav->fmt_chunk->bits_per_sample/8;

    // "data" chunk
    wav->data_chunk = WavDataChunk_new();
    wav->chunks[1] = wav->data_chunk;

    size_t buffer_len = AdpcmAifcFile_estimate_inflate_size(aifc_file);
    wav->data_chunk->data = (uint8_t *)malloc_zero(1, buffer_len);

    wav->data_chunk->ck_data_size = AdpcmAifcFile_decode(aifc_file, wav->data_chunk->data, buffer_len);

    wav->ck_data_size = 
        4 + /* rest of FORM header */
        WAV_FMT_CHUNK_FULL_SIZE + /* "fmt " chunk is const size */
        8 + wav->data_chunk->ck_data_size; /* "data" chunk header, then data size*/

    // "smpl" chunk is optional, this is handled elsewhere.

    TRACE_LEAVE(__func__)

    return wav;
}

/**
 * Converts .wav to .aifc.
 * This is the main entry point for .wav to .aifc conversion.
 * @param wav_file: wav file and sound data to convert.
 * @param book: Optional. Codebook data. If present, sound data will be encoded
 * using "VAPC" compression, otherwise uncompressed.
 * @returns: pointer to new .aifc file.
*/
struct AdpcmAifcFile *AdpcmAifcFile_new_from_wav(struct WavFile *wav_file, struct ALADPCMBook *book)
{
    TRACE_ENTER(__func__)

    if (wav_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: wav_file is NULL\n", __func__, __LINE__);
    }

    if (wav_file->fmt_chunk == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: wav_file->fmt_chunk is NULL\n", __func__, __LINE__);
    }

    if (wav_file->data_chunk == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: wav_file->data_chunk is NULL\n", __func__, __LINE__);
    }

    if (wav_file->data_chunk->ck_data_size < 8)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d>: wav_file->data_chunk->ck_data_size too short: %d\n", __func__, __LINE__, wav_file->data_chunk->ck_data_size);
    }

    struct AdpcmAifcFile *aaf;
    struct AdpcmAifcCommChunk *comm;
    uint32_t compression_type;
    f80 extended_float_sample_rate;
    size_t aifc_root_ck_data_size = 0;
    size_t sound_data_len = 0;

    sound_data_len = wav_file->data_chunk->ck_data_size - 8;

    aaf = AdpcmAifcFile_new_simple(0);
    aifc_root_ck_data_size = 4; // ck_data_size for empty .aifc file

    if (book == NULL)
    {
        compression_type = ADPCM_AIFC_NONE_COMPRESSION_TYPE_ID;
    }
    else
    {
        compression_type = ADPCM_AIFC_VAPC_COMPRESSION_TYPE_ID;
    }

    comm = AdpcmAifcCommChunk_new(compression_type);
    comm->num_channels = wav_file->fmt_chunk->num_channels;
    extended_float_sample_rate = (f80)wav_file->fmt_chunk->sample_rate;

    reverse_into(comm->sample_rate, (uint8_t *)&extended_float_sample_rate, 10);

    comm->sample_size = wav_file->fmt_chunk->bits_per_sample;

    aaf->comm_chunk = comm;
    AdpcmAifcFile_append_chunk(aaf, comm);
    
    // chunk ck_data_size doesn't count bytes for id + ck_data_size so add 8.
    aifc_root_ck_data_size += 8 + comm->ck_data_size;

    if (book != NULL)
    {
        AdpcmAifcFile_add_codebook_from_ALADPCMBook(aaf, book);
        
        // chunk ck_data_size doesn't count bytes for id + ck_data_size so add 8.
        aifc_root_ck_data_size += 8 + aaf->codes_chunk->base.ck_data_size;

        // verify the wav audio is in a format that can be encoded
        if (wav_file->fmt_chunk->bits_per_sample != AIFC_ENCODE_BIT_SAMPLE_SUPPORT)
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> can only encode audio in %d bits per sample, but input audio is in %d\n", __func__, __LINE__, AIFC_ENCODE_BIT_SAMPLE_SUPPORT, wav_file->fmt_chunk->bits_per_sample);
        }

        if (wav_file->fmt_chunk->num_channels > AIFC_ENCODE_MAX_CHANNEL_SUPPORT)
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> can only encode audio with up to %d channel(s), but input audio has %d\n", __func__, __LINE__, AIFC_ENCODE_MAX_CHANNEL_SUPPORT, wav_file->fmt_chunk->num_channels);
        }
    }

    // Adding "smpl" loop chunk to wav is optional, but setting loop data on .aifc
    // should be required. This adds the outline of required data to the .aifc file,
    // but the decoder state information won't be available until processing the audio.
    // This needs to be added before encoding audio.
    if (wav_file->smpl_chunk != NULL)
    {
        AdpcmAifcFile_add_partial_loop_from_wav(aaf, wav_file);
        
        // chunk ck_data_size doesn't count bytes for id + ck_data_size so add 8.
        aifc_root_ck_data_size += 8 + aaf->loop_chunk->base.ck_data_size;
    }

    // Load sound data. When codebook was created the incoming format was validated,
    // so it should be safe to copy straight from wav sound data without any processing here.
    AdpcmAifcFile_encode(aaf, wav_file->data_chunk->data, sound_data_len);
    
    // chunk ck_data_size doesn't count bytes for id + ck_data_size so add 8.
    aifc_root_ck_data_size += 8 + aaf->sound_chunk->ck_data_size;

    // set total file size
    aaf->ck_data_size = (int32_t)aifc_root_ck_data_size;

    TRACE_LEAVE(__func__)

    return aaf;
}

/**
 * This is the main entry point for writing a .tbl file.
 * This traverses the bank_file looking for wavetable objects. Foreach
 * distinct wavetable, the referenced .aifc is loaded and the sound
 * data is extracted and written to the .tbl file. The wavetable
 * {@code base} offsets are updated to reflect the .tbl offset.
 * @param bank_file: object to write.
 * @param tbl_filename: path to write to.
*/
void ALBankFile_write_tbl(struct ALBankFile *bank_file, char* tbl_filename)
{
    TRACE_ENTER(__func__)

    if (bank_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: bank_file is NULL\n", __func__, __LINE__);
    }

    if (tbl_filename == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: tbl_filename is NULL\n", __func__, __LINE__);
    }

    /**
     * Need to honor the sort method.
     * Iterate the bank file and collect all wavetable objects.
     * Apply sort method if needed.
     * Then iterate the wavetable objects and write .aifc sound data to .tbl.
    */

    struct FileInfo *output;
    int bank_count;
    struct LinkedList *list_sounds = LinkedList_new();
    struct LinkedListNode *node;
    struct StringHashTable *seen = StringHashTable_new();

    // first step, collect everything
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

                    for (sound_count=0; sound_count<instrument->sound_count; sound_count++)
                    {
                        struct ALSound *sound = instrument->sounds[sound_count];

                        if (sound != NULL)
                        {
                            struct ALWaveTable *wavetable = sound->wavetable;

                            if (wavetable != NULL && wavetable->aifc_path != NULL && wavetable->aifc_path[0] != '\0')
                            {
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
    }

    // apply sort method if needed.
    if (bank_file->ctl_sort_method == CTL_SORT_METHOD_NATURAL)
    {
        // nothing to do, use the order iterated above.
    }
    else if (bank_file->ctl_sort_method == CTL_SORT_METHOD_META)
    {
        LinkedList_merge_sort(list_sounds, LinkedListNode_sound_write_order_compare_smaller);
    }
    else
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> bank_file->ctl_sort_method not supported: %d\n", __func__, __LINE__, bank_file->ctl_sort_method);
    }

    ALBankFile_clear_visited_flags(bank_file);

    // now write output and set `base` offset.

    output = FileInfo_fopen(tbl_filename, "w");

    node = list_sounds->head;
    while (node != NULL)
    {
        struct ALSound *sound = (struct ALSound *)node->data;

        if (sound->visited == 0)
        {
            struct ALWaveTable *wavetable = sound->wavetable;
            
            sound->visited = 1;

            if (!StringHashTable_contains(seen, wavetable->aifc_path))
            {
                int32_t wavetable_base = (int32_t)FileInfo_ftell(output);
                size_t sound_data_size;

                AdpcmAifcFile_path_write_tbl(wavetable->aifc_path, output, &sound_data_size);

                wavetable->base = wavetable_base;
                wavetable->len = (int)sound_data_size;

                StringHashTable_add(seen, wavetable->aifc_path, wavetable);
            }
            else
            {
                struct ALWaveTable *ht_wavetable = StringHashTable_get(seen, wavetable->aifc_path);

                wavetable->base = ht_wavetable->base;
                wavetable->len = ht_wavetable->len;
            }
        }

        node = node->next;
    }

    // done, cleanup.

    LinkedList_free(list_sounds);

    FileInfo_free(output);

    StringHashTable_free(seen);

    TRACE_LEAVE(__func__)
}

/**
 * This is the main entry point for writing a .ctl file.
 * This traverses the bank_file and writes audio information for all related
 * child structures. The .tbl offsets for the wavetable objects
 * must have previously been set (call {@code ALBankFile_write_tbl} first).
 * @param bank_file: object to write.
 * @param ctl_filename: path to write to.
*/
void ALBankFile_write_ctl(struct ALBankFile *bank_file, char* ctl_filename)
{
    TRACE_ENTER(__func__)

    if (bank_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: bank_file is NULL\n", __func__, __LINE__);
    }

    if (ctl_filename == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: ctl_filename is NULL\n", __func__, __LINE__);
    }

    /**
     * Generally the `offset` doesn't need to be written until after
     * is has been defined, with the exception of the bank offsets
     * at the start of the .ctl. The approach here is to write
     * everything into a buffer in memory then go back and fix
     * up any unknown offsets.
    */

    struct FileInfo *output;
    
    // temp buffer to store output in. Once entire bank_file is processed
    // this will be written to disk.
    uint8_t *buffer;
    size_t buffer_size;
    int pos;
    int file_size;
    uint32_t t32; // temp
    uint16_t t16; // temp
    int bank_count;

    // iterate all the wavetables (again ...) and load the loop
    // and book information.
    ALBankFile_populate_wavetables_from_aifc(bank_file);

    buffer_size = ALBankFile_estimate_ctl_filesize(bank_file);
    buffer = (uint8_t *)malloc_zero(1, buffer_size);

    // start writing data to buffer.
    pos = 0;

    t16 = BSWAP16_INLINE(BANKFILE_MAGIC_BYTES);
    memcpy(&buffer[pos], &t16, 2);
    pos += 2;

    t16 = BSWAP16_INLINE(bank_file->bank_count);
    memcpy(&buffer[pos], &t16, 2);
    pos += 2;

    // Bank offsets are not known at this time. Skip ahead (fill with zero for now).
    pos += 4 * bank_file->bank_count;

    if (bank_file->ctl_sort_method == CTL_SORT_METHOD_NATURAL)
    {
        ALBankFile_write_natural_order_envelope_ctl(bank_file, buffer, buffer_size, &pos);
        ALBankFile_write_natural_order_keymap_ctl(bank_file, buffer, buffer_size, &pos);
        ALBankFile_write_natural_order_sound_ctl(bank_file, buffer, buffer_size, &pos);
    }
    else if (bank_file->ctl_sort_method == CTL_SORT_METHOD_META)
    {
        ALBankFile_write_meta_order_envelope_ctl(bank_file, buffer, buffer_size, &pos);
        ALBankFile_write_meta_order_keymap_ctl(bank_file, buffer, buffer_size, &pos);
        ALBankFile_write_meta_order_sound_ctl(bank_file, buffer, buffer_size, &pos);
    }
    else
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> bank_file->ctl_sort_method not supported: %d\n", __func__, __LINE__, bank_file->ctl_sort_method);
    }

    ALBankFile_write_instrument_ctl(bank_file, buffer, buffer_size, &pos);
    ALBankFile_write_bank_ctl(bank_file, buffer, buffer_size, &pos);

    file_size = pos;

    // reset to beginning of file to write bank offsets.
    pos = 4;
    for (bank_count=0; bank_count<bank_file->bank_count; bank_count++)
    {
        t32 = BSWAP32_INLINE(bank_file->bank_offsets[bank_count]);
        memcpy(&buffer[pos], &t32, 4);
        pos += 4;
    }

    // done writing bank_file to buffer.
    output = FileInfo_fopen(ctl_filename, "w");
    FileInfo_fwrite(output, buffer, file_size, 1);
    FileInfo_free(output);

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("wrote %d bytes to .ctl\n", file_size);
    }

    free(buffer);

    TRACE_LEAVE(__func__)
}

/**
 * Instantiates a new {@code struct ALADPCMLoop} and sets properties
 * from {@code struct AdpcmAifcLoopChunk}.
 * @param loop_chunk: source loop.
 * @returns pointer to new memory.
*/
struct ALADPCMLoop *ALADPCMLoop_new_from_aifc_loop(struct AdpcmAifcLoopChunk *loop_chunk)
{
    TRACE_ENTER(__func__)

    if (loop_chunk == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> loop_chunk is NULL\n", __func__, __LINE__);
    }

    struct ALADPCMLoop *loop = (struct ALADPCMLoop *)malloc_zero(1, sizeof(struct ALADPCMLoop));

    if (loop_chunk->nloops > 1)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> loop_chunk->nloops should be 1, actual: %d\n", __func__, __LINE__, loop_chunk->nloops);
    }
    else if (loop_chunk->nloops == 1)
    {
        struct AdpcmAifcLoopData *loop_data = loop_chunk->loop_data;

        if (loop_data == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> loop_data is NULL\n", __func__, __LINE__);
        }

        loop->count = loop_data->count;
        loop->start = loop_data->start;
        loop->end = loop_data->end;

        memcpy(loop->state, loop_data->state, ADPCM_AIFC_LOOP_STATE_LEN);
    }

    TRACE_LEAVE(__func__)

    return loop;
}

/**
 * Instantiates a new {@code struct ALADPCMBook} and sets properties
 * from {@code struct AdpcmAifcCodebookChunk}.
 * @param book_chunk: source book.
 * @returns pointer to new memory.
*/
struct ALADPCMBook *ALADPCMBook_new_from_aifc_book(struct AdpcmAifcCodebookChunk *book_chunk)
{
    TRACE_ENTER(__func__)

    if (book_chunk == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> book_chunk is NULL\n", __func__, __LINE__);
    }
    
    int book_bytes;

    struct ALADPCMBook *book = (struct ALADPCMBook *)malloc_zero(1, sizeof(struct ALADPCMBook));

    book->order = book_chunk->order;
    book->npredictors = book_chunk->nentries;

    // book, size in bytes = order * npredictors * 16
    book_bytes = book->order * book->npredictors * 16;

    if (book_bytes > 0)
    {
        book->book = (int16_t *)malloc_zero(1, book_bytes);
        // raw byte copy, no bswap
        memcpy(book->book, book_chunk->table_data, book_bytes);
    }

    TRACE_LEAVE(__func__)

    return book;
}

/**
 * Instantiates a new {@code struct ALRawLoop} and sets properties
 * from {@code struct AdpcmAifcLoopChunk}.
 * @param loop_chunk: source loop.
 * @returns pointer to new memory.
*/
struct ALRawLoop *ALRawLoop_new_from_aifc_loop(struct AdpcmAifcLoopChunk *loop_chunk)
{
    TRACE_ENTER(__func__)

    if (loop_chunk == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> loop_chunk is NULL\n", __func__, __LINE__);
    }

    struct ALRawLoop *loop = (struct ALRawLoop *)malloc_zero(1, sizeof(struct ALRawLoop));

    if (loop_chunk->nloops > 1)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> loop_chunk->nloops should be 1, actual: %d\n", __func__, __LINE__, loop_chunk->nloops);
    }
    else if (loop_chunk->nloops == 1)
    {
        struct AdpcmAifcLoopData *loop_data = loop_chunk->loop_data;

        if (loop_data == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> loop_data is NULL\n", __func__, __LINE__);
        }

        loop->count = loop_data->count;
        loop->start = loop_data->start;
        loop->end = loop_data->end;
    }

    TRACE_LEAVE(__func__)

    return loop;
}

/**
 * Helper method, writes the sound into the buffer at the position.
 * @param sound: sound to write.
 * @param buffer: buffer to write to.
 * @param buffer_size: max size in bytes of the buffer.
 * @param pos_ptr: in/out parameter. Start position of buffer to write
 * to, will contain offset of next position to write to.
*/
static void ALSound_write_ctl(struct ALSound *sound, uint8_t *buffer, size_t buffer_size, int *pos_ptr)
{
    TRACE_ENTER(__func__)

    if (sound == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: sound is NULL\n", __func__, __LINE__);
    }

    if (buffer == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: buffer is NULL\n", __func__, __LINE__);
    }

    if (pos_ptr == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: pos_ptr is NULL\n", __func__, __LINE__);
    }

    uint32_t t32; // temp
    int pos = *pos_ptr;

    // write sound properties first.
    struct ALWaveTable *wavetable = sound->wavetable;

    if ((size_t)(pos + 16) > buffer_size)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> exceed buffer length at position %ld writing sound \"%s\"\n", __func__, __LINE__, pos, sound->text_id);
    }

    if (sound->envelope != NULL)
    {
        t32 = BSWAP32_INLINE(sound->envelope->self_offset);
    }
    else
    {
        t32 = 0;
    }
    memcpy(&buffer[pos], &t32, 4);
    pos += 4;

    if (sound->keymap != NULL)
    {
        t32 = BSWAP32_INLINE(sound->keymap->self_offset);
    }
    else
    {
        t32 = 0;
    }
    memcpy(&buffer[pos], &t32, 4);
    pos += 4;

    if (wavetable != NULL)
    {
        if (wavetable->visited == 0)
        {
            // time travel to the future to get the wavetable offset
            sound->wavetable_offset = pos + 8;

            // set offset available to all parents.
            sound->wavetable->self_offset = pos + 8;
        }

        t32 = BSWAP32_INLINE(sound->wavetable->self_offset);
    }
    else
    {
        t32 = 0;
    }
    memcpy(&buffer[pos], &t32, 4);
    pos += 4;

    buffer[pos] = sound->sample_pan;
    pos++;

    buffer[pos] = sound->sample_volume;
    pos++;

    buffer[pos] = sound->flags;
    pos++;

    // 1 byte padding
    pos++;

    // Done with top level sound properties.
    // Now write wavetable properties.

    if (wavetable != NULL && wavetable->visited == 0)
    {
        int wavetable_size_guess = 24;

        // offset to adjust for whether or not there's loop info.
        int book_delta = 0;

        wavetable->visited = 1;

        // A little bit annoying, but calculate whether this will overflow the buffer,
        // then do the same calculations again to write the data...
        if (wavetable->type == AL_RAW16_WAVE)
        {
            if (wavetable->wave_info.raw_wave.loop != NULL)
            {
                wavetable_size_guess += 12 + 4 + ADPCM_STATE_SIZE;

                book_delta = 16 + ADPCM_STATE_SIZE; // this isn't possible ...
            }
        }
        else if (wavetable->type == AL_ADPCM_WAVE)
        {
            if (wavetable->wave_info.adpcm_wave.loop != NULL)
            {
                wavetable_size_guess += 12 + 32 + 4;

                book_delta = 48;
            }

            if (wavetable->wave_info.adpcm_wave.book != NULL)
            {
                wavetable_size_guess +=
                    16 * wavetable->wave_info.adpcm_wave.book->order * wavetable->wave_info.adpcm_wave.book->npredictors;

                wavetable_size_guess += 8;
            }
        }

        // overflow check
        if ((size_t)(pos + wavetable_size_guess) > buffer_size)
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> exceed buffer length at position %ld writing wavetable \"%s\"\n", __func__, __LINE__, pos, wavetable->text_id);
        }

        // set wavetable .ctl file offset (not needed now)
        wavetable->self_offset = pos;

        t32 = BSWAP32_INLINE(wavetable->base);
        memcpy(&buffer[pos], &t32, 4);
        pos += 4;

        t32 = BSWAP32_INLINE(wavetable->len);
        memcpy(&buffer[pos], &t32, 4);
        pos += 4;

        buffer[pos] = wavetable->type;
        pos++;

        buffer[pos] = wavetable->flags;
        pos++;

        // 2 bytes of padding
        pos += 2;

        // time travel to the future to get the loop offset
        if ((wavetable->type == AL_RAW16_WAVE && wavetable->wave_info.raw_wave.loop != NULL)
            || (wavetable->type == AL_ADPCM_WAVE && wavetable->wave_info.adpcm_wave.loop != NULL))
        {
            t32 = BSWAP32_INLINE(pos + 12);
            memcpy(&buffer[pos], &t32, 4);
        }
        pos += 4;

        // time travel to the future to get the book offset
        if (wavetable->type == AL_ADPCM_WAVE && wavetable->wave_info.adpcm_wave.book != NULL)
        {
            t32 = BSWAP32_INLINE(pos + 8 + book_delta);
            memcpy(&buffer[pos], &t32, 4);
        }
        pos += 4;

        // padding
        pos += 4;

        // Done with wavetable info.
        // Now write loop and book values.
        // overflow check for these was handled above.

        if (wavetable->type == AL_RAW16_WAVE)
        {
            if (wavetable->wave_info.raw_wave.loop != NULL)
            {
                struct ALRawLoop *loop = wavetable->wave_info.raw_wave.loop;

                t32 = BSWAP32_INLINE(loop->start);
                memcpy(&buffer[pos], &t32, 4);
                pos += 4;

                t32 = BSWAP32_INLINE(loop->end);
                memcpy(&buffer[pos], &t32, 4);
                pos += 4;

                t32 = BSWAP32_INLINE(loop->count);
                memcpy(&buffer[pos], &t32, 4);
                pos += 4;

                // raw loop uses space for the state but just writes zeros.
                pos += ADPCM_STATE_SIZE;

                // padding
                pos += 4;
            }
        }
        else if (wavetable->type == AL_ADPCM_WAVE)
        {
            if (wavetable->wave_info.adpcm_wave.loop != NULL)
            {
                struct ALADPCMLoop *loop = wavetable->wave_info.adpcm_wave.loop;

                t32 = BSWAP32_INLINE(loop->start);
                memcpy(&buffer[pos], &t32, 4);
                pos += 4;

                t32 = BSWAP32_INLINE(loop->end);
                memcpy(&buffer[pos], &t32, 4);
                pos += 4;

                t32 = BSWAP32_INLINE(loop->count);
                memcpy(&buffer[pos], &t32, 4);
                pos += 4;

                memcpy(&buffer[pos], loop->state, ADPCM_STATE_SIZE);
                pos += ADPCM_STATE_SIZE;

                // padding
                pos += 4;
            }

            if (wavetable->wave_info.adpcm_wave.book != NULL)
            {
                struct ALADPCMBook *book = wavetable->wave_info.adpcm_wave.book;
                int state_size = book->order * book->npredictors * 16;

                t32 = BSWAP32_INLINE(book->order);
                memcpy(&buffer[pos], &t32, 4);
                pos += 4;

                t32 = BSWAP32_INLINE(book->npredictors);
                memcpy(&buffer[pos], &t32, 4);
                pos += 4;

                memcpy(&buffer[pos], book->book, state_size);
                pos += state_size;

                // padding
                pos += 8;
            }
        }
    }

    *pos_ptr = pos;

    TRACE_LEAVE(__func__)
}

/**
 * Helper method, writes the envelope into the buffer at the position.
 * @param envelope: envelope to write.
 * @param buffer: buffer to write to.
 * @param buffer_size: max size in bytes of the buffer.
 * @param pos_ptr: in/out parameter. Start position of buffer to write
 * to, will contain offset of next position to write to.
*/
static void ALEnvelope_write_ctl(struct ALEnvelope *envelope, uint8_t *buffer, size_t buffer_size, int *pos_ptr)
{
    TRACE_ENTER(__func__)

    if (envelope == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: envelope is NULL\n", __func__, __LINE__);
    }

    if (buffer == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: buffer is NULL\n", __func__, __LINE__);
    }

    if (pos_ptr == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: pos_ptr is NULL\n", __func__, __LINE__);
    }

    uint32_t t32; // temp
    int pos = *pos_ptr;

    if ((size_t)(pos + 16) > buffer_size)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> exceed buffer length at position %ld writing envelope \"%s\"\n", __func__, __LINE__, pos, envelope->text_id);
    }

    t32 = BSWAP32_INLINE(envelope->attack_time);
    memcpy(&buffer[pos], &t32, 4);
    pos += 4;

    t32 = BSWAP32_INLINE(envelope->decay_time);
    memcpy(&buffer[pos], &t32, 4);
    pos += 4;

    t32 = BSWAP32_INLINE(envelope->release_time);
    memcpy(&buffer[pos], &t32, 4);
    pos += 4;

    buffer[pos] = envelope->attack_volume;
    pos++;

    buffer[pos] = envelope->decay_volume;
    pos++;

    // there are two bytes of padding
    pos += 2;

    *pos_ptr = pos;
    
    TRACE_LEAVE(__func__)
}

/**
 * Helper method, writes the keymap into the buffer at the position.
 * @param keymap: keymap to write.
 * @param buffer: buffer to write to.
 * @param buffer_size: max size in bytes of the buffer.
 * @param pos_ptr: in/out parameter. Start position of buffer to write
 * to, will contain offset of next position to write to.
*/
static void ALKeyMap_write_ctl(struct ALKeyMap *keymap, uint8_t *buffer, size_t buffer_size, int *pos_ptr)
{
    TRACE_ENTER(__func__)

    if (keymap == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: keymap is NULL\n", __func__, __LINE__);
    }

    if (buffer == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: buffer is NULL\n", __func__, __LINE__);
    }

    if (pos_ptr == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: pos_ptr is NULL\n", __func__, __LINE__);
    }

    int pos = *pos_ptr;

    if ((size_t)(pos + 8) > buffer_size)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> exceed buffer length at position %ld writing keymap \"%s\"\n", __func__, __LINE__, pos, keymap->text_id);
    }

    buffer[pos] = keymap->velocity_min;
    pos++;

    buffer[pos] = keymap->velocity_max;
    pos++;

    buffer[pos] = keymap->key_min;
    pos++;

    buffer[pos] = keymap->key_max;
    pos++;

    buffer[pos] = keymap->key_base;
    pos++;

    buffer[pos] = (uint8_t)keymap->detune;
    pos++;

    // there are two bytes of padding
    pos += 2;

    *pos_ptr = pos;
    
    TRACE_LEAVE(__func__)
}

/**
 * Iterates the bank file and writes all envelopes
 * into the buffer in the order encountered.
 * @param bank_file: bank file containing envelopes.
 * @param buffer: buffer to write to.
 * @param buffer_size: max size in bytes of the buffer.
 * @param pos_ptr: in/out parameter. Start position of buffer to write
 * to, will contain offset of next position to write to.
*/
static void ALBankFile_write_natural_order_envelope_ctl(struct ALBankFile *bank_file, uint8_t *buffer, size_t buffer_size, int *pos_ptr)
{
    TRACE_ENTER(__func__)

    if (bank_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: bank_file is NULL\n", __func__, __LINE__);
    }

    if (buffer == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: buffer is NULL\n", __func__, __LINE__);
    }

    if (pos_ptr == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: pos_ptr is NULL\n", __func__, __LINE__);
    }

    int bank_count;
    int pos = *pos_ptr;

    ALBankFile_clear_visited_flags(bank_file);

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf(".ctl begin writing envelopes position %d\n", pos);
    }

    for (bank_count=0; bank_count<bank_file->bank_count; bank_count++)
    {
        struct ALBank *bank = bank_file->banks[bank_count];

        if (bank != NULL)
        {
            int inst_count;

            for (inst_count=0; inst_count<bank->inst_count; inst_count++)
            {
                struct ALInstrument *instrument = bank->instruments[inst_count];

                if (instrument != NULL && instrument->visited == 0)
                {
                    int sound_count;

                    instrument->visited = 1;

                    for (sound_count=0; sound_count<instrument->sound_count; sound_count++)
                    {
                        struct ALSound *sound = instrument->sounds[sound_count];

                        if (sound != NULL && sound->visited == 0)
                        {
                            struct ALEnvelope *envelope = sound->envelope;

                            sound->visited = 1;

                            if (envelope != NULL && envelope->visited == 0)
                            {
                                envelope->visited = 1;

                                // set envelope parent .ctl file offset
                                sound->envelope_offset = pos;

                                // set offset available to all parents.
                                envelope->self_offset = pos;

                                ALEnvelope_write_ctl(envelope, buffer, buffer_size, &pos);
                            }
                        }
                    }
                }
            }
        }
    }

    *pos_ptr = pos;

    TRACE_LEAVE(__func__)
}

/**
 * Iterates the bank file and writes all keymaps
 * into the buffer in the order encountered.
 * @param bank_file: bank file containing keymaps.
 * @param buffer: buffer to write to.
 * @param buffer_size: max size in bytes of the buffer.
 * @param pos_ptr: in/out parameter. Start position of buffer to write
 * to, will contain offset of next position to write to.
*/
static void ALBankFile_write_natural_order_keymap_ctl(struct ALBankFile *bank_file, uint8_t *buffer, size_t buffer_size, int *pos_ptr)
{
    TRACE_ENTER(__func__)

    if (bank_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: bank_file is NULL\n", __func__, __LINE__);
    }

    if (buffer == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: buffer is NULL\n", __func__, __LINE__);
    }

    if (pos_ptr == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: pos_ptr is NULL\n", __func__, __LINE__);
    }

    int bank_count;
    int pos = *pos_ptr;

    ALBankFile_clear_visited_flags(bank_file);

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf(".ctl begin writing keymaps position %d\n", pos);
    }

    for (bank_count=0; bank_count<bank_file->bank_count; bank_count++)
    {
        struct ALBank *bank = bank_file->banks[bank_count];

        if (bank != NULL)
        {
            int inst_count;

            for (inst_count=0; inst_count<bank->inst_count; inst_count++)
            {
                struct ALInstrument *instrument = bank->instruments[inst_count];

                if (instrument != NULL && instrument->visited == 0)
                {
                    int sound_count;

                    instrument->visited = 1;

                    for (sound_count=0; sound_count<instrument->sound_count; sound_count++)
                    {
                        struct ALSound *sound = instrument->sounds[sound_count];

                        if (sound != NULL && sound->visited == 0)
                        {
                            struct ALKeyMap *keymap = sound->keymap;

                            sound->visited = 1;

                            if (keymap != NULL && keymap->visited == 0)
                            {
                                keymap->visited = 1;

                                // set keymap parent .ctl file offset
                                sound->keymap_offset = pos;

                                // set offset available to all parents.
                                keymap->self_offset = pos;

                                ALKeyMap_write_ctl(keymap, buffer, buffer_size, &pos);
                            }
                        }
                    }
                }
            }
        }
    }

    *pos_ptr = pos;

    TRACE_LEAVE(__func__)
}

/**
 * Merge sort comparison function.
 * Sorts on sound->keymap->ctl_write_order.
 * Use this to sort smallest to largest.
 * @param first: first node
 * @param second: second node
 * @returns: comparison result
*/
static int LinkedListNode_sound_keymap_write_order_compare_smaller(struct LinkedListNode *first, struct LinkedListNode *second)
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
            if (sound_first->keymap->ctl_write_order < sound_second->keymap->ctl_write_order)
            {
                ret = -1;
            }
            else if (sound_first->keymap->ctl_write_order > sound_second->keymap->ctl_write_order)
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
 * Sorts on sound->envelope->ctl_write_order.
 * Use this to sort smallest to largest.
 * @param first: first node
 * @param second: second node
 * @returns: comparison result
*/
static int LinkedListNode_sound_envelope_write_order_compare_smaller(struct LinkedListNode *first, struct LinkedListNode *second)
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
            if (sound_first->envelope->ctl_write_order < sound_second->envelope->ctl_write_order)
            {
                ret = -1;
            }
            else if (sound_first->envelope->ctl_write_order > sound_second->envelope->ctl_write_order)
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
 * Sorts on sound->ctl_write_order.
 * Use this to sort smallest to largest.
 * @param first: first node
 * @param second: second node
 * @returns: comparison result
*/
static int LinkedListNode_sound_write_order_compare_smaller(struct LinkedListNode *first, struct LinkedListNode *second)
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
            if (sound_first->ctl_write_order < sound_second->ctl_write_order)
            {
                ret = -1;
            }
            else if (sound_first->ctl_write_order > sound_second->ctl_write_order)
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
 * Collects all envelopes in the bank file, then sorts them according
 * to the ctl_write_order property. Then writes them to the buffer
 * in this order.
 * @param bank_file: bank file containing envelopes.
 * @param buffer: buffer to write to.
 * @param buffer_size: max size in bytes of the buffer.
 * @param pos_ptr: in/out parameter. Start position of buffer to write
 * to, will contain offset of next position to write to.
*/
static void ALBankFile_write_meta_order_envelope_ctl(struct ALBankFile *bank_file, uint8_t *buffer, size_t buffer_size, int *pos_ptr)
{
    TRACE_ENTER(__func__)

    if (bank_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: bank_file is NULL\n", __func__, __LINE__);
    }

    if (buffer == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: buffer is NULL\n", __func__, __LINE__);
    }

    if (pos_ptr == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: pos_ptr is NULL\n", __func__, __LINE__);
    }

    int bank_count;
    int pos = *pos_ptr;
    struct LinkedList *list_sounds = LinkedList_new();
    struct LinkedListNode *node;

    ALBankFile_clear_visited_flags(bank_file);

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf(".ctl begin writing envelopes position %d\n", pos);
    }

    for (bank_count=0; bank_count<bank_file->bank_count; bank_count++)
    {
        struct ALBank *bank = bank_file->banks[bank_count];

        if (bank != NULL)
        {
            int inst_count;

            for (inst_count=0; inst_count<bank->inst_count; inst_count++)
            {
                struct ALInstrument *instrument = bank->instruments[inst_count];

                if (instrument != NULL && instrument->visited == 0)
                {
                    int sound_count;

                    instrument->visited = 1;

                    for (sound_count=0; sound_count<instrument->sound_count; sound_count++)
                    {
                        struct ALSound *sound = instrument->sounds[sound_count];

                        if (sound != NULL && sound->visited == 0)
                        {
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

    LinkedList_merge_sort(list_sounds, LinkedListNode_sound_envelope_write_order_compare_smaller);
    ALBankFile_clear_visited_flags(bank_file);

    node = list_sounds->head;
    while (node != NULL)
    {
        struct ALSound *sound = (struct ALSound *)node->data;

        if (sound->envelope != NULL && sound->envelope->visited == 0)
        {
            sound->envelope->visited = 1;

            // set envelope parent .ctl file offset
            sound->envelope_offset = pos;

            // set offset available to all parents.
            sound->envelope->self_offset = pos;

            ALEnvelope_write_ctl(sound->envelope, buffer, buffer_size, &pos);
        }

        node = node->next;
    }

    *pos_ptr = pos;

    // cleanup
    LinkedList_free(list_sounds);

    TRACE_LEAVE(__func__)
}

/**
 * Collects all keymaps in the bank file, then sorts them according
 * to the ctl_write_order property. Then writes them to the buffer
 * in this order.
 * @param bank_file: bank file containing keymaps.
 * @param buffer: buffer to write to.
 * @param buffer_size: max size in bytes of the buffer.
 * @param pos_ptr: in/out parameter. Start position of buffer to write
 * to, will contain offset of next position to write to.
*/
static void ALBankFile_write_meta_order_keymap_ctl(struct ALBankFile *bank_file, uint8_t *buffer, size_t buffer_size, int *pos_ptr)
{
    TRACE_ENTER(__func__)

    if (bank_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: bank_file is NULL\n", __func__, __LINE__);
    }

    if (buffer == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: buffer is NULL\n", __func__, __LINE__);
    }

    if (pos_ptr == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: pos_ptr is NULL\n", __func__, __LINE__);
    }

    int bank_count;
    int pos = *pos_ptr;
    struct LinkedList *list_sounds = LinkedList_new();
    struct LinkedListNode *node;

    ALBankFile_clear_visited_flags(bank_file);

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf(".ctl begin writing keymaps position %d\n", pos);
    }

    for (bank_count=0; bank_count<bank_file->bank_count; bank_count++)
    {
        struct ALBank *bank = bank_file->banks[bank_count];

        if (bank != NULL)
        {
            int inst_count;

            for (inst_count=0; inst_count<bank->inst_count; inst_count++)
            {
                struct ALInstrument *instrument = bank->instruments[inst_count];

                if (instrument != NULL && instrument->visited == 0)
                {
                    int sound_count;

                    instrument->visited = 1;

                    for (sound_count=0; sound_count<instrument->sound_count; sound_count++)
                    {
                        struct ALSound *sound = instrument->sounds[sound_count];

                        if (sound != NULL && sound->visited == 0)
                        {
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

    LinkedList_merge_sort(list_sounds, LinkedListNode_sound_keymap_write_order_compare_smaller);
    ALBankFile_clear_visited_flags(bank_file);

    node = list_sounds->head;
    while (node != NULL)
    {
        struct ALSound *sound = (struct ALSound *)node->data;

        if (sound->keymap != NULL && sound->keymap->visited == 0)
        {
            sound->keymap->visited = 1;

            // set keymap parent .ctl file offset
            sound->keymap_offset = pos;

            // set offset available to all parents.
            sound->keymap->self_offset = pos;

            ALKeyMap_write_ctl(sound->keymap, buffer, buffer_size, &pos);
        }

        node = node->next;
    }

    *pos_ptr = pos;

    // cleanup
    LinkedList_free(list_sounds);

    TRACE_LEAVE(__func__)
}

/**
 * Iterates the bank file and writes all sounds
 * into the buffer in the order encountered.
 * No child information is written.
 * @param bank_file: bank file containing sounds.
 * @param buffer: buffer to write to.
 * @param buffer_size: max size in bytes of the buffer.
 * @param pos_ptr: in/out parameter. Start position of buffer to write
 * to, will contain offset of next position to write to.
*/
static void ALBankFile_write_natural_order_sound_ctl(struct ALBankFile *bank_file, uint8_t *buffer, size_t buffer_size, int *pos_ptr)
{
    TRACE_ENTER(__func__)

    if (bank_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: bank_file is NULL\n", __func__, __LINE__);
    }

    if (buffer == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: buffer is NULL\n", __func__, __LINE__);
    }

    if (pos_ptr == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: pos_ptr is NULL\n", __func__, __LINE__);
    }

    int bank_count;
    int pos = *pos_ptr;

    ALBankFile_clear_visited_flags(bank_file);

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf(".ctl begin writing sounds position %d\n", pos);
    }

    for (bank_count=0; bank_count<bank_file->bank_count; bank_count++)
    {
        struct ALBank *bank = bank_file->banks[bank_count];

        if (bank != NULL)
        {
            int inst_count;

            for (inst_count=0; inst_count<bank->inst_count; inst_count++)
            {
                struct ALInstrument *instrument = bank->instruments[inst_count];

                if (instrument != NULL && instrument->visited == 0)
                {
                    int sound_count;

                    instrument->visited = 1;

                    for (sound_count=0; sound_count<instrument->sound_count; sound_count++)
                    {
                        struct ALSound *sound = instrument->sounds[sound_count];
                        
                        if (sound != NULL && sound->visited == 0)
                        {
                            sound->visited = 1;
                            instrument->sound_offsets[sound_count] = pos;
                            sound->self_offset = pos;
                            ALSound_write_ctl(sound, buffer, buffer_size, &pos);
                        }
                    }
                }
            }
        }
    }

    *pos_ptr = pos;

    TRACE_LEAVE(__func__)
}

/**
 * Collects all sounds in the bank file, then sorts them according
 * to the ctl_write_order property. Then writes them to the buffer
 * in this order.
 * No child information is written.
 * @param bank_file: bank file containing sounds.
 * @param buffer: buffer to write to.
 * @param buffer_size: max size in bytes of the buffer.
 * @param pos_ptr: in/out parameter. Start position of buffer to write
 * to, will contain offset of next position to write to.
*/
static void ALBankFile_write_meta_order_sound_ctl(struct ALBankFile *bank_file, uint8_t *buffer, size_t buffer_size, int *pos_ptr)
{
    TRACE_ENTER(__func__)

    if (bank_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: bank_file is NULL\n", __func__, __LINE__);
    }

    if (buffer == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: buffer is NULL\n", __func__, __LINE__);
    }

    if (pos_ptr == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: pos_ptr is NULL\n", __func__, __LINE__);
    }

    int bank_count;
    int pos = *pos_ptr;
    struct LinkedList *list_sounds = LinkedList_new();
    struct LinkedListNode *node;

    ALBankFile_clear_visited_flags(bank_file);

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf(".ctl begin writing sounds position %d\n", pos);
    }

    for (bank_count=0; bank_count<bank_file->bank_count; bank_count++)
    {
        struct ALBank *bank = bank_file->banks[bank_count];

        if (bank != NULL)
        {
            int inst_count;

            for (inst_count=0; inst_count<bank->inst_count; inst_count++)
            {
                struct ALInstrument *instrument = bank->instruments[inst_count];

                if (instrument != NULL && instrument->visited == 0)
                {
                    int sound_count;

                    instrument->visited = 1;

                    for (sound_count=0; sound_count<instrument->sound_count; sound_count++)
                    {
                        struct ALSound *sound = instrument->sounds[sound_count];
                        
                        if (sound != NULL && sound->visited == 0)
                        {
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

    LinkedList_merge_sort(list_sounds, LinkedListNode_sound_write_order_compare_smaller);
    ALBankFile_clear_visited_flags(bank_file);

    node = list_sounds->head;
    while (node != NULL)
    {
        struct ALSound *sound = (struct ALSound *)node->data;

        if (sound->visited == 0)
        {
            sound->visited = 1;

            // no reference to parent, so skip setting that.

            // set offset available to all parents.
            sound->self_offset = pos;

            ALSound_write_ctl(sound, buffer, buffer_size, &pos);
        }

        node = node->next;
    }

    *pos_ptr = pos;

    // cleanup
    LinkedList_free(list_sounds);

    TRACE_LEAVE(__func__)
}

/**
 * Iterates the bank file and writes all instruments
 * into the buffer in the order encountered.
 * No child information is written.
 * @param bank_file: bank file containing instruments.
 * @param buffer: buffer to write to.
 * @param buffer_size: max size in bytes of the buffer.
 * @param pos_ptr: in/out parameter. Start position of buffer to write
 * to, will contain offset of next position to write to.
*/
static void ALBankFile_write_instrument_ctl(struct ALBankFile *bank_file, uint8_t *buffer, size_t buffer_size, int *pos_ptr)
{
    TRACE_ENTER(__func__)

    if (bank_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: bank_file is NULL\n", __func__, __LINE__);
    }

    if (buffer == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: buffer is NULL\n", __func__, __LINE__);
    }

    if (pos_ptr == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: pos_ptr is NULL\n", __func__, __LINE__);
    }

    int bank_count;
    uint32_t t32;
    uint16_t t16;
    int pos = *pos_ptr;

    ALBankFile_clear_visited_flags(bank_file);

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf(".ctl begin writing instruments position %d\n", pos);
    }

    for (bank_count=0; bank_count<bank_file->bank_count; bank_count++)
    {
        struct ALBank *bank = bank_file->banks[bank_count];

        if (bank != NULL)
        {
            int inst_count;

            for (inst_count=0; inst_count<bank->inst_count; inst_count++)
            {
                struct ALInstrument *instrument = bank->instruments[inst_count];

                if (instrument != NULL && instrument->visited == 0)
                {
                    size_t size_check;
                    int sound_count;

                    instrument->visited = 1;
                    bank->inst_offsets[inst_count] = pos;
                    instrument->self_offset = pos;

                    // check for buffer overflow
                    size_check = 16 + (4 * instrument->sound_count);
                    // adjust for current position
                    size_check += pos;
                    // adjust for 8 byte align.
                    if ((size_check % 8) > 0)
                    {
                        int remaining = 8 - (size_check - (int)((uint32_t)size_check & (uint32_t)(~0x7)));
                        size_check += remaining;
                    }

                    if (size_check > buffer_size)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "%s %d> exceed buffer length at position %ld writing instrument \"%s\"\n", __func__, __LINE__, pos, instrument->text_id);
                    }

                    buffer[pos] = instrument->volume;
                    pos++;

                    buffer[pos] = instrument->pan;
                    pos++;

                    buffer[pos] = instrument->priority;
                    pos++;

                    buffer[pos] = instrument->flags;
                    pos++;

                    buffer[pos] = instrument->trem_type;
                    pos++;

                    buffer[pos] = instrument->trem_rate;
                    pos++;

                    buffer[pos] = instrument->trem_depth;
                    pos++;

                    buffer[pos] = instrument->trem_delay;
                    pos++;

                    buffer[pos] = instrument->vib_type;
                    pos++;

                    buffer[pos] = instrument->vib_rate;
                    pos++;

                    buffer[pos] = instrument->vib_depth;
                    pos++;

                    buffer[pos] = instrument->vib_delay;
                    pos++;

                    t16 = BSWAP16_INLINE(instrument->bend_range);
                    memcpy(&buffer[pos], &t16, 2);
                    pos += 2;

                    t16 = BSWAP16_INLINE(instrument->sound_count);
                    memcpy(&buffer[pos], &t16, 2);
                    pos += 2;

                    for (sound_count=0; sound_count<instrument->sound_count; sound_count++)
                    {
                        if (instrument->sounds[sound_count] != NULL)
                        {
                            t32 = BSWAP32_INLINE(instrument->sounds[sound_count]->self_offset);
                        }
                        else
                        {
                            t32 = BSWAP32_INLINE(instrument->sound_offsets[sound_count]);
                        }
                        memcpy(&buffer[pos], &t32, 4);
                        pos += 4;
                    }

                    // pad to 8-byte align
                    if ((pos % 8) > 0)
                    {
                        int remaining = 8 - (pos - (int)((uint32_t)pos & (uint32_t)(~0x7)));
                        pos += remaining;
                    }
                }
            }
        }
    }

    *pos_ptr = pos;

    TRACE_LEAVE(__func__)
}

/**
 * Iterates the bank file and writes all banks
 * into the buffer in the order encountered.
 * No child information is written.
 * @param bank_file: bank file containing banks.
 * @param buffer: buffer to write to.
 * @param buffer_size: max size in bytes of the buffer.
 * @param pos_ptr: in/out parameter. Start position of buffer to write
 * to, will contain offset of next position to write to.
*/
static void ALBankFile_write_bank_ctl(struct ALBankFile *bank_file, uint8_t *buffer, size_t buffer_size, int *pos_ptr)
{
    TRACE_ENTER(__func__)

    if (bank_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: bank_file is NULL\n", __func__, __LINE__);
    }

    if (buffer == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: buffer is NULL\n", __func__, __LINE__);
    }

    if (pos_ptr == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: pos_ptr is NULL\n", __func__, __LINE__);
    }

    int bank_count;
    uint32_t t32;
    uint16_t t16;
    int pos = *pos_ptr;

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf(".ctl begin writing banks position %d\n", pos);
    }

    for (bank_count=0; bank_count<bank_file->bank_count; bank_count++)
    {
        struct ALBank *bank = bank_file->banks[bank_count];

        if (bank != NULL)
        {
            int inst_count;
            size_t size_check;

            // can finally resolve bank offsets needed at start of file.
            bank_file->bank_offsets[bank_count] = pos;

            size_check = 14 + (4 * bank->inst_count) + 4;
            size_check += pos;
            size_check = (size_t)((uint32_t)(size_check + 16) & (uint32_t)(~ 0xf));

            if (size_check > buffer_size)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> exceed buffer length at position %ld writing bank \"%s\"\n", __func__, __LINE__, pos, bank->text_id);
            }

            t16 = BSWAP16_INLINE(bank->inst_count);
            memcpy(&buffer[pos], &t16, 2);
            pos += 2;

            buffer[pos] = bank->flags;
            pos++;

            // unused padding.
            pos++;

            t32 = BSWAP32_INLINE(bank->sample_rate);
            memcpy(&buffer[pos], &t32, 4);
            pos += 4;

            t32 = BSWAP32_INLINE(bank->percussion);
            memcpy(&buffer[pos], &t32, 4);
            pos += 4;

            for (inst_count=0; inst_count<bank->inst_count; inst_count++)
            {
                t32 = BSWAP32_INLINE(bank->inst_offsets[inst_count]);
                memcpy(&buffer[pos], &t32, 4);
                pos += 4;
            }

            // write an empty inst offset, maybe?
            pos += 4;

            // pad to 16-byte align
            int remaining = 16 - (pos - (int)((uint32_t)pos & (uint32_t)(~0xf)));
            pos += remaining;
        }
    }

    *pos_ptr = pos;

    TRACE_LEAVE(__func__)
}

/**
 * Helper method, copies loop and book data from .aifc file into wavetable.
 * @param wavetable: wavetable to set properties on.
 * @param aifc_file: source .aifc file.
*/
static void ALWaveTable_populate_from_aifc(struct ALWaveTable *wavetable, struct AdpcmAifcFile *aifc_file)
{
    TRACE_ENTER(__func__)

    if (wavetable == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> wavetable is NULL\n", __func__, __LINE__);
    }

    if (aifc_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> aifc_file is NULL\n", __func__, __LINE__);
    }

    if (aifc_file->comm_chunk == NULL)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> aifc_file->comm_chunk is NULL\n", __func__, __LINE__);
    }

    if (aifc_file->comm_chunk->compression_type == ADPCM_AIFC_VAPC_COMPRESSION_TYPE_ID)
    {
        /**
         * Loop info, with associated state.
         * Book info, with associated state.
        */
        wavetable->type = AL_ADPCM_WAVE;

        if (aifc_file->loop_chunk != NULL)
        {
            if (wavetable->wave_info.adpcm_wave.loop == NULL)
            {
                wavetable->wave_info.adpcm_wave.loop = ALADPCMLoop_new_from_aifc_loop(aifc_file->loop_chunk);
            }
            else
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> wavetable->wave_info.adpcm_wave.loop previously allocated\n", __func__, __LINE__);
            }
        }

        if (aifc_file->codes_chunk != NULL)
        {
            if (wavetable->wave_info.adpcm_wave.book == NULL)
            {
                wavetable->wave_info.adpcm_wave.book = ALADPCMBook_new_from_aifc_book(aifc_file->codes_chunk);
            }
            else
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> wavetable->wave_info.adpcm_wave.book previously allocated\n", __func__, __LINE__);
            }
        }
    }
    else if (aifc_file->comm_chunk->compression_type == ADPCM_AIFC_NONE_COMPRESSION_TYPE_ID)
    {
        /**
         * Loop info, no associated state.
         * No book data.
        */
        wavetable->type = AL_RAW16_WAVE;

        if (aifc_file->loop_chunk != NULL)
        {
            if (wavetable->wave_info.raw_wave.loop == NULL)
            {
                wavetable->wave_info.raw_wave.loop = ALRawLoop_new_from_aifc_loop(aifc_file->loop_chunk);
            }
            else
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> wavetable->wave_info.raw_wave.loop previously allocated\n", __func__, __LINE__);
            }
        }
    }
    else
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> unsupported compression type: 0x%08x\n", __func__, __LINE__, aifc_file->comm_chunk->compression_type);
    }

    TRACE_LEAVE(__func__)
}

/**
 * Iterates bank file and loads .aifc data into every associated wavetable.
 * @param bank_file: bank file to load data for.
*/
static void ALBankFile_populate_wavetables_from_aifc(struct ALBankFile *bank_file)
{
    TRACE_ENTER(__func__)

    if (bank_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: bank_file is NULL\n", __func__, __LINE__);
    }

    ALBankFile_clear_visited_flags(bank_file);

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

                    for (sound_count=0; sound_count<instrument->sound_count; sound_count++)
                    {
                        struct ALSound *sound = instrument->sounds[sound_count];

                        if (sound != NULL)
                        {
                            struct ALWaveTable *wavetable = sound->wavetable;

                            if (wavetable != NULL && wavetable->aifc_path != NULL && wavetable->aifc_path[0] != '\0')
                            {
                                struct FileInfo *aifc_fi;
                                struct AdpcmAifcFile *aifc_file;

                                if (wavetable->visited == 1)
                                {
                                    continue;
                                }

                                wavetable->visited = 1;

                                aifc_fi = FileInfo_fopen(wavetable->aifc_path, "rb");
                                aifc_file = AdpcmAifcFile_new_from_file(aifc_fi);

                                ALWaveTable_populate_from_aifc(wavetable, aifc_file);
                                
                                AdpcmAifcFile_free(aifc_file);
                                FileInfo_free(aifc_fi);
                            }
                        }
                    }
                }
            }
        }
    }

    TRACE_LEAVE(__func__)
}