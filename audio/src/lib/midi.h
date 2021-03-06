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
#ifndef _GAUDIO_MIDI_H
#define _GAUDIO_MIDI_H

#include <stdint.h>
#include <stdlib.h>

/**
 * Default extension when creating a MIDI file.
*/
#define MIDI_DEFAULT_EXTENSION ".midi"

/**
 * Default extension when creating n64 format MIDI file.
*/
#define MIDI_N64_DEFAULT_EXTENSION ".seq"

/**
 * MIDI file, root chunk fourcc id.
*/
#define MIDI_ROOT_CHUNK_ID 0x4D546864 /* 0x4D546864 = "MThd" */

/**
 * MIDI file, root chunk size in bytes without ck_id or ck_data_size.
*/
#define MIDI_ROOT_CHUNK_BODY_SIZE 6
/**
 * MIDI file, root chunk size in bytes including ck_id or ck_data_size.
*/
#define MIDI_ROOT_CHUNK_FULL_SIZE (4+4+MIDI_ROOT_CHUNK_BODY_SIZE)

/**
 * MIDI file, track chunk fourcc id.
*/
#define MIDI_TRACK_CHUNK_ID 0x4D54726B /* 0x4D54726B = "MTrk" */

// Values larger than this will flag the event as invalid (no matching start event).
#define CSEQ_MAX_END_LOOP_OFFSET 0x1ffff

#define CSEQ_FILE_NUM_TRACKS 16

#define CSEQ_FILE_HEADER_SIZE_BYTES 0x44

#define MIDI_COMMAND_LEN_NOTE_OFF 1
#define MIDI_COMMAND_BYTE_NOTE_OFF 0x80
#define MIDI_COMMAND_NAME_NOTE_OFF "Note Off"
#define MIDI_COMMAND_PARAM_BYTE_NOTE_OFF 2
#define MIDI_COMMAND_NUM_PARAM_NOTE_OFF 2

#define MIDI_COMMAND_LEN_NOTE_ON 1
#define MIDI_COMMAND_BYTE_NOTE_ON 0x90
#define MIDI_COMMAND_NAME_NOTE_ON "Note On"
#define MIDI_COMMAND_PARAM_BYTE_NOTE_ON 2
#define MIDI_COMMAND_NUM_PARAM_NOTE_ON 2

#define CSEQ_COMMAND_LEN_NOTE_ON 1
#define CSEQ_COMMAND_BYTE_NOTE_ON 0x90
#define CSEQ_COMMAND_NAME_NOTE_ON "Note On"
//#define CSEQ_COMMAND_PARAM_BYTE_TEMPO /* length varies */
#define CSEQ_COMMAND_NUM_PARAM_NOTE_ON 3

#define MIDI_COMMAND_LEN_POLYPHONIC_PRESSURE 1
#define MIDI_COMMAND_BYTE_POLYPHONIC_PRESSURE 0xa0
#define MIDI_COMMAND_NAME_POLYPHONIC_PRESSURE "Polyphonic Pressure"
#define MIDI_COMMAND_PARAM_BYTE_POLYPHONIC_PRESSURE 2
#define MIDI_COMMAND_NUM_PARAM_POLYPHONIC_PRESSURE 2

#define MIDI_COMMAND_LEN_CONTROL_CHANGE 1
#define MIDI_COMMAND_BYTE_CONTROL_CHANGE 0xb0
#define MIDI_COMMAND_NAME_CONTROL_CHANGE "Control Change"
#define MIDI_COMMAND_PARAM_BYTE_CONTROL_CHANGE 2
#define MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE 2

#define MIDI_COMMAND_LEN_PROGRAM_CHANGE 1
#define MIDI_COMMAND_BYTE_PROGRAM_CHANGE 0xc0
#define MIDI_COMMAND_NAME_PROGRAM_CHANGE "Program Change"
#define MIDI_COMMAND_PARAM_BYTE_PROGRAM_CHANGE 1
#define MIDI_COMMAND_NUM_PARAM_PROGRAM_CHANGE 1

