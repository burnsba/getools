#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "debug.h"
#include "machine_config.h"
#include "utility.h"
#include "midi.h"

/**
 * This file contains code for the N64 compressed MIDI format,
 * and regular MIDI format.
*/

int g_midi_parse_debug = 0;

/**
 * Allocates memory for a {@code struct CseqTrack}.
 * @param track_index: Track index in parent {@code struct CseqFile}.
 * @returns: pointer to new object.
*/
struct CseqTrack *CseqTrack_new(int32_t track_index)
{
    TRACE_ENTER("CseqTrack_new")

    struct CseqTrack *p = (struct CseqTrack *)malloc_zero(1, sizeof(struct CseqTrack));

    p->track_index = track_index;

    TRACE_LEAVE("CseqTrack_new")

    return p;
}

/**
 * Allocates memory for a {@code struct CseqFile}.
 * @returns: pointer to new object.
*/
struct CseqFile *CseqFile_new()
{
    TRACE_ENTER("CseqFile_new")

    struct CseqFile *p = (struct CseqFile *)malloc_zero(1, sizeof(struct CseqFile));

    TRACE_LEAVE("CseqFile_new")

    return p;
}

/**
 * Allocates memory for a {@code struct CseqFile}.
 * Reads a file and loads the contents into the new {@code struct CseqFile}.
 * @returns: pointer to new object.
*/
struct CseqFile *CseqFile_new_from_file(struct file_info *fi)
{
    TRACE_ENTER("CseqFile_new_from_file")

    int i, j;

    struct CseqFile *p = (struct CseqFile *)malloc_zero(1, sizeof(struct CseqFile));

    // make sure to begin reading at the beginning of the file.
    file_info_fseek(fi, 0, SEEK_SET);

    for (i=0; i<CSEQ_FILE_NUM_TRACKS; i++)
    {
        file_info_fread(fi, &p->track_offset[i], 4, 1);
        BSWAP32(p->track_offset[i]);

        // allocate memory now, but don't load contents until all offsets have been read from file.
        p->tracks[i] = CseqTrack_new(i);
    }

    file_info_fread(fi, &p->division, 4, 1);
    BSWAP32(p->division);

    // Now load track contents.
    // This will seek around the file.
    for (i=0; i<CSEQ_FILE_NUM_TRACKS; i++)
    {
        if (p->track_offset[i] == 0)
        {
            continue;
        }

        struct CseqTrack *cur_track = p->tracks[i];

        int32_t my_offset = p->track_offset[i];
        int32_t next_offset = INT32_MAX;
        size_t data_len = 0;

        // If the list of tracks were longer it might make sense to sort these.
        // For now, there's only 16 items, so just iterate them all to find the next offset.
        for (j=0; j<CSEQ_FILE_NUM_TRACKS; j++)
        {
            if (i == j)
            {
                continue;
            }

            if (p->track_offset[j] > my_offset && p->track_offset[j] < next_offset)
            {
                next_offset = p->track_offset[j];
            }
        }

        // if this is the last offset then there won't be one larger.
        // Set the data length as distance to end of the file.
        if (next_offset == INT32_MAX)
        {
            data_len = fi->len - my_offset - CSEQ_FILE_HEADER_SIZE_BYTES;
        }
        else
        {
            // else the data length is the difference between the next offset
            data_len = next_offset - my_offset;
        }

        cur_track->data_len = data_len;
        cur_track->data = (uint8_t *)malloc_zero(1, data_len);
        
        file_info_fseek(fi, my_offset, SEEK_SET);
        file_info_fread(fi, cur_track->data, cur_track->data_len, 1);
    }

    TRACE_LEAVE("CseqFile_new_from_file")

    return p;
}

/**
 * Frees memory allocated to track.
 * @param track: object to free.
*/
void CseqTrack_free(struct CseqTrack *track)
{
    TRACE_ENTER("CseqTrack_new")

    if (track == NULL)
    {
        return;
    }

    if (track->data_len > 0 && track->data != NULL)
    {
        free(track->data);
        track->data = NULL;
    }

    if (track->unrolled_data_len > 0 && track->unrolled_data != NULL)
    {
        free(track->unrolled_data);
        track->unrolled_data = NULL;
    }

    free(track);

    TRACE_LEAVE("CseqTrack_new")
}

