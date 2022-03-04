#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "debug.h"
#include "common.h"
#include "machine_config.h"
#include "utility.h"
#include "midi.h"
#include "parse.h"

/**
 * This file contains code for the N64 compressed MIDI format,
 * and regular MIDI format.
*/

int g_midi_parse_debug = 0;
int g_midi_debug_loop_delta = 0;

// Configuration flag, when set cseq will throw if start/end event don't match.
// There are several malformed cseq tracks in the release version of goldeneye, so
// this should be disabled when working with game files.
int g_strict_cseq_loop_event = 0;

static const int g_min_pattern_length = 6;        // according to guess
static const int g_max_pattern_length = 0xff;     // according to programmer manual
static const int g_max_pattern_distance = 0xfdff; // according to programmer manual

#define NUM_BUFFER_SIZE 12

// give every event a unique id
static int g_event_id = 0;

enum CSEQ_PATTERN_TYPE {
    // unroll pattern sequence. 4 bytes compressed.
    CSEQ_PATTERN_UNROLL = 0,

    // escape pattern sequence. 2 bytes 0xfefe to 1 byte 0xfe.
    CSEQ_PATTERN_ESCAPE
};

/**
 * Describes where a pattern marker should be placed
 * when applying pattern substitution on a seq file.
*/
struct SeqPatternMatch {
    // cseq_track_index (not midi track number)
    int track_number;
    
    /**
     * Byte offset from beginning of track the pattern begins at.
     * All such start positions are before any pattern
     * unrolling occurs.
    */
    int start_pattern_pos;
    
    /**
     * Difference in bytes between the end of the seq loop end event
     * and the end of the seq loop start event.
     * Inclusively, this is first byte of event after start event
     * (i.e., delta time) to the last byte of the loop end delta value.
    */
    int diff;
    
    // length of pattern to substitute, in bytes
    int pattern_length;

    enum CSEQ_PATTERN_TYPE type;
};

// forward declarations

int LinkedListNode_gmidevent_compare_larger(struct LinkedListNode *first, struct LinkedListNode *second);
int LinkedListNode_gmidevent_compare_smaller(struct LinkedListNode *first, struct LinkedListNode *second);
int LinkedListNode_SeqPatternMatch_compare_smaller(struct LinkedListNode *first, struct LinkedListNode *second);
int LinkedListNode_SeqPatternMatch_is_unroll(struct LinkedListNode *node);
static int measure_unroll_adjust(struct LinkedList *patterns, int start_pos, int end_pos);
static struct SeqPatternMatch *SeqPatternMatch_new_values(int start_pattern_pos, int diff, int pattern_length);
static struct SeqPatternMatch *SeqPatternMatch_new(void);
static void SeqPatternMatch_free(struct SeqPatternMatch *obj);
static void GmidTrack_debug_print(struct GmidTrack *track, enum MIDI_IMPLEMENTATION type);
static void GmidTrack_print(struct GmidTrack *track, enum MIDI_IMPLEMENTATION type);

// end forward declarations

/**
 * Merge sort comparison function.
 * Use this to sort largest to smallest.
 * @param first: first node
 * @param second: second node
 * @returns: comparison result
*/
int LinkedListNode_gmidevent_compare_larger(struct LinkedListNode *first, struct LinkedListNode *second)
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
int LinkedListNode_gmidevent_compare_smaller(struct LinkedListNode *first, struct LinkedListNode *second)
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
 * Merge sort comparison function.
 * Compares {@code struct SeqPatternMatch.start_pattern_pos}.
 * Use this to sort smallest to largest.
 * @param first: first node
 * @param second: second node
 * @returns: comparison result
*/
int LinkedListNode_SeqPatternMatch_compare_smaller(struct LinkedListNode *first, struct LinkedListNode *second)
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
        struct SeqPatternMatch *pattern_first = (struct SeqPatternMatch *)first->data;
        struct SeqPatternMatch *pattern_second = (struct SeqPatternMatch *)second->data;
       
        if (pattern_first == NULL && pattern_second == NULL)
        {
            ret = 0;
        }
        else if (pattern_first == NULL && pattern_second != NULL)
        {
            ret = 1;
        }
        else if (pattern_first != NULL && pattern_second == NULL)
        {
            ret = -1;
        }
        else
        {
            if (pattern_first->start_pattern_pos < pattern_second->start_pattern_pos)
            {
                ret = -1;
            }
            else if (pattern_first->start_pattern_pos > pattern_second->start_pattern_pos)
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
 * Linked list callback to check if node is type {@code CSEQ_PATTERN_UNROLL}.
 * @param node: node of type {@code struct SeqPatternMatch}.
 * @returns: 1 if match, zero otherwise.
*/
int LinkedListNode_SeqPatternMatch_is_unroll(struct LinkedListNode *node)
{
    TRACE_ENTER(__func__)

    int ret = 0;

    if (node != NULL)
    {
        struct SeqPatternMatch *pattern = (struct SeqPatternMatch *)node;
        if (pattern != NULL)
        {
            return pattern->type == CSEQ_PATTERN_UNROLL;
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
struct MidiFile *MidiFile_new_from_file(struct FileInfo *fi)
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
    FileInfo_fseek(fi, 0, SEEK_SET);

    // root chunk id
    FileInfo_fread(fi, &t32, 4, 1);
    file_pos += 4;
    BSWAP32(t32);
    if (t32 != MIDI_ROOT_CHUNK_ID)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> invalid MIDI root chunk id. Expected 0x%08x, actual 0x%08x.\n", __func__, __LINE__, MIDI_ROOT_CHUNK_ID, t32);
    }

    // root chunk size
    FileInfo_fread(fi, &t32, 4, 1);
    file_pos += 4;
    BSWAP32(t32);
    if (t32 != MIDI_ROOT_CHUNK_BODY_SIZE)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> invalid MIDI root chunk size. Expected 0x%08x, actual 0x%08x.\n", __func__, __LINE__, MIDI_ROOT_CHUNK_BODY_SIZE, t32);
    }

    // format
    FileInfo_fread(fi, &p->format, 2, 1);
    file_pos += 2;
    BSWAP16(p->format);
    if (p->format != MIDI_FORMAT_SIMULTANEOUS)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> Unsupported MIDI format %d. Only %d is supported.\n", __func__, __LINE__, p->format, MIDI_FORMAT_SIMULTANEOUS);
    }

    // number of tracks
    FileInfo_fread(fi, &p->num_tracks, 2, 1);
    file_pos += 2;
    BSWAP16(p->num_tracks);
    if (p->num_tracks > CSEQ_FILE_NUM_TRACKS)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> Only %d tracks supported, but MIDI file has %d\n", __func__, __LINE__, p->format, CSEQ_FILE_NUM_TRACKS, p->num_tracks);
    }

    // ticks per quarter note (when positive)
    FileInfo_fread(fi, &p->division, 2, 1);
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

        FileInfo_fread(fi, &t32, 4, 1);
        file_pos += 4;
        BSWAP32(t32);
        if (t32 != MIDI_TRACK_CHUNK_ID)
        {
            skip = 1;
        }

        FileInfo_fread(fi, &track_size, 4, 1);
        file_pos += 4;
        BSWAP32(track_size);

        if (skip)
        {
            file_pos += track_size;
            if (file_pos < fi->len)
            {
                FileInfo_fseek(fi, (long)track_size, SEEK_CUR);
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

        FileInfo_fread(fi, track->data, track_size, 1);
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
struct CseqFile *CseqFile_new_from_file(struct FileInfo *fi)
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
    FileInfo_fseek(fi, 0, SEEK_SET);

    for (i=0; i<CSEQ_FILE_NUM_TRACKS; i++)
    {
        FileInfo_fread(fi, &p->track_offset[i], 4, 1);
        BSWAP32(p->track_offset[i]);
    }

    FileInfo_fread(fi, &p->division, 4, 1);
    BSWAP32(p->division);

    data_len = fi->len - CSEQ_FILE_HEADER_SIZE_BYTES;

    p->compressed_data = (uint8_t *)malloc_zero(1, data_len);
    FileInfo_fread(fi, p->compressed_data, data_len, 1);

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
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> non_empty_num_tracks %d exceeds %d.\n", __func__, __LINE__, p->non_empty_num_tracks, CSEQ_FILE_NUM_TRACKS);
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

    if (gtrack == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> gtrack is NULL\n", __func__, __LINE__);
    }

    struct MidiTrack *p = (struct MidiTrack *)malloc_zero(1, sizeof(struct MidiTrack));

    p->ck_id = MIDI_TRACK_CHUNK_ID;
    p->track_index = gtrack->midi_track_index;

    p->data = (uint8_t *)malloc_zero(1, gtrack->midi_track_size_bytes);
    p->ck_data_size = GmidTrack_write_to_midi_buffer(gtrack, p->data, gtrack->midi_track_size_bytes);

    TRACE_LEAVE(__func__)

    return p;
}

struct MidiFile *MidiFile_new_from_gmid(struct GmidFile *gmid_file)
{
    TRACE_ENTER(__func__)

    if (gmid_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: gmid_file is NULL\n", __func__, __LINE__);
    }

    int i;
    int non_empty_tracks = 0;
    int allocated_tracks = 0;

    for (i=0; i<gmid_file->num_tracks; i++)
    {
        if (gmid_file->tracks[i] != NULL
            && gmid_file->tracks[i]->events != NULL
            && gmid_file->tracks[i]->events->count > 0)
        {
            non_empty_tracks++;
        }
    }

    if (non_empty_tracks == 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d>: error, no tracks found in gmid_file\n", __func__, __LINE__);
    }

    struct MidiFile *midi = MidiFile_new_tracks(MIDI_FORMAT_SIMULTANEOUS, non_empty_tracks);

    for (i=0; i<CSEQ_FILE_NUM_TRACKS; i++)
    {
        if (gmid_file->tracks[i] != NULL
            && gmid_file->tracks[i]->events != NULL
            && gmid_file->tracks[i]->events->count > 0)
        {
            midi->tracks[allocated_tracks] = MidiTrack_new_from_GmidTrack(gmid_file->tracks[i]);
            allocated_tracks++;
        }
    }

    TRACE_LEAVE(__func__)
    return midi;
}

struct GmidFile *GmidFile_new_from_midi(struct MidiFile *midi, int force_track)
{
    TRACE_ENTER(__func__)

    if (midi == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: midi is NULL\n", __func__, __LINE__);
    }

    struct GmidFile *gmid_file;
    struct LinkedList *track_event_holder;
    int source_track_index;
    char *debug_printf_buffer;
    struct LinkedListNode *node;

    debug_printf_buffer = (char *)malloc_zero(1, WRITE_BUFFER_LEN);

    gmid_file = GmidFile_new();

    // collect events seen while parsing the current track.
    track_event_holder = LinkedList_new();

    for (source_track_index=0; source_track_index<midi->num_tracks; source_track_index++)
    {
        struct MidiTrack *source_track = midi->tracks[source_track_index];
        struct GmidEvent *event;
        size_t track_pos;
        size_t buffer_len;
        int32_t command;
        int destination_track = -1;

        /** 
         * running count of absolute time.
         * Every delta event read is added to this value.
         * The first delta event read sets the very first absolute time (not necessarily zero).
        */
        long absolute_time = 0;

        if (g_verbosity >= VERBOSE_DEBUG)
        {
            printf("\n");
            printf("begin parse track %d\n", source_track_index);
        }

        if (source_track == NULL)
        {
            if (g_verbosity >= VERBOSE_DEBUG)
            {
                printf("empty track\n");
            }

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

            absolute_time += event->midi_delta_time.standard_value;
            event->absolute_time = absolute_time;

            if (g_verbosity >= VERBOSE_DEBUG)
            {
                memset(debug_printf_buffer, 0, WRITE_BUFFER_LEN);
                size_t debug_str_len = GmidEvent_to_string(event, debug_printf_buffer, WRITE_BUFFER_LEN - 2, MIDI_IMPLEMENTATION_STANDARD);
                debug_printf_buffer[debug_str_len] = '\n';
                fflush_string(stdout, debug_printf_buffer);
            }

            if (force_track)
            {
                destination_track = source_track_index;
            }
            else
            {
                // Set destination track to first seen command channel.
                if (destination_track == -1 && event->command_channel > -1)
                {
                    destination_track = event->command_channel;
                }
            }

            command = GmidEvent_get_midi_command(event);

            // only allow running status for the "regular" MIDI commands (commands with channels)
            if ((command & 0xffffff00) != 0 || (command & 0xfffffff0) >= 0xf0)
            {
                command = 0;
            }

            node = LinkedListNode_new();
            node->data = event;
            LinkedList_append_node(track_event_holder, node);
        }

        if (g_verbosity >= VERBOSE_DEBUG)
        {
            printf("finish parse track %d\n", source_track_index);
        }

        if (destination_track == -1)
        {
            fflush_printf(stderr, "destination track not resolved, assuming midi track %d\n", source_track_index);
            destination_track = source_track_index;
        }

        if (destination_track > CSEQ_FILE_NUM_TRACKS)
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d>: invalid destination_track %d resolved from nth midi track %d\n", __func__, __LINE__, destination_track, source_track_index);
        }

        gmid_file->tracks[destination_track]->midi_track_index = source_track_index;
        gmid_file->tracks[destination_track]->cseq_track_index = destination_track;

        // now that destination track is known, move from temp list
        // to correct track.
        while (track_event_holder->count > 0)
        {
            node = track_event_holder->head;
            LinkedListNode_move(gmid_file->tracks[destination_track]->events, track_event_holder, node);
        }

        // estimate total track size in bytes.
        GmidTrack_set_track_size_bytes(gmid_file->tracks[destination_track]);
    }

    free(debug_printf_buffer);

    TRACE_LEAVE(__func__)
    return gmid_file;
}

/**
 * Processes a N64 seq file loaded into memory and converts to regular MIDI format.
 * This allocates memory for the new MIDI file.
 * @param cseq: compressed MIDI format track to convert.
 * @param options: conversion options.
 * @returns: pointer to new MidiFile.
*/
struct MidiFile *MidiFile_from_CseqFile(struct CseqFile *cseq, struct MidiConvertOptions *options)
{
    TRACE_ENTER(__func__)

    if (cseq == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: cseq is NULL\n", __func__, __LINE__);
    }

    int i;
    int allocated_tracks = 0;
    struct FileInfo *pattern_file = NULL;
    f_GmidTrack_callback post_unroll_action = NULL;
    int opt_pattern_substitution = 1; // enable by default
    int no_create_sysex = 1; // do not create by default

    if (options != NULL)
    {
        post_unroll_action = options->post_unroll_action;
        opt_pattern_substitution = !options->no_pattern_compression; // negate
        no_create_sysex = !options->sysex_seq_loops; // negate
    }

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
        struct LinkedList *patterns = NULL;
        struct LinkedListNode *pattern_node = NULL;
        struct SeqPatternMatch *pattern = NULL;

        gtrack->midi_track_index = allocated_tracks;
        gtrack->cseq_track_index = i;

        if (opt_pattern_substitution)
        {
            patterns = LinkedList_new();
            CseqFile_unroll(cseq, gtrack, patterns);

            // only write patterns if user specified.
            if (options != NULL && options->use_pattern_marker_file)
            {
                // Only write patterns (and create file) if there's anything to write.
                if (LinkedList_any(patterns, LinkedListNode_SeqPatternMatch_is_unroll))
                {
                    // only open file once, then preserve reference for future tracks.
                    if (pattern_file == NULL)
                    {
                        // seq->midi is write
                        pattern_file = FileInfo_fopen(options->pattern_marker_filename, "wb");
                    }

                    write_patterns_to_file(patterns, pattern_file);
                }
            }
        }
        else
        {
            CseqFile_no_unroll_copy(cseq, gtrack);
        }

        // parse seq format into common events
        GmidTrack_parse_CseqTrack(gtrack);

        if (post_unroll_action != NULL)
        {
            post_unroll_action(gtrack);
        }

        // The seq loop end event contains the loop iteration count, but that's needed in the MIDI
        // loop start event, so resolve all dual pointers to make that available. One complication
        // is that pattern substitution is applied before loop end offset is calculated, so
        // unrolling above means the loop end offsets are now wrong. Pass in the list of patterns
        // found unrolling the track to this method to adjust loop end offsets.
        // This needs to happen before any nodes are added, and before
        // delta events are changed in order to calculate offsets correctly.
        GmidTrack_pair_cseq_loop_events(gtrack, patterns);

        // Mark invalid seq loops as invalid.
        // Create MIDI sysex events (custom events) for invalid seq loop events according to user option.
        // This needs to happen after GmidTrack_pair_cseq_loop_events.
        GmidTrack_invalid_cseq_loop_to_sysex(gtrack, no_create_sysex);

        // Create midi note off events from the seq note-on events.
        // This will break delta events for the track.
        GmidTrack_midi_note_off_from_cseq(gtrack);

        // Sort events by absolute time. This is needed because
        // the applied duration from the note-on can move
        // the new note-off event substantially.
        LinkedList_merge_sort(gtrack->events, LinkedListNode_gmidevent_compare_smaller);

        // Fix delta times, due to adding note-off events.
        GmidTrack_delta_from_absolute(gtrack);

        // Convert cseq loop markers into non-standard controller events
        GmidTrack_cseq_to_midi_loop(gtrack);

        // Fix delta times, due to adding MIDI controller events for looping.
        GmidTrack_delta_from_absolute(gtrack);

        if (g_verbosity >= VERBOSE_DEBUG)
        {
            GmidTrack_debug_print(gtrack, MIDI_IMPLEMENTATION_STANDARD);
        }

        // estimate total track size in bytes.
        GmidTrack_set_track_size_bytes(gtrack);

        midi->tracks[allocated_tracks] = MidiTrack_new_from_GmidTrack(gtrack);
        allocated_tracks++;

        // cleanup
        GmidTrack_free(gtrack);

        if (patterns != NULL)
        {
            pattern_node = patterns->head;
            while (pattern_node != NULL)
            {
                pattern = (struct SeqPatternMatch *)pattern_node->data;
                if (pattern != NULL)
                {
                    SeqPatternMatch_free(pattern);
                    pattern_node->data = NULL;
                }
                pattern_node = pattern_node->next;
            }
            
            LinkedList_free(patterns);
        }
    }

    if (pattern_file != NULL)
    {
        FileInfo_free(pattern_file);
        pattern_file = NULL;
    }

    return midi;

    TRACE_LEAVE(__func__)
}

