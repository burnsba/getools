#ifndef _GAUDIO_WAV_H_
#define _GAUDIO_WAV_H_

#include <stdint.h>

#define WAV_DEFAULT_EXTENSION ".wav"

#define WAV_RIFF_CHUNK_ID 0x52494646 /* 0x52494646 = "RIFF" */
#define WAV_RIFF_TYPE_ID 0x57415645 /* 0x57415645 = "WAVE" */
#define WAV_FMT_CHUNK_ID 0x666d7420 /* 0x666d7420 = "fmt " */
#define WAV_DATA_CHUNK_ID 0x64617461 /* 0x64617461 = "data" */

#define WAV_DEFAULT_NUM_CHUNKS 2

#define WAV_AUDIO_FORMAT 1 /* PCM (uncompressed) */

struct WavDataChunk {
    uint32_t ck_id;
    int32_t ck_data_size;
    uint8_t *data;
};

struct WavFmtChunk {
    uint32_t ck_id;
    int32_t ck_data_size;
    int16_t audio_format;
    int16_t num_channels;
    int32_t sample_rate;
    int32_t byte_rate;
    int16_t block_align;
    int16_t bits_per_sample;
};

struct WavFile {
    uint32_t ck_id;
    int32_t ck_data_size;
    int32_t form_type;
    int32_t chunk_count;
    void **chunks;

    // convenience pointers
    struct WavFmtChunk *fmt_chunk;
    struct WavDataChunk *data_chunk;
};

struct WavDataChunk *WavDataChunk_new();
struct WavFmtChunk *WavFmtChunk_new();
struct WavFile *WavFile_new(size_t num_chunks);
struct WavFile *load_wav_from_aifc(struct AdpcmAifcFile *aifc_file);
void WavDataChunk_frwrite(struct WavDataChunk *chunk, struct file_info *fi);
void WavFmtChunk_frwrite(struct WavFmtChunk *chunk, struct file_info *fi);
void WavFile_frwrite(struct WavFile *wav_file, struct file_info *fi);

#endif