#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "debug.h"
#include "common.h"
#include "utility.h"
#include "adpcm_aifc.h"
#include "wav.h"

/**
 * This file contains primary wav methods.
 * This contains code to convert from other formats to wav.
*/

/**
 * Allocates memory for a {@code struct WavDataChunk} and sets known/const values.
 * @returns: pointer to new chunk.
*/
struct WavDataChunk *WavDataChunk_new()
{
    TRACE_ENTER(__func__)

    struct WavDataChunk *p = (struct WavDataChunk *)malloc_zero(1, sizeof(struct WavDataChunk));

    p->ck_id = WAV_DATA_CHUNK_ID;

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Allocates memory for a {@code struct WavFmtChunk} and sets known/const values.
 * @returns: pointer to new chunk.
*/
struct WavFmtChunk *WavFmtChunk_new()
{
    TRACE_ENTER(__func__)

    struct WavFmtChunk *p = (struct WavFmtChunk *)malloc_zero(1, sizeof(struct WavFmtChunk));

    p->ck_id = WAV_FMT_CHUNK_ID;
    p->ck_data_size = WAV_FMT_CHUNK_BODY_SIZE;

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Allocates memory for a {@code struct WavFile} and sets known/const values.
 * @param num_chunks: length of chunks array to allocate.
 * @returns: pointer to new object.
*/
struct WavFile *WavFile_new(size_t num_chunks)
{
    TRACE_ENTER(__func__)

    struct WavFile *p = (struct WavFile *)malloc_zero(1, sizeof(struct WavFile));

    p->ck_id = WAV_RIFF_CHUNK_ID;
    p->form_type = WAV_RIFF_TYPE_ID;

    p->chunk_count = num_chunks;
    p->chunks = (void **)malloc_zero(num_chunks, sizeof(void *));

    TRACE_LEAVE(__func__)

    return p;
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
 * Frees memory allocated to chunk.
 * @param chunk: object to free.
*/
void WavDataChunk_free(struct WavDataChunk *chunk)
{
    TRACE_ENTER(__func__)

    if (chunk == NULL)
    {
        return;
    }

    if (chunk->data != NULL)
    {
        free(chunk->data);
    }

    free(chunk);

    TRACE_LEAVE(__func__)
}

/**
 * Frees memory allocated to chunk.
 * @param chunk: object to free.
*/
void WavFmtChunk_free(struct WavFmtChunk *chunk)
{
    TRACE_ENTER(__func__)

    if (chunk == NULL)
    {
        return;
    }

    free(chunk);

    TRACE_LEAVE(__func__)
}

/**
 * Frees memory allocated to wav file and all child elements.
 * @param wav_file: object to free.
*/
void WavFile_free(struct WavFile *wav_file)
{
    TRACE_ENTER(__func__)

    int i;

    if (wav_file == NULL)
    {
        return;
    }

    if (wav_file->chunks != NULL)
    {
        // need to iterate the list in case there are duplicates.
        for (i=0; i<wav_file->chunk_count; i++)
        {
            uint32_t ck_id = *(uint32_t *)wav_file->chunks[i];
            switch (ck_id)
            {
                case WAV_FMT_CHUNK_ID:
                    WavFmtChunk_free((struct WavFmtChunk *)wav_file->chunks[i]);
                    wav_file->fmt_chunk = NULL;
                    break;

                case WAV_DATA_CHUNK_ID:
                    WavDataChunk_free((struct WavDataChunk *)wav_file->chunks[i]);
                    wav_file->data_chunk = NULL;
                    break;
                
                default:
                    // ignore unsupported
                    break;
            }
        }

        free(wav_file->chunks);
    }

    free(wav_file);

    TRACE_LEAVE(__func__)
}

/**
 * Writes a {@code struct WavDataChunk} to disk.
 * @param chunk: Chunk to write.
 * @param fi: File handle to write to, using current offset.
*/
void WavDataChunk_fwrite(struct WavDataChunk *chunk, struct file_info *fi)
{
    TRACE_ENTER(__func__)

    file_info_fwrite_bswap(fi, &chunk->ck_id, 4, 1);
    file_info_fwrite(fi, &chunk->ck_data_size, 4, 1);

    if (chunk->ck_data_size > 0)
    {
        file_info_fwrite(fi, chunk->data, (size_t)chunk->ck_data_size, 1);
    }

    TRACE_LEAVE(__func__)
}

/**
 * Writes a {@code struct WavFmtChunk} to disk.
 * @param chunk: Chunk to write.
 * @param fi: File handle to write to, using current offset.
*/
void WavFmtChunk_fwrite(struct WavFmtChunk *chunk, struct file_info *fi)
{
    TRACE_ENTER(__func__)

    file_info_fwrite_bswap(fi, &chunk->ck_id, 4, 1);
    file_info_fwrite(fi, &chunk->ck_data_size, 4, 1);
    file_info_fwrite(fi, &chunk->audio_format, 2, 1);
    file_info_fwrite(fi, &chunk->num_channels, 2, 1);
    file_info_fwrite(fi, &chunk->sample_rate, 4, 1);
    file_info_fwrite(fi, &chunk->byte_rate, 4, 1);
    file_info_fwrite(fi, &chunk->block_align, 2, 1);
    file_info_fwrite(fi, &chunk->bits_per_sample, 2, 1);

    TRACE_LEAVE(__func__)
}

/**
 * Writes the full {@code struct WavFile} to disk.
 * Only supported chunks are written to disk, all others are ignored.
 * @param wav_file: wav to write.
 * @param fi: File handle to write to, using current offset.
*/
void WavFile_fwrite(struct WavFile *wav_file, struct file_info *fi)
{
    TRACE_ENTER(__func__)

    int i;

    file_info_fwrite_bswap(fi, &wav_file->ck_id, 4, 1);
    file_info_fwrite(fi, &wav_file->ck_data_size, 4, 1);
    file_info_fwrite_bswap(fi, &wav_file->form_type, 4, 1);

    for (i=0; i<wav_file->chunk_count; i++)
    {
        uint32_t ck_id = *(uint32_t*)wav_file->chunks[i];
        switch (ck_id)
        {
            case WAV_FMT_CHUNK_ID: // "fmt "
            {
                struct WavFmtChunk *chunk = (struct WavFmtChunk *)wav_file->chunks[i];
                WavFmtChunk_fwrite(chunk, fi);
            }
            break;

            case WAV_DATA_CHUNK_ID: // data
            {
                struct WavDataChunk *chunk = (struct WavDataChunk *)wav_file->chunks[i];
                WavDataChunk_fwrite(chunk, fi);
            }
            break;

            default:
                // ignore unsupported
            {
                if (g_verbosity >= 2)
                {
                    printf("WavFile_fwrite: ignore ck_id 0x%08x\n", ck_id);
                }
            }
            break;
        }
    }

    TRACE_LEAVE(__func__)
}