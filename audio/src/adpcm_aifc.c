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
#include "naudio.h"
#include "llist.h"

/**
 * This file contains primary aifc methods.
 * This contains code to convert from other formats to aifc.
*/

/**
 * When converting .aifc, this specifies the number of times an ADPCM Loop
 * with repeat=infinite should be repeated in the export.
*/
int g_AdpcmLoopInfiniteExportCount = 0;

// forward declarations

static uint8_t get_sound_chunk_byte(struct AdpcmAifcFile *aaf, size_t *ssnd_chunk_pos, int *eof);
static void write_frame_output(uint8_t *out, int32_t *data, size_t size);

// end forward declarations

/**
 * Default constructor for {@code struct AdpcmAifcFile}, only allocates memory
 * for itself and {@code chunks} list.
 * @returns: pointer to new {@code struct AdpcmAifcFile}.
*/
struct AdpcmAifcFile *AdpcmAifcFile_new_simple(size_t chunk_count)
{
    TRACE_ENTER(__func__)

    struct AdpcmAifcFile *p = (struct AdpcmAifcFile *)malloc_zero(1, sizeof(struct AdpcmAifcFile));
    p->ck_id = ADPCM_AIFC_FORM_CHUNK_ID;
    p->form_type = ADPCM_AIFC_FORM_TYPE_ID;

    p->chunk_count = chunk_count;
    p->chunks = (void*)malloc_zero(chunk_count, sizeof(void*));

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Creates new {@code struct AdpcmAifcCommChunk} from aifc file contents.
 * @param fi: aifc file. Reads from current seek position.
 * @param ck_data_size: chunk size in bytes.
 * @returns: pointer to new common chunk.
*/
struct AdpcmAifcCommChunk *AdpcmAifcCommChunk_new_from_file(struct file_info *fi, int32_t ck_data_size)
{
    TRACE_ENTER(__func__)

    int remaining_bytes;

    struct AdpcmAifcCommChunk *p = (struct AdpcmAifcCommChunk *)malloc_zero(1, sizeof(struct AdpcmAifcCommChunk));
    p->ck_id = ADPCM_AIFC_COMMON_CHUNK_ID;
    p->ck_data_size = ck_data_size;
    remaining_bytes = ck_data_size;

    file_info_fread(fi, &p->num_channels, 2, 1);
    BSWAP16(p->num_channels);
    remaining_bytes -= 2;

    file_info_fread(fi, &p->num_sample_frames, 4, 1);
    BSWAP32(p->num_sample_frames);
    remaining_bytes -= 4;

    file_info_fread(fi, &p->sample_size, 2, 1);
    BSWAP16(p->sample_size);
    remaining_bytes -= 2;

    file_info_fread(fi, &p->sample_rate, 10, 1);
    reverse_inplace(p->sample_rate, 10);
    remaining_bytes -= 10;

    file_info_fread(fi, &p->compression_type, 4, 1);
    BSWAP32(p->compression_type);
    remaining_bytes -= 4;
    
    file_info_fread(fi, &p->unknown, 1, 1);
    remaining_bytes -= 1;

    if (remaining_bytes > 0)
    {
        if (remaining_bytes > ADPCM_AIFC_COMPRESSION_NAME_ARR_LEN)
        {
            remaining_bytes = ADPCM_AIFC_COMPRESSION_NAME_ARR_LEN;
        }

        file_info_fread(fi, &p->compression_name, remaining_bytes, 1);
    }

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Creates new application chunk from aifc file contents.
 * Must be codebook or loop chunk, otherwise program exits with error.
 * @param fi: aifc file. Reads from current seek position.
 * @param ck_data_size: chunk size in bytes.
 * @returns: pointer to new application chunk.
*/
struct AdpcmAifcApplicationChunk *AdpcmAifcApplicationChunk_new_from_file(struct file_info *fi, int32_t ck_data_size)
{
    TRACE_ENTER(__func__)

    if (ck_data_size - (5 + ADPCM_AIFC_VADPCM_APPL_NAME_LEN) <= 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "Invalid APPL chunk data size: %d\n", ck_data_size);
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
        p->version = ADPCM_AIFC_VADPCM_LOOP_VERSION;
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
        stderr_exit(EXIT_CODE_GENERAL, "Unsupported APPL chunk: %.*s\n", ADPCM_AIFC_VADPCM_APPL_NAME_LEN, code_string);
    }

    TRACE_LEAVE(__func__)

    return ret;
}

/**
 * Creates new {@code struct AdpcmAifcSoundChunk} from aifc file contents.
 * @param fi: aifc file. Reads from current seek position.
 * @param ck_data_size: chunk size in bytes.
 * @returns: pointer to new sound chunk.
*/
struct AdpcmAifcSoundChunk *AdpcmAifcSoundChunk_new_from_file(struct file_info *fi, int32_t ck_data_size)
{
    TRACE_ENTER(__func__)

    struct AdpcmAifcSoundChunk *p = (struct AdpcmAifcSoundChunk *)malloc_zero(1, sizeof(struct AdpcmAifcSoundChunk));
    p->ck_id = ADPCM_AIFC_SOUND_CHUNK_ID;
    p->ck_data_size = ck_data_size;

    if (ck_data_size - 8 <= 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "Invalid SSND chunk data size: %d\n", ck_data_size);
    }

    file_info_fread(fi, &p->offset, 4, 1);
    BSWAP32(p->offset);

    file_info_fread(fi, &p->block_size, 4, 1);
    BSWAP32(p->block_size);

    p->sound_data = (uint8_t *)malloc_zero(1, (size_t)(ck_data_size - 8));
    file_info_fread(fi, p->sound_data, (size_t)(ck_data_size - 8), 1);

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Seeks to beginning of file and parses as {@code struct AdpcmAifcFile}.
 * @param fi: aifc file.
 * @returns: pointer to new {@code struct AdpcmAifcFile}.
*/
struct AdpcmAifcFile *AdpcmAifcFile_new_from_file(struct file_info *fi)
{
    TRACE_ENTER(__func__)

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
        stderr_exit(EXIT_CODE_GENERAL, "Invalid .aifc file: header too short\n");
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
        stderr_exit(EXIT_CODE_GENERAL, "Invalid .aifc file: FORM chunk id failed. Expected 0x%08x, read 0x%08x.\n", ADPCM_AIFC_FORM_CHUNK_ID, p->ck_id);
    }

