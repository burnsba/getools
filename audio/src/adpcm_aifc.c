#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "debug.h"
#include "common.h"
#include "utility.h"
#include "adpcm_aifc.h"

struct AdpcmAifcFile *AdpcmAifcFile_new_simple(size_t chunk_count)
{
    TRACE_ENTER("AdpcmAifcFile_new_simple")

    struct AdpcmAifcFile *p = (struct AdpcmAifcFile *)malloc_zero(1, sizeof(struct AdpcmAifcFile));
    p->ck_id = ADPCM_AIFC_FORM_CHUNK_ID;
    p->form_type = ADPCM_AIFC_FORM_TYPE_ID;

    p->chunk_count = chunk_count;
    p->chunks = (void*)malloc_zero(chunk_count, sizeof(void*));

    TRACE_LEAVE("AdpcmAifcFile_new_simple");

    return p;
}

struct AdpcmAifcFile *AdpcmAifcFile_new_from_file(struct file_info *fi)
{
    TRACE_ENTER("AdpcmAifcFile_new_from_file")

    file_info_fseek(fi, 0, SEEK_SET);

    struct AdpcmAifcFile *p = (struct AdpcmAifcFile *)malloc_zero(1, sizeof(struct AdpcmAifcFile));
    p->ck_id = ADPCM_AIFC_FORM_CHUNK_ID;
    p->form_type = ADPCM_AIFC_FORM_TYPE_ID;

    TRACE_LEAVE("AdpcmAifcFile_new_from_file")

    return p;
}

struct AdpcmAifcFile *AdpcmAifcFile_new_full(struct ALSound *sound, struct ALBank *bank)
{
    TRACE_ENTER("AdpcmAifcFile_new_full")

    int chunk_count = 3; // COMM, APPL VADPCMCODES, SSND.

    if (sound->wavetable != NULL
        && sound->wavetable->type == AL_ADPCM_WAVE
        // it doesn't matter which wave_info we dereference here
        && sound->wavetable->wave_info.adpcm_wave.loop != NULL)
    {
        chunk_count++; // APPL VADPCMLOOPS
    }

    struct AdpcmAifcFile *aaf = AdpcmAifcFile_new_simple(chunk_count);

    aaf->chunks[0] = AdpcmAifcCommChunk_new();
    aaf->comm_chunk = aaf->chunks[0];

    if (sound->wavetable != NULL
        && sound->wavetable->type == AL_ADPCM_WAVE
        && sound->wavetable->wave_info.adpcm_wave.book != NULL)
    {
        struct ALADPCMBook *book = sound->wavetable->wave_info.adpcm_wave.book;
        aaf->chunks[1] = AdpcmAifcCodebookChunk_new(book->order, book->npredictors);
        aaf->codes_chunk = aaf->chunks[1];
    }
    else
    {
        fprintf(stderr, "Cannot find ALADPCMBook to resolve sound data, sound %s, bank %s\n", sound->text_id, bank->text_id);
        fflush(stderr);
        exit(1);
    }

    if (sound->wavetable != NULL)
    {
        if (sound->wavetable->len > 0)
        {
            aaf->chunks[2] = AdpcmAifcSoundChunk_new(sound->wavetable->len);
            aaf->sound_chunk = aaf->chunks[2];
        }
        else
        {
            fprintf(stderr, "wavetable->len is zero, sound %s, bank %s\n", sound->text_id, bank->text_id);
            fflush(stderr);
            exit(1);
        }
    }
    else
    {
        fprintf(stderr, "wavetable is NULL, sound %s, bank %s\n", sound->text_id, bank->text_id);
        fflush(stderr);
        exit(1);
    }

    if (chunk_count == 4)
    {
        aaf->chunks[3] = AdpcmAifcLoopChunk_new();
        aaf->loop_chunk = aaf->chunks[3];
    }

    TRACE_LEAVE("AdpcmAifcFile_new_full");

    return aaf;
}

