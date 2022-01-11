#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "machine_config.h"
#include "debug.h"
#include "common.h"
#include "math.h"
#include "utility.h"
#include "adpcm_aifc.h"
#include "llist.h"

/**
 * This file contains primary aifc methods.
 * This contains code to convert from other formats to aifc.
*/

// forward declarations

static uint8_t get_sound_chunk_byte(struct AdpcmAifcFile *aaf, size_t *ssnd_chunk_pos, int *eof);
static void write_frame_output(size_t size, uint8_t *out, int32_t *data);

// end forward declarations

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

struct AdpcmAifcCommChunk *AdpcmAifcCommChunk_new_from_file(struct file_info *fi, int32_t ck_data_size)
{
    TRACE_ENTER("AdpcmAifcCommChunk_new_from_file")

    struct AdpcmAifcCommChunk *p = (struct AdpcmAifcCommChunk *)malloc_zero(1, sizeof(struct AdpcmAifcCommChunk));
    p->ck_id = ADPCM_AIFC_COMMON_CHUNK_ID;
    p->ck_data_size = ck_data_size;

    file_info_fread(fi, &p->num_channels, 2, 1);
    BSWAP16(p->num_channels);

    file_info_fread(fi, &p->num_sample_frames, 4, 1);
    BSWAP32(p->num_sample_frames);

    file_info_fread(fi, &p->sample_size, 2, 1);
    BSWAP16(p->sample_size);

    file_info_fread(fi, &p->sample_rate, 10, 1);
    reverse_inplace(p->sample_rate, 10);

    file_info_fread(fi, &p->compression_type, 4, 1);
    BSWAP16(p->compression_type);
    
    file_info_fread(fi, &p->unknown, 1, 1);

    file_info_fread(fi, &p->compression_name, ADPCM_AIFC_VADPCM_COMPRESSION_NAME_LEN, 1);

    TRACE_LEAVE("AdpcmAifcCommChunk_new_from_file");

    return p;
}

struct AdpcmAifcApplicationChunk *AdpcmAifcApplicationChunk_new_from_file(struct file_info *fi, int32_t ck_data_size)
{
    TRACE_ENTER("AdpcmAifcApplicationChunk_new_from_file")

    if (ck_data_size - (5 + ADPCM_AIFC_VADPCM_APPL_NAME_LEN) <= 0)
    {
        stderr_exit(1, "Invalid APPL chunk data size: %d\n", ck_data_size);
    }

    struct AdpcmAifcApplicationChunk *ret;
    int i;

    uint32_t application_signature;
    uint8_t unknown;
    char code_string[ADPCM_AIFC_VADPCM_APPL_NAME_LEN];

    file_info_fread(fi, &application_signature, 4, 1);
    BSWAP32(application_signature);

    file_info_fread(fi, &unknown, 1, 1);

    file_info_fread(fi, &code_string, ADPCM_AIFC_VADPCM_APPL_NAME_LEN, 1);

