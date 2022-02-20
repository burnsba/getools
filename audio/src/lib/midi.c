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

// give every event a unique id
static int g_event_id = 0;

struct seq_unroll_grow {
    int loop_id;
    int start_grow;
    int file_offset;
};

// forward declarations

int llist_node_gmidevent_compare_larger(struct llist_node *first, struct llist_node *second);
int llist_node_gmidevent_compare_smaller(struct llist_node *first, struct llist_node *second);
static struct seq_unroll_grow *seq_unroll_grow_new(void);
static void seq_unroll_grow_free(struct seq_unroll_grow *obj);

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
 * Data pointer remains unitialized and unallocated.
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
 * Allocates memory for a {@code struct MidiFile}.
 * Reads entire file and loads the contents into the new {@code struct MidiFile}.
 * Tracks are allocated and added to file container.
 * Tracks remain unprocesed.
 * @returns: pointer to new object.
*/
struct MidiFile *MidiFile_new_from_file(struct file_info *fi)
{
    TRACE_ENTER(__func__)

    if (fi == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> fi is NULL\n", __func__, __LINE__);
    }

    if (fi->len < 20)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> MIDI file \"%s\" too short\n", __func__, __LINE__, fi->filename);
    }

    uint32_t t32;
    size_t file_pos;
    int tracks_read;
    struct MidiFile *p;
    
    p = MidiFile_new(0); // format zero, will adjust below
    
    // it's ok to over-allocate here.
    p->tracks = (struct MidiTrack **)malloc_zero(CSEQ_FILE_NUM_TRACKS, sizeof(struct MidiTrack *));

    // init
    tracks_read = 0;
    file_pos = 0;

    // make sure to begin reading at the beginning of the file.
    file_info_fseek(fi, 0, SEEK_SET);

    // root chunk id
    file_info_fread(fi, &t32, 4, 1);
    file_pos += 4;
    BSWAP32(t32);
    if (t32 != MIDI_ROOT_CHUNK_ID)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> invalid MIDI root chunk id. Expected 0x%08x, actual 0x%08x.\n", __func__, __LINE__, MIDI_ROOT_CHUNK_ID, t32);
    }

    // root chunk size
    file_info_fread(fi, &t32, 4, 1);
    file_pos += 4;
    BSWAP32(t32);
    if (t32 != MIDI_ROOT_CHUNK_BODY_SIZE)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> invalid MIDI root chunk size. Expected 0x%08x, actual 0x%08x.\n", __func__, __LINE__, MIDI_ROOT_CHUNK_BODY_SIZE, t32);
    }

    // format
    file_info_fread(fi, &p->format, 2, 1);
    file_pos += 2;
    BSWAP16(p->format);
    if (p->format != MIDI_FORMAT_SIMULTANEOUS)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> Unsupported MIDI format %d. Only %d is supported.\n", __func__, __LINE__, p->format, MIDI_FORMAT_SIMULTANEOUS);
    }

    // number of tracks
    file_info_fread(fi, &p->num_tracks, 2, 1);
    file_pos += 2;
    BSWAP16(p->num_tracks);
    if (p->num_tracks > CSEQ_FILE_NUM_TRACKS)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> Only %d tracks supported, but MIDI file has %d\n", __func__, __LINE__, p->format, CSEQ_FILE_NUM_TRACKS, p->num_tracks);
    }

    // ticks per quarter note (when positive)
    file_info_fread(fi, &p->division, 2, 1);
    file_pos += 2;
    BSWAP16(p->division);
    if (p->division < 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> MIDI SMPTE division not supported, should be ticks per quarter. Division: 0x%04x\n", __func__, __LINE__, p->division);
    }

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("read MIDI header: format=%d, num tracks=%d, division=%d\n", p->format, p->num_tracks, p->division);
    }

    while (1)
    {
        int skip = 0;
        int track_size = 0;
        struct MidiTrack *track;

        if (file_pos + 8 >= fi->len)
        {
            break;
        }

        file_info_fread(fi, &t32, 4, 1);
        file_pos += 4;
        BSWAP32(t32);
        if (t32 != MIDI_TRACK_CHUNK_ID)
        {
            skip = 1;
        }

        file_info_fread(fi, &track_size, 4, 1);
        file_pos += 4;
        BSWAP32(track_size);

        if (skip)
        {
            file_pos += track_size;
            if (file_pos < fi->len)
            {
                file_info_fseek(fi, (long)track_size, SEEK_CUR);
            }
            else
            {
                break;
            }
        }

        if (file_pos + track_size > fi->len)
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> Invalid track size %d at offset %ld, exceeds file length %ld.\n", __func__, __LINE__, track_size, file_pos - 4, fi->len);
        }

        track = MidiTrack_new(tracks_read);
        track->ck_data_size = track_size;
        track->data = (uint8_t*)malloc_zero(1, (size_t)track_size);

        file_info_fread(fi, track->data, track_size, 1);
        file_pos += track_size;

        p->tracks[tracks_read] = track;

        tracks_read++;
    }

    if (tracks_read != p->num_tracks)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> MIDI header specified %d tracks, but only %d tracks were read\n", __func__, __LINE__, p->num_tracks, tracks_read);
    }

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

    if (fi == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> fi is NULL\n", __func__, __LINE__);
    }

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
 * Processes a N64 seq file loaded into memory and converts to regular MIDI format.
 * This allocates memory for the new MIDI file.
 * @param cseq: compressed MIDI format track to convert.
 * @returns: pointer to new MidiFile.
*/
struct MidiFile *MidiFile_from_CseqFile(struct CseqFile *cseq, f_GmidTrack_callback post_unroll_action)
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

        if (post_unroll_action != NULL)
        {
            post_unroll_action(gtrack);
        }

        // parse seq format into common events
        GmidTrack_parse_CseqTrack(gtrack);

        // The seq loop end event contains the count, but that's needed at the start,
        // so resolve all dual pointers to make that available.
        // This needs to happen before any nodes are added, and before
        // delta events are changed in order to calculate offsets correctly.
        GmidTrack_ensure_cseq_loop_dual(gtrack);

        // Create midi note off events from the seq note-on events.
        // This will break delta events for the track.
        GmidTrack_midi_note_off_from_cseq(gtrack);

        // Sort events by absolute time. This is needed because
        // the applied duration from the note-on can move
        // the new note-off event substantially.
        llist_root_merge_sort(gtrack->events, llist_node_gmidevent_compare_smaller);

        // Fix delta times, due to adding note-off events.
        GmidTrack_delta_from_absolute(gtrack);

        // Convert cseq loop markers into non-standard controller events
        GmidTrack_cseq_to_midi_loop(gtrack);

        // Fix delta times, due to adding MIDI controller events for looping.
        GmidTrack_delta_from_absolute(gtrack);

        // estimate total track size in bytes.
        GmidTrack_set_track_size_bytes(gtrack);

        midi->tracks[allocated_tracks] = MidiTrack_new_from_GmidTrack(gtrack);
        allocated_tracks++;

        GmidTrack_free(gtrack);
    }

    return midi;

    TRACE_LEAVE(__func__)
}

/**
 * Processes regular MIDI loaded into memory and converts to N64 compressed MIDI format.
 * This allocates memory for the new cseq file.
 * @param midi: Standard MIDI file to convert.
 * @returns: pointer to new cseq file.
*/
struct CseqFile *CseqFile_from_MidiFile(struct MidiFile *midi)
{
    /**
     * So far only Type 1 MIDI is supported (separate tracks), but
     * the parsing here is sort of implemented to support Type 0
     * as well (single track).
     * 
     * Outline of this method:
     * 
     * Create 16 GmidEvent containers.
     * Iterate the tracks in the source MIDI file.
     * For each track,
     *     - Parse data, convert to GmidEvent
     *     - Add GmidEvent to appropriate GmidTrack container.
     * 
     * For each GmidTrack,
     *     - Sort based on event time
     * 
     * Build CseqFile from all GmiTrack
    */
    TRACE_ENTER(__func__)

    if (midi == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: midi is NULL\n", __func__, __LINE__);
    }

    struct CseqFile *result;
    struct GmidTrack **gmidi_tracks;
    struct llist_root *track_event_holder;
    struct llist_node *node;
    int i;
    int source_track_index;

    gmidi_tracks = (struct GmidTrack **)malloc_zero(CSEQ_FILE_NUM_TRACKS, sizeof(struct GmidTrack *));

    for (i=0; i<CSEQ_FILE_NUM_TRACKS; i++)
    {
        gmidi_tracks[i] = GmidTrack_new();
    }

    track_event_holder = llist_root_new();

    for (source_track_index=0; source_track_index<midi->num_tracks; source_track_index++)
    {
        struct MidiTrack *source_track = midi->tracks[source_track_index];
        struct GmidEvent *event;
        size_t track_pos;
        size_t buffer_len;
        int32_t command;
        int destination_track = -1;

        if (source_track == NULL)
        {
            continue;
        }

        command = 0;
        track_pos = 0;
        buffer_len = source_track->ck_data_size;

        /**
         * Iterate the events in this track and add to track_event_holder.
         * This addresses two problems.
         * 1) Some seq tracks are null, and this isn't captured in output MIDI,
         *    the only way to recognize this is by looking at the command channel
         *    in the track events.
         * 2) A track might start with meta events which don't have a command
         *    channel, so the destination seq track won't be known yet.
         * 
         * After all the events from the track are added to the temp list,
         * the command channel should have been seen at least once.
         * Use the first read command channel to determine which seq
         * track these events belong to.
        */
        while (track_pos < buffer_len)
        {
            int bytes_read = 0;
            
            // track position will be updated according to how many bytes read.
            event = GmidEvent_new_from_buffer(source_track->data, &track_pos, buffer_len, MIDI_IMPLEMENTATION_STANDARD, command, &bytes_read);

            if (event == NULL)
            {
                stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: event is NULL\n", __func__, __LINE__);
            }

            if (bytes_read == 0)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d>: invalid bytes_read (0) from GmidEvent_new_from_buffer\n", __func__, __LINE__);
            }

            // Set destination track to first seen command channel.
            if (destination_track == -1 && event->command_channel > -1)
            {
                destination_track = event->command_channel;
            }

            command = GmidEvent_get_midi_command(event);

            // only allow running status for the "regular" MIDI commands
            if ((command & 0xffffff00) != 0 || (command & 0xffffffff) == 0xff)
            {
                command = 0;
            }

            node = llist_node_new();
            node->data = event;
            llist_root_append_node(track_event_holder, node);
        }