/**
 * Processes regular MIDI loaded into memory and converts to N64 compressed MIDI format.
 * This allocates memory for the new cseq file.
 * @param midi: Standard MIDI file to convert.
 * @param options: conversion options.
 * @returns: pointer to new cseq file.
*/
struct CseqFile *CseqFile_from_MidiFile(struct MidiFile *midi, struct MidiConvertOptions *options)
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
    struct GmidFile *gmid_file;
    int i;
    char *debug_printf_buffer;
    uint8_t *cseq_data_buffer = NULL;
    size_t cseq_buffer_pos = 0;
    size_t cseq_buffer_size = 0;
    struct FileInfo *pattern_file = NULL;
    int opt_pattern_substitution = 1; // enable by default

    if (options != NULL)
    {
        opt_pattern_substitution = !options->no_pattern_compression; // negate

        if (options->use_pattern_marker_file)
        {
            // midi->cseq is read
            pattern_file = FileInfo_fopen(options->pattern_marker_filename, "rb");
            options->runtime_pattern_file = pattern_file;
        }
    }

    debug_printf_buffer = (char *)malloc_zero(1, WRITE_BUFFER_LEN);

    gmid_file = GmidFile_new_from_midi(midi, 0);

    // All events from source midi have been added to appropriate tracks.
    // Now process each track and convert from midi to seq format.
    for (i=0; i<CSEQ_FILE_NUM_TRACKS; i++)
    {
        size_t write_len;

        // If no events were added to this track above, then don't processes
        // this track.
        if (gmid_file->tracks[i] == NULL
            || gmid_file->tracks[i]->events == NULL
            || gmid_file->tracks[i]->events->count == 0)
        {
            continue;
        }

        // Transform MIDI sysex escaped cseq event into real cseq event.
        GmidTrack_midi_sysex_to_cseq(gmid_file->tracks[i]);

        // Convert note-off and implicit note-off to cseq format.
        // Absolute times must be accurate in order to calculate duration.
        GmidTrack_cseq_note_on_from_midi(gmid_file->tracks[i]);

        // Sort events by absolute time
        LinkedList_merge_sort(gmid_file->tracks[i]->events, LinkedListNode_gmidevent_compare_smaller);

        GmidTrack_delta_from_absolute(gmid_file->tracks[i]);

        /**
         * cseq2midi needed loop end to be linked to loop start to resolve
         * the loop count, but the other direction can just check that
         * the loop count event follows the loop start event (this is how gaudio converts).
         * This should be done last because the byte offset from the end to the
         * start node needs to be set, and adding additional nodes will break
         * the count.
        */
        GmidTrack_midi_to_cseq_loop(gmid_file->tracks[i]);

        // fix delta times for loop events just added.
        GmidTrack_delta_from_absolute(gmid_file->tracks[i]);

        // Now that delta events are correct, fix seq loop end events.
        // This calculates a byte offset to the start loop event, which
        // requires delta events be correct.
        GmidTrack_seq_fix_loop_end_delta(gmid_file->tracks[i]);

        GmidTrack_set_track_size_bytes(gmid_file->tracks[i]);

        // allocate memory for compressed data, add a margin of error
        gmid_file->tracks[i]->cseq_data = (uint8_t *)malloc_zero(1, gmid_file->tracks[i]->cseq_track_size_bytes + 50);
        
        // extract seq events to byte values and write to buffer
        write_len = GmidTrack_write_to_cseq_buffer(gmid_file->tracks[i], gmid_file->tracks[i]->cseq_data, gmid_file->tracks[i]->cseq_track_size_bytes + 50);
        gmid_file->tracks[i]->cseq_data_len = write_len;

        /**
         * The pattern substitution algorithm can match byte patterns from previous
         * tracks. Therefore the entire cseq written so far needs to be available
         * to this method. This is a little awkward because the data is essentially
         * duplicated, once for the individual track->cseq_data container, and
         * again in the entire cseq_data_buffer.
         * 
         * If there is no pattern matching then
         * 1) there's no need to allocate memory for the entire compressed file.
         * 2) gmid_file->tracks[i]->cseq_data already contains unprocessed data so
         *    there's nothing to do.
        */
        if (opt_pattern_substitution)
        {
            if (cseq_data_buffer == NULL)
            {
                cseq_buffer_size = gmid_file->tracks[i]->cseq_data_len;
                cseq_data_buffer = (uint8_t *)malloc_zero(1, cseq_buffer_size);
            }
            else
            {
                size_t new_size = cseq_buffer_size + write_len;
                malloc_resize(cseq_buffer_size, (void**)&cseq_data_buffer, new_size);
                cseq_buffer_size = new_size;
            }

            /**
             * This will apply pattern substitution and overwrite gmid_file->tracks[i]->cseq_data,
             * but also append the same data to cseq_data_buffer.
            */
            GmidTrack_roll_entry(gmid_file->tracks[i], cseq_data_buffer, &cseq_buffer_pos, cseq_buffer_size, options);
        }
    }

    // This combines all individual track->cseq_data into one file.
    result = CseqFile_new_from_tracks(gmid_file->tracks, CSEQ_FILE_NUM_TRACKS);

    // copy division from source
    result->division = midi->division;

    // cleanup.
    if (pattern_file != NULL)
    {
        FileInfo_free(pattern_file);
        pattern_file = NULL;

        if (options != NULL)
        {
            options->runtime_pattern_file = NULL;
        }
    }

    GmidFile_free(gmid_file);

    free(debug_printf_buffer);

    if (cseq_data_buffer != NULL)
    {
        free(cseq_data_buffer);
    }

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
    p->events = LinkedList_new();

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
        struct LinkedListNode *node;

        node = track->events->head;

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

        LinkedList_free(track->events);
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
 * @param patterns: Optional. If not null, will append pattern marker
 * sequences encountered while unrolling track.
*/
void CseqFile_unroll(struct CseqFile *cseq, struct GmidTrack *track, struct LinkedList *patterns)
{
    TRACE_ENTER(__func__)

    if (cseq == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> cseq is null\n", __func__, __LINE__);
    }

    if (track == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> track is null\n", __func__, __LINE__);
    }

    size_t pos = 0;
    size_t unrolled_pos = 0;
    size_t cseq_len = 0;
    uint8_t *temp_ptr;
    size_t compressed_read_len = 0;
    size_t total_pattern_unroll = 0;

    struct LinkedListNode *node;
    struct SeqPatternMatch *pattern;
    int pattern_counts = 0;

    if (track->cseq_data != NULL)
    {
        free(track->cseq_data);
    }

    // rough guess here, might resize during iteration, will adjust at the end too.
    size_t new_size = (size_t)((float)cseq->track_lengths[track->cseq_track_index] * 1.5f);
    track->cseq_data = (uint8_t *)malloc_zero(1, new_size);

    if (cseq->track_offset[track->cseq_track_index] < CSEQ_FILE_HEADER_SIZE_BYTES)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> track %d, invalid size %d\n", __func__, __LINE__, track->cseq_track_index, cseq->track_offset[track->cseq_track_index]);
    }

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

        // else this is a regular byte
        if (cseq->compressed_data[pos] != 0xfe)
        {
            track->cseq_data[unrolled_pos] = cseq->compressed_data[pos];
            pos++;
            compressed_read_len++;
            unrolled_pos++;
            continue;
        }

        // else this is an escape sequence, read two bytes write one.
        // size check for +2 bytes occurred at the beginning of the loop.
        if (cseq->compressed_data[pos+1] == 0xfe)
        {
            track->cseq_data[unrolled_pos] = cseq->compressed_data[pos];

            if (patterns != NULL)
            {
                pattern = SeqPatternMatch_new_values(unrolled_pos, 0, 1);
                pattern->track_number = track->cseq_track_index;
                pattern->type = CSEQ_PATTERN_ESCAPE;
                node = LinkedListNode_new();
                node->data = pattern;
                LinkedList_append_node(patterns, node);
            }

            total_pattern_unroll++;

            if (g_verbosity >=  VERBOSE_DEBUG)
            {
                printf("found escape sequence. File offset=%ld, track pos=%ld. Total pattern unroll=%ld\n", pos, compressed_read_len, total_pattern_unroll);
            }

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
        total_pattern_unroll += length;

        if (g_verbosity >=  VERBOSE_DEBUG)
        {
            printf("found pattern. File offset=%ld, track pos=%ld, length=%d, diff=%d, pattern_counts=%d. Total pattern unroll=%ld\n", pos, compressed_read_len, length, diff, pattern_counts, total_pattern_unroll);
        }

        if ((int)pos - diff < 0)
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> cseq_track %d references diff %d before start of file, position %ld.\n", __func__, __LINE__, track->cseq_track_index, diff, pos);
        }

        // save pattern if needed
        if (patterns != NULL)
        {
            pattern = SeqPatternMatch_new_values(unrolled_pos, diff, length);
            pattern->track_number = track->cseq_track_index;
            pattern->type = CSEQ_PATTERN_UNROLL;
            node = LinkedListNode_new();
            node->data = pattern;
            LinkedList_append_node(patterns, node);
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

    // cleanup

    TRACE_LEAVE(__func__)
}

/**
 * Iterates a list of patterns and writes to file.
 * @param patterns: List of {@code struct SeqPatternMatch} to write.
 * @param file: File to write to.
*/
void write_patterns_to_file(struct LinkedList *patterns, struct FileInfo *file)
{
    TRACE_ENTER(__func__)

    if (patterns == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> patterns is null\n", __func__, __LINE__);
    }

    if (file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> file is null\n", __func__, __LINE__);
    }

    struct LinkedListNode *node;
    struct SeqPatternMatch *pattern;
    char pattern_write_buffer[WRITE_BUFFER_LEN];

    node = patterns->head;
    while (node != NULL)
    {
        pattern = (struct SeqPatternMatch *)node->data;
        if (pattern != NULL && pattern->type == CSEQ_PATTERN_UNROLL)
        {
            int sprintf_len;
            memset(pattern_write_buffer, 0, WRITE_BUFFER_LEN);
            sprintf_len = sprintf(pattern_write_buffer, "%d,%d,%d,%d\n", pattern->track_number, pattern->start_pattern_pos, pattern->diff, pattern->pattern_length);
            FileInfo_fwrite(file, pattern_write_buffer, sprintf_len, 1);
        }
        node = node->next;
    }

    TRACE_LEAVE(__func__)
}

/**
 * Instead of performing pattern substitution to inflate a file, this copies
 * the track data. This is used when converting between MIDI and seq
 * without pattern substitution. (No 0xfe escaped).
 * @param cseq: Source file (from {@code cseq->compressed_data}).
 * @param track: Destination container (into {@code track->cseq_data}).
*/
void CseqFile_no_unroll_copy(struct CseqFile *cseq, struct GmidTrack *track)
{
    TRACE_ENTER(__func__)

    if (cseq == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> cseq is null\n", __func__, __LINE__);
    }

    if (track == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> track is null\n", __func__, __LINE__);
    }

    if (track->cseq_data != NULL)
    {
        free(track->cseq_data);
    }

    size_t cseq_len;
    size_t pos;

    // allocate destination contain
    cseq_len = cseq->track_lengths[track->cseq_track_index];
    track->cseq_data = (uint8_t *)malloc_zero(1, cseq_len);

    // check that size makes sense
    if (cseq->track_offset[track->cseq_track_index] < CSEQ_FILE_HEADER_SIZE_BYTES)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> track %d, invalid size %d\n", __func__, __LINE__, track->cseq_track_index, cseq->track_offset[track->cseq_track_index]);
    }

    // determine starting position in entire file
    pos = cseq->track_offset[track->cseq_track_index] - CSEQ_FILE_HEADER_SIZE_BYTES;

    // copy to destination and set size
    memcpy(track->cseq_data, &cseq->compressed_data[pos], cseq_len);
    track->cseq_data_len = cseq_len;

    TRACE_LEAVE(__func__)
}


/**
 * Implements naive greedy algorithm to calculate pattern markers
 * in the track. See comments on {@code PATTERN_ALGORITHM_NAIVE}.
 * Track data remains unaltered, this only calculates markers.
 * @param gtrack: Seq track to create markers for.
 * @param write_buffer: Current cseq file buffer written so far. Patterns
 * can refer to locations in previous tracks; unrolling occurs against
 * a snapshot of track, so pattern substitution must be applied to compute
 * accurate offsets.
 * @param current_buffer_pos: Treated as size of buffer, points to next
 * byte address that would be written into buffer.
 * @param matches: Adds pattern markers to this list. Must be previously allocated.
*/
void GmidTrack_get_pattern_matches_naive(struct GmidTrack *gtrack, uint8_t *write_buffer, size_t *current_buffer_pos, struct LinkedList *matches)
{
    TRACE_ENTER(__func__)

    if (gtrack == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> gtrack is null\n", __func__, __LINE__);
    }

    if (gtrack->cseq_data == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> gtrack->cseq_data is null\n", __func__, __LINE__);
    }

    if (write_buffer == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> write_buffer is null\n", __func__, __LINE__);
    }

    if (current_buffer_pos == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> current_buffer_pos is null\n", __func__, __LINE__);
    }

    /**
     * Pattern marker substitution occurs against a snapshot of the track, which
     * means future patterns refer over not-yet-unrolled patterns. In order to
     * calculate pattern markers here without altering the state of the track/file,
     * the existing output so far is copied into a temp buffer. This method appends
     * to the buffer performing pattern substitution, and uses the growing buffer
     * to calculate patterns for the track.
    */

    struct LinkedListNode *node;
    struct SeqPatternMatch *match;
    int pos;
    int compare_pos;
    int pattern_length;
    int compare_lower_limit;
    int compare_upper_limit;
    int track_len;
    int copy_len;

    uint8_t *copy;
    int copy_pos;

    // copy existing output to local buffer
    copy_pos = (int)*current_buffer_pos;
    copy_len = copy_pos + gtrack->cseq_data_len + 100; // some extra for safety
    copy = (uint8_t *)malloc_zero(1, copy_len);
    memcpy(copy, write_buffer, *current_buffer_pos);
    
    track_len = (int)gtrack->cseq_data_len;
    pos = 0;

    /**
     * Simple greedy algorithm, iterate every byte in the track, compare
     * it to every byte before it (up to max distance). Take first
     * matching pattern found over min length.
    */
    while (pos < track_len)
    {
        int pos_increment_amount = 1;
        match = NULL;

        // set lower compare limit to furthest allowed distance in output buffer.
        compare_lower_limit = copy_pos - g_max_pattern_distance;
        if (compare_lower_limit < 0)
        {
            compare_lower_limit = 0;
        }

        // Upper compare limit ends at the current position
        compare_upper_limit = copy_pos;

        if (gtrack->cseq_data[pos] != 0xff)
        {
            for (compare_pos = compare_lower_limit; compare_pos < compare_upper_limit; compare_pos++)
            {
                pattern_length = 0;

                while (
                    // pattern match can't exeed current track
                    pos + pattern_length < track_len
                    // and match doesn't exceed upper limit
                    && compare_pos + pattern_length < compare_upper_limit
                    // escape sequences not allowed in pattern
                    && gtrack->cseq_data[pos + pattern_length] != 0xff
                    // current byte must match prior position byte
                    && copy[compare_pos + pattern_length] == gtrack->cseq_data[pos + pattern_length]
                    // don't exceed max pattern length
                    && pattern_length < g_max_pattern_length)
                {
                    pattern_length++;
                }

                if (pattern_length > g_min_pattern_length)
                {
                    int base_pos = compare_pos; // relative to start of file
                    int start_pattern_pos = pos; // relative to original start of track

                    // current write position difference to starting compare position
                    int diff = copy_pos - compare_pos;

                    if (g_verbosity >= VERBOSE_DEBUG)
                    {
                        printf("found pattern, compare_pos=%d, base_pos=%d, length=%d, for read_pos=%d\n", compare_pos, base_pos, pattern_length, pos);
                    }

                    compare_pos += pattern_length;
                    pos_increment_amount = pattern_length;

                    match = SeqPatternMatch_new_values(start_pattern_pos, diff, pattern_length);
                    match->type = CSEQ_PATTERN_UNROLL;

                    node = LinkedListNode_new();
                    node->data = match;
                    LinkedList_append_node(matches, node);

                    break;
                }
            }
        }

        // no match, copy byte to local output buffer
        if (match == NULL)
        {
            if (gtrack->cseq_data[pos] != 0xfe)
            {
                if (copy_pos + 1 > copy_len)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> buffer write overflow. copy_pos=%d, for pos=%d\n", __func__, __LINE__, copy_pos, pos);
                }

                copy[copy_pos] = gtrack->cseq_data[pos];
                copy_pos++;
            }
            else
            {
                if (copy_pos + 2 > copy_len)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> buffer write overflow. copy_pos=%d, for pos=%d\n", __func__, __LINE__, copy_pos, pos);
                }

                write_buffer[copy_pos] = gtrack->cseq_data[pos];
                copy_pos++;

                write_buffer[copy_pos] = 0xfe;
                copy_pos++;
            }
        }
        else
        {
            // else there was a match, write pattern to local output buffer.

            if (copy_pos + 4 > copy_len)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> buffer write overflow. copy_pos=%d, for pos=%d\n", __func__, __LINE__, copy_pos, pos);
            }

            /**
             * Pattern marker consists of four bytes.
             * 0) 0xfe
             * 1-2) 16-bit offset to start of pattern
             * 3) length of pattern
            */
            write_buffer[copy_pos] = 0xfe;
            copy_pos++;

            write_buffer[copy_pos] = (match->diff >> 8) & 0xff;
            copy_pos++;
            write_buffer[copy_pos] = (match->diff >> 0) & 0xff;
            copy_pos++;

            write_buffer[copy_pos] = match->pattern_length & 0xff;
            copy_pos++;
        }

        pos += pos_increment_amount;
    }

    free(copy);

    TRACE_LEAVE(__func__)
}

