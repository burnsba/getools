#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>
#include "machine_config.h"
#include "debug.h"
#include "common.h"
#include "gaudio_math.h"
#include "utility.h"
#include "adpcm_aifc.h"
#include "naudio.h"
#include "llist.h"

/**
 * This file contains primary aifc methods.
*/

/**
 * When converting .aifc, this specifies the number of times an ADPCM Loop
 * with repeat=infinite should be repeated in the export.
*/
int g_AdpcmLoopInfiniteExportCount = 0;

// forward declarations

static uint8_t get_sound_chunk_byte(struct AdpcmAifcFile *aaf, size_t *ssnd_chunk_pos, int *eof);
static void write_frame_output(uint8_t *out, int32_t *data, size_t size);
static struct AdpcmAifcCommChunk *AdpcmAifcCommChunk_new_from_file(struct file_info *fi, int32_t ck_data_size);
static struct AdpcmAifcApplicationChunk *AdpcmAifcApplicationChunk_new_from_file(struct file_info *fi, int32_t ck_data_size);
static struct AdpcmAifcSoundChunk *AdpcmAifcSoundChunk_new_from_file(struct file_info *fi, int32_t ck_data_size);

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
    
    if (chunk_count > 0)
    {
        p->chunks = (void**)malloc_zero(chunk_count, sizeof(void*));
    }

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

    size_t pos;
    uint32_t chunk_id;
    int chunk_count;
    int chunk_size;
    int seen_comm = 0;
    int seen_appl = 0;
    int seen_ssnd = 0;

    if (fi->len < 12)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s: Invalid .aifc file: header too short\n", __func__);
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
        stderr_exit(EXIT_CODE_GENERAL, "%s: Invalid .aifc file: FORM chunk id failed. Expected 0x%08x, read 0x%08x.\n", __func__, ADPCM_AIFC_FORM_CHUNK_ID, p->ck_id);
    }

    if (p->form_type != ADPCM_AIFC_FORM_TYPE_ID)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s: Invalid .aifc file: FORM type id failed. Expected 0x%08x, read 0x%08x.\n", __func__, ADPCM_AIFC_FORM_TYPE_ID, p->form_type);
    }

    // As the file is scanned, supported chunks will be parsed and added to a list.
    // Once the main aifc container is allocated the allocated chunks will
    // be added to the aifc container chunk list.
    pos = ftell(fi->fp);
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
                if (chunk_count > 0)
                {
                    chunk_count--;
                }
                file_info_fseek(fi, chunk_size, SEEK_CUR);
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
        stderr_exit(EXIT_CODE_GENERAL, "%s: Invalid .aifc file: needs more chonk\n", __func__);
    }

    if (seen_comm == 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s: Invalid .aifc file: missing COMM chunk\n", __func__);
    }

    if (seen_appl == 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s: Invalid .aifc file: missing APPL chunk\n", __func__);
    }

    if (seen_ssnd == 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s: Invalid .aifc file: missing SSND chunk\n", __func__);
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
 * This allocates memory and stores the results in {@code struct AdpcmAifcCodebookChunk.coef_table}.
 * @param chunk: codebook chunk to decode.
*/
void AdpcmAifcCodebookChunk_decode_aifc_codebook(struct AdpcmAifcCodebookChunk *chunk)
{
    TRACE_ENTER(__func__)

    int i,j;
    int code_book_pos = 0;
    int row, col;

    /**
     * Comments are assuming the following example:
     * order=2
     * npredictors=1
     * table_data (raw)=
     *     FA E2 FA D0 FE 04 01 4F
     *     02 98 01 CB 00 29 FF 03
     *     08 1C 03 1A FD F5 FB F2
     *     FD 32 FF C1 01 8B 01 B9
    */

    // allocate memory for list of lists
    chunk->coef_table = malloc_zero(chunk->nentries, sizeof(int32_t**));

    for (i = 0; i < chunk->nentries; i++)
    {
        // Allocate memory for the rows.
        // There are 8 rows.
        chunk->coef_table[i] = malloc_zero(8, sizeof(int32_t*));

        for (j = 0; j < 8; j++)
        {
            // For each row,
            // allocate memory for the column.
            // This is `order` + 8 columns (probably 2 + 8).
            chunk->coef_table[i][j] = malloc_zero(chunk->order + 8, sizeof(int32_t));
        }
    }

    // If `order` is 2, there's now a 8x10 grid allocated.

    for (i = 0; i < chunk->nentries; i++)
    {
        int32_t **table_entry = chunk->coef_table[i];

        /**
         * Copy the `entry` into the coef_table. This travels down the
         * rows and sets the j'th column value to the value read,
         * where j increases up to the `order.
        */
        for (col = 0; col < chunk->order; col++)
        {
            for (row = 0; row < 8; row++)
            {
                // 0x16 is sizeof other stuff in the chunk header
                if (code_book_pos > chunk->base.ck_data_size - 0x16)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s: attempt to read past end of codebook\n", __func__);
                }

                // careful, this needs to pass a signed 16 bit int to bswap, and then sign extend promote to 32 bit.
                int16_t ts = BSWAP16_INLINE(*(int16_t*)(&chunk->table_data[code_book_pos]));
                table_entry[row][col] = (int32_t)ts;
                code_book_pos += 2;
            }
        }

        /**
         * table_entry should now look like (BSWAP applied, values in hex)
         * [FAE2] [081C] [0] [0] [0] [0] [0] [0] [0] [0]
         * [FAD0] [031A] [0] [0] [0] [0] [0] [0] [0] [0]
         * [FE04] [FDF5] [0] [0] [0] [0] [0] [0] [0] [0]
         * [014F] [FBF2] [0] [0] [0] [0] [0] [0] [0] [0]
         * [0298] [FD32] [0] [0] [0] [0] [0] [0] [0] [0]
         * [01CB] [FFC1] [0] [0] [0] [0] [0] [0] [0] [0]
         * [0029] [018B] [0] [0] [0] [0] [0] [0] [0] [0]
         * [FF03] [01B9] [0] [0] [0] [0] [0] [0] [0] [0]
        */

        /**
         * Now fill out the rest of the grid from existing values.
         * The last column set above should be carried forward to the
         * remaining columns, up to column 8. Each time the column
         * is increased, the row number should increase too:
         * 
         * [FAE2] [081C] --> [0000] [0] [0] [0] [0] [0] [0] [0]
         *                     |
         *                     v
         * [FAD0] [031A]     [081C] [0] [0] [0] [0] [0] [0] [0]
         * 
         * The only other info needed is that the first unset value
         * in the first row should be set to 2048. 
        */

        // This approach is probably bad for cache misses, but the data set is
        // so small it doesn't matter.
        for (j=0; j<8; j++)
        {
            int val;
            int col_limit = 8 + chunk->order;

            row = j;
            col = chunk->order;

            if (j == 0)
            {
                val = 2048;
            }
            else
            {
                val = table_entry[row - 1][col - 1];
            }

            while (row < 8 && col < col_limit)
            {
                table_entry[row][col] = val;

                row++;
                col++;
            }
        }

        if (g_verbosity >= VERBOSE_DEBUG)
        {
            printf("aifc codebook / coef table / table_entry:\n");

            for (row=0; row<8; row++)
            {
                for (col=0; col<8+chunk->order; col++)
                {
                    printf("[%04X] ", (uint16_t)table_entry[row][col]);
                }
                printf("\n");
            }

            printf("\n\n");
        }

        /**
         * table_entry should now look like (BSWAP applied, values in hex)
         * [FAE2] [081C] [0800] [0000] [0000] [0000] [0000] [0000] [0000] [0000]
         * [FAD0] [031A] [081C] [0800] [0000] [0000] [0000] [0000] [0000] [0000]
         * [FE04] [FDF5] [031A] [081C] [0800] [0000] [0000] [0000] [0000] [0000]
         * [014F] [FBF2] [FDF5] [031A] [081C] [0800] [0000] [0000] [0000] [0000]
         * [0298] [FD32] [FBF2] [FDF5] [031A] [081C] [0800] [0000] [0000] [0000]
         * [01CB] [FFC1] [FD32] [FBF2] [FDF5] [031A] [081C] [0800] [0000] [0000]
         * [0029] [018B] [FFC1] [FD32] [FBF2] [FDF5] [031A] [081C] [0800] [0000]
         * [FF03] [01B9] [018B] [FFC1] [FD32] [FBF2] [FDF5] [031A] [081C] [0800]
        */
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
    p->version = ADPCM_AIFC_VADPCM_CODEBOOK_VERSION;
    p->order = order;
    p->nentries = nentries;
    
    // no terminating zero
    memcpy(p->base.code_string, ADPCM_AIFC_VADPCM_CODES_NAME, ADPCM_AIFC_VADPCM_APPL_NAME_LEN);

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
 * Convert 16-bit mono PCM samples to format specified in .aifc comm chunk.
 * If the audio needs to be encoded, this is the top level entry to encode it.
 * This assumes "comm" chunk already exists with all parameters correctly set.
 * If looping is to be used, this assumes "loop" chunk already exists with
 * start and end values correctly set; then this method will update loop
 * state to correct value during encode process.
 * @param aaf: Output .aifc container. This method will allocate memory
 * for the sound data. If sound chunk was previously allocated, the
 * contents will be freed.
 * @param buffer: buffer containing samples.
 * @param max_len: Length in bytes of input buffer.
 * @returns: number of bytes written
*/
size_t AdpcmAifcFile_encode(struct AdpcmAifcFile *aaf, uint8_t *buffer, size_t buffer_len)
{
    TRACE_ENTER(__func__)

    if (aaf == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> aaf is NULL\n", __func__, __LINE__);
    }

    if (aaf->comm_chunk == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> aaf->comm_chunk is NULL\n", __func__, __LINE__);
    }

    if (buffer == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> buffer is NULL\n", __func__, __LINE__);
    }

    if (buffer_len == 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> buffer_len is zero\n", __func__, __LINE__);
    }

    size_t write_len = 0;
    size_t ssnd_chunk_pos = 0;
    size_t sound_data_pos = 0;
    size_t ssnd_data_size;

    int use_loop = 0;
    int found_loop_state = 0;

    use_loop = aaf->loop_chunk != NULL
        && aaf->loop_chunk->nloops == 1
        && aaf->loop_chunk->loop_data != NULL;

    /**
     * If this is uncompressed audio then there's no codebook.
    */
    if (aaf->comm_chunk->compression_type == ADPCM_AIFC_NONE_COMPRESSION_TYPE_ID)
    {
        if (g_verbosity >= VERBOSE_DEBUG)
        {
            printf("compression type=NONE\n");
        }

        // no compression means there's nothing to do for the loop state.
        use_loop = 0;

        if (g_verbosity >= VERBOSE_DEBUG)
        {
            printf("write loop data=%d\n", use_loop);
        }

        aaf->sound_chunk = AdpcmAifcSoundChunk_new(buffer_len);
        AdpcmAifcFile_append_chunk(aaf, aaf->sound_chunk);

        if (g_encode_bswap)
        {
            // adjust count to 16-byte pieces.
            int num_16 = buffer_len / 2;
            bswap16_chunk(aaf->sound_chunk->sound_data, buffer, num_16);
        }
        else
        {
            memcpy(aaf->sound_chunk->sound_data, buffer, buffer_len);
        }

        // last debug statement, not protected by DEBUG_ADPCMAIFCFILE_DECODE
        if (g_verbosity >= VERBOSE_DEBUG)
        {
            printf("%s %d: write_len=%ld\n", __func__, __LINE__, buffer_len);
        }
    }
    else if (aaf->comm_chunk->compression_type == ADPCM_AIFC_VAPC_COMPRESSION_TYPE_ID)
    {
        if (g_verbosity >= VERBOSE_DEBUG)
        {
            printf("compression type=VAPC\n");
            printf("write loop data=%d\n", use_loop);
        }

        if (aaf->codes_chunk == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> aaf->codes_chunk is NULL\n", __func__, __LINE__);
        }

        if (aaf->codes_chunk->coef_table == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> aaf->codes_chunk->coef_table is NULL\n", __func__, __LINE__);
        }

        if (aaf->codes_chunk->order < 1)
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> invalid order: %d\n", __func__, __LINE__, aaf->codes_chunk->order);
        }

        if (aaf->codes_chunk->nentries < 1)
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> invalid nentries: %d\n", __func__, __LINE__, aaf->codes_chunk->nentries);
        }

        // if there's no loop, these won't be used, but default to a large value
        // in case any comparisons are made ...
        int32_t loop_start_sample = INT32_MAX;
        int32_t loop_start_byte = INT32_MAX;

        int32_t loop_end_sample = INT32_MAX;
        int32_t loop_end_byte = INT32_MAX;

        if (use_loop)
        {
            loop_start_sample = aaf->loop_chunk->loop_data->start;
            loop_start_byte = loop_start_sample * 2;
            loop_end_sample = aaf->loop_chunk->loop_data->end;
            loop_end_byte = loop_end_sample * 2;

            if (loop_start_byte > loop_end_byte)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> loop_start_byte=%d > loop_end_byte=%d\n", __func__, __LINE__, loop_start_byte, loop_end_byte);
            }

            if ((size_t)loop_start_byte > buffer_len)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> loop_start_byte=%d > buffer_len=%ld\n", __func__, __LINE__, loop_start_byte, buffer_len);
            }

            if ((size_t)loop_end_byte > buffer_len)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> loop_end_byte=%d > buffer_len=%ld\n", __func__, __LINE__, loop_end_byte, buffer_len);
            }
        }

        int32_t *apc_state = (int32_t *)malloc_zero(aaf->codes_chunk->order, sizeof(int32_t));

        // estimate space for sound data.
        ssnd_data_size = 9 * (buffer_len / 32); /* scale 32:9, 16 16-bit samples into 9 byte windows */
        ssnd_data_size += 100;

        aaf->sound_chunk = AdpcmAifcSoundChunk_new(ssnd_data_size);
        AdpcmAifcFile_append_chunk(aaf, aaf->sound_chunk);

        // iterate and encode data into the sound chunk.
        while (ssnd_chunk_pos < ssnd_data_size && sound_data_pos < buffer_len)
        {
            int encode_bytes;
            int16_t sample_buffer[FRAME_DECODE_BUFFER_LEN];
            
            // The loop state information should be the state before the beginning of the loop
            // is processed. This should only get set once, so there's flag to disable
            // checking after it's read.
            if (use_loop && found_loop_state == 0)
            {
                // if there's no loop, it doesn't make sense to compare loop_start_byte
                if ((sound_data_pos + 16) > (size_t)loop_start_byte)
                {
                    int loop_state_index;
                    int order = aaf->codes_chunk->order;

                    found_loop_state = 1;

                    for (loop_state_index=0; loop_state_index<order; loop_state_index++)
                    {
                        aaf->loop_chunk->loop_data->state[ADPCM_AIFC_LOOP_STATE_LEN - order - loop_state_index] = apc_state[loop_state_index];
                    }
                }
            }

            memset(sample_buffer, 0, FRAME_DECODE_BUFFER_LEN * sizeof(int16_t));

            fill_16bit_buffer(
                sample_buffer,
                FRAME_DECODE_BUFFER_LEN,
                buffer,
                &sound_data_pos,
                buffer_len);
            
            if (g_encode_bswap)
            {
                // conversion needs to happen before encoding.
                bswap16_chunk(sample_buffer, sample_buffer, FRAME_DECODE_BUFFER_LEN); // inplace swap is ok
            }

            // Now read the next chunk of sound data. This advances the position counters for
            // the .aifc sound chunk and the buffer. This updates the encoder state.
            encode_bytes = AdpcmAifcFile_encode_frame(aaf, sample_buffer, apc_state, &ssnd_chunk_pos);

            write_len += encode_bytes;
        }

        free(apc_state);

        // last debug statement, not protected by DEBUG_ADPCMAIFCFILE_ENCODE
        if (g_verbosity >= VERBOSE_DEBUG)
        {
            printf("%s %d: write_len=%ld\n", __func__, __LINE__, write_len);
        }
    }
    else
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> unsupported compression type 0x%08x\n", __func__, __LINE__, aaf->comm_chunk->compression_type);
    }

    if (use_loop)
    {
        if (found_loop_state == 0)
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> expected loop state information, but that was never set\n", __func__, __LINE__);
        }
    }

    // set the "num_sample_frames" field, which should be the size of the uncompressed data
    aaf->comm_chunk->num_sample_frames = buffer_len / 2; // 16 bit samples

    // adjust sound chunk size to actual written data size
    aaf->sound_chunk->ck_data_size = 8 + write_len; // 8 = sizeof offset,block_size

    TRACE_LEAVE(__func__)
    return write_len;
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

    // counter for doing chunk-size bswap16
    int num_16;

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
        if (g_verbosity >= VERBOSE_DEBUG)
        {
            printf("compression type=NONE\n");
        }

        if (ssnd_data_size & 0x1)
        {
            fflush_printf(stderr, "warning, ssnd_data_size is odd, truncating last byte (required for bswap)\n");
            ssnd_data_size--;
        }

        if (no_loop_chunk)
        {
            if (g_verbosity >= VERBOSE_DEBUG)
            {
                printf("not looping\n");
            }

            // adjust count to 16-byte pieces.
            num_16 = ssnd_data_size / 2;
            bswap16_chunk(buffer, aaf->sound_chunk->sound_data, num_16);

            // last debug statement, not protected by DEBUG_ADPCMAIFCFILE_DECODE
            if (g_verbosity >= VERBOSE_DEBUG)
            {
                printf("%s %d: write_len=%d\n", __func__, __LINE__, num_16 * 2);
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

            if (g_verbosity >= VERBOSE_DEBUG)
            {
                printf("loop chunk\n");
            }

            if (aaf->loop_chunk->nloops != 1)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s: N64 only supports single loop, aaf->loop_chunk->nloops=%d\n", __func__, aaf->loop_chunk->nloops);
            }

            loop_data = aaf->loop_chunk->loop_data;

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
                if ((loop_start_position - 2) & 0x1)
                {
                    fflush_printf(stderr, "warning, loop_start_position is odd, truncating last byte (required for bswap)\n");
                }

                // adjust count to 16-byte pieces.
                num_16 = (loop_start_position - 2) / 2;

                bswap16_chunk(&buffer[write_len], &aaf->sound_chunk->sound_data[0], num_16);
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
                if (loop_size_bytes & 0x1)
                {
                    fflush_printf(stderr, "warning, loop_size_bytes is odd, truncating last byte (required for bswap)\n");
                }

                // adjust count to 16-byte pieces.
                num_16 = loop_size_bytes / 2;

                ssnd_chunk_pos = loop_start_position;
                bswap16_chunk(&buffer[write_len], &aaf->sound_chunk->sound_data[ssnd_chunk_pos], num_16);
                write_len += loop_size_bytes;
                ssnd_chunk_pos += loop_size_bytes;
            }

            if (after_loop_bytes > 0)
            {
                if (after_loop_bytes & 0x1)
                {
                    fflush_printf(stderr, "warning, after_loop_bytes is odd, truncating last byte (required for bswap)\n");
                }

                // adjust count to 16-byte pieces.
                num_16 = after_loop_bytes / 2;

                bswap16_chunk(&buffer[write_len], &aaf->sound_chunk->sound_data[ssnd_chunk_pos], num_16);
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

        if (g_verbosity >= VERBOSE_DEBUG)
        {
            printf("compression type=VAPC\n");
        }

        // there's no loop chunk
        if (no_loop_chunk)
        {
            if (g_verbosity >= VERBOSE_DEBUG)
            {
                printf("not looping\n");
            }

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

            if (g_verbosity >= VERBOSE_DEBUG)
            {
                printf("loop chunk\n");
            }

            if (aaf->loop_chunk->nloops != 1)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s: N64 only supports single loop, aaf->loop_chunk->nloops=%d\n", __func__, aaf->loop_chunk->nloops);
            }

            loop_data = aaf->loop_chunk->loop_data;

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
 * Extracts the sound data from an .aifc file and writes to .tbl
 * file at the current file offset.
 * @param path: .aifc file to open and extract data from.
 * @param fi: file to write to.
 * @param sound_data_len: out parameter. Optional. If provided, will contain the number of bytes
 * of sound data written. This may be less than the number of bytes written
 * due to padding.
 * @returns: number of bytes written.
*/
size_t AdpcmAifcFile_path_write_tbl(char *path, struct file_info *fi, size_t *sound_data_len)
{
    TRACE_ENTER(__func__)

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("Open file \"%s\" to write to .tbl\n", path);
    }

    struct file_info *aifc_fi = file_info_fopen(path, "rb");
    struct AdpcmAifcFile *aifc_file = AdpcmAifcFile_new_from_file(aifc_fi);
    size_t result = AdpcmAifcFile_write_tbl(aifc_file, fi, sound_data_len);
    AdpcmAifcFile_free(aifc_file);
    file_info_free(aifc_fi);

    TRACE_LEAVE(__func__)

    return result;
}

