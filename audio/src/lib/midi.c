#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "debug.h"
#include "common.h"
#include "machine_config.h"
#include "utility.h"
#include "midi.h"

/**
 * This file contains code for the N64 compressed MIDI format,
 * and regular MIDI format.
*/

int g_midi_parse_debug = 0;

// forward declarations

int llist_node_gmidevent_compare_larger(struct llist_node *first, struct llist_node *second);
int llist_node_gmidevent_compare_smaller(struct llist_node *first, struct llist_node *second);

// end forward declarations

/**
 * Merge sort comparison function.
 * Use this to sort largest to smallest.
 * @param first: first node
 * @param second: second node
 * @returns: comparison result
*/
int llist_node_gmidevent_compare_larger(struct llist_node *first, struct llist_node *second)
{
    TRACE_ENTER(__func__)

    int ret;

    if (first == NULL && second == NULL)
    {
        ret = 0;
    }
    else if (first == NULL && second != NULL)
    {
        ret = 1;
    }
    else if (first != NULL && second == NULL)
    {
        ret = -1;
    }
    else
    {
        struct GmidEvent *gmidevent_first = (struct GmidEvent *)first->data;
        struct GmidEvent *gmidevent_second = (struct GmidEvent *)second->data;
       
        if (gmidevent_first == NULL && gmidevent_second == NULL)
        {
            ret = 0;
        }
        else if (gmidevent_first == NULL && gmidevent_second != NULL)
        {
            ret = 1;
        }
        else if (gmidevent_first != NULL && gmidevent_second == NULL)
        {
            ret = -1;
        }
        else
        {
            if (gmidevent_first->absolute_time < gmidevent_second->absolute_time)
            {
                ret = 1;
            }
            else if (gmidevent_first->absolute_time > gmidevent_second->absolute_time)
            {
                ret = -1;
            }
            else
            {
                ret = 0;
            }
        }
    }

    TRACE_LEAVE(__func__)

    return ret;
}

/**
 * Merge sort comparison function.
 * Use this to sort smallest to largest.
 * @param first: first node
 * @param second: second node
 * @returns: comparison result
*/
int llist_node_gmidevent_compare_smaller(struct llist_node *first, struct llist_node *second)
{
    TRACE_ENTER(__func__)

    int ret;

    if (first == NULL && second == NULL)
    {
        ret = 0;
    }
    else if (first == NULL && second != NULL)
    {
        ret = 1;
    }
    else if (first != NULL && second == NULL)
    {
        ret = -1;
    }
    else
    {
        struct GmidEvent *gmidevent_first = (struct GmidEvent *)first->data;
        struct GmidEvent *gmidevent_second = (struct GmidEvent *)second->data;
       
        if (gmidevent_first == NULL && gmidevent_second == NULL)
        {
            ret = 0;
        }
        else if (gmidevent_first == NULL && gmidevent_second != NULL)
        {
            ret = 1;
        }
        else if (gmidevent_first != NULL && gmidevent_second == NULL)
        {
            ret = -1;
        }
        else
        {
            if (gmidevent_first->absolute_time < gmidevent_second->absolute_time)
            {
                ret = -1;
            }
            else if (gmidevent_first->absolute_time > gmidevent_second->absolute_time)
            {
                ret = 1;
            }
            else
            {
                ret = 0;
            }
        }
    }

    TRACE_LEAVE(__func__)

    return ret;
}

/**
 * Allocates memory for a {@code struct CseqFile}.
 * @returns: pointer to new object.
*/
struct CseqFile *CseqFile_new()
{
    TRACE_ENTER(__func__)

    struct CseqFile *p = (struct CseqFile *)malloc_zero(1, sizeof(struct CseqFile));

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Allocates memory for a {@code struct MidiTrack}.
 * @param track_index: Track index in parent {@code struct MidiFile}.
 * @returns: pointer to new object.
*/
struct MidiTrack *MidiTrack_new(int32_t track_index)
{
    TRACE_ENTER(__func__)

    struct MidiTrack *p = (struct MidiTrack *)malloc_zero(1, sizeof(struct MidiTrack));

    p->ck_id = MIDI_TRACK_CHUNK_ID;
    p->track_index = track_index;

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Allocates memory for a {@code struct MidiFile}.
 * @param format: MIDI file format.
 * @returns: pointer to new object.
*/
struct MidiFile *MidiFile_new(int format)
{
    TRACE_ENTER(__func__)

    struct MidiFile *p = (struct MidiFile *)malloc_zero(1, sizeof(struct MidiFile));

    p->ck_id = MIDI_ROOT_CHUNK_ID;
    p->ck_data_size = MIDI_ROOT_CHUNK_BODY_SIZE;
    p->format = format;

    TRACE_LEAVE(__func__)

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
    TRACE_ENTER(__func__)

    struct MidiFile *p = (struct MidiFile *)malloc_zero(1, sizeof(struct MidiFile));

    p->ck_id = MIDI_ROOT_CHUNK_ID;
    p->ck_data_size = MIDI_ROOT_CHUNK_BODY_SIZE;
    p->format = format;
    p->num_tracks = num_tracks;

    p->tracks = (struct MidiTrack **)malloc_zero(num_tracks, sizeof(struct MidiTrack *));

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Allocates memory for a {@code struct CseqFile}.
 * Reads a file and loads the contents into the new {@code struct CseqFile}.
 * @returns: pointer to new object.
*/
struct CseqFile *CseqFile_new_from_file(struct file_info *fi)
{
    TRACE_ENTER(__func__)

    int i, j;
    size_t data_len = 0;

    struct CseqFile *p = (struct CseqFile *)malloc_zero(1, sizeof(struct CseqFile));

    // make sure to begin reading at the beginning of the file.
    file_info_fseek(fi, 0, SEEK_SET);

    for (i=0; i<CSEQ_FILE_NUM_TRACKS; i++)
    {
        file_info_fread(fi, &p->track_offset[i], 4, 1);
        BSWAP32(p->track_offset[i]);
    }

    file_info_fread(fi, &p->division, 4, 1);
    BSWAP32(p->division);

    data_len = fi->len - CSEQ_FILE_HEADER_SIZE_BYTES;

    p->compressed_data = (uint8_t *)malloc_zero(1, data_len);
    file_info_fread(fi, p->compressed_data, data_len, 1);

    // Now load track contents.
    for (i=0; i<CSEQ_FILE_NUM_TRACKS; i++)
    {
        if (p->track_offset[i] == 0)
        {
            p->track_lengths[i] = 0;

            continue;
        }

        p->non_empty_num_tracks++;

        int32_t my_offset = p->track_offset[i];
        int32_t next_offset = INT32_MAX;
        data_len = 0;

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
            data_len = fi->len - my_offset;
        }
        else
        {
            // else the data length is the difference between the next offset
            data_len = next_offset - my_offset;
        }

        p->track_lengths[i] = data_len;
    }

