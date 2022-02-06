#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "debug.h"
#include "common.h"
#include "utility.h"
#include "wav.h"

/**
 * This file contains primary wav methods.
*/

static uint32_t g_wav_cue_point_id = 0;

/**
 * Allocates memory for a {@code struct WavSampleLoop} and sets known/const values.
 * @returns: pointer to new object.
*/
struct WavSampleLoop *WavSampleLoop_new()
{
    TRACE_ENTER(__func__)

    struct WavSampleLoop *p = (struct WavSampleLoop *)malloc_zero(1, sizeof(struct WavSampleLoop));

    p->cue_point_id = g_wav_cue_point_id;
    g_wav_cue_point_id++;

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Allocates memory for a {@code struct WavSampleChunk} and sets known/const values.
 * Allocates memory for the list of sample loops, and allocates empty sample loops.
 * @returns: pointer to new chunk.
*/
struct WavSampleChunk *WavSampleChunk_new(int number_loops)
{
    TRACE_ENTER(__func__)

    int i;

    struct WavSampleChunk *p = (struct WavSampleChunk *)malloc_zero(1, sizeof(struct WavSampleChunk));

    p->ck_id = WAV_SMPL_CHUNK_ID;
    p->sample_loop_bytes = number_loops * WAV_SAMPLE_LOOP_SIZE;
    p->ck_data_size = WAV_SMPL_CHUNK_BODY_SIZE + p->sample_loop_bytes;
    p->num_sample_loops = number_loops;

    if (number_loops > 0)
    {
        p->loops = (struct WavSampleLoop **)malloc_zero(1, sizeof(struct WavSampleLoop *));

        for (i=0; i<number_loops; i++)
        {
            p->loops[i] = WavSampleLoop_new();
        }
    }

    TRACE_LEAVE(__func__)

    return p;
}

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
 * Frees memory allocated to loop.
 * @param loop: object to free.
*/
void WavSampleLoop_free(struct WavSampleLoop *loop)
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
 * Frees memory allocated to chunk.
 * @param chunk: object to free.
*/
void WavSampleChunk_free(struct WavSampleChunk *chunk)
{
    TRACE_ENTER(__func__)

    int i;

    if (chunk == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (chunk->num_sample_loops > 0)
    {
        for (i=0; i<chunk->num_sample_loops; i++)
        {
            WavSampleLoop_free(chunk->loops[i]);
            chunk->loops[i] = NULL;
        }
    }

    if (chunk->loops != NULL)
    {
        free(chunk->loops);
        chunk->loops = 0;
    }

    free(chunk);

    TRACE_LEAVE(__func__)
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
        TRACE_LEAVE(__func__)
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
        TRACE_LEAVE(__func__)
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
        TRACE_LEAVE(__func__)
        return;
    }

    if (wav_file->chunks != NULL)
    {
        // need to iterate the list in case there are duplicates.
        for (i=0; i<wav_file->chunk_count; i++)
        {
            if (wav_file->chunks[i] == NULL)
            {
                continue;
            }

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

                case WAV_SMPL_CHUNK_ID:
                    WavSampleChunk_free((struct WavSampleChunk *)wav_file->chunks[i]);
                    wav_file->smpl_chunk = NULL;
                    break;
                
                default:
                    // ignore unsupported
                    break;
            }

            wav_file->chunks[i] = NULL;
        }

        free(wav_file->chunks);
    }

    free(wav_file);

    TRACE_LEAVE(__func__)
}

/**
 * Writes a {@code struct WavSampleLoop} to disk.
 * @param chunk: Chunk to write.
 * @param fi: File handle to write to, using current offset.
*/
void WavSampleLoop_fwrite(struct WavSampleLoop *loop, struct file_info *fi)
{
    TRACE_ENTER(__func__)

    file_info_fwrite(fi, &loop->cue_point_id, 4, 1);
    file_info_fwrite(fi, &loop->loop_type, 4, 1);
    file_info_fwrite(fi, &loop->start, 4, 1);
    file_info_fwrite(fi, &loop->end, 4, 1);
    file_info_fwrite(fi, &loop->fraction, 4, 1);
    file_info_fwrite(fi, &loop->play_count, 4, 1);


    TRACE_LEAVE(__func__)
}

