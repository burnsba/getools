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
 * Allocates memory for a {@code struct MidiTrack}.
 * @param track_index: Track index in parent {@code struct MidiFile}.
 * @returns: pointer to new object.
*/
struct MidiTrack *MidiTrack_new(int32_t track_index)
{
    TRACE_ENTER("MidiTrack_new")

    struct MidiTrack *p = (struct MidiTrack *)malloc_zero(1, sizeof(struct MidiTrack));

    p->ck_id = MIDI_TRACK_CHUNK_ID;
    p->track_index = track_index;

    TRACE_LEAVE("MidiTrack_new")

    return p;
}

/**
 * Allocates memory for a {@code struct MidiFile}.
 * @param format: MIDI file format.
 * @returns: pointer to new object.
*/
struct MidiFile *MidiFile_new(int format)
{
    TRACE_ENTER("MidiFile_new")

    struct MidiFile *p = (struct MidiFile *)malloc_zero(1, sizeof(struct MidiFile));

    p->ck_id = MIDI_ROOT_CHUNK_ID;
    p->ck_data_size = MIDI_ROOT_CHUNK_BODY_SIZE;
    p->format = format;

    TRACE_LEAVE("MidiFile_new")

    return p;
}

/**
 * Allocates memory for a {@code struct MidiFile} and allocates memory for tracks list.
 * @param format: MIDI file format.
 * @param num_tracks: number of tracks to allocate pointers for.
 * @returns: pointer to new object.
*/
struct MidiFile *MidiFile_new_tracks(int format, int num_tracks)
{
    TRACE_ENTER("MidiFile_new_tracks")

    struct MidiFile *p = (struct MidiFile *)malloc_zero(1, sizeof(struct MidiFile));

    p->ck_id = MIDI_ROOT_CHUNK_ID;
    p->ck_data_size = MIDI_ROOT_CHUNK_BODY_SIZE;
    p->format = format;
    p->num_tracks = num_tracks;

    p->tracks = (struct MidiTrack **)malloc_zero(num_tracks, sizeof(struct MidiTrack *));

    TRACE_LEAVE("MidiFile_new_tracks")

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

        p->non_empty_num_tracks++;

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

struct MidiTrack *MidiTrack_new_from_GmidTrack(struct GmidTrack *gtrack)
{
    TRACE_ENTER("MidiTrack_new_from_GmidTrack")

    struct MidiTrack *p = (struct MidiTrack *)malloc_zero(1, sizeof(struct MidiTrack));

    p->ck_id = MIDI_TRACK_CHUNK_ID;
    p->track_index = gtrack->track_index;

    p->data = (uint8_t *)malloc_zero(1, gtrack->track_size_bytes);
    GmidTrack_write_to_midi_buffer(gtrack, p->data, gtrack->track_size_bytes);

    TRACE_LEAVE("MidiTrack_new_from_GmidTrack")

    return p;
}

struct MidiFile *MidiFile_from_CseqFile(struct CseqFile *cseq)
{
    TRACE_ENTER("MidiFile_from_CseqFile")

    int i;
    int allocated_tracks = 0;

    struct MidiFile *midi = MidiFile_new_tracks(MIDI_FORMAT_SIMULTANEOUS, cseq->non_empty_num_tracks);

    midi->division = cseq->division;

    for (i=0; i<CSEQ_FILE_NUM_TRACKS; i++)
    {
        if (cseq->tracks[i] == NULL || cseq->track_offset[i] == 0)
        {
            continue;
        }

        struct GmidTrack *gtrack = parse_cseq_track(cseq->tracks[i]);
        gtrack->track_index = allocated_tracks;
        midi->tracks[allocated_tracks] = MidiTrack_new_from_GmidTrack(gtrack);
        allocated_tracks++;

        GmidTrack_free(gtrack);
    }

    return midi;

    TRACE_LEAVE("MidiFile_from_CseqFile")
}

/**
 * Allocates memory for a {@code struct GmidEvent}.
 * @returns: pointer to new object.
*/
struct GmidEvent *GmidEvent_new()
{
    TRACE_ENTER("GmidEvent_new")