    if (strncmp(code_string, ADPCM_AIFC_VADPCM_CODES_NAME, ADPCM_AIFC_VADPCM_APPL_NAME_LEN) == 0)
    {
        struct AdpcmAifcCodebookChunk *p = (struct AdpcmAifcCodebookChunk *)malloc_zero(1, sizeof(struct AdpcmAifcCodebookChunk));
        ret = (struct AdpcmAifcApplicationChunk *)p;
        p->base.ck_id = ADPCM_AIFC_APPLICATION_CHUNK_ID;
        p->base.ck_data_size = ck_data_size;
        p->base.application_signature = application_signature;
        p->base.unknown = unknown;
        
        // no terminating zero
        memcpy(p->base.code_string, ADPCM_AIFC_VADPCM_CODES_NAME, ADPCM_AIFC_VADPCM_APPL_NAME_LEN);

        file_info_fread(fi, &p->version, 2, 1);
        BSWAP16(p->version);

        file_info_fread(fi, &p->order, 2, 1);
        BSWAP16(p->order);

        file_info_fread(fi, &p->nentries, 2, 1);
        BSWAP16(p->nentries);

        size_t table_data_size_bytes = p->order * p->nentries * 16;
        p->table_data = (uint8_t *)malloc_zero(1, table_data_size_bytes);
        file_info_fread(fi, p->table_data, table_data_size_bytes, 1);

        AdpcmAifcCodebookChunk_decode_aifc_codebook(p);
    }
    else if (strncmp(code_string, ADPCM_AIFC_VADPCM_LOOPS_NAME, ADPCM_AIFC_VADPCM_APPL_NAME_LEN) == 0)
    {
        struct AdpcmAifcLoopChunk *p = (struct AdpcmAifcLoopChunk *)malloc_zero(1, sizeof(struct AdpcmAifcLoopChunk));
        ret = (struct AdpcmAifcApplicationChunk *)p;
        p->base.ck_id = ADPCM_AIFC_APPLICATION_CHUNK_ID;
        p->base.ck_data_size = ck_data_size;
        p->base.application_signature = application_signature;
        p->base.unknown = unknown;
        
        // no terminating zero
        memcpy(p->base.code_string, ADPCM_AIFC_VADPCM_LOOPS_NAME, ADPCM_AIFC_VADPCM_APPL_NAME_LEN);

        file_info_fread(fi, &p->version, 2, 1);
        BSWAP16(p->version);

        file_info_fread(fi, &p->nloops, 2, 1);
        BSWAP16(p->nloops);

        size_t loop_data_size_bytes = p->nloops * sizeof(struct AdpcmAifcLoopData);
        p->loop_data = (struct AdpcmAifcLoopData *)malloc_zero(1, loop_data_size_bytes);

        for (i=0; i<p->nloops; i++)
        {
            file_info_fread(fi, &p->loop_data[i].start, 4, 1);
            BSWAP32(p->loop_data[i].start);

            file_info_fread(fi, &p->loop_data[i].end, 4, 1);
            BSWAP32(p->loop_data[i].end);

            file_info_fread(fi, &p->loop_data[i].count, 4, 1);
            BSWAP32(p->loop_data[i].count);

            file_info_fread(fi, p->loop_data[i].state, ADPCM_AIFC_LOOP_STATE_LEN, 1);
        }
    }
    else
    {
        // no terminating zero, requires explicit length
        stderr_exit(1, "Unsupported APPL chunk: %.*s\n", ADPCM_AIFC_VADPCM_APPL_NAME_LEN, code_string);
    }

    TRACE_LEAVE("AdpcmAifcApplicationChunk_new_from_file");

    return ret;
}

struct AdpcmAifcSoundChunk *AdpcmAifcSoundChunk_new_from_file(struct file_info *fi, int32_t ck_data_size)
{
    TRACE_ENTER("AdpcmAifcSoundChunk_new_from_file")

    struct AdpcmAifcSoundChunk *p = (struct AdpcmAifcSoundChunk *)malloc_zero(1, sizeof(struct AdpcmAifcSoundChunk));
    p->ck_id = ADPCM_AIFC_SOUND_CHUNK_ID;
    p->ck_data_size = ck_data_size;

    if (ck_data_size - 8 <= 0)
    {
        stderr_exit(1, "Invalid SSND chunk data size: %d\n", ck_data_size);
    }

    file_info_fread(fi, &p->offset, 4, 1);
    BSWAP32(p->offset);

    file_info_fread(fi, &p->block_size, 4, 1);
    BSWAP32(p->block_size);

    p->sound_data = (uint8_t *)malloc_zero(1, (size_t)(ck_data_size - 8));
    file_info_fread(fi, p->sound_data, (size_t)(ck_data_size - 8), 1);

    TRACE_LEAVE("AdpcmAifcSoundChunk_new_from_file");

    return p;
}

struct AdpcmAifcFile *AdpcmAifcFile_new_from_file(struct file_info *fi)
{
    TRACE_ENTER("AdpcmAifcFile_new_from_file")

    size_t saved_pos;
    size_t pos;
    uint32_t chunk_id;
    int chunk_count;
    int chunk_size;
    int seen_comm = 0;
    int seen_appl = 0;
    int seen_ssnd = 0;

    if (fi->len < 12)
    {
        stderr_exit(1, "Invalid .aifc file: header too short\n");
    }

    file_info_fseek(fi, 0, SEEK_SET);

    struct AdpcmAifcFile *p = (struct AdpcmAifcFile *)malloc_zero(1, sizeof(struct AdpcmAifcFile));

    file_info_fread(fi, &p->ck_id, 4, 1);
    BSWAP32(p->ck_id);