    if (p->non_empty_num_tracks > CSEQ_FILE_NUM_TRACKS)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s: non_empty_num_tracks %d exceeds %d.\n", __func__, p->non_empty_num_tracks, CSEQ_FILE_NUM_TRACKS);
    }

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Creates new {@code struct MidiTrack} from {@code struct GmidTrack}.
 * Allocates memory for track data and extracts standard MIDI events from gtrak.
 * @param gtrack: input track to convert.
 * @returns: pointer to new MIDI track.
*/
struct MidiTrack *MidiTrack_new_from_GmidTrack(struct GmidTrack *gtrack)
{
    TRACE_ENTER(__func__)

    struct MidiTrack *p = (struct MidiTrack *)malloc_zero(1, sizeof(struct MidiTrack));

    p->ck_id = MIDI_TRACK_CHUNK_ID;
    p->track_index = gtrack->midi_track_index;

    p->data = (uint8_t *)malloc_zero(1, gtrack->midi_track_size_bytes);
    p->ck_data_size = GmidTrack_write_to_midi_buffer(gtrack, p->data, gtrack->midi_track_size_bytes);

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Creates new {@code struct MidiFile} from {@code struct CseqFile}.
 * Allocates memory.
 * @param cseq: compressed MIDI format track to convert.
 * @returns: pointer to new MidiFile.
*/
struct MidiFile *MidiFile_from_CseqFile(struct CseqFile *cseq)
{
    TRACE_ENTER(__func__)

    int i;
    int allocated_tracks = 0;

    struct MidiFile *midi = MidiFile_new_tracks(MIDI_FORMAT_SIMULTANEOUS, cseq->non_empty_num_tracks);

    midi->division = cseq->division;

    for (i=0; i<CSEQ_FILE_NUM_TRACKS; i++)
    {
        if (cseq->track_offset[i] == 0)
        {
            continue;
        }

        if (g_verbosity >= VERBOSE_DEBUG)
        {
            printf("MidiFile_from_CseqFile: parse track %d\n", i);
        }

        struct GmidTrack *gtrack = GmidTrack_new();

        gtrack->midi_track_index = allocated_tracks;
        gtrack->cseq_track_index = i;

        CseqFile_unroll(cseq, gtrack);
        GmidTrack_parse_CseqTrack(gtrack);

        midi->tracks[allocated_tracks] = MidiTrack_new_from_GmidTrack(gtrack);
        allocated_tracks++;

        GmidTrack_free(gtrack);
    }

    return midi;

    TRACE_LEAVE(__func__)
}

/**
 * Allocates memory for a {@code struct GmidEvent}.
 * @returns: pointer to new object.
*/
struct GmidEvent *GmidEvent_new()
{
    TRACE_ENTER(__func__)

    struct GmidEvent *p = (struct GmidEvent *)malloc_zero(1, sizeof(struct GmidEvent));

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Allocates memory for a {@code struct GmidTrack}.
 * @returns: pointer to new object.
*/
struct GmidTrack *GmidTrack_new()
{
    TRACE_ENTER(__func__)

    struct GmidTrack *p = (struct GmidTrack *)malloc_zero(1, sizeof(struct GmidTrack));
    p->events = llist_root_new();

    TRACE_LEAVE(__func__)

