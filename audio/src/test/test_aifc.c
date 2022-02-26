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
            printf("%s %d> fail\n", __func__, __LINE__);
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
        free(frame_buffer);

        if (check == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d> fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("aifc test: AdpcmAifcFile_encode_frame\n");
        *run_count = *run_count + 1;
        int check = 0;
        int single_check;
        int i;

        int32_t *apc_state = (int32_t *)malloc_zero(2, sizeof(int32_t));
        size_t ssnd_chunk_pos = 0;
        size_t ssnd_chunk_starting_pos = 0;

        struct AdpcmAifcFile *aaf = AdpcmAifcFile_new_simple(2);
        struct AdpcmAifcSoundChunk *snd = AdpcmAifcSoundChunk_new(128);
        struct AdpcmAifcCodebookChunk *codes = AdpcmAifcCodebookChunk_new(2, 4);

        aaf->chunks[0] = snd;
        aaf->chunks[1] = codes;
        aaf->sound_chunk = snd;
        aaf->codes_chunk = codes;

        uint8_t raw_coef[] = {
            0x00, 0x30, 0xFF, 0xDC, 0x00, 0x1C, 0xFF, 0xEA,
            0x00, 0x12, 0xFF, 0xF2, 0x00, 0x0B, 0xFF, 0xF7,

            0xF9, 0xF1, 0x04, 0xC7, 0xFC, 0x3E, 0x02, 0xF5,
            0xFD, 0xAC, 0x01, 0xD5, 0xFE, 0x8F, 0x01, 0x23,

            0xF9, 0x46, 0xF4, 0x8E, 0xF2, 0x2C, 0xF2, 0x17,
            0xF3, 0xF2, 0xF7, 0x2D, 0xFB, 0x1E, 0xFF, 0x1B,

            0x0D, 0x9E, 0x10, 0x73, 0x10, 0x8D, 0x0E, 0x57,
            0x0A, 0x7F, 0x05, 0xCF, 0x01, 0x10, 0xFC, 0xED,

            0x01, 0x82, 0x01, 0x1E, 0x01, 0x1D, 0x01, 0x09,
            0x00, 0xFA, 0x00, 0xEB, 0x00, 0xDD, 0x00, 0xD0,

            0x05, 0xED, 0x05, 0xE6, 0x05, 0x7C, 0x05, 0x2D,
            0x04, 0xDE, 0x04, 0x95, 0x04, 0x50, 0x04, 0x0F,

            0xF9, 0x2A, 0xF3, 0x8E, 0xEF, 0x2F, 0xEC, 0x06,
            0xE9, 0xFF, 0xE9, 0x04, 0xE8, 0xF5, 0xE9, 0xB0,

            0x0E, 0x90, 0x13, 0xAE, 0x17, 0x61, 0x19, 0xC0,
            0x1A, 0xE6, 0x1A, 0xF8, 0x1A, 0x1C, 0x18, 0x7D
        };

        memcpy(codes->table_data, raw_coef, 8*16);

        AdpcmAifcCodebookChunk_decode_aifc_codebook(codes);

        uint8_t sound_data_raw[] = { 
            0xed, 0x15, 0xef, 0xc0, 0xf2, 0xc5, 0xf6, 0x2a,
            0xf9, 0x0c, 0xfb, 0x0b, 0xfc, 0x8c, 0xfd, 0xb0,
            0xfe, 0x35, 0xfd, 0xa0, 0xfb, 0xa7, 0xf8, 0x6e,
            0xf4, 0xf6, 0xf1, 0xe5, 0xee, 0xed, 0xeb, 0xbe
        };
        size_t sound_data_raw_len = 32;
        size_t sound_data_pos = 0;

        int16_t samples_in[FRAME_DECODE_BUFFER_LEN];
        memset(samples_in, 0, FRAME_DECODE_BUFFER_LEN * sizeof(int16_t));

        fill_16bit_buffer(
                samples_in,
                FRAME_DECODE_BUFFER_LEN,
                sound_data_raw,
                &sound_data_pos,
                sound_data_raw_len);

        bswap16_chunk(samples_in, samples_in, FRAME_DECODE_BUFFER_LEN); // inplace swap is ok

        int32_t starting_apc_state[] = {
            0xffffe7b0, 0xffffe9fc
        };

        memcpy(apc_state, starting_apc_state, 2 * sizeof(int32_t));
        ssnd_chunk_starting_pos = ssnd_chunk_pos;

        // done with setup.

        // printf("apc_state:\n");
        // for (i=0; i<2; i++)
        // {
        //     printf("0x%04x ", (uint16_t)apc_state[i]);
        // }
        // printf("\n");

        AdpcmAifcFile_encode_frame(aaf, samples_in, apc_state, &ssnd_chunk_pos);

        uint8_t expected_ssnd_chunk_data[] = {
            0x63,
            0x2d, 0x11, 0xfd, 0xfe, 0xfb, 0xa9, 0xce, 0xdb
        };

        int32_t expected_apc_state[] = {
            0xffffeed4, 0xffffeba3
        };

        check = 1;

        single_check = ((ssnd_chunk_starting_pos + 9) == ssnd_chunk_pos);
        check &= single_check;

        if (!single_check)
        {
            printf("%s %d> ssnd_chunk_pos mismatch, expected %ld, actual %ld\n", __func__, __LINE__, (ssnd_chunk_starting_pos + 9), ssnd_chunk_pos);
        }

        single_check = 1;
        for (i=0; i<2; i++)
        {
            single_check &= (apc_state[i] == expected_apc_state[i]);
        }
        check &= single_check;

        if (!single_check)
        {
            printf("%s %d> apc_state match failure\n", __func__, __LINE__);

            printf("apc_state:\n");
            for (i=0; i<2; i++)
            {
                printf("0x%04x ", (uint16_t)apc_state[i]);
            }
            printf("\n");
        }

        single_check = 1;
        for (i=0; i<9; i++)
        {
            single_check &= (aaf->sound_chunk->sound_data[ssnd_chunk_starting_pos + i] == expected_ssnd_chunk_data[i]);
        }
        check &= single_check;
        
        if (!single_check)
        {
            printf("%s %d> aaf->sound_chunk->sound_data match failure\n", __func__, __LINE__);

            printf("aaf->sound_chunk->sound_data:\n");
            for (i=0; i<9; i++)
            {
                printf("0x%02x ", (uint8_t)aaf->sound_chunk->sound_data[i]);
            }
            printf("\n");
        }
        
        AdpcmAifcFile_free(aaf);
        free(apc_state);

        if (check == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d> fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }
}