/**
 * Extracts the sound data from an .aifc file and writes to .tbl
 * file at the current file offset.
 * @param aifc_file: file containing sound data to write.
 * @param fi: file to write to.
 * @param sound_data_len: out parameter. Optional. If provided, will contain the number of bytes
 * of sound data written. This may be less than the number of bytes written
 * due to padding.
 * @returns: number of bytes written.
*/
size_t AdpcmAifcFile_write_tbl(struct AdpcmAifcFile *aifc_file, struct file_info *fi, size_t *sound_data_len)
{
    TRACE_ENTER(__func__)

    if (aifc_file->sound_chunk == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> aifc_file->sound_chunk is NULL\n", __func__, __LINE__);
    }

    if (aifc_file->sound_chunk->ck_data_size < 8)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> aifc_file->sound_chunk too small\n", __func__, __LINE__);
    }

    size_t len = aifc_file->sound_chunk->ck_data_size - 8;

    size_t actual = file_info_fwrite(fi, aifc_file->sound_chunk->sound_data, len, 1);

    if (len != actual)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> write error, expected to write %ls but wrote %ld bytes\n", __func__, __LINE__, len, actual);
    }

    if (sound_data_len != NULL)
    {
        *sound_data_len = len;
    }

    /**
     * Implementation note:
     * The .tbl file pads each sound section to the nearest multiple of 8.
     * This is over the length specified in the .ctl file.
    */
    int over = len % 8;
    if (over > 0)
    {
        int remain = 8 - over;
        uint8_t pad[8];
        memset(pad, 0, 8);

        len += file_info_fwrite(fi, pad, remain, 1);
    }

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("Write %ld bytes\n", len);
    }

    TRACE_LEAVE(__func__)

    return len;
}