    struct GmidEvent *p = (struct GmidEvent *)malloc_zero(1, sizeof(struct GmidEvent));

    TRACE_LEAVE("GmidEvent_new")

    return p;
}

/**
 * Allocates memory for a {@code struct GmidTrack}.
 * @returns: pointer to new object.
*/
struct GmidTrack *GmidTrack_new()
{
    TRACE_ENTER("GmidTrack_new")

    struct GmidTrack *p = (struct GmidTrack *)malloc_zero(1, sizeof(struct GmidTrack));
    p->events = llist_root_new();

    TRACE_LEAVE("GmidTrack_new")

    return p;
}

/**
 * Frees memory allocated to event and all child objects.
 * @param event: object to free.
*/
void GmidEvent_free(struct GmidEvent *event)
{
    TRACE_ENTER("GmidEvent_free")

    if (event == NULL)
    {
        return;
    }

    free(event);

    TRACE_LEAVE("GmidEvent_free")
}

/**
 * Frees memory allocated to track and all child objects.
 * @returns: pointer to new object.
*/
void GmidTrack_free(struct GmidTrack *track)
{
    TRACE_ENTER("GmidTrack_free")

    if (track == NULL)
    {
        return;
    }

    if (track->events != NULL)
    {
        struct GmidEvent *data;
        struct llist_node *node;
        struct llist_node *next;

        node = track->events->root;
        while (node != NULL)
        {
            data = (struct GmidEvent *)node->data;
            if (data != NULL)
            {
                GmidEvent_free(data);
            }

            next = node->next;
            llist_node_free(NULL, node);
            node = next;
        }

        track->events->root = NULL;
        llist_node_root_free(track->events);
        track->events = NULL;
    }

    free(track);

    TRACE_LEAVE("GmidTrack_free")
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
 * Frees memory allocated to track.
 * @param track: object to free.
*/
void MidiTrack_free(struct MidiTrack *track)
{
    TRACE_ENTER("MidiTrack_new")

    if (track == NULL)
    {
        return;
    }

    if (track->ck_data_size > 0 && track->data != NULL)
    {
        free(track->data);
        track->data = NULL;
    }

    free(track);

    TRACE_LEAVE("MidiTrack_new")
}

/**
 * Frees memory allocated to compressed MIDI file and all child elements.
 * @param MidiFile: object to free.
*/
void MidiFile_free(struct MidiFile *midi)
{
    TRACE_ENTER("MidiFile_free")

    int i;

    if (midi == NULL)
    {
        return;
    }

    for (i=0; i<midi->num_tracks; i++)
    {
        if (midi->tracks[i] != NULL)
        {
            MidiTrack_free(midi->tracks[i]);
            midi->tracks[i] = NULL;
        }
    }

    free(midi);

    TRACE_LEAVE("MidiFile_free")
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

struct GmidTrack *parse_cseq_track(struct CseqTrack *track)
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
    int event_id = 0;
    int param_len = 0;
    int append_event = 0;
    long absolute_time = 0;
    int fix_delta_times = 0;
    int i, j;

    struct GmidTrack *result = GmidTrack_new();
    struct llist_node *node;
    struct GmidEvent *event = NULL;
    struct GmidEvent *event_dual = NULL;

    if (track->unrolled_data == NULL)
    {
        CseqTrack_unroll(track);
    }

    memset(&varint, 0, sizeof(struct var_length_int));

    while (!done && pos < track->unrolled_data_len)
    {
        append_event = 0;

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

                    if (event == NULL)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "%ld parse_cseq_track: parse error, event is NULL. track index=%d, pos=%d.\n", __LINE__, track->track_index, pos);
                    }

                    // copy raw value in same endian, other values set at end.
                    memcpy(event->command_parameters_raw, &track->unrolled_data[pos], CSEQ_COMMAND_PARAM_BYTE_TEMPO);

                    // parse command values.
                    tempo |= track->unrolled_data[pos++];
                    tempo <<= 8;
                    tempo |= track->unrolled_data[pos++];
                    tempo <<= 8;
                    tempo |= track->unrolled_data[pos++];

                    // optional debug print
                    if (g_midi_parse_debug)
                    {
                        memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                        snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: %d", CSEQ_COMMAND_NAME_TEMPO, tempo);
                        fflush_printf(stdout, "%s\n", print_buffer);
                    }

                    // save parsed data into common format.
                    event->command = 0xff00 | CSEQ_COMMAND_BYTE_TEMPO;
                    event->command_len = CSEQ_COMMAND_LEN_TEMPO;
                    event->command_parameters_raw_len = CSEQ_COMMAND_PARAM_BYTE_TEMPO;
                    event->command_parameters[0] = tempo;
                    event->command_parameters_len = CSEQ_COMMAND_NUM_PARAM_TEMPO;
                    event->dual = NULL;
                }
                else if (b == CSEQ_COMMAND_BYTE_LOOP_START)
                {
                    int loop_number = 0;

                    if (event == NULL)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "%ld parse_cseq_track: parse error, event is NULL. track index=%d, pos=%d.\n", __LINE__, track->track_index, pos);
                    }

                    // copy raw value in same endian, other values set at end.
                    memcpy(event->command_parameters_raw, &track->unrolled_data[pos], CSEQ_COMMAND_PARAM_BYTE_LOOP_START);

                    // parse command values.
                    loop_number = track->unrolled_data[pos++];
                    b = track->unrolled_data[pos++];
                    if (b != 0xff)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "parse_cseq_track: %s: expected end of command byte 0xff but read '0x%x', track index=%d, pos=%d.\n", CSEQ_COMMAND_NAME_LOOP_START, b, track->track_index, pos);
                    }

                    // optional debug print
                    if (g_midi_parse_debug)
                    {
                        memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                        snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: loop number %d", CSEQ_COMMAND_NAME_LOOP_START, loop_number);
                        fflush_printf(stdout, "%s\n", print_buffer);
                    }

                    // save parsed data into common format.
                    event->command = 0xff00 | CSEQ_COMMAND_BYTE_LOOP_START;
                    event->command_len = CSEQ_COMMAND_LEN_LOOP_START;
                    event->command_parameters_raw_len = CSEQ_COMMAND_PARAM_BYTE_LOOP_START;
                    event->command_parameters[0] = loop_number;
                    event->command_parameters[1] = b;
                    event->command_parameters_len = CSEQ_COMMAND_NUM_PARAM_LOOP_START;
                    event->dual = NULL;
                }
                else if (b == CSEQ_COMMAND_BYTE_LOOP_END)
                {
                    int loop_count = 0;
                    int current_loop_count = 0;
                    int32_t loop_difference = 0;

                    if (event == NULL)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "%ld parse_cseq_track: parse error, event is NULL. track index=%d, pos=%d.\n", __LINE__, track->track_index, pos);
                    }

                    // copy raw value in same endian, other values set at end.
                    memcpy(event->command_parameters_raw, &track->unrolled_data[pos], CSEQ_COMMAND_PARAM_BYTE_LOOP_END);

                    // parse command values.
                    loop_count = track->unrolled_data[pos++];
                    current_loop_count = track->unrolled_data[pos++];

                    loop_difference = track->unrolled_data[pos++];
                    loop_difference <<= 8;
                    loop_difference = track->unrolled_data[pos++];
                    loop_difference <<= 8;
                    loop_difference = track->unrolled_data[pos++];
                    loop_difference <<= 8;
                    loop_difference = track->unrolled_data[pos++];

                    // optional debug print
                    if (g_midi_parse_debug)
                    {
                        memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                        snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: count %d, current count %d, diff 0x%08x", CSEQ_COMMAND_NAME_LOOP_END, loop_count, current_loop_count, loop_difference);
                        fflush_printf(stdout, "%s\n", print_buffer);
                    }

                    // save parsed data into common format.
                    event->command = 0xff00 | CSEQ_COMMAND_BYTE_LOOP_END;
                    event->command_len = CSEQ_COMMAND_LEN_LOOP_END;
                    event->command_parameters_raw_len = CSEQ_COMMAND_PARAM_BYTE_LOOP_END;
                    event->command_parameters[0] = loop_count;
                    event->command_parameters[1] = current_loop_count;
                    event->command_parameters[2] = loop_difference;
                    event->command_parameters_len = CSEQ_COMMAND_NUM_PARAM_LOOP_END;
                    event->dual = NULL;
                }
                else if (b == CSEQ_COMMAND_BYTE_END_OF_TRACK)
                {
                    if (event == NULL)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "%ld parse_cseq_track: parse error, event is NULL. track index=%d, pos=%d.\n", __LINE__, track->track_index, pos);
                    }

                    // no parameters to copy.

                    // no values to parse.

                    // optional debug print
                    if (g_midi_parse_debug)
                    {
                        memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                        snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s", CSEQ_COMMAND_NAME_END_OF_TRACK);
                        fflush_printf(stdout, "%s\n", print_buffer);
                    }

                    // save parsed data into common format.
                    event->command = 0xff00 | CSEQ_COMMAND_BYTE_END_OF_TRACK;
                    event->command_len = CSEQ_COMMAND_LEN_END_OF_TRACK;
                    event->command_parameters_raw_len = CSEQ_COMMAND_PARAM_BYTE_END_OF_TRACK;
                    event->command_parameters_len = CSEQ_COMMAND_NUM_PARAM_END_OF_TRACK;
                    event->dual = NULL;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "parse_cseq_track: parse error (system command), track index=%d, pos=%d.\n", track->track_index, pos);
                }

                // append common format as new node into result list, reset for next event.
                node = llist_node_new();
                node->data = event;
                llist_root_append_node(result, node);
                node = NULL;
                event = NULL;

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

                stderr_exit(EXIT_CODE_GENERAL, "parse_cseq_track: parse error -- invalid compressed MIDI, \"pattern\" command not allowed. Track index=%d, pos=%d.\n", track->track_index, pos);
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
            param_len = 0;

            /**
             * implementation note: make sure pos is incrememnted in each every block!
            */

           if (event == NULL)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%ld parse_cseq_track: parse error, event is NULL. track index=%d, pos=%d.\n", __LINE__, track->track_index, pos);
            }

            if (current_command == MIDI_COMMAND_BYTE_NOTE_OFF)
            {
                // note off
                stderr_exit(EXIT_CODE_GENERAL, "parse_cseq_track: parse error -- invalid compressed MIDI, \"note off\" command not allowed. Track index=%d, pos=%d.\n", track->track_index, pos);
            }
            else if (current_command == MIDI_COMMAND_BYTE_NOTE_ON)
            {
                // parse command values.
                note = track->unrolled_data[pos++];
                velocity = track->unrolled_data[pos++];

                memset(&varint, 0, sizeof(struct var_length_int));
                varint_value_to_int32(&track->unrolled_data[pos], 4, &varint);
                pos += varint.num_bytes;

                param_len = 2 + varint.num_bytes;

                // copy raw value in same endian, other values set at end.
                memcpy(event->command_parameters_raw, &track->unrolled_data[pos - param_len], param_len);

                // optional debug print
                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);
                    memset(description_buffer, 0, MIDI_DESCRIPTION_TEXT_BUFFER_LEN);

                    midi_note_to_name(note, description_buffer, MIDI_DESCRIPTION_TEXT_BUFFER_LEN);
                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: channel %d, %s velocity=%d, duration=%d", MIDI_COMMAND_NAME_NOTE_ON, command_channel, description_buffer, velocity, varint.standard_value);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }

                // save parsed data into common format.
                event->command = MIDI_COMMAND_BYTE_NOTE_ON;
                event->command_channel = command_channel;
                event->command_len = MIDI_COMMAND_LEN_NOTE_ON;
                event->command_parameters_raw_len = param_len;
                event->command_parameters[0] = note;
                event->command_parameters[1] = velocity;
                event->command_parameters[2] = varint.standard_value; // duration
                event->command_parameters_len = CSEQ_COMMAND_NUM_PARAM_NOTE_ON;
                event->dual = NULL;

                append_event = 1;
            }
            else if (current_command == MIDI_COMMAND_BYTE_POLYPHONIC_PRESSURE)
            {
                if (event == NULL)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%ld parse_cseq_track: parse error, event is NULL. track index=%d, pos=%d.\n", __LINE__, track->track_index, pos);
                }

                // copy raw value in same endian, other values set at end.
                memcpy(event->command_parameters_raw, &track->unrolled_data[pos], MIDI_COMMAND_PARAM_BYTE_POLYPHONIC_PRESSURE);
                
                // parse command values.
                note = track->unrolled_data[pos++];
                int pressure = track->unrolled_data[pos++];

                // optional debug print
                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);
                    memset(description_buffer, 0, MIDI_DESCRIPTION_TEXT_BUFFER_LEN);

                    midi_note_to_name(note, description_buffer, MIDI_DESCRIPTION_TEXT_BUFFER_LEN);
                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: channel %d, %s pressure=%d", MIDI_COMMAND_NAME_POLYPHONIC_PRESSURE, command_channel, description_buffer, pressure);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }

                // save parsed data into common format.
                event->command = MIDI_COMMAND_BYTE_POLYPHONIC_PRESSURE;
                event->command_channel = command_channel;
                event->command_len = MIDI_COMMAND_LEN_POLYPHONIC_PRESSURE;
                event->command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_POLYPHONIC_PRESSURE;
                event->command_parameters[0] = note;
                event->command_parameters[1] = pressure;
                event->command_parameters_len = MIDI_COMMAND_NUM_PARAM_POLYPHONIC_PRESSURE;
                event->dual = NULL;

                append_event = 1;
            }
            else if (current_command == MIDI_COMMAND_BYTE_CONTROL_CHANGE)
            {
                if (event == NULL)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%ld parse_cseq_track: parse error, event is NULL. track index=%d, pos=%d.\n", __LINE__, track->track_index, pos);
                }

                // copy raw value in same endian, other values set at end.
                memcpy(event->command_parameters_raw, &track->unrolled_data[pos], MIDI_COMMAND_PARAM_BYTE_CONTROL_CHANGE);
                
                // parse command values.
                int controller = track->unrolled_data[pos++];
                int new_value = track->unrolled_data[pos++];

                // optional debug print
                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);
                    memset(description_buffer, 0, MIDI_DESCRIPTION_TEXT_BUFFER_LEN);

                    midi_controller_to_name(controller, description_buffer, MIDI_DESCRIPTION_TEXT_BUFFER_LEN);
                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: channel %d, controller %d = %s, new_value=%d", MIDI_COMMAND_NAME_CONTROL_CHANGE, command_channel, controller, description_buffer, new_value);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }

                // save parsed data into common format.
                event->command = MIDI_COMMAND_BYTE_CONTROL_CHANGE;
                event->command_channel = command_channel;
                event->command_len = MIDI_COMMAND_LEN_CONTROL_CHANGE;
                event->command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_CONTROL_CHANGE;
                event->command_parameters[0] = controller;
                event->command_parameters[1] = new_value;
                event->command_parameters_len = MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
                event->dual = NULL;

                append_event = 1;
            }
            else if (current_command == MIDI_COMMAND_BYTE_PROGRAM_CHANGE)
            {
                if (event == NULL)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%ld parse_cseq_track: parse error, event is NULL. track index=%d, pos=%d.\n", __LINE__, track->track_index, pos);
                }

                // copy raw value in same endian, other values set at end.
                memcpy(event->command_parameters_raw, &track->unrolled_data[pos], MIDI_COMMAND_PARAM_BYTE_PROGRAM_CHANGE);
                
                // parse command values.
                int program = track->unrolled_data[pos++];

                // optional debug print
                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: channel %d, program=%d", MIDI_COMMAND_NAME_PROGRAM_CHANGE, command_channel, program);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }

                // save parsed data into common format.
                event->command = MIDI_COMMAND_BYTE_PROGRAM_CHANGE;
                event->command_channel = command_channel;
                event->command_len = MIDI_COMMAND_LEN_PROGRAM_CHANGE;
                event->command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_PROGRAM_CHANGE;
                event->command_parameters[0] = program;
                event->command_parameters_len = MIDI_COMMAND_NUM_PARAM_PROGRAM_CHANGE;
                event->dual = NULL;

                append_event = 1;
            }
            else if (current_command == MIDI_COMMAND_BYTE_CHANNEL_PRESSURE)
            {
                if (event == NULL)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%ld parse_cseq_track: parse error, event is NULL. track index=%d, pos=%d.\n", __LINE__, track->track_index, pos);
                }

                // copy raw value in same endian, other values set at end.
                memcpy(event->command_parameters_raw, &track->unrolled_data[pos], MIDI_COMMAND_PARAM_BYTE_CHANNEL_PRESSURE);
                
                // parse command values.
                int pressure = track->unrolled_data[pos++];

                // optional debug print
                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: channel %d, pressure=%d", MIDI_COMMAND_NAME_CHANNEL_PRESSURE, command_channel, pressure);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }

                // save parsed data into common format.
                event->command = MIDI_COMMAND_BYTE_CHANNEL_PRESSURE;
                event->command_channel = command_channel;
                event->command_len = MIDI_COMMAND_LEN_CHANNEL_PRESSURE;
                event->command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_CHANNEL_PRESSURE;
                event->command_parameters[0] = pressure;
                event->command_parameters_len = MIDI_COMMAND_NUM_PARAM_CHANNEL_PRESSURE;
                event->dual = NULL;

                append_event = 1;
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

            if (append_event)
            {
                append_event = 0;

                // append common format as new node into result list, reset for next event.
                node = llist_node_new();
                node->data = event;
                llist_root_append_node(result, node);
                node = NULL;
                event = NULL;
            }
        }
        else
        {
            need_delta_time = 0;

            if (event != NULL)
            {
                stderr_exit(EXIT_CODE_GENERAL, "parse_cseq_track: parse error, reading delta time before previous delta time was processed. Track index=%d, pos=%ld\n", track->track_index, pos);
            }

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

            absolute_time += varint.standard_value;

            event = GmidEvent_new();
            event->id = event_id++;
            event->absolute_time = absolute_time;
            varint_copy(&event->varint_delta_time, &varint);
        }
    }   

    if (pos > track->unrolled_data_len)
    {
        stderr_exit(EXIT_CODE_GENERAL, "parse_cseq_track: exceeded track length during parse. Track index=%d\n", track->track_index);
    }

    if (event != NULL)
    {
        stderr_exit(EXIT_CODE_GENERAL, "parse_cseq_track: parse error, unresolved event. Track index=%d\n", track->track_index);
    }

    result->track_size_bytes = 0;

    // create Note Off events from Note On events.
    // This also counts the midi_track_size_bytes.
    node = result->events->root;
    while (node != NULL)
    {
        event = (struct GmidEvent *)node->data;

        result->track_size_bytes += event->varint_delta_time.num_bytes + event->command_len + event->command_parameters_raw_len;

        if (event->command == (0xff00 | MIDI_COMMAND_BYTE_NOTE_ON))
        {
            struct GmidEvent *noteoff = GmidEvent_new();
            struct llist_node *temp_node;

            noteoff->id = event_id++;
            noteoff->absolute_time = event->absolute_time + (long)event->command_parameters[2];
            // delta event time is now broken, will fix after all new nodes have been created.

            noteoff->command = MIDI_COMMAND_BYTE_NOTE_OFF;
            noteoff->command_channel = command_channel;
            noteoff->command_len = MIDI_COMMAND_LEN_NOTE_OFF;
            noteoff->command_parameters_raw_len = param_len;
            noteoff->command_parameters[0] = event->command_parameters[0]; // note
            noteoff->command_parameters[1] = noteoff->command_parameters[1]; // velocity
            noteoff->command_parameters_len = MIDI_COMMAND_NUM_PARAM_NOTE_OFF;

            // add to total track byte size count
            result->track_size_bytes += noteoff->varint_delta_time.num_bytes + noteoff->command_len + noteoff->command_parameters_raw_len;
            
            noteoff->dual = event;
            event->dual = noteoff;

            temp_node = llist_node_new();
            temp_node->data = noteoff;
            
            // Don't want to append the temp_node anywhere forward in the list since
            // that's still being iterated. Therefore just insert before current.
            llist_node_insert_before(result, node, temp_node);

            fix_delta_times = 1;
        }

        node = node->next;
    }

    // Now would be a good time to resolve the loop start <-> loop end dual pointers.
    // Maybe one day.

    // Now sort the events by absolute time and set delta times to correct values.
    if (fix_delta_times)
    {
        struct llist_node *next_node;
        struct llist_node *best_node;
        struct llist_node *t;
        long best_absolute_time;

        node = result->events->root;
        for (i=0; i<result->events->count && node != NULL; i++)
        {
            next_node = node->next;
            event = (struct GmidEvent *)node->data;

            best_node = node;
            best_absolute_time = event->absolute_time;

            t = best_node;
            for (j=i; j<result->events->count && t!= NULL; j++)
            {
                struct GmidEvent *tevent = (struct GmidEvent *)t->data;

                if (tevent->absolute_time < best_absolute_time)
                {
                    best_node = t;
                    best_absolute_time = tevent->absolute_time;
                }

                t = t->next;
            }

            if (node->id != best_node->id)
            {
                llist_node_swap(node, best_node);
            }

            // Fix the current node's delta time.
            // Assume the previous node is correct (don't change the first node in the list)
            if (best_node->prev != NULL)
            {
                struct GmidEvent *current_node_event = (struct GmidEvent *)best_node->data;
                struct GmidEvent *prev_node_event = (struct GmidEvent *)best_node->prev->data;
                int time_delta = (int)(current_node_event->absolute_time - prev_node_event->absolute_time);

                int32_to_varint(time_delta, &current_node_event->varint_delta_time);
            }

            node = next_node;
        }
    }

    return result;

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