/**
 * Frees memory allocated to compressed MIDI file and all child elements.
 * @param CseqFile: object to free.
*/
void CseqFile_free(struct CseqFile *cseq)
{
    TRACE_ENTER("CseqFile_free")

    int i;

    if (cseq == NULL)
    {
        return;
    }

    for (i=0; i<CSEQ_FILE_NUM_TRACKS; i++)
    {
        if (cseq->tracks[i] != NULL)
        {
            CseqTrack_free(cseq->tracks[i]);
            cseq->tracks[i] = NULL;
        }
    }

    free(cseq);

    TRACE_LEAVE("CseqFile_free")
}

/**
 * N64 compressed MIDI format has simple compression. The data sequence can contain "pattern" markers
 * referring to a location earlier in the sequence. This method unrolls the raw data and replaces
 * all markers with actual byte contents. This is a precursor to parsing the actual MIDI file.
 * This method allocates memory for the {@code struct CseqTrack.unrolled_data} and sets
 * {@code struct CseqTrack.unrolled_data_len} accordingly.
 * @param track: compressed track to parse.
*/
void CseqTrack_unroll(struct CseqTrack *track)
{
    TRACE_ENTER("CseqTrack_unroll")

    size_t pos = 0;
    size_t unrolled_pos = 0;
    uint8_t *temp_ptr;

    if (track->unrolled_data != NULL)
    {
        free(track->unrolled_data);
    }

    // rough guess here, might resize during iteration, will adjust at the end too.
    size_t new_size = (size_t)((float)track->data_len * 1.5f);
    track->unrolled_data = (uint8_t *)malloc_zero(1, new_size);

    while (pos < track->data_len)
    {
        int diff;
        int length;
        int i;

        // check size first for reading one or two bytes.
        if (unrolled_pos + 2 >= new_size)
        {
            new_size = (size_t)((float)new_size * 1.5f);
            temp_ptr = (uint8_t *)malloc_zero(1, new_size);
            memcpy(temp_ptr, track->unrolled_data, unrolled_pos);
            free(track->unrolled_data);
            track->unrolled_data = temp_ptr;
        }

        if (track->data[pos] != 0xfe)
        {
            track->unrolled_data[unrolled_pos] = track->data[pos];
            pos++;
            unrolled_pos++;
            continue;
        }

        // escape sequence
        if (track->data[pos+1] == 0xfe)
        {
            track->unrolled_data[unrolled_pos] = track->data[pos];
            
            pos += 2;
            unrolled_pos++;
            continue;
        }

        // else, pattern marker.
        diff = 0;
        diff |= track->data[pos + 1];
        diff <<= 8;
        diff |= track->data[pos + 2];

        length = track->data[pos + 3];

        // now check size for longer segment
        if (unrolled_pos + length >= new_size)
        {
            new_size = (size_t)((float)new_size * 1.5f);
            temp_ptr = (uint8_t *)malloc_zero(1, new_size);
            memcpy(temp_ptr, track->unrolled_data, unrolled_pos);
            free(track->unrolled_data);
            track->unrolled_data = temp_ptr;
        }

        for (i=0; i<length; i++)
        {
            track->unrolled_data[unrolled_pos] = track->data[pos - diff + i];
            unrolled_pos++;
        }

        pos += 4;
    }

    // resize to actual data length.
    temp_ptr = (uint8_t *)malloc_zero(1, unrolled_pos);
    memcpy(temp_ptr, track->unrolled_data, unrolled_pos);
    free(track->unrolled_data);
    track->unrolled_data = temp_ptr;
    track->unrolled_data_len = unrolled_pos;

    TRACE_LEAVE("CseqTrack_unroll")
}

