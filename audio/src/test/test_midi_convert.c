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
    if(0)
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

        // done with setup. execute test.
        cseq_file = CseqFile_from_MidiFile(midi_file, 0, 0);

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
            int color_flag = 0;
            printf("%s %d> fail seq\n", __func__, __LINE__);
            printf("expected\n");
            for (i=0; i<seq_track_len; i++)
            {
                color_flag = 0;
                if (i < (int)cseq_file->compressed_data_len && seq_track_expected[i] != cseq_file->compressed_data[i])
                {
                    printf("\033[32m");
                    color_flag = 1;
                }
                printf("0x%02x ", seq_track_expected[i]);
                if (color_flag)
                {
                    printf("\033[39m");
                    color_flag = 0;
                }
                if (((i+1)%8)==0)
                {
                    printf("\n");
                }
            }
            printf("\n");
            printf("actual\n");
            for (i=0; i<(int)cseq_file->compressed_data_len; i++)
            {
                color_flag = 0;
                if (i < seq_track_len && seq_track_expected[i] != cseq_file->compressed_data[i])
                {
                    printf("\033[31m");
                    color_flag = 1;
                }
                printf("0x%02x ", cseq_file->compressed_data[i]);
                if (color_flag)
                {
                    printf("\033[39m");
                    color_flag = 0;
                }
                if (((i+1)%8)==0)
                {
                    printf("\n");
                }
            }
            printf("\n");
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
            printf("%s %d>fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    if(0)
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
        midi_file = MidiFile_from_CseqFile(cseq_file, NULL, 0);

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
            int color_flag = 0;
            printf("%s %d> fail midi\n", __func__, __LINE__);
            printf("expected\n");
            for (i=0; i<midi_raw_track_data_len; i++)
            {
                color_flag = 0;
                if (i < (int)midi_track->ck_data_size && midi_raw_track_expected[i] != midi_track->data[i])
                {
                    printf("\033[32m");
                    color_flag = 1;
                }
                printf("0x%02x ", midi_raw_track_expected[i]);
                if (color_flag)
                {
                    printf("\033[39m");
                    color_flag = 0;
                }
                if (((i+1)%8)==0)
                {
                    printf("\n");
                }
            }
            printf("\n");
            printf("actual\n");
            for (i=0; i<(int)midi_track->ck_data_size; i++)
            {
                color_flag = 0;
                if (i < midi_raw_track_data_len && midi_raw_track_expected[i] != midi_track->data[i])
                {
                    printf("\033[31m");
                    color_flag = 1;
                }
                printf("0x%02x ", midi_track->data[i]);
                if (color_flag)
                {
                    printf("\033[39m");
                    color_flag = 0;
                }
                if (((i+1)%8)==0)
                {
                    printf("\n");
                }
            }
            printf("\n");
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
            printf("%s %d>fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    if(0)
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
        cseq_file = CseqFile_from_MidiFile(midi_file, 0, 0);

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
            int color_flag = 0;
            printf("%s %d> fail seq\n", __func__, __LINE__);
            printf("expected\n");
            for (i=0; i<seq_track_len; i++)
            {
                color_flag = 0;
                if (i < (int)cseq_file->compressed_data_len && seq_track_expected[i] != cseq_file->compressed_data[i])
                {
                    printf("\033[32m");
                    color_flag = 1;
                }
                printf("0x%02x ", seq_track_expected[i]);
                if (color_flag)
                {
                    printf("\033[39m");
                    color_flag = 0;
                }
                if (((i+1)%8)==0)
                {
                    printf("\n");
                }
            }
            printf("\n");
            printf("actual\n");
            for (i=0; i<(int)cseq_file->compressed_data_len; i++)
            {
                color_flag = 0;
                if (i < seq_track_len && seq_track_expected[i] != cseq_file->compressed_data[i])
                {
                    printf("\033[31m");
                    color_flag = 1;
                }
                printf("0x%02x ", cseq_file->compressed_data[i]);
                if (color_flag)
                {
                    printf("\033[39m");
                    color_flag = 0;
                }
                if (((i+1)%8)==0)
                {
                    printf("\n");
                }
            }
            printf("\n");
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
            printf("%s %d>fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("convert seq roll to cseq -- 4\n");
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
        memcpy(gtrack->cseq_data, seq_data, seq_data_len);
        gtrack->cseq_track_size_bytes = seq_data_len;
        gtrack->cseq_data_len = seq_data_len;

        current_buffer_pos = 0;
        buffer_len = seq_data_len;

        // execute
        GmidTrack_roll(gtrack, write_buffer, &current_buffer_pos, buffer_len, 1);

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
            int color_flag = 0;
            printf("%s %d> fail seq\n", __func__, __LINE__);
            printf("expected\n");
            for (i=0; i<exepected_cseq_data_len; i++)
            {
                color_flag = 0;
                if (i < (int)gtrack->cseq_data_len && exepected_cseq_data[i] != gtrack->cseq_data[i])
                {
                    if (exepected_cseq_data[i] == 0xfe)
                    {
                        printf("\033[31;43m");
                    }
                    else if (gtrack->cseq_data[i] == 0xfe)
                    {
                        printf("\033[31;106m");
                    }
                    else
                    {
                        printf("\033[31m");
                    }
                    color_flag = 1;
                }
                printf("0x%02x", exepected_cseq_data[i]);
                if (color_flag)
                {
                    printf("\033[39;49m");
                    color_flag = 0;
                }
                printf(" ");
                if (((i+1)%8)==0)
                {
                    printf("\n");
                }
            }
            printf("\n");
            printf("actual\n");
            for (i=0; i<(int)gtrack->cseq_data_len; i++)
            {
                color_flag = 0;
                if (i < exepected_cseq_data_len && exepected_cseq_data[i] != gtrack->cseq_data[i])
                {
                    if (exepected_cseq_data[i] == 0xfe)
                    {
                        printf("\033[31;106m");
                    }
                    else if (gtrack->cseq_data[i] == 0xfe)
                    {
                        printf("\033[31;43m");
                    }
                    else
                    {
                        printf("\033[31m");
                    }
                    color_flag = 1;
                }
                printf("0x%02x", gtrack->cseq_data[i]);
                if (color_flag)
                {
                    printf("\033[39;49m");
                    color_flag = 0;
                }
                printf(" ");
                if (((i+1)%8)==0)
                {
                    printf("\n");
                }
            }
            printf("\n");
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
            printf("%s %d>fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }
}