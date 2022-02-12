
default: all

# c compiler
CC := gcc
CFLAGS := -g -Wall -Wextra -pedantic -Wunreachable-code -Wstrict-prototypes -Wmissing-prototypes -Wmissing-declarations -Wmissing-include-dirs -Wno-unused-parameter -Wuninitialized
LINKERS := -lm

# root location of source files (no trailing slash)
SRC := src

# let all header files be available everywhere.
INCLUDES := -I$(SRC)/base -I$(SRC)/lib -I$(SRC)/app -I$(SRC)/test

# location for object files and libraries (no trailing slash)
OBJ := obj

# location to put completed binaries (no trailing slash)
BUILD := bin

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

$(OBJ)/libgaudioshash.a: $(OBJ)/string_hash.o $(OBJ)/int_hash.o $(OBJ)/md5.o $(OBJ)/libgaudiobase.a 
	ar rcs $@ $^

$(OBJ)/libgaudio.a: $(OBJ)/naudio.o $(OBJ)/naudio_parse_inst.o $(OBJ)/naudio_parse_coef.o $(OBJ)/adpcm_aifc.o $(OBJ)/midi.o $(OBJ)/wav.o $(OBJ)/libgaudioshash.a
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
	$(CC) $^ -o $@ $(LINKERS) -Lobj -lgaudio -lgaudioshash -lgaudiobase

$(BUILD)/aifc2wav: $(OBJ)/aifc2wav.o $(OBJ)/libgaudiox.a
	$(CC) $^ -o $@ $(LINKERS) -Lobj -lgaudiox -lgaudio -lgaudioshash -lgaudiobase

$(BUILD)/wav2aifc: $(OBJ)/wav2aifc.o $(OBJ)/libgaudiox.a
	$(CC) $^ -o $@ $(LINKERS) -Lobj -lgaudiox -lgaudio -lgaudioshash -lgaudiobase

$(BUILD)/cseq2midi: $(OBJ)/cseq2midi.o $(OBJ)/libgaudiox.a
	$(CC) $^ -o $@ $(LINKERS) -Lobj -lgaudiox -lgaudio -lgaudioshash -lgaudiobase

$(BUILD)/gic: $(OBJ)/gic.o $(OBJ)/libgaudiox.a
	$(CC) $^ -o $@ $(LINKERS) -Lobj -lgaudiox -lgaudio -lgaudioshash -lgaudiobase

$(BUILD)/test: $(OBJ)/test.o $(OBJ)/test_md5.o $(OBJ)/test_llist.o $(OBJ)/test_string_hash.o $(OBJ)/test_int_hash.o $(OBJ)/test_parse_inst.o $(OBJ)/test_parse_coef.o $(OBJ)/test_aifc.o $(OBJ)/test_common.o $(OBJ)/libgaudio.a $(OBJ)/libgaudiox.a 
	$(CC) $^ -o $@ $(LINKERS) -Lobj -lgaudiox -lgaudio -lgaudioshash -lgaudiobase

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
	@echo "    aifc2wav cseq2midi gic sbksplit tbl2aifc "

####################################################################################################
#
# top level recipes
#

directories:
	mkdir -p bin obj

aifc2wav: directories $(BUILD)/aifc2wav
wav2aifc: directories $(BUILD)/wav2aifc
sbksplit: directories $(BUILD)/sbksplit
tbl2aifc: directories $(BUILD)/tbl2aifc
cseq2midi: directories $(BUILD)/cseq2midi
gic: directories $(BUILD)/gic
test: directories $(BUILD)/test

all: directories $(BUILD)/sbksplit $(BUILD)/tbl2aifc $(BUILD)/aifc2wav $(BUILD)/wav2aifc $(BUILD)/cseq2midi $(BUILD)/gic $(BUILD)/test

clean:
	rm -f $(BUILD)/*.o $(BUILD)/*.a $(OBJ)/*.o $(OBJ)/*.a $(BUILD)/sbksplit $(BUILD)/tbl2aifc $(BUILD)/aifc2wav $(BUILD)/wav2aifc $(BUILD)/cseq2midi $(BUILD)/gic $(BUILD)/test

check: directories $(BUILD)/test
	bin/test

.PHONY: all default clean sbksplit tbl2aifc aifc2wav wav2aifc gic test check directories help