        if (destination_track < 0 || destination_track > CSEQ_FILE_NUM_TRACKS)
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d>: destination_track %d was not resolved from nth midi track %d\n", __func__, __LINE__, destination_track, source_track_index);
        }

        // now that destination track is known, move from temp list
        // to correct track.
        while (track_event_holder->count > 0)
        {
            node = track_event_holder->root;
            llist_node_move(gmidi_tracks[destination_track]->events, track_event_holder, node);
        }
    }

    // All events from source midi have been added to appropriate tracks.
    for (i=0; i<CSEQ_FILE_NUM_TRACKS; i++)
    {
        // Set absolute time based on the delta times,
        // then sort according to absolute times.
        GmidTrack_absolute_from_delta(gmidi_tracks[i]);
        
        // Sort events by absolute time
        llist_root_merge_sort(gmidi_tracks[i]->events, llist_node_gmidevent_compare_smaller);

        // Convert note-off and implicit note-off to cseq format.
        // Absolute times must be accurate in order to calculate duration.
        GmidTrack_cseq_note_on_from_midi(gmidi_tracks[i]);

        // Sort events by absolute time
        llist_root_merge_sort(gmidi_tracks[i]->events, llist_node_gmidevent_compare_smaller);

        /**
         * cseq2midi needed loop end to be linked to loop start to resolve
         * the loop count, but the other direction can just check that
         * the loop count event follows the loop start event (this is how gaudio converts).
         * This should be done last because the byte offset from the end to the
         * start node needs to be set, and adding additional nodes will break
         * the count.
        */
        GmidTrack_midi_to_cseq_loop(gmidi_tracks[i]);

        GmidTrack_set_track_size_bytes(gmidi_tracks[i]);

        // add a margin of error
        gmidi_tracks[i]->cseq_data = (uint8_t *)malloc_zero(1, gmidi_tracks[i]->cseq_track_size_bytes + 50);

        size_t write_len = GmidTrack_write_to_cseq_buffer(gmidi_tracks[i], gmidi_tracks[i]->cseq_data, gmidi_tracks[i]->cseq_track_size_bytes + 50);
        gmidi_tracks[i]->cseq_data_len = write_len;
    }

    result = CseqFile_new_roll_from_tracks(gmidi_tracks, CSEQ_FILE_NUM_TRACKS);

    // copy division from source
    result->division = midi->division;

    for (i=0; i<CSEQ_FILE_NUM_TRACKS; i++)
    {
        free(gmidi_tracks[i]);
    }

    free(gmidi_tracks);

    llist_node_root_free(track_event_holder);

    TRACE_LEAVE(__func__)
    return result;   
}

/**
 * Allocates memory for a {@code struct GmidEvent}.
 * @returns: pointer to new object.
*/
struct GmidEvent *GmidEvent_new()
{
    TRACE_ENTER(__func__)

    struct GmidEvent *event = (struct GmidEvent *)malloc_zero(1, sizeof(struct GmidEvent));
    
    event->id = g_event_id++;
    
    // default to invalid command channel.
    event->command_channel = -1;

    TRACE_LEAVE(__func__)

    return event;
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
 * This unrolls a portion of a compressed MIDI seq into a common
 * track container. This replaces all markers with actual byte contents.
 * Both containers must have been previously instantiated.
 * The container track should have position and length values set,
 * these will be used to read the cseq data.
 * @param cseq: Existing n64 compressed seq file with compressed MIDI data.
 * {@code cseq->track_offset} and {@code cseq->track_lengths} must be set.
 * @param track: Existing Gmidtrack, cseq_data should be unallocated.
 * {@code track->cseq_track_index} must be set.
*/
void CseqFile_unroll(struct CseqFile *cseq, struct GmidTrack *track)
{
    TRACE_ENTER(__func__)

    size_t pos = 0;
    size_t unrolled_pos = 0;
    size_t cseq_len = 0;
    uint8_t *temp_ptr;
    size_t compressed_read_len = 0;

    struct llist_root *loop_stack;
    struct llist_node *node;
    int loop_state_start = 0;
    int loop_state_start_number = 0;
    int previous_loop_state_start_number = -1;
    int loop_state_end = 0;
    int loop_current_offset = 0;
    int32_t loop_state_end_offset = 0;
    int pattern_counts = 0;

    if (track->cseq_data != NULL)
    {
        free(track->cseq_data);
    }

    // The seq loop end event offset is calculated after pattern compression.
    // This method will track the amount of bytes added due to unrolling
    // inside a loop, then add that to the offset, so the value will
    // be correct for an unrolled seq.
    loop_stack = llist_root_new();

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

        switch (loop_state_start)
        {
            case 0:
            {
                loop_state_start_number = 0;

                if (cseq->compressed_data[pos] == 0xff)
                {
                    loop_state_start++;
                }
                else
                {
                    loop_state_start = 0;
                }
            }
            break;

            case 1:
            {
                if (cseq->compressed_data[pos] == 0x2e)
                {
                    loop_state_start++;
                }
                else
                {
                    loop_state_start = 0;
                }
            }
            break;

            case 2:
            {
                /**
                 * This is a hack, Archives channel 9 has two seq meta loop start
                 * events for loop #0, this will ignore any duplicate loop start
                 * for loop number the same as previous.
                */
                if (previous_loop_state_start_number == cseq->compressed_data[pos])
                {
                    // skip next 0xff, to start over back in state 0
                    loop_state_start = 4;
                }
                else
                {
                    previous_loop_state_start_number = loop_state_start_number;
                    loop_state_start_number = cseq->compressed_data[pos];
                    loop_state_start++;
                }
            }
            break;

            case 3:
            {
                if (cseq->compressed_data[pos] == 0xff)
                {
                    struct seq_unroll_grow *grow = seq_unroll_grow_new();
                    grow->start_grow = loop_current_offset;
                    grow->loop_id = loop_state_start_number;
                    grow->file_offset = pos - 3;
                    loop_current_offset = 0;

                    node = llist_node_new();
                    node->data = grow;
                    llist_root_append_node(loop_stack, node);
                }
             
                loop_state_start = 0;
                loop_state_start_number = 0;
            }
            break;

            default:
                loop_state_start = 0;
            break;
        }

        switch (loop_state_end)
        {
            case 0:
            {
                loop_state_end_offset = 0;

                if (cseq->compressed_data[pos] == 0xff)
                {
                    loop_state_end++;
                }
                else
                {
                    loop_state_end = 0;
                }
            }
            break;

            case 1:
            {
                if (cseq->compressed_data[pos] == 0x2d)
                {
                    loop_state_end++;
                }
                else
                {
                    loop_state_end = 0;
                }
            }
            break;

            case 2:
            case 3:
            {
                loop_state_end++;
            }
            break;

            case 4:
            {
                loop_state_end_offset = cseq->compressed_data[pos];
                loop_state_end++;
            }
            break;
            case 5:
            case 6:
            case 7:
            {
                loop_state_end_offset <<= 8;
                loop_state_end_offset |= cseq->compressed_data[pos];
                loop_state_end++;
            }
            break;
        }

        if (loop_state_end == 8)
        {
            loop_state_end = 0;

            // adjust total offset back to parent loop container
            node = loop_stack->tail;
            struct seq_unroll_grow *grow = (struct seq_unroll_grow *)node->data;

            if (grow == NULL)
            {
                stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> grow is NULL\n", __func__, __LINE__);
            }

            // sanity check
            if (grow->file_offset != (int)pos - loop_state_end_offset - 3)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> loop end delta %d from cseq compressed file offset %ld does not equal current loop start position %d\n", __func__, __LINE__, loop_state_end_offset, pos, grow->file_offset);
            }

            if (g_verbosity >=  VERBOSE_DEBUG)
            {
                printf("adjusting loop %d, cseq pos %ld, unroll pos %ld, by %d from %d to %d\n", grow->loop_id, pos, unrolled_pos, loop_current_offset, loop_state_end_offset, loop_state_end_offset + loop_current_offset);
            }

            // Adjust offset from cseq to count unrolled bytes added.
            loop_state_end_offset += loop_current_offset;
            
            // back track into output buffer (3 out of 4 bytes) and set updated values.
            track->cseq_data[unrolled_pos - 3] = (uint8_t)((loop_state_end_offset >> 24) & 0xff);
            track->cseq_data[unrolled_pos - 2] = (uint8_t)((loop_state_end_offset >> 16) & 0xff);
            track->cseq_data[unrolled_pos - 1] = (uint8_t)((loop_state_end_offset >> 8) & 0xff);
            track->cseq_data[unrolled_pos] = (uint8_t)((loop_state_end_offset >> 0) & 0xff);

            // pop total offset back to parent loop.
            loop_current_offset = grow->start_grow;

            // current loop is done, free memory
            seq_unroll_grow_free(grow);
            node->data = NULL;
            llist_node_free(loop_stack, node);

            // adjust current current totals and continue
            pos++;
            compressed_read_len++;
            unrolled_pos++;
            continue;
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
        pattern_counts++;

        if (g_verbosity >=  VERBOSE_DEBUG)
        {
            printf("found pattern of length %d, loop_current_offset=%d, pattern_counts=%d\n", length, loop_current_offset, pattern_counts);
        }

        loop_current_offset += length - 4;

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

    llist_node_root_free(loop_stack);

    TRACE_LEAVE(__func__)
}

/**
 * Creates a new compressed MIDI container from existing track data.
 * The tracks must be passed in order.
 * Null tracks are accepted.
 * Track data is collected, then the "pattern marker" substitution is
 * applied to build the final file.
 * This allocates memory.
 * @param track: List of pointers to {@code GmidTrack}. Unused tracks can be NULL.
 * @param num_tracks: Number of items in {@code track}.
 * @returns: pointer to new file container.
*/
struct CseqFile *CseqFile_new_roll_from_tracks(struct GmidTrack **track, size_t num_tracks)
{
    TRACE_ENTER(__func__)

    // null implementation
    int i;
    int cseq_pointer_pos;
    int non_empty_tracks = 0;
    int num_tracks_int = (int)num_tracks;
    size_t data_size = 0;
    for (i=0; i<num_tracks_int; i++)
    {
        if (track[i] != NULL)
        {
            non_empty_tracks++;

            data_size += track[i]->cseq_data_len;
        }
    }

    struct CseqFile *cseq = CseqFile_new();
    cseq->non_empty_num_tracks = non_empty_tracks;
    cseq->compressed_data = (uint8_t *)malloc_zero(1, data_size);
    cseq->compressed_data_len = data_size;

