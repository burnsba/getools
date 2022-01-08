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

struct WavFile *load_wav_from_aifc(struct AdpcmAifcFile *aifc_file)
{
    TRACE_ENTER("load_wav_from_aifc")

    struct WavFile *wav = WavFile_new(WAV_DEFAULT_NUM_CHUNKS);

    AdpcmAifcFile_decode(aifc_file, wav->data_chunk->data, wav->data_chunk->ck_data_size);

    TRACE_LEAVE("load_wav_from_aifc")

    return wav;
}

void WavDataChunk_frwrite(struct WavDataChunk *chunk, struct file_info *fi)
{
    TRACE_ENTER("WavDataChunk_frwrite")

    file_info_fwrite(fi, &chunk->ck_id, 4, 1);
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

    file_info_fwrite(fi, &chunk->ck_id, 4, 1);
    file_info_fwrite(fi, &chunk->ck_data_size, 4, 1);
    file_info_fwrite(fi, &chunk->audio_format, 2, 1);
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

    file_info_fwrite(fi, &wav_file->ck_id, 4, 1);
    file_info_fwrite(fi, &wav_file->ck_data_size, 4, 1);
    file_info_fwrite(fi, &wav_file->form_type, 4, 1);

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