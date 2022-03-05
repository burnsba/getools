# Gaudio wav2aifc

Converts wav audio (mono 16 bit PCM little endian) to n64 aifc format (mono 16 bit PCM big endian) using codebook coefficients.

Usage:

```
bin/wav2aifc --in file [-c coef_file]
```

Options:

```
    --help                        print this help
    -n,--in=FILE                  input .wav file to convert
    -o,--out=FILE                 output file. Optional. If not provided, will
                                  reuse the input file name but change extension.
    -c,--coef=FILE                coef table / codebook previously generated. Optional.
                                  If no codebook is provided, format will be AL_RAW16_WAVE.
     --swap                       byte swap audio samples before converting to .aifc
                                  This is normally determined automatically, but.
                                  can be forced with this switch.
    -q,--quiet                    suppress output
    -v,--verbose                  more output
```

# Compression

By default `wav2aifc` will not compress the audio data. In order to use compression, codebook coefficients must be supplied in a .coef file. This can be generated using [tabledesign](tabledesign.md), or the N64 SDK tool tabledesign. This file format expects three identifiers, "order", "npredictors", and "book". Values can be in decimal or hex format. An example of a valid .coef file is as follows:

```
order=2;
npredictors=1;
book=
  -769,   -992,   -992,   -907,   -798,   -689,   -590,   -502,
  2643,   2641,   2416,   2126,   1836,   1571,   1338,   1137;
```
