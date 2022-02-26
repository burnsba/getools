
default: all

# c compiler
CC := gcc
CFLAGS := -O2 -g -Wall -Wextra -pedantic -Wunreachable-code -Wstrict-prototypes -Wmissing-prototypes -Wmissing-declarations -Wmissing-include-dirs -Wno-unused-parameter -Wuninitialized
LINKERS := -lm

# root location of source files (no trailing slash)
SRC := src

# let all header files be available everywhere.
INCLUDES := -I$(SRC)/base -I$(SRC)/lib -I$(SRC)/app -I$(SRC)/test

# location for object files and libraries (no trailing slash)
OBJ := obj

# location to put completed binaries (no trailing slash)
BUILD := bin

# if GNU Scientific Library isn't installed then disable tabledesign and related files.
# Otherwise build/include/link
LIB_GSL := 0
TARGET_TABLEDESIGN :=
OBJ_MAGIC :=
ifeq ($(shell /sbin/ldconfig -p | grep -q libgsl.so >/dev/null 2>/dev/null; echo $$?), 0)
	LIB_GSL := 1
	TARGET_TABLEDESIGN :=$(BUILD)/tabledesign
	OBJ_MAGIC :=$(OBJ)/magic.o

	LINKERS += -lgsl
else
	CFLAGS += -DNOGSL
endif

####################################################################################################
#
# compile c file section
#
# Just throwing everything into one `obj` directory until there are name collisions.

$(OBJ)/%.o: $(SRC)/base/%.c
	$(CC) -c $< -o $@ $(CFLAGS) $(INCLUDES)

$(OBJ)/%.o: $(SRC)/lib/%.c
	$(CC) -c $< -o $@ $(CFLAGS) $(INCLUDES)

$(OBJ)/%.o: $(SRC)/app/%.c
	$(CC) -c $< -o $@ $(CFLAGS) $(INCLUDES)

$(OBJ)/%.o: $(SRC)/test/%.c
	$(CC) -c $< -o $@ $(CFLAGS) $(INCLUDES)

####################################################################################################
#
# build static library section
#

$(OBJ)/libgaudiobase.a: $(OBJ)/llist.o $(OBJ)/kvp.o $(OBJ)/common.o $(OBJ)/parse.o $(OBJ)/utility.o $(OBJ)/gaudio_math.o $(OBJ)/debug.o
	ar rcs $@ $^

$(OBJ)/libgaudiohash.a: $(OBJ)/string_hash.o $(OBJ)/int_hash.o $(OBJ)/md5.o $(OBJ)/libgaudiobase.a 
	ar rcs $@ $^

$(OBJ)/libgaudio.a: $(OBJ_MAGIC) $(OBJ)/naudio.o $(OBJ)/naudio_parse_inst.o $(OBJ)/naudio_parse_coef.o $(OBJ)/adpcm_aifc.o $(OBJ)/midi.o $(OBJ)/wav.o $(OBJ)/libgaudiohash.a
	ar rcs $@ $^

$(OBJ)/libgaudiox.a: $(OBJ)/x.o $(OBJ)/libgaudio.a
	ar rcs $@ $^

####################################################################################################
#
# build binaries section
#
# Each lib dependency is explicitly listed as a `-l` parameter. The makefile step lists the
# top level make dependencies.

$(BUILD)/sbksplit: $(OBJ)/sbksplit.o $(OBJ)/libgaudiobase.a
	$(CC) $^ -o $@ $(LINKERS) -Lobj -lgaudiobase

$(BUILD)/tbl2aifc: $(OBJ)/tbl2aifc.o $(OBJ)/libgaudiox.a
	$(CC) $^ -o $@ $(LINKERS) -Lobj -lgaudio -lgaudiohash -lgaudiobase

$(BUILD)/aifc2wav: $(OBJ)/aifc2wav.o $(OBJ)/libgaudiox.a
	$(CC) $^ -o $@ $(LINKERS) -Lobj -lgaudiox -lgaudio -lgaudiohash -lgaudiobase