    return p;
}

/**
 * Frees memory allocated to event and all child objects.
 * @param event: object to free.
*/
void GmidEvent_free(struct GmidEvent *event)
{
    TRACE_ENTER(__func__)

    if (event == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    // If there's an associated dual event update the other
    // pointer and set it to NULL.
    if (event->dual != NULL)
    {
        event->dual->dual = NULL;
    }

    free(event);

    TRACE_LEAVE(__func__)
}

/**
 * Frees memory allocated to track and all child objects.
 * @param event: object to free.
*/
void GmidTrack_free(struct GmidTrack *track)
{
    TRACE_ENTER(__func__)

    if (track == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (track->events != NULL)
    {
        struct GmidEvent *data;
        struct llist_node *node;

        node = track->events->root;

        while (node != NULL)
        {
            data = (struct GmidEvent *)node->data;
            if (data != NULL)
            {
                GmidEvent_free(data);
                node->data = NULL;
            }

            node = node->next;
        }

        llist_node_root_free(track->events);
        track->events = NULL;
    }

    if (track->cseq_data != NULL)
    {
        free(track->cseq_data);
        track->cseq_data = NULL;
    }

    free(track);

    TRACE_LEAVE(__func__)
}

/**
 * Frees memory allocated to compressed MIDI file and all child elements.
 * @param CseqFile: object to free.
*/
void CseqFile_free(struct CseqFile *cseq)
{
    TRACE_ENTER(__func__)

    if (cseq == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (cseq->compressed_data != NULL)
    {
        free(cseq->compressed_data);
        cseq->compressed_data = NULL;
    }

    free(cseq);

    TRACE_LEAVE(__func__)
}

/**
 * Frees memory allocated to track.
 * @param track: object to free.
*/
void MidiTrack_free(struct MidiTrack *track)
{
    TRACE_ENTER(__func__)

    if (track == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (track->ck_data_size > 0 && track->data != NULL)
    {
        free(track->data);
        track->data = NULL;
    }

    free(track);

    TRACE_LEAVE(__func__)
}

/**
 * Frees memory allocated to compressed MIDI file and all child elements.
 * @param MidiFile: object to free.
*/
void MidiFile_free(struct MidiFile *midi)
{
    TRACE_ENTER(__func__)

    int i;

    if (midi == NULL)
    {
        TRACE_LEAVE(__func__)
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

    if (midi->tracks != NULL)
    {
        free(midi->tracks);
        midi->tracks = NULL;
    }

    free(midi);

    TRACE_LEAVE(__func__)
}

/**
 * N64 compressed MIDI format has simple compression. The data sequence can contain "pattern" markers
 * referring to a location earlier in the sequence. This method unrolls the raw data and replaces
 * all markers with actual byte contents. This is a precursor to parsing the actual MIDI file.
 * This method allocates memory for the {@code struct GmidTrack.cseq_data} and sets
 * {@code struct GmidTrack.cseq_data_len} accordingly.
 * @param track: compressed track to parse.
*/
void CseqFile_unroll(struct CseqFile *cseq, struct GmidTrack *track)
{
    TRACE_ENTER(__func__)

    size_t pos = 0;
    size_t unrolled_pos = 0;
    size_t cseq_len = 0;
    uint8_t *temp_ptr;
    size_t compressed_read_len = 0;

    if (track->cseq_data != NULL)
    {
        free(track->cseq_data);
    }

    // rough guess here, might resize during iteration, will adjust at the end too.
    size_t new_size = (size_t)((float)cseq->track_lengths[track->cseq_track_index] * 1.5f);
    track->cseq_data = (uint8_t *)malloc_zero(1, new_size);

    pos = cseq->track_offset[track->cseq_track_index] - CSEQ_FILE_HEADER_SIZE_BYTES;
    cseq_len = cseq->track_lengths[track->cseq_track_index];

    while (compressed_read_len < cseq_len)
    {
        int diff;
        int length;
        int i;

        // ensure allocated size can accept at least two bytes, otherwise resize.
        if (unrolled_pos + 2 >= new_size)
        {
            new_size = (size_t)((float)new_size * 1.5f);
            temp_ptr = (uint8_t *)malloc_zero(1, new_size);
            memcpy(temp_ptr, track->cseq_data, unrolled_pos);
            free(track->cseq_data);
            track->cseq_data = temp_ptr;
        }

        if (cseq->compressed_data[pos] != 0xfe)
        {
            track->cseq_data[unrolled_pos] = cseq->compressed_data[pos];
            pos++;
            compressed_read_len++;
            unrolled_pos++;
            continue;
        }

        // escape sequence, read two bytes write one.
        if (cseq->compressed_data[pos+1] == 0xfe)
        {
            track->cseq_data[unrolled_pos] = cseq->compressed_data[pos];
            
            pos += 2;
            compressed_read_len += 2;
            unrolled_pos++;
            continue;
        }

        // else, pattern marker.
        diff = 0;
        diff |= cseq->compressed_data[pos + 1];
        diff <<= 8;
        diff |= cseq->compressed_data[pos + 2];

        length = cseq->compressed_data[pos + 3];

        if ((int)pos - diff < 0)
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s: cseq_track %d references diff %d before start of file, position %ld.\n", __func__, track->cseq_track_index, diff, pos);
        }

        // ensure pattern bytes can fit in current allocation, otherwise resize.
        if (unrolled_pos + length >= new_size)
        {
            new_size = (size_t)((float)new_size * 1.5f);
            temp_ptr = (uint8_t *)malloc_zero(1, new_size);
            memcpy(temp_ptr, track->cseq_data, unrolled_pos);
            free(track->cseq_data);
            track->cseq_data = temp_ptr;
        }

        // now copy pattern bytes into buffer
        for (i=0; i<length; i++)
        {
            track->cseq_data[unrolled_pos] = cseq->compressed_data[pos - diff + i];
            unrolled_pos++;
        }

        pos += 4;
        compressed_read_len += 4;
    }

    // resize to actual data length.
    temp_ptr = (uint8_t *)malloc_zero(1, unrolled_pos);
    memcpy(temp_ptr, track->cseq_data, unrolled_pos);
    free(track->cseq_data);
    track->cseq_data = temp_ptr;

    // set data length
    track->cseq_data_len = unrolled_pos;

    TRACE_LEAVE(__func__)
}

/**
 * This parses a compressed MIDI track from the internal raw data buffer
 * and converts to {@code struct GmidEvent}, storing the events
 * internally. This allocates memory. This method can't parse pattern markers,
 * {@code CseqFile_unroll} must have been called previously.
 * @param gtrack: track to convert to events.
*/
void GmidTrack_parse_CseqTrack(struct GmidTrack *gtrack)
{
    TRACE_ENTER(__func__)

    /**
     * The general logic of this method is to read through the cseq bytes
     * and create events sequentially. After all bytes are read, the events
     * are iterated and "Note Off" events are generated (actually implicit Note off).
     * If any new events were added then the events list is then sorted.
     * The list is then iterated again, this time setting all delta event times.
     * Note that this needs to happen whether new events were added or not
     * because this sets cseq_ event times and midi_ event times, and some
     * cseq_ events are excluded from MIDI but still want the delta event times.
     * The last step is to iterate the list one more time and calculate
     * the number of bytes used in cseq_ and midi_. Two comments on this last step
     * 1) can probably be combined with the previous step ...
     * 2) there is some difference I can't quite pin down that causes the byte
     * count to be larger than the actually used byte count when writing to file
     * later.
    */

    // current position in the read buffer.
    size_t pos = 0;
    // current byte read
    uint8_t b;
    int done = 0;
    // most recent command channel
    int command_channel = 0;
    // most recent note
    int note;
    // most recent velocity read from buffer
    int velocity;
    // temp varint for parsing/copying values
    struct var_length_int varint;
    // debug print buffer
    char print_buffer[MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN];
    // debug print buffer
    char description_buffer[MIDI_DESCRIPTION_TEXT_BUFFER_LEN];
    // assumes `b` is a valid command, this holds the general command
    int message_type;
    // toggle/flag to indicate if the next byte read should start a event delta time or not
    int need_delta_time = 1;
    // most recent command, used for running status commands. Use this unless `message_type` is known to be accurate.
    int current_command = -1;
    // just gives each event created here a unique id (unique within this track/method)
    int event_id = 0;
    // number of bytes used by varint command parameter
    int param_len = 0;
    // flag to indicate thee current event is done being processed and needs to be appended to the track list
    int append_event = 0;
    /** running count of absolute time.
    * Every delta event read is added to this value.
    * The first delta event read sets the very first absolute time (not necessarily zero).
    */
    long absolute_time = 0;
    // flag to indicate the order of events was perturbed and needs to be sorted (i.e., inserted a new event)
    int need_sort = 0;

    struct llist_node *node;
    struct GmidEvent *event = NULL;

    if (gtrack->cseq_data == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: gtrack->cseq_data is NULL (call CseqFile_unroll first). cseq_track index=%d\n", __func__, gtrack->cseq_track_index);
    }

    memset(&varint, 0, sizeof(struct var_length_int));

    while (!done && pos < gtrack->cseq_data_len)
    {
        append_event = 0;

        // command/status message
        if (need_delta_time == 0 && (gtrack->cseq_data[pos] & 0x80))
        {
            need_delta_time = 0;

            b = gtrack->cseq_data[pos++];
            message_type = b & 0xf0;
            command_channel = b & 0x0f;

            // system command
            if (b == 0xff)
            {
                current_command = -1;
                need_delta_time = 1;

                b = gtrack->cseq_data[pos++];
                
                if (b == CSEQ_COMMAND_BYTE_TEMPO)
                {
                    int tempo = 0;

                    if (event == NULL)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "%s: parse error, event is NULL. track index=%d, pos=%d.\n", __func__, gtrack->cseq_track_index, pos);
                    }

                    // copy raw value in same endian, other values set at end.
                    memcpy(event->cseq_command_parameters_raw, &gtrack->cseq_data[pos], CSEQ_COMMAND_PARAM_BYTE_TEMPO);

                    // parse command values.
                    tempo |= gtrack->cseq_data[pos++];
                    tempo <<= 8;
                    tempo |= gtrack->cseq_data[pos++];
                    tempo <<= 8;
                    tempo |= gtrack->cseq_data[pos++];

                    // optional debug print
                    if (g_midi_parse_debug)
                    {
                        memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                        snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: %d", CSEQ_COMMAND_NAME_TEMPO, tempo);
                        fflush_printf(stdout, "%s\n", print_buffer);
                    }

                    // save parsed data
                    event->dual = NULL;
                    event->command = 0xff00 | CSEQ_COMMAND_BYTE_TEMPO;
                    event->cseq_valid = 1;
                    event->midi_valid = 1;

                    // cseq format
                    event->cseq_command_len = CSEQ_COMMAND_LEN_TEMPO;
                    event->cseq_command_parameters_raw_len = CSEQ_COMMAND_PARAM_BYTE_TEMPO;
                    event->cseq_command_parameters[0] = tempo;
                    event->cseq_command_parameters_len = CSEQ_COMMAND_NUM_PARAM_TEMPO;

                    // convert to MIDI format
                    event->midi_command_len = MIDI_COMMAND_LEN_TEMPO;
                    event->midi_command_parameters_raw[0] = 3; // len
                    memcpy(&event->midi_command_parameters_raw[1], event->cseq_command_parameters_raw, CSEQ_COMMAND_PARAM_BYTE_TEMPO);
                    event->midi_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_TEMPO;
                    event->cseq_command_parameters[0] = 3; // len
                    event->cseq_command_parameters[1] = tempo;
                    event->cseq_command_parameters_len = MIDI_COMMAND_NUM_PARAM_TEMPO;
                }
                else if (b == CSEQ_COMMAND_BYTE_LOOP_START)
                {
                    int loop_number = 0;

                    if (event == NULL)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "%s: parse error, event is NULL. track index=%d, pos=%d.\n", __func__, gtrack->cseq_track_index, pos);
                    }

                    // copy raw value in same endian, other values set at end.
                    memcpy(event->cseq_command_parameters_raw, &gtrack->cseq_data[pos], CSEQ_COMMAND_PARAM_BYTE_LOOP_START);

                    // parse command values.
                    loop_number = gtrack->cseq_data[pos++];
                    b = gtrack->cseq_data[pos++];
                    if (b != 0xff)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "%s: %s: expected end of command byte 0xff but read '0x%x', track index=%d, pos=%d.\n", __func__, CSEQ_COMMAND_NAME_LOOP_START, b, gtrack->cseq_track_index, pos);
                    }

                    // optional debug print
                    if (g_midi_parse_debug)
                    {
                        memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                        snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: loop number %d", CSEQ_COMMAND_NAME_LOOP_START, loop_number);
                        fflush_printf(stdout, "%s\n", print_buffer);
                    }

                    // save parsed data
                    event->command = 0xff00 | CSEQ_COMMAND_BYTE_LOOP_START;
                    event->dual = NULL;
                    event->cseq_valid = 1;
                    event->midi_valid = 0;

                    // cseq format
                    event->cseq_command_len = CSEQ_COMMAND_LEN_LOOP_START;
                    event->cseq_command_parameters_raw_len = CSEQ_COMMAND_PARAM_BYTE_LOOP_START;
                    event->cseq_command_parameters[0] = loop_number;
                    event->cseq_command_parameters[1] = b;
                    event->cseq_command_parameters_len = CSEQ_COMMAND_NUM_PARAM_LOOP_START;

                    // convert to MIDI format
                    // not supported
                }
                else if (b == CSEQ_COMMAND_BYTE_LOOP_END)
                {
                    int loop_count = 0;
                    int current_loop_count = 0;
                    int32_t loop_difference = 0;

                    if (event == NULL)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "%s: parse error, event is NULL. track index=%d, pos=%d.\n", __func__, gtrack->cseq_track_index, pos);
                    }

                    // copy raw value in same endian, other values set at end.
                    memcpy(event->cseq_command_parameters_raw, &gtrack->cseq_data[pos], CSEQ_COMMAND_PARAM_BYTE_LOOP_END);

                    // parse command values.
                    loop_count = gtrack->cseq_data[pos++];
                    current_loop_count = gtrack->cseq_data[pos++];

                    loop_difference = gtrack->cseq_data[pos++];
                    loop_difference <<= 8;
                    loop_difference = gtrack->cseq_data[pos++];
                    loop_difference <<= 8;
                    loop_difference = gtrack->cseq_data[pos++];
                    loop_difference <<= 8;
                    loop_difference = gtrack->cseq_data[pos++];

                    // optional debug print
                    if (g_midi_parse_debug)
                    {
                        memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                        snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: count %d, current count %d, diff 0x%08x", CSEQ_COMMAND_NAME_LOOP_END, loop_count, current_loop_count, loop_difference);
                        fflush_printf(stdout, "%s\n", print_buffer);
                    }

                    // save parsed data
                    event->command = 0xff00 | CSEQ_COMMAND_BYTE_LOOP_END;
                    event->dual = NULL;
                    event->cseq_valid = 1;
                    event->midi_valid = 0;

                    // cseq format
                    event->cseq_command_len = CSEQ_COMMAND_LEN_LOOP_END;
                    event->cseq_command_parameters_raw_len = CSEQ_COMMAND_PARAM_BYTE_LOOP_END;
                    event->cseq_command_parameters[0] = loop_count;
                    event->cseq_command_parameters[1] = current_loop_count;
                    event->cseq_command_parameters[2] = loop_difference;
                    event->cseq_command_parameters_len = CSEQ_COMMAND_NUM_PARAM_LOOP_END;

                    // convert to MIDI format
                    // not supported
                }
                else if (b == CSEQ_COMMAND_BYTE_END_OF_TRACK)
                {
                    if (event == NULL)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "%s: parse error, event is NULL. track index=%d, pos=%d.\n", __func__, gtrack->cseq_track_index, pos);
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

