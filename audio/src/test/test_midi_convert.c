#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "test_common.h"
#include "machine_config.h"
#include "debug.h"
#include "common.h"
#include "gaudio_math.h"
#include "utility.h"
#include "llist.h"
#include "naudio.h"
#include "midi.h"

// forward declarations



// end forward declarations

void midi_convert_all(int *run_count, int *pass_count, int *fail_count)
{
    int sub_count;
    int local_run_count = 0;

    sub_count = 0;
    test_midi_convert(&sub_count, pass_count, fail_count);
    local_run_count += sub_count;

    *run_count = *run_count + local_run_count;
}

void test_midi_convert(int *run_count, int *pass_count, int *fail_count)
{
    {
        printf("convert midi to seq (no pattern)\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct MidiFile *midi_file;
        struct MidiTrack *midi_track;
        struct CseqFile *cseq_file;
        int seq_track_len;
        int midi_raw_track_data_len;
        int i;

        uint8_t midi_raw_track_data[] = {
            /// delta time then event
            0x8E, 0x20, /* */ 0xC1, 0x42,
            0x85, 0x20, /* */ 0xB1, 0x07, 0x7F,
            0x83, 0x00, /* */ 0x5B, 0x5A,
            0x81, 0x40, /* */ 0x66, 0x00,
            0x00, /*       */ 0x69, 0x7F,
            0x02, /*       */ 0x91, 0x3C, 0x7F,
            0x8F, 0x50, /* */ 0x3C, 0x00,
            0x81, 0xB0, 0x2E, 0x3C, 0x7F,
            0x88, 0x7D, /* */ 0x3C, 0x00,
            0x03, /*       */ 0x3C, 0x7F,
            0x8A, 0x40, /* */ 0x3C, 0x00,
            0x96, 0x40, /* */ 0x3C, 0x7F,
            0x85, 0x7F, /* */ 0x3C, 0x00,
            0x01, /*       */ 0x3C, 0x7F,
            0x88, 0x7C, /* */ 0x3C, 0x00,
            0x04, /*       */ 0x3C, 0x7F,
            0x8A, 0x40, /* */ 0x3C, 0x00,
            0x96, 0x40, /* */ 0x3C, 0x7F,
            0x85, 0x6B, /* */ 0x3C, 0x00,
            0x84, 0xC0, 0x15, 0x3C, 0x7F,
            0x89, 0x00, /* */ 0x3C, 0x7F, // <- duplicate note-on
            0x82, 0x5C, /* */ 0x3C, 0x00,
            0x8C, 0x74, /* */ 0x3C, 0x00, // <- duplicate note-off
            0x91, 0x30, /* */ 0x3C, 0x7F,
            0x86, 0x00, /* */ 0x3C, 0x7F, // <- duplicate note-on
            0x81, 0x04, /* */ 0x3C, 0x00,
            0x87, 0x7C, /* */ 0x3C, 0x7F,
            0x82, 0x5C, /* */ 0x3C, 0x00,
            0x8C, 0x74, /* */ 0x3C, 0x00, // <- duplicate note-off
            0x91, 0x30, /* */ 0x3C, 0x7F,
            0x85, 0x74, /* */ 0x3C, 0x00,
            0x0C, /*       */ 0xB1, 0x67, 0x00, 
            0x00, /*       */ 0xFF, 0x2F, 0x00
        };
        midi_raw_track_data_len = sizeof(midi_raw_track_data);

        uint8_t seq_track_expected[] = {
            0x8E, 0x20, /* */ 0xC1, 0x42,
            0x85, 0x20, /* */ 0xB1, 0x07, 0x7F,
            0x83, 0x00, /* */ 0x5B, 0x5A,
            0x81, 0x40, /* */ 0xFF, 0x2E, 0x00, 0xFF,
            0x02, /*       */ 0x91, 0x3C, 0x7F, 0x8F, 0x50,
            0x81, 0xBF, 0x7E, 0x3C, 0x7F, 0x88, 0x7D,
            0x89, 0x00, /* */ 0x3C, 0x7F, 0x8A, 0x40,
            0xA1, 0x00, /* */ 0x3C, 0x7F, 0x85, 0x7F,
            0x86, 0x00, /* */ 0x3C, 0x7F, 0x88, 0x7C,
            0x89, 0x00, /* */ 0x3C, 0x7F, 0x8A, 0x40,
            0xA1, 0x00, /* */ 0x3C, 0x7F, 0x85, 0x6B,
            0x84, 0xC6, 0x00, 0x3C, 0x7F, 0x98, 0x50,
            0x89, 0x00, /* */ 0x3C, 0x7F, 0x82, 0x5C,
            0xA1, 0x00, /* */ 0x3C, 0x7F, 0x9E, 0x50,
            0x86, 0x00, /* */ 0x3C, 0x7F, 0x81, 0x04,
            0x89, 0x00, /* */ 0x3C, 0x7F, 0x82, 0x5C,
            0xA1, 0x00, /* */ 0x3C, 0x7F, 0x85, 0x74,
            0x86, 0x00, 0xFF, 0x2D, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x5A,
            0x00, 0xFF, 0x2F
        };
        seq_track_len = sizeof(seq_track_expected);

        // setup test midi file
        midi_file = MidiFile_new(MIDI_FORMAT_SIMULTANEOUS);
        midi_file->num_tracks = 1;
        
        midi_track = MidiTrack_new(0);
        midi_track->data = (uint8_t *)malloc_zero(1, midi_raw_track_data_len);
        memcpy(midi_track->data, midi_raw_track_data, midi_raw_track_data_len);
        midi_track->ck_data_size = midi_raw_track_data_len;

        midi_file->tracks = (struct MidiTrack **)malloc_zero(1, sizeof(struct MidiTrack *));
        midi_file->tracks[0] = midi_track;

        struct MidiConvertOptions *options = MidiConvertOptions_new();
        options->no_pattern_compression = 1;

        // done with setup. execute test.
        cseq_file = CseqFile_from_MidiFile(midi_file, options);

        MidiConvertOptions_free(options);

        pass_single = cseq_file->division == midi_file->division;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail division: expected %d, actual %d\n", __func__, __LINE__, midi_file->division, cseq_file->division);
        }

        pass_single = seq_track_len == (int)cseq_file->compressed_data_len;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail seq_track_len: expected %d, actual %d\n", __func__, __LINE__, seq_track_len, (int)cseq_file->compressed_data_len);
        }

        pass_single = 1;
        for (i=0; i<seq_track_len && i<(int)cseq_file->compressed_data_len; i++)
        {
            pass_single &= seq_track_expected[i] == cseq_file->compressed_data[i];
            pass &= pass_single;
        }

        if (!pass_single || (seq_track_len != (int)cseq_file->compressed_data_len))
        {
            printf("%s %d> fail seq\n", __func__, __LINE__);
            print_expected_vs_actual_arr(seq_track_expected, seq_track_len, cseq_file->compressed_data, cseq_file->compressed_data_len);
        }

        MidiFile_free(midi_file);
        CseqFile_free(cseq_file);

        if (pass == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d> fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("convert (no pattern) seq to midi -- 2\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct MidiFile *midi_file;
        struct MidiTrack *midi_track;
        struct CseqFile *cseq_file;
        int seq_track_len;
        int midi_raw_track_data_len;
        int i;

        // this test uses channel 6

        uint8_t seq_track[] = {
            0x98, 0x00, 0xFF, 0x2E, 0x00, 0xFF,
            0x81, 0x9C, 0x60, 0xC6, 0x3C,
            0x84, 0x40, 0xB6, 0x07, 0x36,
            0x97, 0x37, 0x96, 0x40, 0x7F, 0x90, 0x7E,
            0x85, 0xA0, 0x42, 0x40, 0x7F, 0x90, 0x7E,
            0xE6, 0x67, 0xFF, 0x2D, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x22, // <- delta is before beginning of track
            0x00, 0xFF, 0x2F
        };
        seq_track_len = sizeof(seq_track);

        uint8_t midi_raw_track_expected[]  = {
            /// delta time then event
            0x98, 0x00, 0xB6, 0x66, 0x00, // loop start
            0x00, 0x69, 0x7f,             // loop count
            0x81, 0x9C, 0x60, 0xC6, 0x3C,
            0x84, 0x40, 0xB6, 0x07, 0x36,
            0x97, 0x37, 0x96, 0x40, 0x7F,
            0x90, 0x7e, 0x40, 0x00,
            0x85, 0x8f, 0x44, 0x40, 0x7F,
            0x90, 0x7e, 0x40, 0x00,
            0xD5, 0x69, 0xB6, 0x67, 0x00,
            0x00, 0xFF, 0x2F, 0x00
        };
        midi_raw_track_data_len = sizeof(midi_raw_track_expected);

        // setup test seq file
        cseq_file = CseqFile_new();
        cseq_file->non_empty_num_tracks = 1;
        cseq_file->track_lengths[6] = seq_track_len;
        cseq_file->compressed_data_len = seq_track_len;
        cseq_file->compressed_data = (uint8_t *)malloc_zero(1, cseq_file->compressed_data_len);
        // initialy the track data offsets are read from the file, which
        // includes the length of the header. This is subtracted out when
        // accessing the data.
        cseq_file->track_offset[6] = CSEQ_FILE_HEADER_SIZE_BYTES;
        memcpy(cseq_file->compressed_data, seq_track, seq_track_len);

        // done with setup. execute test.
        midi_file = MidiFile_from_CseqFile(cseq_file, NULL);

        if (midi_file->tracks == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> midi_file->tracks is NULL\n", __func__, __LINE__);
        }

        if (midi_file->tracks[0] == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> midi_file->tracks[0] is NULL\n", __func__, __LINE__);
        }

        pass_single = cseq_file->division == midi_file->division;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail division: expected %d, actual %d\n", __func__, __LINE__, cseq_file->division, midi_file->division);
        }

        pass_single = midi_file->num_tracks == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_file->num_tracks: expected %d, actual %d\n", __func__, __LINE__, 1, midi_file->num_tracks);
        }

        midi_track = midi_file->tracks[0];

        pass_single = midi_track->ck_data_size == midi_raw_track_data_len;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_track->ck_data_size: expected %d, actual %d\n", __func__, __LINE__, midi_raw_track_data_len, midi_track->ck_data_size);
        }

        pass_single = 1;
        for (i=0; i<midi_raw_track_data_len && midi_track->ck_data_size; i++)
        {
            pass_single &= midi_raw_track_expected[i] == midi_track->data[i];
            pass &= pass_single;
        }

        if (!pass_single || (midi_track->ck_data_size != midi_raw_track_data_len))
        {
            printf("%s %d> fail midi\n", __func__, __LINE__);
            print_expected_vs_actual_arr(midi_raw_track_expected, midi_raw_track_data_len, midi_track->data, midi_track->ck_data_size);
        }

        MidiFile_free(midi_file);
        CseqFile_free(cseq_file);

        if (pass == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d> fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("convert midi to seq (no pattern) -- 3\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct MidiFile *midi_file;
        struct MidiTrack *midi_track;
        struct CseqFile *cseq_file;
        int seq_track_len;
        int midi_raw_track_data_len;
        int i;

        uint8_t midi_raw_track_data[]  = {
            /// delta time then event
            0x98, 0x00, 0xB6, 0x66, 0x00, // loop start
            0x00, 0x69, 0x7f,             // loop count
            0x81, 0x9C, 0x60, 0xC6, 0x3C,
            0x84, 0x40, 0xB6, 0x07, 0x36,
            0x97, 0x37, 0x96, 0x40, 0x7F,
            0x90, 0x7e, 0x40, 0x00,
            0x85, 0x8f, 0x44, 0x40, 0x7F,
            0x90, 0x7e, 0x40, 0x00,
            0xD5, 0x69, 0xB6, 0x67, 0x00,
            0x00, 0xFF, 0x2F, 0x00
        };
        midi_raw_track_data_len = sizeof(midi_raw_track_data);

        uint8_t seq_track_expected[] = {
            0x98, 0x00, 0xFF, 0x2E, 0x00, 0xFF,
            0x81, 0x9C, 0x60, 0xC6, 0x3C,
            0x84, 0x40, 0xB6, 0x07, 0x36,
            0x97, 0x37, 0x96, 0x40, 0x7F, 0x90, 0x7E,
            0x85, 0xA0, 0x42, 0x40, 0x7F, 0x90, 0x7E,
            0xE6, 0x67, 0xFF, 0x2D, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x22, // <- delta is before beginning of track
            0x00, 0xFF, 0x2F
        };
        seq_track_len = sizeof(seq_track_expected);

        // setup test midi file
        midi_file = MidiFile_new(MIDI_FORMAT_SIMULTANEOUS);
        midi_file->num_tracks = 1;
        
        midi_track = MidiTrack_new(0);
        midi_track->data = (uint8_t *)malloc_zero(1, midi_raw_track_data_len);
        memcpy(midi_track->data, midi_raw_track_data, midi_raw_track_data_len);
        midi_track->ck_data_size = midi_raw_track_data_len;

        midi_file->tracks = (struct MidiTrack **)malloc_zero(1, sizeof(struct MidiTrack *));
        midi_file->tracks[0] = midi_track;

        // done with setup. execute test.
        cseq_file = CseqFile_from_MidiFile(midi_file, NULL);

        pass_single = cseq_file->division == midi_file->division;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail division: expected %d, actual %d\n", __func__, __LINE__, midi_file->division, cseq_file->division);
        }

        pass_single = seq_track_len == (int)cseq_file->compressed_data_len;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail seq_track_len: expected %d, actual %d\n", __func__, __LINE__, seq_track_len, (int)cseq_file->compressed_data_len);
        }

        pass_single = 1;
        for (i=0; i<seq_track_len && i<(int)cseq_file->compressed_data_len; i++)
        {
            pass_single &= seq_track_expected[i] == cseq_file->compressed_data[i];
            pass &= pass_single;
        }

        if (!pass_single || (seq_track_len != (int)cseq_file->compressed_data_len))
        {
            printf("%s %d> fail seq\n", __func__, __LINE__);

            print_expected_vs_actual_arr(seq_track_expected, seq_track_len, cseq_file->compressed_data, cseq_file->compressed_data_len);
        }

        MidiFile_free(midi_file);
        CseqFile_free(cseq_file);

        if (pass == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d> fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("convert seq roll to cseq, use_pattern_marker_file -- 4\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;
        
        struct GmidTrack *gtrack;
        uint8_t *write_buffer;
        size_t current_buffer_pos;
        size_t buffer_len;
        int i;

        uint8_t seq_data[] = {
            0x98, 0x00, 0xFF, 0x2E, 0x00, 0xFF, 0x81, 0xB4,
            0x60, 0xCA, 0x3D, 0x83, 0x60, 0xBA, 0x5B, 0x5A,
            0x60, 0x07, 0x6E, 0x83, 0x60, 0x0A, 0x28, 0x83,
            0x00, 0x9A, 
            // forward match 0x0A
            0x34, 0x7F, 0x74, 0x81, 0x40, 0x36, 0x7F, 0x74, 0x81, 0x40,
            // forward match 0x37
            0x38, 0x7F, 0x84, 0x59, 0x86,
            // forward match 0x0B, and next index is forward match 0x21
            0x00, 0x34, 0x7F, 0x74, 0x81, 0x40, 0x36, 0x7F, 0x74, 0x81, 0x40,
            0x37, 0x7F, 0x83, 0x64,
            0x84, 0x40, 0x36, 0x7F, 0x74, 0x81, 0x40, 0x37,
            0x7F, 0x97, 0x3F, 0x98, 0x00, 0x36, 0x7F, 0x85,
            0x7A, 0x86, 0x00, 0x34, 0x7F, 0x74, 0x81, 0x40,
            0x36, 0x7F, 0x74, 0x81, 0x40, 0x38, 0x7F, 0x84,
            0x59, 0x86, 0x00, 
            // start 0xFE, 0x00, 0x41, 0x0A
            0x34, 0x7F, 0x74, 0x81, 0x40, 0x36, 0x7F, 0x74, 0x81, 0x40,
            // end
            0x37, 0x7F, 0x83, 0x64, 0x84, 0x40, 0x36, 0x7F,
            0x74, 0x81, 0x40, 0x37, 0x7F, 0x97, 0x3F, 0x98,
            0x00, 0x36, 0x7F, 0x85, 0x7A, 0x84, 0xC6, 
            // start: 0xFE, 0x00, 0x4D, 0x0B
            0x00, 0x34, 0x7F, 0x74, 0x81, 0x40, 0x36, 0x7F, 0x74, 0x81, 0x40,
            // end
            // start: 0xFE, 0x00, 0x56, 0x37
            0x38, 0x7F, 0x84, 0x59, 0x86, 0x00, 0x34, 0x7F,
            0x74, 0x81, 0x40, 0x36, 0x7F, 0x74, 0x81, 0x40,
            0x37, 0x7F, 0x83, 0x64, 0x84, 0x40, 0x36, 0x7F,
            0x74, 0x81, 0x40, 0x37, 0x7F, 0x97, 0x3F, 0x98,
            0x00, 0x36, 0x7F, 0x85, 0x7A, 0x86, 0x00, 0x34,
            0x7F, 0x74, 0x81, 0x40, 0x36, 0x7F, 0x74, 0x81,
            0x40, 0x38, 0x7F, 0x84, 0x59, 0x86, 0x00,
            // end
            // start: 0xFE, 0x00, 0x54, 0x21
            0x34, 0x7F, 0x74, 0x81, 0x40, 0x36, 0x7F, 0x74,
            0x81, 0x40, 0x37, 0x7F, 0x83, 0x64, 0x84, 0x40,
            0x36, 0x7F, 0x74, 0x81, 0x40, 0x37, 0x7F, 0x97,
            0x3F, 0x98, 0x00, 0x36, 0x7F, 0x85, 0x7A, 0x86,
            0x00, 
            // end
            0xFF, 0x2D, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0xE1,
            0x00, 0xFF, 0x2F
        };
        int seq_data_len = sizeof(seq_data);

        uint8_t exepected_cseq_data[] = {
            0x98, 0x00, 0xFF, 0x2E, 0x00, 0xFF, 0x81, 0xB4,
            0x60, 0xCA, 0x3D, 0x83, 0x60, 0xBA, 0x5B, 0x5A,
            0x60, 0x07, 0x6E, 0x83, 0x60, 0x0A, 0x28, 0x83,
            0x00, 0x9A, 0x34, 0x7F, 0x74, 0x81, 0x40, 0x36,
            0x7F, 0x74, 0x81, 0x40, 0x38, 0x7F, 0x84, 0x59,
            0x86, 0x00, 0x34, 0x7F, 0x74, 0x81, 0x40, 0x36,
            0x7F, 0x74, 0x81, 0x40, 0x37, 0x7F, 0x83, 0x64,
            0x84, 0x40, 0x36, 0x7F, 0x74, 0x81, 0x40, 0x37,
            0x7F, 0x97, 0x3F, 0x98, 0x00, 0x36, 0x7F, 0x85,
            0x7A, 0x86, 0x00, 0x34, 0x7F, 0x74, 0x81, 0x40,
            0x36, 0x7F, 0x74, 0x81, 0x40, 0x38, 0x7F, 0x84,
            0x59, 0x86, 0x00,
            0xFE, 0x00, 0x41, 0x0A,
            0x37, 0x7F, 0x83, 0x64, 0x84, 0x40, 0x36, 0x7F,
            0x74, 0x81, 0x40, 0x37, 0x7F, 0x97, 0x3F, 0x98, 
            0x00, 0x36, 0x7F, 0x85, 0x7A, 0x84, 0xC6, 
            0xFE, 0x00, 0x4D, 0x0B, 
            0xFE, 0x00, 0x56, 0x37, 
            0xFE, 0x00, 0x54, 0x21, 
            0xFF, 0x2D, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x84,
            0x00, 0xFF, 0x2F
        };
        int exepected_cseq_data_len = sizeof(exepected_cseq_data);

        // setup
        write_buffer = (uint8_t *)malloc_zero(1, seq_data_len); // overallocate

        gtrack = GmidTrack_new();
        gtrack->cseq_data = (uint8_t *)malloc_zero(1, seq_data_len);
        gtrack->midi_track_index = 9;
        gtrack->cseq_track_index = 10;
        memcpy(gtrack->cseq_data, seq_data, seq_data_len);
        gtrack->cseq_track_size_bytes = seq_data_len;
        gtrack->cseq_data_len = seq_data_len;

        parse_seq_bytes_to_event_list(seq_data, seq_data_len, gtrack->events);

        current_buffer_pos = 0;
        buffer_len = seq_data_len;

        struct MidiConvertOptions *options = MidiConvertOptions_new();
        options->use_pattern_marker_file = 1;
        options->pattern_marker_filename = "test_cases/midi/patterns0004.csv";

        // execute
        GmidTrack_roll_entry(gtrack, write_buffer, &current_buffer_pos, buffer_len, options);

        MidiConvertOptions_free(options);

        // compare
        pass_single = gtrack->cseq_track_size_bytes == exepected_cseq_data_len;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail gtrack->cseq_track_size_bytes: expected %d, actual %d\n", __func__, __LINE__, exepected_cseq_data_len, gtrack->cseq_track_size_bytes);
        }

        pass_single = (int)gtrack->cseq_data_len == exepected_cseq_data_len;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail gtrack->cseq_data_len: expected %d, actual %ld\n", __func__, __LINE__, exepected_cseq_data_len, gtrack->cseq_data_len);
        }

        pass_single = 1;
        for (i=0; i<exepected_cseq_data_len && i<(int)gtrack->cseq_data_len; i++)
        {
            pass_single &= exepected_cseq_data[i] == gtrack->cseq_data[i];
            pass &= pass_single;
        }

        if (!pass_single || (exepected_cseq_data_len != (int)gtrack->cseq_data_len))
        {
            printf("%s %d> fail seq\n", __func__, __LINE__);
            print_expected_vs_actual_arr(exepected_cseq_data, exepected_cseq_data_len, gtrack->cseq_data, gtrack->cseq_data_len);
        }

        // cleanup
        GmidTrack_free(gtrack);
        free(write_buffer);

        if (pass == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d> fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("convert seq roll to cseq, naive algorithm, same track -- 5\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;
        
        struct GmidTrack *gtrack;
        uint8_t *write_buffer;
        size_t current_buffer_pos;
        size_t buffer_len;
        int i;

        uint8_t seq_data[] = {
            0x00, 0x96, 0x3c, 0x51, 0x82, 0x71,
            0x98, 0x10, 0x3d, 0x50, 0x81, 0x70,
            0x98, 0x10, 0x3e, 0x50, 0x81, 0x70,
            0x98, 0x10, 0x3d, 0x50, 0x81, 0x70,
            0x98, 0x10, 0x3e, 0x50, 0x81, 0x70,
            0x98, 0x10, 0x41, 0x52, 0x83, 0x72,
            0x98, 0x10, 0x3d, 0x50, 0x81, 0x70,
            0x98, 0x10, 0x3e, 0x50, 0x81, 0x70,
            0x98, 0x10, 0x3d, 0x50, 0x81, 0x70,
            0x98, 0x10, 0x3e, 0x50, 0x81, 0x70,
            0x98, 0x10, 0x41, 0x52, 0x83, 0x72,
            0x98, 0x11, 0x4b, 0x50, 0x81, 0x70,
            0x98, 0x10, 0x4c, 0x50, 0x81, 0x70,
            0x00, 0xFF, 0x2F
        };
        int seq_data_len = sizeof(seq_data);

        uint8_t exepected_cseq_data[] = {
            0x00, 0x96, 0x3c, 0x51, 0x82, 0x71,
            0x98, 0x10, 0x3d, 0x50, 0x81, 0x70,
            0x98, 0x10, 0x3e, 0x50, 0x81, 0x70,
            0xfe, 0x00, 0x0c, 0x0c,
            0x98, 0x10, 0x41, 0x52, 0x83, 0x72,
            0xfe, 0x00, 0x16, 0x0c,
            0xfe, 0x00, 0x1a, 0x0c,
            0x98, 0x10, 0x41, 0x52, 0x83, 0x72,
            0x98, 0x11, 0x4b, 0x50, 0x81, 0x70,
            0x98, 0x10, 0x4c, 0x50, 0x81, 0x70,
            0x00, 0xFF, 0x2F
        };
        int exepected_cseq_data_len = sizeof(exepected_cseq_data);

        // setup
        write_buffer = (uint8_t *)malloc_zero(1, seq_data_len); // overallocate

        gtrack = GmidTrack_new();
        gtrack->cseq_data = (uint8_t *)malloc_zero(1, seq_data_len);
        gtrack->midi_track_index = 6;
        gtrack->cseq_track_index = 6;
        memcpy(gtrack->cseq_data, seq_data, seq_data_len);
        gtrack->cseq_track_size_bytes = seq_data_len;
        gtrack->cseq_data_len = seq_data_len;

        parse_seq_bytes_to_event_list(seq_data, seq_data_len, gtrack->events);

        current_buffer_pos = 0;
        buffer_len = seq_data_len;

        struct MidiConvertOptions *options = MidiConvertOptions_new();
        options->use_pattern_marker_file = 0;
        options->pattern_algorithm = PATTERN_ALGORITHM_NAIVE;

        // execute
        GmidTrack_roll_entry(gtrack, write_buffer, &current_buffer_pos, buffer_len, options);

        MidiConvertOptions_free(options);

        // compare
        pass_single = gtrack->cseq_track_size_bytes == exepected_cseq_data_len;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail gtrack->cseq_track_size_bytes: expected %d, actual %d\n", __func__, __LINE__, exepected_cseq_data_len, gtrack->cseq_track_size_bytes);
        }

        pass_single = (int)gtrack->cseq_data_len == exepected_cseq_data_len;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail gtrack->cseq_data_len: expected %d, actual %ld\n", __func__, __LINE__, exepected_cseq_data_len, gtrack->cseq_data_len);
        }

        pass_single = 1;
        for (i=0; i<exepected_cseq_data_len && i<(int)gtrack->cseq_data_len; i++)
        {
            pass_single &= exepected_cseq_data[i] == gtrack->cseq_data[i];
            pass &= pass_single;
        }

        pass_single = (int)current_buffer_pos == exepected_cseq_data_len;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> current_buffer_pos: expected %d, actual %ld\n", __func__, __LINE__, exepected_cseq_data_len, current_buffer_pos);
        }

        pass_single = 1;
        for (i=0; i<exepected_cseq_data_len && i<(int)current_buffer_pos; i++)
        {
            pass_single &= exepected_cseq_data[i] == write_buffer[i];
            pass &= pass_single;
        }

        if (!pass_single || (exepected_cseq_data_len != (int)current_buffer_pos))
        {
            printf("%s %d> fail seq\n", __func__, __LINE__);
            print_expected_vs_actual_arr(exepected_cseq_data, exepected_cseq_data_len, write_buffer, current_buffer_pos);
        }

        // cleanup
        GmidTrack_free(gtrack);
        free(write_buffer);

        if (pass == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d> fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }
    
    {
        printf("convert seq roll to cseq, naive algorithm, across tracks -- 5\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;
        
        struct GmidTrack *gtrack6;
        struct GmidTrack *gtrack7;
        uint8_t *write_buffer;
        size_t current_buffer_pos;
        size_t buffer_len;
        int i;

        uint8_t seq_data_6[] = {
            0x00, 0x96, 0x3c, 0x51, 0x82, 0x71,
            0x98, 0x10, 0x3d, 0x50, 0x81, 0x70,
            0x98, 0x10, 0x3e, 0x50, 0x81, 0x70,
            0x98, 0x10, 0x3d, 0x50, 0x81, 0x70,
            0x98, 0x10, 0x3e, 0x50, 0x81, 0x70,
            0x98, 0x10, 0x41, 0x52, 0x83, 0x72,
            0x98, 0x10, 0x3d, 0x50, 0x81, 0x70,
            0x98, 0x10, 0x3e, 0x50, 0x81, 0x70,
            0x98, 0x10, 0x3d, 0x50, 0x81, 0x70,
            0x98, 0x10, 0x3e, 0x50, 0x81, 0x70,
            0x98, 0x10, 0x41, 0x52, 0x83, 0x72,
            0x98, 0x11, 0x4b, 0x50, 0x81, 0x70,
            0x98, 0x10, 0x4c, 0x50, 0x81, 0x70,
            0x00, 0xFF, 0x2F
        };
        int seq_data_6_len = sizeof(seq_data_6);

        uint8_t seq_data_7[] = {
            0x00, 0x97, 0x3c, 0x51, 0x82, 0x72,
            0x98, 0x10, 0x3d, 0x50, 0x81, 0x70,
            0x98, 0x10, 0x3e, 0x50, 0x81, 0x70,
            0x98, 0x10, 0x3d, 0x50, 0x81, 0x70,
            0x98, 0x10, 0x3e, 0x50, 0x81, 0x70,
            0x98, 0x10, 0x41, 0x52, 0x83, 0x72,
            0x98, 0x13, 0x4b, 0x50, 0x81, 0x72,
            0x98, 0x12, 0x4c, 0x50, 0x81, 0x71,
            0x00, 0xFF, 0x2F
        };
        int seq_data_7_len = sizeof(seq_data_7);

        uint8_t exepected_cseq_data[] = {
            0x00, 0x96, 0x3c, 0x51, 0x82, 0x71,
            0x98, 0x10, 0x3d, 0x50, 0x81, 0x70,
            0x98, 0x10, 0x3e, 0x50, 0x81, 0x70,
            0xfe, 0x00, 0x0c, 0x0c,
            0x98, 0x10, 0x41, 0x52, 0x83, 0x72,
            0xfe, 0x00, 0x16, 0x0c,
            0xfe, 0x00, 0x1a, 0x0c,
            0x98, 0x10, 0x41, 0x52, 0x83, 0x72,
            0x98, 0x11, 0x4b, 0x50, 0x81, 0x70,
            0x98, 0x10, 0x4c, 0x50, 0x81, 0x70,
            0x00, 0xFF, 0x2F,

            0x00, 0x97, 0x3c, 0x51, 0x82, 0x72,
            0xfe, 0x00, 0x39, 0x0c,
            0xfe, 0x00, 0x3d, 0x0c,
            0xfe, 0x00, 0x23, 0x07,
                  0x13, 0x4b, 0x50, 0x81, 0x72,
            0x98, 0x12, 0x4c, 0x50, 0x81, 0x71,
            0x00, 0xFF, 0x2F
        };
        int exepected_cseq_data_len = sizeof(exepected_cseq_data);

        // setup
        write_buffer = (uint8_t *)malloc_zero(1, seq_data_6_len + seq_data_7_len);

        gtrack6 = GmidTrack_new();
        gtrack6->cseq_data = (uint8_t *)malloc_zero(1, seq_data_6_len);
        gtrack6->midi_track_index = 6;
        gtrack6->cseq_track_index = 6;
        memcpy(gtrack6->cseq_data, seq_data_6, seq_data_6_len);
        gtrack6->cseq_track_size_bytes = seq_data_6_len;
        gtrack6->cseq_data_len = seq_data_6_len;

        parse_seq_bytes_to_event_list(seq_data_6, seq_data_6_len, gtrack6->events);

        gtrack7 = GmidTrack_new();
        gtrack7->cseq_data = (uint8_t *)malloc_zero(1, seq_data_7_len);
        gtrack7->midi_track_index = 7;
        gtrack7->cseq_track_index = 7;
        memcpy(gtrack7->cseq_data, seq_data_7, seq_data_7_len);
        gtrack7->cseq_track_size_bytes = seq_data_7_len;
        gtrack7->cseq_data_len = seq_data_7_len;

        parse_seq_bytes_to_event_list(seq_data_7, seq_data_7_len, gtrack7->events);

        current_buffer_pos = 0;
        buffer_len = seq_data_6_len + seq_data_7_len;

        struct MidiConvertOptions *options = MidiConvertOptions_new();
        options->use_pattern_marker_file = 0;
        options->pattern_algorithm = PATTERN_ALGORITHM_NAIVE;

        // execute
        GmidTrack_roll_entry(gtrack6, write_buffer, &current_buffer_pos, buffer_len, options);
        GmidTrack_roll_entry(gtrack7, write_buffer, &current_buffer_pos, buffer_len, options);

        MidiConvertOptions_free(options);

        // compare
        pass_single = (int)current_buffer_pos == exepected_cseq_data_len;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> current_buffer_pos: expected %d, actual %ld\n", __func__, __LINE__, exepected_cseq_data_len, current_buffer_pos);
        }

        pass_single = 1;
        for (i=0; i<exepected_cseq_data_len && i<(int)current_buffer_pos; i++)
        {
            pass_single &= exepected_cseq_data[i] == write_buffer[i];
            pass &= pass_single;
        }

        if (!pass_single || (exepected_cseq_data_len != (int)current_buffer_pos))
        {
            printf("%s %d> fail seq\n", __func__, __LINE__);
            print_expected_vs_actual_arr(exepected_cseq_data, exepected_cseq_data_len, write_buffer, current_buffer_pos);
        }

        // cleanup
        GmidTrack_free(gtrack6);
        GmidTrack_free(gtrack7);
        free(write_buffer);

        if (pass == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d> fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("convert seq roll to cseq, naive algorithm, loop end diff -- 6\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;
        
        struct MidiFile *midi_file;
        struct MidiTrack *midi_track;
        struct CseqFile *cseq_file;
        int i;

        uint8_t midi_data[] = {
            0x8D, 0x40, 0xC9, 0x46, 0x82, 0x20, 0xB9, 0x07,
            0x51, 0x83, 0x60, 0x5B, 0x5A, 0x83, 0x00, 0x0A,
            0x64, 0x81, 0x40, 0x66, 0x00, 0x00, 0x69, 0x7F,
            0x02, 0x99, 0x1C, 0x5C, 0xDF, 0x74, 0x1C, 0x00,
            0x0A, 0x1C, 0x70, 0xDE, 0x0D, 0x1C, 0x00, 0x81,
            0x73, 0x1C, 0x7F, 0x89, 0x00, 0x1C, 0x00, 0x00,
            0x18, 0x7F, 0xA0, 0x74, 0x18, 0x00, 0x0C, 0x1A,
            0x7F, 0x86, 0x00, 0x1A, 0x00, 0x00, 0x1C, 0x7F,
            0x88, 0x67, 0x1C, 0x00, 0x19, 0x18, 0x7F, 0xA0,
            0x37, 0x18, 0x00, 0x49, 0x1A, 0x7F, 0x85, 0x54,
            0x1A, 0x00, 0x2C, 0x1C, 0x70, 0xDF, 0x76, 0x1C,
            0x00, 0x0A, 0x1C, 0x70, 0xDF, 0x66, 0x1C, 0x00,
            0x1A, 0x1C, 0x5C, 0xDF, 0x71, 0x1C, 0x00, 0x0F,
            0x1C, 0x70, 0xDE, 0x0D, 0x1C, 0x00, 0x81, 0x73,
            0xB9, 0x66, 0x00, 0x02, 0x99, 0x1C, 0x5C, 0xDF,
            0x74, 0x1C, 0x00, 0x0A, 0x1C, 0x70, 0xE0, 0x00,
            0x1C, 0x00, 0x00, 0x1C, 0x7F, 0x89, 0x00, 0x1C,
            0x00, 0x00, 0x18, 0x7F, 0xA0, 0x74, 0x18, 0x00,
            0x0C, 0x1A, 0x7F, 0x86, 0x00, 0x1A, 0x00, 0x00,
            0x1C, 0x7F, 0x88, 0x67, 0x1C, 0x00, 0x19, 0x18,
            0x7F, 0xA0, 0x37, 0x18, 0x00, 0x49, 0x1A, 0x7F,
            0x85, 0x54, 0x1A, 0x00, 0x2C, 0xB9, 0x67, 0x00,
            0x00, 0xFF, 0x2F, 0x00

        };
        int midi_data_len = sizeof(midi_data);

        uint8_t exepected_cseq_data[] = {
            0x8D, 0x40, 0xC9, 0x46, 0x82, 0x20, 0xB9, 0x07,
            0x51, 0x83, 0x60, 0x5B, 0x5A, 0x83, 0x00, 0x0A,
            0x64, 0x81, 0x40, 0xFF, 0x2E, 0x00, 0xFF, 0x02,
            0x99, 0x1C, 0x5C, 0xDF, 0x74, 0xDF, 0x7E, 0x1C,
            0x70, 0xDE, 0x0D, 0xE0, 0x00, 0x1C, 0x7F, 0x89,
            0x00, 0x89, 0x00, 0x18, 0x7F, 0xA0, 0x74, 0xA1,
            0x00, 0x1A, 0x7F, 0x86, 0x00, 0x86, 0x00, 0x1C,
            0x7F, 0x88, 0x67, 0x89, 0x00, 0x18, 0x7F, 0xA0,
            0x37, 0xA1, 0x00, 0x1A, 0x7F, 0x85, 0x54, 0x86,
            0x00, 0x1C, 0x70, 0xDF, 0x76, 0xE0, 0x00, 0x1C,
            0x70, 0xDF, 0x66, 0xE0, 0x00, 0x1C, 0x5C, 0xDF,
            0x71, 0xE0, 0x00, 0x1C, 0x70, 0xDE, 0x0D, 0xE0,
            0x00, 0xFF, 0x2E, 0x00, 0xFF, 0xFE, 0x00, 0x4E,
            0x0A, 0xE0, 0x00, 0xFE, 0x00, 0x48, 0x26, 0xFF,
            0x2D, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x60, 0x00,
            0xFF, 0x2F
        };
        int exepected_cseq_data_len = sizeof(exepected_cseq_data);

        // setup test midi file
        midi_file = MidiFile_new(MIDI_FORMAT_SIMULTANEOUS);
        midi_file->num_tracks = 1;
        
        midi_track = MidiTrack_new(0);
        midi_track->data = (uint8_t *)malloc_zero(1, midi_data_len);
        memcpy(midi_track->data, midi_data, midi_data_len);
        midi_track->ck_data_size = midi_data_len;

        midi_file->tracks = (struct MidiTrack **)malloc_zero(1, sizeof(struct MidiTrack *));
        midi_file->tracks[0] = midi_track;

        // done with setup. execute test.
        // use default (naive) pattern substitution
        cseq_file = CseqFile_from_MidiFile(midi_file, NULL);

        // compare
        pass_single = exepected_cseq_data_len == (int)cseq_file->compressed_data_len;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail exepected_cseq_data_len: expected %d, actual %d\n", __func__, __LINE__, exepected_cseq_data_len, (int)cseq_file->compressed_data_len);
        }

        pass_single = 1;
        for (i=0; i<exepected_cseq_data_len && i<(int)cseq_file->compressed_data_len; i++)
        {
            pass_single &= exepected_cseq_data[i] == cseq_file->compressed_data[i];
            pass &= pass_single;
        }

        if (!pass_single || (exepected_cseq_data_len != (int)cseq_file->compressed_data_len))
        {
            printf("%s %d> fail seq\n", __func__, __LINE__);
            print_expected_vs_actual_arr(exepected_cseq_data, exepected_cseq_data_len, cseq_file->compressed_data, cseq_file->compressed_data_len);
        }

        MidiFile_free(midi_file);
        CseqFile_free(cseq_file);

        if (pass == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d> fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("round trip seq -- 7\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;
        
        struct CseqFile *cseq_file;
        struct MidiFile *midi_file;
        struct MidiTrack *midi_track;
        struct CseqFile *result_cseq_file;
        struct MidiConvertOptions *convert_options;
        int i;

        uint8_t seq_data[] = {
            0x00, 0xCA, 0x2F, 0x02, 0xBA, 0x07, 0x50, 0x01,
            0x0A, 0x5A, 0x01, 0x5B, 0x14, 0x01, 0x9A, 0x42,
            0x35, 0x60, 0x5B, 0x44, 0x34, 0x60, 0x60, 0x42,
            0x34, 0x60, 0x60, 0x44, 0x34, 0x60, 0x60, 0x44,
            0x34, 0x60, 0x60, 0x42, 0x34, 0x60, 0x60, 0x42,
            0x34, 0x60, 0x60, 0x44, 0x34, 0x60, 0x60, 0x42,
            0x34, 0x60, 0x60, 0x44, 0x34, 0x60, 0x60, 0x44,
            0x34, 0x60, 0x60, 0x42, 0x34, 0x60, 0x60, 0x42,
            0x34, 0x60, 0x60, 0x44, 0x34, 0x60, 0x60, 0x42,
            0x34, 0x60, 0x60, 0x44, 0x34, 0x60, 0x61, 0x42,
            0x35, 0x60, 0x5F, 0x44, 0x34, 0x60, 0x60, 0x42,
            0x34, 0x60, 0x60, 0x44, 0xFE, 0x00, 0x44, 0x07,
            0x42, 0xFE, 0x00, 0x4D, 0x0B, 0xFE, 0x00, 0x4E,
            0x1F, 0x61, 0x42, 0x35, 0x60, 0x5F, 0xFE, 0x00,
            0x5B, 0x49, 0xFE, 0x00, 0x56, 0x32, 0xFE, 0x00,
            0x50, 0x1A, 0x33, 0x60, 0x60, 0x44, 0x33, 0x60,
            0x60, 0x42, 0x33, 0x60, 0x60, 0x44, 0x33, 0x60,
            0x60, 0x44, 0x33, 0x60, 0x60, 0x42, 0x33, 0x60,
            0x60, 0x42, 0x33, 0x60, 0x60, 0x44, 0x33, 0x60,
            0x60, 0x42, 0x33, 0x60, 0x60, 0x44, 0x33, 0x60,
            0x60, 0x42, 0x31, 0x60, 0x60, 0x44, 0x31, 0x60,
            0x60, 0x42, 0x32, 0x60, 0x60, 0x44, 0x32, 0x60,
            0x60, 0x44, 0x33, 0x60, 0x60, 0x42, 0x33, 0xFE,
            0x00, 0xA2, 0x07, 0x35, 0x60, 0x60, 0x42, 0x35,
            0x60, 0x60, 0x44, 0x36, 0x60, 0x60, 0x44, 0x36,
            0x60, 0x60, 0x42, 0x37, 0x60, 0x60, 0x42, 0x37,
            0x60, 0x60, 0x44, 0x38, 0x60, 0x60, 0x42, 0x38,
            0x60, 0x60, 0x44, 0x3A, 0x60, 0x60, 0x42, 0x3C,
            0x60, 0x60, 0x44, 0x3E, 0x60, 0x60, 0x42, 0x40,
            0x60, 0x60, 0x44, 0x42, 0x60, 0x60, 0x44, 0x44,
            0x60, 0x60, 0x42, 0x45, 0x60, 0x60, 0x42, 0x47,
            0x60, 0x60, 0x44, 0x49, 0x60, 0x60, 0x42, 0x4B,
            0x60, 0x60, 0x44, 0x4D, 0x60, 0x60, 0x44, 0x4F,
            0x60, 0x60, 0x42, 0x51, 0x60, 0x60, 0x42, 0x52,
            0x60, 0x60, 0x44, 0x54, 0x60, 0x60, 0x42, 0x56,
            0x60, 0x60, 0x44, 0x58, 0x60, 0x60, 0xFF, 0x2E,
            0x00, 0xFF, 0x02, 0x9A, 0x42, 0x3F, 0x60, 0x5E,
            0x44, 0x3E, 0x60, 0x60, 0x42, 0x3E, 0x60, 0x60,
            0x44, 0x3D, 0x60, 0x60, 0x44, 0x3C, 0x60, 0x60,
            0x42, 0x3B, 0x60, 0x60, 0x42, 0x3B, 0x60, 0x60,
            0x44, 0x3B, 0x60, 0x60, 0x42, 0x3B, 0x60, 0x60,
            0x44, 0x3A, 0xFE, 0x00, 0x72, 0x07, 0x39, 0x60,
            0x60, 0x42, 0x39, 0xFE, 0x00, 0x83, 0x07, 0x37,
            0x60, 0x60, 0x44, 0x37, 0x60, 0x60, 0xFF, 0x2D,
            0x3F, 0x3F, 0x00, 0x00, 0x00, 0x44, 0x01, 0x9A,
            0xFE, 0x01, 0x19, 0x0D, 0xFE, 0x01, 0x50, 0x40,
            0xFE, 0x01, 0x54, 0x40, 0xFE, 0x01, 0x58, 0x40,
            0xFE, 0x01, 0x5C, 0x32, 0xFE, 0x01, 0x56, 0x1A,
            0xFE, 0x01, 0x06, 0x3D, 0xFE, 0x01, 0x6F, 0x07,
            0xFE, 0x00, 0xCD, 0x63, 0xFF, 0x2D, 0xFF, 0xFF,
            0x00, 0x00, 0x00, 0x72, 0x00, 0xFF, 0x2F
        };
        int seq_data_len = sizeof(seq_data);

        // setup

        cseq_file = CseqFile_new();
        cseq_file->non_empty_num_tracks = 1;
        cseq_file->track_lengths[10] = seq_data_len;
        cseq_file->compressed_data_len = seq_data_len;
        cseq_file->compressed_data = (uint8_t *)malloc_zero(1, cseq_file->compressed_data_len);
        // initialy the track data offsets are read from the file, which
        // includes the length of the header. This is subtracted out when
        // accessing the data.
        cseq_file->track_offset[10] = CSEQ_FILE_HEADER_SIZE_BYTES;
        memcpy(cseq_file->compressed_data, seq_data, seq_data_len);

        convert_options = MidiConvertOptions_new();
        convert_options->sysex_seq_loops = 1;

        // execute
        midi_file = MidiFile_from_CseqFile(cseq_file, convert_options);

        convert_options->use_pattern_marker_file = 1;
        convert_options->pattern_marker_filename = "test_cases/midi/patterns0007.csv";

        result_cseq_file = CseqFile_from_MidiFile(midi_file, convert_options);

        if (midi_file == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> midi_file is NULL\n", __func__, __LINE__);
        }

        if (midi_file->tracks[0] == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> midi_file->tracks[0] is NULL\n", __func__, __LINE__);
        }

        if (result_cseq_file == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> result_cseq_file is NULL\n", __func__, __LINE__);
        }

        midi_track = midi_file->tracks[0];

        // compare
        pass_single = midi_track->ck_data_size > 0;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_track->ck_data_size is zero\n", __func__, __LINE__);
        }

        pass_single = result_cseq_file->track_lengths[10] == (size_t)seq_data_len;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail result_cseq_file->track_lengths[10]: expected %d, actual %ld\n", __func__, __LINE__, seq_data_len, result_cseq_file->track_lengths[10]);
        }

        pass_single = 1;
        for (i=0; i<seq_data_len && i<(int)result_cseq_file->track_lengths[10]; i++)
        {
            pass_single &= seq_data[i] == result_cseq_file->compressed_data[i];
            pass &= pass_single;
        }

        if (!pass_single)
        {
            printf("%s %d> fail track byte match\n", __func__, __LINE__);
            print_expected_vs_actual_arr(seq_data, seq_data_len, result_cseq_file->compressed_data, result_cseq_file->track_lengths[10]);
        }

        // cleanup
        MidiConvertOptions_free(convert_options);
        CseqFile_free(cseq_file);
        CseqFile_free(result_cseq_file);
        MidiFile_free(midi_file);

        if (pass == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d> fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("round trip seq -- 8\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;
        
        struct CseqFile *cseq_file;
        struct MidiFile *midi_file;
        struct MidiTrack *midi_track;
        struct CseqFile *result_cseq_file;
        struct MidiConvertOptions *convert_options;
        int i;

        uint8_t seq_data[] = {
            0x8C, 0x00, 0xCF, 0x1D, 0x83, 0x00, 0xBF, 0x07,
            0x4B, 0x83, 0x00, 0x5B, 0x3C, 0xC2, 0x00, 0xFF,
            0x2E, 0x00, 0xFF, 0x02, 0x9F, 0x25, 0x7F, 0x81,
            0x40, 0x82, 0x7E, 0x25, 0x7F, 0x81, 0x40, 0x81,
            0x40, 0x25, 0x5A, 0x86, 0x00, 0x57, 0x25, 0x6B,
            0x1E, 0x4D, 0x25, 0x5F, 0x13, 0x1C, 0x25, 0x7F,
            0x1A, 0x59, 0x25, 0x65, 0x25, 0x66, 0x25, 0x5F,
            0x0C, 0x66, 0x25, 0x6B, 0x0F, 0x5B, 0x25, 0x7F,
            0x0E, 0x81, 0x40, 0x25, 0x7F, 0x83, 0x02, 0x65,
            0x25, 0x5C, 0x0D, 0x20, 0x25, 0x4D, 0x12, 0x17,
            0x25, 0x62, 0x1F, 0x26, 0x25, 0x7F, 0x2A, 0x82,
            0x7E, 0x25, 0x7F, 0x81, 0x40, 0x81, 0x40, 0x25,
            0x5A, 0x86, 0x00, 0x57, 0x25, 0x6B, 0x1E, 0x4D,
            0x25, 0x5F, 0x13, 0x1C, 0x25, 0x7F, 0x1A, 0x59,
            0x25, 0x65, 0x25, 0x66, 0x25, 0x5F, 0x0C, 0x66,
            0x25, 0x6B, 0x0F, 0x5B, 0x25, 0x7F, 0x0E, 0x81,
            0x40, 0x25, 0x7F, 0x83, 0x02, 0x65, 0x25, 0x5C,
            0x0D, 0x20, 0x25, 0x4D, 0x12, 0x17, 0x25, 0x62,
            0x1F, 0x24, 0xFF, 0x2E, 0x03, 0xFF, 0x02, 0x9F,
            0x25, 0x7F, 0x2A, 0xFE, 0x00, 0x82, 0x3A, 0x24,
            0xFF, 0x2D, 0x3B, 0x3B, 0x00, 0x00, 0x00, 0x12,
            0x02, 0x9F, 0x25, 0x7F, 0x2A, 0xFE, 0x00, 0x94,
            0x6A, 0x81, 0x6C, 0xFE, 0x00, 0x6C, 0x0C, 0xD4,
            0x24, 0xFF, 0x2D, 0xFF, 0xFF, 0x00, 0x00, 0x00,
            0xAE, 0x00, 0xFF, 0x2F
        };
        int seq_data_len = sizeof(seq_data);

        // setup

        cseq_file = CseqFile_new();
        cseq_file->non_empty_num_tracks = 1;
        cseq_file->track_lengths[15] = seq_data_len;
        cseq_file->compressed_data_len = seq_data_len;
        cseq_file->compressed_data = (uint8_t *)malloc_zero(1, cseq_file->compressed_data_len);
        // initialy the track data offsets are read from the file, which
        // includes the length of the header. This is subtracted out when
        // accessing the data.
        cseq_file->track_offset[15] = CSEQ_FILE_HEADER_SIZE_BYTES;
        memcpy(cseq_file->compressed_data, seq_data, seq_data_len);

        convert_options = MidiConvertOptions_new();
        convert_options->sysex_seq_loops = 1;

        // execute
        midi_file = MidiFile_from_CseqFile(cseq_file, convert_options);

        convert_options->use_pattern_marker_file = 1;
        convert_options->pattern_marker_filename = "test_cases/midi/patterns0008.csv";

        result_cseq_file = CseqFile_from_MidiFile(midi_file, convert_options);

        if (midi_file == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> midi_file is NULL\n", __func__, __LINE__);
        }

        if (midi_file->tracks[0] == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> midi_file->tracks[0] is NULL\n", __func__, __LINE__);
        }

        if (result_cseq_file == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> result_cseq_file is NULL\n", __func__, __LINE__);
        }

        midi_track = midi_file->tracks[0];

        // compare
        pass_single = midi_track->ck_data_size > 0;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_track->ck_data_size is zero\n", __func__, __LINE__);
        }

        pass_single = result_cseq_file->track_lengths[15] == (size_t)seq_data_len;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail result_cseq_file->track_lengths[15]: expected %d, actual %ld\n", __func__, __LINE__, seq_data_len, result_cseq_file->track_lengths[15]);
        }

        pass_single = 1;
        for (i=0; i<seq_data_len && i<(int)result_cseq_file->track_lengths[15]; i++)
        {
            pass_single &= seq_data[i] == result_cseq_file->compressed_data[i];
            pass &= pass_single;
        }

        if (!pass_single)
        {
            printf("%s %d> fail track byte match\n", __func__, __LINE__);
            print_expected_vs_actual_arr(seq_data, seq_data_len, result_cseq_file->compressed_data, result_cseq_file->track_lengths[15]);
        }

        // cleanup
        MidiConvertOptions_free(convert_options);
        CseqFile_free(cseq_file);
        CseqFile_free(result_cseq_file);
        MidiFile_free(midi_file);

        if (pass == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d> fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }
}