$(BUILD)/wav2aifc: $(OBJ)/wav2aifc.o $(OBJ)/libgaudiox.a
	$(CC) $^ -o $@ $(LINKERS) -Lobj -lgaudiox -lgaudio -lgaudiohash -lgaudiobase

$(BUILD)/cseq2midi: $(OBJ)/cseq2midi.o $(OBJ)/libgaudiox.a
	$(CC) $^ -o $@ $(LINKERS) -Lobj -lgaudiox -lgaudio -lgaudiohash -lgaudiobase

$(BUILD)/midi2cseq: $(OBJ)/midi2cseq.o $(OBJ)/libgaudiox.a
	$(CC) $^ -o $@ $(LINKERS) -Lobj -lgaudiox -lgaudio -lgaudiohash -lgaudiobase

$(BUILD)/gic: $(OBJ)/gic.o $(OBJ)/libgaudiox.a
	$(CC) $^ -o $@ $(LINKERS) -Lobj -lgaudiox -lgaudio -lgaudiohash -lgaudiobase

ifeq ($(LIB_GSL), 0)
$(BUILD)/tabledesign:
	@echo "missing system library libgsl.so"
	@exit 1
else
$(BUILD)/tabledesign: $(OBJ)/tabledesign.o $(OBJ)/libgaudiox.a
	$(CC) $^ -o $@ $(LINKERS) -Lobj -lgaudiox -lgaudio -lgaudiohash -lgaudiobase	
endif

$(BUILD)/test: $(OBJ)/test.o $(OBJ)/test_md5.o $(OBJ)/test_llist.o $(OBJ)/test_string_hash.o $(OBJ)/test_int_hash.o $(OBJ)/test_midi.o $(OBJ)/test_midi_convert.o $(OBJ)/test_parse_inst.o $(OBJ)/test_parse_coef.o $(OBJ)/test_magic.o $(OBJ)/test_aifc.o $(OBJ)/test_common.o $(OBJ)/libgaudio.a $(OBJ)/libgaudiox.a 
	$(CC) $^ -o $@ $(LINKERS) -Lobj -lgaudiox -lgaudio -lgaudiohash -lgaudiobase

####################################################################################################

help:
	@echo "makefile help"
	@echo ""
	@echo "  supported targets:"
	@echo ""
	@echo "    all                         build all (default)"
	@echo "    clean                       rm all build artifacts"
	@echo "    check                       build and run tests"
	@echo ""
	@echo "  single app targets:"
	@echo ""
	@echo "    aifc2wav cseq2midi gic midi2cseq sbksplit tabledesign tbl2aifc wav2aifc"

####################################################################################################
#
# top level recipes
#

directories:
	mkdir -p bin obj

aifc2wav: directories $(BUILD)/aifc2wav
cseq2midi: directories $(BUILD)/cseq2midi
midi2cseq: directories $(BUILD)/midi2cseq
gic: directories $(BUILD)/gic
sbksplit: directories $(BUILD)/sbksplit
tabledesign: directories $(BUILD)/tabledesign
tbl2aifc: directories $(BUILD)/tbl2aifc
wav2aifc: directories $(BUILD)/wav2aifc

test: directories $(BUILD)/test

all: directories $(BUILD)/sbksplit $(BUILD)/tbl2aifc $(BUILD)/aifc2wav $(BUILD)/wav2aifc $(BUILD)/cseq2midi $(BUILD)/midi2cseq $(BUILD)/gic $(BUILD)/test $(TARGET_TABLEDESIGN)

clean:
	rm -f $(BUILD)/*.o $(BUILD)/*.a $(OBJ)/*.o $(OBJ)/*.a $(BUILD)/sbksplit $(BUILD)/tbl2aifc $(BUILD)/aifc2wav $(BUILD)/wav2aifc $(BUILD)/cseq2midi $(BUILD)/midi2cseq $(BUILD)/gic $(BUILD)/tabledesign $(BUILD)/test

check: directories $(BUILD)/test
	bin/test

.PHONY: all default clean sbksplit cseq2midi midi2cseq tbl2aifc aifc2wav wav2aifc gic tabledesign test check directories help