#define MIDI_COMMAND_LEN_CHANNEL_PRESSURE 1
#define MIDI_COMMAND_BYTE_CHANNEL_PRESSURE 0xd0
#define MIDI_COMMAND_NAME_CHANNEL_PRESSURE "Channel Pressure"
#define MIDI_COMMAND_PARAM_BYTE_CHANNEL_PRESSURE 1
#define MIDI_COMMAND_NUM_PARAM_CHANNEL_PRESSURE 1

#define MIDI_COMMAND_LEN_PITCH_BEND 1
#define MIDI_COMMAND_BYTE_PITCH_BEND 0xe0
#define MIDI_COMMAND_NAME_PITCH_BEND "Pitch Bend"
#define MIDI_COMMAND_PARAM_BYTE_PITCH_BEND 2
#define MIDI_COMMAND_NUM_PARAM_PITCH_BEND 2

#define MIDI_COMMAND_BYTE_META 0xff
#define MIDI_COMMAND_NAME_META "meta"

/**
 * Length in bytes of command. Includes 0xff prefix.
*/
#define CSEQ_COMMAND_LEN_TEMPO 2
#define CSEQ_COMMAND_BYTE_TEMPO 0x51
#define CSEQ_COMMAND_BYTE_TEMPO_WITH_META  (0xff00 | CSEQ_COMMAND_BYTE_TEMPO)
#define CSEQ_COMMAND_NAME_TEMPO "cseq Tempo"
#define CSEQ_COMMAND_PARAM_BYTE_TEMPO 3
#define CSEQ_COMMAND_NUM_PARAM_TEMPO 1

#define MIDI_COMMAND_LEN_TEMPO 2
#define MIDI_COMMAND_BYTE_TEMPO 0x51
#define MIDI_COMMAND_BYTE_TEMPO_WITH_META  (0xff00 | MIDI_COMMAND_BYTE_TEMPO)
#define MIDI_COMMAND_NAME_TEMPO "MIDI Tempo"
#define MIDI_COMMAND_PARAM_BYTE_TEMPO 4
#define MIDI_COMMAND_NUM_PARAM_TEMPO 2

/*
 * Loop end is composed of eight bytes: 0xff 0x2d 0x.. 0x.. 0x.. 0x.. 0x.. 0x..
 * byte[2]: loop count
 * byte[3]: current loop count
 * byte[4-7]: difference from end of loop end event to start of loop start event.
 * 
*/
#define CSEQ_COMMAND_LEN_LOOP_END 2
#define CSEQ_COMMAND_BYTE_LOOP_END 0x2d
#define CSEQ_COMMAND_BYTE_LOOP_END_WITH_META  (0xff00 | CSEQ_COMMAND_BYTE_LOOP_END)
#define CSEQ_COMMAND_NAME_LOOP_END "cseq Loop End"
// loop count, loop count2, offset
#define CSEQ_COMMAND_PARAM_BYTE_LOOP_END 6
// loop count, loop count2, offset
#define CSEQ_COMMAND_NUM_PARAM_LOOP_END 3

/*
 * Loop start is composed of four bytes: 0xff 0x2e 0x.. 0xff
 * byte[2] is the loop number.
*/
/**
 * Length in bytes of command. Includes 0xff prefix.
*/
#define CSEQ_COMMAND_LEN_LOOP_START 2
#define CSEQ_COMMAND_BYTE_LOOP_START 0x2e
#define CSEQ_COMMAND_BYTE_LOOP_START_WITH_META  (0xff00 | CSEQ_COMMAND_BYTE_LOOP_START)
#define CSEQ_COMMAND_NAME_LOOP_START "cseq Loop Start"
// loop number, closing byte
#define CSEQ_COMMAND_PARAM_BYTE_LOOP_START 2
// loop number, closing byte
#define CSEQ_COMMAND_NUM_PARAM_LOOP_START 2

#define MIDI_COMMAND_LEN_END_OF_TRACK 3
#define MIDI_COMMAND_BYTE_END_OF_TRACK 0x2f
#define MIDI_COMMAND_BYTE_END_OF_TRACK_WITH_META  (0xff00 | MIDI_COMMAND_BYTE_END_OF_TRACK)
#define MIDI_COMMAND_FULL_END_OF_TRACK 0xff2f00
#define MIDI_COMMAND_NAME_END_OF_TRACK "MIDI End Of Track"
#define MIDI_COMMAND_PARAM_BYTE_END_OF_TRACK 0
#define MIDI_COMMAND_NUM_PARAM_END_OF_TRACK 0