/**
 * Adds codebook chunk to .aifc file.
 * This allocates memory and dynamically resizes the chunk list on the .aifc file.
 * @param aaf: file to add codebook chunk to.
 * @param book: codebook.
*/
void AdpcmAifcFile_add_codebook_from_ALADPCMBook(struct AdpcmAifcFile *aaf, struct ALADPCMBook *book)
{
    TRACE_ENTER(__func__)

    if (aaf == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> aaf is NULL\n", __func__, __LINE__);
    }

    if (book == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> book is NULL\n", __func__, __LINE__);
    }

    int order = book->order;
    int npredictors = book->npredictors;

    if (order < 1)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> invalid order: %d\n", __func__, __LINE__, order);
    }

    if (npredictors < 1)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> invalid order: %d\n", __func__, __LINE__, npredictors);
    }

    struct AdpcmAifcCodebookChunk *chunk = AdpcmAifcCodebookChunk_new((int16_t)order, (int16_t)npredictors);

    size_t table_data_size_bytes = order * npredictors * 16;
    memcpy(chunk->table_data, book->book, table_data_size_bytes);
    AdpcmAifcCodebookChunk_decode_aifc_codebook(chunk);

    aaf->codes_chunk = chunk;
    AdpcmAifcFile_append_chunk(aaf, aaf->codes_chunk);

    TRACE_LEAVE(__func__)
}