/**
 * Applies previously computed pattern matches on a {@code gtrack->cseq_data}.
 * Will append track data to {@code write_buffer}. Even if no patterns are
 * applied on this track, byte escape sequences (0xfe) are applied.
 * @param gtrack: Track to update.
 * @param write_buffer: The write_buffer should contain everything written to the cseq
 * file so far. Updated track data will be appended.
 * @param current_buffer_pos: In/out parameter. Current position in {@code write_buffer}.
 * @param buffer_len: Size in bytes of {@code write_buffer}.
 * @param matches: List of {@code struct SeqPatternMatch} to apply.
*/
void GmidTrack_roll_apply_patterns(struct GmidTrack *gtrack, uint8_t *write_buffer, size_t *current_buffer_pos, size_t buffer_len, struct LinkedList *matches)
{
    TRACE_ENTER(__func__)

    if (gtrack == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> gtrack is null\n", __func__, __LINE__);
    }

    if (gtrack->cseq_data == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> gtrack->cseq_data is null\n", __func__, __LINE__);
    }

    if (write_buffer == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> write_buffer is null\n", __func__, __LINE__);
    }

    if (current_buffer_pos == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> current_buffer_pos is null\n", __func__, __LINE__);
    }

    struct LinkedListNode *node;
    struct SeqPatternMatch *match;
    int pos;
    int lower_limit;
    int nothing_count;
    int write_buffer_pos;
    int match_index;
    int matches_count = 0;

    node = NULL;
    match = NULL;

    // ensure patterns are sorted according to location within the track.
    if (matches != NULL)
    {
        matches_count = matches->count;
        LinkedList_merge_sort(matches, LinkedListNode_SeqPatternMatch_compare_smaller);
    }

    match_index = 0;
    lower_limit = 0;
    nothing_count = 0;
    pos = 0;
    write_buffer_pos = (int)*current_buffer_pos;

    // If there are no patterns then perform standard copy/escape
    // to end of track.
    if (matches == NULL || matches_count == 0)
    {
        lower_limit = (int)gtrack->cseq_data_len;
    }
    
    /**
     * The algorithm here is to:
     * 1) Copy/escape up to lower_limit. This is the next pattern or end of track.
     * 2) Write the next pattern information
     * 3) Increment to next pattern and set next lower_limit.
    */
    while (pos < (int)gtrack->cseq_data_len)
    {
        // copy/escape bytes.
        while (pos < lower_limit)
        {
            if (gtrack->cseq_data[pos] != 0xfe)
            {
                if (write_buffer_pos + 1 > (int)buffer_len)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> buffer write overflow. write_buffer_pos=%d, for pos=%d\n", __func__, __LINE__, write_buffer_pos, pos);
                }

                write_buffer[write_buffer_pos] = gtrack->cseq_data[pos];
                write_buffer_pos++;
                pos++;
            }
            else
            {
                if (write_buffer_pos + 2 > (int)buffer_len)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> buffer write overflow. write_buffer_pos=%d, for pos=%d\n", __func__, __LINE__, write_buffer_pos, pos);
                }

                write_buffer[write_buffer_pos] = gtrack->cseq_data[pos];
                write_buffer_pos++;
                pos++;

                write_buffer[write_buffer_pos] = 0xfe;
                write_buffer_pos++;
            }

            nothing_count = 0;
        }

        // Write next pattern information.
        if (match != NULL)
        {
            if (write_buffer_pos + 4 > (int)buffer_len)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> buffer write overflow. write_buffer_pos=%d, length=%d, for pos=%d\n", __func__, __LINE__, write_buffer_pos, match->pattern_length, pos);
            }

            /**
             * Pattern marker consists of four bytes.
             * 0) 0xfe
             * 1-2) 16-bit offset to start of pattern
             * 3) length of pattern
            */
            write_buffer[write_buffer_pos] = 0xfe;
            write_buffer_pos++;

            write_buffer[write_buffer_pos] = (match->diff >> 8) & 0xff;
            write_buffer_pos++;
            write_buffer[write_buffer_pos] = (match->diff >> 0) & 0xff;
            write_buffer_pos++;

            write_buffer[write_buffer_pos] = match->pattern_length & 0xff;
            write_buffer_pos++;

            if (g_verbosity >= VERBOSE_DEBUG)
            {
                printf("write pattern, position=%d, diff=%d, length=%d\n", write_buffer_pos-4, match->diff, match->pattern_length);
            }

            // There was a match, so this means any seq loops that contain
            // the substitution are now incorrect (end delta is wrong).
            // Since the loop end events contains offset to beginning, just iterate
            // the list of events for end events and check if it's in range.
            // If so, adjust the event in the list (just to be nice), but more
            // importantly, update cseq_data.
            struct LinkedListNode *end_node;
            end_node = gtrack->events->head;
            while (end_node != NULL)
            {
                struct GmidEvent *end_event = end_node->data;
                if (end_event != NULL
                    && end_event->cseq_valid == 1
                    // if this is loop end event
                    && end_event->command == CSEQ_COMMAND_BYTE_LOOP_END_WITH_META
                    // and it's after the current position in the source bytes
                    && (int)end_event->file_offset > pos
                    // and the loop delta says the loop starts before the current position
                    && (int)end_event->file_offset - end_event->cseq_command_parameters[2] < pos
                    // and this isn't a sysex loop event imported from midi (which already has preset value)
                    && (end_event->flags & MIDI_MALFORMED_EVENT_LOOP) == 0)
                {
                    int loop_adjust = match->pattern_length - 4;
                    int adjust_byte_pos;
                    int new_end_delta;

                    // update event. Delta is paramater index 2.
                    new_end_delta = end_event->cseq_command_parameters[2] - loop_adjust;

                    if (g_verbosity >= VERBOSE_DEBUG)
                    {
                        printf("change loop end event, position=%ld, from delta=%d to %d\n", end_event->file_offset, end_event->cseq_command_parameters[2], new_end_delta);
                    }

                    end_event->cseq_command_parameters[2] = new_end_delta;
                    // first two parameters are one byte each, delta starts at byte index 2.
                    end_event->cseq_command_parameters_raw[2] = (uint8_t)((new_end_delta >> 24) & 0xff);
                    end_event->cseq_command_parameters_raw[3] = (uint8_t)((new_end_delta >> 16) & 0xff);
                    end_event->cseq_command_parameters_raw[4] = (uint8_t)((new_end_delta >> 8) & 0xff);
                    end_event->cseq_command_parameters_raw[5] = (uint8_t)((new_end_delta >> 0) & 0xff);
                    
                    if (end_event->file_offset + 
                        end_event->cseq_command_len + 
                        end_event->cseq_delta_time.num_bytes > gtrack->cseq_data_len)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "%s %d> loop end event offset %ld exceeds track data length %ld\n", __func__, __LINE__, end_event->file_offset, gtrack->cseq_data_len);
                    }

                    // update cseq_data
                    adjust_byte_pos = end_event->file_offset + end_event->cseq_delta_time.num_bytes;
                    // 2 bytes for command, 2 bytes for two loop counts, delta starts at byte index 4.
                    gtrack->cseq_data[adjust_byte_pos + 4] = (uint8_t)((new_end_delta >> 24) & 0xff);
                    gtrack->cseq_data[adjust_byte_pos + 5] = (uint8_t)((new_end_delta >> 16) & 0xff);
                    gtrack->cseq_data[adjust_byte_pos + 6] = (uint8_t)((new_end_delta >> 8) & 0xff);
                    gtrack->cseq_data[adjust_byte_pos + 7] = (uint8_t)((new_end_delta >> 0) & 0xff);
                }

                end_node = end_node->next;
            }

            // move index in cseq_data forward by the amount skipped.
            pos += match->pattern_length;

            // mark that something happened this loop
            nothing_count = 0;
        }

        // Increment to next match.
        if (match_index < matches_count && matches != NULL)
        {
            if (match_index == 0)
            {
                node = matches->head;
            }
            else
            {
                node = node->next;
            }

            if (node == NULL)
            {
                stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> node is null\n", __func__, __LINE__);
            }

            match = (struct SeqPatternMatch *)node->data;

            if (match == NULL)
            {
                stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> match is null\n", __func__, __LINE__);
            }

            match_index++;
            nothing_count = 0;
        }
        else
        {
            match = NULL;
        }

        // Set next lower limit, either pattern or end of track.
        if (match != NULL)
        {
            lower_limit = match->start_pattern_pos;
        }
        else
        {
            lower_limit = (int)gtrack->cseq_data_len;
        }

        // safety flag to avoid infinite loop.
        nothing_count++;

        if (nothing_count > 5)
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> infinite loop\n", __func__, __LINE__);
        }
    }

    // Done with existing track data. Free the associated memory before
    // overwriting the pointer.
    if (gtrack->cseq_data != NULL)
    {
        free(gtrack->cseq_data);
    }

    // find the number of bytes written to output buffer.
    // This is the difference between start and end location.
    int write_byte_len = write_buffer_pos - (int)*current_buffer_pos;

    // allocate memory for track compressed data
    gtrack->cseq_data = (uint8_t *)malloc_zero(1, write_byte_len);

    // memcpy from the original starting position of the write buffer
    // into the new cseq data container
    memcpy(gtrack->cseq_data, &write_buffer[*current_buffer_pos], write_byte_len);

    // update out parameter to current write buffer position
    *current_buffer_pos = write_buffer_pos;

    // Set track data length to correct value
    gtrack->cseq_data_len = write_byte_len;
    gtrack->cseq_track_size_bytes = write_byte_len;

    // cleanup

    TRACE_LEAVE(__func__)
}

/**
 * Parses a previously written file into a list of {@code struct SeqPatternMatch}.
 * Simple csv file format, there should be one struct per line.
 * This accepts whitespace (space or tab) or commas as delimiters.
 * There should be one entry for each field in the struct on the line, in
 * order of fields declared.
 * @param options: Conversion options, notably filename to parse.
 * @param matches: List to append results to. Must be previously allocated.
*/
void GmidTrack_get_pattern_matches_file(struct MidiConvertOptions *options, struct LinkedList *matches)
{
    TRACE_ENTER(__func__)

    if (options == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> options is null\n", __func__, __LINE__);
    }

    struct FileInfo *pattern_file = options->runtime_pattern_file;

    if (pattern_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> pattern_file is null\n", __func__, __LINE__);
    }

    if (matches == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> matches is null\n", __func__, __LINE__);
    }

    if (pattern_file->len < 1)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> pattern_file->len == 0\n", __func__, __LINE__);
    }

    struct LinkedListNode *node;
    struct SeqPatternMatch *match;
    uint8_t *file_contents;
    int pos;
    int len;
    char number_buffer[NUM_BUFFER_SIZE];
    int buffer_pos;
    int current_line_number;
    int line_buffer_pos;
    int line_values = 0;
    int c_int;
    int previous_c;
    int state = 0;
    char c;

    len = (int)pattern_file->len;

    file_contents = (uint8_t *)malloc_zero(1, len);
    FileInfo_fseek(pattern_file, 0, SEEK_SET);
    FileInfo_fread(pattern_file, file_contents, len, 1);

    pos = 0;
    line_buffer_pos = 0;
    current_line_number = 1;
    c_int = -1;
    previous_c = -1;
    buffer_pos = 0;
    memset(number_buffer, 0, NUM_BUFFER_SIZE);

    while (pos < len)
    {
        previous_c = c_int;
        c = file_contents[pos];
        c_int = 0xff & (int)c;

        pos++;
        line_buffer_pos++;

        // any '\n' or '\r' increments the count.
        if (is_newline(c))
        {
            if (buffer_pos > 0 && line_values < 3)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> incomplete line %d, expecting 4 values\n", __func__, __LINE__, current_line_number);
            }

            current_line_number++;
            line_buffer_pos = 0;
            // don't reset value buffer, could be reading last value on line.
        }
        
        // but if this is windows, adjust for "\r\n" double counting.
        if (is_windows_newline(c_int, previous_c))
        {
            current_line_number--;
        }

        /**
         * Simple state machine, this is either waiting for int,
         * or reading int.
         * Then proceeds to `state=2`, where the value is parsed
         * and added to current match.
        */
        switch (state)
        {
            case 0: // waiting for char
            {
                if (is_newline(c) || is_whitespace(c))
                {
                    // nothing to do
                }
                else if (is_numeric_int(c))
                {
                    if ((buffer_pos + 2) > NUM_BUFFER_SIZE)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "%s %d> buffer overflow (line position %d) readline line, pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, line_buffer_pos, pos, current_line_number, state);
                    }

                    number_buffer[buffer_pos] = c;
                    buffer_pos++;
                    state = 1;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c' (line position %d) readline line, pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, line_buffer_pos, pos, current_line_number, state);
                }
            }
            break;

            case 1: // reading int
            {
                if (c == ',' || is_newline(c) || is_whitespace(c))
                {
                    state = 2;
                }
                else if (is_numeric_int(c))
                {
                    if ((buffer_pos + 2) > NUM_BUFFER_SIZE)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "%s %d> buffer overflow (line position %d) readline line, pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, line_buffer_pos, pos, current_line_number, state);
                    }

                    number_buffer[buffer_pos] = c;
                    buffer_pos++;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c' (line position %d) readline line, pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, line_buffer_pos, pos, current_line_number, state);
                }
            }
            break;
        }

        // finished reading int, add to current match.
        if (state == 2)
        {
            int file_value;

            if (buffer_pos == 0)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> attempted to add empty value as integer (line position %d), pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, line_buffer_pos, pos, current_line_number, state);
            }
            
            file_value = parse_int(number_buffer);
            memset(number_buffer, 0, NUM_BUFFER_SIZE);
            buffer_pos = 0;

            state = 0;

            // simple state machine, how many values have been parsed on this line.
            switch (line_values)
            {
                case 0:
                {
                    match = SeqPatternMatch_new();
                    match->track_number = file_value;
                    match->type = CSEQ_PATTERN_UNROLL;
                    line_values++;
                }
                break;

                case 1:
                {
                    match->start_pattern_pos = file_value;
                    line_values++;
                }
                break;

                case 2:
                {
                    match->diff = file_value;
                    line_values++;
                }
                break;

                case 3:
                {
                    match->pattern_length = file_value;
                    node = LinkedListNode_new();
                    node->data = match;
                    LinkedList_append_node(matches, node);
                    line_values = 0;
                }
                break;
            }
        }
    }

    // it's possible the last byte of the file is part of the last int value,
    // check for that here
    if (match != NULL && buffer_pos > 0)
    {
        int file_value;
        
        file_value = parse_int(number_buffer);
        memset(number_buffer, 0, NUM_BUFFER_SIZE);
        buffer_pos = 0;

        state = 0;

        switch (line_values)
        {
            // since this is the end of the file, only care if this finishes
            // the current match
            case 3:
            {
                match->pattern_length = file_value;
                node = LinkedListNode_new();
                node->data = match;
                LinkedList_append_node(matches, node);
                line_values = 0;
            }
            break;
        }
    }

    // sanity check.
    if (line_values != 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> incomplete last line, expecting 4 values\n", __func__, __LINE__);
    }

    // cleanup.
    free(file_contents);

    TRACE_LEAVE(__func__)
}

/**
 * Compares {@code SeqPatternMatch->track_number} to value.
 * Function pointer for use in filtering list of {@code SeqPatternMatch}.
 * @param node: Node containing match.
 * @param arg1: track number to compare to.
 * @returns: 1 if {@code SeqPatternMatch->track_number} equals track number, 0 otherwise.
*/
int where_SeqPatternMatch_is_track(struct LinkedListNode *node, int arg1)
{
    TRACE_ENTER(__func__)

    if (node == NULL)
    {
        TRACE_LEAVE(__func__)
        return 0;
    }

    struct SeqPatternMatch *match = node->data;

    if (match == NULL)
    {
        TRACE_LEAVE(__func__)
        return 0;
    }

    return match->track_number == arg1;

    TRACE_LEAVE(__func__)
}

/**
 * Top level entry point to resolve how to apply pattern substitution to a track (including none).
 * @param gtrack: Track to update. If pattern substitution is applied then
 * {@code gtrack->cseq_data} will be over-written.
 * @param write_buffer: Patterns can refer to locations before the previous
 * track. This write_buffer should contain everything written to the cseq
 * file so far. If pattern substitution is applied then this will have
 * the current track data appended.
 * @param current_buffer_pos: In/out parameter. Current position in {@code write_buffer}.
 * @param buffer_len: Size in bytes of {@code write_buffer}.
 * @param options: Conversion options.
*/
void GmidTrack_roll_entry(struct GmidTrack *gtrack, uint8_t *write_buffer, size_t *current_buffer_pos, size_t buffer_len, struct MidiConvertOptions *options)
{
    TRACE_ENTER(__func__)

    if (gtrack == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> gtrack is NULL\n", __func__, __LINE__);
    }

    if (write_buffer == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> write_buffer is NULL\n", __func__, __LINE__);
    }

    if (current_buffer_pos == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> current_buffer_pos is NULL\n", __func__, __LINE__);
    }

    struct LinkedList *matches;
    struct LinkedListNode *node;
    struct SeqPatternMatch *match;
    int apply_patterns;
    int is_double_reference;

    // The where filter clones the node, but doesn't clone the data, resulting in
    // two references to data, resulting in double free.
    is_double_reference = 0;
    apply_patterns = 1;
    matches = LinkedList_new();

    // by default, apply naive pattern compression.
    if (options == NULL)
    {
        GmidTrack_get_pattern_matches_naive(gtrack, write_buffer, current_buffer_pos, matches);
    }
    else if (options->use_pattern_marker_file)
    {
        if (options->runtime_pattern_file == NULL)
        {
            options->runtime_pattern_file = FileInfo_fopen(options->pattern_marker_filename, "rb");
        }

        if (options->runtime_patterns_list == NULL)
        {
            options->runtime_patterns_list = LinkedList_new();
            GmidTrack_get_pattern_matches_file(options, options->runtime_patterns_list);
        }

        LinkedList_where_i(matches, options->runtime_patterns_list, where_SeqPatternMatch_is_track, gtrack->cseq_track_index);
        is_double_reference = 1;
    }
    else if (options->no_pattern_compression == 0)
    {
        if (options->pattern_algorithm == PATTERN_ALGORITHM_NAIVE)
        {
            GmidTrack_get_pattern_matches_naive(gtrack, write_buffer, current_buffer_pos, matches);
        }
        else
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> unsupported pattern_algorithm=%d\n", __func__, __LINE__, options->pattern_algorithm);
        }
    }
    else // options->no_pattern_compression == 1
    {
        apply_patterns = 0;
    }

    /**
     * gtrack->cseq_data contains sequence data, any of the above options will
     * apply pattern substitution, which will overwrite gtrack->cseq_data and
     * append to write_buffer. If there's no pattern matching, then gtrack->cseq_data
     * already contains the correct data, and write_buffer isn't used so there's
     * nothing to do.
     * Note: doing nothing has the intended side-effect of not escaping 0xfe.
    */
    if (apply_patterns)
    {
        if (g_verbosity >= VERBOSE_DEBUG)
        {
            printf("GmidTrack_roll_apply_patterns matches->count = %ld\n", matches->count);
        }

        GmidTrack_roll_apply_patterns(gtrack, write_buffer, current_buffer_pos, buffer_len, matches);
    }

    // cleanup.
    node = matches->head;
    while (node != NULL)
    {
        match = (struct SeqPatternMatch *)node->data;
        
        // Only free SeqPatternMatch if this isn't a clone.
        if (is_double_reference == 0 && match != NULL)
        {
            SeqPatternMatch_free(match);
            node->data = NULL;
        }
        node = node->next;
    }

    LinkedList_free(matches);

    TRACE_LEAVE(__func__)
}

