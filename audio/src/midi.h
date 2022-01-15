#ifndef _GAUDIO_MIDI_H
#define _GAUDIO_MIDI_H

/**
 * Default extension when creating a MIDI file.
*/
#define MIDI_DEFAULT_EXTENSION ".midi"

#define CSEQ_FILE_NUM_TRACKS 16

#define CSEQ_FILE_HEADER_SIZE_BYTES 44

#define MIDI_COMMAND_BYTE_NOTE_OFF 0x80
#define MIDI_COMMAND_NAME_NOTE_OFF "Note Off"
#define MIDI_COMMAND_BYTE_NOTE_ON 0x90
#define MIDI_COMMAND_NAME_NOTE_ON "Note On"
#define MIDI_COMMAND_BYTE_POLYPHONIC_PRESSURE 0xa0
#define MIDI_COMMAND_NAME_POLYPHONIC_PRESSURE "Polyphonic Pressure"
#define MIDI_COMMAND_BYTE_CONTROL_CHANGE 0xb0
#define MIDI_COMMAND_NAME_CONTROL_CHANGE "Control Change"
#define MIDI_COMMAND_BYTE_PROGRAM_CHANGE 0xc0
#define MIDI_COMMAND_NAME_PROGRAM_CHANGE "Program Change"
#define MIDI_COMMAND_BYTE_CHANNEL_PRESSURE 0xd0
#define MIDI_COMMAND_NAME_CHANNEL_PRESSURE "Channel Pressure"
#define MIDI_COMMAND_BYTE_PITCH_BEND 0xe0
#define MIDI_COMMAND_NAME_PITCH_BEND "Pitch Bend"

#define CSEQ_COMMAND_BYTE_TEMPO 0x51
#define CSEQ_COMMAND_NAME_TEMPO "cseq Tempo"
#define CSEQ_COMMAND_BYTE_LOOP_END 0x2d
#define CSEQ_COMMAND_NAME_LOOP_END "cseq Loop End"
#define CSEQ_COMMAND_BYTE_LOOP_START 0x2e
#define CSEQ_COMMAND_NAME_LOOP_START "cseq Loop Start"
#define CSEQ_COMMAND_BYTE_END_OF_TRACK 0x2f
#define CSEQ_COMMAND_NAME_END_OF_TRACK "cseq End Of Track"
#define CSEQ_COMMAND_BYTE_PATTERN 0xfe
#define CSEQ_COMMAND_NAME_PATTERN "cseq pattern"

#define MIDI_DESCRIPTION_TEXT_BUFFER_LEN 40

/**
 * Individual track
*/
struct CseqTrack {

    /* begin file format (write elements to disk in order declared according to endianess) */
    
    /**
     * Raw track data.
    */
    uint8_t *data;

    /* end file format ------------------------------------------------------------------- */

    /**
     * Zero based index into parent {@code struct CseqFile.tracks}.
    */
    int32_t track_index;

    /**
     * Length in bytes of track data.
    */
    size_t data_len;

    /**
     * Data has been parsed, pattern markers have been replaced with actual byte values.
    */
    uint8_t *unrolled_data;

    /**
     * Length in bytes of unrolled_data.
    */
    size_t unrolled_data_len;
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

    /* end file format ------------------------------------------------------------------- */

    /**
     * List of tracks.
     * These can be empty/null.
    */
    struct CseqTrack *tracks[CSEQ_FILE_NUM_TRACKS];
};

#define MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN 255
extern int g_midi_parse_debug;

struct CseqTrack *CseqTrack_new(int32_t track_index);
struct CseqFile *CseqFile_new();
struct CseqFile *CseqFile_new_from_file(struct file_info *fi);
void CseqTrack_unroll(struct CseqTrack *track);
void CseqTrack_free(struct CseqTrack *track);
void CseqFile_free(struct CseqFile *cseq);

void parse_cseq_track(struct CseqTrack *track);
void midi_controller_to_name(int controller, char *result, size_t max_length);
void midi_note_to_name(int note, char* result, size_t max_length);

#endif