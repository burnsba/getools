#!/bin/bash

BIN_VERSION="1.0"
BIN_NAME="gaudio soundbank compiler"
GZ=
NAMES_FILE=
# requires trailing slash
SOURCE_DIR="./"
DEBUG=0
SEQ_EXTENSION=.seq
SEQ_RZ_EXTENSION=.seq.rz
OUTPUT_FILE=out.sbk
OUT_COMPRESS_WRITE_SIZE=0
OUT_METADATA_WRITE_SIZE=0
OUT_SIZE=0

# perform 1172 compression.
# requires $GZ be set to gzip binary
# param 1: input file to compress
# param 2: output file
function compress1172() {
    # create
    echo -n -e \\x11\\x72 > "$2"
    # append
    cat "$1" | $GZ --no-name --best | tail --bytes=+11 | head --bytes=-8 >> "$2"
    
    # if the compressed file size is an odd number of bytes
    # then append a newline character.
    local LOCAL_TMP_GZIP_FILE_LEN
    LOCAL_TMP_GZIP_FILE_LEN=$(stat -c%s "$2")
    
    if [ `expr $LOCAL_TMP_GZIP_FILE_LEN % 2` == 1 ] ; then
        printf "\n" >> "$2"
    fi    
}

function printf_u8()
{
    local i
    local val
    val=$(printf "%02x" $1)
    for ((i=0; i<2; i+=2))
    do
        printf "\x${val:i:2}";
    done;
}

function printf_u16()
{
    local i
    local val
    val=$(printf "%04x" $1)
    for ((i=0; i<4; i+=2))
    do
        printf "\x${val:i:2}";
    done;
}

function printf_u32()
{
    local i
    local val
    val=$(printf "%08x" $1)
    for ((i=0; i<8; i+=2))
    do
        printf "\x${val:i:2}";
    done;
}

function usage() {
    echo "${BIN_NAME} ${BIN_VERSION}. Compress seq files and combine into .sbk"
    echo ""
    echo "usage:"
    echo ""
    echo "  $0 -z GZIP -i DIR -n NAMES -o OUTPUT"
    echo ""
    echo "options:"
    echo ""
    echo "    -z BIN                        Path to gzip binary."
    echo "    -n FILE                       File containing list of music track filenames to compile, one"
    echo "                                  entry per line. Do not include extension or directory prefix in track name."
    echo "                                  Leading and trailing spaces are stripped, but allowed within."
    echo "                                  Lines beginning with # are ignored."
    echo "    -i DIR                        Input directory containing seq files (listed in names file)."
    echo "                                  Default is current directory."
    echo "    -o FILE                       Output filename. Default=${OUTPUT_FILE}"
    echo ""
    exit 0;
}

[ $# -eq 0 ] && usage
while getopts "z:n:i:o:hG" arg; do
  case $arg in
    z )
        GZ="${OPTARG}"
      ;;
    n )
        NAMES_FILE="${OPTARG}"
      ;;
    i )
        SOURCE_DIR="${OPTARG}"
      ;;
    o )
        OUTPUT_FILE="${OPTARG}"
      ;;
    G )
        DEBUG=1
      ;;
    h | *) # Display help.
      usage
      exit 0
      ;;
  esac
done

if [ -z "${GZ}" ]; then
    usage
fi

if [ -z "${NAMES_FILE}" ]; then
    usage
fi

if [ ! -f "${GZ}" ]; then
    echo "gzip file not found: ${GZ}"
    exit 1
fi

if [ ! -f "${NAMES_FILE}" ]; then
    echo "names file not found: ${NAMES_FILE}"
    exit 1
fi

if [ ! -z "${SOURCE_DIR}" ]; then
    SOURCE_DIR=$(echo "${SOURCE_DIR}" | sed 's:/*$::')
    SOURCE_DIR="${SOURCE_DIR}/"
fi

if [ $DEBUG -ne 0 ] ; then
    echo "GZ=${GZ}"
    echo "NAMES_FILE=${NAMES_FILE}"
    echo "SOURCE_DIR=${SOURCE_DIR}"
    echo "OUTPUT_FILE=${OUTPUT_FILE}"
fi

if [ -f "${OUTPUT_FILE}" ]; then
    rm "${OUTPUT_FILE}"
fi

MUSIC_COUNT=0

