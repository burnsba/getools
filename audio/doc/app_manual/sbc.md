# Gaudio sbc

Soundbank compiler shell script. Compress seq files and combine into .sbk

Usage:

```
shell/sbc.sh -z GZIP -i DIR -n NAMES -o OUTPUT
```

Options:

```
    -z BIN                        Path to gzip binary.
    -n FILE                       File containing list of music track filenames to compile, one
                                  entry per line. Do not include extension or directory prefix in track name.
                                  Leading and trailing spaces are stripped, but allowed within.
                                  Lines beginning with # are ignored.
    -i DIR                        Input directory containing seq files (listed in names file).
                                  Default is current directory.
    -o FILE                       Output filename. Default=out.sbk
```

# gzip

Results can vary depending on which version of gzip is used. It seems a recent system package of gzip is more likely to compile into a .sbk file that exactly matches the original.

# Names

The names file option `-n` specifies the music files on disk to include in the soundbank. This will probably be the same name file used by `tbl2aifc`. This file should contain names without file extension and without directory prefix. Directory containing the files should be given with `-i` option.
