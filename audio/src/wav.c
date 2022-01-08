#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "debug.h"
#include "common.h"
#include "utility.h"
#include "adpcm_aifc.h"
#include "wav.h"

struct WavDataChunk *WavDataChunk_new()
{
    TRACE_ENTER("WavDataChunk_new")

    struct WavDataChunk *p = (struct WavDataChunk *)malloc_zero(1, sizeof(struct WavDataChunk));

    p->ck_id = WAV_DATA_CHUNK_ID;

    TRACE_LEAVE("WavDataChunk_new");

    return p;
}

struct WavFmtChunk *WavFmtChunk_new()
{
    TRACE_ENTER("WavFmtChunk_new")

    struct WavFmtChunk *p = (struct WavFmtChunk *)malloc_zero(1, sizeof(struct WavFmtChunk));

    p->ck_id = WAV_FMT_CHUNK_ID;
    p->ck_data_size = WAV_FMT_CHUNK_BODY_SIZE;

    TRACE_LEAVE("WavFmtChunk_new");

    return p;
}

struct WavFile *WavFile_new(size_t num_chunks)
{
    TRACE_ENTER("WavFile_new")

    struct WavFile *p = (struct WavFile *)malloc_zero(1, sizeof(struct WavFile));

    p->ck_id = WAV_RIFF_CHUNK_ID;
    p->form_type = WAV_RIFF_TYPE_ID;

    p->chunk_count = num_chunks;
    p->chunks = (void **)malloc_zero(num_chunks, sizeof(void *));

    TRACE_LEAVE("WavFile_new");

    return p;
}

struct WavFile *WavFile_load_from_aifc(struct AdpcmAifcFile *aifc_file)
{
    TRACE_ENTER("WavFile_load_from_aifc")

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

    // this should be using ~4:1 compression, overestimate a bit by taking x5.
    size_t buffer_len = aifc_file->sound_chunk->ck_data_size * 5;
    wav->data_chunk->data = (uint8_t *)malloc_zero(1, buffer_len);

    wav->data_chunk->ck_data_size = AdpcmAifcFile_decode(aifc_file, wav->data_chunk->data, buffer_len);

    wav->ck_data_size = 
        4 + /* rest of FORM header */
        WAV_FMT_CHUNK_FULL_SIZE + /* "fmt " chunk is const size */
        8 + wav->data_chunk->ck_data_size; /* "data" chunk header, then data size*/

    TRACE_LEAVE("WavFile_load_from_aifc")

    return wav;
}

void WavDataChunk_frwrite(struct WavDataChunk *chunk, struct file_info *fi)
{
    TRACE_ENTER("WavDataChunk_frwrite")

    file_info_fwrite_bswap(fi, &chunk->ck_id, 4, 1);
    file_info_fwrite(fi, &chunk->ck_data_size, 4, 1);

    if (chunk->ck_data_size > 0)
    {
        file_info_fwrite(fi, chunk->data, (size_t)chunk->ck_data_size, 1);
    }

    TRACE_LEAVE("WavDataChunk_frwrite")
}

void WavFmtChunk_frwrite(struct WavFmtChunk *chunk, struct file_info *fi)
{
    TRACE_ENTER("WavFmtChunk_frwrite")

    file_info_fwrite_bswap(fi, &chunk->ck_id, 4, 1);
    file_info_fwrite(fi, &chunk->ck_data_size, 4, 1);
    file_info_fwrite(fi, &chunk->audio_format, 2, 1);
    file_info_fwrite(fi, &chunk->num_channels, 2, 1);
    file_info_fwrite(fi, &chunk->sample_rate, 4, 1);
    file_info_fwrite(fi, &chunk->byte_rate, 4, 1);
    file_info_fwrite(fi, &chunk->block_align, 2, 1);
    file_info_fwrite(fi, &chunk->bits_per_sample, 2, 1);

    TRACE_LEAVE("WavFmtChunk_frwrite")
}

void WavFile_frwrite(struct WavFile *wav_file, struct file_info *fi)
{
    TRACE_ENTER("WavFile_frwrite")

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
                WavFmtChunk_frwrite(chunk, fi);
            }
            break;

            case WAV_DATA_CHUNK_ID: // data
            {
                struct WavDataChunk *chunk = (struct WavDataChunk *)wav_file->chunks[i];
                WavDataChunk_frwrite(chunk, fi);
            }
            break;

            default:
                // ignore unsupported
            {
                if (g_verbosity >= 2)
                {
                    printf("WavFile_frwrite: ignore ck_id 0x%08x\n", ck_id);
                }
            }
            break;
        }
    }

    TRACE_LEAVE("WavFile_frwrite")
}