TMP_SBK_SEQRZ_DATA_FILE=$(mktemp /tmp/gaudio-sbc.XXXXXX)
TMP_GZIP_FILE=$(mktemp /tmp/gaudio-sbc.XXXXXX)
ARR_SBK_INFLATE_LEN=()
ARR_SBK_COMPRESS_LEN=()

touch "${TMP_SBK_SEQRZ_DATA_FILE}"
touch "${OUTPUT_FILE}"

while read -r LINE
do
    # ignore lines beginning with comment character
    LINE=$(echo "${LINE}" | sed 's/\s*#.*//g')
    
    # strip almost all non alphanumeric characters
    LINE=$(echo "${LINE}" | tr -cd '[:alnum:] ._-~')
    
    # if whatever is left in the line is empty then skip it
    if [ -z "${LINE}" ]; then
        continue
    fi
    
    SEQ_PATH="${SOURCE_DIR}${LINE}${SEQ_EXTENSION}"
    
    if [ ! -f "${SEQ_PATH}" ]; then
        echo "seq file not found: ${SEQ_PATH}"
        exit 1
    fi
    
    ARR_SBK_INFLATE_LEN+=( $(stat -c%s "${SEQ_PATH}") )
    
    compress1172 "${SEQ_PATH}" "${TMP_GZIP_FILE}"
    ARR_SBK_COMPRESS_LEN+=( $(stat -c%s "${TMP_GZIP_FILE}") )
    
    if [ $DEBUG -ne 0 ] ; then
        echo "seq $MUSIC_COUNT, file \"${LINE}\", filesize: ${ARR_SBK_INFLATE_LEN[$MUSIC_COUNT]}, compressed size: ${ARR_SBK_COMPRESS_LEN[$MUSIC_COUNT]}"
    fi
    
    cat "${TMP_GZIP_FILE}" >> "${TMP_SBK_SEQRZ_DATA_FILE}"

    rm "${TMP_GZIP_FILE}"
    
    MUSIC_COUNT=$(($MUSIC_COUNT + 1))

# append extra new line on end so that the last line is always guaranteed to be read
done < <(cat "${NAMES_FILE}"; echo;)

# write sound bank header
# first u16: number of music tracks
printf_u16 "${MUSIC_COUNT}" >> "${OUTPUT_FILE}"
# second u16: unused
printf_u16 0 >> "${OUTPUT_FILE}"

# setup start address counter
SBK_ADDR_POS=0
SBK_ADDR_POS=$(($SBK_ADDR_POS + 4))
SBK_ADDR_POS=$(($SBK_ADDR_POS + 8*$MUSIC_COUNT))

# iterate each metadata for the music tracks.
# write the address into the soundbank file, the uncompressed length, and compressed length.
for ((i=0;i<"${MUSIC_COUNT}";i++)); do
    printf_u32 ${SBK_ADDR_POS} >> "${OUTPUT_FILE}"
    printf_u16 ${ARR_SBK_INFLATE_LEN[$i]} >> "${OUTPUT_FILE}"
    printf_u16 ${ARR_SBK_COMPRESS_LEN[$i]} >> "${OUTPUT_FILE}"
    
    # track totals for final stats summary
    OUT_METADATA_WRITE_SIZE=$(($OUT_METADATA_WRITE_SIZE + 8))
    OUT_COMPRESS_WRITE_SIZE=$(($OUT_COMPRESS_WRITE_SIZE + ${ARR_SBK_COMPRESS_LEN[$i]}))
    
    # update soundbank address according to compressed file length
    SBK_ADDR_POS=$(($SBK_ADDR_POS + ${ARR_SBK_COMPRESS_LEN[$i]}))
done

# Done writing metadata, now append the compressed file data.
cat "${TMP_SBK_SEQRZ_DATA_FILE}" >> "${OUTPUT_FILE}"

# done with compressed data
rm "${TMP_SBK_SEQRZ_DATA_FILE}"

OUT_SIZE=$(stat -c%s "${OUTPUT_FILE}")
echo "Processed ${MUSIC_COUNT} music tracks."
echo "Wrote ${OUT_SIZE} bytes to \"${OUTPUT_FILE}\", metadata size=${OUT_METADATA_WRITE_SIZE}, compressesd data size=${OUT_COMPRESS_WRITE_SIZE}"