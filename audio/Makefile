
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

$(BUILD)/libgaudiobase.a: $(BUILD)/llist.o $(BUILD)/kvp.o $(BUILD)/common.o $(BUILD)/utility.o $(BUILD)/math.o $(BUILD)/debug.o
	ar rcs $@ $^

$(BUILD)/libgaudiostring_hash.a: $(BUILD)/string_hash.o $(BUILD)/md5.o $(BUILD)/libgaudiobase.a 
	ar rcs $@ $^

$(BUILD)/libgaudionaudio.a: $(BUILD)/naudio.o $(BUILD)/naudio_parse_inst.o $(BUILD)/libgaudiostring_hash.a
	ar rcs $@ $^

$(BUILD)/libgaudiomidi.a: $(BUILD)/midi.o $(BUILD)/libgaudionaudio.a
	ar rcs $@ $^

$(BUILD)/libgaudioaifc.a: $(BUILD)/adpcm_aifc.o $(BUILD)/libgaudionaudio.a
	ar rcs $@ $^

$(BUILD)/libgaudiowav.a: $(BUILD)/wav.o $(BUILD)/libgaudioaifc.a
	ar rcs $@ $^

$(BUILD)/sbksplit: $(BUILD)/sbksplit.o $(BUILD)/libgaudiobase.a
	$(CC) $^ -o $@ $(LINKERS) -Lbin -lgaudiobase

$(BUILD)/tbl2aifc: $(BUILD)/tbl2aifc.o $(BUILD)/libgaudioaifc.a
	$(CC) $^ -o $@ $(LINKERS) -Lbin -lgaudioaifc -lgaudionaudio -lgaudiostring_hash -lgaudiobase 

$(BUILD)/aifc2wav: $(BUILD)/aifc2wav.o $(BUILD)/libgaudiowav.a
	$(CC) $^ -o $@ $(LINKERS) -Lbin -lgaudiowav -lgaudioaifc -lgaudionaudio -lgaudiostring_hash -lgaudiobase 

$(BUILD)/cseq2midi: $(BUILD)/cseq2midi.o $(BUILD)/libgaudiomidi.a
	$(CC) $^ -o $@ $(LINKERS) -Lbin -lgaudiomidi -lgaudionaudio -lgaudiostring_hash -lgaudiobase 

$(BUILD)/test: $(BUILD)/test.o $(BUILD)/libgaudiowav.a $(BUILD)/libgaudioaifc.a $(BUILD)/libgaudiomidi.a $(BUILD)/libgaudionaudio.a
	$(CC) $^ -o $@ $(LINKERS) -Lbin -lgaudiomidi -lgaudiowav -lgaudioaifc -lgaudionaudio -lgaudiostring_hash -lgaudiobase 

aifc2wav: $(BUILD)/aifc2wav
sbksplit: $(BUILD)/sbksplit
tbl2aifc: $(BUILD)/tbl2aifc
cseq2midi: $(BUILD)/cseq2midi
test: $(BUILD)/test

all: $(BUILD)/sbksplit $(BUILD)/tbl2aifc $(BUILD)/aifc2wav $(BUILD)/cseq2midi $(BUILD)/test

clean:
	rm -f $(BUILD)/*.o $(BUILD)/*.a $(BUILD)/sbksplit $(BUILD)/tbl2aifc $(BUILD)/aifc2wav $(BUILD)/cseq2midi $(BUILD)/test

check: $(BUILD)/test
	bin/test

.PHONY: all default clean sbksplit tbl2aifc aifc2wav test check