int32_t GmidEvent_get_midi_command(struct GmidEvent *event)
{
    TRACE_ENTER("GmidEvent_get_midi_command")

    if (event->command == MIDI_COMMAND_BYTE_NOTE_OFF)
    {
        return MIDI_COMMAND_BYTE_NOTE_OFF | event->command_channel;
    }
    else if (event->command == MIDI_COMMAND_BYTE_NOTE_ON)
    {
        return MIDI_COMMAND_BYTE_NOTE_ON | event->command_channel;
    }
    else if (event->command == MIDI_COMMAND_BYTE_POLYPHONIC_PRESSURE)
    {
        return MIDI_COMMAND_BYTE_POLYPHONIC_PRESSURE | event->command_channel;
    }
    else if (event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE)
    {
        return MIDI_COMMAND_BYTE_CONTROL_CHANGE | event->command_channel;
    }
    else if (event->command == MIDI_COMMAND_BYTE_PROGRAM_CHANGE)
    {
        return MIDI_COMMAND_BYTE_PROGRAM_CHANGE | event->command_channel;
    }
    else if (event->command == MIDI_COMMAND_BYTE_CHANNEL_PRESSURE)
    {
        return MIDI_COMMAND_BYTE_CHANNEL_PRESSURE | event->command_channel;
    }
    else if (event->command == MIDI_COMMAND_BYTE_PITCH_BEND)
    {
        return MIDI_COMMAND_BYTE_PITCH_BEND | event->command_channel;
    }
    else if (event->command == CSEQ_COMMAND_BYTE_TEMPO)
    {
        return 0xff00 | CSEQ_COMMAND_BYTE_TEMPO;
    }
    else if (event->command == CSEQ_COMMAND_BYTE_LOOP_END)
    {
        return 0xff00 | CSEQ_COMMAND_BYTE_LOOP_END;
    }
    else if (event->command == CSEQ_COMMAND_BYTE_LOOP_START)
    {
        return 0xff00 | CSEQ_COMMAND_BYTE_LOOP_START;
    }
    else if (event->command == CSEQ_COMMAND_BYTE_END_OF_TRACK)
    {
        return 0xff00 | CSEQ_COMMAND_BYTE_END_OF_TRACK;
    }
    else
    {
        stderr_exit(EXIT_CODE_GENERAL, "GmidEvent_get_midi_command: command not supported: 0x%04x.\n", event->command);
    }

    TRACE_LEAVE("GmidEvent_get_midi_command")
}

