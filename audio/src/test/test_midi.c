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

static void test_midi_parser(int *run_count, int *pass_count, int *fail_count);

// end forward declarations

void midi_all(int *run_count, int *pass_count, int *fail_count)
{
    int sub_count;
    int local_run_count = 0;

    sub_count = 0;
    test_midi_parser(&sub_count, pass_count, fail_count);
    local_run_count += sub_count;

    sub_count = 0;
    midi_convert_all(&sub_count, pass_count, fail_count);
    local_run_count += sub_count;

    *run_count = *run_count + local_run_count;
}

#define DEBUG_PARSE_SEQ_BYTES_TO_EVENT_LIST 0
void parse_seq_bytes_to_event_list(uint8_t *data, size_t buffer_len, struct llist_root *event_list)
{
    struct llist_node *node;
    struct GmidEvent *event;
    size_t track_pos;
    int32_t command;
    long absolute_time;

#if DEBUG_PARSE_SEQ_BYTES_TO_EVENT_LIST
    char *debug_printf_buffer;
    debug_printf_buffer = (char *)malloc_zero(1, WRITE_BUFFER_LEN);
#endif

    absolute_time = 0;
    command = 0;
    track_pos = 0;

    while (track_pos < buffer_len)
    {
        int bytes_read = 0;
        
        // track position will be updated according to how many bytes read.
        event = GmidEvent_new_from_buffer(data, &track_pos, buffer_len, MIDI_IMPLEMENTATION_SEQ, command, &bytes_read);

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

#if DEBUG_PARSE_SEQ_BYTES_TO_EVENT_LIST
        if (g_verbosity >= VERBOSE_DEBUG)
        {
            memset(debug_printf_buffer, 0, WRITE_BUFFER_LEN);
            size_t debug_str_len = GmidEvent_to_string(event, debug_printf_buffer, WRITE_BUFFER_LEN - 2, MIDI_IMPLEMENTATION_SEQ);
            debug_printf_buffer[debug_str_len] = '\n';
            fflush_string(stdout, debug_printf_buffer);
        }
#endif

        command = GmidEvent_get_midi_command(event);

        // only allow running status for the "regular" MIDI commands
        if ((command & 0xffffff00) != 0 || (command & 0xffffffff) == 0xff)
        {
            command = 0;
        }

        node = llist_node_new();
        node->data = event;
        llist_root_append_node(event_list, node);
    }

#if DEBUG_PARSE_SEQ_BYTES_TO_EVENT_LIST
    free(debug_printf_buffer);
#endif
}

