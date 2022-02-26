#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "machine_config.h"
#include "debug.h"
#include "common.h"
#include "gaudio_math.h"
#include "utility.h"
#include "llist.h"
#include "string_hash.h"
#include "int_hash.h"
#include "md5.h"
#include "naudio.h"
#include "test_common.h"

// forward declarations

int parse_inst_default(struct FileInfo *fi);

// end forward declarations

/**
 * Parses file as first described in `0002.inst`
*/
int parse_inst_default(struct FileInfo *fi)
{
    struct ALBankFile *bank_file = ALBankFile_new_from_inst(fi);

    struct ALBank *bank;
    struct ALInstrument *instrument;
    struct ALSound *sound;
    struct ALWaveTable *wavetable;
    struct ALKeyMap *keymap;
    struct ALEnvelope *envelope;
    int i;

    int pass = 1;
    if (bank_file == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: fail: bank_file is NULL\n", __func__, __LINE__);
    }

    if (bank_file->bank_count != 1)
    {
        pass = 0;
        printf("%s %d>fail: bank_file->bank_count=%d, expected 1\n", __func__, __LINE__, bank_file->bank_count);
    }

    if (bank_file->banks == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: fail: bank_file->banks is NULL\n", __func__, __LINE__);
    }

    bank = bank_file->banks[0];

    if (bank == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: bank is NULL\n", __func__, __LINE__);
    }

    if (bank->inst_count != 1)
    {
        pass = 0;
        printf("%s %d>fail: bank->inst_count=%d, expected 1\n", __func__, __LINE__, bank->inst_count);
    }

    if (bank->instruments == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: bank->instruments is NULL\n", __func__, __LINE__);
    }

    instrument = bank->instruments[0];

    if (instrument == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: instrument is NULL\n", __func__, __LINE__);
    }

    if (instrument->sound_count != 3)
    {
        pass = 0;
        printf("%s %d>fail: instrument->sound_count=%d, expected 3\n", __func__, __LINE__, instrument->sound_count);
    }

    if (instrument->volume != 70)
    {
        pass = 0;
        printf("%s %d>fail: instrument->volume=%d, expected 70\n", __func__, __LINE__, instrument->volume);
    }

    if (instrument->pan != 49)
    {
        pass = 0;
        printf("%s %d>fail: instrument->pan=%d, expected 49\n", __func__, __LINE__, instrument->pan);
    }

    if (instrument->sounds == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: instrument->sounds is NULL\n", __func__, __LINE__);
    }

    int seen_sound_index_0 = 0;
    int seen_sound_index_1 = 0;
    int seen_sound_index_2 = 0;

    for (i=0; i<instrument->sound_count; i++)
    {
        sound = instrument->sounds[i];

        if (sound == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: sound is NULL\n", __func__, __LINE__);
        }

        if (strcmp(sound->text_id, "sound1") == 0)
        {
            seen_sound_index_0++;

            if (sound->sample_volume != 127)
            {
                pass = 0;
                printf("%s %d>fail: sound->sample_volume=%d, expected 127\n", __func__, __LINE__, sound->sample_volume);
            }

            if (sound->sample_pan != 64)
            {
                pass = 0;
                printf("%s %d>fail: sound->sample_pan=%d, expected 64\n", __func__, __LINE__, sound->sample_pan);
            }
            
            wavetable = sound->wavetable;

            if (wavetable == NULL)
            {
                stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: wavetable is NULL\n", __func__, __LINE__);
            }

            if (strcmp(wavetable->aifc_path, "../sounds/thunk.aifc") != 0)
            {
                pass = 0;
                printf("%s %d>fail: wavetable->aifc_path=\"%s\", expected \"%s\"\n", __func__, __LINE__, wavetable->aifc_path, "../sounds/thunk.aifc");
            }

            envelope = sound->envelope;

            if (envelope == NULL)
            {
                stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: envelope is NULL\n", __func__, __LINE__);
            }

            if (envelope->attack_time != 5000)
            {
                pass = 0;
                printf("%s %d>fail: envelope->attack_time=%d, expected %d\n", __func__, __LINE__, envelope->attack_time, 5000);
            }

            if (envelope->attack_volume != 127)
            {
                pass = 0;
                printf("%s %d>fail: envelope->attack_volume=%d, expected %d\n", __func__, __LINE__, envelope->attack_volume, 127);
            }

            if (envelope->decay_time != 364920)
            {
                pass = 0;
                printf("%s %d>fail: envelope->decay_time=%d, expected %d\n", __func__, __LINE__, envelope->decay_time, 364920);
            }

            if (envelope->decay_volume != 127)
            {
                pass = 0;
                printf("%s %d>fail: envelope->decay_volume=%d, expected %d\n", __func__, __LINE__, envelope->decay_volume, 127);
            }

            if (envelope->release_time != 1234)
            {
                pass = 0;
                printf("%s %d>fail: envelope->release_time=%d, expected %d\n", __func__, __LINE__, envelope->release_time, 1234);
            }

            keymap = sound->keymap;

            if (keymap == NULL)
            {
                stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: keymap is NULL\n", __func__, __LINE__);
            }

            if (keymap->velocity_min != 1)
            {
                pass = 0;
                printf("%s %d>fail: keymap->velocity_min=%d, expected %d\n", __func__, __LINE__, keymap->velocity_min, 1);
            }

            if (keymap->velocity_max != 127)
            {
                pass = 0;
                printf("%s %d>fail: keymap->velocity_max=%d, expected %d\n", __func__, __LINE__, keymap->velocity_max, 127);
            }

            if (keymap->key_min != 41)
            {
                pass = 0;
                printf("%s %d>fail: keymap->key_min=%d, expected %d\n", __func__, __LINE__, keymap->key_min, 41);
            }

            if (keymap->key_max != 42)
            {
                pass = 0;
                printf("%s %d>fail: keymap->key_max=%d, expected %d\n", __func__, __LINE__, keymap->key_max, 42);
            }

            if (keymap->key_base != 43)
            {
                pass = 0;
                printf("%s %d>fail: keymap->key_base=%d, expected %d\n", __func__, __LINE__, keymap->key_base, 43);
            }

            if (keymap->detune != 5)
            {
                pass = 0;
                printf("%s %d>fail: keymap->detune=%d, expected %d\n", __func__, __LINE__, keymap->detune, 5);
            }
        }
        else if (strcmp(sound->text_id, "glass_sound") == 0)
        {
            seen_sound_index_1++;
            
            if (sound->sample_volume != 120)
            {
                pass = 0;
                printf("%s %d>fail: sound->sample_volume=%d, expected 120\n", __func__, __LINE__, sound->sample_volume);
            }

            if (sound->sample_pan != 60)
            {
                pass = 0;
                printf("%s %d>fail: sound->sample_pan=%d, expected 60\n", __func__, __LINE__, sound->sample_pan);
            }
            
            wavetable = sound->wavetable;

            if (wavetable == NULL)
            {
                stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: wavetable is NULL\n", __func__, __LINE__);
            }

            if (strcmp(wavetable->aifc_path, "../sounds/glass.aifc") != 0)
            {
                pass = 0;
                printf("%s %d>fail: wavetable->aifc_path=\"%s\", expected \"%s\"\n", __func__, __LINE__, wavetable->aifc_path, "../sounds/glass.aifc");
            }

            envelope = sound->envelope;

            if (envelope == NULL)
            {
                stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: envelope is NULL\n", __func__, __LINE__);
            }

            if (envelope->attack_time != 5000)
            {
                pass = 0;
                printf("%s %d>fail: envelope->attack_time=%d, expected %d\n", __func__, __LINE__, envelope->attack_time, 5000);
            }

            if (envelope->attack_volume != 127)
            {
                pass = 0;
                printf("%s %d>fail: envelope->attack_volume=%d, expected %d\n", __func__, __LINE__, envelope->attack_volume, 127);
            }

            if (envelope->decay_time != -1)
            {
                pass = 0;
                printf("%s %d>fail: envelope->decay_time=%d, expected %d\n", __func__, __LINE__, envelope->decay_time, -1);
            }

            if (envelope->decay_volume != 127)
            {
                pass = 0;
                printf("%s %d>fail: envelope->decay_volume=%d, expected %d\n", __func__, __LINE__, envelope->decay_volume, 127);
            }

            if (envelope->release_time != 5000)
            {
                pass = 0;
                printf("%s %d>fail: envelope->release_time=%d, expected %d\n", __func__, __LINE__, envelope->release_time, 5000);
            }

            keymap = sound->keymap;

            if (keymap == NULL)
            {
                stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: keymap is NULL\n", __func__, __LINE__);
            }

            if (keymap->velocity_min != 1)
            {
                pass = 0;
                printf("%s %d>fail: keymap->velocity_min=%d, expected %d\n", __func__, __LINE__, keymap->velocity_min, 1);
            }

            if (keymap->velocity_max != 127)
            {
                pass = 0;
                printf("%s %d>fail: keymap->velocity_max=%d, expected %d\n", __func__, __LINE__, keymap->velocity_max, 127);
            }

            if (keymap->key_min != 41)
            {
                pass = 0;
                printf("%s %d>fail: keymap->key_min=%d, expected %d\n", __func__, __LINE__, keymap->key_min, 41);
            }

            if (keymap->key_max != 41)
            {
                pass = 0;
                printf("%s %d>fail: keymap->key_max=%d, expected %d\n", __func__, __LINE__, keymap->key_max, 41);
            }

            if (keymap->key_base != 41)
            {
                pass = 0;
                printf("%s %d>fail: keymap->key_base=%d, expected %d\n", __func__, __LINE__, keymap->key_base, 41);
            }

            if (keymap->detune != 5)
            {
                pass = 0;
                printf("%s %d>fail: keymap->detune=%d, expected %d\n", __func__, __LINE__, keymap->detune, 5);
            }
        }
        else if (strcmp(sound->text_id, "Sound0138") == 0)
        {
            seen_sound_index_2++;
            
            if (sound->sample_volume != 110)
            {
                pass = 0;
                printf("%s %d>fail: sound->sample_volume=%d, expected 110\n", __func__, __LINE__, sound->sample_volume);
            }

            if (sound->sample_pan != 72)
            {
                pass = 0;
                printf("%s %d>fail: sound->sample_pan=%d, expected 72\n", __func__, __LINE__, sound->sample_pan);
            }
            
            wavetable = sound->wavetable;

            if (wavetable == NULL)
            {
                stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: wavetable is NULL\n", __func__, __LINE__);
            }

            if (strcmp(wavetable->aifc_path, "hit.aifc") != 0)
            {
                pass = 0;
                printf("%s %d>fail: wavetable->aifc_path=\"%s\", expected \"%s\"\n", __func__, __LINE__, wavetable->aifc_path, "hit.aifc");
            }

            envelope = sound->envelope;

            if (envelope == NULL)
            {
                stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: envelope is NULL\n", __func__, __LINE__);
            }

            if (envelope->attack_time != 11)
            {
                pass = 0;
                printf("%s %d>fail: envelope->attack_time=%d, expected %d\n", __func__, __LINE__, envelope->attack_time, 11);
            }

            if (envelope->attack_volume != 127)
            {
                pass = 0;
                printf("%s %d>fail: envelope->attack_volume=%d, expected %d\n", __func__, __LINE__, envelope->attack_volume, 127);
            }

            if (envelope->decay_time != 117913)
            {
                pass = 0;
                printf("%s %d>fail: envelope->decay_time=%d, expected %d\n", __func__, __LINE__, envelope->decay_time, 117913);
            }

            if (envelope->decay_volume != 127)
            {
                pass = 0;
                printf("%s %d>fail: envelope->decay_volume=%d, expected %d\n", __func__, __LINE__, envelope->decay_volume, 127);
            }

            if (envelope->release_time != 2000)
            {
                pass = 0;
                printf("%s %d>fail: envelope->release_time=%d, expected %d\n", __func__, __LINE__, envelope->release_time, 2000);
            }

            keymap = sound->keymap;

            if (keymap == NULL)
            {
                stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: keymap is NULL\n", __func__, __LINE__);
            }

            if (keymap->velocity_min != 9)
            {
                pass = 0;
                printf("%s %d>fail: keymap->velocity_min=%d, expected %d\n", __func__, __LINE__, keymap->velocity_min, 9);
            }

            if (keymap->velocity_max != 15)
            {
                pass = 0;
                printf("%s %d>fail: keymap->velocity_max=%d, expected %d\n", __func__, __LINE__, keymap->velocity_max, 15);
            }

            if (keymap->key_min != 4)
            {
                pass = 0;
                printf("%s %d>fail: keymap->key_min=%d, expected %d\n", __func__, __LINE__, keymap->key_min, 4);
            }

            if (keymap->key_max != 2)
            {
                pass = 0;
                printf("%s %d>fail: keymap->key_max=%d, expected %d\n", __func__, __LINE__, keymap->key_max, 2);
            }

            if (keymap->key_base != 48)
            {
                pass = 0;
                printf("%s %d>fail: keymap->key_base=%d, expected %d\n", __func__, __LINE__, keymap->key_base, 48);
            }

            if (keymap->detune != 19)
            {
                pass = 0;
                printf("%s %d>fail: keymap->detune=%d, expected %d\n", __func__, __LINE__, keymap->detune, 19);
            }
        }
    }

    if (seen_sound_index_0 != 1)
    {
        pass = 0;
        printf("%s %d>fail: seen_sound_index_0=%d, expected 1\n", __func__, __LINE__, seen_sound_index_0);
    }

    if (seen_sound_index_1 != 1)
    {
        pass = 0;
        printf("%s %d>fail: seen_sound_index_1=%d, expected 1\n", __func__, __LINE__, seen_sound_index_1);
    }

    if (seen_sound_index_2 != 1)
    {
        pass = 0;
        printf("%s %d>fail: seen_sound_index_2=%d, expected 1\n", __func__, __LINE__, seen_sound_index_2);
    }

    ALBankFile_free(bank_file);

    return pass;
}