    if (p->form_type != ADPCM_AIFC_FORM_TYPE_ID)
    {
        stderr_exit(EXIT_CODE_GENERAL, "Invalid .aifc file: FORM type id failed. Expected 0x%08x, read 0x%08x.\n", ADPCM_AIFC_FORM_TYPE_ID, p->form_type);
    }

    // do a pass through the file once counting the number of chunks.
    // this will let us finish malloc'ing the FORM chunk.
    // As the file is scanned, supported chunks will be parsed and added to a list.
    // Once the main aifc container is allocated the allocated chunks will
    // be added to the aifc container chunk list.
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
                // ignore unsupported chunks
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
        stderr_exit(EXIT_CODE_GENERAL, "Invalid .aifc file: needs more chonk\n");
    }

    if (seen_comm == 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "Invalid .aifc file: missing COMM chunk\n");
    }

    if (seen_appl == 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "Invalid .aifc file: missing APPL chunk\n");
    }

    if (seen_ssnd == 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "Invalid .aifc file: missing SSND chunk\n");
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

    TRACE_LEAVE(__func__)

    return p;
}

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
 * Allocates memory for a new {@code struct AdpcmAifcCommChunk} and sets default values.
 * @param compression_type: Should be big endian 32-bit id of compression type used.
 * @returns: pointer to new {@code struct AdpcmAifcCommChunk}.
*/
struct AdpcmAifcCommChunk *AdpcmAifcCommChunk_new(uint32_t compression_type)
{
    TRACE_ENTER(__func__)

    // everything except the name string.
    const int base_chunk_size = 2 + 4 + 2 + 10 + 4 + 1;

    struct AdpcmAifcCommChunk *p = (struct AdpcmAifcCommChunk *)malloc_zero(1, sizeof(struct AdpcmAifcCommChunk));
    p->ck_id = ADPCM_AIFC_COMMON_CHUNK_ID;
    p->num_channels = DEFAULT_NUM_CHANNELS;
    p->unknown = 0xb;

    switch (compression_type)
    {
        case ADPCM_AIFC_VAPC_COMPRESSION_TYPE_ID:
        {
            p->ck_data_size = base_chunk_size + ADPCM_AIFC_VADPCM_APPL_NAME_LEN;
            p->compression_type = ADPCM_AIFC_VAPC_COMPRESSION_TYPE_ID;
            
            // no terminating zero
            memcpy(p->compression_name, ADPCM_AIFC_VADPCM_COMPRESSION_NAME, ADPCM_AIFC_VADPCM_APPL_NAME_LEN);
        }
        break;

        case ADPCM_AIFC_NONE_COMPRESSION_TYPE_ID:
        {
            p->ck_data_size = base_chunk_size + ADPCM_AIFC_NONE_COMPRESSION_NAME_LEN;
            p->compression_type = ADPCM_AIFC_NONE_COMPRESSION_TYPE_ID;
            
            // no terminating zero
            memcpy(p->compression_name, ADPCM_AIFC_NONE_COMPRESSION_NAME, ADPCM_AIFC_NONE_COMPRESSION_NAME_LEN);
        }
        break;
        
        default:
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> unsupported compression type 0x%08x\n", __func__, __LINE__, compression_type);
        }
        break;
    }
    
    TRACE_LEAVE(__func__)
    
    return p;
}

/**
 * Parses {@code struct AdpcmAifcCodebookChunk.table_data} and converts to aifc coefficient table.
 * This allocates memory and stores the results in {@code struct AdpcmAifcCodebookChunk.coef_table}.@
 * @param chunk: codebook chunk to decode.
*/
void AdpcmAifcCodebookChunk_decode_aifc_codebook(struct AdpcmAifcCodebookChunk *chunk)
{
    TRACE_ENTER(__func__)

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
                    stderr_exit(EXIT_CODE_GENERAL, "AdpcmAifcCodebookChunk_decode_aifc_codebook: attempt to read past end of codebook\n");
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

    TRACE_LEAVE(__func__)
}

/**
 * Allocates memory for a new {@code struct AdpcmAifcCodebookChunk} and sets default values.
 * @param order: codebook order
 * @param nentries: codebook nentries (aka npredictors)
 * @returns: pointer to new {@code struct AdpcmAifcCodebookChunk}.
*/
struct AdpcmAifcCodebookChunk *AdpcmAifcCodebookChunk_new(int16_t order, uint16_t nentries)
{
    TRACE_ENTER(__func__)

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

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Allocates memory for a new {@code struct AdpcmAifcSoundChunk} and sets default values.
 * @param sound_data_size_bytes: size in bytes to allocate for the sound data.
 * @returns: pointer to new {@code struct AdpcmAifcSoundChunk}.
*/
struct AdpcmAifcSoundChunk *AdpcmAifcSoundChunk_new(size_t sound_data_size_bytes)
{
    TRACE_ENTER(__func__)

    struct AdpcmAifcSoundChunk *p = (struct AdpcmAifcSoundChunk *)malloc_zero(1, sizeof(struct AdpcmAifcSoundChunk));
    p->ck_id = ADPCM_AIFC_SOUND_CHUNK_ID;
    p->ck_data_size = 4 + 4 + sound_data_size_bytes;

    p->sound_data = (uint8_t *)malloc_zero(1, sound_data_size_bytes);

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Allocates memory for a new {@code struct AdpcmAifcLoopChunk} and sets default values.
 * @returns: pointer to new {@code struct AdpcmAifcLoopChunk}.
*/
struct AdpcmAifcLoopChunk *AdpcmAifcLoopChunk_new()
{
    TRACE_ENTER(__func__)

