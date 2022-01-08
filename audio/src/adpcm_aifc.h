#ifndef _GAUDIO_ADPCM_H_
#define _GAUDIO_ADPCM_H_

#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>

#include "naudio.h"

#define ADPCM_AIFC_SOUND_CHUNK_ID 0x53534E44 /* 0x53534E44 = "SSND" */
#define ADPCM_AIFC_APPLICATION_CHUNK_ID 0x4150504C /* 0x4150504C = "APPL" */
#define ADPCM_AIFC_APPLICATION_SIGNATURE 0x73746F63 /* 0x73746F63 = "stoc" */
#define ADPCM_AIFC_COMMON_CHUNK_ID 0x434F4D4D /* 0x434F4D4D = "COMM" */
#define ADPCM_AIFC_COMPRESSION_TYPE_ID 0x56415043 /* 0x56415043 = "VAPC" */
#define ADPCM_AIFC_FORM_CHUNK_ID 0x464F524D /* 0x464F524D = "FORM" */
#define ADPCM_AIFC_FORM_TYPE_ID 0x41494643 /* 0x41494643 = "AIFC" */

#define ADPCM_AIFC_VADPCM_CODES_NAME "VADPCMCODES"
#define ADPCM_AIFC_VADPCM_LOOPS_NAME "VADPCMLOOPS"
#define ADPCM_AIFC_VADPCM_COMPRESSION_NAME "VADPCM ~4-1"

#define ADPCM_AIFC_VADPCM_APPL_NAME_LEN 11
#define ADPCM_AIFC_VADPCM_COMPRESSION_NAME_LEN 11
#define ADPCM_AIFC_LOOP_STATE_LEN 0x20

#define FRAME_DECODE_BUFFER_LEN 16

struct AdpcmAifcSoundChunk {
    uint32_t ck_id;
    int32_t ck_data_size;
    uint32_t offset; /* should be 0 */
    uint32_t block_size; /* should be 0 */
    uint8_t *sound_data;
};

struct AdpcmAifcApplicationChunk {
    uint32_t ck_id;
    int32_t ck_data_size;
    uint32_t application_signature;
    uint8_t unknown;
    char code_string[ADPCM_AIFC_VADPCM_APPL_NAME_LEN];
};

struct AdpcmAifcCodebookChunk {
    struct AdpcmAifcApplicationChunk base;
    /* code_string is "VADPCMCODES", no terminating '\0' */
    uint16_t version; /* should be 1 */
    int16_t order;
    uint16_t nentries; /* aka npredictors */
    uint8_t *table_data; /* length of the tableData field is order*nEntries*16 bytes. */

    /**
     * parsed and decoded table_data will be loaded into the coef_table.
    */
    int32_t ***coef_table;
};

struct AdpcmAifcLoopData {
    int32_t start;
    int32_t end;
    int32_t count;
    uint8_t state[ADPCM_AIFC_LOOP_STATE_LEN];
};

struct AdpcmAifcLoopChunk {
    struct AdpcmAifcApplicationChunk base;
    /* code_string is "VADPCMLOOPS", no terminating '\0' */
    uint16_t version; /* should be 1 */
    int16_t nloops;
    struct AdpcmAifcLoopData *loop_data;
};

struct AdpcmAifcCommChunk {
    uint32_t ck_id;
    int32_t ck_data_size;
    int16_t num_channels;
    uint32_t num_sample_frames;
    int16_t sample_size;
    uint8_t sample_rate[10];    /* 80 bit float */
    uint32_t compression_type;
    uint8_t unknown;
    char compression_name[ADPCM_AIFC_VADPCM_COMPRESSION_NAME_LEN];     /* "VADPCM ~4-1", no terminating '\0' */
};

struct AdpcmAifcFile {
    uint32_t ck_id;
    int32_t ck_data_size;
    int32_t form_type;
    int32_t chunk_count;
    void **chunks;

    // convenience pointers
    struct AdpcmAifcCommChunk *comm_chunk;
    struct AdpcmAifcCodebookChunk *codes_chunk;
    struct AdpcmAifcSoundChunk *sound_chunk;
    struct AdpcmAifcLoopChunk *loop_chunk;
};

struct AdpcmAifcFile *AdpcmAifcFile_new_simple(size_t chunk_count);
struct AdpcmAifcFile *AdpcmAifcFile_new_from_file(struct file_info *fi);
struct AdpcmAifcFile *AdpcmAifcFile_new_full(struct ALSound *sound, struct ALBank *bank);
struct AdpcmAifcCommChunk *AdpcmAifcCommChunk_new();
struct AdpcmAifcCodebookChunk *AdpcmAifcCodebookChunk_new(int16_t order, uint16_t nentries);
struct AdpcmAifcSoundChunk *AdpcmAifcSoundChunk_new(size_t sound_data_size_bytes);
struct AdpcmAifcLoopChunk *AdpcmAifcLoopChunk_new();
void load_aifc_from_sound(struct AdpcmAifcFile *aaf, struct ALSound *sound, uint8_t *tbl_file_contents, struct ALBank *bank);
void AdpcmAifcCommChunk_frwrite(struct AdpcmAifcCommChunk *chunk, struct file_info *fi);
void AdpcmAifcApplicationChunk_frwrite(struct AdpcmAifcApplicationChunk *chunk, struct file_info *fi);
void AdpcmAifcCodebookChunk_decode_aifc_codebook(struct AdpcmAifcCodebookChunk *chunk);
void AdpcmAifcCodebookChunk_frwrite(struct AdpcmAifcCodebookChunk *chunk, struct file_info *fi);
void AdpcmAifcSoundChunk_frwrite(struct AdpcmAifcSoundChunk *chunk, struct file_info *fi);
void AdpcmAifcLoopData_frwrite(struct AdpcmAifcLoopData *loop, struct file_info *fi);
void AdpcmAifcLoopChunk_frwrite(struct AdpcmAifcLoopChunk *chunk, struct file_info *fi);
void AdpcmAifcFile_frwrite(struct AdpcmAifcFile *aaf, struct file_info *fi);
void write_sound_to_aifc(struct ALSound *sound, struct ALBank *bank, uint8_t *tbl_file_contents, struct file_info *fi);
void write_bank_to_aifc(struct ALBankFile *bank_file, uint8_t *tbl_file_contents);
size_t AdpcmAifcFile_decode(struct AdpcmAifcFile *aaf, uint8_t *buffer, size_t max_len);
int32_t AdpcmAifcFile_get_int_sample_rate(struct AdpcmAifcFile *aaf);

#endif