void parse_inst_all(int *run_count, int *pass_count, int *fail_count)
{
    {
        /**
         * simple parse test
        */
        printf("parse inst test: 0001 - basic read\n");
        int pass = 1;
        *run_count = *run_count + 1;

        struct FileInfo *fi = FileInfo_fopen("test_cases/inst_parse/0001.inst", "rb");
        struct ALBankFile *bank_file = ALBankFile_new_from_inst(fi);

        struct ALBank *bank;
        struct ALInstrument *instrument;
        struct ALSound *sound;
        struct ALWaveTable *wavetable;
        struct ALKeyMap *keymap;
        struct ALEnvelope *envelope;

        pass = 1;
        if (bank_file == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: bank_file is NULL\n", __func__, __LINE__);
        }

        if (bank_file->bank_count != 1)
        {
            pass = 0;
            printf("%s %d>fail: bank_file->bank_count=%d, expected 1\n", __func__, __LINE__, bank_file->bank_count);
        }

        if (bank_file->banks == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: bank_file->banks is NULL\n", __func__, __LINE__);
        }

        bank = bank_file->banks[0];

        if (bank == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: bank is NULL\n", __func__, __LINE__);
        }

        if (bank->inst_count != 1)
        {
            pass = 0;
            printf("%s %d>fail: bank->inst_count=%d, expected 1\n", __func__, __LINE__, bank->inst_count);
        }

        if (bank->instruments == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: bank->instruments is NULL\n", __func__, __LINE__);
        }

        instrument = bank->instruments[0];

        if (instrument == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: instrument is NULL\n", __func__, __LINE__);
        }

        if (instrument->sound_count != 1)
        {
            pass = 0;
            printf("%s %d>fail: instrument->sound_count=%d, expected 1\n", __func__, __LINE__, instrument->sound_count);
        }

        if (instrument->sounds == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: instrument->sounds is NULL\n", __func__, __LINE__);
        }

        sound = instrument->sounds[0];

        if (sound == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: sound is NULL\n", __func__, __LINE__);
        }

        wavetable = sound->wavetable;

        if (wavetable == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: wavetable is NULL\n", __func__, __LINE__);
        }

        if (strcmp(wavetable->aifc_path, "sound_effect_0001.aifc") != 0)
        {
            pass = 0;
            printf("%s %d>fail: wavetable->aifc_path=\"%s\", expected \"%s\"\n", __func__, __LINE__, wavetable->aifc_path, "sound_effect_0001.aifc");
        }

        envelope = sound->envelope;

        if (envelope == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: envelope is NULL\n", __func__, __LINE__);
        }

        if (envelope->attack_volume != 127)
        {
            pass = 0;
            printf("%s %d>fail: envelope->attack_volume=%d, expected %d\n", __func__, __LINE__, envelope->attack_volume, 127);
        }

        keymap = sound->keymap;

        if (keymap == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: keymap is NULL\n", __func__, __LINE__);
        }

        if (keymap->key_min != 1)
        {
            pass = 0;
            printf("%s %d>fail: keymap->key_min=%d, expected %d\n", __func__, __LINE__, keymap->key_min, 1);
        }

        ALBankFile_free(bank_file);
        FileInfo_free(fi);

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
        /**
         * test all supported values
        */
        printf("parse inst test: 0002 - read all supported values\n");
        int pass = 1;
        *run_count = *run_count + 1;

        struct FileInfo *fi = FileInfo_fopen("test_cases/inst_parse/0002.inst", "rb");
        struct ALBankFile *bank_file = ALBankFile_new_from_inst(fi);

        pass = parse_inst_default(fi);

        ALBankFile_free(bank_file);
        FileInfo_free(fi);

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
        /**
         * parse test for whitespace and comments
        */
        printf("parse inst test: 0003 - whitespace and comments\n");
        int pass = 1;
        *run_count = *run_count + 1;

        struct FileInfo *fi = FileInfo_fopen("test_cases/inst_parse/0003.inst", "rb");
        struct ALBankFile *bank_file = ALBankFile_new_from_inst(fi);

        pass = parse_inst_default(fi);

        ALBankFile_free(bank_file);
        FileInfo_free(fi);

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
        /**
         * test (class) elements out of order
        */
        printf("parse inst test: 0004 - re-ordered class elements\n");
        int pass = 1;
        *run_count = *run_count + 1;

        struct FileInfo *fi = FileInfo_fopen("test_cases/inst_parse/0004.inst", "rb");
        struct ALBankFile *bank_file = ALBankFile_new_from_inst(fi);

        pass = parse_inst_default(fi);

        ALBankFile_free(bank_file);
        FileInfo_free(fi);

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
        /**
         * test sorting instrument->sound array elements (default)
        */
        printf("parse inst test: 0005 (0002.inst)- sort instrument->sound by .inst array index\n");
        int pass = 1;
        *run_count = *run_count + 1;

        struct FileInfo *fi = FileInfo_fopen("test_cases/inst_parse/0002.inst", "rb");
        struct ALBankFile *bank_file = ALBankFile_new_from_inst(fi);

        pass = parse_inst_default(fi);

        if (pass)
        {
            if (strcmp(bank_file->banks[0]->instruments[0]->sounds[0]->text_id, "sound1") != 0)
            {
                pass = 0;
                printf("%s %d>fail: sounds[0]->text_id=\"%s\", expected \"%s\"\n", __func__, __LINE__, bank_file->banks[0]->instruments[0]->sounds[0]->text_id, "sound1");
            }

            if (strcmp(bank_file->banks[0]->instruments[0]->sounds[1]->text_id, "glass_sound") != 0)
            {
                pass = 0;
                printf("%s %d>fail: sounds[1]->text_id=\"%s\", expected \"%s\"\n", __func__, __LINE__, bank_file->banks[0]->instruments[0]->sounds[1]->text_id, "glass_sound");
            }

            if (strcmp(bank_file->banks[0]->instruments[0]->sounds[2]->text_id, "Sound0138") != 0)
            {
                pass = 0;
                printf("%s %d>fail: sounds[2]->text_id=\"%s\", expected \"%s\"\n", __func__, __LINE__, bank_file->banks[0]->instruments[0]->sounds[2]->text_id, "Sound0138");
            }
        }

        ALBankFile_free(bank_file);
        FileInfo_free(fi);

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
        /**
         * test sorting instrument->sound array elements
        */
        printf("parse inst test: 0006 - sort instrument->sound by .inst array index\n");
        int pass = 1;
        *run_count = *run_count + 1;

        struct FileInfo *fi = FileInfo_fopen("test_cases/inst_parse/0006.inst", "rb");
        struct ALBankFile *bank_file = ALBankFile_new_from_inst(fi);

        pass = parse_inst_default(fi);

        if (pass)
        {
            if (strcmp(bank_file->banks[0]->instruments[0]->sounds[0]->text_id, "sound1") != 0)
            {
                pass = 0;
                printf("%s %d>fail: sounds[0]->text_id=\"%s\", expected \"%s\"\n", __func__, __LINE__, bank_file->banks[0]->instruments[0]->sounds[0]->text_id, "sound1");
            }

            if (strcmp(bank_file->banks[0]->instruments[0]->sounds[1]->text_id, "glass_sound") != 0)
            {
                pass = 0;
                printf("%s %d>fail: sounds[1]->text_id=\"%s\", expected \"%s\"\n", __func__, __LINE__, bank_file->banks[0]->instruments[0]->sounds[1]->text_id, "glass_sound");
            }

            if (strcmp(bank_file->banks[0]->instruments[0]->sounds[2]->text_id, "Sound0138") != 0)
            {
                pass = 0;
                printf("%s %d>fail: sounds[2]->text_id=\"%s\", expected \"%s\"\n", __func__, __LINE__, bank_file->banks[0]->instruments[0]->sounds[2]->text_id, "Sound0138");
            }
        }

        ALBankFile_free(bank_file);
        FileInfo_free(fi);

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
        /**
         * test duplicate refereences
        */
        printf("parse inst test: 0007 - duplicate references\n");
        int pass = 1;
        *run_count = *run_count + 1;

        struct FileInfo *fi = FileInfo_fopen("test_cases/inst_parse/0007.inst", "rb");
        struct ALBankFile *bank_file = ALBankFile_new_from_inst(fi);

        struct ALBank *bank;
        struct ALSound *sound0;
        struct ALSound *sound1;

        if (bank_file->bank_count != 1)
        {
            pass = 0;
            printf("%s %d>fail: bank_file->bank_count=%d, expected %d\n", __func__, __LINE__, bank_file->bank_count, 1);
        }

        bank = bank_file->banks[0];

        if (bank == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> bank_file->banks[0] IS NULL\n", __func__, __LINE__);
        }

        if (bank->instruments[0] == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> fatal error: bank->instruments[0] IS NULL\n", __func__, __LINE__);
        }

        if (bank->instruments[1] == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> fatal error: bank->instruments[1] IS NULL\n", __func__, __LINE__);
        }

        if (bank->inst_count != 2)
        {
            pass = 0;
            printf("%s %d>fail: bank->inst_count=%d, expected %d\n", __func__, __LINE__, bank->inst_count, 2);
        }

        if (bank->instruments[0]->sound_count != 2)
        {
            pass = 0;
            printf("%s %d>fail: bank->instruments[0]->sound_count=%d, expected %d\n", __func__, __LINE__, bank->instruments[0]->sound_count, 2);
        }

        if (bank->instruments[1]->sound_count != 1)
        {
            pass = 0;
            printf("%s %d>fail: bank->instruments[1]->sound_count=%d, expected %d\n", __func__, __LINE__, bank->instruments[1]->sound_count, 1);
        }

        if (bank->instruments[0]->sounds[0] == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> fatal error: bank->instruments[0]->sounds[0] IS NULL\n", __func__, __LINE__);
        }

        if (bank->instruments[0]->sounds[1] == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> fatal error: bank->instruments[0]->sounds[1] IS NULL\n", __func__, __LINE__);
        }

        if (bank->instruments[1]->sounds[0] == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> fatal error: bank->instruments[1]->sounds[0] IS NULL\n", __func__, __LINE__);
        }

        if (strcmp(bank->instruments[0]->sounds[0]->text_id, "Sound0000") != 0)
        {
            pass = 0;
            printf("%s %d>fail: bank->instruments[0]->sounds[0]->text_id=\"%s\", expected \"%s\"\n", __func__, __LINE__, bank->instruments[0]->sounds[0]->text_id, "Sound0000");
        }

        if (strcmp(bank->instruments[0]->sounds[1]->text_id, "Sound0001") != 0)
        {
            pass = 0;
            printf("%s %d>fail: bank->instruments[0]->sounds[1]->text_id=\"%s\", expected \"%s\"\n", __func__, __LINE__, bank->instruments[0]->sounds[1]->text_id, "Sound0001");
        }

        if (strcmp(bank->instruments[1]->sounds[0]->text_id, "Sound0000") != 0)
        {
            pass = 0;
            printf("%s %d>fail: bank->instruments[1]->sounds[0]->text_id=\"%s\", expected \"%s\"\n", __func__, __LINE__, bank->instruments[1]->sounds[0]->text_id, "Sound0000");
        }

        if (bank->instruments[0]->sounds[0]->id != bank->instruments[1]->sounds[0]->id)
        {
            pass = 0;
            printf("%s %d>fail: values should be identical: bank->instruments[0]->sounds[0]->id=%d, bank->instruments[1]->sounds[0]->id=%d\n", __func__, __LINE__, bank->instruments[0]->sounds[0]->id, bank->instruments[1]->sounds[0]->id);
        }

        sound0 = bank->instruments[0]->sounds[0];
        sound1 = bank->instruments[0]->sounds[1];

        if (strcmp(sound0->envelope->text_id, "Envelope0000") != 0)
        {
            pass = 0;
            printf("%s %d>fail: sound0->envelope->text_id=\"%s\", expected \"%s\"\n", __func__, __LINE__, sound0->envelope->text_id, "Envelope0000");
        }

        if (strcmp(sound1->envelope->text_id, "Envelope0000") != 0)
        {
            pass = 0;
            printf("%s %d>fail: sound1->envelope->text_id=\"%s\", expected \"%s\"\n", __func__, __LINE__, sound1->envelope->text_id, "Envelope0000");
        }

        if (sound0->envelope->id != sound1->envelope->id)
        {
            pass = 0;
            printf("%s %d>fail: values should be identical: sound0->envelope->id=%d, sound1->envelope->id=%d\n", __func__, __LINE__, sound0->envelope->id, sound1->envelope->id);
        }

        if (strcmp(sound0->keymap->text_id, "Keymap0000") != 0)
        {
            pass = 0;
            printf("%s %d>fail: sound0->keymap->text_id=\"%s\", expected \"%s\"\n", __func__, __LINE__, sound0->keymap->text_id, "Keymap0000");
        }

        if (strcmp(sound1->keymap->text_id, "Keymap0000") != 0)
        {
            pass = 0;
            printf("%s %d>fail: sound1->keymap->text_id=\"%s\", expected \"%s\"\n", __func__, __LINE__, sound1->keymap->text_id, "Keymap0000");
        }

        if (sound0->keymap->id != sound1->keymap->id)
        {
            pass = 0;
            printf("%s %d>fail: values should be identical: sound0->keymap->id=%d, sound1->keymap->id=%d\n", __func__, __LINE__, sound0->keymap->id, sound1->keymap->id);
        }

        ALBankFile_free(bank_file);
        FileInfo_free(fi);

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