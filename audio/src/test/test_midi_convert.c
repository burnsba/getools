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

        // done with setup. execute test.
        cseq_file = CseqFile_from_MidiFile(midi_file);

        pass_single = cseq_file->division == midi_file->division;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail division: expected %d, actual %d\n", __func__, __LINE__, midi_file->division, cseq_file->division);
        }

        pass_single = 1;
        for (i=0; i<seq_track_len; i++)
        {
            pass_single &= seq_track_expected[i] == cseq_file->compressed_data[i];
            pass &= pass_single;
        }

        if (!pass_single)
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
}