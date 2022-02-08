#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "machine_config.h"
#include "debug.h"
#include "common.h"
#include "gaudio_math.h"
#include "utility.h"
#include "llist.h"
#include "string_hash.h"
#include "int_hash.h"
#include "md5.h"
#include "naudio.h"
#include "test_common.h"
#include "adpcm_aifc.h"

void aifc_all(int *run_count, int *pass_count, int *fail_count)
{
    {
        printf("aifc test: AdpcmAifcCodebookChunk_decode_aifc_codebook\n");
        *run_count = *run_count + 1;
        int check = 0;
        int i,j;

        struct AdpcmAifcCodebookChunk *chunk = AdpcmAifcCodebookChunk_new(2, 1);

        uint8_t raw_coef[] = {
            0xFA, 0xE2, 0xFA, 0xD0, 0xFE, 0x04, 0x01, 0x4F,
            0x02, 0x98, 0x01, 0xCB, 0x00, 0x29, 0xFF, 0x03,
            0x08, 0x1C, 0x03, 0x1A, 0xFD, 0xF5, 0xFB, 0xF2,
            0xFD, 0x32, 0xFF, 0xC1, 0x01, 0x8B, 0x01, 0xB9
        };

        memcpy(chunk->table_data, raw_coef, 32);

        AdpcmAifcCodebookChunk_decode_aifc_codebook(chunk);

        uint16_t expected[] = {
            0xFAE2, 0x081C, 0x0800, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000,
            0xFAD0, 0x031A, 0x081C, 0x0800, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000,
            0xFE04, 0xFDF5, 0x031A, 0x081C, 0x0800, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000,
            0x014F, 0xFBF2, 0xFDF5, 0x031A, 0x081C, 0x0800, 0x0000, 0x0000, 0x0000, 0x0000,
            0x0298, 0xFD32, 0xFBF2, 0xFDF5, 0x031A, 0x081C, 0x0800, 0x0000, 0x0000, 0x0000,
            0x01CB, 0xFFC1, 0xFD32, 0xFBF2, 0xFDF5, 0x031A, 0x081C, 0x0800, 0x0000, 0x0000,
            0x0029, 0x018B, 0xFFC1, 0xFD32, 0xFBF2, 0xFDF5, 0x031A, 0x081C, 0x0800, 0x0000,
            0xFF03, 0x01B9, 0x018B, 0xFFC1, 0xFD32, 0xFBF2, 0xFDF5, 0x031A, 0x081C, 0x0800
        };

        check = 1;

        for (i=0; i<8; i++)
        {
            for (j=0; j<10; j++)
            {
                check &= ((uint16_t)(chunk->coef_table[0][i][j]) == expected[i*10+j]);
            }
        }
        
        AdpcmAifcCodebookChunk_free(chunk);

        if (check == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d>fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("aifc test: AdpcmAifcFile_decode_frame\n");
        *run_count = *run_count + 1;
        int check = 0;
        int i;

        int32_t *frame_buffer = (int32_t *)malloc_zero(FRAME_DECODE_BUFFER_LEN, sizeof(int32_t));
        size_t ssnd_chunk_pos = 0;
        int end_of_ssnd = 0;

        struct AdpcmAifcFile *aaf = AdpcmAifcFile_new_simple(2);
        struct AdpcmAifcSoundChunk *snd = AdpcmAifcSoundChunk_new(32);
        struct AdpcmAifcCodebookChunk *codes = AdpcmAifcCodebookChunk_new(2, 1);

        aaf->chunks[0] = snd;
        aaf->chunks[1] = codes;
        aaf->sound_chunk = snd;
        aaf->codes_chunk = codes;

        uint8_t raw_coef[] = {
            0xFA, 0xE2, 0xFA, 0xD0, 0xFE, 0x04, 0x01, 0x4F,
            0x02, 0x98, 0x01, 0xCB, 0x00, 0x29, 0xFF, 0x03,
            0x08, 0x1C, 0x03, 0x1A, 0xFD, 0xF5, 0xFB, 0xF2,
            0xFD, 0x32, 0xFF, 0xC1, 0x01, 0x8B, 0x01, 0xB9
        };

        memcpy(codes->table_data, raw_coef, 32);

        AdpcmAifcCodebookChunk_decode_aifc_codebook(codes);

        uint8_t sound_data[] = { 
            0xb0,
            0xca,0xbb,0xcc,0x11,0x35,0x34,0xee,0xe3,
        };

        memcpy(snd->sound_data, sound_data, 9);

        // only the last two entries in the frame_buffer are carried forward.
        int32_t starting_frame_buffer[] = {
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000, 
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0xffffd7e1, 0xffffbe7b
        };

        memcpy(frame_buffer, starting_frame_buffer, 16 * sizeof(int32_t));

        // done with setup.

        AdpcmAifcFile_decode_frame(aaf, frame_buffer, &ssnd_chunk_pos, &end_of_ssnd);

        int32_t expected[] = {
            0xffffb73f, 0xffffb02d, 0xffffb59f, 0xffffbfa9,
            0xffffce62, 0xffffd6d7, 0xfffffe04, 0x00002053,
            0x00003a09, 0x00004e25, 0x00004216, 0x000030ff,
            0xfffff763, 0xffffc7ee, 0xffffbcac, 0xfffff7a2
        };

        check = 1;

        for (i=0; i<16; i++)
        {
            check &= (frame_buffer[i] == expected[i]);
        }
        
        AdpcmAifcFile_free(aaf);

        if (check == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d>fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }
}