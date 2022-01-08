#ifndef _GAUDIO_NAUDIO_H_
#define _GAUDIO_NAUDIO_H_

// n64 lib + Rare audio related

#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include "utility.h"

#define NAUDIO_AIFC_OUT_DEFAULT_EXTENSION       ".aifc" /* Nintendo custom .aiff format */

#define INST_OBJ_ID_STRING_LEN 25

#define BANKFILE_MAGIC_BYTES 0x4231

/**
 * Unused, but including for reference:
 * 
 * Metadata for a sequence "file" entry / data content of single sequence.
 * Based on original ALSeqData in n64devkit\ultra\usr\include\PR\libaudio.h.
 */
struct RareALSeqData
{
    // address is offset from the start of .sbk file
    uint8_t *address;

    // seq length after uncompressed.
    uint16_t uncompressed_len;

    // len is data segment length in the ROM. This is the 1172 compressed length.
    uint16_t len;
};

enum {
    AL_ADPCM_WAVE = 0,
    AL_RAW16_WAVE
};

struct ALADPCMBook {
    int32_t order;
    int32_t npredictors;
    int16_t *book;        /* Must be 8-byte aligned */
};

#define ADPCM_STATE_SIZE 0x20 /* size in bytes */

struct ALADPCMloop {
    uint32_t start;
    uint32_t end;
    uint32_t count;
    uint8_t state[ADPCM_STATE_SIZE];
};

struct ALRawLoop {
    uint32_t start;
    uint32_t end;
    uint32_t count;
};

struct ALADPCMWaveInfo {
    int32_t loop_offset;
    struct ALADPCMloop *loop;
    int32_t book_offset;
    struct ALADPCMBook *book;
};

struct ALRAWWaveInfo {
    int32_t loop_offset;
    struct ALRawLoop *loop;
};

struct ALWaveTable {
    int32_t id;
    char text_id[INST_OBJ_ID_STRING_LEN];
    char *aifc_path;
    int32_t base; /* offset into .tbl file, can be zero */
    int32_t len;
    uint8_t type;
    uint8_t flags;
    uint16_t unused_padding;
    union {
        struct ALADPCMWaveInfo adpcm_wave;
        struct ALRAWWaveInfo raw_wave;
    } wave_info;
};

struct ALKeyMap {
    int32_t id;
    char text_id[INST_OBJ_ID_STRING_LEN];
    uint8_t velocity_min;
    uint8_t velocity_max;
    uint8_t key_min;
    uint8_t key_max;
    uint8_t key_base;
    int8_t detune;
};

struct ALEnvelope {
    int32_t id;
    char text_id[INST_OBJ_ID_STRING_LEN];
    int32_t attack_time;
    int32_t decay_time;
    int32_t release_time;
    uint8_t attack_volume;
    uint8_t decay_volume;
};

struct ALSound {
    int32_t id;
    char text_id[INST_OBJ_ID_STRING_LEN];
    int32_t envelope_offset;
    struct ALEnvelope *envelope;
    int32_t key_map_offset;
    struct ALKeyMap *keymap;
    int32_t wavetable_offfset;
    struct ALWaveTable *wavetable;
    uint8_t sample_pan;
    uint8_t sample_volume;
    uint8_t flags;
};

struct ALInstrument {
    int32_t id;
    char text_id[INST_OBJ_ID_STRING_LEN];
    uint8_t volume;
    uint8_t pan;
    uint8_t priority;
    uint8_t flags;
    uint8_t trem_type;
    uint8_t trem_rate;
    uint8_t trem_depth;
    uint8_t trem_delay;
    uint8_t vib_type;
    uint8_t vib_rate;
    uint8_t vib_depth;
    uint8_t vib_delay;
    int16_t bend_range;
    int16_t sound_count;
    int32_t *sound_offsets;
    struct ALSound **sounds;
};

struct ALBank {
    int32_t id;
    char text_id[INST_OBJ_ID_STRING_LEN];
    int16_t inst_count;
    uint8_t flags;
    uint8_t pad;
    int32_t sample_rate;
    int32_t percussion; /* is this a pointer? */
    int32_t *inst_offsets;
    struct ALInstrument **instruments;
};

struct ALBankFile {
    int32_t id;
    char text_id[INST_OBJ_ID_STRING_LEN];
    int16_t revision;
    int16_t bank_count;
    int32_t *bank_offsets;
    struct ALBank **banks;
};

enum OUTPUT_MODE {
    OUTPUT_MODE_SFX = 0,
    OUTPUT_MODE_MUSIC
};

typedef void (*wavetable_init_callback) (struct ALWaveTable *wavetable);

extern wavetable_init_callback wavetable_init_callback_ptr;

void adpcm_loop_init_load(struct ALADPCMloop *adpcm_loop, uint8_t *ctl_file_contents, int32_t load_from_offset);
void adpcm_book_init_load(struct ALADPCMBook *adpcm_book, uint8_t *ctl_file_contents, int32_t load_from_offset);
void raw_loop_init_load(struct ALRawLoop *raw_loop, uint8_t *ctl_file_contents, int32_t load_from_offset);
void envelope_init_load(struct ALEnvelope *envelope, uint8_t *ctl_file_contents, int32_t load_from_offset);
void envelope_write_to_fp(struct ALEnvelope *envelope, struct file_info *fi);
void keymap_init_load(struct ALKeyMap *keymap, uint8_t *ctl_file_contents, int32_t load_from_offset);
void keymap_write_to_fp(struct ALKeyMap *keymap, struct file_info *fi);
void wavetable_init_load(struct ALWaveTable *wavetable, uint8_t *ctl_file_contents, int32_t load_from_offset);
void sound_init_load(struct ALSound *sound, uint8_t *ctl_file_contents, int32_t load_from_offset);
void sound_write_to_fp(struct ALSound *sound, struct file_info *fi);
void instrument_init_load(struct ALInstrument *instrument, uint8_t *ctl_file_contents, int32_t load_from_offset);
void instrument_write_to_fp(struct ALInstrument *instrument, struct file_info *fi);
void bank_init_load(struct ALBank *bank, uint8_t *ctl_file_contents, int32_t load_from_offset);
void bank_write_to_fp(struct ALBank *bank, struct file_info *fi);
void bank_file_init_load(struct ALBankFile *bank_file, uint8_t *ctl_file_contents);
void write_inst(struct ALBankFile *bank_file, char* inst_filename);

#endif