                    // save parsed data
                    event->command = 0xff00 | CSEQ_COMMAND_BYTE_END_OF_TRACK;
                    event->dual = NULL;
                    event->cseq_valid = 1;
                    event->midi_valid = 1;

                    // cseq format
                    event->cseq_command_len = CSEQ_COMMAND_LEN_END_OF_TRACK;
                    event->cseq_command_parameters_raw_len = CSEQ_COMMAND_PARAM_BYTE_END_OF_TRACK;
                    event->cseq_command_parameters_len = CSEQ_COMMAND_NUM_PARAM_END_OF_TRACK;

                    // convert to MIDI format
                    // NOTE: command is wrong for MIDI, correct command is retrieved via `GmidEvent_get_midi_command`
                    event->midi_command_len = MIDI_COMMAND_LEN_END_OF_TRACK;
                    event->midi_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_END_OF_TRACK;
                    event->midi_command_parameters_len = MIDI_COMMAND_NUM_PARAM_END_OF_TRACK;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s: parse error (system command), track index=%d, pos=%d.\n", __func__, gtrack->cseq_track_index, pos);
                }

                // append common format as new node into result list, reset for next event.
                node = llist_node_new();
                node->data = event;
                llist_root_append_node(gtrack->events, node);
                node = NULL;
                event = NULL;

