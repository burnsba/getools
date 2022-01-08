
default: all

# c compiler
CC := gcc
CFLAGS := -Wall -Wextra -pedantic -Wunreachable-code

# location of source files
SRC := src

# output build location
BUILD := bin

$(BUILD)/%.o: $(SRC)/%.c
	$(CC) -c $< -o $@ $(CFLAGS) 

SBKSPLIT_OBJS := $(BUILD)/common.o $(BUILD)/sbksplit.o
$(BUILD)/sbksplit: $(SBKSPLIT_OBJS)
	$(CC) $(SBKSPLIT_OBJS) -o $@ $(CFLAGS)

TBL2AIFC_OBJS := $(BUILD)/common.o $(BUILD)/tbl2aifc.o $(BUILD)/llist.o $(BUILD)/utility.o
$(BUILD)/tbl2aifc: $(TBL2AIFC_OBJS)
	$(CC) $(TBL2AIFC_OBJS) -o $@ $(CFLAGS)

sbksplit: $(BUILD)/sbksplit
tbl2aifc: $(BUILD)/tbl2aifc
all: $(BUILD)/sbksplit $(BUILD)/tbl2aifc

clean:
	rm -f $(BUILD)/*.o $(BUILD)/sbksplit $(BUILD)/tbl2aifc

.PHONY: all default clean sbksplit tbl2aifc