    cseq_pointer_pos = 0;
    for (i=0; i<num_tracks_int; i++)
    {
        if (track[i] != NULL)
        {
            cseq->track_offset[i] = cseq_pointer_pos;
            // adjust offset to start of data in file.
            cseq->track_offset[i] += CSEQ_FILE_HEADER_SIZE_BYTES;
            // copy into output buffer
            memcpy(&cseq->compressed_data[cseq_pointer_pos], track[i]->cseq_data, track[i]->cseq_data_len);
            cseq_pointer_pos += track[i]->cseq_data_len;
        }
        else
        {
            cseq->track_offset[i] = 0;
        }
    }

    TRACE_LEAVE(__func__)
    return cseq;
}

/**
 * Reads a buffer of MIDI data (regular MIDI, or n64 seq) and parses
 * into gaudio common event format. Delta event time is set
 * on event for both MIDI and seq, but absolute time remains
 * default value.
 * This allocates memory.
 * @param buffer: Buffer containing MIDI data. Should be address of start of buffer.
 * @param pos_ptr: In/Out parameter. Current position to read from in the buffer.
 * @param buffer_len: Length of buffer in bytes.
 * @param buffer_type: Type of MIDI data.
 * @param current_command: most recent command, used for running status commands. This needs to include the command channel.
 * @param bytes_read: Out parameter. Optional. Will contain the number of bytes read from the buffer.
 * @returns: new event container.
*/
struct GmidEvent *GmidEvent_new_from_buffer(uint8_t *buffer, size_t *pos_ptr, size_t buffer_len, enum MIDI_IMPLEMENTATION buffer_type, int32_t current_command, int *bytes_read)
{
    /**
     * error check for exceeding buffer_len generally happens before
     * the next read, except for certain varint instances.
    */
    TRACE_ENTER(__func__)

    if (buffer == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> buffer is null\n", __func__, __LINE__);
    }

    if (pos_ptr == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> pos_ptr is null\n", __func__, __LINE__);
    }

    if (buffer_len == 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> buffer_len is zero\n", __func__, __LINE__);
    }

    if (buffer_type == MIDI_IMPLEMENTATION_COMPRESSED_SEQ)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> Compressed seq must be unrolled first\n", __func__, __LINE__);
    }

    // current position in buffer
    size_t pos;
    // current byte read
    uint8_t b;
    // most recent command channel
    int command_channel = -1;
    int command_without_channel;
    // temp varint for parsing/copying values
    struct var_length_int varint;
    // debug print buffer
    char print_buffer[MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN];
    // debug print buffer
    char description_buffer[MIDI_DESCRIPTION_TEXT_BUFFER_LEN];
    // assumes `b` is a valid command, this holds the general command
    int message_type;
    // flag to indicate meta event or not.
    int read_command_values;
    int local_bytes_read;

    struct GmidEvent *event = NULL;

    pos = *pos_ptr;
    local_bytes_read = 0;
    read_command_values = 1;

    // if meta command was passed as current, ignore it.
    if (((uint32_t)current_command & 0xffffff00) == 0xff00)
    {
        current_command = 0;
    }
    else
    {
        command_channel = current_command & 0x0f;
    }

    command_without_channel = current_command & 0xf0;

    memset(&varint, 0, sizeof(struct var_length_int));
    varint_value_to_int32(&buffer[pos], 4, &varint);

