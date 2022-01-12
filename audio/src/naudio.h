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
 * Rare .sbk file uses this type.
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
enum AL_WAVETABLE_TYPE {
    AL_ADPCM_WAVE = 0,
    AL_RAW16_WAVE
};

/**
 * same as libultra struct.
*/
struct ALADPCMBook {
    /**
     * big endian.
    */
    int32_t order;

    /**
     * aka nentries.
     * big endian.
    */
    int32_t npredictors;

    /**
     * Must be 8-byte aligned
    */
    int16_t *book;
};

#define ADPCM_STATE_SIZE 0x20 /* size in bytes */

/**
 * same as libultra struct.
*/
struct ALADPCMLoop {
    /**
     * Sample offset of the loop start point.
     * big endian.
    */
    uint32_t start;

    /**
     * Sample offset of the loop end point.
     * big endian.
    */
    uint32_t end;

    /**
     * Number of times to loop. -1 is infinite.
     * big endian.
    */
    uint32_t count;

    /**
     * ADPCM decoder state information.
    */
    uint8_t state[ADPCM_STATE_SIZE];
};

/**
 * same as libultra struct.
*/
struct ALRawLoop {
    /**
     * Sample offset of the loop start point.
     * big endian.
    */
    uint32_t start;

    /**
     * Sample offset of the loop end point.
     * big endian.
    */
    uint32_t end;

    /**
     * Number of times to loop. -1 is infinite.
     * big endian.
    */
    uint32_t count;
};

/**
 * Modified libultra struct.
*/
struct ALADPCMWaveInfo {
    /* begin file format (write elements to disk in order declared according to endianess) */
    
    /**
     * File offset of loop data as read from file.
     * This is never promoted to a pointer by this library.
     * big endian.
    */
    int32_t loop_offset;

    /**
     * File offset of codebook data as read from file.
     * This is never promoted to a pointer by this library.
     * big endian.
    */
    int32_t book_offset;

    /* end file format ------------------------------------------------------------------- */

    /**
     * Pointer to loop data.
    */
    struct ALADPCMLoop *loop;

    /**
     * Pointer to codebook data.
    */
    struct ALADPCMBook *book;
};

/**
 * Modified libultra struct.
*/
struct ALRAWWaveInfo {
    /* begin file format (write elements to disk in order declared according to endianess) */

    /**
     * File offset of loop data as read from file.
     * This is never promoted to a pointer by this library.
     * big endian.
    */
    int32_t loop_offset;

    /* end file format ------------------------------------------------------------------- */

    /**
     * Pointer to loop data.
    */
    struct ALRawLoop *loop;
};

/**
 * Modified libultra struct.
*/
struct ALWaveTable {
    /* begin file format (write elements to disk in order declared according to endianess) */

    /**
     * This is the file offset into the .tbl file
     * of the sound data. Can be zero.
     * This is never promoted to a pointer by this library.
     * big endian.
    */
    int32_t base;

    /**
     * Length in bytes of the sound data in the .tbl file.
     * big endian.
    */
    int32_t len;

    /**
     * Type {@code AL_WAVETABLE_TYPE}.
    */
    uint8_t type;

    /**
     * Libultra uses this as pointer/offset flag, such that
     * zero indicates offsets, and one indicates pointers.
     * Unused by this library, therefore always zero.
    */
    uint8_t flags;

    /**
     * unused (explicit padding).
    */
    uint16_t unused_padding;

    /**
     * Type specific info.
    */
    union {
        // if type is AL_ADPCM_WAVE
        struct ALADPCMWaveInfo adpcm_wave;

        // if type is AL_RAW16_WAVE
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

    /**
     * Filename with extension and path where aifc data is located.
    */
    char *aifc_path;
};

/**
 * Modified libultra struct.
 * The programming manual says:
 *     "The ALKeyMap describes how the sound is mapped to the keyboard. It allows
 *     the sequencer to determine at what pitch to play a sound, given its MIDI
 *     key number and note on velocity."
 * 
 * The programming manual also says keymaps are only used by the sequence player
 * (ignored by sound effect player).
*/
struct ALKeyMap {
    /* begin file format (write elements to disk in order declared according to endianess) */

    /**
     * Minimum note-on velocity for this map.
    */
    uint8_t velocity_min;

    /**
     * Maximum note-on velocity for this map.
    */
    uint8_t velocity_max;

    /**
     * Lowest MIDI note in this key map.
    */
    uint8_t key_min;

    /**
     * Highest MIDI note in this key map.
    */
    uint8_t key_max;

    /**
     * The MIDI note equivalent to the sound played at unity pitch.
    */
    uint8_t key_base;

    /**
     * Amount in cents to fine-tune this sample.
    */
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

    /**
     * Time in microseconds to ramp from zero gain to attackVolume. 
     * big endian.
    */
    int32_t attack_time;

    /**
     * Time in microseconds to ramp from the attackVolume to the decayVolume.
     * big endian.
    */
    int32_t decay_time;

    /**
     * Time in microseconds to ramp to zero volume.
     * big endian.
    */
    int32_t release_time;