// TODO: do something with parsed content
void parse_cseq_track(struct CseqTrack *track)
{
    TRACE_ENTER("parse_cseq_track")

    size_t pos = 0;
    uint8_t b;
    int done = 0;
    int command_channel = 0;
    int note;
    int velocity;
    struct var_length_int varint;
    char print_buffer[MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN];
    char description_buffer[MIDI_DESCRIPTION_TEXT_BUFFER_LEN];
    int message_type;
    int need_delta_time = 1;
    int current_command = -1;

    if (track->unrolled_data == NULL)
    {
        CseqTrack_unroll(track);
    }

    memset(&varint, 0, sizeof(struct var_length_int));

    while (!done && pos < track->unrolled_data_len)
    {
        // command/status message
        if (need_delta_time == 0 && (track->unrolled_data[pos] & 0x80))
        {
            need_delta_time = 0;

            b = track->unrolled_data[pos++];
            message_type = b & 0xf0;
            command_channel = b & 0x0f;

            // system command
            if (b == 0xff)
            {
                current_command = -1;
                need_delta_time = 1;

                b = track->unrolled_data[pos++];
                
                if (b == CSEQ_COMMAND_BYTE_TEMPO)
                {
                    int tempo = 0;

                    tempo |= track->unrolled_data[pos++];
                    tempo <<= 8;
                    tempo |= track->unrolled_data[pos++];
                    tempo <<= 8;
                    tempo |= track->unrolled_data[pos++];

                    if (g_midi_parse_debug)
                    {
                        memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                        snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: %d", CSEQ_COMMAND_NAME_TEMPO, tempo);
                        fflush_printf(stdout, "%s\n", print_buffer);
                    }
                }
                else if (b == CSEQ_COMMAND_BYTE_LOOP_START)
                {
                    int loop_number = 0;

                    loop_number = track->unrolled_data[pos++];
                    b = track->unrolled_data[pos++];
                    if (b != 0xff)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "parse_cseq_track: %s: expected end of command byte 0xff but read '0x%x', track index=%d, pos=%d.\n", CSEQ_COMMAND_NAME_LOOP_START, b, track->track_index, pos);
                    }

                    if (g_midi_parse_debug)
                    {
                        memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                        snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: loop number %d", CSEQ_COMMAND_NAME_LOOP_START, loop_number);
                        fflush_printf(stdout, "%s\n", print_buffer);
                    }
                }
                else if (b == CSEQ_COMMAND_BYTE_LOOP_END)
                {
                    int loop_count = 0;
                    int current_loop_count = 0;
                    int32_t loop_difference = 0;

                    loop_count = track->unrolled_data[pos++];
                    current_loop_count = track->unrolled_data[pos++];

                    loop_difference = track->unrolled_data[pos++];
                    loop_difference <<= 8;
                    loop_difference = track->unrolled_data[pos++];
                    loop_difference <<= 8;
                    loop_difference = track->unrolled_data[pos++];
                    loop_difference <<= 8;
                    loop_difference = track->unrolled_data[pos++];

                    if (g_midi_parse_debug)
                    {
                        memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                        snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: count %d, current count %d, diff 0x%08x", CSEQ_COMMAND_NAME_LOOP_END, loop_count, current_loop_count, loop_difference);
                        fflush_printf(stdout, "%s\n", print_buffer);
                    }
                }
                else if (b == CSEQ_COMMAND_BYTE_END_OF_TRACK)
                {
                    if (g_midi_parse_debug)
                    {
                        memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                        snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s", CSEQ_COMMAND_NAME_END_OF_TRACK);
                        fflush_printf(stdout, "%s\n", print_buffer);
                    }
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "parse_cseq_track: parse error (system command), track index=%d, pos=%d.\n", track->track_index, pos);
                }

                continue;
            }
            else if (b == CSEQ_COMMAND_BYTE_PATTERN)
            {
                int32_t pattern_difference = 0;
                int pattern_length = 0;

                pattern_difference = track->unrolled_data[pos++];
                pattern_difference <<= 8;
                pattern_difference = track->unrolled_data[pos++];
                
                pattern_length = track->unrolled_data[pos++];

                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: diff 0x%08x, length=%d", CSEQ_COMMAND_NAME_PATTERN, pattern_difference, pattern_length);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }
            }
            else if (message_type == MIDI_COMMAND_BYTE_NOTE_OFF)
            {
                // note off
                stderr_exit(EXIT_CODE_GENERAL, "parse_cseq_track: parse error -- invalid compressed MIDI, \"note off\" command not allowed. Track index=%d, pos=%d.\n", track->track_index, pos);
            }
            else if (message_type == MIDI_COMMAND_BYTE_NOTE_ON)
            {
                current_command = MIDI_COMMAND_BYTE_NOTE_ON;

                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "command %s: channel %d", MIDI_COMMAND_NAME_NOTE_ON, command_channel);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }
            }
            else if (message_type == MIDI_COMMAND_BYTE_POLYPHONIC_PRESSURE)
            {
                current_command = MIDI_COMMAND_BYTE_POLYPHONIC_PRESSURE;

                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "command %s: channel %d", MIDI_COMMAND_NAME_NOTE_ON, command_channel);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }
            }
            else if (message_type == MIDI_COMMAND_BYTE_CONTROL_CHANGE)
            {
                current_command = MIDI_COMMAND_BYTE_CONTROL_CHANGE;

                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "command %s: channel %d", MIDI_COMMAND_NAME_NOTE_ON, command_channel);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }
            }
            else if (message_type == MIDI_COMMAND_BYTE_PROGRAM_CHANGE)
            {
                current_command = MIDI_COMMAND_BYTE_PROGRAM_CHANGE;

                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "command %s: channel %d", MIDI_COMMAND_NAME_NOTE_ON, command_channel);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }
            }
            else if (message_type == MIDI_COMMAND_BYTE_CHANNEL_PRESSURE)
            {
                current_command = MIDI_COMMAND_BYTE_CHANNEL_PRESSURE;

                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "command %s: channel %d", MIDI_COMMAND_NAME_NOTE_ON, command_channel);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }
            }
            else if (message_type == MIDI_COMMAND_BYTE_PITCH_BEND)
            {
                // Pitch Bend
                stderr_exit(EXIT_CODE_GENERAL, "parse_cseq_track: Pitch Bend not implemented, track index=%d, pos=%d.\n", track->track_index, pos);
            }
            else
            {
                stderr_exit(EXIT_CODE_GENERAL, "parse_cseq_track: parse error (command), track index=%d, pos=%d.\n", track->track_index, pos);
            }
        }
        
        // data bytes
        if (need_delta_time == 0)
        {
            need_delta_time = 1;

            /**
             * implementation note: make sure pos is incrememnted in each every block!
            */

            if (current_command == MIDI_COMMAND_BYTE_NOTE_OFF)
            {
                // note off
                stderr_exit(EXIT_CODE_GENERAL, "parse_cseq_track: parse error -- invalid compressed MIDI, \"note off\" command not allowed. Track index=%d, pos=%d.\n", track->track_index, pos);
            }
            else if (current_command == MIDI_COMMAND_BYTE_NOTE_ON)
            {
                // note on
                note = track->unrolled_data[pos++];
                velocity = track->unrolled_data[pos++];

                memset(&varint, 0, sizeof(struct var_length_int));
                varint_value_to_int32(&track->unrolled_data[pos], 4, &varint);
                pos += varint.num_bytes;

                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);
                    memset(description_buffer, 0, MIDI_DESCRIPTION_TEXT_BUFFER_LEN);

                    midi_note_to_name(note, description_buffer, MIDI_DESCRIPTION_TEXT_BUFFER_LEN);
                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: channel %d, %s velocity=%d, duration=%d", MIDI_COMMAND_NAME_NOTE_ON, command_channel, description_buffer, velocity, varint.standard_value);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }
            }
            else if (current_command == MIDI_COMMAND_BYTE_POLYPHONIC_PRESSURE)
            {
                // Polyphonic Pressure
                note = track->unrolled_data[pos++];
                int pressure = track->unrolled_data[pos++];

                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);
                    memset(description_buffer, 0, MIDI_DESCRIPTION_TEXT_BUFFER_LEN);

                    midi_note_to_name(note, description_buffer, MIDI_DESCRIPTION_TEXT_BUFFER_LEN);
                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: channel %d, %s pressure=%d", MIDI_COMMAND_NAME_POLYPHONIC_PRESSURE, command_channel, description_buffer, pressure);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }
            }
            else if (current_command == MIDI_COMMAND_BYTE_CONTROL_CHANGE)
            {
                // Control Change
                int controller = track->unrolled_data[pos++];
                int new_value = track->unrolled_data[pos++];

                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);
                    memset(description_buffer, 0, MIDI_DESCRIPTION_TEXT_BUFFER_LEN);

                    midi_controller_to_name(controller, description_buffer, MIDI_DESCRIPTION_TEXT_BUFFER_LEN);
                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: channel %d, controller %d = %s, new_value=%d", MIDI_COMMAND_NAME_CONTROL_CHANGE, command_channel, controller, description_buffer, new_value);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }
            }
            else if (current_command == MIDI_COMMAND_BYTE_PROGRAM_CHANGE)
            {
                // Program Change
                int program = track->unrolled_data[pos++];

                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: channel %d, program=%d", MIDI_COMMAND_NAME_PROGRAM_CHANGE, command_channel, program);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }
            }
            else if (current_command == MIDI_COMMAND_BYTE_CHANNEL_PRESSURE)
            {
                // Channel Pressure
                int pressure = track->unrolled_data[pos++];

                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: channel %d, pressure=%d", MIDI_COMMAND_NAME_CHANNEL_PRESSURE, command_channel, pressure);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }
            }
            else if (current_command == MIDI_COMMAND_BYTE_PITCH_BEND)
            {
                // Pitch Bend
                stderr_exit(EXIT_CODE_GENERAL, "parse_cseq_track: Pitch Bend not implemented, track index=%d, pos=%d.\n", track->track_index, pos);
            }
            else
            {
                stderr_exit(EXIT_CODE_GENERAL, "parse_cseq_track: parse error (command=%d), track index=%d, pos=%d.\n", current_command, track->track_index, pos);
            }
        }
        else
        {
            need_delta_time = 0;

            memset(&varint, 0, sizeof(struct var_length_int));
            varint_value_to_int32(&track->unrolled_data[pos], 4, &varint);

            if (varint.num_bytes < 1)
            {
                stderr_exit(EXIT_CODE_GENERAL, "parse_cseq_track: parse error, could not read variable length integer. Track index=%d, pos=%ld\n", track->track_index, pos);
            }

            pos += varint.num_bytes;

            if (g_midi_parse_debug)
            {
                memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                snprintf(print_buffer, MIDI_DESCRIPTION_TEXT_BUFFER_LEN, "delta time: %d (varint=%d)", varint.standard_value, varint.value);
                fflush_printf(stdout, "%s\n", print_buffer);
            }
        }
    }   

    if (pos > track->unrolled_data_len)
    {
        stderr_exit(EXIT_CODE_GENERAL, "parse_cseq_track: exceeded track length during parse. Track index=%d\n", track->track_index);
    }

    TRACE_LEAVE("parse_cseq_track")
}