/**
 * This dynamically resizes the chunk array and adds item to end of new list.
 * This does not update any convenience pointers.
 * @param aifc_file: .aifc file to add chunk to.
 * @param chunk: chunk to add.
*/
void AdpcmAifcFile_append_chunk(struct AdpcmAifcFile *aifc_file, void *chunk)
{
    TRACE_ENTER(__func__)

    if (aifc_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: aifc_file is NULL\n", __func__, __LINE__);
    }

    if (chunk == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: chunk is NULL\n", __func__, __LINE__);
    }

    aifc_file->chunk_count++;

    // this could be initializing the chunks container for the first time.
    if (aifc_file->chunks == NULL)
    {
        aifc_file->chunks = (void**)malloc_zero(aifc_file->chunk_count, sizeof(void*));
    }
    else
    {
        size_t old_size = sizeof(void*) * (aifc_file->chunk_count - 1);
        size_t new_size = sizeof(void*) * (aifc_file->chunk_count);

        malloc_resize(old_size, (void**)&aifc_file->chunks, new_size);
    }

    aifc_file->chunks[aifc_file->chunk_count - 1] = chunk;

    TRACE_LEAVE(__func__)
}

/**
 * Applies the standard .aifc encode algorithm on incoming sound data and writes it
 * into the {@code struct AdpcmAifcFile} sound chunk.
 * @param aaf: File to write compressed sound data into. Thus must have
 * been previously initialized, sound chunk must be allocated with enough space,
 * and codebook must be setup and loaded.
 * @param sound_data: Incoming sound data. This is uncompressed PCM data, as used
 * by .aiff or .wav sound chunks.
 * @param apc_state: "Adaptive Predictive Coding" state. This contains the data
 * carried forward to encode the next block of audio.
 * @param ssnd_chunk_pos: The current position in {@code aaf} that will be written to.
 * @param sound_data_pos: The current position in {@code sound_data} to be read.
 * @param sound_data_len: Length in bytes of {@code sound_data}.
 * @returns: the number of bytes written.
*/
int AdpcmAifcFile_encode_frame(
    struct AdpcmAifcFile *aaf,
    int16_t *samples_in,
    int32_t *apc_state,
    size_t *ssnd_chunk_pos)
{
    TRACE_ENTER(__func__)

    // validation checks

    if (aaf == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> aaf is NULL\n", __func__, __LINE__);
    }

    if (aaf->codes_chunk == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> aaf->codes_chunk is NULL\n", __func__, __LINE__);
    }

    if (aaf->codes_chunk->coef_table == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> aaf->codes_chunk->coef_table is NULL\n", __func__, __LINE__);
    }

    if (aaf->sound_chunk == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> aaf->sound_chunk is NULL\n", __func__, __LINE__);
    }

    if (samples_in == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> samples_in is NULL\n", __func__, __LINE__);
    }

    if (apc_state == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> apc_state is NULL\n", __func__, __LINE__);
    }

    if (ssnd_chunk_pos == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> ssnd_chunk_pos is NULL\n", __func__, __LINE__);
    }

    if (aaf->sound_chunk->ck_data_size < 8)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> invalid aaf->sound_chunk size: %d\n", __func__, __LINE__, aaf->sound_chunk->ck_data_size);
    }

    if ((*ssnd_chunk_pos + 16) > (size_t)(aaf->sound_chunk->ck_data_size - 8))
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> writing frame would exceed sound chunk length, aaf->sound_chunk->ck_data_size: %d, ssnd_chunk_pos= %ld\n", __func__, __LINE__, aaf->sound_chunk->ck_data_size, *ssnd_chunk_pos);
    }

    // done validating.

    // declare variables

    // convenience pointer
    int32_t ***coefTable;
    // convenience value
    int npredictors;
    // convenience value
    int order;

    // number of bytes written
    int write_len;

    // holds main values during row processing. Allocated, freed.
    int32_t *working;

    // copy of best state seen while evaluating scales.
    int32_t *best_state;

    // points to current row of the `samples_in` buffer.
    int16_t *samples_in_row;

    // look index, current predictor
    int predictor;

    // best predictor seen so far
    int best_predictor;

    // loop value/index.
    int32_t scale;

    // best scale seen so far.
    int best_scale;

    // sum of (error * error)
    float square_error;

    // best (smallest) error seen so far
    float best_square_error;

    // max clip amount seen for this predictor
    int32_t max_clip;

    // best max clip value for all predictors
    int32_t best_max_clip;

    // holds qanitization error for current loop iteration
    int quantize_error;

    // container for prediction values
    int32_t prediction[FRAME_DECODE_BUFFER_LEN];

    // pointer to current row in prediction array
    int32_t *prediction_row;

    // container for prediction errors
    float prediction_error[FRAME_DECODE_BUFFER_LEN];

    // pointer to current row in the prediction error buffer
    float *error_row;

    // holds main output values during final row processing while iterating scales.
    int32_t working_encode[FRAME_DECODE_BUFFER_LEN];

    // holds best encoded value seen so far while iterating scales
    int32_t best_encode[FRAME_DECODE_BUFFER_LEN];

    // pointer to current row in working encode buffer
    int32_t *working_encode_row;

    // frame buffer row index
    int fbri;

    uint8_t header;
    uint8_t c;
    int i;

    // done declaring variables

    // copy from aaf for convenience
    order = aaf->codes_chunk->order;
    npredictors = aaf->codes_chunk->nentries;
    coefTable = aaf->codes_chunk->coef_table;

    memset(prediction, 0, FRAME_DECODE_ROW_LEN * sizeof(int32_t));

    working = (int32_t *)malloc_zero(order + FRAME_DECODE_ROW_LEN, sizeof(int32_t));
    best_state = (int32_t *)malloc_zero(order, sizeof(int32_t));

    best_square_error = UINT32_MAX;
    best_max_clip = INT32_MAX;
    scale = 0;
    best_scale = -1;
    best_predictor = 0;
    write_len = 0;

    /**
     * Phase 1:
     * Iterate the predictors and find the one that generates the least square error.
    */
    if (npredictors > 1)
    {
        for (predictor = 0; predictor < npredictors; predictor++)
        {
            // The first `order` bytes are feedback from the previous frame.
            for (i = 0; i < order; i++)
            {
                working[i] = apc_state[i];
            }

            // reset to starting row.
            prediction_row = &prediction[0];
            samples_in_row = &samples_in[0];
            error_row = &prediction_error[0];

            // reset variables / buffers
            memset(prediction_error, 0, FRAME_DECODE_BUFFER_LEN * sizeof(float));
            memset(prediction, 0, FRAME_DECODE_BUFFER_LEN * sizeof(int32_t));
            square_error = 0.0f;

            // Evaluate the frame, the important part is measuring the error.
            for (fbri=0; fbri<2; fbri++)
            {
                for (i = 0; i < FRAME_DECODE_ROW_LEN; i++)
                {
                    prediction_row[i] = dot_product_i32(coefTable[predictor][i], working, order + i);
                    prediction_row[i] = divide_round_down(prediction_row[i], FRAME_DECODE_SCALE);
                    working[i + order] = samples_in_row[i] - prediction_row[i];
                    error_row[i] = (float) working[i + order];
                    square_error += error_row[i] * error_row[i];
                }

                // Carry forward the feedback
                for (i = 0; i < order; i++)
                {
                    working[i] = prediction_row[FRAME_DECODE_ROW_LEN - order + i] + working[FRAME_DECODE_ROW_LEN + i];
                }

                // advance to next row.
                prediction_row = &prediction_row[FRAME_DECODE_ROW_LEN];
                samples_in_row = &samples_in_row[FRAME_DECODE_ROW_LEN];
                error_row = &error_row[FRAME_DECODE_ROW_LEN];
            }

            /**
             * If this is the best error amount seen so far then
             * mark this predictor to be used.
            */
            if (square_error < best_square_error)
            {
                best_square_error = square_error;
                best_predictor = predictor;
            }
        }
    }

    // declared on the stack, so make sure this is empty.
    memset(best_encode, 0, FRAME_DECODE_BUFFER_LEN * sizeof(int32_t));

    /**
     * Phase 2:
     * Iterate the scales and find the first one that drops quantized
     * error below 2.
    */
    for (scale = 0; scale <= FRAME_ENCODE_MAX_POW_SCALE; scale++)
    {
        // reset to starting row.
        prediction_row = &prediction[0];
        samples_in_row = &samples_in[0];
        error_row = &prediction_error[0];
        working_encode_row = &working_encode[0];

        // reset variables / buffers
        memset(prediction_error, 0, FRAME_DECODE_BUFFER_LEN * sizeof(float));
        memset(prediction, 0, FRAME_DECODE_BUFFER_LEN * sizeof(int32_t));
        memset(working_encode, 0, FRAME_DECODE_BUFFER_LEN * sizeof(int32_t));
        memset(working, 0, (order + FRAME_DECODE_ROW_LEN) * sizeof(int32_t));
        max_clip = 0;

        // The first `order` bytes are feedback from the previous frame.
        for (i = 0; i < order; i++)
        {
            working[i] = apc_state[i];
        }

        /**
         * Evaluate the frame. This time the metric is against the quantized error.
        */
        for (fbri=0; fbri<2; fbri++)
        {
            for (i = 0; i < FRAME_DECODE_ROW_LEN; i++)
            {
                prediction_row[i] = dot_product_i32(coefTable[best_predictor][i], working, order + i);
                prediction_row[i] = divide_round_down(prediction_row[i], FRAME_DECODE_SCALE);
                
                error_row[i] = samples_in_row[i] - prediction_row[i];
                working_encode_row[i] = forward_quantize(error_row[i], 1 << scale);
                quantize_error = (int16_t) clamp(working_encode_row[i], ADPCM_ENCODE_VAL_SIGNED_MIN, ADPCM_ENCODE_VAL_SIGNED_MAX) - working_encode_row[i];
                working_encode_row[i] += quantize_error;
                working[i + order] = working_encode_row[i] * (1 << scale);

                if (max_clip < abs(quantize_error))
                {
                    max_clip = abs(quantize_error);
                }
            }

            // Carry forward the feedback
            for (i = 0; i < order; i++)
            {
                working[i] = prediction_row[FRAME_DECODE_ROW_LEN - order + i] + working[FRAME_DECODE_ROW_LEN + i];
            }

            // advance to next row.
            prediction_row = &prediction_row[FRAME_DECODE_ROW_LEN];
            samples_in_row = &samples_in_row[FRAME_DECODE_ROW_LEN];
            error_row = &error_row[FRAME_DECODE_ROW_LEN];
            working_encode_row = &working_encode_row[FRAME_DECODE_ROW_LEN];
        }

        /**
         * If this is the best error amount seen so far then
         * mark this scale to be used. Save the feedback state
         * and the entire encoded values here, these will then
         * be sent directly to output.
        */
        if (max_clip < best_max_clip)
        {
            best_scale = scale;
            best_max_clip = max_clip;

            for (i = 0; i < order; i++)
            {
                best_state[i] = working[i];
            }

            for (i = 0; i < FRAME_DECODE_BUFFER_LEN; i++)
            {
                best_encode[i] = working_encode[i];
            }
        }

        // Once the error is small enough then exit.
        if (best_max_clip <= 2)
        {
            break;
        }
    }

    /**
     * Copy the best decode state to the In/Out parameter.
    */
    for (i = 0; i < order; i++)
    {
        apc_state[i] = best_state[i];
    }

    if (best_scale < 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> could not determine scale\n", __func__, __LINE__);
    }

    // write header byte.
    header = (uint8_t)(best_scale << 4) | (uint8_t)(best_predictor & 0xf);
    aaf->sound_chunk->sound_data[*ssnd_chunk_pos] = header;
    *ssnd_chunk_pos = *ssnd_chunk_pos + 1;
    write_len++;

    // Write the output bytes.
    for (i = 0; i < FRAME_DECODE_BUFFER_LEN; i += 2)
    {
        c = (uint8_t)(best_encode[i] << 4) | (uint8_t)(best_encode[i + 1] & 0xf);
        aaf->sound_chunk->sound_data[*ssnd_chunk_pos] = c;
        *ssnd_chunk_pos = *ssnd_chunk_pos + 1;
        write_len++;
    }

    // cleanup
    free(working);
    free(best_state);

    TRACE_LEAVE(__func__)
    return write_len;
}