    if (varint.num_bytes < 1)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> parse error, could not read variable length integer. pos=%ld\n", __func__, __LINE__, pos);
    }

    if (pos > buffer_len)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> parse error, exceed buffer length when reading delta time pos=%ld\n", __func__, __LINE__, pos);
    }

    pos += varint.num_bytes;
    local_bytes_read += varint.num_bytes;

    if (g_midi_parse_debug)
    {
        memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

        snprintf(print_buffer, MIDI_DESCRIPTION_TEXT_BUFFER_LEN, "delta time: %d (varint=0x%06x)", varint.standard_value, varint.value);
        fflush_printf(stdout, "%s\n", print_buffer);
    }

    event = GmidEvent_new();

    // Set the file offset the event begins at. pos was updated, but the pointer
    // of the argument won't be updated until the end of this method.
    event->file_offset = *pos_ptr;

    // Set delta time. Absolute time won't be set until all events are added to the track.
    varint_copy(&event->cseq_delta_time, &varint);
    varint_copy(&event->midi_delta_time, &varint);

    /**
     * If current command channel is known, set that on event.
     * If the event being read has a command channel, that will
     * update the event. If this is a system command, the channel
     * should be reset to -1.
     * The command parameter parsing should not change the channel.
    */
    if (command_channel > -1)
    {
        event->command_channel = command_channel;
    }

    // check for command
    if (buffer[pos] & 0x80)
    {
        if (pos + 1 > buffer_len)
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> exceeded buffer len %ld when parsing event\n", __func__, __LINE__, buffer_len);
        }

        b = buffer[pos++];
        local_bytes_read++;
        message_type = b & 0xf0;
        command_channel = b & 0x0f;

        // set command channel on event. If this is system, will be cleared.
        event->command_channel = command_channel;

        // system command
        if (b == 0xff)
        {
            command_without_channel = -1;
            read_command_values = 0;

            if (pos + 1 > buffer_len)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> exceeded buffer len %ld when parsing event\n", __func__, __LINE__, buffer_len);
            }

            // if this is a system command, then channel isn't included in event
            event->command_channel = -1;

            b = buffer[pos++];
            local_bytes_read++;

            if (b == CSEQ_COMMAND_BYTE_TEMPO && buffer_type == MIDI_IMPLEMENTATION_SEQ)
            {
                if (pos + CSEQ_COMMAND_PARAM_BYTE_TEMPO > buffer_len)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> exceeded buffer len %ld when parsing event\n", __func__, __LINE__, buffer_len);
                }

                int tempo = 0;

                // copy raw value in same endian, other values set at end.
                memcpy(event->cseq_command_parameters_raw, &buffer[pos], CSEQ_COMMAND_PARAM_BYTE_TEMPO);

                // parse command values.
                tempo |= buffer[pos++];
                local_bytes_read++;
                tempo <<= 8;
                tempo |= buffer[pos++];
                local_bytes_read++;
                tempo <<= 8;
                tempo |= buffer[pos++];
                local_bytes_read++;

                // optional debug print
                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: %d", CSEQ_COMMAND_NAME_TEMPO, tempo);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }

                // save parsed data
                event->dual = NULL;
                event->command = CSEQ_COMMAND_BYTE_TEMPO_WITH_META;
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
                event->midi_command_parameters[0] = 3; // len
                event->midi_command_parameters[1] = tempo;
                event->midi_command_parameters_len = MIDI_COMMAND_NUM_PARAM_TEMPO;
            }
            else if (b == MIDI_COMMAND_BYTE_TEMPO && buffer_type == MIDI_IMPLEMENTATION_STANDARD)
            {
                if (pos + MIDI_COMMAND_PARAM_BYTE_TEMPO > buffer_len)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> exceeded buffer len %ld when parsing event\n", __func__, __LINE__, buffer_len);
                }

                b = buffer[pos++];
                local_bytes_read++;
                if (b != 0x03)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s: %s: expected len=0x03 but read '0x%x', pos=%ld.\n", __func__, MIDI_COMMAND_NAME_TEMPO, b, pos);
                }

                int tempo = 0;

                // copy raw value in same endian, other values set at end.
                memcpy(event->midi_command_parameters_raw, &buffer[pos], MIDI_COMMAND_PARAM_BYTE_TEMPO);

                // parse command values.
                tempo |= buffer[pos++];
                local_bytes_read++;
                tempo <<= 8;
                tempo |= buffer[pos++];
                local_bytes_read++;
                tempo <<= 8;
                tempo |= buffer[pos++];
                local_bytes_read++;

                // optional debug print
                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: %d", MIDI_COMMAND_NAME_TEMPO, tempo);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }

                // save parsed data
                event->dual = NULL;
                event->command = CSEQ_COMMAND_BYTE_TEMPO_WITH_META;
                event->cseq_valid = 1;
                event->midi_valid = 1;

                // MIDI format
                event->midi_command_len = MIDI_COMMAND_LEN_TEMPO;
                event->midi_command_parameters_raw[0] = 3; // len
                event->midi_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_TEMPO;
                event->midi_command_parameters[0] = 3; // len
                event->midi_command_parameters[1] = tempo;
                event->midi_command_parameters_len = MIDI_COMMAND_NUM_PARAM_TEMPO;

                // convert to cseq format
                event->cseq_command_len = CSEQ_COMMAND_LEN_TEMPO;
                event->cseq_command_parameters_raw_len = CSEQ_COMMAND_PARAM_BYTE_TEMPO;
                event->cseq_command_parameters[0] = tempo;
                event->cseq_command_parameters_raw[0] = (uint8_t)((tempo >> 16) & 0xff);
                event->cseq_command_parameters_raw[1] = (uint8_t)((tempo >> 8) & 0xff);
                event->cseq_command_parameters_raw[2] = (uint8_t)((tempo >> 0) & 0xff);
                event->cseq_command_parameters_len = CSEQ_COMMAND_NUM_PARAM_TEMPO;
            }
            else if (b == CSEQ_COMMAND_BYTE_LOOP_START && buffer_type == MIDI_IMPLEMENTATION_SEQ)
            {
                if (pos + CSEQ_COMMAND_PARAM_BYTE_LOOP_START > buffer_len)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> exceeded buffer len %ld when parsing event\n", __func__, __LINE__, buffer_len);
                }

                int loop_number = 0;

                // copy raw value in same endian, other values set at end.
                memcpy(event->cseq_command_parameters_raw, &buffer[pos], CSEQ_COMMAND_PARAM_BYTE_LOOP_START);

                // parse command values.
                loop_number = buffer[pos++];
                local_bytes_read++;
                b = buffer[pos++];
                local_bytes_read++;
                if (b != 0xff)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s: %s: expected end of command byte 0xff but read '0x%x', pos=%ld.\n", __func__, CSEQ_COMMAND_NAME_LOOP_START, b, pos);
                }

                // optional debug print
                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: loop number %d", CSEQ_COMMAND_NAME_LOOP_START, loop_number);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }

                // save parsed data
                event->command = CSEQ_COMMAND_BYTE_LOOP_START_WITH_META;
                event->dual = NULL;
                event->cseq_valid = 1;
                event->midi_valid = 0;

                // cseq format
                event->cseq_command_len = CSEQ_COMMAND_LEN_LOOP_START;
                event->cseq_command_parameters_raw_len = CSEQ_COMMAND_PARAM_BYTE_LOOP_START;
                event->cseq_command_parameters[0] = loop_number;
                event->cseq_command_parameters[1] = b;
                event->cseq_command_parameters_len = CSEQ_COMMAND_NUM_PARAM_LOOP_START;

                // convert to MIDI format: not supported
            }
            else if (b == CSEQ_COMMAND_BYTE_LOOP_END && buffer_type == MIDI_IMPLEMENTATION_SEQ)
            {
                if (pos + CSEQ_COMMAND_PARAM_BYTE_LOOP_END > buffer_len)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> exceeded buffer len %ld when parsing event\n", __func__, __LINE__, buffer_len);
                }

                int loop_count = 0;
                int current_loop_count = 0;
                int32_t loop_difference = 0;

                // copy raw value in same endian, other values set at end.
                memcpy(event->cseq_command_parameters_raw, &buffer[pos], CSEQ_COMMAND_PARAM_BYTE_LOOP_END);

                // parse command values.
                loop_count = buffer[pos++];
                local_bytes_read++;
                current_loop_count = buffer[pos++];
                local_bytes_read++;

                loop_difference |= buffer[pos++];
                local_bytes_read++;
                loop_difference <<= 8;
                loop_difference |= buffer[pos++];
                local_bytes_read++;
                loop_difference <<= 8;
                loop_difference |= buffer[pos++];
                local_bytes_read++;
                loop_difference <<= 8;
                loop_difference |= buffer[pos++];
                local_bytes_read++;

                // optional debug print
                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: count %d, current count %d, diff 0x%08x", CSEQ_COMMAND_NAME_LOOP_END, loop_count, current_loop_count, loop_difference);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }

                // save parsed data
                event->command = CSEQ_COMMAND_BYTE_LOOP_END_WITH_META;
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

                // convert to MIDI format: not supported
            }
            else if (b == CSEQ_COMMAND_BYTE_END_OF_TRACK && buffer_type == MIDI_IMPLEMENTATION_SEQ)
            {
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
                event->command = MIDI_COMMAND_BYTE_END_OF_TRACK_WITH_META;
                event->dual = NULL;
                event->cseq_valid = 1;
                event->midi_valid = 1;

                // cseq format
                event->cseq_command_len = CSEQ_COMMAND_LEN_END_OF_TRACK;
                event->cseq_command_parameters_raw_len = CSEQ_COMMAND_PARAM_BYTE_END_OF_TRACK;
                event->cseq_command_parameters_len = CSEQ_COMMAND_NUM_PARAM_END_OF_TRACK;

                // MIDI format
                // NOTE: command is wrong for MIDI, correct command is retrieved via `GmidEvent_get_midi_command`
                event->midi_command_len = MIDI_COMMAND_LEN_END_OF_TRACK;
                event->midi_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_END_OF_TRACK;
                event->midi_command_parameters_len = MIDI_COMMAND_NUM_PARAM_END_OF_TRACK;
            }
            else if (b == MIDI_COMMAND_BYTE_END_OF_TRACK && buffer_type == MIDI_IMPLEMENTATION_STANDARD)
            {
                // no parameters to copy.

                b = buffer[pos++];
                local_bytes_read++;
                if (b != 0x00)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s: %s: expected end of command byte 0x00 but read '0x%x', pos=%ld.\n", __func__, MIDI_COMMAND_NAME_END_OF_TRACK, b, pos);
                }

                // optional debug print
                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s", MIDI_COMMAND_NAME_END_OF_TRACK);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }

                // save parsed data
                event->command = MIDI_COMMAND_BYTE_END_OF_TRACK_WITH_META;
                event->dual = NULL;
                event->cseq_valid = 1;
                event->midi_valid = 1;

                // cseq format
                event->cseq_command_len = CSEQ_COMMAND_LEN_END_OF_TRACK;
                event->cseq_command_parameters_raw_len = CSEQ_COMMAND_PARAM_BYTE_END_OF_TRACK;
                event->cseq_command_parameters_len = CSEQ_COMMAND_NUM_PARAM_END_OF_TRACK;

                // MIDI format
                // NOTE: command is wrong for MIDI, correct command is retrieved via `GmidEvent_get_midi_command`
                event->midi_command_len = MIDI_COMMAND_LEN_END_OF_TRACK;
                event->midi_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_END_OF_TRACK;
                event->midi_command_parameters_len = MIDI_COMMAND_NUM_PARAM_END_OF_TRACK;
            }
            else
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> parse error (system command), buffer type=%d, pos=%ld.\n", __func__, __LINE__, buffer_type, pos);
            }
        }
        else if (b == CSEQ_COMMAND_BYTE_PATTERN && buffer_type == MIDI_IMPLEMENTATION_SEQ)
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s: parse error -- invalid compressed MIDI, \"pattern\" command not allowed. pos=%ld.\n", __func__, pos);
        }
        else if (message_type == MIDI_COMMAND_BYTE_NOTE_OFF && buffer_type == MIDI_IMPLEMENTATION_SEQ)
        {
            // note off
            stderr_exit(EXIT_CODE_GENERAL, "%s: parse error -- invalid compressed MIDI, \"note off\" command not allowed. pos=%d.\n", __func__, pos);
        }
        else if (message_type == MIDI_COMMAND_BYTE_NOTE_OFF && buffer_type == MIDI_IMPLEMENTATION_STANDARD)
        {
            command_without_channel = MIDI_COMMAND_BYTE_NOTE_OFF;

            if (g_midi_parse_debug)
            {
                memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "command %s: channel %d", MIDI_COMMAND_NAME_NOTE_OFF, command_channel);
                fflush_printf(stdout, "%s\n", print_buffer);
            }
        }
        else if (message_type == MIDI_COMMAND_BYTE_NOTE_ON /* regular and seq */)
        {
            command_without_channel = MIDI_COMMAND_BYTE_NOTE_ON;

            if (g_midi_parse_debug)
            {
                memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "command %s: channel %d", MIDI_COMMAND_NAME_NOTE_ON, command_channel);
                fflush_printf(stdout, "%s\n", print_buffer);
            }
        }
        else if (message_type == MIDI_COMMAND_BYTE_POLYPHONIC_PRESSURE /* regular and seq */)
        {
            command_without_channel = MIDI_COMMAND_BYTE_POLYPHONIC_PRESSURE;

            if (g_midi_parse_debug)
            {
                memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "command %s: channel %d", MIDI_COMMAND_NAME_POLYPHONIC_PRESSURE, command_channel);
                fflush_printf(stdout, "%s\n", print_buffer);
            }
        }
        else if (message_type == MIDI_COMMAND_BYTE_CONTROL_CHANGE /* regular and seq */)
        {
            command_without_channel = MIDI_COMMAND_BYTE_CONTROL_CHANGE;

            if (g_midi_parse_debug)
            {
                memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "command %s: channel %d", MIDI_COMMAND_NAME_CONTROL_CHANGE, command_channel);
                fflush_printf(stdout, "%s\n", print_buffer);
            }
        }
        else if (message_type == MIDI_COMMAND_BYTE_PROGRAM_CHANGE /* regular and seq */)
        {
            command_without_channel = MIDI_COMMAND_BYTE_PROGRAM_CHANGE;

            if (g_midi_parse_debug)
            {
                memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "command %s: channel %d", MIDI_COMMAND_NAME_PROGRAM_CHANGE, command_channel);
                fflush_printf(stdout, "%s\n", print_buffer);
            }
        }
        else if (message_type == MIDI_COMMAND_BYTE_CHANNEL_PRESSURE /* regular and seq */)
        {
            command_without_channel = MIDI_COMMAND_BYTE_CHANNEL_PRESSURE;

            if (g_midi_parse_debug)
            {
                memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "command %s: channel %d", MIDI_COMMAND_NAME_CHANNEL_PRESSURE, command_channel);
                fflush_printf(stdout, "%s\n", print_buffer);
            }
        }
        else if (message_type == MIDI_COMMAND_BYTE_PITCH_BEND /* regular and seq */)
        {
            // Pitch Bend
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> Pitch Bend not implemented, track index=%d, pos=%d.\n", __func__, __LINE__, pos);
        }
        else
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> parse error (command), buffer type=%d, pos=%ld.\n", __func__, __LINE__, buffer_type, pos);
        }
    }

    if (read_command_values)
    {
        if (command_without_channel < 1)
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> Invalid current_command.\n", __func__, __LINE__);
        }

        if (command_channel < 0)
        {
            command_channel = current_command & 0x0f;
        }

        // done with command, now read event
        if (command_without_channel == MIDI_COMMAND_BYTE_NOTE_OFF && buffer_type == MIDI_IMPLEMENTATION_SEQ)
        {
            // note off
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> parse error -- invalid compressed MIDI, \"note off\" command not allowed. pos=%ld.\n", __func__, __LINE__, pos);
        }
        else if (command_without_channel == MIDI_COMMAND_BYTE_NOTE_OFF && buffer_type == MIDI_IMPLEMENTATION_STANDARD)
        {
            if (pos + 2 > buffer_len)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> exceeded buffer len %ld when parsing event\n", __func__, __LINE__, buffer_len);
            }

            // parse command values.
            int note = buffer[pos++];
            local_bytes_read++;
            int velocity = buffer[pos++];
            local_bytes_read++;

            // copy raw value in same endian, other values set at end.
            memcpy(event->cseq_command_parameters_raw, &buffer[pos - MIDI_COMMAND_PARAM_BYTE_NOTE_OFF], MIDI_COMMAND_PARAM_BYTE_NOTE_OFF);
            memcpy(event->midi_command_parameters_raw, &buffer[pos - MIDI_COMMAND_PARAM_BYTE_NOTE_OFF], MIDI_COMMAND_PARAM_BYTE_NOTE_OFF);

            // optional debug print
            if (g_midi_parse_debug)
            {
                memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);
                memset(description_buffer, 0, MIDI_DESCRIPTION_TEXT_BUFFER_LEN);

                midi_note_to_name(note, description_buffer, MIDI_DESCRIPTION_TEXT_BUFFER_LEN);
                snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: channel %d, %s velocity=%d, duration=%d", MIDI_COMMAND_NAME_NOTE_OFF, command_channel, description_buffer, velocity, varint.standard_value);
                fflush_printf(stdout, "%s\n", print_buffer);
            }

            // save parsed data
            event->command = MIDI_COMMAND_BYTE_NOTE_OFF;
            event->dual = NULL;
            event->cseq_valid = 0;
            event->midi_valid = 1;

            // MIDI format
            event->midi_command_parameters_raw[0] = note;
            event->midi_command_parameters_raw[1] = velocity;
            event->midi_command_len = MIDI_COMMAND_LEN_NOTE_OFF;
            event->midi_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_NOTE_OFF;
            event->midi_command_parameters[0] = note;
            event->midi_command_parameters[1] = velocity;
            event->midi_command_parameters_len = MIDI_COMMAND_NUM_PARAM_NOTE_OFF;

            // not valid for cseq
        }
        else if (command_without_channel == MIDI_COMMAND_BYTE_NOTE_ON && buffer_type == MIDI_IMPLEMENTATION_SEQ)
        {
            if (pos + 2 > buffer_len)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> exceeded buffer len %ld when parsing event\n", __func__, __LINE__, buffer_len);
            }

            // parse command values.
            int note = buffer[pos++];
            local_bytes_read++;
            int velocity = buffer[pos++];
            local_bytes_read++;

            memset(&varint, 0, sizeof(struct var_length_int));
            varint_value_to_int32(&buffer[pos], 4, &varint);
            pos += varint.num_bytes;
            local_bytes_read += varint.num_bytes;

            if (pos > buffer_len)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> parse error, exceed buffer length when reading delta time pos=%ld\n", __func__, __LINE__, pos);
            }

            int param_len = 2 + varint.num_bytes;

            // copy raw value in same endian, other values set at end.
            memcpy(event->cseq_command_parameters_raw, &buffer[pos - param_len], param_len);
            memcpy(event->midi_command_parameters_raw, event->cseq_command_parameters_raw, MIDI_COMMAND_PARAM_BYTE_NOTE_ON);

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
        }
        else if (command_without_channel == MIDI_COMMAND_BYTE_NOTE_ON && buffer_type == MIDI_IMPLEMENTATION_STANDARD)
        {
            if (pos + 2 > buffer_len)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> exceeded buffer len %ld when parsing event\n", __func__, __LINE__, buffer_len);
            }

            // parse command values.
            int note = buffer[pos++];
            local_bytes_read++;
            int velocity = buffer[pos++];
            local_bytes_read++;

            // copy raw value in same endian, other values set at end.
            memcpy(event->cseq_command_parameters_raw, &buffer[pos - MIDI_COMMAND_PARAM_BYTE_NOTE_ON], MIDI_COMMAND_PARAM_BYTE_NOTE_ON);
            memcpy(event->midi_command_parameters_raw, &buffer[pos - MIDI_COMMAND_PARAM_BYTE_NOTE_ON], MIDI_COMMAND_PARAM_BYTE_NOTE_ON);

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
            event->cseq_valid = 0;
            event->midi_valid = 1;

            // MIDI format
            event->midi_command_parameters_raw[0] = note;
            event->midi_command_parameters_raw[1] = velocity;
            event->midi_command_len = MIDI_COMMAND_LEN_NOTE_ON;
            event->midi_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_NOTE_ON;
            event->midi_command_parameters[0] = note;
            event->midi_command_parameters[1] = velocity;
            event->midi_command_parameters_len = MIDI_COMMAND_NUM_PARAM_NOTE_ON;

            // don't have enough information to convert to cseq format yet
            event->cseq_command_len = CSEQ_COMMAND_LEN_NOTE_ON;
            event->cseq_command_parameters_raw_len = 0;
            event->cseq_command_parameters[0] = note;
            event->cseq_command_parameters[1] = velocity;
            event->cseq_command_parameters[2] = 0; // duration, unknown at this time
            event->cseq_command_parameters_len = CSEQ_COMMAND_NUM_PARAM_NOTE_ON;
        }
        else if (command_without_channel == MIDI_COMMAND_BYTE_POLYPHONIC_PRESSURE /* regular and seq */)
        {
            if (pos + MIDI_COMMAND_PARAM_BYTE_POLYPHONIC_PRESSURE > buffer_len)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> exceeded buffer len %ld when parsing event\n", __func__, __LINE__, buffer_len);
            }

            // copy raw value in same endian, other values set at end.
            memcpy(event->cseq_command_parameters_raw, &buffer[pos], MIDI_COMMAND_PARAM_BYTE_POLYPHONIC_PRESSURE);
            
            // parse command values.
            int note = buffer[pos++];
            local_bytes_read++;
            int pressure = buffer[pos++];
            local_bytes_read++;

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
        }
        else if (command_without_channel == MIDI_COMMAND_BYTE_CONTROL_CHANGE /* regular and seq */)
        {
            if (pos + MIDI_COMMAND_PARAM_BYTE_CONTROL_CHANGE > buffer_len)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> exceeded buffer len %ld when parsing event\n", __func__, __LINE__, buffer_len);
            }

            // copy raw value in same endian, other values set at end.
            memcpy(event->cseq_command_parameters_raw, &buffer[pos], MIDI_COMMAND_PARAM_BYTE_CONTROL_CHANGE);
            
            // parse command values.
            int controller = buffer[pos++];
            local_bytes_read++;
            int new_value = buffer[pos++];
            local_bytes_read++;

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

            // special case: reading standard MIDI file and get N64 non-standard loop command
            if (buffer_type == MIDI_IMPLEMENTATION_STANDARD)
            {
                if (controller == MIDI_CONTROLLER_LOOP_START
                    || controller == MIDI_CONTROLLER_LOOP_END
                    || controller == MIDI_CONTROLLER_LOOP_COUNT_0
                    || controller == MIDI_CONTROLLER_LOOP_COUNT_128)
                {
                    event->cseq_valid = 0;
                }
            }
        }
        else if (command_without_channel == MIDI_COMMAND_BYTE_PROGRAM_CHANGE /* regular and seq */)
        {
            if (pos + MIDI_COMMAND_PARAM_BYTE_PROGRAM_CHANGE > buffer_len)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> exceeded buffer len %ld when parsing event\n", __func__, __LINE__, buffer_len);
            }

            // copy raw value in same endian, other values set at end.
            memcpy(event->cseq_command_parameters_raw, &buffer[pos], MIDI_COMMAND_PARAM_BYTE_PROGRAM_CHANGE);
            
            // parse command values.
            int program = buffer[pos++];
            local_bytes_read++;

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
        }
        else if (command_without_channel == MIDI_COMMAND_BYTE_CHANNEL_PRESSURE /* regular and seq */)
        {
            if (pos + MIDI_COMMAND_PARAM_BYTE_CHANNEL_PRESSURE > buffer_len)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> exceeded buffer len %ld when parsing event\n", __func__, __LINE__, buffer_len);
            }

            // copy raw value in same endian, other values set at end.
            memcpy(event->cseq_command_parameters_raw, &buffer[pos], MIDI_COMMAND_PARAM_BYTE_CHANNEL_PRESSURE);
            
            // parse command values.
            int pressure = buffer[pos++];
            local_bytes_read++;

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
        }
        else if (command_without_channel == MIDI_COMMAND_BYTE_PITCH_BEND /* regular and seq */)
        {
            // Pitch Bend
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> Pitch Bend not implemented, buffer type=%d, pos=%ld.\n", __func__, __LINE__, buffer_type, pos);
        }
        else
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> parse error (command), buffer type=%d, pos=%ld.\n", __func__, __LINE__, buffer_type, pos);
        }
    }

    // set out parameters
    *pos_ptr = pos;
    if (bytes_read != NULL)
    {
        *bytes_read = local_bytes_read;
    }

    // done
    TRACE_LEAVE(__func__)
    return event;
}