struct AdpcmAifcCommChunk *AdpcmAifcCommChunk_new()
{
    TRACE_ENTER("AdpcmAifcCommChunk_new")

    struct AdpcmAifcCommChunk *p = (struct AdpcmAifcCommChunk *)malloc_zero(1, sizeof(struct AdpcmAifcCommChunk));
    p->ck_id = ADPCM_AIFC_COMMON_CHUNK_ID;
    p->num_channels = DEFAULT_NUM_CHANNELS;
    p->ck_data_size = 2 + 4 + 2 + 10 + 4 + 1 + 11;
    p->unknown = 0xb;
    p->compression_type = ADPCM_AIFC_COMPRESSION_TYPE_ID;
    
    // no terminating zero
    memcpy(p->compression_name, ADPCM_AIFC_VADPCM_COMPRESSION_NAME, 11);
    
    TRACE_LEAVE("AdpcmAifcCommChunk_new");
    
    return p;
}

struct AdpcmAifcCodebookChunk *AdpcmAifcCodebookChunk_new(int16_t order, uint16_t nentries)
{
    TRACE_ENTER("AdpcmAifcCodebookChunk_new")

    size_t table_data_size_bytes = order * nentries * 16;
    struct AdpcmAifcCodebookChunk *p = (struct AdpcmAifcCodebookChunk *)malloc_zero(1, sizeof(struct AdpcmAifcCommChunk));
    p->base.ck_id = ADPCM_AIFC_APPLICATION_CHUNK_ID;
    p->base.ck_data_size = 4 + 1 + 11 + 2 + 2 + 2 + table_data_size_bytes;
    p->base.application_signature = ADPCM_AIFC_APPLICATION_SIGNATURE;
    
    // no terminating zero
    memcpy(p->base.code_string, ADPCM_AIFC_VADPCM_CODES_NAME, 11);

    p->nentries = nentries;

    p->table_data = (uint8_t *)malloc_zero(1, table_data_size_bytes);

    TRACE_LEAVE("AdpcmAifcCodebookChunk_new");

    return p;
}

struct AdpcmAifcSoundChunk *AdpcmAifcSoundChunk_new(size_t sound_data_size_bytes)
{
    TRACE_ENTER("AdpcmAifcSoundChunk_new")

    struct AdpcmAifcSoundChunk *p = (struct AdpcmAifcSoundChunk *)malloc_zero(1, sizeof(struct AdpcmAifcSoundChunk));
    p->ck_id = ADPCM_AIFC_SOUND_CHUNK_ID;
    p->ck_data_size = 4 + 4 + sound_data_size_bytes;

    p->sound_data = (uint8_t *)malloc_zero(1, sound_data_size_bytes);

    TRACE_LEAVE("AdpcmAifcSoundChunk_new");

    return p;
}

struct AdpcmAifcLoopChunk *AdpcmAifcLoopChunk_new()
{
    TRACE_ENTER("AdpcmAifcLoopChunk_new")

    // nloops * sizeof(struct AdpcmAifcLoopData)
    size_t loop_data_size_bytes = 4 + 4 + 4 + 0x20;
    struct AdpcmAifcLoopChunk *p = (struct AdpcmAifcLoopChunk *)malloc_zero(1, sizeof(struct AdpcmAifcLoopChunk));
    p->base.ck_id = ADPCM_AIFC_APPLICATION_CHUNK_ID;
    p->base.ck_data_size = 4 + 1 + 11 + 2 + 2 + loop_data_size_bytes;
    p->base.application_signature = ADPCM_AIFC_APPLICATION_SIGNATURE;
    
    // no terminating zero
    memcpy(p->base.code_string, ADPCM_AIFC_VADPCM_LOOPS_NAME, 11);

    p->nloops = 1;

    p->loop_data = (struct AdpcmAifcLoopData *)malloc_zero(1, loop_data_size_bytes);

    TRACE_LEAVE("AdpcmAifcLoopChunk_new");

    return p;
}

