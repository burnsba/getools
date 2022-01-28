
default: all

# c compiler
CC := gcc
CFLAGS := -g -Wall -Wextra -pedantic -Wunreachable-code
LINKERS := -lm

# location of source files
SRC := src

# output build location
BUILD := bin

$(BUILD)/%.o: $(SRC)/%.c
	$(CC) -c $< -o $@ $(CFLAGS) 

SBKSPLIT_OBJS := $(BUILD)/common.o $(BUILD)/sbksplit.o $(BUILD)/naudio.o $(BUILD)/naudio_parse_inst.o $(BUILD)/string_hash.o $(BUILD)/md5.o $(BUILD)/utility.o $(BUILD)/llist.o $(BUILD)/debug.o
$(BUILD)/sbksplit: $(SBKSPLIT_OBJS)
	$(CC) -o $@ $(LINKERS) $(SBKSPLIT_OBJS)

TBL2AIFC_OBJS := $(BUILD)/tbl2aifc.o $(BUILD)/common.o $(BUILD)/naudio.o $(BUILD)/naudio_parse_inst.o $(BUILD)/string_hash.o $(BUILD)/md5.o $(BUILD)/adpcm_aifc.o $(BUILD)/llist.o $(BUILD)/utility.o $(BUILD)/math.o $(BUILD)/debug.o
$(BUILD)/tbl2aifc: $(TBL2AIFC_OBJS)
	$(CC) -o $@ $(LINKERS) $(TBL2AIFC_OBJS)

AIFC2WAV_OBJS := $(BUILD)/aifc2wav.o $(BUILD)/string_hash.o $(BUILD)/md5.o $(BUILD)/common.o $(BUILD)/wav.o $(BUILD)/naudio.o $(BUILD)/naudio_parse_inst.o $(BUILD)/adpcm_aifc.o $(BUILD)/llist.o $(BUILD)/utility.o $(BUILD)/math.o $(BUILD)/debug.o
$(BUILD)/aifc2wav: $(AIFC2WAV_OBJS)
	$(CC) -o $@ $(LINKERS) $(AIFC2WAV_OBJS)

CSEQ2MIDI_OBJS := $(BUILD)/cseq2midi.o $(BUILD)/common.o $(BUILD)/midi.o $(BUILD)/naudio.o $(BUILD)/naudio_parse_inst.o $(BUILD)/string_hash.o $(BUILD)/md5.o $(BUILD)/llist.o $(BUILD)/utility.o $(BUILD)/math.o $(BUILD)/debug.o
$(BUILD)/cseq2midi: $(CSEQ2MIDI_OBJS)
	$(CC) -o $@ $(LINKERS) $(CSEQ2MIDI_OBJS)

TESTSUITE_OBJS := $(BUILD)/test.o $(BUILD)/common.o $(BUILD)/midi.o $(BUILD)/naudio.o $(BUILD)/naudio_parse_inst.o $(BUILD)/string_hash.o $(BUILD)/md5.o $(BUILD)/llist.o $(BUILD)/utility.o $(BUILD)/math.o $(BUILD)/debug.o
$(BUILD)/test: $(TESTSUITE_OBJS)
	$(CC) -o $@ $(LINKERS) $(TESTSUITE_OBJS)

aifc2wav: $(BUILD)/aifc2wav
sbksplit: $(BUILD)/sbksplit
tbl2aifc: $(BUILD)/tbl2aifc
cseq2midi: $(BUILD)/cseq2midi
test: $(BUILD)/test
all: $(BUILD)/sbksplit $(BUILD)/tbl2aifc $(BUILD)/aifc2wav $(BUILD)/cseq2midi $(BUILD)/test

clean:
	rm -f $(BUILD)/*.o $(BUILD)/sbksplit $(BUILD)/tbl2aifc $(BUILD)/aifc2wav $(BUILD)/cseq2midi $(BUILD)/test

.PHONY: all default clean sbksplit tbl2aifc aifc2wav test