#define CSEQ_COMMAND_LEN_END_OF_TRACK 2
#define CSEQ_COMMAND_BYTE_END_OF_TRACK 0x2f
#define CSEQ_COMMAND_BYTE_END_OF_TRACK_WITH_META  (0xff00 | CSEQ_COMMAND_BYTE_END_OF_TRACK)
#define CSEQ_COMMAND_NAME_END_OF_TRACK "cseq End Of Track"
#define CSEQ_COMMAND_PARAM_BYTE_END_OF_TRACK 0
#define CSEQ_COMMAND_NUM_PARAM_END_OF_TRACK 0

#define CSEQ_COMMAND_LEN_PATTERN 1
#define CSEQ_COMMAND_BYTE_PATTERN 0xfe
#define CSEQ_COMMAND_NAME_PATTERN "cseq pattern"
#define CSEQ_COMMAND_PARAM_BYTE_PATTERN 3
#define CSEQ_COMMAND_NUM_PARAM_PATTERN 2

/**
 * There are too many variations on malformed seq loop events in the retail game
 * to reliably convert to midi and back to seq. Therefore, adding a sysex event
 * to capture the bad event and store as MIDI, and then convert back to seq.
 * Values from the event will be split into upper and lower bytes to avoid
 * transmitting a set most-significant bit inside data values. E.g., the value
 * of 0x87 will be transmitted 0x08 0x07.
 * 
 * -----------------------------------------------------------------------------
 * Loop start event.
 * -----------------
 * The only value of interest is the loop number: [a][b]
 * Will be translated to the following:
 *     0xf0 0x7d 0x47 0x2e [a][b] 0xf7
 * -----------------------------------------------------------------------------
 * Loop end event.
 * -----------------
 * Loop count (1): [a][b]
 * Loop count (2): [c][d]
 * Start offset: [e][f] [g][h] [i][j] [k][l]
 * Will be translated to the following:
 *     0xf0 0x7d 0x47 0x2d [a][b] [c][d] [e][f] [g][h] [i][j] [k][l] 0xf7
*/
#define MIDI_COMMAND_LEN_SYSEX 1
#define MIDI_COMMAND_BYTE_SYSEX_START 0xf0
#define MIDI_COMMAND_BYTE_SYSEX_UNIVERSAL_NON_COMMERICIAL 0x7d
#define MIDI_COMMAND_BYTE_SYSEX_GAUDIO_PREFIX 0x47
#define MIDI_COMMAND_BYTE_SYSEX_END 0xf7

// all bytes after 0xf0, individually, including 0xf7
#define MIDI_COMMAND_PARAM_BYTE_SYSEX_SEQ_LOOP_START 6
#define MIDI_COMMAND_NUM_PARAM_SYSEX_SEQ_LOOP_START 6

// all bytes after 0xf0, individually, including 0xf7
#define MIDI_COMMAND_PARAM_BYTE_SYSEX_SEQ_LOOP_END 16
#define MIDI_COMMAND_NUM_PARAM_SYSEX_SEQ_LOOP_END 16





// needs to be as long as the longest possible paramaters
#define GMID_EVENT_PARAMTER_BYTE_LEN   MIDI_COMMAND_PARAM_BYTE_SYSEX_SEQ_LOOP_END
#define GMID_EVENT_PARAMTER_LEN        MIDI_COMMAND_NUM_PARAM_SYSEX_SEQ_LOOP_END




#define MIDI_DESCRIPTION_TEXT_BUFFER_LEN 60