/**
 * Converts controller parameter id to text name.
 * @param controller: id of controller.
 * @param result: out parameter, will contain text result. Must be previously allocated.
 * @param max_length: max string length of {@code result}.
*/
void midi_controller_to_name(int controller, char *result, size_t max_length)
{
    TRACE_ENTER("midi_controller_to_name")

    switch (controller)
    {
        case 0x00: snprintf(result, max_length, "bank select"); break;
        case 0x07: snprintf(result, max_length, "channel volume"); break;
        case 0x08: snprintf(result, max_length, "balance"); break;
        case 0x0a: snprintf(result, max_length, "pan"); break;
        case 0x2a: snprintf(result, max_length, "pan"); break;
        case 0x5b: snprintf(result, max_length, "effects 1 depth"); break;
        case 0x5c: snprintf(result, max_length, "effects 2 depth"); break;
        case 0x5d: snprintf(result, max_length, "effects 3 depth"); break;
        case 0x5e: snprintf(result, max_length, "effects 4 depth"); break;
        case 0x5f: snprintf(result, max_length, "effects 5 depth"); break;
        
        default:
            snprintf(result, max_length, "unknown controller 0x%x", controller);
        break;
    }

    TRACE_LEAVE("midi_controller_to_name")
}

/**
 * Converts note id to text name.
 * @param note: id of note.
 * @param result: out parameter, will contain text result. Must be previously allocated.
 * @param max_length: max string length of {@code result}.
*/
void midi_note_to_name(int note, char* result, size_t max_length)
{
    TRACE_ENTER("midi_note_to_name")

    int octave = -1;

    while (note > 11)
    {
        note -= 12;
        octave++;
    }

    switch (note)
    {
        case 0:
        snprintf(result, max_length, "C (octave %d)", octave);
        break;

        case 1:
        snprintf(result, max_length, "C# (octave %d)", octave);
        break;

        case 2:
        snprintf(result, max_length, "D (octave %d)", octave);
        break;

        case 3:
        snprintf(result, max_length, "D# (octave %d)", octave);
        break;

        case 4:
        snprintf(result, max_length, "E (octave %d)", octave);
        break;

        case 5:
        snprintf(result, max_length, "F (octave %d)", octave);
        break;

        case 6:
        snprintf(result, max_length, "F# (octave %d)", octave);
        break;

        case 7:
        snprintf(result, max_length, "G (octave %d)", octave);
        break;

        case 8:
        snprintf(result, max_length, "G# (octave %d)", octave);
        break;

        case 9:
        snprintf(result, max_length, "A (octave %d)", octave);
        break;

        case 10:
        snprintf(result, max_length, "A# (octave %d)", octave);
        break;

        case 11:
        snprintf(result, max_length, "B (octave %d)", octave);
        break;

        default:
        stderr_exit(EXIT_CODE_GENERAL, "midi_note_to_name: note=%d note supported.\n", note);
        break;
    }

    TRACE_LEAVE("midi_note_to_name")
}