                continue;
            }
            else if (b == CSEQ_COMMAND_BYTE_PATTERN)
            {
                int32_t pattern_difference = 0;
                int pattern_length = 0;

                pattern_difference = gtrack->cseq_data[pos++];
                pattern_difference <<= 8;
                pattern_difference = gtrack->cseq_data[pos++];
                
                pattern_length = gtrack->cseq_data[pos++];

                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: diff 0x%08x, length=%d", CSEQ_COMMAND_NAME_PATTERN, pattern_difference, pattern_length);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }

                stderr_exit(EXIT_CODE_GENERAL, "%s: parse error -- invalid compressed MIDI, \"pattern\" command not allowed. Track index=%d, pos=%d.\n", __func__, gtrack->cseq_track_index, pos);
            }
            else if (message_type == MIDI_COMMAND_BYTE_NOTE_OFF)
            {
                // note off
                stderr_exit(EXIT_CODE_GENERAL, "%s: parse error -- invalid compressed MIDI, \"note off\" command not allowed. Track index=%d, pos=%d.\n", __func__, gtrack->cseq_track_index, pos);
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

                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "command %s: channel %d", MIDI_COMMAND_NAME_POLYPHONIC_PRESSURE, command_channel);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }
            }
            else if (message_type == MIDI_COMMAND_BYTE_CONTROL_CHANGE)
            {
                current_command = MIDI_COMMAND_BYTE_CONTROL_CHANGE;

                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "command %s: channel %d", MIDI_COMMAND_NAME_CONTROL_CHANGE, command_channel);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }
            }
            else if (message_type == MIDI_COMMAND_BYTE_PROGRAM_CHANGE)
            {
                current_command = MIDI_COMMAND_BYTE_PROGRAM_CHANGE;

                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "command %s: channel %d", MIDI_COMMAND_NAME_PROGRAM_CHANGE, command_channel);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }
            }
            else if (message_type == MIDI_COMMAND_BYTE_CHANNEL_PRESSURE)
            {
                current_command = MIDI_COMMAND_BYTE_CHANNEL_PRESSURE;

                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "command %s: channel %d", MIDI_COMMAND_NAME_CHANNEL_PRESSURE, command_channel);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }
            }
            else if (message_type == MIDI_COMMAND_BYTE_PITCH_BEND)
            {
                // Pitch Bend
                stderr_exit(EXIT_CODE_GENERAL, "%s: Pitch Bend not implemented, track index=%d, pos=%d.\n", __func__, gtrack->cseq_track_index, pos);
            }
            else
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s: parse error (command), track index=%d, pos=%d.\n", __func__, gtrack->cseq_track_index, pos);
            }
        }
        
        // data bytes
        if (need_delta_time == 0)
        {
            need_delta_time = 1;

            /**
             * implementation note: make sure pos is incrememnted in each every block!
            */

           if (event == NULL)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s: parse error, event is NULL. track index=%d, pos=%d.\n", __func__, gtrack->cseq_track_index, pos);
            }

            if (current_command == MIDI_COMMAND_BYTE_NOTE_OFF)
            {
                // note off
                stderr_exit(EXIT_CODE_GENERAL, "%s: parse error -- invalid compressed MIDI, \"note off\" command not allowed. Track index=%d, pos=%d.\n", __func__, gtrack->cseq_track_index, pos);
            }
            else if (current_command == MIDI_COMMAND_BYTE_NOTE_ON)
            {
                // parse command values.
                note = gtrack->cseq_data[pos++];
                velocity = gtrack->cseq_data[pos++];

                memset(&varint, 0, sizeof(struct var_length_int));
                varint_value_to_int32(&gtrack->cseq_data[pos], 4, &varint);
                pos += varint.num_bytes;

                param_len = 2 + varint.num_bytes;

                // copy raw value in same endian, other values set at end.
                memcpy(event->cseq_command_parameters_raw, &gtrack->cseq_data[pos - param_len], param_len);

                // optional debug print
                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);
                    memset(description_buffer, 0, MIDI_DESCRIPTION_TEXT_BUFFER_LEN);

                    midi_note_to_name(note, description_buffer, MIDI_DESCRIPTION_TEXT_BUFFER_LEN);
                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: channel %d, %s velocity=%d, duration=%d", MIDI_COMMAND_NAME_NOTE_ON, command_channel, description_buffer, velocity, varint.standard_value);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }

                // save parsed data
                event->command = MIDI_COMMAND_BYTE_NOTE_ON;
                event->dual = NULL;
                event->cseq_valid = 1;
                event->midi_valid = 1;

                // cseq format
                event->command_channel = command_channel;
                event->cseq_command_len = CSEQ_COMMAND_LEN_NOTE_ON;
                event->cseq_command_parameters_raw_len = param_len;
                event->cseq_command_parameters[0] = note;
                event->cseq_command_parameters[1] = velocity;
                event->cseq_command_parameters[2] = varint.standard_value; // duration
                event->cseq_command_parameters_len = CSEQ_COMMAND_NUM_PARAM_NOTE_ON;

                // convert to MIDI format
                event->midi_command_parameters_raw[0] = note;
                event->midi_command_parameters_raw[1] = velocity;
                event->midi_command_len = MIDI_COMMAND_LEN_NOTE_ON;
                event->midi_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_NOTE_ON;
                event->midi_command_parameters[0] = note;
                event->midi_command_parameters[1] = velocity;
                event->midi_command_parameters_len = MIDI_COMMAND_NUM_PARAM_NOTE_ON;

                append_event = 1;
            }
            else if (current_command == MIDI_COMMAND_BYTE_POLYPHONIC_PRESSURE)
            {
                if (event == NULL)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s: parse error, event is NULL. track index=%d, pos=%d.\n", __func__, gtrack->cseq_track_index, pos);
                }

                // copy raw value in same endian, other values set at end.
                memcpy(event->cseq_command_parameters_raw, &gtrack->cseq_data[pos], MIDI_COMMAND_PARAM_BYTE_POLYPHONIC_PRESSURE);
                
                // parse command values.
                note = gtrack->cseq_data[pos++];
                int pressure = gtrack->cseq_data[pos++];

                // optional debug print
                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);
                    memset(description_buffer, 0, MIDI_DESCRIPTION_TEXT_BUFFER_LEN);

                    midi_note_to_name(note, description_buffer, MIDI_DESCRIPTION_TEXT_BUFFER_LEN);
                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: channel %d, %s pressure=%d", MIDI_COMMAND_NAME_POLYPHONIC_PRESSURE, command_channel, description_buffer, pressure);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }

                // save parsed data
                event->command = MIDI_COMMAND_BYTE_POLYPHONIC_PRESSURE;
                event->dual = NULL;
                event->cseq_valid = 1;
                event->midi_valid = 1;

                // cseq format
                event->command_channel = command_channel;
                event->cseq_command_len = MIDI_COMMAND_LEN_POLYPHONIC_PRESSURE;
                event->cseq_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_POLYPHONIC_PRESSURE;
                event->cseq_command_parameters[0] = note;
                event->cseq_command_parameters[1] = pressure;
                event->cseq_command_parameters_len = MIDI_COMMAND_NUM_PARAM_POLYPHONIC_PRESSURE;

                // convert to MIDI format (same as above)
                memcpy(event->midi_command_parameters_raw, event->cseq_command_parameters_raw, MIDI_COMMAND_PARAM_BYTE_POLYPHONIC_PRESSURE);
                event->midi_command_len = MIDI_COMMAND_LEN_NOTE_ON;
                event->midi_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_NOTE_ON;
                event->midi_command_parameters[0] = note;
                event->midi_command_parameters[1] = pressure;
                event->midi_command_parameters_len = MIDI_COMMAND_NUM_PARAM_NOTE_ON;

                append_event = 1;
            }
            else if (current_command == MIDI_COMMAND_BYTE_CONTROL_CHANGE)
            {
                if (event == NULL)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s: parse error, event is NULL. track index=%d, pos=%d.\n", __func__, gtrack->cseq_track_index, pos);
                }

                // copy raw value in same endian, other values set at end.
                memcpy(event->cseq_command_parameters_raw, &gtrack->cseq_data[pos], MIDI_COMMAND_PARAM_BYTE_CONTROL_CHANGE);
                
                // parse command values.
                int controller = gtrack->cseq_data[pos++];
                int new_value = gtrack->cseq_data[pos++];

                // optional debug print
                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);
                    memset(description_buffer, 0, MIDI_DESCRIPTION_TEXT_BUFFER_LEN);

                    midi_controller_to_name(controller, description_buffer, MIDI_DESCRIPTION_TEXT_BUFFER_LEN);
                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: channel %d, controller %d = %s, new_value=%d", MIDI_COMMAND_NAME_CONTROL_CHANGE, command_channel, controller, description_buffer, new_value);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }

                // save parsed data
                event->command = MIDI_COMMAND_BYTE_CONTROL_CHANGE;
                event->dual = NULL;
                event->cseq_valid = 1;
                event->midi_valid = 1;

                // cseq format
                event->command_channel = command_channel;
                event->cseq_command_len = MIDI_COMMAND_LEN_CONTROL_CHANGE;
                event->cseq_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_CONTROL_CHANGE;
                event->cseq_command_parameters[0] = controller;
                event->cseq_command_parameters[1] = new_value;
                event->cseq_command_parameters_len = MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;

                // convert to MIDI format (same as above)
                memcpy(event->midi_command_parameters_raw, event->cseq_command_parameters_raw, MIDI_COMMAND_PARAM_BYTE_CONTROL_CHANGE);
                event->midi_command_len = MIDI_COMMAND_LEN_CONTROL_CHANGE;
                event->midi_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_CONTROL_CHANGE;
                event->midi_command_parameters[0] = controller;
                event->midi_command_parameters[1] = new_value;
                event->midi_command_parameters_len = MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;

                append_event = 1;
            }
            else if (current_command == MIDI_COMMAND_BYTE_PROGRAM_CHANGE)
            {
                if (event == NULL)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s: parse error, event is NULL. track index=%d, pos=%d.\n", __func__, gtrack->cseq_track_index, pos);
                }

                // copy raw value in same endian, other values set at end.
                memcpy(event->cseq_command_parameters_raw, &gtrack->cseq_data[pos], MIDI_COMMAND_PARAM_BYTE_PROGRAM_CHANGE);
                
                // parse command values.
                int program = gtrack->cseq_data[pos++];

                // optional debug print
                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: channel %d, program=%d", MIDI_COMMAND_NAME_PROGRAM_CHANGE, command_channel, program);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }

                // save parsed data
                event->command = MIDI_COMMAND_BYTE_PROGRAM_CHANGE;
                event->dual = NULL;
                event->cseq_valid = 1;
                event->midi_valid = 1;

                // cseq format
                event->command_channel = command_channel;
                event->cseq_command_len = MIDI_COMMAND_LEN_PROGRAM_CHANGE;
                event->cseq_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_PROGRAM_CHANGE;
                event->cseq_command_parameters[0] = program;
                event->cseq_command_parameters_len = MIDI_COMMAND_NUM_PARAM_PROGRAM_CHANGE;

                // convert to MIDI format (same as above)
                memcpy(event->midi_command_parameters_raw, event->cseq_command_parameters_raw, MIDI_COMMAND_PARAM_BYTE_PROGRAM_CHANGE);
                event->midi_command_len = MIDI_COMMAND_LEN_PROGRAM_CHANGE;
                event->midi_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_PROGRAM_CHANGE;
                event->midi_command_parameters[0] = program;
                event->midi_command_parameters_len = MIDI_COMMAND_NUM_PARAM_PROGRAM_CHANGE;

                append_event = 1;
            }
            else if (current_command == MIDI_COMMAND_BYTE_CHANNEL_PRESSURE)
            {
                if (event == NULL)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s: parse error, event is NULL. track index=%d, pos=%d.\n", __func__, gtrack->cseq_track_index, pos);
                }

                // copy raw value in same endian, other values set at end.
                memcpy(event->cseq_command_parameters_raw, &gtrack->cseq_data[pos], MIDI_COMMAND_PARAM_BYTE_CHANNEL_PRESSURE);
                
                // parse command values.
                int pressure = gtrack->cseq_data[pos++];

                // optional debug print
                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: channel %d, pressure=%d", MIDI_COMMAND_NAME_CHANNEL_PRESSURE, command_channel, pressure);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }

                // save parsed data
                event->command = MIDI_COMMAND_BYTE_CHANNEL_PRESSURE;
                event->dual = NULL;
                event->cseq_valid = 1;
                event->midi_valid = 1;

                // cseq format
                event->command_channel = command_channel;
                event->cseq_command_len = MIDI_COMMAND_LEN_CHANNEL_PRESSURE;
                event->cseq_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_CHANNEL_PRESSURE;
                event->cseq_command_parameters[0] = pressure;
                event->cseq_command_parameters_len = MIDI_COMMAND_NUM_PARAM_CHANNEL_PRESSURE;

                // convert to MIDI format (same as above)
                memcpy(event->midi_command_parameters_raw, event->cseq_command_parameters_raw, MIDI_COMMAND_PARAM_BYTE_CHANNEL_PRESSURE);
                event->midi_command_len = MIDI_COMMAND_LEN_CHANNEL_PRESSURE;
                event->midi_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_CHANNEL_PRESSURE;
                event->midi_command_parameters[0] = pressure;
                event->midi_command_parameters_len = MIDI_COMMAND_NUM_PARAM_CHANNEL_PRESSURE;

                append_event = 1;
            }
            else if (current_command == MIDI_COMMAND_BYTE_PITCH_BEND)
            {
                // Pitch Bend
                stderr_exit(EXIT_CODE_GENERAL, "%s: Pitch Bend not implemented, track index=%d, pos=%d.\n", __func__, gtrack->cseq_track_index, pos);
            }
            else
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s: parse error (command=%d), track index=%d, pos=%d.\n", __func__, current_command, gtrack->cseq_track_index, pos);
            }

            if (append_event)
            {
                append_event = 0;

                // append common format as new node into result list, reset for next event.
                node = llist_node_new();
                node->data = event;
                llist_root_append_node(gtrack->events, node);
                node = NULL;
                event = NULL;
            }
        }
        else
        {
            need_delta_time = 0;

            if (event != NULL)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s: parse error, reading delta time before previous delta time was processed. Track index=%d, pos=%ld\n", __func__, gtrack->cseq_track_index, pos);
            }

            memset(&varint, 0, sizeof(struct var_length_int));
            varint_value_to_int32(&gtrack->cseq_data[pos], 4, &varint);

            if (varint.num_bytes < 1)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s: parse error, could not read variable length integer. Track index=%d, pos=%ld\n", __func__, gtrack->cseq_track_index, pos);
            }

            pos += varint.num_bytes;

            if (g_midi_parse_debug)
            {
                memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                snprintf(print_buffer, MIDI_DESCRIPTION_TEXT_BUFFER_LEN, "delta time: %d (varint=0x%06x)", varint.standard_value, varint.value);
                fflush_printf(stdout, "%s\n", print_buffer);
            }

            absolute_time += varint.standard_value;

            event = GmidEvent_new();
            event->id = event_id++;
            event->absolute_time = absolute_time;
            varint_copy(&event->cseq_delta_time, &varint);
        }
    }   

    if (pos > gtrack->cseq_data_len)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s: exceeded track length during parse. Track index=%d, pos=%ld, length=%ld\n", __func__, gtrack->cseq_track_index, pos, gtrack->cseq_data_len);
    }

    if (event != NULL)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s: parse error, unresolved event. Track index=%d\n", __func__, gtrack->cseq_track_index);
    }

    // create Note Off events from Note On events.
    node = gtrack->events->root;
    while (node != NULL)
    {
        event = (struct GmidEvent *)node->data;

        if (event->command == MIDI_COMMAND_BYTE_NOTE_ON)
        {
            struct GmidEvent *noteoff = GmidEvent_new();
            struct llist_node *temp_node;

            noteoff->id = event_id++;

            // duration was decoded from varint when it was saved into command_parameters[2]
            noteoff->absolute_time = event->absolute_time + (long)event->cseq_command_parameters[2];

            // delta event time is now broken, will fix after all new nodes have been created.

            // this is adding a Note Off command, using implicit format (Note On with zero velocity).
            noteoff->command = MIDI_COMMAND_BYTE_NOTE_ON;
            noteoff->command_channel = event->command_channel;

            noteoff->cseq_command_len = MIDI_COMMAND_LEN_NOTE_ON;
            noteoff->cseq_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_NOTE_ON;
            noteoff->cseq_command_parameters[0] = event->cseq_command_parameters[0]; // note
            noteoff->cseq_command_parameters[1] = 0; // velocity
            noteoff->cseq_command_parameters_len = MIDI_COMMAND_NUM_PARAM_NOTE_ON;
            noteoff->cseq_command_parameters_raw[0] = event->cseq_command_parameters_raw[0]; // note
            noteoff->cseq_command_parameters_raw[1] = 0; // velocity

            noteoff->midi_command_len = MIDI_COMMAND_LEN_NOTE_ON;
            noteoff->midi_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_NOTE_ON;
            noteoff->midi_command_parameters[0] = event->midi_command_parameters[0]; // note
            noteoff->midi_command_parameters[1] = 0; // velocity
            noteoff->midi_command_parameters_len = MIDI_COMMAND_NUM_PARAM_NOTE_ON;
            noteoff->midi_command_parameters_raw[0] = event->cseq_command_parameters_raw[0]; // note
            noteoff->midi_command_parameters_raw[1] = 0; // velocity

            noteoff->cseq_valid = 1;
            noteoff->midi_valid = 1;

            noteoff->dual = event;
            event->dual = noteoff;

            temp_node = llist_node_new();
            temp_node->data = noteoff;
            
            // Don't want to append the temp_node anywhere forward in the list since
            // that's still being iterated. Therefore just insert before current.
            llist_node_insert_before(gtrack->events, node, temp_node);

            need_sort = 1;
        }

        node = node->next;
    }

    // Now would be a good time to resolve the loop start <-> loop end dual pointers.
    // Maybe one day.

    // Now sort the events by absolute times.
    if (need_sort)
    {
        llist_root_merge_sort(gtrack->events, llist_node_gmidevent_compare_smaller);
    }

    // If nodes were inserted above then the delta event times are wrong.
    // MIDI event times need to be calculated anyway, since the cseq loop events
    // won't be included in MIDI output but they have delta times.
    // Note: list must be sorted first!
    {
        long cseq_prev_absolute_time = 0;
        long midi_prev_absolute_time = 0;
        int time_delta;

        node = gtrack->events->root;
        while (node != NULL)
        {
            struct GmidEvent *current_node_event = (struct GmidEvent *)node->data;

            time_delta = 0;

            // set the event cseq event time
            if (current_node_event->cseq_valid)
            {
                time_delta = (int)(current_node_event->absolute_time - cseq_prev_absolute_time);
                int32_to_varint(time_delta, &current_node_event->cseq_delta_time);
                cseq_prev_absolute_time = current_node_event->absolute_time;
            }

            // set the event MIDI event time
            if (current_node_event->midi_valid)
            {
                time_delta = (int)(current_node_event->absolute_time - midi_prev_absolute_time);
                int32_to_varint(time_delta, &current_node_event->midi_delta_time);
                midi_prev_absolute_time = current_node_event->absolute_time;
            }

            node = node->next;
        }
    }

    // now that all events have been created/written/updated,
    // calculate the total size of the regular MIDI track in bytes.
    gtrack->cseq_track_size_bytes = 0;
    gtrack->midi_track_size_bytes = 0;

    node = gtrack->events->root;
    for (; node != NULL; node = node->next)
    {
        event = (struct GmidEvent *)node->data;

        if (event->cseq_valid)
        {
            gtrack->cseq_track_size_bytes += event->cseq_delta_time.num_bytes + event->cseq_command_len + event->cseq_command_parameters_raw_len;
        }

        if (event->midi_valid)
        {
            gtrack->midi_track_size_bytes += event->midi_delta_time.num_bytes + event->midi_command_len + event->midi_command_parameters_raw_len;
        }
    }

    TRACE_LEAVE(__func__)
}