#define MIDI_CONTROLLER_BANK_SELECT       0
#define MIDI_CONTROLLER_CHANNEL_VOLUME    7
#define MIDI_CONTROLLER_CHANNEL_BALANCE   8
#define MIDI_CONTROLLER_CHANNEL_PAN      10
#define MIDI_CONTROLLER_CHANNEL_PAN2   0x2a
#define MIDI_CONTROLLER_SUSTAIN          64
#define MIDI_CONTROLLER_EFFECTS_1_DEPTH  91
#define MIDI_CONTROLLER_EFFECTS_2_DEPTH  92
#define MIDI_CONTROLLER_EFFECTS_3_DEPTH  93
#define MIDI_CONTROLLER_EFFECTS_4_DEPTH  94
#define MIDI_CONTROLLER_EFFECTS_5_DEPTH  95
#define MIDI_CONTROLLER_LOOP_START      102
#define MIDI_CONTROLLER_LOOP_END        103
#define MIDI_CONTROLLER_LOOP_COUNT_0    104
#define MIDI_CONTROLLER_LOOP_COUNT_128  105

// bit flags:
#define MIDI_MALFORMED_EVENT_LOOP       (0x01)
#define MIDI_MIDI_EVENT_LOOP_END_HANDLED (0x04)

enum MIDI_FORMAT {
    /**
     * Single multi-channel track.
    */
    MIDI_FORMAT_SINGLE = 0,

    /**
     * One or more simultaneous tracks.
    */
    MIDI_FORMAT_SIMULTANEOUS = 1,

    /**
     * One or more sequentially indepdendent tracks.
    */
    MIDI_FORMAT_SEQUENTIAL = 2
};

enum MIDI_IMPLEMENTATION {
    /**
     * Standard MIDI file format.
    */
    MIDI_IMPLEMENTATION_STANDARD = 0,

    /**
     * N64 seq MIDI format (no compression).
    */
    MIDI_IMPLEMENTATION_SEQ,

    /**
     * N64 compressed seq MIDI format.
    */
    MIDI_IMPLEMENTATION_COMPRESSED_SEQ
};

/**
 * Algorithm used to compress seq data.
*/
enum GAUDIO_PATTERN_ALGORITHM {
    /**
     * General algorithm note, pattern unrolling occurs from a snapshot
     * of the compressed file. That is, later pattern markers refer
     * back over previous pattern markers (4 bytes). This means
     * algorithms need to roll-as-you-go.
    */

    /**
     * Start at the beginning of the track and move forward.
     * For each byte, backtrack max allowed and then
     * compare following bytes. Iterate by stepping compare_pos forward
     * one byte from backtrack to current position.
     * Once all sequences in between are compared, advance
     * current position until end of track.
    */
    PATTERN_ALGORITHM_NAIVE = 0,
};

/**
 * Compressed MIDI format file has a 44 byte header. This is 16 offsets to channels, and a division value.
 * The rest of the file is track data.
*/
struct CseqFile {
    /* begin file format (write elements to disk in order declared according to endianess) */

    /**
     * Offsets to individual tracks.
     * Actual value depends on context.
     * When reading cseq file from disk to export, this is offset from start of file.
     * When converting into cseq format to write to disk, this is offset from start
     * of data block.
     * Some offsets might be set to 0.
     * big endian.
    */
    int32_t track_offset[CSEQ_FILE_NUM_TRACKS];

    /**
     * MIDI division.
     * big endian.
    */
    int32_t division;

    /**
     * Output container when converting from MIDI. Will always be in seq format.
     * May or may not have pattern substitution applied.
    */
    uint8_t *compressed_data;

    /* end file format ------------------------------------------------------------------- */

    /**
     * Number of tracks that have data.
    */
    int non_empty_num_tracks;

    /**
     * Length in bytes of the compressed track.
    */
    size_t track_lengths[CSEQ_FILE_NUM_TRACKS];

    /**
     * Length in bytes of the compressed_data field.
    */
    size_t compressed_data_len;
};

struct MidiTrack {
    /* begin file format (write elements to disk in order declared according to endianess) */

    /**
     * Chunk id.
     * big endian.
    */
    uint32_t ck_id;

    /**
     * Chunk size (length of data in bytes).
     * big endian.
    */
    int32_t ck_data_size;

    /**
     * Raw track data.
    */
    uint8_t *data;

    /* end file format ------------------------------------------------------------------- */

    /**
     * Zero based index into parent {@code struct MidiFile.tracks}.
    */
    int32_t track_index;
};

struct MidiFile {
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
     * MIDI file format.
     * See enum MIDI_FORMAT.
     * big endian.
    */
    int16_t format;