/**
 * Applies the standard .aifc decode algorithm from the sound chunk and writes
 * result to the frame buffer.
 * @param aaf: container file.
 * @param frame_buffer: standard frame buffer, two rows of 8.
 * @param ssnd_chunk_pos: in/out paramter. Current byte position within the sound chunk. If not
 * {@code eof} then will be set to next byte position.
 * @param end_of_ssnd: out parameter. If {@code ssnd_chunk_pos} is less than the size of the sound data
 * in the sound chunk this is set to 1. Otherwise set to zero.
*/
void AdpcmAifcFile_decode_frame(struct AdpcmAifcFile *aaf, int32_t *frame_buffer, size_t *ssnd_chunk_pos, int *end_of_ssnd)
{
    TRACE_ENTER(__func__)

    // which coef_table to use
    int32_t table_index;

    // how much to re-scale each input value
    int32_t scale;

    size_t convl_size;

    // one row, length is `order` + FRAME_DECODE_ROW_LEN
    int32_t *convl_frame;
    int convl_position;
    
    int order;

    uint8_t frame_header;
    uint8_t c;

    int i;

    int32_t *frame_buffer_feedback;
    int32_t *frame_buffer_row;
    
    // which row in frame buffer, ~ frame buffer row index
    int fbri;

    if (aaf == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: aaf is NULL\n", __func__);
    }

    if (aaf->codes_chunk == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s:  aaf->codes_chunk is NULL\n", __func__);
    }

    // grab order from file for convenience
    order = aaf->codes_chunk->order;

    convl_size = order + FRAME_DECODE_ROW_LEN;
    convl_frame = (int32_t *)malloc_zero(1, convl_size * sizeof(int32_t));

    /**
     * The last `order` bytes written to the frame buffer are carried forward
     * in the decode algorithm below. The frame buffer is essentially
     * two rows, so this will get updated to the end of the first row once the first
     * row is written. But for starting conditions, this has to be last
     * bytes of the entire frame_buffer since that was written last.
    */
    frame_buffer_feedback = &frame_buffer[FRAME_DECODE_BUFFER_LEN - order]; // total frame buffer len

    /**
     * Pointer to current row in output frame buffer. Start at the first row,
     * this will advance through the frame_buffer while processing.
    */
    frame_buffer_row = frame_buffer;

    /**
     * The frame header is split into two 4-bit numbers.
     * The upper four bits are exponent (base 2) for scale, as 2^s.
     * The lower four bits are the table in the coefficient table.
    */
    frame_header = get_sound_chunk_byte(aaf, ssnd_chunk_pos, end_of_ssnd);
    scale = 1 << (frame_header >> 4);
    table_index = frame_header & 0xf;

    /**
     * An incoming frame is 9 bytes. It consists of the frame header (above)
     * and eight bytes of data. Each 4-byte chunk will be one row
     * in the output frame_buffer.
    */
    for (fbri=0; fbri<2; fbri++) // loop for each row in the frame_buffer .... is this `order`?
    {
        convl_position = 0;

        /**
         * Copy the feedback state into the next row to be processed.
         * This is written to the first `order` bytes of the row.
        */
        for (i=0; i<order; i++)
        {
            convl_frame[convl_position] = frame_buffer_feedback[i];
            convl_position++;
        }

        /**
         * 4 bytes of input decode into one row of frame_buffer.
         * This will be put into the convl_frame just after the prior
         * state bytes.
        */
        for (i=0; i<4; i++)
        {
            c = get_sound_chunk_byte(aaf, ssnd_chunk_pos, end_of_ssnd);

            /**
             * Two samples were compressed into one byte (4 bits each).
             * These need to be re-scaled by the scale factor defined in the frame header.
             * These were stored signed, 0-7 is positive, 8-15 corresponds to -8 through -1.
             * This splits the compressed byte back into two samples
             * in big endian format.
            */
            convl_frame[convl_position] = c >> 4; // upper
            if (convl_frame[convl_position] > ADPCM_ENCODE_VAL_SIGNED_MAX)
            {
                convl_frame[convl_position] -= ADPCM_ENCODE_VAL_RANGE;
            }
            convl_frame[convl_position] *= scale;
            convl_position++;
            
            convl_frame[convl_position] = c & 0xf; // lower
            if (convl_frame[convl_position] > ADPCM_ENCODE_VAL_SIGNED_MAX)
            {
                convl_frame[convl_position] -= ADPCM_ENCODE_VAL_RANGE;
            }
            convl_frame[convl_position] *= scale;
            convl_position++;
        }

        /**
         * Compute the frame_buffer row.
         * The i'th column in the frame_buffer is the convl_frame (dot product) i'th coef_table row
        */
        for (i=0; i<FRAME_DECODE_ROW_LEN; i++)
        {
            frame_buffer_row[i] = dot_product_i32(aaf->codes_chunk->coef_table[table_index][i], convl_frame, convl_size);
            frame_buffer_row[i] = divide_round_down(frame_buffer_row[i], FRAME_DECODE_SCALE);
        }

        // Set pointer for the feedback state for next iteration.
        // This needs to point to the end of the row just written above.
        frame_buffer_feedback = &frame_buffer_row[FRAME_DECODE_ROW_LEN - order]; // row len

        // Advance to next row in frame buffer.
        frame_buffer_row = &frame_buffer_row[FRAME_DECODE_ROW_LEN];
    }

    free(convl_frame);

    TRACE_LEAVE(__func__)
}