int GmidEvent_get_midi_command_parameters(struct GmidEvent *event, uint8_t *buffer, size_t buffer_len)
{
    TRACE_ENTER("GmidEvent_get_midi_command")

    memset(buffer, 0, buffer_len);
    int write_len = 0;

    if (event->command == MIDI_COMMAND_BYTE_NOTE_ON)
    {
        if (buffer_len >= 2)
        {
            buffer[0] = (uint8_t)event->command_parameters[0];
            buffer[1] = (uint8_t)event->command_parameters[1];

            write_len = 2;
        }
        else
        {
            stderr_exit(EXIT_CODE_GENERAL, "GmidEvent_get_midi_command_parameters: buffer too small, command: 0x%04x.\n", event->command);
        }
    }
    else
    {
        if (buffer_len >= event->command_parameters_raw_len)
        {
            memcpy(buffer, event->command_parameters_raw, event->command_parameters_raw_len);
            write_len = event->command_parameters_raw_len;
        }
        else
        {
            stderr_exit(EXIT_CODE_GENERAL, "GmidEvent_get_midi_command_parameters: buffer too small, command: 0x%04x.\n", event->command);
        }
    }

    TRACE_LEAVE("GmidEvent_get_midi_command")

    return write_len;
}

void GmidTrack_write_to_midi_buffer(struct GmidTrack *gtrack, uint8_t *buffer, size_t max_len)
{
    TRACE_ENTER("GmidTrack_write_to_midi_buffer")

    size_t write_len = 0;
    struct llist_node *node = gtrack->events->root;
    while (node != NULL && write_len < max_len)
    {
        struct GmidEvent *event = (struct GmidEvent *)node->data;
        int32_t command = GmidEvent_get_midi_command(event);
        uint8_t command_param_buffer[GMID_EVENT_PARAMTER_BYTE_LEN];
        int command_len;

        memcpy(buffer, &event->varint_delta_time.value, event->varint_delta_time.num_bytes);
        write_len += event->varint_delta_time.num_bytes;

        memcpy(buffer, &command, event->command_len);
        write_len += event->command_len;

        command_len = GmidEvent_get_midi_command_parameters(event, command_param_buffer, GMID_EVENT_PARAMTER_BYTE_LEN);
        memcpy(buffer, command_param_buffer, command_len);
        write_len += command_len;

        node = node->next;
    }

    if (write_len > max_len)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%ld: GmidTrack_write_to_midi_buffer: exceeded max_len when writing to buffer.\n", __LINE__);
    }

    TRACE_LEAVE("GmidTrack_write_to_midi_buffer")
}

