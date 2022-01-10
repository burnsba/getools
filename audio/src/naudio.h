#ifndef _GAUDIO_NAUDIO_H_
#define _GAUDIO_NAUDIO_H_

#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include "utility.h"

/**
 * This file contains structs and defines for supporting Rare's audio structs,
 * and Nintendo's (libultra) audio structs.
*/

/**
 * Default extension when creating a .aifc file.
*/
#define NAUDIO_AIFC_OUT_DEFAULT_EXTENSION ".aifc"

/**
 * The .inst file needs named references for objects. This is the max
 * name length, including trailing '\0'.
*/
#define INST_OBJ_ID_STRING_LEN 25

/**
 * The .ctl file must begin with this two byte sequence (big endian).
*/
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

/* same as libultra */
enum {
    AL_ADPCM_WAVE = 0,
    AL_RAW16_WAVE
};

/**
 * same as libultra struct.
*/
struct ALADPCMBook {
    int32_t order;
    int32_t npredictors;
    int16_t *book;        /* Must be 8-byte aligned */
};

#define ADPCM_STATE_SIZE 0x20 /* size in bytes */

/**
 * same as libultra struct.
*/
struct ALADPCMloop {
    uint32_t start;
    uint32_t end;
    uint32_t count;
    uint8_t state[ADPCM_STATE_SIZE];
};

/**
 * same as libultra struct.
*/
struct ALRawLoop {
    uint32_t start;
    uint32_t end;
    uint32_t count;
};

/**
 * Modified libultra struct.
*/
struct ALADPCMWaveInfo {
    /* begin file format (write elements to disk in order declared according to endianess) */
    int32_t loop_offset;
    int32_t book_offset;

    /* end file format ------------------------------------------------------------------- */

    struct ALADPCMloop *loop;
    struct ALADPCMBook *book;
};

/**
 * Modified libultra struct.
*/
struct ALRAWWaveInfo {
    /* begin file format (write elements to disk in order declared according to endianess) */

    int32_t loop_offset;

    /* end file format ------------------------------------------------------------------- */

    struct ALRawLoop *loop;
};

/**
 * Modified libultra struct.
*/
struct ALWaveTable {
    /* begin file format (write elements to disk in order declared according to endianess) */

    int32_t base; /* offset into .tbl file, can be zero */
    int32_t len;
    uint8_t type;
    uint8_t flags;
    uint16_t unused_padding;
    union {
        struct ALADPCMWaveInfo adpcm_wave;
        struct ALRAWWaveInfo raw_wave;
    } wave_info;

    /* end file format ------------------------------------------------------------------- */

    /**
     * Library/runtime id of wave table.
    */
    int32_t id;

    /**
     * inst file text id.
    */
    char text_id[INST_OBJ_ID_STRING_LEN];
    char *aifc_path;
};

/**
 * Modified libultra struct.
*/
struct ALKeyMap {
    /* begin file format (write elements to disk in order declared according to endianess) */
    uint8_t velocity_min;
    uint8_t velocity_max;
    uint8_t key_min;
    uint8_t key_max;
    uint8_t key_base;
    int8_t detune;

    /* end file format ------------------------------------------------------------------- */

    /**
     * Library/runtime id of keymap.
    */
    int32_t id;

    /**
     * inst file text id.
    */
    char text_id[INST_OBJ_ID_STRING_LEN];
};

/**
 * Modified libultra struct.
*/
struct ALEnvelope {
    /* begin file format (write elements to disk in order declared according to endianess) */

    int32_t attack_time;
    int32_t decay_time;
    int32_t release_time;
    uint8_t attack_volume;
    uint8_t decay_volume;

    /* end file format ------------------------------------------------------------------- */

    /**
     * Library/runtime id of envelope.
    */
    int32_t id;

    /**
     * inst file text id.
    */
    char text_id[INST_OBJ_ID_STRING_LEN];
};

/**
 * Modified libultra struct.
*/
struct ALSound {
    /* begin file format (write elements to disk in order declared according to endianess) */

    int32_t envelope_offset;
    int32_t key_map_offset;
    int32_t wavetable_offfset;
    uint8_t sample_pan;
    uint8_t sample_volume;
    uint8_t flags;

    /* end file format ------------------------------------------------------------------- */

    /**
     * Library/runtime id of sound.
    */
    int32_t id;

    /**
     * inst file text id.
    */
    char text_id[INST_OBJ_ID_STRING_LEN];

    struct ALEnvelope *envelope;
    struct ALKeyMap *keymap;
    struct ALWaveTable *wavetable;
};

/**
 * Modified libultra struct.
*/
struct ALInstrument {
    /* begin file format (write elements to disk in order declared according to endianess) */

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
    
    /* end file format ------------------------------------------------------------------- */

    /**
     * Library/runtime id of instrument.
    */
    int32_t id;

    /**
     * inst file text id.
    */
    char text_id[INST_OBJ_ID_STRING_LEN];

    /**
     * Array of pointers to each sound.
    */
    struct ALSound **sounds;
};

/**
 * Modified libultra struct.
*/
struct ALBank {
    /* begin file format (write elements to disk in order declared according to endianess) */

    /**
     * Number of instruments in the bank,
     * and length of `instruments` array.
     * big endian.
    */
    int16_t inst_count;

    /**
     * Libultra uses this as pointer/offset flag, such that
     * zero indicates offsets, and one indicates pointers.
     * Unused by this library, therefore always zero.
    */
    uint8_t flags;

    /**
     * Unused.
    */
    uint8_t pad;

    /**
     * Playback sample rate.
     * big endian.
    */
    int32_t sample_rate;

    /**
     * Offset/pointer to percussion instrument.
     * Unused.
     * big endian.
    */
    int32_t percussion;

    /**
     * File offsets of instruments as read from file.
     * These are never promoted to pointers by this library.
     * big endian.
    */
    int32_t *inst_offsets;

    /* end file format ------------------------------------------------------------------- */

    /**
     * Library/runtime id of bank.
    */
    int32_t id;

    /**
     * inst file text id.
    */
    char text_id[INST_OBJ_ID_STRING_LEN];

    /**
     * Array of pointers to each instrument.
    */
    struct ALInstrument **instruments;
};

/**
 * Modified libultra struct.
 * Base container for bank.
*/
struct ALBankFile {

    /* begin file format (write elements to disk in order declared according to endianess) */

    /**
     * Revision....?
     * Must be BANKFILE_MAGIC_BYTES.
     * big endian.
    */
    int16_t revision;

    /**
     * Number of banks in the file,
     * and length of `bank_offsets` array.
     * big endian.
    */
    int16_t bank_count;

    /**
     * Offsets into the current file of banks.
     * big endian.
    */
    int32_t *bank_offsets;

    /* end file format ------------------------------------------------------------------- */

    /**
     * Library/runtime id of bank file.
    */
    int32_t id;

    /**
     * inst file text id.
    */
    char text_id[INST_OBJ_ID_STRING_LEN];

    /**
     * Array of pointers to each bank.
    */
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