    file_info_fread(fi, &p->ck_data_size, 4, 1);
    BSWAP32(p->ck_data_size);

    file_info_fread(fi, &p->form_type, 4, 1);
    BSWAP32(p->form_type);

    if (p->ck_id != ADPCM_AIFC_FORM_CHUNK_ID)
    {
        stderr_exit(1, "Invalid .aifc file: FORM chunk id failed. Expected 0x%08x, read 0x%08x.\n", ADPCM_AIFC_FORM_CHUNK_ID, p->ck_id);
    }

    if (p->form_type != ADPCM_AIFC_FORM_TYPE_ID)
    {
        stderr_exit(1, "Invalid .aifc file: FORM type id failed. Expected 0x%08x, read 0x%08x.\n", ADPCM_AIFC_FORM_TYPE_ID, p->form_type);
    }

    // do a pass through the file once counting the number of chunks.
    // this will let us finish malloc'ing the FORM chunk.
    saved_pos = ftell(fi->fp);

    pos = saved_pos;
    chunk_count = 0;

    struct llist_root chunk_list;
    memset(&chunk_list, 0, sizeof(struct llist_root));

    while (pos < fi->len)
    {
        if (pos + 8 < fi->len)
        {
            struct llist_node *chunk_node;

            pos += 8;
            chunk_count++;

            file_info_fread(fi, &chunk_id, 4, 1);
            BSWAP32(chunk_id);

            file_info_fread(fi, &chunk_size, 4, 1);
            BSWAP32(chunk_size);

            switch (chunk_id)
            {
                case ADPCM_AIFC_COMMON_CHUNK_ID:
                seen_comm++;
                chunk_node = llist_node_new();
                chunk_node->data = (void *)AdpcmAifcCommChunk_new_from_file(fi, chunk_size);
                llist_root_append_node(&chunk_list, chunk_node);
                break;
                
                case ADPCM_AIFC_APPLICATION_CHUNK_ID:
                seen_appl++;
                chunk_node = llist_node_new();
                chunk_node->data = (void *)AdpcmAifcApplicationChunk_new_from_file(fi, chunk_size);
                if (chunk_node->data != NULL)
                {
                    llist_root_append_node(&chunk_list, chunk_node);
                }
                else
                {
                    llist_node_free(NULL, chunk_node);
                }
                break;
                
                case ADPCM_AIFC_SOUND_CHUNK_ID:
                seen_ssnd++;
                chunk_node = llist_node_new();
                chunk_node->data = (void *)AdpcmAifcSoundChunk_new_from_file(fi, chunk_size);
                llist_root_append_node(&chunk_list, chunk_node);
                break;

                default:
                break;
            }

            pos += chunk_size;
        }
        else
        {
            break;
        }
    }

    if (chunk_count < 3)
    {
        stderr_exit(1, "Invalid .aifc file: needs more chonk\n");
    }

    if (seen_comm == 0)
    {
        stderr_exit(1, "Invalid .aifc file: missing COMM chunk\n");
    }

    if (seen_appl == 0)
    {
        stderr_exit(1, "Invalid .aifc file: missing APPL chunk\n");
    }

    if (seen_ssnd == 0)
    {
        stderr_exit(1, "Invalid .aifc file: missing SSND chunk\n");
    }

    p->chunk_count = chunk_count;
    p->chunks = (void*)malloc_zero(chunk_count, sizeof(void*));

    // Done with FORM header.
    // Now iterate the list and assign pointers.
    struct llist_node *node = chunk_list.root;
    chunk_count = 0;