    // nloops * sizeof(struct AdpcmAifcLoopData)
    size_t loop_data_size_bytes = 4 + 4 + 4 + 0x20;
    struct AdpcmAifcLoopChunk *p = (struct AdpcmAifcLoopChunk *)malloc_zero(1, sizeof(struct AdpcmAifcLoopChunk));
    p->base.ck_id = ADPCM_AIFC_APPLICATION_CHUNK_ID;
    p->base.ck_data_size = 4 + 1 + ADPCM_AIFC_VADPCM_APPL_NAME_LEN + 2 + 2 + loop_data_size_bytes;
    p->version = ADPCM_AIFC_VADPCM_LOOP_VERSION;
    p->base.application_signature = ADPCM_AIFC_APPLICATION_SIGNATURE;
    p->base.unknown = 0xb;
    
    // no terminating zero
    memcpy(p->base.code_string, ADPCM_AIFC_VADPCM_LOOPS_NAME, ADPCM_AIFC_VADPCM_APPL_NAME_LEN);

    p->nloops = 1;

    p->loop_data = (struct AdpcmAifcLoopData *)malloc_zero(1, loop_data_size_bytes);

    TRACE_LEAVE(__func__)

    return p;
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
 * Write {@code struct AdpcmAifcCommChunk} to disk.
 * Calls write for all relevant child objects.
 * @param chunk: object to write.
 * @param fi: file_info to write to. Uses current seek position.
*/
void AdpcmAifcCommChunk_fwrite(struct AdpcmAifcCommChunk *chunk, struct file_info *fi)
{
    TRACE_ENTER(__func__)

    int remaining_bytes = chunk->ck_data_size;

    file_info_fwrite_bswap(fi, &chunk->ck_id, 4, 1);
    file_info_fwrite_bswap(fi, &chunk->ck_data_size, 4, 1);

    file_info_fwrite_bswap(fi, &chunk->num_channels, 2, 1);
    remaining_bytes -= 2;

    file_info_fwrite_bswap(fi, &chunk->num_sample_frames, 4, 1);
    remaining_bytes -= 4;

    file_info_fwrite_bswap(fi, &chunk->sample_size, 2, 1);
    remaining_bytes -= 2;

    file_info_fwrite_bswap(fi, chunk->sample_rate, 10, 1);
    remaining_bytes -= 10;

    file_info_fwrite_bswap(fi, &chunk->compression_type, 4, 1);
    remaining_bytes -= 4;

    file_info_fwrite_bswap(fi, &chunk->unknown, 1, 1);
    remaining_bytes -= 1;

    if (remaining_bytes > 0)
    {
        if (remaining_bytes > ADPCM_AIFC_COMPRESSION_NAME_ARR_LEN)
        {
            remaining_bytes = ADPCM_AIFC_COMPRESSION_NAME_ARR_LEN;
        }

        file_info_fwrite_bswap(fi, chunk->compression_name, remaining_bytes, 1);
    }
    
    TRACE_LEAVE(__func__)
}

/**
 * Write {@code struct AdpcmAifcApplicationChunk} to disk.
 * Calls write for all relevant child objects.
 * @param chunk: object to write.
 * @param fi: file_info to write to. Uses current seek position.
*/
void AdpcmAifcApplicationChunk_fwrite(struct AdpcmAifcApplicationChunk *chunk, struct file_info *fi)
{
    TRACE_ENTER(__func__)

    file_info_fwrite_bswap(fi, &chunk->ck_id, 4, 1);
    file_info_fwrite_bswap(fi, &chunk->ck_data_size, 4, 1);
    file_info_fwrite_bswap(fi, &chunk->application_signature, 4, 1);
    file_info_fwrite_bswap(fi, &chunk->unknown, 1, 1);
    file_info_fwrite_bswap(fi, chunk->code_string, ADPCM_AIFC_VADPCM_APPL_NAME_LEN, 1);
    
    TRACE_LEAVE(__func__)
}

/**
 * Write {@code struct AdpcmAifcCodebookChunk} to disk.
 * Calls write for all relevant child objects.
 * @param chunk: object to write.
 * @param fi: file_info to write to. Uses current seek position.
*/
void AdpcmAifcCodebookChunk_fwrite(struct AdpcmAifcCodebookChunk *chunk, struct file_info *fi)
{
    TRACE_ENTER(__func__)

    size_t table_size;

    AdpcmAifcApplicationChunk_fwrite(&chunk->base, fi);

    file_info_fwrite_bswap(fi, &chunk->version, 2, 1);
    file_info_fwrite_bswap(fi, &chunk->order, 2, 1);
    file_info_fwrite_bswap(fi, &chunk->nentries, 2, 1);

    table_size = chunk->order * chunk->nentries * 16;

    file_info_fwrite_bswap(fi, chunk->table_data, table_size, 1);

    TRACE_LEAVE(__func__)
}

/**
 * Write {@code struct AdpcmAifcSoundChunk} to disk.
 * Calls write for all relevant child objects.
 * @param chunk: object to write.
 * @param fi: file_info to write to. Uses current seek position.
*/
void AdpcmAifcSoundChunk_fwrite(struct AdpcmAifcSoundChunk *chunk, struct file_info *fi)
{
    TRACE_ENTER(__func__)

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

    TRACE_LEAVE(__func__)
}

/**
 * Write {@code struct AdpcmAifcLoopData} to disk.
 * Calls write for all relevant child objects.
 * @param chunk: object to write.
 * @param fi: file_info to write to. Uses current seek position.
*/
void AdpcmAifcLoopData_fwrite(struct AdpcmAifcLoopData *loop, struct file_info *fi)
{
    TRACE_ENTER(__func__)

    file_info_fwrite_bswap(fi, &loop->start, 4, 1);
    file_info_fwrite_bswap(fi, &loop->end, 4, 1);
    file_info_fwrite_bswap(fi, &loop->count, 4, 1);
    file_info_fwrite_bswap(fi, &loop->state, 0x20, 1);

    TRACE_LEAVE(__func__)
}

/**
 * Write {@code struct AdpcmAifcLoopChunk} to disk.
 * Calls write for all relevant child objects.
 * @param chunk: object to write.
 * @param fi: file_info to write to. Uses current seek position.
*/
void AdpcmAifcLoopChunk_fwrite(struct AdpcmAifcLoopChunk *chunk, struct file_info *fi)
{
    TRACE_ENTER(__func__)

    int i;

    AdpcmAifcApplicationChunk_fwrite(&chunk->base, fi);

    file_info_fwrite_bswap(fi, &chunk->version, 2, 1);
    file_info_fwrite_bswap(fi, &chunk->nloops, 2, 1);

    for (i=0; i<chunk->nloops; i++)
    {
        AdpcmAifcLoopData_fwrite(&chunk->loop_data[i], fi);
    }

    TRACE_LEAVE(__func__)
}

/**
 * Write {@code struct AdpcmAifcFile} to disk.
 * Calls write for all relevant child objects.
 * @param aaf: .aifc in-memory file to write.
 * @param fi: file_info to write to. Uses current seek position.
*/
void AdpcmAifcFile_fwrite(struct AdpcmAifcFile *aaf, struct file_info *fi)
{
    TRACE_ENTER(__func__)

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
                AdpcmAifcCommChunk_fwrite(chunk, fi);
            }
            break;