/**
 * Converts controller parameter id to text name.
 * @param controller: id of controller.
 * @param result: out parameter, will contain text result. Must be previously allocated.
 * @param max_length: max string length of {@code result}.
*/
void midi_controller_to_name(int controller, char *result, size_t max_length)
{
    TRACE_ENTER(__func__)

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

    TRACE_LEAVE(__func__)
}

/**
 * Converts note id to text name.
 * @param note: id of note.
 * @param result: out parameter, will contain text result. Must be previously allocated.
 * @param max_length: max string length of {@code result}.
*/
void midi_note_to_name(int note, char* result, size_t max_length)
{
    TRACE_ENTER(__func__)

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
        stderr_exit(EXIT_CODE_GENERAL, "%s: note=%d note supported.\n", __func__, note);
        break;
    }

    TRACE_LEAVE(__func__)
}

/**
 * Converts command from event into MIDI command, adding the channel number.
 * (The stored command doesn't include channel in the command.)
 * Maybe refactor this to split yet another parameter into two properties ...
 * But for now, the command paramter evaluated is the "full" compressed MIDI command (without channel),
 * this contains logic to adjust differences from cseq to MIDI.
 * @param event: event to build MIDI command from.
 * @returns: standard MIDI command with channel.
*/
int32_t GmidEvent_get_midi_command(struct GmidEvent *event)
{
    TRACE_ENTER(__func__)

    int upper = (0xff00 & event->command) >> 8;

    if (event->command == MIDI_COMMAND_BYTE_NOTE_OFF)
    {
        TRACE_LEAVE(__func__)
        return MIDI_COMMAND_BYTE_NOTE_OFF | event->command_channel;
    }
    else if (event->command == MIDI_COMMAND_BYTE_NOTE_ON)
    {
        TRACE_LEAVE(__func__)
        return MIDI_COMMAND_BYTE_NOTE_ON | event->command_channel;
    }
    else if (event->command == MIDI_COMMAND_BYTE_POLYPHONIC_PRESSURE)
    {
        TRACE_LEAVE(__func__)
        return MIDI_COMMAND_BYTE_POLYPHONIC_PRESSURE | event->command_channel;
    }
    else if (event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE)
    {
        TRACE_LEAVE(__func__)
        return MIDI_COMMAND_BYTE_CONTROL_CHANGE | event->command_channel;
    }
    else if (event->command == MIDI_COMMAND_BYTE_PROGRAM_CHANGE)
    {
        TRACE_LEAVE(__func__)
        return MIDI_COMMAND_BYTE_PROGRAM_CHANGE | event->command_channel;
    }
    else if (event->command == MIDI_COMMAND_BYTE_CHANNEL_PRESSURE)
    {
        TRACE_LEAVE(__func__)
        return MIDI_COMMAND_BYTE_CHANNEL_PRESSURE | event->command_channel;
    }
    else if (event->command == MIDI_COMMAND_BYTE_PITCH_BEND)
    {
        TRACE_LEAVE(__func__)
        return MIDI_COMMAND_BYTE_PITCH_BEND | event->command_channel;
    }
    else if (upper == 0xff)
    {
        int lower = 0xff & event->command;

        switch (lower)
        {
            /**
             * System command 0xff followed by single byte:
            */
            case CSEQ_COMMAND_BYTE_TEMPO:
            case CSEQ_COMMAND_BYTE_LOOP_END:
            case CSEQ_COMMAND_BYTE_LOOP_START:
            TRACE_LEAVE(__func__)
            return event->command;

            // translate from cseq to MIDI
            case CSEQ_COMMAND_BYTE_END_OF_TRACK:
            TRACE_LEAVE(__func__)
            return MIDI_COMMAND_FULL_END_OF_TRACK;
        }
    }

    stderr_exit(EXIT_CODE_GENERAL, "%s: command not supported: 0x%04x.\n", __func__, event->command);

    TRACE_LEAVE(__func__)

    // be quiet gcc
    return -1;
}

