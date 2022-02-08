#ifndef _GAUDIO_ADPCM_H_
#define _GAUDIO_ADPCM_H_

#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>

#include "naudio.h"

/**
 * This file contains structs and defines for supporting .aifc audio files.
*/

/**
 * aifc file, root chunk fourcc id.
*/
#define ADPCM_AIFC_FORM_CHUNK_ID 0x464F524D /* 0x464F524D = "FORM" */

/**
 * aifc file, root chunk type.
*/
#define ADPCM_AIFC_FORM_TYPE_ID 0x41494643 /* 0x41494643 = "AIFC" */

/**
 * aifc file, sound chunk fourcc id.
*/
#define ADPCM_AIFC_SOUND_CHUNK_ID 0x53534E44 /* 0x53534E44 = "SSND" */

/**
 * aifc file, application chunk fourcc id.
*/
#define ADPCM_AIFC_APPLICATION_CHUNK_ID 0x4150504C /* 0x4150504C = "APPL" */

/**
 * aifc file, application chunk signature.
*/
#define ADPCM_AIFC_APPLICATION_SIGNATURE 0x73746F63 /* 0x73746F63 = "stoc" */

/**
 * aifc file, common chunk fourcc id.
*/
#define ADPCM_AIFC_COMMON_CHUNK_ID 0x434F4D4D /* 0x434F4D4D = "COMM" */

/**
 * aifc file, common chunk, "VAPC" compression type (AL_ADPCM_WAVE).
*/
#define ADPCM_AIFC_VAPC_COMPRESSION_TYPE_ID 0x56415043 /* 0x56415043 = "VAPC" */

/**
 * aifc file, common chunk, type when no compression is used (AL_RAW16_WAVE).
*/
#define ADPCM_AIFC_NONE_COMPRESSION_TYPE_ID 0x4E4F4E45 /* 0x4E4F4E45 = "NONE" */

/**
 * aifc file, common chunk, compression name.
 * There is no terminating '\0'.
*/
#define ADPCM_AIFC_VADPCM_COMPRESSION_NAME "VADPCM ~4-1"

/**
 * Length of ADPCM_AIFC_VADPCM_COMPRESSION_NAME.
 * There is no terminating '\0'.
*/
#define ADPCM_AIFC_VADPCM_COMPRESSION_NAME_LEN 11

/**
 * aifc file, common chunk, compression name.
 * There is no terminating '\0'.
 * ref: http://www-mmsp.ece.mcgill.ca/Documents/AudioFormats/AIFF/AIFF.html
*/
#define ADPCM_AIFC_NONE_COMPRESSION_NAME "not compressed"

/**
 * Length of ADPCM_AIFC_NONE_COMPRESSION_NAME.
 * There is no terminating '\0'.
*/
#define ADPCM_AIFC_NONE_COMPRESSION_NAME_LEN 14

/**
 * Length of static char array to hold compression name.
 * Should be max of any possible names above.
*/
#define ADPCM_AIFC_COMPRESSION_NAME_ARR_LEN 14

/**
 * aifc file, application chunk, codebook chunk/name (code string).
 * There is no terminating '\0'.
*/
#define ADPCM_AIFC_VADPCM_CODES_NAME "VADPCMCODES"

/**
 * aifc file, application chunk, loop chunk/name (code string).
 * There is no terminating '\0'.
*/
#define ADPCM_AIFC_VADPCM_LOOPS_NAME "VADPCMLOOPS"

/**
 * Length of application chunk type name  (code string).
 * e.g., length of ADPCM_AIFC_VADPCM_CODES_NAME
 * There is no terminating '\0'.
*/
#define ADPCM_AIFC_VADPCM_APPL_NAME_LEN 11

/**
 * Size in bytes of application chunk, loop type, state container.
*/
#define ADPCM_AIFC_LOOP_STATE_LEN 0x20

/**
 * Number of elements in frame buffer to decompress audio.
*/
#define FRAME_DECODE_BUFFER_LEN 16