void test_midi_parser(int *run_count, int *pass_count, int *fail_count)
{
    /**
     * test cases:
     * =================================
     * MIDI, new status:
     * 
     * MIDI: note off
     * MIDI: note on
     * MIDI: polyphonic key pressure
     * MIDI: controller: volume
     * MIDI: controller: pan
     * MIDI: controller: sustain
     * MIDI: controller: effects
     * MIDI: controller: loop start
     * MIDI: controller: loop end
     * MIDI: controller: loop count 0
     * MIDI: controller: loop count 128
     * MIDI: program control change
     * MIDI: meta: tempo
     * MIDI: meta: end of track
     * 
     * seq, new status:
     * 
     * seq: note on
     * seq: polyphonic key pressure
     * seq: controller: volume
     * seq: controller: pan
     * seq: controller: sustain
     * seq: controller: effects
     * seq: program control change
     * seq: meta: tempo
     * seq: meta: loop start
     * seq: meta: loop end
     * seq: meta: end of track
     * 
     * MIDI, running status:
     * 
     * MIDI: note on
     * MIDI: controller: volume
     * MIDI: controller: pan
     * MIDI: controller: sustain
     * MIDI: controller: effects
     * MIDI: program control change
     * 
     * seq, running status:
     * 
     * seq: note on
     * seq: controller: volume
     * seq: controller: pan
     * seq: controller: sustain
     * seq: controller: effects
     * seq: program control change
    */

    {
        printf("MIDI parse: standard MIDI, command note off\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_note_key = 64;
        int command_note_velocity = 200;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(MIDI_COMMAND_BYTE_NOTE_OFF | command_channel);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_note_key);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_note_velocity);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_STANDARD;
        current_command = 0;
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_NOTE_OFF;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_NOTE_OFF, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_NOTE_OFF;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_NOTE_OFF, event->midi_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_note_key;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_note_key, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_note_velocity;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_note_velocity, event->midi_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 0;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 0, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: standard MIDI, command note on, new status\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_note_key = 64;
        int command_note_velocity = 200;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(MIDI_COMMAND_BYTE_NOTE_ON | command_channel);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_note_key);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_note_velocity);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_STANDARD;
        current_command = 0;
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_NOTE_ON;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_NOTE_ON, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_NOTE_ON;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_NOTE_ON, event->midi_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_note_key;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_note_key, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_note_velocity;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_note_velocity, event->midi_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 0;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: standard MIDI, command note on, running status\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_note_key = 64;
        int command_note_velocity = 200;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(command_note_key);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_note_velocity);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_STANDARD;
        current_command = (MIDI_COMMAND_BYTE_NOTE_ON | command_channel);
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_NOTE_ON;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_NOTE_ON, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_NOTE_ON;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_NOTE_ON, event->midi_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_note_key;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_note_key, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_note_velocity;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_note_velocity, event->midi_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 0;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 0, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: seq, command note on, new status\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_note_key = 64;
        int command_note_velocity = 200;
        int command_note_duration = 4097;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(CSEQ_COMMAND_BYTE_NOTE_ON | command_channel);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_note_key);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_note_velocity);
        test_buffer_pos += 1;
        int32_to_varint(command_note_duration, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        test_buffer_pos += varint.num_bytes;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_SEQ;
        current_command = 0;
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == CSEQ_COMMAND_BYTE_NOTE_ON;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, CSEQ_COMMAND_BYTE_NOTE_ON, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_NOTE_ON;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_NOTE_ON, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == CSEQ_COMMAND_NUM_PARAM_NOTE_ON;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, CSEQ_COMMAND_NUM_PARAM_NOTE_ON, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_note_key;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_note_key, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_note_velocity;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_note_velocity, event->midi_command_parameters[1]);
        }

        pass_single = event->cseq_command_parameters[0] == command_note_key;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_note_key, event->cseq_command_parameters[0]);
        }

        pass_single = event->cseq_command_parameters[1] == command_note_velocity;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_note_velocity, event->cseq_command_parameters[1]);
        }

        pass_single = event->cseq_command_parameters[2] == command_note_duration;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_note_duration, event->cseq_command_parameters[2]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: seq, command note on, running status\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_note_key = 64;
        int command_note_velocity = 200;
        int command_note_duration = 4097;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(command_note_key);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_note_velocity);
        test_buffer_pos += 1;
        int32_to_varint(command_note_duration, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        test_buffer_pos += varint.num_bytes;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_SEQ;
        current_command = (CSEQ_COMMAND_BYTE_NOTE_ON | command_channel);
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == CSEQ_COMMAND_BYTE_NOTE_ON;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, CSEQ_COMMAND_BYTE_NOTE_ON, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_NOTE_ON;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_NOTE_ON, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == CSEQ_COMMAND_NUM_PARAM_NOTE_ON;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, CSEQ_COMMAND_NUM_PARAM_NOTE_ON, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_note_key;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_note_key, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_note_velocity;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_note_velocity, event->midi_command_parameters[1]);
        }

        pass_single = event->cseq_command_parameters[0] == command_note_key;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_note_key, event->cseq_command_parameters[0]);
        }

        pass_single = event->cseq_command_parameters[1] == command_note_velocity;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_note_velocity, event->cseq_command_parameters[1]);
        }

        pass_single = event->cseq_command_parameters[2] == command_note_duration;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_note_duration, event->cseq_command_parameters[2]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: standard MIDI, command polyphonic key pressure\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_note_key = 64;
        int command_note_pressure = 200;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(MIDI_COMMAND_BYTE_POLYPHONIC_PRESSURE | command_channel);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_note_key);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_note_pressure);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_STANDARD;
        current_command = 0;
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_POLYPHONIC_PRESSURE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_POLYPHONIC_PRESSURE, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_NOTE_OFF;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_NOTE_OFF, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == MIDI_COMMAND_NUM_PARAM_NOTE_OFF;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_NOTE_OFF, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_note_key;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_note_key, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_note_pressure;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_note_pressure, event->midi_command_parameters[1]);
        }

        pass_single = event->cseq_command_parameters[0] == command_note_key;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_note_key, event->cseq_command_parameters[0]);
        }

        pass_single = event->cseq_command_parameters[1] == command_note_pressure;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_note_pressure, event->cseq_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: seq, command polyphonic key pressure\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_note_key = 64;
        int command_note_pressure = 200;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(MIDI_COMMAND_BYTE_POLYPHONIC_PRESSURE | command_channel);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_note_key);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_note_pressure);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_SEQ;
        current_command = 0;
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_POLYPHONIC_PRESSURE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_POLYPHONIC_PRESSURE, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_NOTE_OFF;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_NOTE_OFF, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == MIDI_COMMAND_NUM_PARAM_NOTE_OFF;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_NOTE_OFF, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_note_key;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_note_key, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_note_pressure;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_note_pressure, event->midi_command_parameters[1]);
        }

        pass_single = event->cseq_command_parameters[0] == command_note_key;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_note_key, event->cseq_command_parameters[0]);
        }

        pass_single = event->cseq_command_parameters[1] == command_note_pressure;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_note_pressure, event->cseq_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: standard MIDI, command controller: volume, new status\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_controller_number = MIDI_CONTROLLER_CHANNEL_VOLUME;
        int command_controller_value = 50;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(MIDI_COMMAND_BYTE_CONTROL_CHANGE | command_channel);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_number);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_value);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_STANDARD;
        current_command = 0;
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_CONTROL_CHANGE, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->midi_command_parameters[1]);
        }

        pass_single = event->cseq_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->cseq_command_parameters[0]);
        }

        pass_single = event->cseq_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->cseq_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: standard MIDI, command controller: volume, running status\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_controller_number = MIDI_CONTROLLER_CHANNEL_VOLUME;
        int command_controller_value = 50;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_number);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_value);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_STANDARD;
        current_command = (MIDI_COMMAND_BYTE_CONTROL_CHANGE | command_channel);
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_CONTROL_CHANGE, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->midi_command_parameters[1]);
        }

        pass_single = event->cseq_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->cseq_command_parameters[0]);
        }

        pass_single = event->cseq_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->cseq_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: seq, command controller: volume, new status\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_controller_number = MIDI_CONTROLLER_CHANNEL_VOLUME;
        int command_controller_value = 50;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(MIDI_COMMAND_BYTE_CONTROL_CHANGE | command_channel);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_number);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_value);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_SEQ;
        current_command = 0;
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_CONTROL_CHANGE, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->midi_command_parameters[1]);
        }

        pass_single = event->cseq_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->cseq_command_parameters[0]);
        }

        pass_single = event->cseq_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->cseq_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: seq, command controller: volume, running status\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_controller_number = MIDI_CONTROLLER_CHANNEL_VOLUME;
        int command_controller_value = 50;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_number);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_value);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_SEQ;
        current_command = (MIDI_COMMAND_BYTE_CONTROL_CHANGE | command_channel);
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_CONTROL_CHANGE, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->midi_command_parameters[1]);
        }

        pass_single = event->cseq_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->cseq_command_parameters[0]);
        }

        pass_single = event->cseq_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->cseq_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: standard MIDI, command controller: pan, new status\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_controller_number = MIDI_CONTROLLER_CHANNEL_PAN;
        int command_controller_value = 50;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(MIDI_COMMAND_BYTE_CONTROL_CHANGE | command_channel);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_number);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_value);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_STANDARD;
        current_command = 0;
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_CONTROL_CHANGE, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->midi_command_parameters[1]);
        }

        pass_single = event->cseq_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->cseq_command_parameters[0]);
        }

        pass_single = event->cseq_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->cseq_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: standard MIDI, command controller: pan, running status\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_controller_number = MIDI_CONTROLLER_CHANNEL_PAN;
        int command_controller_value = 50;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_number);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_value);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_STANDARD;
        current_command = (MIDI_COMMAND_BYTE_CONTROL_CHANGE | command_channel);
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_CONTROL_CHANGE, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->midi_command_parameters[1]);
        }

        pass_single = event->cseq_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->cseq_command_parameters[0]);
        }

        pass_single = event->cseq_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->cseq_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: seq, command controller: pan, new status\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_controller_number = MIDI_CONTROLLER_CHANNEL_PAN;
        int command_controller_value = 50;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(MIDI_COMMAND_BYTE_CONTROL_CHANGE | command_channel);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_number);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_value);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_SEQ;
        current_command = 0;
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_CONTROL_CHANGE, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->midi_command_parameters[1]);
        }

        pass_single = event->cseq_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->cseq_command_parameters[0]);
        }

        pass_single = event->cseq_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->cseq_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: seq, command controller: pan, running status\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_controller_number = MIDI_CONTROLLER_CHANNEL_PAN;
        int command_controller_value = 50;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_number);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_value);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_SEQ;
        current_command = (MIDI_COMMAND_BYTE_CONTROL_CHANGE | command_channel);
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_CONTROL_CHANGE, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->midi_command_parameters[1]);
        }

        pass_single = event->cseq_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->cseq_command_parameters[0]);
        }

        pass_single = event->cseq_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->cseq_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: standard MIDI, command controller: sustain, new status\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_controller_number = MIDI_CONTROLLER_SUSTAIN;
        int command_controller_value = 50;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(MIDI_COMMAND_BYTE_CONTROL_CHANGE | command_channel);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_number);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_value);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_STANDARD;
        current_command = 0;
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_CONTROL_CHANGE, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->midi_command_parameters[1]);
        }

        pass_single = event->cseq_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->cseq_command_parameters[0]);
        }

        pass_single = event->cseq_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->cseq_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: standard MIDI, command controller: sustain, running status\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_controller_number = MIDI_CONTROLLER_SUSTAIN;
        int command_controller_value = 50;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_number);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_value);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_STANDARD;
        current_command = (MIDI_COMMAND_BYTE_CONTROL_CHANGE | command_channel);
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_CONTROL_CHANGE, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->midi_command_parameters[1]);
        }

        pass_single = event->cseq_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->cseq_command_parameters[0]);
        }

        pass_single = event->cseq_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->cseq_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: seq, command controller: sustain, new status\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_controller_number = MIDI_CONTROLLER_SUSTAIN;
        int command_controller_value = 50;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(MIDI_COMMAND_BYTE_CONTROL_CHANGE | command_channel);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_number);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_value);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_SEQ;
        current_command = 0;
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_CONTROL_CHANGE, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->midi_command_parameters[1]);
        }

        pass_single = event->cseq_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->cseq_command_parameters[0]);
        }

        pass_single = event->cseq_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->cseq_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: seq, command controller: sustain, running status\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_controller_number = MIDI_CONTROLLER_SUSTAIN;
        int command_controller_value = 50;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_number);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_value);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_SEQ;
        current_command = (MIDI_COMMAND_BYTE_CONTROL_CHANGE | command_channel);
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_CONTROL_CHANGE, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->midi_command_parameters[1]);
        }

        pass_single = event->cseq_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->cseq_command_parameters[0]);
        }

        pass_single = event->cseq_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->cseq_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: standard MIDI, command controller: effects, new status\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_controller_number = MIDI_CONTROLLER_EFFECTS_1_DEPTH;
        int command_controller_value = 50;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(MIDI_COMMAND_BYTE_CONTROL_CHANGE | command_channel);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_number);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_value);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_STANDARD;
        current_command = 0;
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_CONTROL_CHANGE, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->midi_command_parameters[1]);
        }

        pass_single = event->cseq_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->cseq_command_parameters[0]);
        }

        pass_single = event->cseq_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->cseq_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: standard MIDI, command controller: effects, running status\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_controller_number = MIDI_CONTROLLER_EFFECTS_1_DEPTH;
        int command_controller_value = 50;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_number);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_value);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_STANDARD;
        current_command = (MIDI_COMMAND_BYTE_CONTROL_CHANGE | command_channel);
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_CONTROL_CHANGE, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->midi_command_parameters[1]);
        }

        pass_single = event->cseq_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->cseq_command_parameters[0]);
        }

        pass_single = event->cseq_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->cseq_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: seq, command controller: effects, new status\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_controller_number = MIDI_CONTROLLER_EFFECTS_1_DEPTH;
        int command_controller_value = 50;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(MIDI_COMMAND_BYTE_CONTROL_CHANGE | command_channel);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_number);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_value);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_SEQ;
        current_command = 0;
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_CONTROL_CHANGE, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->midi_command_parameters[1]);
        }

        pass_single = event->cseq_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->cseq_command_parameters[0]);
        }

        pass_single = event->cseq_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->cseq_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: seq, command controller: effects, running status\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_controller_number = MIDI_CONTROLLER_EFFECTS_1_DEPTH;
        int command_controller_value = 50;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_number);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_value);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_SEQ;
        current_command = (MIDI_COMMAND_BYTE_CONTROL_CHANGE | command_channel);
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_CONTROL_CHANGE, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->midi_command_parameters[1]);
        }

        pass_single = event->cseq_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->cseq_command_parameters[0]);
        }

        pass_single = event->cseq_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->cseq_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: standard MIDI, program control change, new status\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_program = 27;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(MIDI_COMMAND_BYTE_PROGRAM_CHANGE | command_channel);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_program);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_STANDARD;
        current_command = 0;
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_PROGRAM_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_PROGRAM_CHANGE, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_PROGRAM_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_PROGRAM_CHANGE, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == MIDI_COMMAND_NUM_PARAM_PROGRAM_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_PROGRAM_CHANGE, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_program;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_program, event->midi_command_parameters[0]);
        }

        pass_single = event->cseq_command_parameters[0] == command_program;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_program, event->cseq_command_parameters[0]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: standard MIDI, program control change, running status\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_program = 27;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(command_program);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_STANDARD;
        current_command = (MIDI_COMMAND_BYTE_PROGRAM_CHANGE | command_channel);
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_PROGRAM_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_PROGRAM_CHANGE, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_PROGRAM_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_PROGRAM_CHANGE, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == MIDI_COMMAND_NUM_PARAM_PROGRAM_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_PROGRAM_CHANGE, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_program;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_program, event->midi_command_parameters[0]);
        }

        pass_single = event->cseq_command_parameters[0] == command_program;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_program, event->cseq_command_parameters[0]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: seq, program control change, new status\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_program = 27;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(MIDI_COMMAND_BYTE_PROGRAM_CHANGE | command_channel);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_program);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_SEQ;
        current_command = 0;
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_PROGRAM_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_PROGRAM_CHANGE, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_PROGRAM_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_PROGRAM_CHANGE, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == MIDI_COMMAND_NUM_PARAM_PROGRAM_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_PROGRAM_CHANGE, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_program;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_program, event->midi_command_parameters[0]);
        }

        pass_single = event->cseq_command_parameters[0] == command_program;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_program, event->cseq_command_parameters[0]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: seq, program control change, running status\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_program = 27;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(command_program);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_SEQ;
        current_command = (MIDI_COMMAND_BYTE_PROGRAM_CHANGE | command_channel);
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_PROGRAM_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_PROGRAM_CHANGE, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_PROGRAM_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_PROGRAM_CHANGE, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == MIDI_COMMAND_NUM_PARAM_PROGRAM_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_PROGRAM_CHANGE, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_program;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_program, event->midi_command_parameters[0]);
        }

        pass_single = event->cseq_command_parameters[0] == command_program;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_program, event->cseq_command_parameters[0]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: standard MIDI, command controller: loop start\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_controller_number = MIDI_CONTROLLER_LOOP_START;
        int command_controller_value = 4;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(MIDI_COMMAND_BYTE_CONTROL_CHANGE | command_channel);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_number);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_value);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_STANDARD;
        current_command = 0;
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_CONTROL_CHANGE, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->midi_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 0;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 0, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: standard MIDI, command controller: loop end\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_controller_number = MIDI_CONTROLLER_LOOP_END;
        int command_controller_value = 4;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(MIDI_COMMAND_BYTE_CONTROL_CHANGE | command_channel);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_number);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_value);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_STANDARD;
        current_command = 0;
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_CONTROL_CHANGE, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->midi_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 0;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 0, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: standard MIDI, command controller: loop count +0\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_controller_number = MIDI_CONTROLLER_LOOP_COUNT_0;
        int command_controller_value = 4;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(MIDI_COMMAND_BYTE_CONTROL_CHANGE | command_channel);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_number);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_value);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_STANDARD;
        current_command = 0;
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_CONTROL_CHANGE, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->midi_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 0;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 0, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: standard MIDI, command controller: loop count +128\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int command_channel = 1;
        int command_controller_number = MIDI_CONTROLLER_LOOP_COUNT_128;
        int command_controller_value = 4;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(MIDI_COMMAND_BYTE_CONTROL_CHANGE | command_channel);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_number);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(command_controller_value);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_STANDARD;
        current_command = 0;
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_CONTROL_CHANGE, event->command);
        }

        pass_single = event->command_channel == command_channel;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, command_channel, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_CONTROL_CHANGE, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == command_controller_number;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, command_controller_number, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == command_controller_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, command_controller_value, event->midi_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 0;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 0, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: standard MIDI, meta: tempo\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int tempo = (250000) & 0xffffff; // three bytes

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(MIDI_COMMAND_BYTE_META);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(MIDI_COMMAND_BYTE_TEMPO);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(3); // length
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)((tempo >> 16) & 0xff);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)((tempo >> 8) & 0xff);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)((tempo >> 0) & 0xff);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_STANDARD;
        current_command = 0;
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_TEMPO_WITH_META;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_TEMPO_WITH_META, event->command);
        }

        pass_single = event->command_channel == -1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, -1, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_TEMPO;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_TEMPO, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == CSEQ_COMMAND_NUM_PARAM_TEMPO;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, CSEQ_COMMAND_NUM_PARAM_TEMPO, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == 3;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 3, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == tempo;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, tempo, event->midi_command_parameters[1]);
        }

        pass_single = event->cseq_command_parameters[0] == tempo;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, tempo, event->cseq_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 0, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: seq, meta: tempo\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int tempo = (250000) & 0xffffff; // three bytes

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(MIDI_COMMAND_BYTE_META);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(MIDI_COMMAND_BYTE_TEMPO);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)((tempo >> 16) & 0xff);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)((tempo >> 8) & 0xff);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)((tempo >> 0) & 0xff);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_SEQ;
        current_command = 0;
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == CSEQ_COMMAND_BYTE_TEMPO_WITH_META;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, CSEQ_COMMAND_BYTE_TEMPO_WITH_META, event->command);
        }

        pass_single = event->command_channel == -1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, -1, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_TEMPO;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_TEMPO, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == CSEQ_COMMAND_NUM_PARAM_TEMPO;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, CSEQ_COMMAND_NUM_PARAM_TEMPO, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_command_parameters[0] == 3;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 3, event->midi_command_parameters[0]);
        }

        pass_single = event->midi_command_parameters[1] == tempo;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, tempo, event->midi_command_parameters[1]);
        }

        pass_single = event->cseq_command_parameters[0] == tempo;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, tempo, event->cseq_command_parameters[1]);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 0, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: standard MIDI, meta: end of track\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(MIDI_COMMAND_BYTE_META);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(CSEQ_COMMAND_BYTE_END_OF_TRACK);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(0);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_STANDARD;
        current_command = 0;
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_END_OF_TRACK_WITH_META;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_END_OF_TRACK_WITH_META, event->command);
        }

        pass_single = event->command_channel == -1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, -1, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_END_OF_TRACK;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_END_OF_TRACK, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == CSEQ_COMMAND_NUM_PARAM_END_OF_TRACK;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, CSEQ_COMMAND_NUM_PARAM_END_OF_TRACK, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 0, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: seq, meta: end of track\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(MIDI_COMMAND_BYTE_META);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(CSEQ_COMMAND_BYTE_END_OF_TRACK);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_SEQ;
        current_command = 0;
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == MIDI_COMMAND_BYTE_END_OF_TRACK_WITH_META;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_BYTE_END_OF_TRACK_WITH_META, event->command);
        }

        pass_single = event->command_channel == -1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, -1, event->command_channel);
        }

        pass_single = event->midi_command_parameters_len == MIDI_COMMAND_NUM_PARAM_END_OF_TRACK;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, MIDI_COMMAND_NUM_PARAM_END_OF_TRACK, event->midi_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters_len == CSEQ_COMMAND_NUM_PARAM_END_OF_TRACK;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, CSEQ_COMMAND_NUM_PARAM_END_OF_TRACK, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 1, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 0, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: seq, meta: loop start\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int loop_number = 4;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(MIDI_COMMAND_BYTE_META);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(CSEQ_COMMAND_BYTE_LOOP_START);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(loop_number);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(0xff);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_SEQ;
        current_command = 0;
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == CSEQ_COMMAND_BYTE_LOOP_START_WITH_META;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, CSEQ_COMMAND_BYTE_LOOP_START_WITH_META, event->command);
        }

        pass_single = event->command_channel == -1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, -1, event->command_channel);
        }

        pass_single = event->cseq_command_parameters_len == CSEQ_COMMAND_NUM_PARAM_LOOP_START;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, CSEQ_COMMAND_NUM_PARAM_LOOP_START, event->cseq_command_parameters_len);
        }

        pass_single = event->midi_valid == 0;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 0, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 0, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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
        printf("MIDI parse: seq, meta: loop end\n");
        int pass = 1;
        int pass_single;
        *run_count = *run_count + 1;

        struct GmidEvent *event;
        size_t pos;
        size_t pos_expected;
        enum MIDI_IMPLEMENTATION buffer_type;
        int32_t current_command;
        int bytes_read;
        int bytes_read_expected;
        struct var_length_int varint;
        size_t test_buffer_pos;
        int32_t delta_time_varint_expected;

        // setup test data
        uint8_t buffer[20] = { 0 };
        size_t buffer_len = 20;
        int delta_time_expected = 0x200000;
        int loop_count = 17;
        int current_loop_count = loop_count; // current loop count is supposed to be the same as loop count
        int32_t loop_delta = 0x12345678;

        memset(&varint, 0, sizeof(struct var_length_int));
        test_buffer_pos = 0;
        int32_to_varint(delta_time_expected, &varint);
        varint_write_value_big(&buffer[test_buffer_pos], &varint);
        delta_time_varint_expected = varint_get_value_big(&varint);
        test_buffer_pos += varint.num_bytes;
        buffer[test_buffer_pos] = (uint8_t)(MIDI_COMMAND_BYTE_META);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(CSEQ_COMMAND_BYTE_LOOP_END);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(loop_count);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)(current_loop_count);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)((loop_delta >> 24) & 0xff);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)((loop_delta >> 16) & 0xff);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)((loop_delta >> 8) & 0xff);
        test_buffer_pos += 1;
        buffer[test_buffer_pos] = (uint8_t)((loop_delta >> 0) & 0xff);
        test_buffer_pos += 1;
        
        pos_expected = test_buffer_pos;
        bytes_read_expected = test_buffer_pos;

        pos = 0;
        buffer_type = MIDI_IMPLEMENTATION_SEQ;
        current_command = 0;
        bytes_read = 0;

        // execute
        event = GmidEvent_new_from_buffer(buffer, &pos, buffer_len, buffer_type, current_command, &bytes_read);

        // evaluate results
        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->midi_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->midi_delta_time));
        }

        pass_single = delta_time_expected == event->midi_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail midi_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->midi_delta_time.standard_value);
        }

        pass_single = delta_time_varint_expected == (int)varint_get_value_big(&event->cseq_delta_time);
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.value: expected %d, actual %d\n", __func__, __LINE__, delta_time_varint_expected, varint_get_value_big(&event->cseq_delta_time));
        }

        pass_single = delta_time_expected == event->cseq_delta_time.standard_value;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_delta_time.standard_value: expected %d, actual %d\n", __func__, __LINE__, delta_time_expected, event->cseq_delta_time.standard_value);
        }

        pass_single = pos == pos_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail pos: expected %ld, actual %ld\n", __func__, __LINE__, pos_expected, pos);
        }

        pass_single = bytes_read == bytes_read_expected;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail bytes_read: expected %d, actual %d\n", __func__, __LINE__, bytes_read_expected, bytes_read);
        }

        pass_single = event->command == CSEQ_COMMAND_BYTE_LOOP_END_WITH_META;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command: expected %d, actual %d\n", __func__, __LINE__, CSEQ_COMMAND_BYTE_LOOP_END_WITH_META, event->command);
        }

        pass_single = event->command_channel == -1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail command_channel: expected %d, actual %d\n", __func__, __LINE__, -1, event->command_channel);
        }

        pass_single = event->cseq_command_parameters_len == CSEQ_COMMAND_NUM_PARAM_LOOP_END;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail cseq_command_parameters_len: expected %d, actual %d\n", __func__, __LINE__, CSEQ_COMMAND_NUM_PARAM_LOOP_END, event->cseq_command_parameters_len);
        }

        pass_single = event->cseq_command_parameters[0] == loop_count;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, loop_count, event->cseq_command_parameters[0]);
        }

        pass_single = event->cseq_command_parameters[1] == current_loop_count;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, current_loop_count, event->cseq_command_parameters[1]);
        }

        pass_single = event->cseq_command_parameters[2] == loop_delta;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail velocity: expected %d, actual %d\n", __func__, __LINE__, loop_delta, event->cseq_command_parameters[2]);
        }

        pass_single = event->midi_valid == 0;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 0, event->midi_valid);
        }

        pass_single = event->cseq_valid == 1;
        pass &= pass_single;
        if (!pass_single)
        {
            printf("%s %d> fail note: expected %d, actual %d\n", __func__, __LINE__, 0, event->cseq_valid);
        }

        // cleanup
        GmidEvent_free(event);

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