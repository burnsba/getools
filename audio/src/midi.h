#ifndef _GAUDIO_MIDI_H
#define _GAUDIO_MIDI_H

/**
 * Default extension when creating a MIDI file.
*/
#define MIDI_DEFAULT_EXTENSION ".midi"

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

#define MIDI_COMMAND_BYTE_PITCH_BEND 0xe0
#define MIDI_COMMAND_NAME_PITCH_BEND "Pitch Bend"

/**
 * Length in bytes of command. Includes 0xff prefix.
*/
#define CSEQ_COMMAND_LEN_TEMPO 2
#define CSEQ_COMMAND_BYTE_TEMPO 0x51
#define CSEQ_COMMAND_NAME_TEMPO "cseq Tempo"
#define CSEQ_COMMAND_PARAM_BYTE_TEMPO 3
#define CSEQ_COMMAND_NUM_PARAM_TEMPO 1

#define MIDI_COMMAND_LEN_TEMPO 2
#define MIDI_COMMAND_BYTE_TEMPO 0x51
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
#define CSEQ_COMMAND_NAME_LOOP_END "cseq Loop End"
#define CSEQ_COMMAND_PARAM_BYTE_LOOP_END 6
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
#define CSEQ_COMMAND_NAME_LOOP_START "cseq Loop Start"
#define CSEQ_COMMAND_PARAM_BYTE_LOOP_START 2
#define CSEQ_COMMAND_NUM_PARAM_LOOP_START 2

#define MIDI_COMMAND_LEN_END_OF_TRACK 3
#define MIDI_COMMAND_BYTE_END_OF_TRACK 0x2f
#define MIDI_COMMAND_FULL_END_OF_TRACK 0xff2f00
#define MIDI_COMMAND_NAME_END_OF_TRACK "MIDI End Of Track"
#define MIDI_COMMAND_PARAM_BYTE_END_OF_TRACK 0
#define MIDI_COMMAND_NUM_PARAM_END_OF_TRACK 0

#define CSEQ_COMMAND_LEN_END_OF_TRACK 2
#define CSEQ_COMMAND_BYTE_END_OF_TRACK 0x2f
#define CSEQ_COMMAND_NAME_END_OF_TRACK "cseq End Of Track"
#define CSEQ_COMMAND_PARAM_BYTE_END_OF_TRACK 0
#define CSEQ_COMMAND_NUM_PARAM_END_OF_TRACK 0

#define CSEQ_COMMAND_BYTE_PATTERN 0xfe
#define CSEQ_COMMAND_NAME_PATTERN "cseq pattern"

#define MIDI_DESCRIPTION_TEXT_BUFFER_LEN 40

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

/**
 * Compressed MIDI format file has a 44 byte header. This is 16 offsets to channels, and a division value.
 * The rest of the file is track data.
*/
struct CseqFile {
    /* begin file format (write elements to disk in order declared according to endianess) */

    /**
     * Offsets to individual tracks.
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
     * MIDI file data, in compressed (pattern) format. Contains all track data.
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
};

struct MidiTrack {
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

#define GMID_EVENT_PARAMTER_BYTE_LEN 8
#define GMID_EVENT_PARAMTER_LEN 4

/**
 * gaudio MIDI format event.
 * Has information common to regular MIDI and compressed MIDI.
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
     * Length in bytes of command.
    */
    int cseq_command_len;

    /**
     * Length in bytes of command.
    */
    int midi_command_len;

    uint8_t cseq_valid;
    uint8_t midi_valid;

    /**
     * MIDI channel command applies to.
    */
    int command_channel;

    /**
     * Delta time of event.
    */
    struct var_length_int varint_delta_time;

    /**
     * Absolute time of event, since start of track.
    */
    long absolute_time;

    /**
     * Command parameters / values.
    */
    uint8_t cseq_command_parameters_raw[GMID_EVENT_PARAMTER_BYTE_LEN];

    /**
     * Length in bytes of raw command parameters.
    */
    int cseq_command_parameters_raw_len;

    /**
     * Decoded / processed command parameters.
    */
    int cseq_command_parameters[GMID_EVENT_PARAMTER_LEN];

    /**
     * Number of command parameters.
    */
    int cseq_command_parameters_len;

    /**
     * Command parameters / values.
    */
    uint8_t midi_command_parameters_raw[GMID_EVENT_PARAMTER_BYTE_LEN];

    /**
     * Length in bytes of raw command parameters.
    */
    int midi_command_parameters_raw_len;

    /**
     * Decoded / processed command parameters.
    */
    int midi_command_parameters[GMID_EVENT_PARAMTER_LEN];

    /**
     * Number of command parameters.
    */
    int midi_command_parameters_len;

    /**
     * Associated dual event.
     * For example: Note On <-> Note Off; loop start <-> loop end; etc.
     * May be NULL.
    */
    struct GmidEvent *dual;
};

/**
 * gaudio MIDI format track.
*/
struct GmidTrack {
    /**
     * Track index in MIDI file.
     * MIDI track listing will not include empty/missing tracks.
    */
    int midi_track_index;

    /**
     * Track index, if cseq.
     * cseq track listing can have NULL/empty tracks.
    */
    int cseq_track_index;

    /**
     * Size in bytes of events list if it were written as MIDI track data.
    */
    int cseq_track_size_bytes;
    int midi_track_size_bytes;

    /**
     * List of MIDI events.
    */
    struct llist_root *events;

    /**
     * Size in bytes of data buffer.
    */
    size_t cseq_data_len;

    /**
     * cseq format MIDI contents.
     * Data is in Nintendo Compressed MIDI format, but without any compression.
    */
    uint8_t *cseq_data;
};

#define MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN 255
extern int g_midi_parse_debug;

struct CseqFile *CseqFile_new();
struct MidiTrack *MidiTrack_new(int32_t track_index);
struct MidiFile *MidiFile_new(int format);
struct MidiFile *MidiFile_new_tracks(int format, int num_tracks);
struct CseqFile *CseqFile_new_from_file(struct file_info *fi);
struct MidiTrack *MidiTrack_new_from_GmidTrack(struct GmidTrack *gtrack);
struct MidiFile *MidiFile_from_CseqFile(struct CseqFile *cseq);
struct GmidEvent *GmidEvent_new();
struct GmidTrack *GmidTrack_new();
void GmidEvent_free(struct GmidEvent *event);
void GmidTrack_free(struct GmidTrack *track);
void CseqFile_free(struct CseqFile *cseq);
void MidiTrack_free(struct MidiTrack *track);
void MidiFile_free(struct MidiFile *midi);
void CseqFile_unroll(struct CseqFile *cseq, struct GmidTrack *track);
void GmidTrack_parse_CseqTrack(struct GmidTrack *gtrack);
void midi_controller_to_name(int controller, char *result, size_t max_length);
void midi_note_to_name(int note, char* result, size_t max_length);
int32_t GmidEvent_get_midi_command(struct GmidEvent *event);
size_t GmidTrack_write_to_midi_buffer(struct GmidTrack *gtrack, uint8_t *buffer, size_t max_len);
void MidiTrack_fwrite(struct MidiTrack *track, struct file_info *fi);
void MidiFile_fwrite(struct MidiFile *midi_file, struct file_info *fi);

#endif