#define FRAME_DECODE_ROW_LEN 8

#define FRAME_DECODE_SCALE (1 << 11) /* 2048 */

/**
 * Loops version number, required to be 1 by some applications.
*/
#define ADPCM_AIFC_VADPCM_LOOP_VERSION 1

/**
 * The number of bytes written to output wav file,
 * for each input data.
*/
#define ADPCM_WAV_OUTPUT_SAMPLE_NUM_BYTES 2

/**
 * Size in bytes of each element in the loop state array.
*/
#define ADPCM_LOOP_STATE_ELEMENT_SIZE 2

#define ADPCM_ENCODE_VAL_RANGE       16
#define ADPCM_ENCODE_VAL_SIGNED_MAX  ((ADPCM_ENCODE_VAL_RANGE >> 1) - 1) /* = 7 */

/**
 * aifc container for sound chunk.
*/
struct AdpcmAifcSoundChunk {
    /* begin file format (write elements to disk in order declared according to endianess) */

    /**
     * Chunk id.
     * big endian.
    */
    uint32_t ck_id;
    
    /**
     * Chunk size.
     * big endian.
    */
    int32_t ck_data_size;

    /**
     * Where the first sample frame in the sound_data starts.
     * Should always be zero.
     * big endian.
    */
    uint32_t offset;

    /**
     * Size in bytes of the blocks that sound data is aligned to.
     * Should always be zero.
     * big endian.
    */
    uint32_t block_size;

    /**
     * Data.
    */
    uint8_t *sound_data;

    /* end file format ------------------------------------------------------------------- */
};

/**
 * Shared base object for application type chunks.
*/
struct AdpcmAifcApplicationChunk {
    /* begin file format (write elements to disk in order declared according to endianess) */

    /**
     * Chunk id.
     * big endian.
    */
    uint32_t ck_id;
    
    /**
     * Chunk size.
     * big endian.
    */
    int32_t ck_data_size;

    /**
     * Application chunk signature.
     * Should always be ADPCM_AIFC_APPLICATION_SIGNATURE.
     * big endian.
    */
    uint32_t application_signature;

    /**
     * Unknown / undocumented byte value.
     * Always seems to be 0xb.
    */
    uint8_t unknown;

    /**
     * Application type string.
     * no terminating '\0'
    */
    char code_string[ADPCM_AIFC_VADPCM_APPL_NAME_LEN];

    /* end file format ------------------------------------------------------------------- */
};

/**
 * aifc container for codebook chunk.
 * Defines predictors used to decode compressed ADPCM data.
 * Extends AdpcmAifcApplicationChunk.
*/
struct AdpcmAifcCodebookChunk {
    /* begin file format (write elements to disk in order declared according to endianess) */

    struct AdpcmAifcApplicationChunk base;

    /**
     * Version.
     * Should always be one.
     * big endian.
    */
    uint16_t version;

    /**
     * ADPCM predictor order.
     * Should always be two.
     * big endian.
    */
    int16_t order;

    /**
     * aka npredictors.
     * Anything from 1 to 8.
     * big endian.
    */
    uint16_t nentries;

    /**
     * raw codebook data.
     * length of the tableData field is order*nEntries*16 bytes.
    */
    uint8_t *table_data;

    /* end file format ------------------------------------------------------------------- */

    /**
     * parsed and decoded table_data will be loaded into the coef_table.
    */
    int32_t ***coef_table;
};

/**
 * Describes loop element stored in loop_data array in loop chunk.
 * 
 * Loop data is used to repeat a section of a sound. Only one loop
 * is supported, so this is most commonly used to explain how to repeat a "tail"
 * section of the sound so the sound can be played indefinitely.
 * 
 * The n64 programming manual says:
 *     "state defines the internal state of the ADPCM decoder at
 *     the start of the loop and is necessary for smooth playback
 *     across the loop point. The start and end values are represented
 *     in number of samples. Count defines the number of times the loop
 *     is played before the sound completes. Setting count to -1
 *     indicates that the loop should play indefinitely."
*/
struct AdpcmAifcLoopData {
    /* begin file format (write elements to disk in order declared according to endianess) */