/**
 * Writes the MIDI track to disk.
 * @param midi_file: MIDI to write.
 * @param fi: File handle to write to, using current offset.
*/
void MidiTrack_frwrite(struct MidiTrack *track, struct file_info *fi)
{
    TRACE_ENTER("MidiTrack_frwrite")

    int i;

    file_info_fwrite_bswap(fi, &track->ck_id, 4, 1);
    file_info_fwrite_bswap(fi, &track->ck_data_size, 4, 1);

    file_info_fwrite(fi, &track->data, track->ck_data_size, 1);

    TRACE_LEAVE("MidiTrack_frwrite")
}

/**
 * Writes the full {@code struct MidiFile} to disk.
 * @param midi_file: MIDI to write.
 * @param fi: File handle to write to, using current offset.
*/
void MidiFile_frwrite(struct MidiFile *midi_file, struct file_info *fi)
{
    TRACE_ENTER("MidiFile_frwrite")

    int i;

    file_info_fwrite_bswap(fi, &midi_file->ck_id, 4, 1);
    file_info_fwrite_bswap(fi, &midi_file->ck_data_size, 4, 1);
    file_info_fwrite_bswap(fi, &midi_file->format, 2, 1);
    file_info_fwrite_bswap(fi, &midi_file->num_tracks, 2, 1);
    file_info_fwrite_bswap(fi, &midi_file->division, 2, 1);

    for (i=0; i<midi_file->num_tracks; i++)
    {
        MidiTrack_frwrite(midi_file->tracks[i], fi);
    }

    TRACE_LEAVE("MidiFile_frwrite")
}