    // This will overriter the base AdpcmAifcFile convenience pointers if there
    // are duplicate chunks.
    while (node != NULL)
    {
        struct llist_node *next = node->next;

        chunk_id = *(uint32_t *)node->data;
        switch (chunk_id)
        {
            case ADPCM_AIFC_COMMON_CHUNK_ID:
            p->comm_chunk = (struct AdpcmAifcCommChunk*)node->data;
            break;

            case ADPCM_AIFC_SOUND_CHUNK_ID:
            p->sound_chunk = (struct AdpcmAifcSoundChunk*)node->data;
            break;

            case ADPCM_AIFC_APPLICATION_CHUNK_ID:
            {
                struct AdpcmAifcApplicationChunk *appl = (struct AdpcmAifcApplicationChunk *)node->data;
                // code_string doesn't have terminating zero, requires explicit length
                if (strncmp(appl->code_string, ADPCM_AIFC_VADPCM_CODES_NAME, ADPCM_AIFC_VADPCM_APPL_NAME_LEN) == 0)
                {
                    p->codes_chunk = (struct AdpcmAifcCodebookChunk *)node->data;
                }
                else if (strncmp(appl->code_string, ADPCM_AIFC_VADPCM_LOOPS_NAME, ADPCM_AIFC_VADPCM_APPL_NAME_LEN) == 0)
                {
                    p->loop_chunk = (struct AdpcmAifcLoopChunk *)node->data;
                }
            }
            break;
        }

        p->chunks[chunk_count] = node->data;

        llist_node_free(NULL, node);
        node = next;
        chunk_count++;
    }

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
        stderr_exit(1, "Cannot find ALADPCMBook to resolve sound data, sound %s, bank %s\n", sound->text_id, bank->text_id);
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
            stderr_exit(1, "wavetable->len is zero, sound %s, bank %s\n", sound->text_id, bank->text_id);
        }
    }
    else
    {
        stderr_exit(1, "wavetable is NULL, sound %s, bank %s\n", sound->text_id, bank->text_id);
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
    p->ck_data_size = 2 + 4 + 2 + 10 + 4 + 1 + ADPCM_AIFC_VADPCM_APPL_NAME_LEN;
    p->unknown = 0xb;
    p->compression_type = ADPCM_AIFC_COMPRESSION_TYPE_ID;
    
    // no terminating zero
    memcpy(p->compression_name, ADPCM_AIFC_VADPCM_COMPRESSION_NAME, ADPCM_AIFC_VADPCM_APPL_NAME_LEN);
    
    TRACE_LEAVE("AdpcmAifcCommChunk_new");
    
    return p;
}

void AdpcmAifcCodebookChunk_decode_aifc_codebook(struct AdpcmAifcCodebookChunk *chunk)
{
    TRACE_ENTER("AdpcmAifcCodebookChunk_decode_aifc_codebook")

    int i,j,k;
    int code_book_pos = 0;

    chunk->coef_table = malloc_zero(chunk->nentries, sizeof(int32_t**));

    for (i = 0; i < chunk->nentries; i++)
    {
        chunk->coef_table[i] = malloc_zero(8, sizeof(int32_t*));

        for (j = 0; j < 8; j++)
        {
            chunk->coef_table[i][j] = malloc_zero(chunk->order + 8, sizeof(int32_t));
        }
    }

    for (i = 0; i < chunk->nentries; i++)
    {
        int32_t **table_entry = chunk->coef_table[i];

        for (j = 0; j < chunk->order; j++)
        {
            for (k = 0; k < 8; k++)
            {
                // 0x16 is sizeof other stuff in the chunk header
                if (code_book_pos > chunk->base.ck_data_size - 0x16)
                {
                    stderr_exit(1, "AdpcmAifcCodebookChunk_decode_aifc_codebook: attempt to read past end of codebook\n");
                }

                // careful, this needs to pass a signed 16 bit int to bswap, and then sign extend promote to 32 bit.
                int16_t ts = BSWAP16_INLINE(*(int16_t*)(&chunk->table_data[code_book_pos]));
                table_entry[k][j] = (int32_t)ts;
                code_book_pos += 2;
            }
        }

        for (k = 1; k < 8; k++)
        {
            table_entry[k][chunk->order] = table_entry[k - 1][chunk->order - 1];
        }

        table_entry[0][chunk->order] = 2048;

        for (k = 1; k < 8; k++)
        {
            // value of j is carried into second loop
            for (j = 0; j < k; j++)
            {
                table_entry[j][k + chunk->order] = 0;
            }

            for (; j < 8; j++)
            {
                table_entry[j][k + chunk->order] = table_entry[j - k][chunk->order];
            }
        }
    }

    TRACE_LEAVE("AdpcmAifcCodebookChunk_decode_aifc_codebook");
}

struct AdpcmAifcCodebookChunk *AdpcmAifcCodebookChunk_new(int16_t order, uint16_t nentries)
{
    TRACE_ENTER("AdpcmAifcCodebookChunk_new")