/**
 * Creates a new compressed MIDI container from existing track data.
 * The tracks must be passed in order.
 * Null tracks are accepted.
 * No pattern substitution occurs.
 * This allocates memory.
 * This copies each {@code track->cseq_data} into {@code cseq->compressed_data}.
 * @param track: List of pointers to {@code GmidTrack}. Unused tracks can be NULL.
 * @param num_tracks: Number of items in {@code track}.
 * @returns: pointer to new file container.
*/
struct CseqFile *CseqFile_new_from_tracks(struct GmidTrack **track, size_t num_tracks)
{
    TRACE_ENTER(__func__)

    if (track == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> track is null\n", __func__, __LINE__);
    }

    // null implementation, no pattern substitution
    int i;
    int cseq_pointer_pos;
    int non_empty_tracks = 0;
    int num_tracks_int = (int)num_tracks;
    size_t data_size = 0;

    // Count required memory allocation size
    for (i=0; i<num_tracks_int; i++)
    {
        if (track[i] != NULL)
        {
            non_empty_tracks++;

            data_size += track[i]->cseq_data_len;
        }
    }

    // allocated new cseq file
    struct CseqFile *cseq = CseqFile_new();
    cseq->non_empty_num_tracks = non_empty_tracks;
    cseq->compressed_data = (uint8_t *)malloc_zero(1, data_size);
    cseq->compressed_data_len = data_size;

    cseq_pointer_pos = 0;
    for (i=0; i<num_tracks_int; i++)
    {
        if (track[i] != NULL
            && track[i]->events != NULL
            && track[i]->events->count > 0)
        {
            cseq->track_offset[i] = cseq_pointer_pos;

            // adjust offset to start of data in file.
            cseq->track_offset[i] += CSEQ_FILE_HEADER_SIZE_BYTES;

            // copy into output buffer
            memcpy(&cseq->compressed_data[cseq_pointer_pos], track[i]->cseq_data, track[i]->cseq_data_len);
            cseq_pointer_pos += track[i]->cseq_data_len;

            // set cseq track size
            cseq->track_lengths[i] = track[i]->cseq_data_len;
        }
        else
        {
            // null tracks should have the offset set to 0
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
 * default value (should be set externally).
 * This allocates memory.
 * @param buffer: Buffer containing MIDI data. Should be address of start of buffer.
 * @param pos_ptr: In/Out parameter. Current position to read from in the buffer.
 * @param buffer_len: Length of buffer in bytes.
 * @param buffer_type: Type of MIDI data.
 * @param current_command: most recent command, used for running status commands. This needs to include the command channel.
 * @param bytes_read: Out parameter. Optional. Will contain the number of bytes read from the buffer.
 * @returns: new event container.
*/
struct GmidEvent *GmidEvent_new_from_buffer(
    uint8_t *buffer,
    size_t *pos_ptr,
    size_t buffer_len,
    enum MIDI_IMPLEMENTATION buffer_type,
    int32_t current_command,
    int *bytes_read)
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
    int command_without_channel = 0;
    // temp varint for parsing/copying values
    struct VarLengthInt varint;
    // debug print buffer
    char print_buffer[MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN];
    // debug print buffer
    char description_buffer[MIDI_DESCRIPTION_TEXT_BUFFER_LEN];
    // assumes `b` is a valid command, this holds the general command
    int message_type;
    // flag to indicate meta event or not.
    int read_command_values;
    int local_bytes_read;
    int copy_current_command = current_command;

    struct GmidEvent *event = NULL;

    pos = *pos_ptr;
    local_bytes_read = 0;
    read_command_values = 1;

    // if meta command was passed as current, ignore it.
    if (((uint32_t)current_command & 0xffffff00) == 0xff00)
    {
        current_command = 0;
    }

    if (current_command != 0)
    {
        command_channel = current_command & 0x0f;
        command_without_channel = current_command & 0xf0;
    }

    memset(&varint, 0, sizeof(struct VarLengthInt));
    VarLengthInt_value_to_int32(&buffer[pos], 4, &varint);

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

        int32_t varint_value = VarLengthInt_get_value_big(&varint);
        snprintf(print_buffer, MIDI_DESCRIPTION_TEXT_BUFFER_LEN, "delta time: %d (varint=0x%06x)", varint.standard_value, varint_value);
        fflush_printf(stdout, "%s\n", print_buffer);
    }

    event = GmidEvent_new();

    // Set the file offset the event begins at. pos was updated, but the pointer
    // of the argument won't be updated until the end of this method.
    event->file_offset = *pos_ptr;

    // Set delta time. Absolute time won't be set until all events are added to the track.
    VarLengthInt_copy(&event->cseq_delta_time, &varint);
    VarLengthInt_copy(&event->midi_delta_time, &varint);

    /**
     * If current command channel is known, set that on event.
     * If the event being read has a command channel, that will
     * update the event. If this is a system command, the channel
     * should be reset to -1.
     * The command parameter parsing should not change the channel.
    */
    if (command_channel != -1)
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
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> %s: expected len=0x03 but read '0x%x', pos=%ld.\n", __func__, __LINE__, MIDI_COMMAND_NAME_TEMPO, b, pos);
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
                event->midi_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_TEMPO;
                event->midi_command_parameters_raw[0] = 3; // len
                event->midi_command_parameters_raw[1] = (uint8_t)((tempo >> 16) & 0xff);
                event->midi_command_parameters_raw[2] = (uint8_t)((tempo >> 8) & 0xff);
                event->midi_command_parameters_raw[3] = (uint8_t)((tempo >> 0) & 0xff);
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
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> %s: expected end of command byte 0xff but read '0x%x', pos=%ld.\n", __func__, __LINE__, CSEQ_COMMAND_NAME_LOOP_START, b, pos);
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
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> %s: expected end of command byte 0x00 but read '0x%x', pos=%ld.\n", __func__, __LINE__, MIDI_COMMAND_NAME_END_OF_TRACK, b, pos);
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
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> parse error (system command=0x%0x), buffer type=%d, pos=%ld.\n", __func__, __LINE__, b, buffer_type, pos);
            }
        }
        else if (b == MIDI_COMMAND_BYTE_SYSEX_START && buffer_type == MIDI_IMPLEMENTATION_STANDARD)
        {
            // only supported sysex commands are the gaudio format sysex seq loop start and end.
            int sysex_first;
            int sysex_check; 
            int escaped_command; 

            command_without_channel = -1;
            read_command_values = 0;

            // 3 bytes to figure out what's being read
            if (pos + 3 > buffer_len)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> exceeded buffer len %ld when parsing system exclusive event\n", __func__, __LINE__, buffer_len);
            }

            // if this is a system command, then channel isn't included in event
            event->command_channel = -1;

            b = buffer[pos++];
            local_bytes_read++;
            sysex_first = b;

            b = buffer[pos++];
            local_bytes_read++;
            sysex_check = b;

            b = buffer[pos++];
            local_bytes_read++;
            escaped_command = b;

            event->midi_command_parameters[0] = sysex_first;
            event->midi_command_parameters_raw[0] = sysex_first;
            event->midi_command_parameters[1] = sysex_check;
            event->midi_command_parameters_raw[1] = sysex_check;
            event->midi_command_parameters[2] = escaped_command;
            event->midi_command_parameters_raw[2] = escaped_command;

            if (sysex_first != MIDI_COMMAND_BYTE_SYSEX_UNIVERSAL_NON_COMMERICIAL
                || sysex_check != MIDI_COMMAND_BYTE_SYSEX_GAUDIO_PREFIX)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> parse error -- unsupport sysex command 0x%0x%0x. pos=%ld.\n", __func__, __LINE__, sysex_first, sysex_check, pos);
            }

            if (escaped_command != CSEQ_COMMAND_BYTE_LOOP_START 
                && escaped_command != CSEQ_COMMAND_BYTE_LOOP_END)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> parse error -- unsupport sysex command 0x%0x%0x%0x. pos=%ld.\n", __func__, __LINE__, sysex_first, sysex_check, escaped_command, pos);
            }

            if (escaped_command == CSEQ_COMMAND_BYTE_LOOP_START)
            {
                // what is 3: two bytes for loop number, one byte for 0xf7
                if (pos + 3 > buffer_len)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> exceeded buffer len %ld when parsing system exclusive event\n", __func__, __LINE__, buffer_len);
                }

                int param_pos = 3;
                int param_i;
                int param_remain = 3;
                
                // The only thing that matters is the closing 0xf7.
                for (param_i=0; param_i < param_remain; param_i++)
                {
                    b = buffer[pos++];
                    local_bytes_read++;
                    event->midi_command_parameters[param_pos] = b;
                    event->midi_command_parameters_raw[param_pos] = b;
                    param_pos++;
                }

                if (b != MIDI_COMMAND_BYTE_SYSEX_END)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> gaudio sysex escape %s: expected end of sysex command 0x%0x but read '0x%x', pos=%ld.\n", __func__, __LINE__, CSEQ_COMMAND_NAME_LOOP_START, MIDI_COMMAND_BYTE_SYSEX_END, b, pos);
                }

                // optional debug print
                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "gaudio sysex escape %s", CSEQ_COMMAND_NAME_LOOP_START);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }

                // save parsed data
                event->command = MIDI_COMMAND_BYTE_SYSEX_START;
                event->dual = NULL;
                event->cseq_valid = 0;
                event->midi_valid = 1;

                // MIDI: store original

                event->midi_command_len = MIDI_COMMAND_LEN_SYSEX;
                event->midi_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_SYSEX_SEQ_LOOP_START;
                event->midi_command_parameters_len = MIDI_COMMAND_PARAM_BYTE_SYSEX_SEQ_LOOP_START;
                // paramaters set above when reading.

                // cseq: not valid. Will be converted if needed.
            }
            else if (escaped_command == CSEQ_COMMAND_BYTE_LOOP_END)
            {
                // what is 13: 12 bytes for loop seq end event, one byte for 0xf7
                if (pos + 13 > buffer_len)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> exceeded buffer len %ld when parsing system exclusive event\n", __func__, __LINE__, buffer_len);
                }

                int param_pos = 3;
                int param_i;
                int param_remain = 13;
                
                // The only thing that matters is the closing 0xf7.
                for (param_i=0; param_i < param_remain; param_i++)
                {
                    b = buffer[pos++];
                    local_bytes_read++;
                    event->midi_command_parameters[param_pos] = b;
                    event->midi_command_parameters_raw[param_pos] = b;
                    param_pos++;
                }
                
                if (b != MIDI_COMMAND_BYTE_SYSEX_END)
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> gaudio sysex escape %s: expected end of sysex command 0x%0x but read '0x%x', pos=%ld.\n", __func__, __LINE__, CSEQ_COMMAND_NAME_LOOP_START, MIDI_COMMAND_BYTE_SYSEX_END, b, pos);
                }

                // optional debug print
                if (g_midi_parse_debug)
                {
                    memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                    snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "gaudio sysex escape %s", CSEQ_COMMAND_NAME_LOOP_END);
                    fflush_printf(stdout, "%s\n", print_buffer);
                }

                // save parsed data
                event->command = MIDI_COMMAND_BYTE_SYSEX_START;
                event->dual = NULL;
                event->cseq_valid = 0;
                event->midi_valid = 1;

                // MIDI: store original

                event->midi_command_len = MIDI_COMMAND_LEN_SYSEX;
                event->midi_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_SYSEX_SEQ_LOOP_END;
                event->midi_command_parameters_len = MIDI_COMMAND_PARAM_BYTE_SYSEX_SEQ_LOOP_END;
                // paramaters set above when reading.

                // cseq: not valid. Will be converted if needed.
            }
        }
        else if (b == CSEQ_COMMAND_BYTE_PATTERN && buffer_type == MIDI_IMPLEMENTATION_SEQ)
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> parse error -- invalid compressed MIDI, \"pattern\" command not allowed. pos=%ld.\n", __func__, __LINE__, pos);
        }
        else if (message_type == MIDI_COMMAND_BYTE_NOTE_OFF && buffer_type == MIDI_IMPLEMENTATION_SEQ)
        {
            // note off
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> parse error -- invalid compressed MIDI, \"note off\" command not allowed. pos=%d.\n", __func__, __LINE__, pos);
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
            command_without_channel = MIDI_COMMAND_BYTE_PITCH_BEND;

            if (g_midi_parse_debug)
            {
                memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "command %s: channel %d", MIDI_COMMAND_NAME_PITCH_BEND, command_channel);
                fflush_printf(stdout, "%s\n", print_buffer);
            }            
        }
        else
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> parse error (command=0x%0x), buffer type=%d, pos=%ld.\n", __func__, __LINE__, b, buffer_type, pos);
        }
    }

    if (read_command_values)
    {
        if (command_without_channel < 1)
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> Invalid current_command: 0x%08x\n", __func__, __LINE__, copy_current_command);
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

            memset(&varint, 0, sizeof(struct VarLengthInt));
            VarLengthInt_value_to_int32(&buffer[pos], 4, &varint);
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
            event->cseq_command_parameters_raw[0] = note;
            event->cseq_command_parameters_raw[1] = velocity;
            event->cseq_command_len = CSEQ_COMMAND_LEN_NOTE_ON;
            event->cseq_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_NOTE_ON; // this is wrong, but all we know for now
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
            if (pos + MIDI_COMMAND_PARAM_BYTE_PITCH_BEND > buffer_len)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> exceeded buffer len %ld when parsing event\n", __func__, __LINE__, buffer_len);
            }

            // copy raw value in same endian, other values set at end.
            memcpy(event->cseq_command_parameters_raw, &buffer[pos], MIDI_COMMAND_PARAM_BYTE_PITCH_BEND);
            
            // parse command values. Pitch bend is 14 bits, spread over two bytes (lower 7 bits each byte).
            int lsb = buffer[pos++];
            local_bytes_read++;
            int msb = buffer[pos++];
            local_bytes_read++;

            int pitch_bend = (msb << 7) | lsb;

            // optional debug print
            if (g_midi_parse_debug)
            {
                memset(print_buffer, 0, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN);

                snprintf(print_buffer, MIDI_PARSE_DEBUG_PRINT_BUFFER_LEN, "%s: pitch bend=%d from lower=%d, upper=%d", MIDI_COMMAND_NAME_PITCH_BEND, pitch_bend, lsb, msb);
                fflush_printf(stdout, "%s\n", print_buffer);
            }

            // save parsed data
            event->command = MIDI_COMMAND_BYTE_PITCH_BEND;
            event->dual = NULL;
            event->cseq_valid = 1;
            event->midi_valid = 1;

            // cseq format
            event->cseq_command_len = MIDI_COMMAND_LEN_PITCH_BEND;
            event->cseq_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_PITCH_BEND;
            event->cseq_command_parameters[0] = lsb;
            event->cseq_command_parameters[1] = msb;
            event->cseq_command_parameters_len = MIDI_COMMAND_NUM_PARAM_PITCH_BEND;

            // convert to MIDI format (same as above)
            memcpy(event->midi_command_parameters_raw, event->cseq_command_parameters_raw, MIDI_COMMAND_PARAM_BYTE_PITCH_BEND);
            event->midi_command_len = MIDI_COMMAND_LEN_PITCH_BEND;
            event->midi_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_PITCH_BEND;
            event->midi_command_parameters[0] = lsb;
            event->midi_command_parameters[1] = msb;
            event->midi_command_parameters_len = MIDI_COMMAND_NUM_PARAM_PITCH_BEND;
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

    struct LinkedListNode *node;
    struct LinkedListNode *new_node;
    struct LinkedListNode *end_node;
    struct GmidEvent *event;
    struct GmidEvent *midi_count_event;
    struct GmidEvent *midi_end_event;
    char *debug_printf_buffer;

    debug_printf_buffer = (char *)malloc_zero(1, WRITE_BUFFER_LEN);

    if (g_midi_debug_loop_delta && g_verbosity >= VERBOSE_DEBUG)
    {
        printf("\n");
        printf("enter GmidTrack_midi_to_cseq_loop\n");
    }

    /**
     * This iterates the list until finding a non standard MIDI loop start.
     * The node is converted to seq format and inserted before current.
     * The next node after the start is then checked. If it's a count node,
     * the end node should exist (according to how gaudio exports), so
     * iterate the event list searching for a matching end event
     * Once found, create a new end event node.
     * 
     * Note: The end event needs an offset to the start event, but this
     * can't be calculated until all events are processed. That is, if
     * there was an inner loop not procssed yet, the outer loop event
     * would have wrong delta offset once the inner loop was inserted.
     * Therefore, end events are created here, but there's a seperate
     * function call to fix up end event offsets (which also requires
     * all in-between delta times be accurate).
    */
    node = gtrack->events->head;
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
                VarLengthInt_copy(&seq_loop_start->cseq_delta_time, &event->midi_delta_time);
                VarLengthInt_copy(&seq_loop_start->midi_delta_time, &event->midi_delta_time);

                if (g_midi_debug_loop_delta && g_verbosity >= VERBOSE_DEBUG)
                {
                    printf("create start event, loop # %d\n", seq_loop_start->cseq_command_parameters[0]);
                }

                // done with the start node, insert new node before current.
                new_node = LinkedListNode_new();
                new_node->data = seq_loop_start;
                LinkedListNode_insert_before(gtrack->events, node, new_node);

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
                            // if this is an end loop event, and it's not claimed by any start loop event
                            if (midi_end_event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE
                                && midi_end_event->midi_command_parameters[0] == MIDI_CONTROLLER_LOOP_END
                                && (midi_end_event->flags & MIDI_MIDI_EVENT_LOOP_END_HANDLED) == 0)
                            {
                                // for the purposes of error handlind, want to track the number of unclaimed
                                // end nodes seen. This loop node only cares if the loop numbers
                                // match though.
                                any_end++;

                                if (midi_end_event->midi_command_parameters[1] == event->midi_command_parameters[1])
                                {
                                    seq_loop_end = GmidEvent_new();

                                    seq_loop_end->dual = seq_loop_start;
                                    seq_loop_start->dual = seq_loop_end;

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
                                    // offset can't be set until all events between the start and end event
                                    // are converted and have delta times set.
                                    seq_loop_end->cseq_command_parameters_len = CSEQ_COMMAND_NUM_PARAM_LOOP_END;

                                    // inherit existing event timing information,
                                    // but overwrite midi->cseq
                                    seq_loop_end->absolute_time = midi_end_event->absolute_time;
                                    VarLengthInt_copy(&seq_loop_end->cseq_delta_time, &midi_end_event->midi_delta_time);
                                    VarLengthInt_copy(&seq_loop_end->midi_delta_time, &midi_end_event->midi_delta_time);

                                    // done with the end node, insert new node before current.
                                    new_node = LinkedListNode_new();
                                    new_node->data = seq_loop_end;
                                    LinkedListNode_insert_before(gtrack->events, end_node, new_node);

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

    free(debug_printf_buffer);

    TRACE_LEAVE(__func__)
}

/**
 * Iterates track list of events and sets seq loop end offset.
 * This requries all loop end events have correct {@code event->dual} pointer
 * set to start event.
 * This requires all delta times be accurate.
 * @param gtrack: Track to set event offsets for.
*/
void GmidTrack_seq_fix_loop_end_delta(struct GmidTrack *gtrack)
{
    TRACE_ENTER(__func__)

    struct LinkedList *fix_end_delta_list;
    struct LinkedListNode *end_node;
    struct LinkedListNode *node;
    struct GmidEvent *event;
    char *debug_printf_buffer;
    int i;

    debug_printf_buffer = (char *)malloc_zero(1, WRITE_BUFFER_LEN);

    fix_end_delta_list = LinkedList_new();

    /**
     * First iterate all events in the track and extract just
     * the seq loop end events.
    */
    node = gtrack->events->head;
    while (node != NULL)
    {
        event = (struct GmidEvent *)node->data;
        if (event != NULL
            && event->cseq_valid
            && event->command == CSEQ_COMMAND_BYTE_LOOP_END_WITH_META
            && (event->flags & MIDI_MALFORMED_EVENT_LOOP) == 0)
        {
            LinkedList_append_node(fix_end_delta_list, LinkedListNode_copy(node));
        }
        node = node->next;
    }

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("GmidTrack_seq_fix_loop_end_delta: will adjust %ld loops\n", fix_end_delta_list->count);
    }

    /**
     * For each loop end event,
     * Find the start event in the track events.
     * Iterate the event list between start and end to compute the byte offset.
     * Then set the end event byte offset to this value.
    */
    end_node = fix_end_delta_list->head;
    while (end_node != NULL)
    {
        // Set the end event from node.
        struct GmidEvent *end_event = (struct GmidEvent *)end_node->data;

        // Declare start node, need to find start event
        struct LinkedListNode *start_node = gtrack->events->head;
        while (start_node != NULL)
        {
            // Search event list until finding start event, based on
            // end event dual pointer.
            struct GmidEvent *start_event = (struct GmidEvent *)start_node->data;
            if (start_event != NULL && start_event == end_event->dual)
            {
                long byte_offset = 0;

                // need to track running status command to determine whether
                // to include command in byte count or not.
                int command = 0;
                int previous_command = -1;

                // Iterate between start event and end event to compute byte offset.
                node = start_node;

                // node is a copy, so compare the thing that matters which is pointer to event.
                while (node != NULL && node->data != end_node->data)
                {
                    struct GmidEvent *iter_event = (struct GmidEvent *)node->data;
                    if (iter_event != NULL)
                    {
                        if (iter_event->cseq_valid)
                        {
                            int inc_amount = 0;
                            int escape_bytes = 0;

                            if (g_midi_debug_loop_delta && g_verbosity >= VERBOSE_DEBUG)
                            {
                                memset(debug_printf_buffer, 0, WRITE_BUFFER_LEN);
                                size_t debug_str_len = GmidEvent_to_string(iter_event, debug_printf_buffer, WRITE_BUFFER_LEN - 2, MIDI_IMPLEMENTATION_SEQ);
                                debug_printf_buffer[debug_str_len] = '\n';
                                fflush_string(stdout, debug_printf_buffer);
                            }

                            for(i=0; i<iter_event->cseq_delta_time.num_bytes; i++)
                            {
                                if (iter_event->cseq_delta_time.value_bytes[i] == 0xfe)
                                {
                                    escape_bytes++;
                                }
                            }

                            if (g_midi_debug_loop_delta && g_verbosity >= VERBOSE_DEBUG && escape_bytes > 0)
                            {
                                printf("loop> + (escape_bytes=%d)\n", escape_bytes);
                            }

                            inc_amount += escape_bytes;

                            command = GmidEvent_get_cseq_command(iter_event);

                            inc_amount +=
                                iter_event->cseq_delta_time.num_bytes
                                + iter_event->cseq_command_parameters_raw_len;

                            // if this is a "running status" then no need to write command, otherwise write command bytes
                            if (previous_command != command)
                            {
                                inc_amount += iter_event->cseq_command_len;
                                previous_command = command;
                            }

                            if (g_midi_debug_loop_delta && g_verbosity >= VERBOSE_DEBUG)
                            {
                                printf("loop> byte_offset=%ld + (inc_amount=%d) = %ld\n", byte_offset, inc_amount, inc_amount+byte_offset);
                            }

                            byte_offset += inc_amount;

                            // only allow running status for the "regular" MIDI commands (commands with channels)
                            if ((command & 0xffffff00) != 0 || (command & 0xfffffff0) >= 0xf0)
                            {
                                previous_command = 0;
                            }
                        }
                    }

                    node = node->next;
                }

                // node is a copy, so compare the thing that matters which is pointer to event.
                if (node->data == end_node->data)
                {
                    int escape_bytes = 0;

                    for(i=0; i<end_event->cseq_delta_time.num_bytes; i++)
                    {
                        if (end_event->cseq_delta_time.value_bytes[i] == 0xfe)
                        {
                            escape_bytes++;
                        }
                    }

                    if (g_midi_debug_loop_delta && g_verbosity >= VERBOSE_DEBUG && escape_bytes > 0)
                    {
                        printf("loop (end event)> + (escape_bytes=%d)\n", escape_bytes);
                    }

                    byte_offset += escape_bytes;
                    byte_offset += CSEQ_COMMAND_LEN_LOOP_END + CSEQ_COMMAND_PARAM_BYTE_LOOP_END;
                    byte_offset += end_event->cseq_delta_time.num_bytes;

                    // The programmer manual says it's from the end of loop end event to the beginning
                    // of loop start (with or without delta? doesn't say), but goldeneye seems
                    // to use the end of the loop start. So adjust by the start event size.
                    byte_offset -= start_event->cseq_delta_time.num_bytes
                        + start_event->cseq_command_parameters_raw_len
                        + start_event->cseq_command_len;

                    if (g_midi_debug_loop_delta && g_verbosity >= VERBOSE_DEBUG)
                    {
                        printf("loop> final byte_offset=%ld\n", byte_offset);
                    }

                    end_event->cseq_command_parameters[2] = byte_offset; // delta to loop start
                    end_event->cseq_command_parameters_raw[2] = (uint8_t)((byte_offset >> 24) & 0xff);
                    end_event->cseq_command_parameters_raw[3] = (uint8_t)((byte_offset >> 16) & 0xff);
                    end_event->cseq_command_parameters_raw[4] = (uint8_t)((byte_offset >> 8) & 0xff);
                    end_event->cseq_command_parameters_raw[5] = (uint8_t)((byte_offset >> 0) & 0xff);
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> lost reference to end node\n", __func__, __LINE__);
                }

                // done with this end event
                break;
            }
            start_node = start_node->next;
        }
        end_node = end_node->next;
    }

    LinkedList_free(fix_end_delta_list);

    free(debug_printf_buffer);

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

    struct LinkedListNode *node;
    struct GmidEvent *event;

    gtrack->cseq_track_size_bytes = 0;
    gtrack->midi_track_size_bytes = 0;

    node = gtrack->events->head;
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
 * Iterate the event list of a track, and for each seq meta loop start event and loop
 * end event, ensure there is a matching reference and set the {@code GmidEvent->dual} pointer to it.
 * If the event already has a dual pointer set, no changes are made to that event.
 * All remaining loop events will be flagged with {@code MIDI_MALFORMED_EVENT_LOOP}.
 * This method needs to be run before any changes are made to
 * the event list, otherwise offsets will be incorrect and this can fail.
*/
void GmidTrack_pair_cseq_loop_events(struct GmidTrack *gtrack, struct LinkedList *patterns)
{
    TRACE_ENTER(__func__)

    if (gtrack == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> gtrack is NULL\n", __func__, __LINE__);
    }

    struct LinkedListNode *node;
    struct LinkedListNode *search_node;
    struct GmidEvent *event;
    struct GmidEvent *search_event;
    struct LinkedList *duplicate_start;
    struct LinkedListNode *duplicate_node;
    struct GmidEvent *duplicate_event;

    // Iterate the list of events and try to match start events to end events.
    node = gtrack->events->head;
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

                search_node = node->next;
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
                                    duplicate_start = LinkedList_new();
                                }

                                duplicate_node = LinkedListNode_new();
                                duplicate_node->data = search_event;
                                LinkedList_append_node(duplicate_start, duplicate_node);

                                if (g_verbosity >= VERBOSE_DEBUG)
                                {
                                    printf("found duplicate loop start event for loop %d\n", search_event->cseq_command_parameters[0]);
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
                            int unroll_adjust = 0;

                            any_end++;

                            if (patterns != NULL)
                            {
                                // escape byte sequence can be in the delta time of the loop end event.
                                // Example: Facility track 6.
                                unroll_adjust = measure_unroll_adjust(patterns, event->file_offset, search_event->file_offset + search_event->cseq_delta_time.num_bytes);
                            }

                            fixed_delta = search_event->cseq_command_parameters[2];
                            compare_delta = (int32_t)(
                                search_event->file_offset
                                + search_event->cseq_delta_time.num_bytes
                                + search_event->cseq_command_parameters_raw_len
                                - event->file_offset
                                - event->cseq_delta_time.num_bytes
                                - event->cseq_command_parameters_raw_len);
                            if (fixed_delta + unroll_adjust == compare_delta)
                            {
                                event->dual = search_event;
                                search_event->dual = event;

                                match = 1;
                            }
                            else
                            {
                                if (g_verbosity >= VERBOSE_DEBUG)
                                {
                                    printf("Found loop end event, but wrong offset. Event value=%d, actual offset=%d, unroll_adjust=%d\n", fixed_delta, compare_delta, unroll_adjust);
                                }
                            }

                            // if the starting node didn't match, check to see if any others did
                            if (match == 0 && duplicate_start != NULL)
                            {
                                duplicate_node = duplicate_start->head;
                                while (duplicate_node != NULL && match == 0)
                                {
                                    duplicate_event = (struct GmidEvent *)duplicate_node->data;

                                    unroll_adjust = 0;
                                    if (patterns != NULL)
                                    {
                                        // escape byte sequence can be in the delta time of the loop end event.
                                        // Example: Facility track 6.
                                        unroll_adjust = measure_unroll_adjust(patterns, event->file_offset, duplicate_event->file_offset + duplicate_event->cseq_delta_time.num_bytes);
                                    }

                                    fixed_delta = search_event->cseq_command_parameters[2];
                                    compare_delta = (int32_t)(
                                        search_event->file_offset
                                        + search_event->cseq_delta_time.num_bytes
                                        + search_event->cseq_command_parameters_raw_len
                                        - duplicate_event->file_offset
                                        - duplicate_event->cseq_delta_time.num_bytes
                                        - duplicate_event->cseq_command_parameters_raw_len);
                                    if (fixed_delta + unroll_adjust == compare_delta)
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
                    LinkedList_free(duplicate_start);
                    duplicate_start = NULL;
                }

                // Generally try to avoid throwing exception, should be able to
                // just flag events as bad.
                if (match == 0 && g_strict_cseq_loop_event)
                {
                    /**
                     * Archives channel 7.
                     * DeathSolo, track 6. Start loop 0, start loop 0, end loop 0 (first), end loop offset 0x00ffffff.
                     * DeathSolo, track 8. Start loop 0, start loop 0, end loop 0 (first), end loop offset 0x00ffffff.
                     * ....
                    */
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> can not find seq loop end event for loop start event at file offset %ld\n", __func__, __LINE__, event->file_offset);
                }
            }
        }

        node = node->next;
    }

    // Iterate list and flag any leftover events as bad.
    node = gtrack->events->head;
    while (node != NULL)
    {
        event = (struct GmidEvent *)node->data;
        if (event != NULL)
        {
            if (event->dual == NULL
                && (
                    event->command == CSEQ_COMMAND_BYTE_LOOP_START_WITH_META
                    || event->command == CSEQ_COMMAND_BYTE_LOOP_END_WITH_META
                )
            )
            {
                if (g_verbosity >= 1)
                {
                    if (event->command == CSEQ_COMMAND_BYTE_LOOP_START_WITH_META)
                    {
                        fflush_printf(
                            stderr,
                            "bad seq loop start. seq track %d, offset %ld, absolute time %ld, delta %d, loop number %d\n",
                            gtrack->cseq_track_index,
                            event->file_offset,
                            event->absolute_time,
                            event->cseq_delta_time.standard_value,
                            event->cseq_command_parameters[0]);
                    }
                    else if (event->command == CSEQ_COMMAND_BYTE_LOOP_END_WITH_META)
                    {
                        fflush_printf(
                            stderr,
                            "bad seq loop end. seq track %d, offset %ld, absolute time %ld, delta %d, loop count %d, loop count2 %d, loop start offset %d\n",
                            gtrack->cseq_track_index,
                            event->file_offset,
                            event->absolute_time,
                            event->cseq_delta_time.standard_value,
                            event->cseq_command_parameters[0],
                            event->cseq_command_parameters[1],
                            event->cseq_command_parameters[2]);
                    }
                    else
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "%s %d> event command 0x%04x not supported\n", event->command);
                    }
                }

                event->flags |= MIDI_MALFORMED_EVENT_LOOP;
            }
        }
        node = node->next;
    }

    TRACE_LEAVE(__func__)
}