/**
 * Iterates {@code struct GmidTrack} events list and writes to out buffer
 * in standard MIDI format. This is writing the track data without the track header.
 * Events list must have been previously populated.
 * @param gtrack: track to convert.
 * @param buffer: buffer to write to.
 * @param max_len: size in bytes of the buffer.
 * @returns: number of bytes written to buffer.
*/
size_t GmidTrack_write_to_midi_buffer(struct GmidTrack *gtrack, uint8_t *buffer, size_t max_len)
{
    TRACE_ENTER(__func__)

    size_t write_len = 0;
    int32_t previous_command = 0;
    struct llist_node *node = gtrack->events->root;
    uint8_t rev[4];
    int32_t command;

    for (; node != NULL && write_len < max_len; node = node->next)
    {
        struct GmidEvent *event = (struct GmidEvent *)node->data;

        if (!event->midi_valid)
        {
            continue;
        }

        command = GmidEvent_get_midi_command(event);

        // write varint delta time value
        memcpy(&buffer[write_len], &event->midi_delta_time.value, event->midi_delta_time.num_bytes);
        write_len += event->midi_delta_time.num_bytes;

        // if this is a "running status" then no need to write command, otherwise write command bytes
        if (previous_command != command)
        {
            memset(rev, 0, 4);
            memcpy(rev, &command, event->midi_command_len);
            // byte swap
            reverse_inplace(rev, event->midi_command_len);
            memcpy(&buffer[write_len], rev, event->midi_command_len);
            write_len += event->midi_command_len;
            previous_command = command;
        }

        // only allow running status for the "regular" MIDI commands
        if ((command & 0xffffff00) != 0 || (command & 0xffffffff) == 0xff)
        {
            previous_command = 0;
        }

        // write command parameters
        if (event->midi_command_parameters_raw_len > 0)
        {
            memcpy(&buffer[write_len], event->midi_command_parameters_raw, event->midi_command_parameters_raw_len);
            write_len += event->midi_command_parameters_raw_len;
        }
    }

    if (write_len > max_len)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s: write_len %ld exceeded max_len %ld when writing to buffer.\n", __func__, write_len, max_len);
    }

    TRACE_LEAVE(__func__)

    return write_len;
}

