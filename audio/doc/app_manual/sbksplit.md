# Gaudio sbksplit

Splits a Rare .sbk file into individual .seq.rz files. No decompression is performed.

Usage:
```
bin/sbksplit -i file
```

Options:

```
    --help                        print this help
    -i,--in=FILE                  input file (required)
    -p,--prefix=STRING            string to prepend to output files. (optional)
                                  default=music_
    -n,--names=FILE               sound names. One name per line. Lines starting with # ignored.
                                  Names applied in order read, if the list is too short
                                  subsequent items will be given numeric id (0001, 0002, ...).
                                  Non alphanumeric characters ignored.
                                  Names listed in file should not include filename extension.
    -q,--quiet                    suppress output
    -v,--verbose                  more output
```

# Format

Rare soundbank file begins with a header section, followed by individual .seq files 1172 compressed.

The header begins with a 16 bit integer giving a count of the number of sequences in the file. The next 16 bits are unused (word padding?)

Following are `RareALSeqData` descriptions of the sequences in the file.

Following the header section are the individual .seq files in 1172 compressed format.