    /**
     * Index of start sample loop should begin at.
     * big endian.
    */
    int32_t start;

    /**
     * Index of end sample loop should end at.
     * big endian.
    */
    int32_t end;

    /**
     * Number of times the loop should play. -1 indicates infinite.
     * big endian.
    */
    int32_t count;

    /**
     * Internal state of ADPCM decoder.
    */
    uint8_t state[ADPCM_AIFC_LOOP_STATE_LEN];

    /* end file format ------------------------------------------------------------------- */
};

/**
 * aifc container for loop chunk.
 * Contains information necessary to allow the ADPCM decompresser to loop a sound.
 * Extends AdpcmAifcApplicationChunk.
*/
struct AdpcmAifcLoopChunk {
    /* begin file format (write elements to disk in order declared according to endianess) */

    struct AdpcmAifcApplicationChunk base;

    /**
     * Version.
     * Should always be one.
     * big endian.
    */
    uint16_t version;

    /**
     * Number of loops, and length of loop_data.
     * The n64 programming manual says "only one loop point can be specified".
     * big endian.
    */
    int16_t nloops;

    /**
     * Array of loop datas.
    */
    struct AdpcmAifcLoopData *loop_data;

    /* end file format ------------------------------------------------------------------- */
};

/**
 * aifc container for common chunk.
*/
struct AdpcmAifcCommChunk {
    /* begin file format (write elements to disk in order declared according to endianess) */

    /**
     * Chunk id.
     * big endian.
    */
    uint32_t ck_id;

    /**
     * Chunk size.
     * big endian.
    */
    int32_t ck_data_size;

    /**
     * Number of channels. The n64 programming manual says only a single channel
     * is supported.
     * big endian.
    */
    int16_t num_channels;

    /**
     * Number of sample frames, maybe.
     * The programming manual says, "The numSampleFrames field should be set to the number of bytes
     * represented by the compressed data, not the the number of bytes used." That is, the uncompressed
     * audio size.
     * big endian.
    */
    uint32_t num_sample_frames;

    /**
     * Sample size in bits.
     * Should always be 16.
     * big endian.
    */
    int16_t sample_size;

    /**
     * Sample rate, as an 80 bit "extended" float.
     * big endian. 
    */
    uint8_t sample_rate[10];

    /**
     * Compression type used by the audio.
     * Audio compressed with the Nintendo tools should be ADPCM_AIFC_VAPC_COMPRESSION_TYPE_ID.
     * big endian. Uncompressed audio should be ADPCM_AIFC_NONE_COMPRESSION_TYPE_ID.
    */
    uint32_t compression_type;

    /**
     * Unknown / undocumented byte value.
     * Always seems to be 0xb.
    */
    uint8_t unknown;

    /**
     * Name of compression used. Note, n64 programming manual says
     * "VADPCM ~ 4:1" but actual value used is
     * "VADPCM ~4-1".
     * no terminating '\0'
    */
    char compression_name[ADPCM_AIFC_COMPRESSION_NAME_ARR_LEN];

    /* end file format ------------------------------------------------------------------- */
};

/**
 * Base container for aifc file.
*/
struct AdpcmAifcFile {
    /* begin file format (write elements to disk in order declared according to endianess) */

    /**
     * Chunk id.
     * Always ADPCM_AIFC_FORM_CHUNK_ID.
     * big endian.
    */
    uint32_t ck_id;

    /**
     * Chunk size.
     * big endian.
    */
    int32_t ck_data_size;

    /**
     * aifc file form type.
     * Always ADPCM_AIFC_FORM_TYPE_ID.
     * big endian.
    */
    int32_t form_type;

    /* end file format ------------------------------------------------------------------- */

