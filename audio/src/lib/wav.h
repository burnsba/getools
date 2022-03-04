/**
 * Copyright 2022 Ben Burns
*/
/**
 * This file is part of Gaudio.
 * 
 * Gaudio is free software: you can redistribute it and/or modify it under the
 * terms of the GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * Gaudio is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
 * without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with Gaudio. If not, see <https://www.gnu.org/licenses/>. 
*/
#ifndef _GAUDIO_WAV_H_
#define _GAUDIO_WAV_H_

#include <stdint.h>

/**
 * This file contains structs and defines for supporting .wav audio files.
 * 
 * .wav files are assumed to be in LITTLE endian format.
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
 * wav file, "smpl" chunk fourcc id.
*/
#define WAV_SMPL_CHUNK_ID 0x736D706C /* 0x736D706C = "smpl" */

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
 * wav file, size of "smpl" chunk without ck_id or ck_data_size or sample loops.
*/
#define WAV_SMPL_CHUNK_BODY_SIZE 36

/**
 * wav file, size of "smpl" chunk including ck_id or
 * ck_data_size, but still without any sample loops.
*/
#define WAV_SMPL_CHUNK_FULL_SIZE (4 + 4 + WAV_SMPL_CHUNK_BODY_SIZE)

/**
 * Size in bytes of a single {@code struct WavSampleLoop}.
*/
#define WAV_SAMPLE_LOOP_SIZE 24

/**
 * wav file default audio format
*/
#define WAV_AUDIO_FORMAT 1 /* PCM (uncompressed) */

enum SAMPLE_LOOP_TYPE {
    SAMPLE_LOOP_FORWARD = 0,
    SAMPLE_LOOP_ALTERNATING,
    SAMPLE_LOOP_BACKWARD,
    
    SAMPLE_LOOP_RESERVED = 3,        /* 3  -         31 */
    SAMPLE_LOOP_MANUFACTURER = 32    /* 32 - 0xffffffff */
};

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
 * wav file, subtybe to describe loop used in "smpl" chunk.
*/
struct WavSampleLoop {
    /* begin file format (write elements to disk in order declared according to endianess) */

    /**
     * Cue point id.
     * Unique within the wav file.
    */
    uint32_t cue_point_id;

    /**
     * Sample loop type. See {@code SAMPLE_LOOP_TYPE}.
    */
    uint32_t loop_type;

    /**
     * Index of sample into ssnd data chunk of first sample to be played.
     * Note: some documentation says this is a byte offset, but it's actually a sample count.
    */
    uint32_t start;

    /**
     * Index of sample into ssnd data chunk of last sample to be played.
     * Note: some documentation says this is a byte offset, but it's actually a sample count.
    */
    uint32_t end;

    /**
     * Fraction of a sample to start the loop at.
     * Normalized out of 2^32.
     * 0x80000000 means 1/2 of a sample.
    */
    uint32_t fraction;

    /**
     * Number of times to loop.
     * 0 = infinite,
     * else finite.
    */
    uint32_t play_count;

    /* end file format ------------------------------------------------------------------- */
};

/**
 * wav file container for "smpl" chunk.
*/
struct WavSampleChunk {
    /* begin file format (write elements to disk in order declared according to endianess) */

    /**
     * Chunk id.
     * big endian.
    */
    uint32_t ck_id;

    /**
     * Chunk size. Includes all sample data/loops.
     * little endian.
    */
    int32_t ck_data_size;

    /**
     * Manufacturer code. Default 0.
     * little endian.
    */
    int32_t manufacturer;

    /**
     * Product code. Default 0.
     * little endian.
    */
    int32_t product;

    /**
     * Sample period. Should be equal to (1 / WavFmtChunk->sample_rate).
     * little endian.
    */
    int32_t sample_period;

    /**
     * MIDI Unity Note (~ keybase). Note which sample should be played for original sample rate.
     * little endian.
    */
    int32_t midi_unity_note;

    /**
     * MIDI Pitch Fraction (~ detune).
     * Normalized out of 2^32.
     * A value of 0x80000000 means 1/2 semitone (50 cents).
     * little endian.
    */
    int32_t midi_pitch_fraction;

    /**
     * SMPTE Format.
     * little endian.
    */
    int32_t smpte_format;

    /**
     * SMPTE Offset.
     * little endian.
    */
    int32_t smpte_offset;

    /**
     * Number of sample loops described.
     * little endian.
    */
    int32_t num_sample_loops;

    /**
     * Size in bytes of the sample loops.
     * {@code ck_data_size} already counts these bytes.
     * little endian.
    */
    int32_t sample_loop_bytes;

    /**
     * List of sample loops.
    */
    struct WavSampleLoop **loops;

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

    /**
     * Convenience pointer to the only/last "smpl" chunk.
    */
    struct WavSampleChunk *smpl_chunk;
};

struct WavSampleLoop *WavSampleLoop_new(void);
struct WavSampleChunk *WavSampleChunk_new(int number_loops);
struct WavDataChunk *WavDataChunk_new(void);
struct WavFmtChunk *WavFmtChunk_new(void);
struct WavFile *WavFile_new(size_t num_chunks);
struct WavFile *WavFile_new_from_file(struct FileInfo *fi);

void WavSampleLoop_fwrite(struct WavSampleLoop *loop, struct FileInfo *fi);
void WavSampleChunk_fwrite(struct WavSampleChunk *chunk, struct FileInfo *fi);
void WavDataChunk_fwrite(struct WavDataChunk *chunk, struct FileInfo *fi);
void WavFmtChunk_fwrite(struct WavFmtChunk *chunk, struct FileInfo *fi);
void WavFile_fwrite(struct WavFile *wav_file, struct FileInfo *fi);

void WavSampleLoop_free(struct WavSampleLoop *loop);
void WavSampleChunk_free(struct WavSampleChunk *chunk);
void WavDataChunk_free(struct WavDataChunk *chunk);
void WavFmtChunk_free(struct WavFmtChunk *chunk);
void WavFile_free(struct WavFile *wav_file);

double WavFile_get_frequency(struct WavFile *wav_file);
void WavFile_set_frequency(struct WavFile *wav_file, double frequency);
void WavFile_append_smpl_chunk(struct WavFile *wav_file, struct WavSampleChunk *chunk);

#endif