/**
 * Writes the MIDI track to disk.
 * @param midi_file: MIDI to write.
 * @param fi: File handle to write to, using current offset.
*/
void MidiTrack_fwrite(struct MidiTrack *track, struct file_info *fi)
{
    TRACE_ENTER(__func__)

    file_info_fwrite_bswap(fi, &track->ck_id, 4, 1);
    file_info_fwrite_bswap(fi, &track->ck_data_size, 4, 1);

    file_info_fwrite(fi, track->data, track->ck_data_size, 1);

    TRACE_LEAVE(__func__)
}

/**
 * Writes the full {@code struct MidiFile} to disk.
 * @param midi_file: MIDI to write.
 * @param fi: File handle to write to, using current offset.
*/
void MidiFile_fwrite(struct MidiFile *midi_file, struct file_info *fi)
{
    TRACE_ENTER(__func__)

    int i;

    file_info_fwrite_bswap(fi, &midi_file->ck_id, 4, 1);
    file_info_fwrite_bswap(fi, &midi_file->ck_data_size, 4, 1);
    file_info_fwrite_bswap(fi, &midi_file->format, 2, 1);
    file_info_fwrite_bswap(fi, &midi_file->num_tracks, 2, 1);
    file_info_fwrite_bswap(fi, &midi_file->division, 2, 1);

    for (i=0; i<midi_file->num_tracks; i++)
    {
        MidiTrack_fwrite(midi_file->tracks[i], fi);
    }

    TRACE_LEAVE(__func__)
}