    /**
     * Target time for attack segment.
     * Note: different type from the programming manual documentation.
    */
    uint8_t attack_volume;

    /**
     * Target time for decay segment.
     * Note: different type from the programming manual documentation.
    */
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

    /**
     * File offset of envelope as read from file.
     * This is never promoted to a pointer by this library.
     * big endian.
    */
    int32_t envelope_offset;

    /**
     * File offset of keymap as read from file.
     * This is never promoted to a pointer by this library.
     * big endian.
    */
    int32_t key_map_offset;

    /**
     * File offset of wavetable as read from file.
     * This is never promoted to a pointer by this library.
     * big endian.
    */
    int32_t wavetable_offfset;

    /**
     * Pan. 0=left, 64=center, 127=right.
    */
    uint8_t sample_pan;

    /**
     * Sample playback volume.
    */
    uint8_t sample_volume;

    /**
     * Libultra uses this as pointer/offset flag, such that
     * zero indicates offsets, and one indicates pointers.
     * Unused by this library, therefore always zero.
    */
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

    /**
     * Pointer to envelope.
    */
    struct ALEnvelope *envelope;

    /**
     * Pointer to keymap.
    */
    struct ALKeyMap *keymap;

    /**
     * Pointer to wavetable.
    */
    struct ALWaveTable *wavetable;
};

/**
 * Modified libultra struct.
*/
struct ALInstrument {
    /* begin file format (write elements to disk in order declared according to endianess) */

    /**
     * Instrument playback volume.
    */
    uint8_t volume;

    /**
     * Pan. 0=left, 64=center, 127=right.
    */
    uint8_t pan;

    /**
     * Voice priority. 0=lowest, 10=highest.
    */
    uint8_t priority;

    /**
     * Libultra uses this as pointer/offset flag, such that
     * zero indicates offsets, and one indicates pointers.
     * Unused by this library, therefore always zero.
    */
    uint8_t flags;

    // undocumented in n64 programming manual:

    uint8_t trem_type;
    uint8_t trem_rate;
    uint8_t trem_depth;
    uint8_t trem_delay;
    uint8_t vib_type;
    uint8_t vib_rate;
    uint8_t vib_depth;
    uint8_t vib_delay;

    /**
     * Pitch bend range, in cents.
     * big endian.
    */
    int16_t bend_range;

    /**
     * Number of sounds in the instrument,
     * and length of `sounds` array.
     * big endian.
    */
    int16_t sound_count;

    /**
     * File offsets of sounds as read from file.
     * These are never promoted to pointers by this library.
     * big endian.
    */
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

/**
 * Output mode.
 * The inst file prints different parameters based on whether it's
 * for sound effect files or music sequences.
*/
enum OUTPUT_MODE {
    OUTPUT_MODE_SFX = 0,
    OUTPUT_MODE_MUSIC
};

/**
 * Callback function, for use when creating a new wavetable.
 * This allows setting the aifc filename based on filenames the user provides.
 * Returns void, accepts one parameter of type {@code struct ALWaveTable *wavetable}.
*/
typedef void (*wavetable_init_callback) (struct ALWaveTable *wavetable);

extern wavetable_init_callback wavetable_init_callback_ptr;

struct ALADPCMLoop *ALADPCMLoop_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset);
struct ALADPCMBook *ALADPCMBook_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset);
struct ALRawLoop *ALRawLoop_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset);
struct ALEnvelope *ALEnvelope_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset);
void ALEnvelope_write_to_fp(struct ALEnvelope *envelope, struct file_info *fi);
struct ALKeyMap *ALKeyMap_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset);
void ALKeyMap_write_to_fp(struct ALKeyMap *keymap, struct file_info *fi);
struct ALWaveTable *ALWaveTable_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset);
struct ALSound *ALSound_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset);
void ALSound_write_to_fp(struct ALSound *sound, struct file_info *fi);
struct ALInstrument *ALInstrument_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset);
void ALInstrument_write_to_fp(struct ALInstrument *instrument, struct file_info *fi);
struct ALBank *ALBank_new_from_ctl(uint8_t *ctl_file_contents, int32_t load_from_offset);
void ALBank_write_to_fp(struct ALBank *bank, struct file_info *fi);
struct ALBankFile *ALBankFile_new_from_ctl(uint8_t *ctl_file_contents);
void write_inst(struct ALBankFile *bank_file, char* inst_filename);
void ALADPCMLoop_free(struct ALADPCMLoop *loop);
void ALADPCMBook_free(struct ALADPCMBook *book);
void ALRawLoop_free(struct ALRawLoop *loop);
void ALEnvelope_free(struct ALEnvelope *envelope);
void ALKeyMap_free(struct ALKeyMap *keymap);
void ALWaveTable_free(struct ALWaveTable *wavetable);
void ALSound_free(struct ALSound *sound);
void ALInstrument_free(struct ALInstrument *instrument);
void ALBank_free(struct ALBank *bank);
void ALBankFile_free(struct ALBankFile *bank_file);

#endif