/**
 * Iterates list of events and a new custom gaudio sysex event is created
 * to capture malformed seq loop events. These are placed prior to the seq
 * loop events. The seq loop events are then no longer treated as valid
 * in the event list ({@code cseq_valid} is cleared).
 * This must be run after {@code GmidTrack_pair_cseq_loop_events}.
 * @param gtrack: track to update.
 * @param no_create: When set, will only clear {@code cseq_valid} on invalid events,
 * no sysex events will be created.
*/
void GmidTrack_invalid_cseq_loop_to_sysex(struct GmidTrack *gtrack, int no_create)
{
    TRACE_ENTER(__func__)

    if (gtrack == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> gtrack is NULL\n", __func__, __LINE__);
    }

    struct LinkedListNode *node;
    struct LinkedListNode *sysex_node;
    struct GmidEvent *event;
    struct GmidEvent *sysex_event;
    uint8_t b;
    int i;
    int source_index, dest_index;

    node = gtrack->events->head;
    while (node != NULL)
    {
        event = (struct GmidEvent *)node->data;
        if (event != NULL
            && event->dual == NULL
            && (event->flags & MIDI_MALFORMED_EVENT_LOOP) > 0
            && (
                event->command == CSEQ_COMMAND_BYTE_LOOP_START_WITH_META
                || event->command == CSEQ_COMMAND_BYTE_LOOP_END_WITH_META
            )
        )
        {
            // Invalidate base seq event.
            event->cseq_valid = 0;
            event->midi_valid = 0;

            if (no_create == 0)
            {
                if (event->command == CSEQ_COMMAND_BYTE_LOOP_START_WITH_META)
                {
                    // copy values shared by start and end
                    sysex_event = GmidEvent_new();
                    sysex_event->command = MIDI_COMMAND_BYTE_SYSEX_START;
                    sysex_event->midi_command_len = MIDI_COMMAND_LEN_SYSEX;
                    sysex_event->midi_valid = 1;
                    sysex_event->command_channel = -1;
                    sysex_event->absolute_time = event->absolute_time;
                    sysex_event->file_offset = event->file_offset;
                    sysex_event->flags = event->flags;
                    VarLengthInt_copy(&sysex_event->cseq_delta_time, &event->cseq_delta_time);
                    VarLengthInt_copy(&sysex_event->midi_delta_time, &event->midi_delta_time);

                    // sysex start loop values
                    sysex_event->midi_command_parameters_len = MIDI_COMMAND_NUM_PARAM_SYSEX_SEQ_LOOP_START;
                    sysex_event->midi_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_SYSEX_SEQ_LOOP_START;
                    sysex_event->midi_command_parameters[0] = MIDI_COMMAND_BYTE_SYSEX_UNIVERSAL_NON_COMMERICIAL;
                    sysex_event->midi_command_parameters_raw[0] = MIDI_COMMAND_BYTE_SYSEX_UNIVERSAL_NON_COMMERICIAL;
                    sysex_event->midi_command_parameters[1] = MIDI_COMMAND_BYTE_SYSEX_GAUDIO_PREFIX;
                    sysex_event->midi_command_parameters_raw[1] = MIDI_COMMAND_BYTE_SYSEX_GAUDIO_PREFIX;
                    sysex_event->midi_command_parameters[2] = CSEQ_COMMAND_BYTE_LOOP_START;
                    sysex_event->midi_command_parameters_raw[2] = CSEQ_COMMAND_BYTE_LOOP_START;

                    source_index = 0;
                    dest_index = 3;
                    for (i=0; i<1; i++)
                    {
                        // loop number
                        b = (event->cseq_command_parameters_raw[source_index] & 0xf0) >> 4;
                        sysex_event->midi_command_parameters[dest_index] = b;
                        sysex_event->midi_command_parameters_raw[dest_index] = b;
                        dest_index++;

                        b = (event->cseq_command_parameters_raw[source_index] & 0x0f);
                        sysex_event->midi_command_parameters[dest_index] = b;
                        sysex_event->midi_command_parameters_raw[dest_index] = b;
                        dest_index++;

                        source_index++;
                    }

                    sysex_event->midi_command_parameters[5] = MIDI_COMMAND_BYTE_SYSEX_END;
                    sysex_event->midi_command_parameters_raw[5] = MIDI_COMMAND_BYTE_SYSEX_END;

                    // add to event list
                    sysex_node = LinkedListNode_new();
                    sysex_node->data = sysex_event;
                    LinkedListNode_insert_before(gtrack->events, node, sysex_node);
                }
                else if (event->command == CSEQ_COMMAND_BYTE_LOOP_END_WITH_META)
                {
                    // copy values shared by start and end
                    sysex_event = GmidEvent_new();
                    sysex_event->command = MIDI_COMMAND_BYTE_SYSEX_START;
                    sysex_event->midi_command_len = MIDI_COMMAND_LEN_SYSEX;
                    sysex_event->midi_valid = 1;
                    sysex_event->command_channel = -1;
                    sysex_event->absolute_time = event->absolute_time;
                    sysex_event->file_offset = event->file_offset;
                    sysex_event->flags = event->flags;
                    VarLengthInt_copy(&sysex_event->cseq_delta_time, &event->cseq_delta_time);
                    VarLengthInt_copy(&sysex_event->midi_delta_time, &event->midi_delta_time);

                    // sysex start loop values
                    sysex_event->midi_command_parameters_len = MIDI_COMMAND_NUM_PARAM_SYSEX_SEQ_LOOP_END;
                    sysex_event->midi_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_SYSEX_SEQ_LOOP_END;
                    sysex_event->midi_command_parameters[0] = MIDI_COMMAND_BYTE_SYSEX_UNIVERSAL_NON_COMMERICIAL;
                    sysex_event->midi_command_parameters_raw[0] = MIDI_COMMAND_BYTE_SYSEX_UNIVERSAL_NON_COMMERICIAL;
                    sysex_event->midi_command_parameters[1] = MIDI_COMMAND_BYTE_SYSEX_GAUDIO_PREFIX;
                    sysex_event->midi_command_parameters_raw[1] = MIDI_COMMAND_BYTE_SYSEX_GAUDIO_PREFIX;
                    sysex_event->midi_command_parameters[2] = CSEQ_COMMAND_BYTE_LOOP_END;
                    sysex_event->midi_command_parameters_raw[2] = CSEQ_COMMAND_BYTE_LOOP_END;

                    source_index = 0;
                    dest_index = 3;
                    for (i=0; i<6; i++)
                    {
                        // source byte[0]: loop count
                        // source byte[1]: loop count2
                        // source byte[2-5]: loop end offset to loop start
                        b = (event->cseq_command_parameters_raw[source_index] & 0xf0) >> 4;
                        sysex_event->midi_command_parameters[dest_index] = b;
                        sysex_event->midi_command_parameters_raw[dest_index] = b;
                        dest_index++;

                        b = (event->cseq_command_parameters_raw[source_index] & 0x0f);
                        sysex_event->midi_command_parameters[dest_index] = b;
                        sysex_event->midi_command_parameters_raw[dest_index] = b;
                        dest_index++;

                        source_index++;
                    }

                    sysex_event->midi_command_parameters[15] = MIDI_COMMAND_BYTE_SYSEX_END;
                    sysex_event->midi_command_parameters_raw[15] = MIDI_COMMAND_BYTE_SYSEX_END;

                    // add to event list
                    sysex_node = LinkedListNode_new();
                    sysex_node->data = sysex_event;
                    LinkedListNode_insert_before(gtrack->events, node, sysex_node);
                }
            }
        }

        node = node->next;
    }

    TRACE_LEAVE(__func__)
}