            case ADPCM_AIFC_SOUND_CHUNK_ID: // SSND
            {
                struct AdpcmAifcSoundChunk *chunk = (struct AdpcmAifcSoundChunk *)aaf->chunks[i];
                AdpcmAifcSoundChunk_fwrite(chunk, fi);
            }
            break;

            case ADPCM_AIFC_APPLICATION_CHUNK_ID: // APPL
            {
                struct AdpcmAifcApplicationChunk *basechunk = (struct AdpcmAifcApplicationChunk *)aaf->chunks[i];
                // code_string doesn't have terminating zero, requires explicit length
                if (strncmp(basechunk->code_string, ADPCM_AIFC_VADPCM_CODES_NAME, ADPCM_AIFC_VADPCM_APPL_NAME_LEN) == 0)
                {
                    struct AdpcmAifcCodebookChunk *chunk = (struct AdpcmAifcCodebookChunk *)basechunk;
                    AdpcmAifcCodebookChunk_fwrite(chunk, fi);
                }
                else if (strncmp(basechunk->code_string, ADPCM_AIFC_VADPCM_LOOPS_NAME, ADPCM_AIFC_VADPCM_APPL_NAME_LEN) == 0)
                {
                    struct AdpcmAifcLoopChunk *chunk = (struct AdpcmAifcLoopChunk *)basechunk;
                    AdpcmAifcLoopChunk_fwrite(chunk, fi);
                }
                // else, ignore unsupported
                else
                {
                    if (g_verbosity >= 2)
                    {
                        // no terminating zero, requires explicit length
                        printf("AdpcmAifcFile_fwrite: APPL ignore code_string '%.*s'\n", ADPCM_AIFC_VADPCM_APPL_NAME_LEN, basechunk->code_string);
                    }
                }
            }
            break;

            default:
                // ignore unsupported
            {
                if (g_verbosity >= 2)
                {
                    printf("AdpcmAifcFile_fwrite: ignore ck_id 0x%08x\n", ck_id);
                }
            }
            break;
        }
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
 * Applies the standard .aifc decode algorithm from the sound chunk and writes
 * result to the frame buffer.
 * @param aaf: container file.
 * @param frame_buffer: standard frame buffer.
 * @param ssnd_chunk_pos: in/out paramter. Current byte position within the sound chunk. If not
 * {@code eof} then will be set to next byte position.
 * @param end_of_ssnd: out parameter. If {@code ssnd_chunk_pos} is less than the size of the sound data
 * in the sound chunk this is set to 1. Otherwise set to zero.
*/
void AdpcmAifcFile_decode_frame(struct AdpcmAifcFile *aaf, int32_t *frame_buffer, size_t *ssnd_chunk_pos, int *end_of_ssnd)
{
    TRACE_ENTER(__func__)

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

    TRACE_LEAVE(__func__)
}

/**
 * Decode .aifc audio and write to output buffer.
 * If the audio is compressed, this is the top level entry into decompressing it.
 * @param aaf: input source
 * @param buffer: output buffer. Must be previously allocated.
 * @param max_len: max number of bytes to write to output buffer.
 * @returns: number of bytes written
*/
size_t AdpcmAifcFile_decode(struct AdpcmAifcFile *aaf, uint8_t *buffer, size_t max_len)
{
    TRACE_ENTER(__func__)

    size_t write_len = 0;
    size_t ssnd_chunk_pos = 0;
    int end_of_ssnd = 0;
    int i;
    int no_loop_chunk;

    // determine max size in bytes to read from input .aifc
    int32_t ssnd_data_size = aaf->sound_chunk->ck_data_size - 8;
    if (ssnd_data_size < 0)
    {
        ssnd_data_size = 0;
    }

    if ((size_t)ssnd_data_size > max_len)
    {
        ssnd_data_size = (int32_t)max_len;
    }

    if (aaf == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: aaf is NULL\n", __func__);
    }

    if (aaf->comm_chunk == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: aaf->comm_chunk is NULL\n", __func__);
    }

    no_loop_chunk = aaf->loop_chunk == NULL
        // there is a loop chunk, but nloops is zero
        || (aaf->loop_chunk != NULL && aaf->loop_chunk->nloops == 0)
        // there is a loop, but it's an infinite loop, and user option is to ignore
        || (aaf->loop_chunk != NULL && aaf->loop_chunk->loop_data != NULL && aaf->loop_chunk->loop_data->count == -1 && g_AdpcmLoopInfiniteExportCount == 0);

    /**
     * If this is uncompressed audio then there's no codebook.
    */
    if (aaf->comm_chunk->compression_type == ADPCM_AIFC_NONE_COMPRESSION_TYPE_ID)
    {
        if (ssnd_data_size & 0x1)
        {
            printf("warning, ssnd_data_size is odd, truncating last byte (required for bswap)\n");
            ssnd_data_size--;
        }

        if (no_loop_chunk)
        {
            bswap16_memcpy(buffer, aaf->sound_chunk->sound_data, ssnd_data_size);

            // last debug statement, not protected by DEBUG_ADPCMAIFCFILE_DECODE
            if (g_verbosity >= VERBOSE_DEBUG)
            {
                printf("%s %d: write_len=%d\n", __func__, __LINE__, ssnd_data_size);
            }

            TRACE_LEAVE(__func__)
            return ssnd_data_size;
        }
        else
        {
            /**
             * (slightly different from AL_ADPCM_WAVE)
             * Otherwise there are loops. Well, only one loop since the N64 only supports one loop.
             * The logic here is roughly:
             * - copy samples up to loop start
             * - copy loop data for the specified number of times
             * - copy any remaining samples after loop end marker
            */

            struct AdpcmAifcLoopData *loop_data;

            // byte offset into ssnd chunk
            size_t loop_start_position;

            // byte offset into ssnd chunk
            size_t loop_end_position;

            size_t loop_size_bytes;

            size_t after_loop_bytes;

            // number of times to decode loop data
            int loop_times;

            if (aaf->loop_chunk->nloops != 1)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s: N64 only supports single loop, aaf->loop_chunk->nloops=%d\n", __func__, aaf->loop_chunk->nloops);
            }

            loop_data = &aaf->loop_chunk->loop_data[0];

            if (loop_data == NULL)
            {
                stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: loop_data pointer is NULL\n", __func__);
            }

            if (loop_data->start < 0)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s: invalid loop_data->start offset: %d\n", __func__, loop_data->start);
            }

            if (loop_data->end < 0)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s: invalid loop_data->end offset: %d\n", __func__, loop_data->end);
            }

