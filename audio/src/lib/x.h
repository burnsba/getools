#ifndef _GAUDIO_X_H_
#define _GAUDIO_X_H_

#include "naudio.h"
#include "adpcm_aifc.h"


struct WavFile *WavFile_new_from_aifc(struct AdpcmAifcFile *aifc_file);
struct AdpcmAifcFile *AdpcmAifcFile_new_from_wav(struct WavFile *wav_file, struct ALADPCMBook *book);
struct AdpcmAifcFile *AdpcmAifcFile_new_full(struct ALSound *sound, struct ALBank *bank);
void load_aifc_from_sound(struct AdpcmAifcFile *aaf, struct ALSound *sound, uint8_t *tbl_file_contents, struct ALBank *bank);
void write_sound_to_aifc(struct ALSound *sound, struct ALBank *bank, uint8_t *tbl_file_contents, struct file_info *fi);
void write_bank_to_aifc(struct ALBankFile *bank_file, uint8_t *tbl_file_contents);
void ALBankFile_write_tbl(struct ALBankFile *bank_file, char* tbl_filename);
void ALBankFile_write_ctl(struct ALBankFile *bank_file, char* ctl_filename);

void WavFile_check_append_aifc_loop(struct WavFile *wav, struct AdpcmAifcFile *aaf);
void AdpcmAifcFile_add_partial_loop_from_wav(struct AdpcmAifcFile *aaf, struct WavFile *wav);

struct ALADPCMLoop *ALADPCMLoop_new_from_aifc_loop(struct AdpcmAifcLoopChunk *loop_chunk);
struct ALADPCMBook *ALADPCMBook_new_from_aifc_book(struct AdpcmAifcCodebookChunk *book_chunk);
struct ALRawLoop *ALRawLoop_new_from_aifc_loop(struct AdpcmAifcLoopChunk *loop_chunk);

#endif