/**
 * This iterates the event list and each MIDI gaudio sysex event 
 * is escaped to the cseq event it captures.
 * @param gtrack: track to update.
*/
void GmidTrack_midi_sysex_to_cseq(struct GmidTrack *gtrack)
{
    TRACE_ENTER(__func__)

    if (gtrack == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> gtrack is NULL\n", __func__, __LINE__);
    }

    struct LinkedListNode *node;
    struct LinkedListNode *new_node;
    struct GmidEvent *event;
    struct GmidEvent *new_event;
    int pos;
    int i;

    node = gtrack->events->head;
    while (node != NULL)
    {
        event = (struct GmidEvent *)node->data;
        if (event != NULL
            // Only evaluate events marked as valid.
            && event->midi_valid == 1
            // and sysex event
            && event->command == MIDI_COMMAND_BYTE_SYSEX_START)
        {
            /**
             * The event was previoulsy parsed, so it is assumed the [0] byte is 0xd7,
             * the [1] byte is MIDI_COMMAND_BYTE_SYSEX_GAUDIO_PREFIX,
             * and [2] byte is the start of a cseq event
            */

            if (event->midi_command_parameters_raw[2] == CSEQ_COMMAND_BYTE_LOOP_START)
            {
                int loop_number = 0;

                pos = 3;
                loop_number = event->midi_command_parameters_raw[pos++];
                loop_number <<= 4;
                loop_number |= event->midi_command_parameters_raw[pos++];

                new_event = GmidEvent_new();

                new_event->absolute_time = event->absolute_time;
                VarLengthInt_copy(&new_event->cseq_delta_time, &event->cseq_delta_time);
                VarLengthInt_copy(&new_event->midi_delta_time, &event->midi_delta_time);

                new_event->flags |= MIDI_MALFORMED_EVENT_LOOP;

                new_event->command = CSEQ_COMMAND_BYTE_LOOP_START_WITH_META;
                new_event->dual = NULL;
                new_event->cseq_valid = 1;
                new_event->midi_valid = 0;

                // cseq format
                new_event->cseq_command_len = CSEQ_COMMAND_LEN_LOOP_START;
                new_event->cseq_command_parameters_raw_len = CSEQ_COMMAND_PARAM_BYTE_LOOP_START;
                new_event->cseq_command_parameters[0] = loop_number;
                new_event->cseq_command_parameters_raw[0] = loop_number;
                new_event->cseq_command_parameters[1] = 0xff;
                new_event->cseq_command_parameters_raw[1] = 0xff;
                new_event->cseq_command_parameters_len = CSEQ_COMMAND_NUM_PARAM_LOOP_START;

                // insert in event list, before corresponding sysex event.
                new_node = LinkedListNode_new();
                new_node->data = new_event;
                LinkedListNode_insert_before(gtrack->events, node, new_node);
            }
            else if (event->midi_command_parameters_raw[2] == CSEQ_COMMAND_BYTE_LOOP_END)
            {
                int loop_count = 0;
                int current_loop_count = 0;
                int32_t loop_difference = 0;

                pos = 3;
                loop_count = event->midi_command_parameters_raw[pos++];
                loop_count <<= 4;
                loop_count |= event->midi_command_parameters_raw[pos++];

                current_loop_count = event->midi_command_parameters_raw[pos++];
                current_loop_count <<= 4;
                current_loop_count |= event->midi_command_parameters_raw[pos++];

                for (i=0; i<7; i++)
                {
                    loop_difference |= event->midi_command_parameters_raw[pos++];
                    loop_difference <<= 4;
                }
                loop_difference |= event->midi_command_parameters_raw[pos++];

                new_event = GmidEvent_new();

                new_event->absolute_time = event->absolute_time;
                VarLengthInt_copy(&new_event->cseq_delta_time, &event->cseq_delta_time);
                VarLengthInt_copy(&new_event->midi_delta_time, &event->midi_delta_time);

                new_event->flags |= MIDI_MALFORMED_EVENT_LOOP;

                // save parsed data
                new_event->command = CSEQ_COMMAND_BYTE_LOOP_END_WITH_META;
                new_event->dual = NULL;
                new_event->cseq_valid = 1;
                new_event->midi_valid = 0;

                // cseq format
                new_event->cseq_command_len = CSEQ_COMMAND_LEN_LOOP_END;
                new_event->cseq_command_parameters_raw_len = CSEQ_COMMAND_PARAM_BYTE_LOOP_END;
                new_event->cseq_command_parameters[0] = loop_count;
                new_event->cseq_command_parameters[1] = current_loop_count;
                new_event->cseq_command_parameters[2] = loop_difference;
                new_event->cseq_command_parameters_len = CSEQ_COMMAND_NUM_PARAM_LOOP_END;

                new_event->cseq_command_parameters_raw[0] = loop_count;
                new_event->cseq_command_parameters_raw[1] = current_loop_count;
                new_event->cseq_command_parameters_raw[2] = (loop_difference >> 24) & 0xff;
                new_event->cseq_command_parameters_raw[3] = (loop_difference >> 16) & 0xff;
                new_event->cseq_command_parameters_raw[4] = (loop_difference >> 8) & 0xff;
                new_event->cseq_command_parameters_raw[5] = (loop_difference >> 0) & 0xff;

                // insert in event list, before corresponding sysex event.
                new_node = LinkedListNode_new();
                new_node->data = new_event;
                LinkedListNode_insert_before(gtrack->events, node, new_node);
            }
            else
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> parse error -- unsupport sysex command 0x%0x%0x%0x.\n", __func__, __LINE__, event->midi_command_parameters_raw[0], event->midi_command_parameters_raw[1], event->midi_command_parameters_raw[2]);
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
 * Note: {@code GmidTrack_pair_cseq_loop_events} should be called
 * prior to this, or {@code MIDI_MALFORMED_EVENT_LOOP} needs to be
 * set in start event flag that doesn't have end event.
 * @param gtrack: track to update.
*/
void GmidTrack_cseq_to_midi_loop(struct GmidTrack *gtrack)
{
    TRACE_ENTER(__func__)

    if (gtrack == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> gtrack is NULL\n", __func__, __LINE__);
    }

    struct LinkedListNode *node;
    struct LinkedListNode *new_node;
    struct LinkedListNode *search_node;
    struct GmidEvent *seq_event;
    struct GmidEvent *seq_end_event;
    struct GmidEvent *midi_start_event;
    struct GmidEvent *midi_count_event;
    struct GmidEvent *midi_end_event;

    // seq loop is a meta event so there's no channel information, but
    // that should be included in the command.
    uint8_t track_channel = gtrack->cseq_track_index & 0xf;

    node = gtrack->events->head;
    while (node != NULL)
    {
        seq_event = (struct GmidEvent *)node->data;
        if (seq_event != NULL
            // Only evaluate events marked as valid. The GmidTrack_invalid_cseq_loop_to_sysex
            // method might have filtered out events that should now be excluded from export.
            && seq_event->cseq_valid == 1
            // and loop start event
            && seq_event->command == CSEQ_COMMAND_BYTE_LOOP_START_WITH_META)
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
            midi_start_event->command_channel = track_channel;

            midi_start_event->midi_command_len = MIDI_COMMAND_LEN_CONTROL_CHANGE;
            midi_start_event->midi_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_CONTROL_CHANGE;
            midi_start_event->midi_command_parameters[0] = MIDI_CONTROLLER_LOOP_START;
            midi_start_event->midi_command_parameters[1] = channel_unique_loop_number;
            midi_start_event->midi_command_parameters_len = MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
            midi_start_event->midi_command_parameters_raw[0] = MIDI_CONTROLLER_LOOP_START;
            midi_start_event->midi_command_parameters_raw[1] = channel_unique_loop_number;

            // Start event without end event. Set time then insert and continue.
            // Note this is separate from cseq_valid value. 
            if ((seq_event->flags & MIDI_MALFORMED_EVENT_LOOP) > 0)
            {
                // set absolute times
                midi_start_event->absolute_time = seq_event->absolute_time;

                // insert in event list, before corresponding seq event.
                new_node = LinkedListNode_new();
                new_node->data = midi_start_event;
                LinkedListNode_insert_before(gtrack->events, node, new_node);
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
                midi_count_event->command_channel = track_channel;

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
                midi_end_event->command_channel = track_channel;

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
                new_node = LinkedListNode_new();
                new_node->data = midi_start_event;
                LinkedListNode_insert_before(gtrack->events, node, new_node);
                new_node = LinkedListNode_new();
                new_node->data = midi_count_event;
                LinkedListNode_insert_before(gtrack->events, node, new_node);

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

                new_node = LinkedListNode_new();
                new_node->data = midi_end_event;
                LinkedListNode_insert_before(gtrack->events, search_node, new_node);
            }
        }

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

    struct LinkedListNode *node;
    long cseq_prev_absolute_time = 0;
    long midi_prev_absolute_time = 0;
    int cseq_time_delta;
    int midi_time_delta;

    node = gtrack->events->head;
    while (node != NULL)
    {
        struct GmidEvent *current_node_event = (struct GmidEvent *)node->data;

        cseq_time_delta = 0;
        midi_time_delta = 0;

        // set the event cseq event time
        if (current_node_event->cseq_valid)
        {
            cseq_time_delta = (int)(current_node_event->absolute_time - cseq_prev_absolute_time);
            int32_to_VarLengthInt(cseq_time_delta, &current_node_event->cseq_delta_time);
            cseq_prev_absolute_time = current_node_event->absolute_time;
            // do not set midi delta time here.
        }

        // set the event MIDI event time
        if (current_node_event->midi_valid)
        {
            midi_time_delta = (int)(current_node_event->absolute_time - midi_prev_absolute_time);
            int32_to_VarLengthInt(midi_time_delta, &current_node_event->midi_delta_time);
            midi_prev_absolute_time = current_node_event->absolute_time;
            // do not set cseq delta time here.
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

    struct LinkedListNode *node;
    struct GmidEvent *event;

    node = gtrack->events->head;
    while (node != NULL)
    {
        event = (struct GmidEvent *)node->data;

        if (event->command == MIDI_COMMAND_BYTE_NOTE_ON)
        {
            struct GmidEvent *noteoff = GmidEvent_new();
            struct LinkedListNode *temp_node;

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

            temp_node = LinkedListNode_new();
            temp_node->data = noteoff;
            
            // Don't want to append the temp_node anywhere forward in the list since
            // that's still being iterated. Therefore just insert before current.
            LinkedListNode_insert_before(gtrack->events, node, temp_node);
        }

        node = node->next;
    }

    TRACE_LEAVE(__func__)
}

/**
 * This iterates the event in a track and updates note-on events
 * to set the duration used by seq note-on. The {@code event->cseq_valid}
 * is changed from zero to one.
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
    struct LinkedListNode *node;
    struct LinkedListNode *node_off;
    struct VarLengthInt varint;

    char *debug_printf_buffer;
    debug_printf_buffer = (char *)malloc_zero(1, WRITE_BUFFER_LEN);

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("begin %s\n", __func__);
    }

    node = gtrack->events->head;
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

                /**
                 * Count the depth of duplicate note-on events. Treat this as a stack.
                 * For example, three duplicate note-on events followed by one
                 * note-off, the note-off will be matched to the last note-on.
                */
                int duplicate_stack = 0;

                node_off = node;
                while (node_off != NULL && match == 0)
                {
                    event_off = (struct GmidEvent *)node_off->data;
                    if (event_off != NULL)
                    {
                        // if this is a note-on event
                        if (event_off->command == MIDI_COMMAND_BYTE_NOTE_ON
                            // velocity == 0 means implicit note-off, so check this is positive
                            && event_off->midi_command_parameters[1] > 0
                            // and this is for the same note
                            && event_off->midi_command_parameters[0] == event->midi_command_parameters[0])
                        {
                            // increase stack depth of duplicates.
                            duplicate_stack++;
                        }
                        else if (
                            // this is note off event, or implicit note-off event
                            (
                                (event_off->command == MIDI_COMMAND_BYTE_NOTE_OFF)
                                || (event_off->command == MIDI_COMMAND_BYTE_NOTE_ON && event_off->midi_command_parameters[1] == 0)
                            )
                            // and this is for the same note
                            && event_off->midi_command_parameters[0] == event->midi_command_parameters[0]
                        )
                        {
                            // decrase stack depth of duplicates.
                            duplicate_stack--;

                            if (duplicate_stack == 0)
                            {
                                long absolute_delta = event_off->absolute_time - event->absolute_time;
                                memset(&varint, 0, sizeof(struct VarLengthInt));
                                int32_to_VarLengthInt((int32_t)absolute_delta, &varint);

                                // set "easy access" value for duration.
                                // Offset 0 and 1 are note and velocity, so start at 2.
                                event->cseq_command_parameters[2] = (int32_t)absolute_delta;
                                // write duration into cseq parameters.
                                VarLengthInt_write_value_big(&event->cseq_command_parameters_raw[2], &varint);
                                // update byte length of parameters to write.
                                event->cseq_command_parameters_raw_len += varint.num_bytes;
                                // flag cseq now as valid.
                                event->cseq_valid = 1;

                                match = 1;

                                if (g_verbosity >= VERBOSE_DEBUG)
                                {
                                    int32_t varint_value = VarLengthInt_get_value_big(&varint);
                                    printf("set valid cseq note on, duration=%ld, varint=0x%08x\n", absolute_delta, varint_value);
                                    
                                    memset(debug_printf_buffer, 0, WRITE_BUFFER_LEN);
                                    size_t debug_str_len = GmidEvent_to_string(event, debug_printf_buffer, WRITE_BUFFER_LEN - 2, MIDI_IMPLEMENTATION_SEQ);
                                    debug_printf_buffer[debug_str_len] = '\n';
                                    fflush_string(stdout, debug_printf_buffer);
                                }
                            }
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

    free(debug_printf_buffer);

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("end %s\n", __func__);
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
    struct LinkedListNode *node;
    size_t track_pos;
    size_t buffer_len;
    int32_t command;
    char *debug_printf_buffer;

    debug_printf_buffer = (char *)malloc_zero(1, WRITE_BUFFER_LEN);

    /** 
     * running count of absolute time.
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

        if (g_verbosity >= VERBOSE_DEBUG)
        {
            memset(debug_printf_buffer, 0, WRITE_BUFFER_LEN);
            size_t debug_str_len = GmidEvent_to_string(event, debug_printf_buffer, WRITE_BUFFER_LEN - 2, MIDI_IMPLEMENTATION_SEQ);
            debug_printf_buffer[debug_str_len] = '\n';
            fflush_string(stdout, debug_printf_buffer);
        }

        command = GmidEvent_get_cseq_command(event);

        // only allow running status for the "regular" MIDI commands (commands with channels)
        if ((command & 0xffffff00) != 0 || (command & 0xfffffff0) >= 0xf0)
        {
            command = 0;
        }

        node = LinkedListNode_new();
        node->data = event;
        // The command channel in the event specifies the track number, so
        // add the new event to the appropriate track linked list.
        LinkedList_append_node(gtrack->events, node);
    }

    free(debug_printf_buffer);

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

    if (result == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> result is NULL\n", __func__, __LINE__);
    }

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

    if (result == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> result is NULL\n", __func__, __LINE__);
    }

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
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> note=%d note supported.\n", __func__, __LINE__, note);
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

    if (event == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> event is NULL\n", __func__, __LINE__);
    }

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
    else if (event->command == MIDI_COMMAND_BYTE_SYSEX_START)
    {
        TRACE_LEAVE(__func__)
        return MIDI_COMMAND_BYTE_SYSEX_START;
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

    stderr_exit(EXIT_CODE_GENERAL, "%s %d> midi command not supported: 0x%04x.\n", __func__, __LINE__, event->command);

    TRACE_LEAVE(__func__)

    // be quiet gcc
    return -1;
}

/**
 * Converts command from event into seq command, adding the channel number.
 * (The stored command doesn't include channel in the command.)
 * The command paramter evaluated is the "full" seq command (without channel),
 * this contains logic to adjust differences from MIDI to cseq.
 * @param event: event to build seq command from.
 * @returns: seq command with channel.
*/
int32_t GmidEvent_get_cseq_command(struct GmidEvent *event)
{
    TRACE_ENTER(__func__)

    if (event == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> event is NULL\n", __func__, __LINE__);
    }

    /**
     * Maybe refactor this to split yet another parameter into two properties ...
    */

    int upper = (0xff00 & event->command) >> 8;

    if (event->command == CSEQ_COMMAND_BYTE_NOTE_ON)
    {
        TRACE_LEAVE(__func__)
        return CSEQ_COMMAND_BYTE_NOTE_ON | event->command_channel;
    }
    // shared with midi
    else if (event->command == MIDI_COMMAND_BYTE_POLYPHONIC_PRESSURE)
    {
        TRACE_LEAVE(__func__)
        return MIDI_COMMAND_BYTE_POLYPHONIC_PRESSURE | event->command_channel;
    }
    // shared with midi
    else if (event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE)
    {
        TRACE_LEAVE(__func__)
        return MIDI_COMMAND_BYTE_CONTROL_CHANGE | event->command_channel;
    }
    // shared with midi
    else if (event->command == MIDI_COMMAND_BYTE_PROGRAM_CHANGE)
    {
        TRACE_LEAVE(__func__)
        return MIDI_COMMAND_BYTE_PROGRAM_CHANGE | event->command_channel;
    }
    // shared with midi
    else if (event->command == MIDI_COMMAND_BYTE_CHANNEL_PRESSURE)
    {
        TRACE_LEAVE(__func__)
        return MIDI_COMMAND_BYTE_CHANNEL_PRESSURE | event->command_channel;
    }
    // shared with midi
    else if (event->command == MIDI_COMMAND_BYTE_PITCH_BEND)
    {
        TRACE_LEAVE(__func__)
        return MIDI_COMMAND_BYTE_PITCH_BEND | event->command_channel;
    }
    // shared with midi
    else if (event->command == MIDI_COMMAND_BYTE_SYSEX_START)
    {
        TRACE_LEAVE(__func__)
        return MIDI_COMMAND_BYTE_SYSEX_START;
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
            case CSEQ_COMMAND_BYTE_END_OF_TRACK:
            TRACE_LEAVE(__func__)
            return event->command;
        }
    }

    stderr_exit(EXIT_CODE_GENERAL, "%s %d> seq command not supported: 0x%04x.\n", __func__, __LINE__, event->command);

    TRACE_LEAVE(__func__)

    // be quiet gcc
    return -1;
}

/**
 * Iterates {@code struct GmidTrack} events list and writes to out buffer
 * in standard MIDI format. This is writing the track data without the track header.
 * This updates {@code file_offset} to position written within the track.
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
    struct LinkedListNode *node = gtrack->events->head;
    uint8_t rev[4];
    int32_t command;

    for (; node != NULL && write_len < max_len; node = node->next)
    {
        struct GmidEvent *event = (struct GmidEvent *)node->data;

        if (!event->midi_valid)
        {
            continue;
        }

        event->file_offset = write_len;

        command = GmidEvent_get_midi_command(event);

        if (event->midi_delta_time.num_bytes == 0)
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> invalid midi delta time (0)\n", __func__, __LINE__);
        }

        // write varint delta time value
        VarLengthInt_write_value_big(&buffer[write_len], &event->midi_delta_time);
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

        // only allow running status for the "regular" MIDI commands (commands with channels)
        if ((command & 0xffffff00) != 0 || (command & 0xfffffff0) >= 0xf0)
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
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> write_len %ld exceeded max_len %ld when writing to buffer.\n", __func__, __LINE__, write_len, max_len);
    }

    TRACE_LEAVE(__func__)

    return write_len;
}

/**
 * Iterates {@code struct GmidTrack} events list and writes to out buffer
 * in seq format. This is writing the track data without the track header.
 * This updates {@code file_offset} to position written within the track.
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
    struct LinkedListNode *node = gtrack->events->head;
    uint8_t rev[4];
    int32_t command;

    for (; node != NULL && write_len < max_len; node = node->next)
    {
        struct GmidEvent *event = (struct GmidEvent *)node->data;

        if (!event->cseq_valid)
        {
            continue;
        }

        event->file_offset = write_len;

        command = GmidEvent_get_cseq_command(event);

        if (event->cseq_delta_time.num_bytes == 0)
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> invalid midi delta time (0)\n", __func__, __LINE__);
        }

        // write varint delta time value
        VarLengthInt_write_value_big(&buffer[write_len], &event->cseq_delta_time);
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

        // only allow running status for the "regular" MIDI commands (commands with channels)
        if ((command & 0xffffff00) != 0 || (command & 0xfffffff0) >= 0xf0)
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
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> write_len %ld exceeded max_len %ld when writing to buffer.\n", __func__, __LINE__, write_len, max_len);
    }

    TRACE_LEAVE(__func__)

    return write_len;
}

/**
 * Writes the MIDI track to disk.
 * @param midi_file: MIDI to write.
 * @param fi: File handle to write to, using current offset.
*/
void MidiTrack_fwrite(struct MidiTrack *track, struct FileInfo *fi)
{
    TRACE_ENTER(__func__)

    if (track == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> track is NULL\n", __func__, __LINE__);
    }

    if (fi == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> fi is NULL\n", __func__, __LINE__);
    }

    FileInfo_fwrite_bswap(fi, &track->ck_id, 4, 1);
    FileInfo_fwrite_bswap(fi, &track->ck_data_size, 4, 1);

    FileInfo_fwrite(fi, track->data, track->ck_data_size, 1);

    TRACE_LEAVE(__func__)
}

/**
 * Writes the full {@code struct MidiFile} to disk, and all child elements.
 * @param midi_file: MIDI to write.
 * @param fi: File handle to write to, using current offset.
*/
void MidiFile_fwrite(struct MidiFile *midi_file, struct FileInfo *fi)
{
    TRACE_ENTER(__func__)

    if (midi_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> midi_file is NULL\n", __func__, __LINE__);
    }

    if (fi == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> fi is NULL\n", __func__, __LINE__);
    }

    int i;

    FileInfo_fwrite_bswap(fi, &midi_file->ck_id, 4, 1);
    FileInfo_fwrite_bswap(fi, &midi_file->ck_data_size, 4, 1);
    FileInfo_fwrite_bswap(fi, &midi_file->format, 2, 1);
    FileInfo_fwrite_bswap(fi, &midi_file->num_tracks, 2, 1);
    FileInfo_fwrite_bswap(fi, &midi_file->division, 2, 1);

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
void CseqFile_fwrite(struct CseqFile *cseq, struct FileInfo *fi)
{
    TRACE_ENTER(__func__)

    if (cseq == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> cseq is NULL\n", __func__, __LINE__);
    }

    if (fi == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> fi is NULL\n", __func__, __LINE__);
    }

    int i;
    for (i=0; i<CSEQ_FILE_NUM_TRACKS; i++)
    {
        FileInfo_fwrite_bswap(fi, &cseq->track_offset[i], 4, 1);
    }

    FileInfo_fwrite_bswap(fi, &cseq->division, 4, 1);
    FileInfo_fwrite(fi, cseq->compressed_data, cseq->compressed_data_len, 1);

    TRACE_LEAVE(__func__)
}

/**
 * Write the event to string in standard notation.
 * @param event: event to write.
 * @param buffer: buffer to write into.
 * @param buffer_len: size of buffer in bytes.
 * @param type: which values of event to retrieve.
 * @returns: size of string, without '\0' terminator.
*/
size_t GmidEvent_to_string(struct GmidEvent *event, char *buffer, size_t bufer_len, enum MIDI_IMPLEMENTATION type)
{
    TRACE_ENTER(__func__)

    if (event == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> event is NULL\n", __func__, __LINE__);
    }

    if (buffer == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> buffer is NULL\n", __func__, __LINE__);
    }

    int i;
    size_t write_len = 0;

    if (type == MIDI_IMPLEMENTATION_SEQ)
    {
        if (event->cseq_valid)
        {
            int32_t varint_value = -1;
            // could be trying to print event in inconsistent state where absolute
            // time is valid, but delta time is wrong and about to be updated.
            if (event->cseq_delta_time.num_bytes > 0)
            {
                varint_value = VarLengthInt_get_value_big(&event->cseq_delta_time);
            }

            write_len = sprintf(
                buffer,
                "id=%d, offset=%ld, abs=%ld, delta=%d (0x%06x), chan=%d, command=0x%04x",
                event->id,
                event->file_offset,
                event->absolute_time,
                event->cseq_delta_time.standard_value,
                varint_value,
                event->command_channel,
                event->command
                );

            for (i=0; i<event->cseq_command_parameters_len; i++)
            {
                write_len += sprintf(&buffer[write_len], ", [%d]=%d", i, event->cseq_command_parameters[i]);
            }
        }
        else
        {
            write_len = sprintf(buffer, "id=%d, cseq invalid", event->id);
        }
    }
    else if (type == MIDI_IMPLEMENTATION_STANDARD)
    {
        if (event->midi_valid)
        {
            int32_t varint_value = -1;
            // could be trying to print event in inconsistent state where absolute
            // time is valid, but delta time is wrong and about to be updated.
            if (event->midi_delta_time.num_bytes > 0)
            {
                varint_value = VarLengthInt_get_value_big(&event->midi_delta_time);
            }

            write_len = sprintf(
                buffer,
                "id=%d, offset=%ld, abs=%ld, delta=%d (0x%06x), chan=%d, command=0x%04x",
                event->id,
                event->file_offset,
                event->absolute_time,
                event->midi_delta_time.standard_value,
                varint_value,
                event->command_channel,
                event->command
                );

            for (i=0; i<event->midi_command_parameters_len; i++)
            {
                write_len += sprintf(&buffer[write_len], ", [%d]=%d", i, event->midi_command_parameters[i]);
            }
        }
        else
        {
            write_len = sprintf(buffer, "id=%d, midi invalid", event->id);
        }
    }
    else
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> type not supported: %d\n", __func__, __LINE__, type);
    }

    if (write_len > bufer_len)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> buffer overflow. write_len=%ld, buffer_len=%ld \n", __func__, __LINE__, write_len, bufer_len);
    }

    TRACE_LEAVE(__func__)
    return write_len;
}

/**
 * Allocates memory for a new {@code struct MidiConvertOptions}.
 * @returns: new object.
*/
struct MidiConvertOptions *MidiConvertOptions_new(void)
{
    TRACE_ENTER(__func__)

    struct MidiConvertOptions *result = (struct MidiConvertOptions *)malloc_zero(1, sizeof(struct MidiConvertOptions));

    TRACE_LEAVE(__func__)
    return result;    
}

/**
 * Frees memory associated to options and all child objects.
 * This will free any {@code struct SeqPatternMatch} in {@code options->runtime_patterns_list}.
 * This will free {@code options->runtime_pattern_file}.
 * @param optons: Options to free.
*/
void MidiConvertOptions_free(struct MidiConvertOptions *options)
{
    TRACE_ENTER(__func__)

    if (options == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (options->runtime_patterns_list != NULL)
    {
        struct LinkedListNode *node;
        node = options->runtime_patterns_list->head;

        while (node != NULL)
        {
            struct SeqPatternMatch *match = node->data;
            if (match != NULL)
            {
                SeqPatternMatch_free(match);
                node->data = NULL;
            }

            node = node->next;
        }

        LinkedList_free(options->runtime_patterns_list);
        options->runtime_patterns_list = NULL;
    }
    
    if (options->runtime_pattern_file != NULL)
    {
        FileInfo_free(options->runtime_pattern_file);
        options->runtime_pattern_file = NULL;
    }

    free(options);

    TRACE_LEAVE(__func__)
}


struct MidiFile *MidiFile_transform_make_channel_track(struct MidiFile *midi_file)
{
    TRACE_ENTER(__func__)

    if (midi_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> midi_file is NULL\n", __func__, __LINE__);
    }

    struct GmidFile *gmid_file = GmidFile_new_from_midi(midi_file, 1);

    GmidFile_transform_make_channel_track(gmid_file);

    struct MidiFile *result = MidiFile_new_from_gmid(gmid_file);

    result->division = midi_file->division;

    TRACE_LEAVE(__func__)
    return result;
}

struct MidiFile *MidiFile_transform_set_channel_instrument(struct MidiFile *midi_file, int existing_channel, int new_instrument)
{
    TRACE_ENTER(__func__)

    if (midi_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> midi_file is NULL\n", __func__, __LINE__);
    }

    struct GmidFile *gmid_file = GmidFile_new_from_midi(midi_file, 0);

    GmidFile_transform_set_channel_instrument(gmid_file, existing_channel, new_instrument);

    struct MidiFile *result = MidiFile_new_from_gmid(gmid_file);

    result->division = midi_file->division;

    TRACE_LEAVE(__func__)
    return result;
}

struct MidiFile *MidiFile_transform_remove_loop(struct MidiFile *midi_file, int loop_number, int track)
{
    TRACE_ENTER(__func__)

    if (midi_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> midi_file is NULL\n", __func__, __LINE__);
    }

    struct GmidFile *gmid_file = GmidFile_new_from_midi(midi_file, 0);

    GmidFile_transform_remove_loop(gmid_file, loop_number, track);

    struct MidiFile *result = MidiFile_new_from_gmid(gmid_file);

    result->division = midi_file->division;

    TRACE_LEAVE(__func__)
    return result;
}

struct MidiFile *MidiFile_transform_add_note_loop(struct MidiFile *midi_file, int loop_number, int track)
{
    TRACE_ENTER(__func__)

    if (midi_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> midi_file is NULL\n", __func__, __LINE__);
    }

    struct GmidFile *gmid_file = GmidFile_new_from_midi(midi_file, 0);

    GmidFile_transform_add_note_loop(gmid_file, loop_number, track);

    struct MidiFile *result = MidiFile_new_from_gmid(gmid_file);

    result->division = midi_file->division;

    TRACE_LEAVE(__func__)
    return result;
}

void MidiFile_parse(struct MidiFile *midi_file, int parse_track_arg)
{
    TRACE_ENTER(__func__)

    if (midi_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> midi_file is NULL\n", __func__, __LINE__);
    }

    struct GmidFile *gmid_file = GmidFile_new_from_midi(midi_file, 0);
    int i;
    int print_count = 0;

    if (parse_track_arg == -1)
    {
        for (i=0; i<gmid_file->num_tracks; i++)
        {
            if (gmid_file->tracks[i] != NULL
                && gmid_file->tracks[i]->events != NULL
                && gmid_file->tracks[i]->events->count > 0)
            {
                GmidTrack_print(gmid_file->tracks[i], MIDI_IMPLEMENTATION_STANDARD);
                print_count++;
            }
        }
    }
    else
    {
        for (i=0; i<gmid_file->num_tracks; i++)
        {
            if (gmid_file->tracks[i] != NULL
                && gmid_file->tracks[i]->events != NULL
                && gmid_file->tracks[i]->events->count > 0
                && gmid_file->tracks[i]->midi_track_index == parse_track_arg)
            {
                GmidTrack_print(gmid_file->tracks[i], MIDI_IMPLEMENTATION_STANDARD);
                print_count++;
            }
        }
    }

    if (print_count == 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> could not find any matching tracks to print\n", __func__, __LINE__);
    }

    TRACE_LEAVE(__func__)
}

void GmidFile_transform_make_channel_track(struct GmidFile *gmid_file)
{
    TRACE_ENTER(__func__)

    if (gmid_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> gmid_file is NULL\n", __func__, __LINE__);
    }

    int i;

    for (i=0; i<gmid_file->num_tracks; i++)
    {
        if (gmid_file->tracks[i] != NULL
            && gmid_file->tracks[i]->events != NULL
            && gmid_file->tracks[i]->events->count > 0)
        {
            struct GmidEvent *event;
            struct LinkedListNode *node;

            node = gmid_file->tracks[i]->events->head;
            while (node != NULL)
            {
                event = (struct GmidEvent *)node->data;
                if (event != NULL
                    && event->midi_valid)
                {
                    event->command_channel = gmid_file->tracks[i]->midi_track_index;
                }
                node = node->next;
            }
        }
    }

    TRACE_LEAVE(__func__)
}

void GmidFile_transform_set_channel_instrument(struct GmidFile *gmid_file, int existing_channel, int new_instrument)
{
    TRACE_ENTER(__func__)

    if (gmid_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> gmid_file is NULL\n", __func__, __LINE__);
    }

    int i;

    for (i=0; i<gmid_file->num_tracks; i++)
    {
        if (gmid_file->tracks[i] != NULL
            && gmid_file->tracks[i]->events != NULL
            && gmid_file->tracks[i]->events->count > 0)
        {
            struct GmidEvent *event;
            struct LinkedListNode *node;

            node = gmid_file->tracks[i]->events->head;
            while (node != NULL)
            {
                event = (struct GmidEvent *)node->data;
                if (event != NULL
                    && event->midi_valid)
                {
                    if (event->command == MIDI_COMMAND_BYTE_PROGRAM_CHANGE
                        && event->command_channel == existing_channel)
                    {
                        if (g_verbosity >= 1)
                        {
                            printf("Changing MIDI \"%s\" event in track index %d, from %d to %d\n", MIDI_COMMAND_NAME_PROGRAM_CHANGE, gmid_file->tracks[i]->midi_track_index, event->midi_command_parameters[0], new_instrument);
                        }

                        event->midi_command_parameters[0] = new_instrument;
                        event->midi_command_parameters_raw[0] = new_instrument;
                    }
                }
                node = node->next;
            }
        }
    }
    
    TRACE_LEAVE(__func__)
}

void GmidFile_transform_add_note_loop(struct GmidFile *gmid_file, int loop_number, int track)
{
    TRACE_ENTER(__func__)

    if (gmid_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> gmid_file is NULL\n", __func__, __LINE__);
    }

    struct GmidEvent *event;
    struct LinkedListNode *node;
    struct LinkedListNode *new_node;
    struct GmidEvent *midi_start_event;
    struct GmidEvent *midi_count_event;
    struct GmidEvent *midi_end_event;

    // pre create events. This method will either succeed and fill in the rest of the details or fail.

    // create MIDI loop start event.
    midi_start_event = GmidEvent_new();
    midi_start_event->cseq_valid = 0;
    midi_start_event->midi_valid = 1;
    
    midi_start_event->command = MIDI_COMMAND_BYTE_CONTROL_CHANGE;
    midi_start_event->command_channel = track;

    midi_start_event->midi_command_len = MIDI_COMMAND_LEN_CONTROL_CHANGE;
    midi_start_event->midi_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_CONTROL_CHANGE;
    midi_start_event->midi_command_parameters[0] = MIDI_CONTROLLER_LOOP_START;
    midi_start_event->midi_command_parameters[1] = loop_number;
    midi_start_event->midi_command_parameters_len = MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
    midi_start_event->midi_command_parameters_raw[0] = MIDI_CONTROLLER_LOOP_START;
    midi_start_event->midi_command_parameters_raw[1] = loop_number;

    // create MIDI loop count event.
    midi_count_event = GmidEvent_new();
    midi_count_event->cseq_valid = 0;
    midi_count_event->midi_valid = 1;
    
    midi_count_event->command = MIDI_COMMAND_BYTE_CONTROL_CHANGE;
    midi_count_event->command_channel = track;

    midi_count_event->midi_command_len = MIDI_COMMAND_LEN_CONTROL_CHANGE;
    midi_count_event->midi_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_CONTROL_CHANGE;
    midi_count_event->midi_command_parameters[0] = MIDI_CONTROLLER_LOOP_COUNT_128;
    midi_count_event->midi_command_parameters[1] = 0xff; // loop count
    midi_count_event->midi_command_parameters_len = MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
    midi_count_event->midi_command_parameters_raw[0] = MIDI_CONTROLLER_LOOP_COUNT_128;
    midi_count_event->midi_command_parameters_raw[1] = 0xff; // loop count

    // create MIDI loop end event.
    midi_end_event = GmidEvent_new();
    midi_end_event->cseq_valid = 0;
    midi_end_event->midi_valid = 1;
    
    midi_end_event->command = MIDI_COMMAND_BYTE_CONTROL_CHANGE;
    midi_end_event->command_channel = track;

    midi_end_event->midi_command_len = MIDI_COMMAND_LEN_CONTROL_CHANGE;
    midi_end_event->midi_command_parameters_raw_len = MIDI_COMMAND_PARAM_BYTE_CONTROL_CHANGE;
    midi_end_event->midi_command_parameters[0] = MIDI_CONTROLLER_LOOP_END;
    midi_end_event->midi_command_parameters[1] = loop_number;
    midi_end_event->midi_command_parameters_len = MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
    midi_end_event->midi_command_parameters_raw[0] = MIDI_CONTROLLER_LOOP_END;
    midi_end_event->midi_command_parameters_raw[1] = loop_number;

    // connect dual pointers.
    midi_start_event->dual = midi_end_event->dual;
    midi_end_event->dual = midi_start_event->dual;

    // find where to place start event
    if (gmid_file->tracks[track] != NULL
        && gmid_file->tracks[track]->events != NULL
        && gmid_file->tracks[track]->events->count > 0)
    {
        node = gmid_file->tracks[track]->events->head;
        while (node != NULL)
        {
            event = (struct GmidEvent *)node->data;
            if (event != NULL
                && event->midi_valid)
            {
                if (event->command == MIDI_COMMAND_BYTE_NOTE_ON
                    // velocity == 0 means implicit note-off, so check this is positive
                    && event->midi_command_parameters[1] > 0
                )
                {
                    // set absolute times
                    midi_start_event->absolute_time = event->absolute_time;
                    midi_count_event->absolute_time = event->absolute_time; // same as start

                    if (g_verbosity >= VERBOSE_DEBUG)
                    {
                        printf("creating loop %d start event at abs time %ld\n", loop_number, event->absolute_time);
                    }

                    // insert in event list, before corresponding seq event.
                    // The order should be new start node, then new count node.
                    new_node = LinkedListNode_new();
                    new_node->data = midi_start_event;
                    LinkedListNode_insert_before(gmid_file->tracks[track]->events, node, new_node);
                    new_node = LinkedListNode_new();
                    new_node->data = midi_count_event;
                    LinkedListNode_insert_before(gmid_file->tracks[track]->events, node, new_node);

                    break;
                }
            }
            node = node->next;
        }
    }

    // find where to place end event
    if (gmid_file->tracks[track] != NULL
        && gmid_file->tracks[track]->events != NULL
        && gmid_file->tracks[track]->events->count > 0)
    {
        // iterate backwards
        node = gmid_file->tracks[track]->events->tail;
        while (node != NULL)
        {
            event = (struct GmidEvent *)node->data;
            if (event != NULL
                && event->midi_valid)
            {
                if (event->command == MIDI_COMMAND_BYTE_NOTE_OFF
                    || 
                    (
                        event->command == MIDI_COMMAND_BYTE_NOTE_ON
                        // velocity == 0 means implicit note-off
                        && event->midi_command_parameters[1] == 0
                    )
                )
                {
                    // actually want to insert this loop end event before the next node,
                    // which is after this last note off
                    if (node->next == NULL)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "%s %d> error iterating event list, missing track end event?\n", __func__, __LINE__);
                    }

                    // change back to next node.
                    node = node->next;
                    event = (struct GmidEvent *)node->data;

                    // set absolute times
                    midi_end_event->absolute_time = event->absolute_time;

                    if (g_verbosity >= VERBOSE_DEBUG)
                    {
                        printf("creating loop %d end event at abs time %ld\n", loop_number, event->absolute_time);
                    }

                    // insert in event list, before corresponding seq event.
                    // The order should be new start node, then new count node.
                    new_node = LinkedListNode_new();
                    new_node->data = midi_end_event;
                    LinkedListNode_insert_before(gmid_file->tracks[track]->events, node, new_node);

                    break;
                }
            }
            node = node->prev;
        }
    }

    if (midi_start_event == NULL)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> Could not find Note On event to insert loop start before.\n", __func__, __LINE__);
    }

    if (midi_end_event == NULL)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> Could not find last Note Off (or implicit Note Off) event to insert loop start before.\n", __func__, __LINE__);
    }

    // delta times and track sizes are now wrong, so recalculate those.
    GmidTrack_delta_from_absolute(gmid_file->tracks[track]);
    
    // estimate total track size in bytes.
    GmidTrack_set_track_size_bytes(gmid_file->tracks[track]);

    TRACE_LEAVE(__func__)
}

void GmidFile_transform_remove_loop(struct GmidFile *gmid_file, int loop_number, int track)
{
    TRACE_ENTER(__func__)

    if (gmid_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> gmid_file is NULL\n", __func__, __LINE__);
    }

    struct GmidEvent *event;
    struct LinkedListNode *node;
    int delete_count = 0;

    if (gmid_file->tracks[track] != NULL
        && gmid_file->tracks[track]->events != NULL
        && gmid_file->tracks[track]->events->count > 0)
    {
        node = gmid_file->tracks[track]->events->head;
        while (node != NULL)
        {
            
            struct LinkedListNode *next_node = node->next;

            event = (struct GmidEvent *)node->data;
            if (event != NULL
                && event->midi_valid)
            {
                if (event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE
                    && (
                        event->midi_command_parameters[0] == MIDI_CONTROLLER_LOOP_START
                        || event->midi_command_parameters[0] == MIDI_CONTROLLER_LOOP_END
                        || event->midi_command_parameters[0] == MIDI_CONTROLLER_LOOP_COUNT_0
                        || event->midi_command_parameters[0] == MIDI_CONTROLLER_LOOP_COUNT_128
                    )
                )
                {
                    delete_count++;
                    GmidTrack_delete_event(gmid_file->tracks[track], node);
                    LinkedListNode_free(gmid_file->tracks[track]->events, node);
                }
            }
            node = next_node;
        }
    }

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("Found %d events to remove from track %d\n", delete_count, track);
    }

    TRACE_LEAVE(__func__)
}

void GmidTrack_delete_event(struct GmidTrack *gtrack, struct LinkedListNode *node)
{
    TRACE_ENTER(__func__)

    if (gtrack == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> gtrack is NULL\n", __func__, __LINE__);
    }

    if (node == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> node is NULL\n", __func__, __LINE__);
    }

    struct GmidEvent *event = (struct GmidEvent *)node->data;
    struct LinkedListNode* forward;
    struct GmidEvent *forward_event;

    if (event == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> event is NULL\n", __func__, __LINE__);
    }

    // if current node is a valid MIDI event, carry forward the delta time and adjust track size.
    if (event->midi_valid)
    {
        int midi_bytes = event->midi_command_len + event->midi_command_parameters_raw_len + event->midi_delta_time.num_bytes;
        int midi_carry_delta = event->midi_delta_time.standard_value;

        forward = node->next;
        while (forward != NULL)
        {
            forward_event = (struct GmidEvent *)forward->data;
            if (forward_event->midi_valid)
            {
                int forward_delta_bytes_start = forward_event->midi_delta_time.num_bytes;
                int32_to_VarLengthInt(forward_event->midi_delta_time.standard_value + midi_carry_delta, &forward_event->midi_delta_time);
                int forward_delta_bytes_end = forward_event->midi_delta_time.num_bytes;

                midi_bytes += forward_delta_bytes_end - forward_delta_bytes_start;

                break;
            }

            forward = forward->next;
        }

        gtrack->midi_track_size_bytes -= midi_bytes;
    }

    // if current node is a valid cseq event, carry forward the delta time and adjust track size.
    if (event->cseq_valid)
    {
        int cseq_bytes = event->cseq_command_len + event->cseq_command_parameters_raw_len + event->cseq_delta_time.num_bytes;
        int cseq_carry_delta = event->cseq_delta_time.standard_value;

        forward = node->next;
        while (forward != NULL)
        {
            forward_event = (struct GmidEvent *)forward->data;
            if (forward_event->cseq_valid)
            {
                int forward_delta_bytes_start = forward_event->cseq_delta_time.num_bytes;
                int32_to_VarLengthInt(forward_event->cseq_delta_time.standard_value + cseq_carry_delta, &forward_event->cseq_delta_time);
                int forward_delta_bytes_end = forward_event->cseq_delta_time.num_bytes;

                cseq_bytes += forward_delta_bytes_end - forward_delta_bytes_start;

                break;
            }

            forward = forward->next;
        }

        gtrack->cseq_track_size_bytes -= cseq_bytes;
    }

    GmidEvent_free(event);
    node->data = NULL;

    TRACE_LEAVE(__func__)
}

/**
 * Allocates memory for GmidFile, and instantiates 16 child tracks.
 * @returns: pointer to new object.
*/
struct GmidFile *GmidFile_new()
{
    TRACE_ENTER(__func__)
    int i;

    struct GmidFile *gmid_file = (struct GmidFile *)malloc_zero(1, sizeof(struct GmidFile));

    gmid_file->tracks = (struct GmidTrack **)malloc_zero(CSEQ_FILE_NUM_TRACKS, sizeof(struct GmidTrack *));

    for (i=0; i<CSEQ_FILE_NUM_TRACKS; i++)
    {
        gmid_file->tracks[i] = GmidTrack_new();
    }

    gmid_file->num_tracks = CSEQ_FILE_NUM_TRACKS;

    TRACE_LEAVE(__func__)
    return gmid_file;
}

/**
 * Free memory associated to GmidFile and all child objects.
 * @param gmid_file: object to free.
*/
void GmidFile_free(struct GmidFile *gmid_file)
{
    TRACE_ENTER(__func__)
    int i;

    if (gmid_file == NULL)
    {
        return;
        TRACE_LEAVE(__func__)
    }

    if (gmid_file->tracks != NULL)
    {
        for (i=0; i<gmid_file->num_tracks; i++)
        {
            if (gmid_file->tracks[i] != NULL)
            {
                GmidTrack_free(gmid_file->tracks[i]);
                gmid_file->tracks[i] = NULL;
            }
        }

        gmid_file->num_tracks = 0;

        free(gmid_file->tracks);
        gmid_file->tracks = NULL;
    }

    free(gmid_file);
    
    TRACE_LEAVE(__func__)
}

/**
 * Calculates the number of bytes adjusted between start and end track position
 * due to pattern unrolling and escape sequences.
 * @param patterns: List of patterns encountered in the track during unroll process.
 * @param start_pos: Start position to begin looking for adjustments.
 * @param end_pos: End position to stop looking for adjustments.
*/
static int measure_unroll_adjust(struct LinkedList *patterns, int start_pos, int end_pos)
{
    TRACE_ENTER(__func__)

    if (patterns == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> patterns is NULL\n", __func__, __LINE__);
    }

    struct LinkedListNode *node;
    struct SeqPatternMatch *pattern;
    int result = 0;

    node = patterns->head;
    while (node != NULL)
    {
        pattern = (struct SeqPatternMatch *)node->data;
        if (pattern != NULL)
        {
            if (pattern->start_pattern_pos >= start_pos && pattern->start_pattern_pos <= end_pos)
            {
                if (pattern->type == CSEQ_PATTERN_UNROLL)
                {
                    // Pattern unroll added `length` bytes, but subtract size of command.
                    result += pattern->pattern_length - (CSEQ_COMMAND_LEN_PATTERN + CSEQ_COMMAND_PARAM_BYTE_PATTERN);
                }
                else if (pattern->type == CSEQ_PATTERN_ESCAPE)
                {
                    // Escape sequence is net -1 byte.
                    result -= pattern->pattern_length;
                }
            }
        }
        node = node->next;
    }

    TRACE_LEAVE(__func__)
    return result;
}

static struct SeqPatternMatch *SeqPatternMatch_new(void)
{
    TRACE_ENTER(__func__)

    struct SeqPatternMatch *result = (struct SeqPatternMatch *)malloc_zero(1, sizeof(struct SeqPatternMatch));

    TRACE_LEAVE(__func__)
    return result;
}

static struct SeqPatternMatch *SeqPatternMatch_new_values(int start_pattern_pos, int diff, int pattern_length)
{
    TRACE_ENTER(__func__)

    struct SeqPatternMatch *result = (struct SeqPatternMatch *)malloc_zero(1, sizeof(struct SeqPatternMatch));
    result->start_pattern_pos = start_pattern_pos;
    result->diff = diff;
    result->pattern_length = pattern_length;

    TRACE_LEAVE(__func__)
    return result;
}

static void SeqPatternMatch_free(struct SeqPatternMatch *obj)
{
    TRACE_ENTER(__func__)

    if (obj != NULL)
    {
        free(obj);
    }

    TRACE_LEAVE(__func__)
}

static void GmidTrack_debug_print(struct GmidTrack *track, enum MIDI_IMPLEMENTATION type)
{
    TRACE_ENTER(__func__)

    if (track == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> track is NULL\n", __func__, __LINE__);
    }

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        char *debug_printf_buffer;
        struct LinkedListNode *node;
        struct GmidEvent *event;

        debug_printf_buffer = (char *)malloc_zero(1, WRITE_BUFFER_LEN);

        printf("Print track midi # %d, cseq # %d\n", track->midi_track_index, track->cseq_track_index);

        node = track->events->head;
        while (node != NULL)
        {
            event = (struct GmidEvent *)node->data;
            if (event != NULL)
            {
                memset(debug_printf_buffer, 0, WRITE_BUFFER_LEN);
                size_t debug_str_len = GmidEvent_to_string(event, debug_printf_buffer, WRITE_BUFFER_LEN - 2, type);
                debug_printf_buffer[debug_str_len] = '\n';
                fflush_string(stdout, debug_printf_buffer);
            }

            node = node->next;
        }

        free(debug_printf_buffer);
    }

    TRACE_LEAVE(__func__)
}

// non-debug version
static void GmidTrack_print(struct GmidTrack *track, enum MIDI_IMPLEMENTATION type)
{
    TRACE_ENTER(__func__)

    if (track == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> track is NULL\n", __func__, __LINE__);
    }

    char *debug_printf_buffer;
    struct LinkedListNode *node;
    struct GmidEvent *event;

    debug_printf_buffer = (char *)malloc_zero(1, WRITE_BUFFER_LEN);

    if (type == MIDI_IMPLEMENTATION_STANDARD)
    {
        printf("Print MIDI track # %d\n", track->midi_track_index);
    }
    else if (type == MIDI_IMPLEMENTATION_SEQ)
    {
        printf("Print seq track # %d\n", track->cseq_track_index);
    }

    node = track->events->head;
    while (node != NULL)
    {
        event = (struct GmidEvent *)node->data;
        if (event != NULL)
        {
            memset(debug_printf_buffer, 0, WRITE_BUFFER_LEN);
            size_t debug_str_len = GmidEvent_to_string(event, debug_printf_buffer, WRITE_BUFFER_LEN - 2, type);
            debug_printf_buffer[debug_str_len] = '\n';
            fflush_string(stdout, debug_printf_buffer);
        }

        node = node->next;
    }

    printf("\n");

    free(debug_printf_buffer);

    TRACE_LEAVE(__func__)
}