/**
 * Writes a {@code struct WavSampleChunk} to disk.
 * @param chunk: Chunk to write.
 * @param fi: File handle to write to, using current offset.
*/
void WavSampleChunk_fwrite(struct WavSampleChunk *chunk, struct file_info *fi)
{
    TRACE_ENTER(__func__)

    file_info_fwrite_bswap(fi, &chunk->ck_id, 4, 1);
    file_info_fwrite(fi, &chunk->ck_data_size, 4, 1);

    file_info_fwrite(fi, &chunk->manufacturer, 4, 1);
    file_info_fwrite(fi, &chunk->product, 4, 1);
    file_info_fwrite(fi, &chunk->sample_period, 4, 1);
    file_info_fwrite(fi, &chunk->midi_unity_note, 4, 1);
    file_info_fwrite(fi, &chunk->midi_pitch_fraction, 4, 1);
    file_info_fwrite(fi, &chunk->smpte_format, 4, 1);
    file_info_fwrite(fi, &chunk->smpte_offset, 4, 1);
    file_info_fwrite(fi, &chunk->num_sample_loops, 4, 1);
    file_info_fwrite(fi, &chunk->sample_loop_bytes, 4, 1);

    if (chunk->num_sample_loops > 0)
    {
        int i;
        for (i=0; i<chunk->num_sample_loops; i++)
        {
            if (chunk->loops[i] != NULL)
            {
                WavSampleLoop_fwrite(chunk->loops[i], fi);
            }
        }
    }

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

            case WAV_SMPL_CHUNK_ID: // "smpl"
            {
                struct WavSampleChunk *chunk = (struct WavSampleChunk *)wav_file->chunks[i];
                WavSampleChunk_fwrite(chunk, fi);
            }
            break;

            default:
                // ignore unsupported
            {
                if (g_verbosity >= 2)
                {
                    printf("%s: ignore ck_id 0x%08x\n", __func__, ck_id);
                }
            }
            break;
        }
    }

    TRACE_LEAVE(__func__)
}

/**
 * Gets the frequency of the fmt chunk in the wav file.
 * @param wav_file: wav to get frequency for.
 * @returns: fmt chunk frequency.
*/
double WavFile_get_frequency(struct WavFile *wav_file)
{
    TRACE_ENTER(__func__)

    if (wav_file->fmt_chunk == NULL)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s: fmt chunk not found\n", __func__);
    }

    return (double)wav_file->fmt_chunk->sample_rate;

    TRACE_LEAVE(__func__)
}

/**
 * Sets the frequency of the fmt chunk in the wav file.
 * Truncated to int.
 * @param wav_file: wav file to set frequency.
 * @param frequency: new frequency.
*/
void WavFile_set_frequency(struct WavFile *wav_file, double frequency)
{
    TRACE_ENTER(__func__)

    if (wav_file->fmt_chunk == NULL)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s: fmt chunk not found\n", __func__);
    }

    wav_file->fmt_chunk->sample_rate = (int32_t)frequency;

    TRACE_LEAVE(__func__)
}

/**
 * Adds a "smpl" chunk to the wav file.
 * This dynamically resizes the chunk array.
 * The {@code wav_file->smpl_chunk} is set to the new chunk.
 * @param wav_file: wav file to add chunk to.
 * @param chunk: chunk to add.
*/
void WavFile_append_smpl_chunk(struct WavFile *wav_file, struct WavSampleChunk *chunk)
{
    TRACE_ENTER(__func__)

    if (wav_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: wav_file is NULL\n", __func__, __LINE__);
    }

    if (chunk == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: chunk is NULL\n", __func__, __LINE__);
    }

    wav_file->chunk_count++;

    size_t old_size = sizeof(void*) * (wav_file->chunk_count - 1);
    size_t new_size = sizeof(void*) * (wav_file->chunk_count);

    malloc_resize(old_size, (void**)&wav_file->chunks, new_size);

    wav_file->chunks[wav_file->chunk_count - 1] = chunk;
    wav_file->smpl_chunk = chunk;

    TRACE_LEAVE(__func__)
}