    size_t table_data_size_bytes = order * nentries * 16;
    struct AdpcmAifcCodebookChunk *p = (struct AdpcmAifcCodebookChunk *)malloc_zero(1, sizeof(struct AdpcmAifcCodebookChunk));
    p->base.ck_id = ADPCM_AIFC_APPLICATION_CHUNK_ID;
    p->base.ck_data_size = 4 + 1 + ADPCM_AIFC_VADPCM_APPL_NAME_LEN + 2 + 2 + 2 + table_data_size_bytes;
    p->base.application_signature = ADPCM_AIFC_APPLICATION_SIGNATURE;
    p->base.unknown = 0xb;
    
    // no terminating zero
    memcpy(p->base.code_string, ADPCM_AIFC_VADPCM_CODES_NAME, ADPCM_AIFC_VADPCM_APPL_NAME_LEN);

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
    p->base.ck_data_size = 4 + 1 + ADPCM_AIFC_VADPCM_APPL_NAME_LEN + 2 + 2 + loop_data_size_bytes;
    p->base.application_signature = ADPCM_AIFC_APPLICATION_SIGNATURE;
    p->base.unknown = 0xb;
    
    // no terminating zero
    memcpy(p->base.code_string, ADPCM_AIFC_VADPCM_LOOPS_NAME, ADPCM_AIFC_VADPCM_APPL_NAME_LEN);

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

        AdpcmAifcCodebookChunk_decode_aifc_codebook(aaf->codes_chunk);

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
    file_info_fwrite_bswap(fi, chunk->compression_name, ADPCM_AIFC_VADPCM_COMPRESSION_NAME_LEN, 1);
    
    TRACE_LEAVE("AdpcmAifcCommChunk_frwrite");
}