            // `start` is the starting sample, samples are 16 bits, so multiply by 2 to get offset into buffer.
            loop_start_position = loop_data->start * 2;

            if (loop_start_position > (size_t)ssnd_data_size)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s: loop start offset %ld is after end of ssnd data offset %d\n", __func__, loop_start_position, ssnd_data_size);
            }

            // same as above
            loop_end_position = loop_data->end * 2;

            if (loop_end_position > (size_t)ssnd_data_size)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s: loop end offset %ld is after end of ssnd data offset %d\n", __func__, loop_end_position, ssnd_data_size);
            }

            if (loop_start_position > loop_end_position)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s: loop end offset %ld is before loop start offset %d\n", __func__, loop_end_position, loop_start_position);
            }

            // checks above should ensure this is valid.
            loop_size_bytes = loop_end_position - loop_start_position;
            after_loop_bytes = ssnd_data_size - loop_end_position;

            if (DEBUG_ADPCMAIFCFILE_DECODE && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("%s %d: loop_start_position=0x%08lx, loop_end_position=0x%08lx\n", __func__, __LINE__, loop_start_position, loop_end_position);
            }

            // copy samples up to one (sample) less than the start loop position
            if (loop_start_position > 2)
            {
                bswap16_memcpy(&buffer[write_len], &aaf->sound_chunk->sound_data[0], loop_start_position - 2);
                write_len += loop_start_position - 2;
                ssnd_chunk_pos += loop_start_position - 2;
            }

            // Determine number of loops to decode. If this is an infinite loop then
            // fallback to user specification.
            loop_times = loop_data->count;
            if (loop_times == -1)
            {
                loop_times = g_AdpcmLoopInfiniteExportCount;
            }

            if (DEBUG_ADPCMAIFCFILE_DECODE && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("%s %d: loop_times=%d\n", __func__, __LINE__, loop_times);
            }

            /**
             * ADPCM mode copies up to the loop end point before considering looping, but this only
             * copies up to the start, so use `<=` here.
            */
            for (i=0; i<=loop_times; i++)
            {
                ssnd_chunk_pos = loop_start_position;
                bswap16_memcpy(&buffer[write_len], &aaf->sound_chunk->sound_data[ssnd_chunk_pos], loop_size_bytes);
                write_len += loop_size_bytes;
                ssnd_chunk_pos += loop_size_bytes;
            }

            if (after_loop_bytes > 0)
            {
                bswap16_memcpy(&buffer[write_len], &aaf->sound_chunk->sound_data[ssnd_chunk_pos], after_loop_bytes);
                write_len += after_loop_bytes;
                ssnd_chunk_pos += after_loop_bytes;
            }

            // last debug statement, not protected by DEBUG_ADPCMAIFCFILE_DECODE
            if (g_verbosity >= VERBOSE_DEBUG)
            {
                printf("%s %d: write_len=%ld\n", __func__, __LINE__, write_len);
            }

            TRACE_LEAVE(__func__)
            return write_len;
        }
    }
    else if (aaf->comm_chunk->compression_type == ADPCM_AIFC_VAPC_COMPRESSION_TYPE_ID)
    {
        int32_t *frame_buffer = (int32_t *)malloc_zero(FRAME_DECODE_BUFFER_LEN, sizeof(int32_t));

        // there's no loop chunk
        if (no_loop_chunk)
        {
            /**
             * No loops, just decode the frames and writeout result until end of ssnd.
            */
            while (end_of_ssnd == 0 && ssnd_chunk_pos < (size_t)(ssnd_data_size) && write_len < max_len)
            {
                AdpcmAifcFile_decode_frame(aaf, frame_buffer, &ssnd_chunk_pos, &end_of_ssnd);
                write_frame_output(&buffer[write_len], frame_buffer, 16);
                write_len += 16 * ADPCM_WAV_OUTPUT_SAMPLE_NUM_BYTES;
            }

            // last debug statement, not protected by DEBUG_ADPCMAIFCFILE_DECODE
            if (g_verbosity >= VERBOSE_DEBUG)
            {
                printf("%s %d: write_len=%ld\n", __func__, __LINE__, write_len);
            }
        }
        else
        {
            /**
             * (slightly different from AL_RAW16_WAVE)
             * Otherwise there are loops. Well, only one loop since the N64 only supports one loop.
             * The logic here is roughly:
             * - decode frames as above but only until the loop end offset.
             * - for each loop:
             *     - seek to loop start point in ssnd, and load state into framebuffer
             *     - decode frames again until loop end point
             * - decode frames until end of ssnd
             * 
             * The other main difference from above is that these loop points are not necessarily
             * on a multiple of 16. That means the first and last decode in each of the listed
             * steps will need to check the size to write to output buffer.
            */
            struct AdpcmAifcLoopData *loop_data;

            // offset, in bytes
            size_t interval16_offset;

            // count of remaining bytes in partial frame
            size_t interval16_delta;

            // number of frames
            size_t interval16_times;

            // loop counter (0,1,2,3 ...)
            size_t interval16_counter;

            // byte offset into ssnd chunk
            size_t loop_start_position;

            // number of times to decode loop data
            int loop_times;

            if (aaf->loop_chunk->nloops != 1)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s: N64 only supports single loop, aaf->loop_chunk->nloops=%d\n", __func__, aaf->loop_chunk->nloops);
            }

            loop_data = &aaf->loop_chunk->loop_data[0];

            if (loop_data == NULL)
            {
                stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: loop_data pointer is NULL\n", __func__);
            }

            if (loop_data->start < 0)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s: invalid loop_data->start offset: %d\n", __func__, loop_data->start);
            }

            if (loop_data->end < 0)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s: invalid loop_data->end offset: %d\n", __func__, loop_data->end);
            }

            loop_start_position = ((loop_data->start >> 4) + 1) * 9;

            if (loop_start_position > (size_t)ssnd_data_size)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s: loop start offset %ld is after end of ssnd data offset %d\n", __func__, loop_start_position, ssnd_data_size);
            }

            if (DEBUG_ADPCMAIFCFILE_DECODE && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("%s %d: loop_start_position=0x%08lx, loop_data->end=0x%08x\n", __func__, __LINE__, loop_start_position, loop_data->end);
            }

            interval16_times = loop_data->end >> 4;
            interval16_offset = interval16_times << 4;
            interval16_delta = loop_data->end - interval16_offset;

            if (DEBUG_ADPCMAIFCFILE_DECODE && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("%s %d: interval16_offset=0x%08lx, interval16_delta=%ld\n", __func__, __LINE__, interval16_offset, interval16_delta);
            }

            // decode frames until the loop end offset (closest multiple of 16)
            interval16_counter = 0;
            while (end_of_ssnd == 0
                && ssnd_chunk_pos < (size_t)(ssnd_data_size)
                && write_len < max_len
                && interval16_counter < interval16_times)
            {
                AdpcmAifcFile_decode_frame(aaf, frame_buffer, &ssnd_chunk_pos, &end_of_ssnd);
                write_frame_output(&buffer[write_len], frame_buffer, 16);
                write_len += 16 * ADPCM_WAV_OUTPUT_SAMPLE_NUM_BYTES;

                interval16_counter++;
            }

            if (DEBUG_ADPCMAIFCFILE_DECODE && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("%s %d: interval16_counter=%ld\n", __func__, __LINE__, interval16_counter);
                printf("%s %d: write_len=%ld\n", __func__, __LINE__, write_len);
            }

            // now decode one more entire frame (if required), but only write delta bytes.
            if (interval16_delta > 0)
            {
                AdpcmAifcFile_decode_frame(aaf, frame_buffer, &ssnd_chunk_pos, &end_of_ssnd);
                write_frame_output(&buffer[write_len], frame_buffer, interval16_delta);
                write_len += interval16_delta * ADPCM_WAV_OUTPUT_SAMPLE_NUM_BYTES;

                if (DEBUG_ADPCMAIFCFILE_DECODE && g_verbosity >= VERBOSE_DEBUG)
                {
                    printf("%s %d: write_len=%ld\n", __func__, __LINE__, write_len);
                }
            }

            // Determine number of loops to decode. If this is an infinite loop then
            // fallback to user specification.
            loop_times = loop_data->count;
            if (loop_times == -1)
            {
                loop_times = g_AdpcmLoopInfiniteExportCount;
            }

            if (DEBUG_ADPCMAIFCFILE_DECODE && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("%s %d: loop_times=%d\n", __func__, __LINE__, loop_times);
            }

            for (i=0; i<loop_times; i++)
            {
                int state_frame_buffer_index;
                int state_loop_data_index;

                if (DEBUG_ADPCMAIFCFILE_DECODE && g_verbosity >= VERBOSE_DEBUG)
                {
                    printf("%s %d: loop i=%d\n", __func__, __LINE__, i);
                }
                
                // Load initial loop state.
                // This copies values from array of type int16_t
                // to array of type int32_t.
                memset(frame_buffer, 0, ADPCM_AIFC_LOOP_STATE_LEN);
                state_frame_buffer_index = 0;
                state_loop_data_index = 0;

                if (DEBUG_ADPCMAIFCFILE_DECODE && g_verbosity >= VERBOSE_DEBUG)
                {
                    printf("%s %d: frame_buffer: \n", __func__, __LINE__);
                }

                while (state_frame_buffer_index < (ADPCM_AIFC_LOOP_STATE_LEN / ADPCM_LOOP_STATE_ELEMENT_SIZE))
                {
                    // loop state is stored big endian
                    int16_t t;
                    memcpy(&t, &loop_data->state[state_loop_data_index], ADPCM_LOOP_STATE_ELEMENT_SIZE);
                    BSWAP16(t);

                    // implicit cast for sign extend.
                    frame_buffer[state_frame_buffer_index] = t;

                    if (DEBUG_ADPCMAIFCFILE_DECODE && g_verbosity >= VERBOSE_DEBUG)
                    {
                        printf("0x%08x ", frame_buffer[state_frame_buffer_index]);
                    }

                    state_frame_buffer_index++;
                    state_loop_data_index += ADPCM_LOOP_STATE_ELEMENT_SIZE;
                }

                if (DEBUG_ADPCMAIFCFILE_DECODE && g_verbosity >= VERBOSE_DEBUG)
                {
                    printf("\n");
                }

                interval16_delta = loop_data->start & 0xf;

                if (DEBUG_ADPCMAIFCFILE_DECODE && g_verbosity >= VERBOSE_DEBUG)
                {
                    printf("%s %d: interval16_offset=0x%08lx, interval16_delta=%ld\n", __func__, __LINE__, interval16_offset, interval16_delta);
                }

                // Write preliminary loop framebuffer data.
                // This is index zero for the count of frames written.
                write_frame_output(&buffer[write_len], &frame_buffer[interval16_delta], 16 - interval16_delta);
                write_len += (16 - interval16_delta) * ADPCM_WAV_OUTPUT_SAMPLE_NUM_BYTES;

                if (DEBUG_ADPCMAIFCFILE_DECODE && g_verbosity >= VERBOSE_DEBUG)
                {
                    printf("%s %d: write_len=%ld\n", __func__, __LINE__, write_len);
                }

                // Calculate number of (full) frames to decode.
                interval16_times = (loop_data->end >> 4) - (loop_data->start >> 4);
                
                // seek input ssnd position to loop start
                ssnd_chunk_pos = loop_start_position;

                // Any prequel for the start of the loop frame was already written above, which
                // is considered counter index zero. Therefore, the main loop needs to start
                // counting on frame 1.
                interval16_counter = 1;

                // write loop data until end of loop
                while (end_of_ssnd == 0
                    && ssnd_chunk_pos < (size_t)(ssnd_data_size)
                    && write_len < max_len
                    && interval16_counter < interval16_times)
                {
                    interval16_counter++;

                    AdpcmAifcFile_decode_frame(aaf, frame_buffer, &ssnd_chunk_pos, &end_of_ssnd);
                    write_frame_output(&buffer[write_len], frame_buffer, 16);
                    write_len += 16 * ADPCM_WAV_OUTPUT_SAMPLE_NUM_BYTES;
                }

                if (DEBUG_ADPCMAIFCFILE_DECODE && g_verbosity >= VERBOSE_DEBUG)
                {
                    printf("%s %d: interval16_counter=%ld\n", __func__, __LINE__, interval16_counter);
                    printf("%s %d: write_len=%ld\n", __func__, __LINE__, write_len);
                }

                // Decode any remainder data after the last full frame.
                interval16_delta = (loop_data->end - loop_data->start) - (interval16_times << 4);
                if (interval16_delta > 0)
                {
                    AdpcmAifcFile_decode_frame(aaf, frame_buffer, &ssnd_chunk_pos, &end_of_ssnd);
                    write_frame_output(&buffer[write_len], frame_buffer, interval16_delta);
                    write_len += interval16_delta * ADPCM_WAV_OUTPUT_SAMPLE_NUM_BYTES;

                    if (DEBUG_ADPCMAIFCFILE_DECODE && g_verbosity >= VERBOSE_DEBUG)
                    {
                        printf("%s %d: write_len=%ld\n", __func__, __LINE__, write_len);
                    }
                }
            }

            if (DEBUG_ADPCMAIFCFILE_DECODE && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("%s %d: write_len=%ld\n", __func__, __LINE__, write_len);
            }

            // now just regular decode until end of file
            while (end_of_ssnd == 0 && ssnd_chunk_pos < (size_t)(ssnd_data_size) && write_len < max_len)
            {
                AdpcmAifcFile_decode_frame(aaf, frame_buffer, &ssnd_chunk_pos, &end_of_ssnd);
                write_frame_output(&buffer[write_len], frame_buffer, 16);
                write_len += 16 * ADPCM_WAV_OUTPUT_SAMPLE_NUM_BYTES;
            }

            // last debug statement, not protected by DEBUG_ADPCMAIFCFILE_DECODE
            if (g_verbosity >= VERBOSE_DEBUG)
            {
                printf("%s %d: write_len=%ld\n", __func__, __LINE__, write_len);
            }
        }

        free(frame_buffer);

        TRACE_LEAVE(__func__)

        return write_len;
    }
    else
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> unsupported compression type 0x%08x\n", __func__, __LINE__, aaf->comm_chunk->compression_type);
    }

    TRACE_LEAVE(__func__)

    // be quiet gcc
    return 0;
}

