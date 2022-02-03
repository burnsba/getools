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

/**
 * This contains code that converts between audio formats.
*/

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
                stderr_exit(EXIT_CODE_GENERAL, "Cannot find ALADPCMBook to resolve codebook data, sound %s, bank %s\n", sound->text_id, bank->text_id);
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
            stderr_exit(EXIT_CODE_GENERAL, "wavetable->len is zero, sound %s, bank %s\n", sound->text_id, bank->text_id);
        }
    }
    else
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "wavetable is NULL, sound %s, bank %s\n", sound->text_id, bank->text_id);
    }

    if (need_loop)
    {
        aaf->chunks[alloc_chunk_count] = AdpcmAifcLoopChunk_new();
        aaf->loop_chunk = aaf->chunks[alloc_chunk_count];
        alloc_chunk_count++;
    }

    if (alloc_chunk_count != expected_chunk_count)
    {
        stderr_exit(EXIT_CODE_GENERAL, "Expected to allocate %d chunks, but only allocated %d. Sound %s, bank %s\n", expected_chunk_count, alloc_chunk_count, sound->text_id, bank->text_id);
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

    aaf->ck_data_size = 0;

    if (aaf->comm_chunk == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "error loading aifc from sound, common chunk is NULL, sound %s, bank %s\n", sound->text_id, bank->text_id);
    }

    // COMM chunk
    aaf->comm_chunk->num_channels = DEFAULT_NUM_CHANNELS;
    aaf->comm_chunk->sample_size = DEFAULT_SAMPLE_SIZE;

    f80 float_rate = (f80)(bank->sample_rate);
    reverse_into(aaf->comm_chunk->sample_rate, (uint8_t *)&float_rate, 10);

    aaf->ck_data_size += aaf->comm_chunk->ck_data_size;

    if (sound->wavetable == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "wavetable is NULL, sound %s, bank %s\n", sound->text_id, bank->text_id);
    }

    if (sound->wavetable->wave_info.adpcm_wave.book != NULL)
    {
        if (aaf->codes_chunk == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "error loading aifc from sound, codebook chunk is NULL, sound %s, bank %s\n", sound->text_id, bank->text_id);
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
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "error loading aifc from sound, sound chunk is NULL, sound %s, bank %s\n", sound->text_id, bank->text_id);
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
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "error loading aifc from sound, loop chunk is NULL, sound %s, bank %s\n", sound->text_id, bank->text_id);
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
                ADPCM_STATE_SIZE);
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
            stderr_exit(EXIT_CODE_GENERAL, "Unsupported loop type: %d. Sound %s, bank %s\n", sound->wavetable->type, sound->text_id, bank->text_id);
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
 * @param fi: file_info to write to. Uses current seek position.
*/
void write_sound_to_aifc(struct ALSound *sound, struct ALBank *bank, uint8_t *tbl_file_contents, struct file_info *fi)
{
    TRACE_ENTER(__func__)

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

    struct file_info *output;
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
                    stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: sound is NULL\n", __func__);
                }

                if (sound->wavetable == NULL)
                {
                    stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: sound->wavetable is NULL\n", __func__);
                }

                if (sound->wavetable->visited == 0)
                {
                    sound->wavetable->visited = 1;

                    if (g_verbosity >= VERBOSE_DEBUG)
                    {
                        printf("opening sound file for output aifc: \"%s\"\n", sound->wavetable->aifc_path);
                    }

                    output = file_info_fopen(sound->wavetable->aifc_path, "w");

                    write_sound_to_aifc(sound, bank, tbl_file_contents, output);

                    file_info_free(output);
                }
            }
        }
    }

    TRACE_LEAVE(__func__)
}


/**
 * Translates from .aifc to .wav.
 * The .aifc must be loaded into memory.
 * Allocates memory for resulting wav file, including uncompressed audio.
 * The exact .aifc uncompressed audio size is not known, so slightly more space
 * is allocated than is needed.
 * @param aifc_file: aifc file to convert to wav
 * @returns: pointer to new wav file.
*/
struct WavFile *WavFile_load_from_aifc(struct AdpcmAifcFile *aifc_file)
{
    TRACE_ENTER(__func__)

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

    TRACE_LEAVE(__func__)

    return wav;
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

    struct file_info *output;
    int bank_count;
    struct StringHashTable *seen = StringHashTable_new();

    output = file_info_fopen(tbl_filename, "w");

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
                                if (!StringHashTable_contains(seen, wavetable->aifc_path))
                                {
                                    int32_t wavetable_base = (int32_t)file_info_ftell(output);

                                    AdpcmAifcFile_path_write_tbl(wavetable->aifc_path, output);

                                    wavetable->base = wavetable_base;

                                    StringHashTable_add(seen, wavetable->aifc_path, wavetable);
                                }
                                else
                                {
                                    struct ALWaveTable *ht_wavetable = StringHashTable_get(seen, wavetable->aifc_path);

                                    wavetable->base = ht_wavetable->base;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    file_info_free(output);

    StringHashTable_free(seen);

    TRACE_LEAVE(__func__)
}