/**
 * This iterates the event list and creates new seq meta events
 * from the MIDI controller loop start/count/end events.
 * @param gtrack: track to update.
*/
void GmidTrack_midi_to_cseq_loop(struct GmidTrack *gtrack)
{
    TRACE_ENTER(__func__)

    if (gtrack == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> gtrack is NULL\n", __func__, __LINE__);
    }

    struct llist_node *node;
    struct llist_node *new_node;
    struct llist_node *end_node;
    struct GmidEvent *event;
    struct GmidEvent *midi_count_event;
    struct GmidEvent *midi_end_event;

    /**
     * This iterates the list until finding a non standard MIDI loop start.
     * The node is converted to seq format and inserted before current.
     * The next node after the start is then checked. If it's a count node,
     * and end node should exist (according to how gaudio exports), so
     * iterate the event list searching for a matching end event,
     * keep tracking of the number of bytes between start and end.
     * Once found, create a new end event node.
    */
    node = gtrack->events->root;
    while (node != NULL)
    {
        event = (struct GmidEvent *)node->data;
        if (event != NULL)
        {
            // if the current event is the non standard MIDI loop start
            if (event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE
                && event->midi_command_parameters[0] == MIDI_CONTROLLER_LOOP_START)
            {
                struct GmidEvent *seq_loop_start;
                struct GmidEvent *seq_loop_end;

                // there's always enough information to insert a seq meta loop start event.
                seq_loop_start = GmidEvent_new();

                seq_loop_start->command = CSEQ_COMMAND_BYTE_LOOP_START_WITH_META;
                seq_loop_start->cseq_valid = 1;
                seq_loop_start->midi_valid = 0;

                // cseq format
                seq_loop_start->cseq_command_len = CSEQ_COMMAND_LEN_LOOP_START;
                seq_loop_start->cseq_command_parameters_raw_len = CSEQ_COMMAND_PARAM_BYTE_LOOP_START;
                // non standard MIDI loop start, parameter 1 is loop number.
                seq_loop_start->cseq_command_parameters[0] = event->midi_command_parameters[1];
                seq_loop_start->cseq_command_parameters_raw[0] = (uint8_t)(event->midi_command_parameters[1] & 0xff);
                seq_loop_start->cseq_command_parameters[1] = 0xff; // always 0xff
                seq_loop_start->cseq_command_parameters_raw[1] = (uint8_t)(0xff);
                seq_loop_start->cseq_command_parameters_len = CSEQ_COMMAND_NUM_PARAM_LOOP_START;

                // inherit existing event timing information,
                // but overwrite midi->cseq
                seq_loop_start->absolute_time = event->absolute_time;
                varint_copy(&seq_loop_start->cseq_delta_time, &event->midi_delta_time);
                varint_copy(&seq_loop_start->midi_delta_time, &event->midi_delta_time);

                // done with the start node, insert new node before current.
                new_node = llist_node_new();
                new_node->data = seq_loop_start;
                llist_node_insert_before(gtrack->events, node, new_node);

                // if the next node is the non standard MIDI loop count then a loop end node should exist.
                if (node->next != NULL
                    && node->next->data != NULL
                    && ((struct GmidEvent *)node->next->data)->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE
                    && (
                        ((struct GmidEvent *)node->next->data)->midi_command_parameters[0] == MIDI_CONTROLLER_LOOP_COUNT_0
                        || ((struct GmidEvent *)node->next->data)->midi_command_parameters[0] == MIDI_CONTROLLER_LOOP_COUNT_128
                    )
                )
                {
                    long byte_offset = 0;
                    int seq_loop_count = 0;
                    int found_end = 0;
                    int any_end = 0;

                    midi_count_event = (struct GmidEvent *)node->next->data;

                    if (midi_count_event->midi_command_parameters[0] == MIDI_CONTROLLER_LOOP_COUNT_0)
                    {
                        seq_loop_count = midi_count_event->midi_command_parameters[1];
                    }
                    else if (midi_count_event->midi_command_parameters[0] == MIDI_CONTROLLER_LOOP_COUNT_128)
                    {
                        seq_loop_count = 128 + midi_count_event->midi_command_parameters[1];
                    }

                    // start at the next node (the count) so the byte offset starts after the end
                    // of the start node.
                    end_node = node->next;
                    while (end_node != NULL && found_end == 0)
                    {
                        midi_end_event = (struct GmidEvent *)end_node->data;
                        if (midi_end_event != NULL)
                        {
                            byte_offset +=
                                midi_end_event->midi_delta_time.num_bytes
                                + midi_end_event->midi_command_parameters_raw_len;

                            // if this is an end loop event, and it's not claimed by any start loop event
                            if (midi_end_event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE
                                && midi_end_event->midi_command_parameters[0] == MIDI_CONTROLLER_LOOP_END
                                && (midi_end_event->flags & MIDI_MIDI_EVENT_LOOP_END_HANDLED) == 0)
                            {
                                any_end++;

                                // for the purposes of error handlind, want to track the number of unclaimed
                                // end nodes seen. This loop node only cares if the loop numbers
                                // match though.
                                if (midi_end_event->midi_command_parameters[1] == event->midi_command_parameters[1])
                                {
                                    seq_loop_end = GmidEvent_new();

                                    seq_loop_end->command = CSEQ_COMMAND_BYTE_LOOP_END_WITH_META;
                                    seq_loop_end->cseq_valid = 1;
                                    seq_loop_end->midi_valid = 0;

                                    // cseq format
                                    seq_loop_end->cseq_command_len = CSEQ_COMMAND_LEN_LOOP_END;
                                    seq_loop_end->cseq_command_parameters_raw_len = CSEQ_COMMAND_PARAM_BYTE_LOOP_END;
                                    seq_loop_end->cseq_command_parameters[0] = seq_loop_count; // loop count
                                    seq_loop_end->cseq_command_parameters_raw[0] = (uint8_t)(seq_loop_count & 0xff);
                                    seq_loop_end->cseq_command_parameters[1] = seq_loop_count; // loop count copy
                                    seq_loop_end->cseq_command_parameters_raw[1] = (uint8_t)(seq_loop_count & 0xff);
                                    seq_loop_end->cseq_command_parameters[2] = byte_offset; // delta to loop start
                                    seq_loop_end->cseq_command_parameters_raw[2] = (uint8_t)((byte_offset >> 24) & 0xff);
                                    seq_loop_end->cseq_command_parameters_raw[3] = (uint8_t)((byte_offset >> 16) & 0xff);
                                    seq_loop_end->cseq_command_parameters_raw[4] = (uint8_t)((byte_offset >> 8) & 0xff);
                                    seq_loop_end->cseq_command_parameters_raw[5] = (uint8_t)((byte_offset >> 0) & 0xff);
                                    seq_loop_end->cseq_command_parameters_len = CSEQ_COMMAND_NUM_PARAM_LOOP_END;

                                    // inherit existing event timing information,
                                    // but overwrite midi->cseq
                                    seq_loop_end->absolute_time = midi_end_event->absolute_time;
                                    varint_copy(&seq_loop_end->cseq_delta_time, &midi_end_event->midi_delta_time);
                                    varint_copy(&seq_loop_end->midi_delta_time, &midi_end_event->midi_delta_time);

                                    // done with the end node, insert new node before current.
                                    new_node = llist_node_new();
                                    new_node->data = seq_loop_end;
                                    llist_node_insert_before(gtrack->events, end_node, new_node);

                                    // set flags
                                    midi_end_event->flags |= MIDI_MIDI_EVENT_LOOP_END_HANDLED;
                                    found_end = 1;
                                }
                            }
                        }

                        end_node = end_node->next;
                    }

                    if (found_end == 0)
                    {
                        // This is a start loop event, and there is no end loop event in this track.
                        // In that case, flag this start event as bad and don't try to process.
                        // This happens on Archives channel 7.
                        if (any_end == 0)
                        {
                            event->flags |= MIDI_MALFORMED_EVENT_LOOP;
                        }
                        else
                        {
                            stderr_exit(EXIT_CODE_GENERAL, "%s %d> could not find MIDI non standard loop end event\n", __func__, __LINE__);
                        }
                    }
                }
                else
                {
                    // This is a start loop event, and there is no end loop event in this track.
                    // In that case, flag this start event as bad and don't try to process.
                    // This happens on Archives channel 7.
                    event->flags |= MIDI_MALFORMED_EVENT_LOOP;
                }
            }
        }

        node = node->next;
    }

    TRACE_LEAVE(__func__)
}

