#ifndef _GAUDIO_X_H_
#define _GAUDIO_X_H_

#include "naudio.h"
#include "adpcm_aifc.h"


struct WavFile *WavFile_load_from_aifc(struct AdpcmAifcFile *aifc_file);
struct AdpcmAifcFile *AdpcmAifcFile_new_full(struct ALSound *sound, struct ALBank *bank);
void load_aifc_from_sound(struct AdpcmAifcFile *aaf, struct ALSound *sound, uint8_t *tbl_file_contents, struct ALBank *bank);
void write_sound_to_aifc(struct ALSound *sound, struct ALBank *bank, uint8_t *tbl_file_contents, struct file_info *fi);
void write_bank_to_aifc(struct ALBankFile *bank_file, uint8_t *tbl_file_contents);
void ALBankFile_write_tbl(struct ALBankFile *bank_file, char* tbl_filename);
void ALBankFile_write_ctl(struct ALBankFile *bank_file, char* ctl_filename);

#endif