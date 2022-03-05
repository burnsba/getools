# Gaudio midi2cseq

Converts regular MIDI for playback on N64

Note: gaudio generally refers to the n64 compressed format MIDI files as sequence files.

Usage:

```
bin/midi2cseq --in file
```

Options:

```
    --help                        print this help
    -n,--in=FILE                  input .aifc file to convert
    -o,--out=FILE                 output file. Optional. If not provided, will
                                  reuse the input file name but change extension.
    --no-pattern-compression      By default, MIDI conversion will perform pattern
                                  substituion to reduce file size, this option
                                  disables that.
    --pattern-file=FILE           Reads pattern markers from previously saved file. Only
                                  applies when pattern compression is not disabled.
    -q,--quiet                    suppress output
    -v,--verbose                  more output
```

# Compression

The sequence files use a simple compression algorithm. Gaudio refers to this as pattern compression. This is enabled by default, unless the option `--no-pattern-compression` is set.

The algorithm used by gaudio to resolve pattern markers is generally identical to how original sequence files were built, but there are some exceptions. It seems likely there were additional MIDI events or meta events that have been lost from the retail version of the game. In order to rebuild an exactly matching sequence file, pattern markers from the initial conversion through `cseq2midi` need to be used. This is specified with `--pattern-file=FILE`. This is a simple csv format, listing the (sequence) track number and regular pattern marker info.