    /**
     * Number of tracks in the file.
     * big endian.
    */
    int16_t num_tracks;

    /**
     * MIDI division value.
     * Delta time ticks per quarter note (when positive).
     * Negative SMPTE format not supported.
     * big endian.
    */
    int16_t division;

    /* end file format ------------------------------------------------------------------- */

    /**
     * Array of pointers to each track chunk.
     * There's only one type of chunk so this is strongly typed.
    */
    struct MidiTrack **tracks;
};

/**
 * gaudio MIDI format event.
 * This is common format for both n64 compressed midi and standard midi.
*/
struct GmidEvent {

    /**
     * Internal id.
    */
    int id;

    /**
     * Associated command.
    */
    int command;

    /**
     * Length in bytes of compressed MIDI format command.
    */
    int cseq_command_len;

    /**
     * Length in bytes of standard MIDI command.
    */
    int midi_command_len;

    /**
     * Flag to indicate this is a valid event for compressed MIDI.
    */
    uint8_t cseq_valid;

    /**
     * Flag to indicate this is a valid event for standard MIDI.
    */
    uint8_t midi_valid;

    /**
     * The MIDI channel the command applies to.
    */
    int command_channel;

    /**
     * Delta time of event when the event is used in compressed MIDI format track.
    */
    struct VarLengthInt cseq_delta_time;
    
    /**
     * Delta time of event when the event is used in standard MIDI track.
    */
    struct VarLengthInt midi_delta_time;

    /**
     * Absolute time of event, since start of track.
    */
    long absolute_time;

    /**
     * Command parameters / values for compressed MIDI format command.
    */
    uint8_t cseq_command_parameters_raw[GMID_EVENT_PARAMTER_BYTE_LEN];

    /**
     * Length in bytes of raw command parameters for compressed MIDI format command.
    */
    int cseq_command_parameters_raw_len;

    /**
     * Decoded / processed command parameters for compressed MIDI format command.
    */
    int cseq_command_parameters[GMID_EVENT_PARAMTER_LEN];

    /**
     * Number of command parameters for compressed MIDI format command.
    */
    int cseq_command_parameters_len;

    /**
     * Command parameters / values for standard MIDI command.
    */
    uint8_t midi_command_parameters_raw[GMID_EVENT_PARAMTER_BYTE_LEN];

    /**
     * Length in bytes of raw command parameters for standard MIDI command.
    */
    int midi_command_parameters_raw_len;

    /**
     * Decoded / processed command parameters for standard MIDI command.
    */
    int midi_command_parameters[GMID_EVENT_PARAMTER_LEN];

    /**
     * Number of command parameters for standard MIDI command.
    */
    int midi_command_parameters_len;

    /**
     * File offset the event begins at.
    */
    size_t file_offset;

    /**
     * Special flags.
    */
    uint32_t flags;

    /**
     * Associated dual event.
     * For example: Note On <-> Note Off; loop start <-> loop end; etc.
     * May be NULL.
    */
    struct GmidEvent *dual;
};

/**
 * gaudio MIDI format track.
 * This is common format for both n64 compressed midi and standard midi.
*/
struct GmidTrack {
    /**
     * Track index in standard MIDI file.
     * MIDI track listing will not include empty/missing tracks.
    */
    int midi_track_index;

    /**
     * Track index in compressed MIDI file.
     * cseq track listing can have NULL/empty tracks.
    */
    int cseq_track_index;

    /**
     * Size in bytes of events list if it were written as compressed MIDI track data.
    */
    int cseq_track_size_bytes;

    /**
     * Size in bytes of events list if it were written as standard MIDI track data.
    */
    int midi_track_size_bytes;

    /**
     * List of cseq/MIDI events.
    */
    struct LinkedList *events;

    /**
     * Size in bytes of data buffer.
    */
    size_t cseq_data_len;

    /**
     * Compressed MIDI format contents as raw bytes.
     * Data is in Nintendo Compressed MIDI format, but without any compression.
    */
    uint8_t *cseq_data;
};


/**
 * Simple container for common format.
*/
struct GmidFile {
    // Available tracks.
    struct GmidTrack **tracks;

    /**
     * Number of elements in tracks pointer.
     * Do not change this value unless memory allocated to tracks changes.
    */
    int num_tracks;
};

