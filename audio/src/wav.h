#ifndef _GAUDIO_WAV_H_
#define _GAUDIO_WAV_H_

#include <stdint.h>

/**
 * This file contains structs and defines for supporting .wav audio files.
*/

/**
 * Default extension when creating a wav file.
*/
#define WAV_DEFAULT_EXTENSION ".wav"

/**
 * wav file, root chunk fourcc id.
*/
#define WAV_RIFF_CHUNK_ID 0x52494646 /* 0x52494646 = "RIFF" */

/**
 * wav file, root chunk type id.
*/
#define WAV_RIFF_TYPE_ID 0x57415645 /* 0x57415645 = "WAVE" */

/**
 * wav file, "fmt " chunk fourcc id.
*/
#define WAV_FMT_CHUNK_ID 0x666d7420 /* 0x666d7420 = "fmt " */

/**
 * wav file, "data" chunk fourcc id.
*/
#define WAV_DATA_CHUNK_ID 0x64617461 /* 0x64617461 = "data" */

/**
 * This library tracks pointers to the chunks in the file.
 * When creating a new file, this is the default number
 * of pointer chunks to allocate.
*/
#define WAV_DEFAULT_NUM_CHUNKS 2

/**
 * wav file, size of "fmt " chunk without ck_id or ck_data_size.
*/
#define WAV_FMT_CHUNK_BODY_SIZE 16

/**
 * wav file, size of "fmt " chunk including ck_id or ck_data_size.
*/
#define WAV_FMT_CHUNK_FULL_SIZE (4 + 4 + WAV_FMT_CHUNK_BODY_SIZE)

/**
 * wav file default audio format
*/
#define WAV_AUDIO_FORMAT 1 /* PCM (uncompressed) */

/**
 * wav file container for "data" chunk.
*/
struct WavDataChunk {
    /* begin file format (write elements to disk in order declared according to endianess) */

    /**
     * Chunk id.
     * big endian.
    */
    uint32_t ck_id;

    /**
     * Chunk size.
     * little endian.
    */
    int32_t ck_data_size;

    /**
     * Pointer to sound data of chunk.
    */
    uint8_t *data;

    /* end file format ------------------------------------------------------------------- */
};

/**
 * wav file container for "fmt " chunk.
*/
struct WavFmtChunk {
    /* begin file format (write elements to disk in order declared according to endianess) */

    /**
     * Chunk id.
     * big endian.
    */
    uint32_t ck_id;

    /**
     * Chunk size.
     * little endian.
    */
    int32_t ck_data_size;

    /**
     * Audio format.
     * currently only WAV_AUDIO_FORMAT is supported.
     * little endian.
    */
    int16_t audio_format;

    /**
     * Number of channels.
     * currently only mono (1 channel) is supported.
     * little endian.
    */
    int16_t num_channels;

    /**
     * Sample rate, aka frequency.
     * little endian.
    */
    int32_t sample_rate;

    /**
     * Byte rate, equal to sample_rate * num_channels * bits_per_sample/8
     * little endian.
    */
    int32_t byte_rate;

    /**
     * block alignment, equal to num_channels * bits_per_sample/8.
     * little endian.
    */
    int16_t block_align;

    /**
     * Bits per sample.
     * This should be DEFAULT_SAMPLE_SIZE.
     * little endian.
    */
    int16_t bits_per_sample;

    /* end file format ------------------------------------------------------------------- */
};

/**
 * Base container for wav file.
*/
struct WavFile {
    /* begin file format (write elements to disk in order declared according to endianess) */

    /**
     * Chunk id.
     * big endian.
    */
    uint32_t ck_id;

    /**
     * Chunk size.
     * little endian.
    */
    int32_t ck_data_size;

    /**
     * form type.
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
     * Convenience pointer to the only/last "fmt " chunk.
    */
    struct WavFmtChunk *fmt_chunk;

    /**
     * Convenience pointer to the only/last "data" chunk.
    */
    struct WavDataChunk *data_chunk;
};

struct WavDataChunk *WavDataChunk_new();
struct WavFmtChunk *WavFmtChunk_new();
struct WavFile *WavFile_new(size_t num_chunks);
struct WavFile *WavFile_load_from_aifc(struct AdpcmAifcFile *aifc_file);
void WavDataChunk_fwrite(struct WavDataChunk *chunk, struct file_info *fi);
void WavFmtChunk_fwrite(struct WavFmtChunk *chunk, struct file_info *fi);
void WavFile_fwrite(struct WavFile *wav_file, struct file_info *fi);
void WavDataChunk_free(struct WavDataChunk *chunk);
void WavFmtChunk_free(struct WavFmtChunk *chunk);
void WavFile_free(struct WavFile *wav_file);
double WavFile_get_frequency(struct WavFile *wav_file);
void WavFile_set_frequency(struct WavFile *wav_file, double frequency);

#endif