/**
 * Returns sample rate as a 32 bit integer.
 * @param aaf: file to get sample rate from.
 * @returns: converted sample rate, or -1 on error.
*/
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

/**
 * Frees memory allocated to chunk.
 * @param chunk: object to free.
*/
void AdpcmAifcCommChunk_free(struct AdpcmAifcCommChunk *chunk)
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
 * Frees memory allocated to chunk.
 * @param chunk: object to free.
*/
void AdpcmAifcSoundChunk_free(struct AdpcmAifcSoundChunk *chunk)
{
    TRACE_ENTER(__func__)

    if (chunk == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (chunk->sound_data != NULL)
    {
        free(chunk->sound_data);
        chunk->sound_data = NULL;
    }

    free(chunk);

    TRACE_LEAVE(__func__)
}

/**
 * Frees memory allocated to chunk.
 * @param chunk: object to free.
*/
void AdpcmAifcCodebookChunk_free(struct AdpcmAifcCodebookChunk *chunk)
{
    TRACE_ENTER(__func__)

    if (chunk == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (chunk->table_data != NULL)
    {
        free(chunk->table_data);
        chunk->table_data = NULL;
    }

    if (chunk->coef_table != NULL)
    {
        int i,j;

        for (i = 0; i < chunk->nentries; i++)
        {
            if (chunk->coef_table[i] != NULL)
            {
                for (j = 0; j < 8; j++)
                {
                    if (chunk->coef_table[i][j] != NULL)
                    {
                        free(chunk->coef_table[i][j]);
                    }
                }

                free(chunk->coef_table[i]);
            }
        }

        free(chunk->coef_table);
        chunk->coef_table = NULL;
    }

    free(chunk);

    TRACE_LEAVE(__func__)
}

/**
 * Frees memory allocated to chunk.
 * @param chunk: object to free.
*/
void AdpcmAifcLoopChunk_free(struct AdpcmAifcLoopChunk *chunk)
{
    TRACE_ENTER(__func__)

    if (chunk == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (chunk->loop_data != NULL)
    {
        free(chunk->loop_data);
        chunk->loop_data = NULL;
    }

    free(chunk);

    TRACE_LEAVE(__func__)
}

/**
 * Frees memory allocated to aifc file and all child elements.
 * @param aifc_file: object to free.
*/
void AdpcmAifcFile_free(struct AdpcmAifcFile *aifc_file)
{
    TRACE_ENTER(__func__)

    int i;

    if (aifc_file == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (aifc_file->chunks != NULL)
    {
        // need to iterate the list in case there are duplicates.
        for (i=0; i<aifc_file->chunk_count; i++)
        {
            uint32_t ck_id = *(uint32_t *)aifc_file->chunks[i];
            switch (ck_id)
            {
                case ADPCM_AIFC_COMMON_CHUNK_ID:
                    AdpcmAifcCommChunk_free((struct AdpcmAifcCommChunk *)aifc_file->chunks[i]);
                    aifc_file->comm_chunk = NULL;
                    break;

                case ADPCM_AIFC_SOUND_CHUNK_ID:
                    AdpcmAifcSoundChunk_free((struct AdpcmAifcSoundChunk *)aifc_file->chunks[i]);
                    aifc_file->sound_chunk = NULL;
                    break;

                case ADPCM_AIFC_APPLICATION_CHUNK_ID:
                {
                    struct AdpcmAifcApplicationChunk *appl = (struct AdpcmAifcApplicationChunk *)aifc_file->chunks[i];
                    // code_string doesn't have terminating zero, requires explicit length
                    if (strncmp(appl->code_string, ADPCM_AIFC_VADPCM_CODES_NAME, ADPCM_AIFC_VADPCM_APPL_NAME_LEN) == 0)
                    {
                        AdpcmAifcCodebookChunk_free((struct AdpcmAifcCodebookChunk *)appl);
                        aifc_file->codes_chunk = NULL;
                    }
                    else if (strncmp(appl->code_string, ADPCM_AIFC_VADPCM_LOOPS_NAME, ADPCM_AIFC_VADPCM_APPL_NAME_LEN) == 0)
                    {
                        AdpcmAifcLoopChunk_free((struct AdpcmAifcLoopChunk *)appl);
                        aifc_file->loop_chunk = NULL;
                    }
                }
                
                default:
                    // ignore unsupported
                    break;
            }
        }

        free(aifc_file->chunks);
    }

    free(aifc_file);

    TRACE_LEAVE(__func__)
}

/**
 * Estimates the number of bytes required to inflate the AIFC ssnd data including
 * space required for looping.
 * This always assumes the file is compressed; this will over allocate a little
 * for AL_ADPCM_WAVE and over allocate a lot for AL_RAW16_WAVE.
 * @param aifc_file: file to estimate.
 * @returns: size in bytes. This should always be larger than the actual number of bytes required.
*/
size_t AdpcmAifcFile_estimate_inflate_size(struct AdpcmAifcFile *aifc_file)
{
    TRACE_ENTER(__func__)

    size_t estimate;
    int i;

    if (aifc_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: aifc_file is NULL\n", __func__);
    }

    if (aifc_file->sound_chunk == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: aifc_file->sound_chunk is NULL\n", __func__);
    }

    // start with regular ssnd chunk size
    estimate = aifc_file->sound_chunk->ck_data_size;

    if (g_AdpcmLoopInfiniteExportCount > 0 && aifc_file->loop_chunk != NULL)
    {
        int nloops = aifc_file->loop_chunk->nloops;
        struct AdpcmAifcLoopData *loop_data;
        int32_t loop_size = 0;

        // there should only be one loop, but don't worry about that here.
        for (i=0; i<nloops; i++)
        {
            int32_t current_loop_size;
            loop_data = &aifc_file->loop_chunk->loop_data[i];

            if (loop_data == NULL)
            {
                stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: loop_data is NULL\n", __func__);
            }

            current_loop_size = loop_data->end - loop_data->start;

            if (current_loop_size < 0)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s: invalid loop size: %d\n", __func__, current_loop_size);
            }

            if (aifc_file->loop_chunk->loop_data->count > 0)
            {
                loop_size += current_loop_size * aifc_file->loop_chunk->loop_data->count;
            }
            else
            {
                loop_size += current_loop_size * g_AdpcmLoopInfiniteExportCount;
            }
        }
        
        estimate += loop_size;
    }

    // this should be using ~4:1 compression, overestimate a bit by taking x5.
    estimate *= 5;

    TRACE_LEAVE(__func__)

    return estimate;
}

/**
 * Reads the next byte from the sound chunk. If the end of the chunk has been reached
 * then zero is returned.
 * @param aaf: container file.
 * @param ssnd_chunk_pos: in/out paramter. Current byte position within the sound chunk. If not
 * {@code eof} then will be set to next byte position.
 * @param eof: out parameter. If {@code ssnd_chunk_pos} is less than the size of the sound data
 * in the sound chunk this is set to 1. Otherwise set to zero.
 * @returns: next byte from sound chunk or zero.
*/
static uint8_t get_sound_chunk_byte(struct AdpcmAifcFile *aaf, size_t *ssnd_chunk_pos, int *eof)
{
    TRACE_ENTER(__func__)

    // careful with the comparison, valgrind will confirm if reading past allocated size though.
    if (*ssnd_chunk_pos >= (size_t)(aaf->sound_chunk->ck_data_size - 8))
    {
        *eof = 1;

        TRACE_LEAVE(__func__)
        return 0;
    }

    *eof = 0;
    uint8_t ret = aaf->sound_chunk->sound_data[*ssnd_chunk_pos];
    *ssnd_chunk_pos = *ssnd_chunk_pos + 1;

    TRACE_LEAVE(__func__)

    return ret;
}

/**
 * Writes to output buffer within clamped range in 16 bit elements.
 * No allocations are performed.
 * @param out: destination buffer.
 * @param data: data to clamp and write to output buffer.
 * @param size: number of 16 bit elements to write.
*/
static void write_frame_output(uint8_t *out, int32_t *data, size_t size)
{
    TRACE_ENTER(__func__)

    size_t i;
    for (i=0; i<size; i++)
    {
        int16_t val = (int16_t)clamp(data[i], -0x7fff, 0x7fff);

        ((int16_t *)out)[i] = val;
    }

    TRACE_LEAVE(__func__)
}