# Gaudio cseq2midi

Converts n64 compressed format MIDI to regular MIDI.

Note: gaudio generally refers to the n64 compressed format MIDI files as sequence files.

# Overview

Usage:

```
bin/cseq2midi --in file
```

Options:

```
    --help                        print this help
    -n,--in=FILE                  input .aifc file to convert
    -o,--out=FILE                 output file. Optional. If not provided, will
                                  reuse the input file name but change extension.
    --write-seq-tracks            Perform pattern substitution (unroll track) then
                                  write track to disk.
    --no-pattern-compression      By default, it is assumed the source seq file has
                                  "pattern marker" compression, which escapes bytes
                                  like 0xfe. This option disables that.
    --pattern-file=FILE           Saves all pattern markers (with track number) to
                                  specified file. Only applies when pattern compression
                                  is not disabled.
    --export-invalid-loop         The retail version of the game has 72 invalid seq loop events
                                  (no start, no end, invalid offset, etc), this flag will
                                  convert those events to MIDI system exclusive command to
                                  include in output. Otherwise these events are not included
                                  in the output file.
    -q,--quiet                    suppress output
    -v,--verbose                  more output
```

# Compression

The sequence files use a simple compression algorithm. Gaudio refers to this as pattern compression. If you are processing a file that has already extracted sequence track data, but is otherwise in the sequence file format, use option `--no-pattern-compression`.

The compression algorithm is rather simple and easy to implement, but it seems some information was lost from the original files used to build the retail version of the game. Therefore, to fully reverse from MIDI back to sequence format, the original pattern compression markers are saved to for use later. The output file for this is given by `--pattern-file=FILE`. This is a simple csv format, listing the (sequence) track number and regular pattern marker info.

# Invalid Loops

There are many instances of invalid loop events in the retail version of the game. These are difficult or impossible to fully reverse from MIDI back to sequence format since some required information is missing. Therefore, invalid loop events are not exported from the sequence files by default.

To include invalid loop events, use option `--export-invalid-loop`. This will wrap the invalid sequence loop events in a MIDI System Exclusive event when converting to MIDI. This may cause playback issues on media player software. Note: this is tested to work in Renoise, and Presonus Studio One. This is known to cause problems with Windows Media Player.