void AdpcmAifcApplicationChunk_frwrite(struct AdpcmAifcApplicationChunk *chunk, struct file_info *fi)
{
    TRACE_ENTER("AdpcmAifcApplicationChunk_frwrite")

    file_info_fwrite_bswap(fi, &chunk->ck_id, 4, 1);
    file_info_fwrite_bswap(fi, &chunk->ck_data_size, 4, 1);
    file_info_fwrite_bswap(fi, &chunk->application_signature, 4, 1);
    file_info_fwrite_bswap(fi, &chunk->unknown, 1, 1);
    file_info_fwrite_bswap(fi, chunk->code_string, ADPCM_AIFC_VADPCM_APPL_NAME_LEN, 1);
    
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
                // code_string doesn't have terminating zero, requires explicit length
                if (strncmp(basechunk->code_string, ADPCM_AIFC_VADPCM_CODES_NAME, ADPCM_AIFC_VADPCM_APPL_NAME_LEN) == 0)
                {
                    struct AdpcmAifcCodebookChunk *chunk = (struct AdpcmAifcCodebookChunk *)basechunk;
                    AdpcmAifcCodebookChunk_frwrite(chunk, fi);
                }
                else if (strncmp(basechunk->code_string, ADPCM_AIFC_VADPCM_LOOPS_NAME, ADPCM_AIFC_VADPCM_APPL_NAME_LEN) == 0)
                {
                    struct AdpcmAifcLoopChunk *chunk = (struct AdpcmAifcLoopChunk *)basechunk;
                    AdpcmAifcLoopChunk_frwrite(chunk, fi);
                }
                // else, ignore unsupported
                else
                {
                    if (g_verbosity >= 2)
                    {
                        // no terminating zero, requires explicit length
                        printf("AdpcmAifcFile_frwrite: APPL ignore code_string '%.*s'\n", ADPCM_AIFC_VADPCM_APPL_NAME_LEN, basechunk->code_string);
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

void AdpcmAifcFile_decode_frame(struct AdpcmAifcFile *aaf, int32_t *frame_buffer, size_t *ssnd_chunk_pos, int *end_of_ssnd)
{
    TRACE_ENTER("AdpcmAifcFile_decode_frame");

    int32_t optimalp;
    int32_t scale;
    int32_t max_level;
    
    int32_t scaled_frame[FRAME_DECODE_BUFFER_LEN];
    int32_t convl_frame[FRAME_DECODE_BUFFER_LEN];

    memset(scaled_frame, 0, FRAME_DECODE_BUFFER_LEN * sizeof(int32_t));
    memset(convl_frame, 0, FRAME_DECODE_BUFFER_LEN * sizeof(int32_t));
    
    int i,j;

    uint8_t frame_header;
    uint8_t c;

    max_level = 7;
    frame_header = get_sound_chunk_byte(aaf, ssnd_chunk_pos, end_of_ssnd);
    scale = 1 << (frame_header >> 4);
    optimalp = frame_header & 0xf;

    for (i = 0; i < FRAME_DECODE_BUFFER_LEN; i += 2)
    {
        c = get_sound_chunk_byte(aaf, ssnd_chunk_pos, end_of_ssnd);

        scaled_frame[i] = c >> 4;
        scaled_frame[i + 1] = c & 0xf;

        for (j=0; j<2; j++)
        {
            if (scaled_frame[i + j] <= max_level)
            {
                scaled_frame[i + j] *= scale;
            }
            else
            {
                scaled_frame[i + j] = (-0x10 - -scaled_frame[i + j]) * scale;
            }
        }
    }

    for (j = 0; j < 2; j++)
    {
        for (i = 0; i < 8; i++)
        {
            convl_frame[i + aaf->codes_chunk->order] = scaled_frame[j * 8 + i];
        }

        if (j == 0)
        {
            for (i = 0; i < aaf->codes_chunk->order; i++)
            {
                convl_frame[i] = frame_buffer[16 - aaf->codes_chunk->order + i];
            }
        }
        else
        {
            for (i = 0; i < aaf->codes_chunk->order; i++)
            {
                convl_frame[i] = frame_buffer[j * 8 - aaf->codes_chunk->order + i];
            }
        }

        for (i = 0; i < 8; i++)
        {
            frame_buffer[i + j * 8] = dot_product_i32(aaf->codes_chunk->coef_table[optimalp][i], convl_frame, aaf->codes_chunk->order + 8);
            frame_buffer[i + j * 8] = divide_round_down(frame_buffer[i + j * 8], 2048);
        }
    }

    TRACE_LEAVE("AdpcmAifcFile_decode_frame");
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
    size_t ssnd_chunk_pos = 0;
    int end_of_ssnd = 0;
    size_t buffer_pos = 0;

    int32_t ssnd_data_size = aaf->sound_chunk->ck_data_size - 8;
    if (ssnd_data_size < 0)
    {
        ssnd_data_size = 0;
    }

    int32_t *frame_buffer = (int32_t *)malloc_zero(FRAME_DECODE_BUFFER_LEN, sizeof(int32_t));

    if (aaf->loop_chunk == NULL)
    {
        while (end_of_ssnd == 0 && ssnd_chunk_pos < (size_t)(ssnd_data_size) && write_len < max_len)
        {
            AdpcmAifcFile_decode_frame(aaf, frame_buffer, &ssnd_chunk_pos, &end_of_ssnd);
            write_frame_output(16, &buffer[buffer_pos], frame_buffer);
            write_len += 16 * 2;
            buffer_pos += 16 * 2;
        }
    }
    else
    {
        stderr_exit(1, "AdpcmAifcFile_decode: loop not supported\n");
    }

    free(frame_buffer);

    TRACE_LEAVE("AdpcmAifcFile_decode");

    return write_len;
}

int32_t AdpcmAifcFile_get_int_sample_rate(struct AdpcmAifcFile *aaf)
{
    if (aaf == NULL)
    {
        return -1;
    }

    if (aaf->comm_chunk == NULL)
    {
        return -1;
    }

    f80 float_sample_rate;
    memcpy(&float_sample_rate, &aaf->comm_chunk->sample_rate, 10);

    return (int32_t)float_sample_rate;
}

static uint8_t get_sound_chunk_byte(struct AdpcmAifcFile *aaf, size_t *ssnd_chunk_pos, int *eof)
{
    if (*ssnd_chunk_pos > (size_t)(aaf->sound_chunk->ck_data_size - 8))
    {
        *eof = 1;
        return 0;
    }

    *eof = 0;
    uint8_t ret = aaf->sound_chunk->sound_data[*ssnd_chunk_pos];
    *ssnd_chunk_pos = *ssnd_chunk_pos + 1;
    return ret;
}

static void write_frame_output(size_t size, uint8_t *out, int32_t *data)
{
    size_t i;
    for (i=0; i<size; i++)
    {
        int16_t val = (int16_t)clamp(data[i], -0x7fff, 0x7fff);

        ((int16_t *)out)[i] = val;
    }
}