    /**
     * Number of chunks in the file,
     * and length of `chunks` array.
    */
    int32_t chunk_count;

    /**
     * Array of pointers to each chunk.
    */
    void **chunks;

    /**
     * Convenience pointer to the only/last common chunk.
     * Should always exist.
    */
    struct AdpcmAifcCommChunk *comm_chunk;

    /**
     * Convenience pointer to the only/last codebook chunk.
     * Only set for AL_ADPCM_WAVE.
    */
    struct AdpcmAifcCodebookChunk *codes_chunk;

    /**
     * Convenience pointer to the only/last sound chunk.
     * Should always exist.
    */
    struct AdpcmAifcSoundChunk *sound_chunk;

    /**
     * Convenience pointer to the only/last loop chunk.
     * May be NULL.
    */
    struct AdpcmAifcLoopChunk *loop_chunk;
};

extern int g_AdpcmLoopInfiniteExportCount;

struct AdpcmAifcFile *AdpcmAifcFile_new_simple(size_t chunk_count);
struct AdpcmAifcFile *AdpcmAifcFile_new_from_file(struct file_info *fi);
struct AdpcmAifcCommChunk *AdpcmAifcCommChunk_new(uint32_t compression_type);
struct AdpcmAifcCodebookChunk *AdpcmAifcCodebookChunk_new(int16_t order, uint16_t nentries);
struct AdpcmAifcSoundChunk *AdpcmAifcSoundChunk_new(size_t sound_data_size_bytes);
struct AdpcmAifcLoopChunk *AdpcmAifcLoopChunk_new(void);
void AdpcmAifcCommChunk_fwrite(struct AdpcmAifcCommChunk *chunk, struct file_info *fi);
void AdpcmAifcApplicationChunk_fwrite(struct AdpcmAifcApplicationChunk *chunk, struct file_info *fi);
void AdpcmAifcCodebookChunk_decode_aifc_codebook(struct AdpcmAifcCodebookChunk *chunk);
void AdpcmAifcCodebookChunk_fwrite(struct AdpcmAifcCodebookChunk *chunk, struct file_info *fi);
void AdpcmAifcSoundChunk_fwrite(struct AdpcmAifcSoundChunk *chunk, struct file_info *fi);
void AdpcmAifcLoopData_fwrite(struct AdpcmAifcLoopData *loop, struct file_info *fi);
void AdpcmAifcLoopChunk_fwrite(struct AdpcmAifcLoopChunk *chunk, struct file_info *fi);
void AdpcmAifcFile_fwrite(struct AdpcmAifcFile *aaf, struct file_info *fi);
size_t AdpcmAifcFile_decode(struct AdpcmAifcFile *aaf, uint8_t *buffer, size_t max_len);
int32_t AdpcmAifcFile_get_int_sample_rate(struct AdpcmAifcFile *aaf);
void AdpcmAifcCommChunk_free(struct AdpcmAifcCommChunk *chunk);
void AdpcmAifcSoundChunk_free(struct AdpcmAifcSoundChunk *chunk);
void AdpcmAifcCodebookChunk_free(struct AdpcmAifcCodebookChunk *chunk);
void AdpcmAifcLoopChunk_free(struct AdpcmAifcLoopChunk *chunk);
void AdpcmAifcFile_free(struct AdpcmAifcFile *aifc_file);
size_t AdpcmAifcFile_estimate_inflate_size(struct AdpcmAifcFile *aifc_file);

size_t AdpcmAifcFile_path_write_tbl(char *path, struct file_info *fi, size_t *sound_data_len);
size_t AdpcmAifcFile_write_tbl(struct AdpcmAifcFile *aifc_file, struct file_info *fi, size_t *sound_data_len);

// Exposed publicly for testing.

void AdpcmAifcFile_decode_frame(struct AdpcmAifcFile *aaf, int32_t *frame_buffer, size_t *ssnd_chunk_pos, int *end_of_ssnd);

#endif