/**
 * This iterates the event list and tallies the expected file size in bytes.
 * @param gtrack: track to update.
*/
void GmidTrack_set_track_size_bytes(struct GmidTrack *gtrack)
{
    TRACE_ENTER(__func__)

    if (gtrack == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> gtrack is NULL\n", __func__, __LINE__);
    }

    struct llist_node *node;
    struct GmidEvent *event;

    gtrack->cseq_track_size_bytes = 0;
    gtrack->midi_track_size_bytes = 0;

    node = gtrack->events->root;
    for (; node != NULL; node = node->next)
    {
        event = (struct GmidEvent *)node->data;

        /**
         * There's a small error somewhere that is causing this to be not-quite-correct.
         * Theory: adjusting delta times (adding/removing nodes) changes the size of varint.
        */

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
 * Iterate the event list of a track, and for each seq meta
 * loop start event and loop end event, ensure there is a matching
 * reference and set the {@code GmidEvent->dual} pointer to it.
 * If the loop start event already has a dual pointer set,
 * no changes are made to that event.
 * This method needs to be run before any changes are made to
 * the event list, otherwise offsets will be incorrect and
 * this can fail.
*/
void GmidTrack_ensure_cseq_loop_dual(struct GmidTrack *gtrack)
{
    TRACE_ENTER(__func__)

    if (gtrack == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> gtrack is NULL\n", __func__, __LINE__);
    }

    struct llist_node *node;
    struct llist_node *search_node;
    struct GmidEvent *event;
    struct GmidEvent *search_event;
    struct llist_root *duplicate_start;
    struct llist_node *duplicate_node;
    struct GmidEvent *duplicate_event;

    node = gtrack->events->root;
    while (node != NULL)
    {
        event = (struct GmidEvent *)node->data;
        if (event != NULL)
        {
            // only consider seq loop start events that don't have a dual pointer set,
            // or haven't previously been marked as bad.
            if (event->command == CSEQ_COMMAND_BYTE_LOOP_START_WITH_META
                && (
                    (event->dual == NULL)
                    || ((event->flags & MIDI_MALFORMED_EVENT_LOOP) == 0)
                )
            )
            {
                int match = 0;
                int any_end = 0;

                duplicate_start = NULL;

                search_node = node;
                while (search_node != NULL && match == 0)
                {
                    search_event = (struct GmidEvent *)search_node->data;
                    if (search_event != NULL)
                    {
                        /**
                         * It's possible the same loop start can be included (incorrectly) in
                         * the same track more than once. Like Archives track 9, which has
                         * two loop #0 starts, and only one loop end. If another loop
                         * start event is encountered for the current loop, add it to a tracking
                         * list. The one with the correct offset will win and the others
                         * marked as invalid.
                        */
                        if (search_event->command == CSEQ_COMMAND_BYTE_LOOP_START_WITH_META)
                        {
                            if (search_event->cseq_command_parameters[0] == event->cseq_command_parameters[0])
                            {
                                if (duplicate_start == NULL)
                                {
                                    duplicate_start = llist_root_new();
                                    duplicate_node = llist_node_new();
                                    duplicate_node->data = search_event;
                                    llist_root_append_node(duplicate_start, duplicate_node);
                                }
                            }
                        }
                        /**
                         * Only consider end events that don't have a matching start node.
                        */
                        else if (search_event->command == CSEQ_COMMAND_BYTE_LOOP_END_WITH_META
                            && search_event->dual == NULL)
                        {
                            int32_t fixed_delta;
                            int32_t compare_delta;

                            any_end++;

                            fixed_delta = search_event->cseq_command_parameters[2];
                            compare_delta = (int32_t)(
                                search_event->file_offset
                                + search_event->cseq_delta_time.num_bytes
                                + search_event->cseq_command_parameters_raw_len
                                - event->file_offset
                                - event->cseq_delta_time.num_bytes
                                - event->cseq_command_parameters_raw_len);
                            if (fixed_delta == compare_delta)
                            {
                                event->dual = search_event;
                                search_event->dual = event;

                                match = 1;
                            }

                            // if the starting node didn't match, check to see if any others did
                            if (match == 0 && duplicate_start != NULL)
                            {
                                duplicate_node = duplicate_start->root;
                                while (duplicate_node != NULL && match == 0)
                                {
                                    duplicate_event = (struct GmidEvent *)duplicate_node->data;
                                    fixed_delta = search_event->cseq_command_parameters[2];
                                    compare_delta = (int32_t)(
                                        search_event->file_offset
                                        + search_event->cseq_delta_time.num_bytes
                                        + search_event->cseq_command_parameters_raw_len
                                        - duplicate_event->file_offset
                                        - duplicate_event->cseq_delta_time.num_bytes
                                        - duplicate_event->cseq_command_parameters_raw_len);
                                    if (fixed_delta == compare_delta)
                                    {
                                        duplicate_event->dual = search_event;
                                        search_event->dual = duplicate_event;

                                        match = 1;
                                    }
                                    
                                    duplicate_node = duplicate_node->next;
                                }
                            }
                        }
                    }

                    search_node = search_node->next;
                }

                if (duplicate_start != NULL)
                {
                    llist_node_root_free(duplicate_start);
                    duplicate_start = NULL;
                }

                if (match == 0)
                {
                    // This is a start loop event, and there is no end loop event in this track.
                    // In that case, flag this start event as bad and don't try to process.
                    // This happens on Archives channel 7.
                    if (any_end == 0)
                    {
                        event->flags |= MIDI_MALFORMED_EVENT_LOOP;
                    }
                    else
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "%s %d> can not find seq loop end event for loop start event at file offset %ld\n", __func__, __LINE__, event->file_offset);
                    }
                }
            }
        }

        node = node->next;
    }

    TRACE_LEAVE(__func__)
}