void load_aifc_from_sound(struct AdpcmAifcFile *aaf, struct ALSound *sound, uint8_t *tbl_file_contents, struct ALBank *bank)
{
    TRACE_ENTER("load_aifc_from_sound")

    aaf->ck_data_size = 0;

    // COMM chunk
    aaf->comm_chunk->num_channels = DEFAULT_NUM_CHANNELS;
    aaf->comm_chunk->sample_size = DEFAULT_SAMPLE_SIZE;

    f80 float_rate = (f80)(bank->sample_rate);
    reverse_into(aaf->comm_chunk->sample_rate, (uint8_t *)&float_rate, 10);

    aaf->ck_data_size += aaf->comm_chunk->ck_data_size;

    // code book chunk
    aaf->codes_chunk->base.unknown = 0xb; // ??
    aaf->codes_chunk->version = 1; // ??
    if (sound->wavetable != NULL
        && sound->wavetable->type == AL_ADPCM_WAVE
        && sound->wavetable->wave_info.adpcm_wave.book != NULL)
    {
        int code_len;
        struct ALADPCMBook *book = sound->wavetable->wave_info.adpcm_wave.book;
        aaf->codes_chunk->order = book->order;
        aaf->codes_chunk->nentries = book->npredictors;
        code_len = book->order * book->npredictors * 16;
        memcpy(aaf->codes_chunk->table_data, book->book, code_len);

        aaf->ck_data_size += aaf->codes_chunk->base.ck_data_size;
    }

    // sound chunk
    if (sound->wavetable != NULL
        && sound->wavetable->len > 0)
    {
        if (g_verbosity >= VERBOSE_DEBUG)
        {
            printf("copying tbl data from offset 0x%06x len 0x%06x\n", sound->wavetable->base, sound->wavetable->len);
            fflush(stdout);
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
    if (aaf->loop_chunk != NULL
        && sound->wavetable != NULL
        && sound->wavetable->type == AL_ADPCM_WAVE
        && sound->wavetable->wave_info.adpcm_wave.loop != NULL)
    {
        struct ALADPCMloop *loop = sound->wavetable->wave_info.adpcm_wave.loop;

        aaf->loop_chunk->nloops = 1;
        aaf->loop_chunk->loop_data->start = loop->start;
        aaf->loop_chunk->loop_data->end = loop->end;
        aaf->loop_chunk->loop_data->count = loop->count;

        memcpy(
            aaf->loop_chunk->loop_data->state,
            loop->state,
            ADPCM_STATE_SIZE);

        aaf->ck_data_size += aaf->loop_chunk->base.ck_data_size;
    }

    TRACE_LEAVE("load_aifc_from_sound");
}

void AdpcmAifcCommChunk_frwrite(struct AdpcmAifcCommChunk *chunk, struct file_info *fi)
{
    TRACE_ENTER("AdpcmAifcCommChunk_frwrite")

    file_info_fwrite_bswap(fi, &chunk->ck_id, 4, 1);
    file_info_fwrite_bswap(fi, &chunk->ck_data_size, 4, 1);
    file_info_fwrite_bswap(fi, &chunk->num_channels, 2, 1);
    file_info_fwrite_bswap(fi, &chunk->num_sample_frames, 4, 1);
    file_info_fwrite_bswap(fi, &chunk->sample_size, 2, 1);
    file_info_fwrite_bswap(fi, chunk->sample_rate, 10, 1);
    file_info_fwrite_bswap(fi, &chunk->compression_type, 4, 1);
    file_info_fwrite_bswap(fi, &chunk->unknown, 1, 1);
    file_info_fwrite_bswap(fi, chunk->compression_name, 11, 1);
    
    TRACE_LEAVE("AdpcmAifcCommChunk_frwrite");
}

void AdpcmAifcApplicationChunk_frwrite(struct AdpcmAifcApplicationChunk *chunk, struct file_info *fi)
{
    TRACE_ENTER("AdpcmAifcApplicationChunk_frwrite")

    file_info_fwrite_bswap(fi, &chunk->ck_id, 4, 1);
    file_info_fwrite_bswap(fi, &chunk->ck_data_size, 4, 1);
    file_info_fwrite_bswap(fi, &chunk->application_signature, 4, 1);
    file_info_fwrite_bswap(fi, &chunk->unknown, 1, 1);
    file_info_fwrite_bswap(fi, chunk->code_string, 11, 1);
    
    TRACE_LEAVE("AdpcmAifcApplicationChunk_frwrite");
}

void AdpcmAifcCodebookChunk_frwrite(struct AdpcmAifcCodebookChunk *chunk, struct file_info *fi)
{
    TRACE_ENTER("AdpcmAifcCodebookChunk_frwrite")

    size_t table_size;

    AdpcmAifcApplicationChunk_frwrite(&chunk->base, fi);

    file_info_fwrite_bswap(fi, &chunk->version, 2, 1);
    file_info_fwrite_bswap(fi, &chunk->order, 2, 1);
    file_info_fwrite_bswap(fi, &chunk->nentries, 2, 1);

    table_size = chunk->order * chunk->nentries * 16;

    file_info_fwrite_bswap(fi, chunk->table_data, table_size, 1);

    TRACE_LEAVE("AdpcmAifcCodebookChunk_frwrite");
}

void AdpcmAifcSoundChunk_frwrite(struct AdpcmAifcSoundChunk *chunk, struct file_info *fi)
{
    TRACE_ENTER("AdpcmAifcSoundChunk_frwrite")

    size_t table_size;

    file_info_fwrite_bswap(fi, &chunk->ck_id, 4, 1);
    file_info_fwrite_bswap(fi, &chunk->ck_data_size, 4, 1);
    file_info_fwrite_bswap(fi, &chunk->offset, 4, 1);
    file_info_fwrite_bswap(fi, &chunk->block_size, 4, 1);
    
    table_size = chunk->ck_data_size - 8;
    if (table_size > 0 && table_size < INT32_MAX)
    {
        file_info_fwrite(fi, chunk->sound_data, table_size, 1);
    }

    TRACE_LEAVE("AdpcmAifcSoundChunk_frwrite");
}

void AdpcmAifcLoopData_frwrite(struct AdpcmAifcLoopData *loop, struct file_info *fi)
{
    TRACE_ENTER("AdpcmAifcLoopData_frwrite")

    file_info_fwrite_bswap(fi, &loop->start, 4, 1);
    file_info_fwrite_bswap(fi, &loop->end, 4, 1);
    file_info_fwrite_bswap(fi, &loop->count, 4, 1);
    file_info_fwrite_bswap(fi, &loop->state, 0x20, 1);

    TRACE_LEAVE("AdpcmAifcLoopData_frwrite");
}

void AdpcmAifcLoopChunk_frwrite(struct AdpcmAifcLoopChunk *chunk, struct file_info *fi)
{
    TRACE_ENTER("AdpcmAifcLoopChunk_frwrite")

    int i;

    AdpcmAifcApplicationChunk_frwrite(&chunk->base, fi);

    file_info_fwrite_bswap(fi, &chunk->version, 2, 1);
    file_info_fwrite_bswap(fi, &chunk->nloops, 2, 1);

    for (i=0; i<chunk->nloops; i++)
    {
        AdpcmAifcLoopData_frwrite(&chunk->loop_data[i], fi);
    }

    TRACE_LEAVE("AdpcmAifcLoopChunk_frwrite");
}

void AdpcmAifcFile_frwrite(struct AdpcmAifcFile *aaf, struct file_info *fi)
{
    TRACE_ENTER("AdpcmAifcFile_frwrite")

    int i;

    file_info_fwrite_bswap(fi, &aaf->ck_id, 4, 1);
    file_info_fwrite_bswap(fi, &aaf->ck_data_size, 4, 1);
    file_info_fwrite_bswap(fi, &aaf->form_type, 4, 1);

    for (i=0; i<aaf->chunk_count; i++)
    {
        uint32_t ck_id = *(uint32_t*)aaf->chunks[i];
        switch (ck_id)
        {
            case ADPCM_AIFC_COMMON_CHUNK_ID: // COMM
            {
                struct AdpcmAifcCommChunk *chunk = (struct AdpcmAifcCommChunk *)aaf->chunks[i];
                AdpcmAifcCommChunk_frwrite(chunk, fi);
            }
            break;

            case ADPCM_AIFC_SOUND_CHUNK_ID: // SSND
            {
                struct AdpcmAifcSoundChunk *chunk = (struct AdpcmAifcSoundChunk *)aaf->chunks[i];
                AdpcmAifcSoundChunk_frwrite(chunk, fi);
            }
            break;

            case ADPCM_AIFC_APPLICATION_CHUNK_ID: // APPL
            {
                struct AdpcmAifcApplicationChunk *basechunk = (struct AdpcmAifcApplicationChunk *)aaf->chunks[i];
                if (strncmp(basechunk->code_string, ADPCM_AIFC_VADPCM_CODES_NAME, 10) == 0)
                {
                    struct AdpcmAifcCodebookChunk *chunk = (struct AdpcmAifcCodebookChunk *)basechunk;
                    AdpcmAifcCodebookChunk_frwrite(chunk, fi);
                }
                else if (strncmp(basechunk->code_string, ADPCM_AIFC_VADPCM_LOOPS_NAME, 10) == 0)
                {
                    struct AdpcmAifcLoopChunk *chunk = (struct AdpcmAifcLoopChunk *)basechunk;
                    AdpcmAifcLoopChunk_frwrite(chunk, fi);
                }
                // else, ignore unsupported
                else
                {
                    if (g_verbosity >= 2)
                    {
                        printf("AdpcmAifcFile_frwrite: APPL ignore code_string '%s'\n", basechunk->code_string);
                    }
                }
            }
            break;

            default:
                // ignore unsupported
            {
                if (g_verbosity >= 2)
                {
                    printf("AdpcmAifcFile_frwrite: ignore ck_id 0x%08x\n", ck_id);
                }
            }
            break;
        }
    }

    TRACE_LEAVE("AdpcmAifcFile_frwrite");
}

void write_sound_to_aifc(struct ALSound *sound, struct ALBank *bank, uint8_t *tbl_file_contents, struct file_info *fi)
{
    TRACE_ENTER("write_sound_to_aifc")

    struct AdpcmAifcFile *aaf = AdpcmAifcFile_new_full(sound, bank);

    load_aifc_from_sound(aaf, sound, tbl_file_contents, bank);

    AdpcmAifcFile_frwrite(aaf, fi);

    TRACE_LEAVE("write_sound_to_aifc");
}

void write_bank_to_aifc(struct ALBankFile *bank_file, uint8_t *tbl_file_contents)
{
    TRACE_ENTER("write_bank_to_aifc")

    struct file_info *output;
    int i,j,k;

    for (i=0; i<bank_file->bank_count; i++)
    {
        struct ALBank *bank = bank_file->banks[i];
        for (j=0; j<bank->inst_count; j++)
        {
            struct ALInstrument *inst = bank->instruments[j];
            for (k=0; k<inst->sound_count; k++)
            {
                struct ALSound *sound = inst->sounds[k];

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

    TRACE_LEAVE("write_bank_to_aifc");
}

/**
 * Decode .aifc compressed audio and write to output buffer.
 * @param aaf: input source
 * @param buffer: output buffer
 * @param max_len: max number of bytes to write to output buffer
 * @returns: number of bytes written
*/
size_t AdpcmAifcFile_decode(struct AdpcmAifcFile *aaf, uint8_t *buffer, size_t max_len)
{
    TRACE_ENTER("AdpcmAifcFile_decode")

    size_t write_len = 0;

    TRACE_LEAVE("AdpcmAifcFile_decode");

    return write_len;
}