/**
 * Callback function with single parameter, pointer to GmidTrack.
 * Returns void.
*/
typedef void (*f_GmidTrack_callback)(struct GmidTrack *);

/**
 * Container for options when converting between MIDI
 * and seq format.
*/
struct MidiConvertOptions {
    /**
     * Callback function to perform on each track after initial unroll.
    */
    f_GmidTrack_callback post_unroll_action;

    /**
     * Flag to indicate whether pattern substitution should be performed.
     * When converting seq->midi, if this flag is set (pattern
     * matching is disabled), then the track will be copied as-is
     * without unrolling. If this flag is cleared (default), this
     * will unroll patterns.
     * When converting midi->seq, if this flag is set (pattern
     * matching is disable), then the track will be copied as-is
     * without compression. If this flag is cleared (default), then
     * pattern substitution will be applied.
     * Note that when pattern compression is disabled 0xfe will
     * not be escaped.
    */
    int no_pattern_compression;

    /**
     * Flag to indicate override for pattern markers. When set, will
     * use the matching file to read pattern marker locations from disk
     * instead of computing them based on track data.
     * Only applies if pattern compression is used.
    */
    int use_pattern_marker_file;

    /**
     * Filename to read pattern markers from disk.
     * Points to previously allocated memory.
     * Should not be freed.
    */
    char *pattern_marker_filename;

    /**
     * Flag to indicate whether invalid seq loops should be supported.
     * When set for cseq->midi conversion, will escape invalid loop events
     * to gaudio sysex events.
     * When set for midi->cseq conversion, will parse gaudio sysex into
     * invalid seq loop events.
    */
    int sysex_seq_loops;

    /**
     * Which pattern substitution algorithm to use when converting
     * from midi to cseq.
    */
    enum GAUDIO_PATTERN_ALGORITHM pattern_algorithm;

    // not a configuration option, used at runtime.
    struct FileInfo *runtime_pattern_file;
    struct LinkedList *runtime_patterns_list;
};

#define MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN 255
extern int g_midi_parse_debug;
extern int g_midi_debug_loop_delta;
extern int g_strict_cseq_loop_event;

// seq declarations

struct CseqFile *CseqFile_new(void);
struct CseqFile *CseqFile_new_from_file(struct FileInfo *fi);
struct CseqFile *CseqFile_from_MidiFile(struct MidiFile *midi, struct MidiConvertOptions *options);
void CseqFile_free(struct CseqFile *cseq);
void CseqFile_unroll(struct CseqFile *cseq, struct GmidTrack *track, struct LinkedList *patterns);
void CseqFile_fwrite(struct CseqFile *cseq, struct FileInfo *fi);

// midi declarations

struct MidiTrack *MidiTrack_new(int32_t track_index);
struct MidiTrack *MidiTrack_new_from_GmidTrack(struct GmidTrack *gtrack);
struct MidiFile *MidiFile_new(int format);
struct MidiFile *MidiFile_new_tracks(int format, int num_tracks);
struct MidiFile *MidiFile_from_CseqFile(struct CseqFile *cseq, struct MidiConvertOptions *options);
struct MidiFile *MidiFile_new_from_file(struct FileInfo *fi);
struct MidiFile *MidiFile_new_from_gmid(struct GmidFile *gmid_file);
void MidiTrack_free(struct MidiTrack *track);
void MidiFile_free(struct MidiFile *midi);
void MidiTrack_fwrite(struct MidiTrack *track, struct FileInfo *fi);
void MidiFile_fwrite(struct MidiFile *midi_file, struct FileInfo *fi);
void MidiFile_parse(struct MidiFile *midi_file, int parse_track_arg);

struct MidiFile *MidiFile_transform_set_channel_instrument(struct MidiFile *midi_file, int existing_channel, int new_instrument);
void GmidFile_transform_set_channel_instrument(struct GmidFile *gmid_file, int existing_channel, int new_instrument);
struct MidiFile *MidiFile_transform_make_channel_track(struct MidiFile *midi_file);
void GmidFile_transform_make_channel_track(struct GmidFile *gmid_file);
struct MidiFile *MidiFile_transform_remove_loop(struct MidiFile *midi_file, int loop_number, int track);
void GmidFile_transform_remove_loop(struct GmidFile *gmid_file, int loop_number, int track);
struct MidiFile *MidiFile_transform_add_note_loop(struct MidiFile *midi_file, int loop_number, int track);
void GmidFile_transform_add_note_loop(struct GmidFile *gmid_file, int loop_number, int track);