/**
 * This iterates the event list and each seq loop start/end event
 * is used to create a new standard MIDI, controller non-standard
 * loop/start/count event. These will be inserted in the correct
 * order, but delta times will be wrong.
 * @param gtrack: track to update.
*/
void GmidTrack_cseq_to_midi_loop(struct GmidTrack *gtrack)
{
    TRACE_ENTER(__func__)

    if (gtrack == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> gtrack is NULL\n", __func__, __LINE__);
    }

    struct llist_node *node;
    struct llist_node *new_node;
    struct llist_node *search_node;
    struct GmidEvent *seq_event;
    struct GmidEvent *seq_end_event;
    struct GmidEvent *midi_start_event;
    struct GmidEvent *midi_count_event;
    struct GmidEvent *midi_end_event;

    node = gtrack->events->root;
    while (node != NULL)
    {
        seq_event = (struct GmidEvent *)node->data;
        if (seq_event != NULL)
        {
            if (seq_event->command == CSEQ_COMMAND_BYTE_LOOP_START_WITH_META)
            {
                /**
                 * If this is seq meta start event without end event, it will only
                 * be possible to create a MIDI start event, not count or end.
                 * Set what's possible in the start event, then check
                 * whether that's the case or not.
                */
                int channel_unique_loop_number = seq_event->cseq_command_parameters[0];

                // create MIDI loop start event.
                midi_start_event = GmidEvent_new();
                midi_start_event->cseq_valid = 0;
                midi_start_event->midi_valid = 1;
                
                midi_start_event->command = MIDI_COMMAND_BYTE_CONTROL_CHANGE;
                midi_start_event->command_channel = seq_event->command_channel;

                midi_start_event->midi_command_len = MIDI_COMMAND_LEN_CONTROL_CHANGE;
                midi_start_event->midi_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_CONTROL_CHANGE;
                midi_start_event->midi_command_parameters[0] = MIDI_CONTROLLER_LOOP_START;
                midi_start_event->midi_command_parameters[1] = channel_unique_loop_number;
                midi_start_event->midi_command_parameters_len = MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
                midi_start_event->midi_command_parameters_raw[0] = MIDI_CONTROLLER_LOOP_START;
                midi_start_event->midi_command_parameters_raw[1] = channel_unique_loop_number;

                // start event without end event. Set time then insert and continue.
                if ((seq_event->flags & MIDI_MALFORMED_EVENT_LOOP) > 0)
                {
                    // set absolute times
                    midi_start_event->absolute_time = seq_event->absolute_time;

                    // insert in event list, before corresponding seq event.
                    new_node = llist_node_new();
                    new_node->data = midi_start_event;
                    llist_node_insert_before(gtrack->events, node, new_node);
                }
                else
                {
                    /**
                     * else, seq meta end loop event should exist. That means the MIDI count
                     * and MIDI end events can be created, and the dual pointers can be set.
                    */
                    seq_end_event = seq_event->dual;

                    if (seq_end_event == NULL)
                    {
                        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> seq_end_event is NULL\n", __func__, __LINE__);
                    }

                    int seq_loop_count = seq_end_event->cseq_command_parameters[0];

                    /**
                     * MIDI loop spec states there are two controllers, one for (seq) loop counts 0-127
                     * and the second for loop count (seq) 128-255. The MIDI value stored should
                     * be between 0-127, using the second controller will add 128 to the loop count.
                    */
                    int midi_controller_loop_type =
                        (seq_loop_count < 128)
                        ? MIDI_CONTROLLER_LOOP_COUNT_0
                        : MIDI_CONTROLLER_LOOP_COUNT_128;
                    int midi_loop_count = seq_loop_count;
                    while (midi_loop_count > 128)
                    {
                        midi_loop_count -= 128;
                    }

                    // create MIDI loop count event.
                    midi_count_event = GmidEvent_new();
                    midi_count_event->cseq_valid = 0;
                    midi_count_event->midi_valid = 1;
                    
                    midi_count_event->command = MIDI_COMMAND_BYTE_CONTROL_CHANGE;
                    midi_count_event->command_channel = seq_event->command_channel;

                    midi_count_event->midi_command_len = MIDI_COMMAND_LEN_CONTROL_CHANGE;
                    midi_count_event->midi_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_CONTROL_CHANGE;
                    midi_count_event->midi_command_parameters[0] = midi_controller_loop_type;
                    midi_count_event->midi_command_parameters[1] = midi_loop_count;
                    midi_count_event->midi_command_parameters_len = MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
                    midi_count_event->midi_command_parameters_raw[0] = midi_controller_loop_type;
                    midi_count_event->midi_command_parameters_raw[1] = midi_loop_count;

                    // create MIDI loop end event.
                    midi_end_event = GmidEvent_new();
                    midi_end_event->cseq_valid = 0;
                    midi_end_event->midi_valid = 1;
                    
                    midi_end_event->command = MIDI_COMMAND_BYTE_CONTROL_CHANGE;
                    midi_end_event->command_channel = seq_event->command_channel;

                    midi_end_event->midi_command_len = MIDI_COMMAND_LEN_CONTROL_CHANGE;
                    midi_end_event->midi_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_CONTROL_CHANGE;
                    midi_end_event->midi_command_parameters[0] = MIDI_CONTROLLER_LOOP_END;
                    midi_end_event->midi_command_parameters[1] = channel_unique_loop_number;
                    midi_end_event->midi_command_parameters_len = MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
                    midi_end_event->midi_command_parameters_raw[0] = MIDI_CONTROLLER_LOOP_END;
                    midi_end_event->midi_command_parameters_raw[1] = channel_unique_loop_number;

                    // connect dual pointers.
                    midi_start_event->dual = midi_end_event->dual;
                    midi_end_event->dual = midi_start_event->dual;

                    // set absolute times
                    midi_start_event->absolute_time = seq_event->absolute_time;
                    midi_count_event->absolute_time = seq_event->absolute_time; // same as start
                    midi_end_event->absolute_time = seq_end_event->absolute_time;

                    // insert in event list, before corresponding seq event.
                    // The order should be new start node, then new count node.
                    new_node = llist_node_new();
                    new_node->data = midi_start_event;
                    llist_node_insert_before(gtrack->events, node, new_node);
                    new_node = llist_node_new();
                    new_node->data = midi_count_event;
                    llist_node_insert_before(gtrack->events, node, new_node);

                    // unfortunately the node containing the seq end event
                    // isn't available, even though it's now known to exist.
                    // Track that down in order to insert before it.
                    search_node = node;
                    while (search_node != NULL)
                    {
                        if (search_node->data == seq_end_event)
                        {
                            break;
                        }
                        search_node = search_node->next;
                    }

                    if (search_node == NULL)
                    {
                        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> search_node is NULL\n", __func__, __LINE__);
                    }

                    new_node = llist_node_new();
                    new_node->data = midi_end_event;
                    llist_node_insert_before(gtrack->events, search_node, new_node);
                }
            }
        }

        node = node->next;
    }

    TRACE_LEAVE(__func__)
}

/**
 * This iterates the event list and sets the absolute time
 * markers based on the delta times of each event.
 * There are two separate running tallies, one for valid MIDI events
 * and one for valid seq events.
 * @param gtrack: track to update.
*/
void GmidTrack_absolute_from_delta(struct GmidTrack *gtrack)
{
    TRACE_ENTER(__func__)

    if (gtrack == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> gtrack is NULL\n", __func__, __LINE__);
    }

    struct llist_node *node;
    long absolute_time = 0;

    node = gtrack->events->root;
    while (node != NULL)
    {
        struct GmidEvent *current_node_event = (struct GmidEvent *)node->data;

        if (current_node_event->cseq_valid == 1 && current_node_event->midi_valid == 0)
        {
            absolute_time += current_node_event->cseq_delta_time.standard_value;
        }
        else
        {
            absolute_time += current_node_event->midi_delta_time.standard_value;
        }

        current_node_event->absolute_time = absolute_time;

        node = node->next;
    }

    TRACE_LEAVE(__func__)
}

/**
 * This iterates the event list and sets the delta times
 * based on the absolute time of each event.
 * There are two separate running tallies, one for valid MIDI events
 * and one for valid seq events.
 * @param gtrack: track to update.
*/
void GmidTrack_delta_from_absolute(struct GmidTrack *gtrack)
{
    TRACE_ENTER(__func__)

    if (gtrack == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> gtrack is NULL\n", __func__, __LINE__);
    }

    /**
     * Note: list must be sorted first!
    */

    struct llist_node *node;
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
            
            // make sure midi delta stays synced
            memcpy(&current_node_event->midi_delta_time, &current_node_event->cseq_delta_time, sizeof(struct var_length_int));
        }

        // set the event MIDI event time
        if (current_node_event->midi_valid)
        {
            time_delta = (int)(current_node_event->absolute_time - midi_prev_absolute_time);
            int32_to_varint(time_delta, &current_node_event->midi_delta_time);
            midi_prev_absolute_time = current_node_event->absolute_time;
            // make sure cseq delta stays synced
            memcpy(&current_node_event->cseq_delta_time, &current_node_event->midi_delta_time, sizeof(struct var_length_int));
        }

        node = node->next;
    }

    TRACE_LEAVE(__func__)
}

/**
 * This iterates the event in a track and creates MIDI note-off events
 * from the seq note-on events (actually creates running status note-on).
 * These events will have correct absolute time, but the delta event
 * times for the track will be incorrect after calling this method.
 * @param gtrack: track to update.
*/
void GmidTrack_midi_note_off_from_cseq(struct GmidTrack *gtrack)
{
    TRACE_ENTER(__func__)

    if (gtrack == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> gtrack is NULL\n", __func__, __LINE__);
    }

    struct llist_node *node;
    struct GmidEvent *event;

    node = gtrack->events->root;
    while (node != NULL)
    {
        event = (struct GmidEvent *)node->data;

        if (event->command == MIDI_COMMAND_BYTE_NOTE_ON)
        {
            struct GmidEvent *noteoff = GmidEvent_new();
            struct llist_node *temp_node;

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
        }

        node = node->next;
    }

    TRACE_LEAVE(__func__)
}

