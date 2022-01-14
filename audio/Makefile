
default: all

# c compiler
CC := gcc
CFLAGS := -g -Wall -Wextra -pedantic -Wunreachable-code

# location of source files
SRC := src

# output build location
BUILD := bin

$(BUILD)/%.o: $(SRC)/%.c
	$(CC) -c $< -o $@ $(CFLAGS) 

SBKSPLIT_OBJS := $(BUILD)/common.o $(BUILD)/sbksplit.o $(BUILD)/naudio.o $(BUILD)/utility.o $(BUILD)/llist.o $(BUILD)/debug.o
$(BUILD)/sbksplit: $(SBKSPLIT_OBJS)
	$(CC) -o $@ $(SBKSPLIT_OBJS)

TBL2AIFC_OBJS := $(BUILD)/tbl2aifc.o $(BUILD)/common.o $(BUILD)/naudio.o $(BUILD)/adpcm_aifc.o $(BUILD)/llist.o $(BUILD)/utility.o $(BUILD)/math.o $(BUILD)/debug.o
$(BUILD)/tbl2aifc: $(TBL2AIFC_OBJS)
	$(CC) -o $@ $(TBL2AIFC_OBJS)

AIFC2WAV_OBJS := $(BUILD)/aifc2wav.o $(BUILD)/common.o $(BUILD)/wav.o $(BUILD)/naudio.o $(BUILD)/adpcm_aifc.o $(BUILD)/llist.o $(BUILD)/utility.o $(BUILD)/math.o $(BUILD)/debug.o
$(BUILD)/aifc2wav: $(AIFC2WAV_OBJS)
	$(CC) -o $@ $(AIFC2WAV_OBJS)

CSEQ2MIDI_OBJS := $(BUILD)/cseq2midi.o $(BUILD)/common.o $(BUILD)/midi.o $(BUILD)/naudio.o $(BUILD)/llist.o $(BUILD)/utility.o $(BUILD)/math.o $(BUILD)/debug.o
$(BUILD)/cseq2midi: $(CSEQ2MIDI_OBJS)
	$(CC) -o $@ $(CSEQ2MIDI_OBJS)

aifc2wav: $(BUILD)/aifc2wav
sbksplit: $(BUILD)/sbksplit
tbl2aifc: $(BUILD)/tbl2aifc
cseq2midi: $(BUILD)/cseq2midi
all: $(BUILD)/sbksplit $(BUILD)/tbl2aifc $(BUILD)/aifc2wav $(BUILD)/cseq2midi

clean:
	rm -f $(BUILD)/*.o $(BUILD)/sbksplit $(BUILD)/tbl2aifc $(BUILD)/aifc2wav $(BUILD)/cseq2midi

.PHONY: all default clean sbksplit tbl2aifc aifc2wav cseq2midi