/**
 * Creates new {@code struct AdpcmAifcCommChunk} from aifc file contents.
 * @param fi: aifc file. Reads from current seek position.
 * @param ck_data_size: chunk size in bytes.
 * @returns: pointer to new common chunk.
*/
static struct AdpcmAifcCommChunk *AdpcmAifcCommChunk_new_from_file(struct file_info *fi, int32_t ck_data_size)
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
static struct AdpcmAifcApplicationChunk *AdpcmAifcApplicationChunk_new_from_file(struct file_info *fi, int32_t ck_data_size)
{
    TRACE_ENTER(__func__)

    if (ck_data_size - (5 + ADPCM_AIFC_VADPCM_APPL_NAME_LEN) <= 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s: Invalid APPL chunk data size: %d\n", __func__, ck_data_size);
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

            // AL_RAW16_WAVE has empty state
            file_info_fread(fi, p->loop_data[i].state, ADPCM_AIFC_LOOP_STATE_LEN, 1);
        }
    }
    else
    {
        // no terminating zero, requires explicit length
        stderr_exit(EXIT_CODE_GENERAL, "%s: Unsupported APPL chunk: %.*s\n", __func__, ADPCM_AIFC_VADPCM_APPL_NAME_LEN, code_string);
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
static struct AdpcmAifcSoundChunk *AdpcmAifcSoundChunk_new_from_file(struct file_info *fi, int32_t ck_data_size)
{
    TRACE_ENTER(__func__)

    struct AdpcmAifcSoundChunk *p = (struct AdpcmAifcSoundChunk *)malloc_zero(1, sizeof(struct AdpcmAifcSoundChunk));
    p->ck_id = ADPCM_AIFC_SOUND_CHUNK_ID;
    p->ck_data_size = ck_data_size;

    if (ck_data_size - 8 <= 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s: Invalid SSND chunk data size: %d\n", __func__, ck_data_size);
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