/**
 * This iterates the event in a track and updates note-on events
 * to set the duration used by seq note-on.
 * @param gtrack: track to update.
*/
void GmidTrack_cseq_note_on_from_midi(struct GmidTrack *gtrack)
{
    TRACE_ENTER(__func__)

    if (gtrack == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> gtrack is NULL\n", __func__, __LINE__);
    }

    struct GmidEvent *event;
    struct GmidEvent *event_off;
    struct llist_node *node;
    struct llist_node *node_off;
    struct var_length_int varint;

    node = gtrack->events->root;
    while (node != NULL)
    {
        event = (struct GmidEvent *)node->data;
        if (event != NULL)
        {
            // look for note-on events. Zero velocity is implicit note-off.
            if (event->command == MIDI_COMMAND_BYTE_NOTE_ON
                && event->midi_command_parameters[1] > 0)
            {
                int match = 0;

                node_off = node;
                while (node_off != NULL && match == 0)
                {
                    event_off = (struct GmidEvent *)node_off->data;
                    if (event_off != NULL)
                    {
                        if (
                            // this is note off event, or implicit note-off event
                            (
                                (event_off->command == MIDI_COMMAND_BYTE_NOTE_OFF)
                                || (event_off->command == MIDI_COMMAND_BYTE_NOTE_ON && event_off->midi_command_parameters[1] == 0)
                            )
                            // and this is for the same note
                            && event_off->midi_command_parameters[0] == event->midi_command_parameters[0]
                        )
                        {
                            long absolute_delta = event_off->absolute_time - event->absolute_time;
                            memset(&varint, 0, sizeof(struct var_length_int));
                            int32_to_varint((int32_t)absolute_delta, &varint);

                            // set "easy access" value for duration.
                            // Offset 0 and 1 are note and velocity, so start at 2.
                            event->cseq_command_parameters[2] = (int32_t)absolute_delta;
                            // write duration into cseq parameters.
                            memcpy(&event->cseq_command_parameters_raw[2], &varint.value, varint.num_bytes);
                            // update byte length of parameters to write.
                            event->cseq_command_parameters_raw_len += varint.num_bytes;
                            // flag cseq now as valid.
                            event->cseq_valid = 1;
                            // flag the note off event as captured. This will prevent writing to output file.
                            event_off->flags |= MIDI_SEQ_EVENT_NOTE_OFF_HANDLED;

                            match = 1;
                        }
                    }

                    node_off = node_off->next;
                }

                if (match == 0)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> could not find note-off event for note-on. MIDI track %d, absolute_time %ld\n", __func__, __LINE__, gtrack->midi_track_index, event->absolute_time);
                }
            }
        }

        node = node->next;
    }    

    TRACE_LEAVE(__func__)
}

/**
 * This parses a compressed MIDI track from the internal raw data buffer {@code cseq_data}
 * and converts to {@code struct GmidEvent}, storing the events
 * internally. This allocates memory. This method can't parse pattern markers,
 * {@code CseqFile_unroll} must have been called previously.
 * @param gtrack: track to convert to events.
*/
void GmidTrack_parse_CseqTrack(struct GmidTrack *gtrack)
{
    TRACE_ENTER(__func__)

    if (gtrack == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> gtrack is NULL\n", __func__, __LINE__);
    }

    struct GmidEvent *event;
    struct llist_node *node;
    size_t track_pos;
    size_t buffer_len;
    int32_t command;

    /** running count of absolute time.
    * Every delta event read is added to this value.
    * The first delta event read sets the very first absolute time (not necessarily zero).
    */
    long absolute_time = 0;

    command = 0;
    track_pos = 0;
    buffer_len = gtrack->cseq_data_len;

    while (track_pos < buffer_len)
    {
        int bytes_read = 0;
        
        // track position will be updated in the method call according to how many bytes read.
        event = GmidEvent_new_from_buffer(gtrack->cseq_data, &track_pos, buffer_len, MIDI_IMPLEMENTATION_SEQ, command, &bytes_read);

        if (event == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: event is NULL\n", __func__, __LINE__);
        }

        if (bytes_read == 0)
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d>: invalid bytes_read (0) from GmidEvent_new_from_buffer\n", __func__, __LINE__);
        }

        absolute_time += event->midi_delta_time.standard_value;
        event->absolute_time = absolute_time;

        command = GmidEvent_get_midi_command(event);

        // only allow running status for the "regular" MIDI commands
        if ((command & 0xffffff00) != 0 || (command & 0xffffffff) == 0xff)
        {
            command = 0;
        }

        node = llist_node_new();
        node->data = event;
        // The command channel in the event specifies the track number, so
        // add the new event to the appropriate track linked list.
        llist_root_append_node(gtrack->events, node);
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
        case MIDI_CONTROLLER_BANK_SELECT    : snprintf(result, max_length, "bank select"); break;
        case MIDI_CONTROLLER_CHANNEL_VOLUME : snprintf(result, max_length, "channel volume"); break;
        case MIDI_CONTROLLER_CHANNEL_BALANCE: snprintf(result, max_length, "balance"); break;
        case MIDI_CONTROLLER_CHANNEL_PAN    : snprintf(result, max_length, "pan"); break;
        case MIDI_CONTROLLER_CHANNEL_PAN2   : snprintf(result, max_length, "pan"); break;
        case MIDI_CONTROLLER_SUSTAIN        : snprintf(result, max_length, "sustain"); break;
        case MIDI_CONTROLLER_EFFECTS_1_DEPTH: snprintf(result, max_length, "effects 1 depth"); break;
        case MIDI_CONTROLLER_EFFECTS_2_DEPTH: snprintf(result, max_length, "effects 2 depth"); break;
        case MIDI_CONTROLLER_EFFECTS_3_DEPTH: snprintf(result, max_length, "effects 3 depth"); break;
        case MIDI_CONTROLLER_EFFECTS_4_DEPTH: snprintf(result, max_length, "effects 4 depth"); break;
        case MIDI_CONTROLLER_EFFECTS_5_DEPTH: snprintf(result, max_length, "effects 5 depth"); break;
        case MIDI_CONTROLLER_LOOP_START     : snprintf(result, max_length, "MIDI loop start (non standard)"); break;
        case MIDI_CONTROLLER_LOOP_END       : snprintf(result, max_length, "MIDI loop end (non standard)"); break;
        case MIDI_CONTROLLER_LOOP_COUNT_0   : snprintf(result, max_length, "MIDI loop count +0 (non standard)"); break;
        case MIDI_CONTROLLER_LOOP_COUNT_128 : snprintf(result, max_length, "MIDI loop count +128 (non standard)"); break;
        
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

    if (gtrack == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> gtrack is NULL\n", __func__, __LINE__);
    }

    if (buffer == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> buffer is NULL\n", __func__, __LINE__);
    }

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

        if (event->midi_delta_time.num_bytes == 0)
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> invalid midi delta time (0)\n", __func__, __LINE__);
        }

        // write varint delta time value
        memset(rev, 0, 4);
        memcpy(rev, &event->midi_delta_time.value, event->midi_delta_time.num_bytes);
        // byte swap
        reverse_inplace(rev, event->midi_delta_time.num_bytes);
        memcpy(&buffer[write_len], rev, event->midi_delta_time.num_bytes);
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
 * Iterates {@code struct GmidTrack} events list and writes to out buffer
 * in seq format. This is writing the track data without the track header.
 * Events list must have been previously populated.
 * No pattern substition (compression) occurs.
 * @param gtrack: track to convert.
 * @param buffer: buffer to write to. Must be previously allocated.
 * @param max_len: size in bytes of the buffer.
 * @returns: number of bytes written to buffer.
*/
size_t GmidTrack_write_to_cseq_buffer(struct GmidTrack *gtrack, uint8_t *buffer, size_t max_len)
{
    TRACE_ENTER(__func__)

    if (gtrack == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> gtrack is NULL\n", __func__, __LINE__);
    }

    if (buffer == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> buffer is NULL\n", __func__, __LINE__);
    }

    size_t write_len = 0;
    int32_t previous_command = 0;
    struct llist_node *node = gtrack->events->root;
    uint8_t rev[4];
    int32_t command;

    for (; node != NULL && write_len < max_len; node = node->next)
    {
        struct GmidEvent *event = (struct GmidEvent *)node->data;

        if (!event->cseq_valid)
        {
            continue;
        }

        command = GmidEvent_get_midi_command(event);

        if (event->cseq_delta_time.num_bytes == 0)
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> invalid midi delta time (0)\n", __func__, __LINE__);
        }

        // write varint delta time value
        memset(rev, 0, 4);
        memcpy(rev, &event->cseq_delta_time.value, event->cseq_delta_time.num_bytes);
        // byte swap
        reverse_inplace(rev, event->cseq_delta_time.num_bytes);
        memcpy(&buffer[write_len], rev, event->cseq_delta_time.num_bytes);
        write_len += event->cseq_delta_time.num_bytes;

        // if this is a "running status" then no need to write command, otherwise write command bytes
        if (previous_command != command)
        {
            memset(rev, 0, 4);
            memcpy(rev, &command, event->cseq_command_len);
            // byte swap
            reverse_inplace(rev, event->cseq_command_len);
            memcpy(&buffer[write_len], rev, event->cseq_command_len);
            write_len += event->cseq_command_len;
            previous_command = command;
        }

        // only allow running status for the "regular" MIDI commands
        if ((command & 0xffffff00) != 0 || (command & 0xffffffff) == 0xff)
        {
            previous_command = 0;
        }

        // write command parameters
        if (event->cseq_command_parameters_raw_len > 0)
        {
            memcpy(&buffer[write_len], event->cseq_command_parameters_raw, event->cseq_command_parameters_raw_len);
            write_len += event->cseq_command_parameters_raw_len;
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
 * Writes the full {@code struct MidiFile} to disk, and all child elements.
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

/**
 * Writes the full {@code struct CseqFile} to disk, and all child elements.
 * @param cseq: Compressed N64 MIDI to write.
 * @param fi: File handle to write to, using current offset.
*/
void CseqFile_fwrite(struct CseqFile *cseq, struct file_info *fi)
{
    TRACE_ENTER(__func__)

    int i;
    for (i=0; i<CSEQ_FILE_NUM_TRACKS; i++)
    {
        file_info_fwrite_bswap(fi, &cseq->track_offset[i], 4, 1);
    }

    file_info_fwrite_bswap(fi, &cseq->division, 4, 1);
    file_info_fwrite(fi, cseq->compressed_data, cseq->compressed_data_len, 1);

    TRACE_LEAVE(__func__)
}

static struct seq_unroll_grow *seq_unroll_grow_new(void)
{
    TRACE_ENTER(__func__)
    struct seq_unroll_grow *result = (struct seq_unroll_grow *)malloc_zero(1, sizeof(struct seq_unroll_grow));
    TRACE_LEAVE(__func__)
    return result;
}

static void seq_unroll_grow_free(struct seq_unroll_grow *obj)
{
    TRACE_ENTER(__func__)
    if (obj != NULL)
    {
        free(obj);
    }
    TRACE_LEAVE(__func__)
}