// common (GmidFile, GmidTrack, GmidEvent) declarations

struct GmidFile *GmidFile_new_from_midi(struct MidiFile *midi, int force_track);
struct GmidFile *GmidFile_new(void);
void GmidFile_free(struct GmidFile *gmid_file);

struct GmidEvent *GmidEvent_new(void);
struct GmidEvent *GmidEvent_new_from_buffer(
    uint8_t *buffer,
    size_t *pos_ptr,
    size_t buffer_len,
    enum MIDI_IMPLEMENTATION buffer_type,
    int32_t running_status,
    int *bytes_read);
struct GmidTrack *GmidTrack_new(void);
void GmidTrack_free(struct GmidTrack *track);
void GmidEvent_free(struct GmidEvent *event);
int32_t GmidEvent_get_midi_command(struct GmidEvent *event);
int32_t GmidEvent_get_cseq_command(struct GmidEvent *event);
void GmidTrack_parse_CseqTrack(struct GmidTrack *gtrack);
size_t GmidTrack_write_to_midi_buffer(struct GmidTrack *gtrack, uint8_t *buffer, size_t max_len);
void GmidTrack_delta_from_absolute(struct GmidTrack *gtrack);
void GmidTrack_midi_to_cseq_loop(struct GmidTrack *gtrack);
void GmidTrack_cseq_to_midi_loop(struct GmidTrack *gtrack);
void GmidTrack_midi_note_off_from_cseq(struct GmidTrack *gtrack);
void GmidTrack_cseq_note_on_from_midi(struct GmidTrack *gtrack);
void GmidTrack_set_track_size_bytes(struct GmidTrack *gtrack);
void GmidTrack_pair_cseq_loop_events(struct GmidTrack *gtrack, struct LinkedList *patterns);
void GmidTrack_invalid_cseq_loop_to_sysex(struct GmidTrack *gtrack, int no_create);
void GmidTrack_midi_sysex_to_cseq(struct GmidTrack *gtrack);
size_t GmidTrack_write_to_cseq_buffer(struct GmidTrack *gtrack, uint8_t *buffer, size_t max_len);
struct CseqFile *CseqFile_new_from_tracks(struct GmidTrack **track, size_t num_tracks);
void GmidTrack_get_pattern_matches_naive(struct GmidTrack *gtrack, uint8_t *write_buffer, size_t *current_buffer_pos, struct LinkedList *matches);
void CseqFile_no_unroll_copy(struct CseqFile *cseq, struct GmidTrack *track);
void GmidTrack_roll_entry(struct GmidTrack *gtrack, uint8_t *write_buffer, size_t *current_buffer_pos, size_t buffer_len, struct MidiConvertOptions *options);
void GmidTrack_roll_apply_patterns(struct GmidTrack *gtrack, uint8_t *write_buffer, size_t *current_buffer_pos, size_t buffer_len, struct LinkedList *matches);
void GmidTrack_get_pattern_matches_file(struct MidiConvertOptions *options, struct LinkedList *matches);
int where_SeqPatternMatch_is_track(struct LinkedListNode *node, int arg1);
void GmidTrack_seq_fix_loop_end_delta(struct GmidTrack *gtrack);
void GmidTrack_delete_event(struct GmidTrack *gtrack, struct LinkedListNode *node);

// options

struct MidiConvertOptions *MidiConvertOptions_new(void);
void MidiConvertOptions_free(struct MidiConvertOptions *obj);

// helper methods

void write_patterns_to_file(struct LinkedList *patterns, struct FileInfo *file);
void midi_controller_to_name(int controller, char *result, size_t max_length);
void midi_note_to_name(int note, char* result, size_t max_length);
size_t GmidEvent_to_string(struct GmidEvent *event, char *buffer, size_t bufer_len, enum MIDI_IMPLEMENTATION type);


#endif