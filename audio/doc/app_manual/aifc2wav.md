# Gaudio aifc2wav

Converts n64 aifc audio (mono 16 bit PCM big endian) to wav format (mono 16 bit PCM little endian). For samples containing loops, the loop audio data can be automatically unrolled as many times as specified. By default, samples are converted without looping.

Note: The game applies effects such as reverb during playback, so wav samples will sound different than they will in game.

# Overview

Usage:

```
bin/aifc2wav --in file
```

Options:

```
    --help                        print this help
    -n,--in=FILE                  input .aifc file to convert
    -o,--out=FILE                 output file. Optional. If not provided, will
                                  reuse the input file name but change extension.
    -l,--loop=NUMBER              This specifies the number of times an ADPCM Loop
                                  should be repeated. Only applies to infinite loops.
                                  Default=0.
    --write-smpl                  Setting this flag will add the "smpl" chunk to
                                  output wav. This requires resolving keybase, which
                                  will use the same value (or default) as freq adjust.
    --no-freq-adjust              Disables frequency adjust mode, but allows setting
                                  keybase or searching .inst file to use with writing
                                  wav "smpl" chunk.
    -q,--quiet                    suppress output
    -v,--verbose                  more output

freq_adjust_mode = explicit

    Keybase and detune parameters are explicitly set. Setting either value
    implicitly toggles this mode. These options are incompatible with `search` mode.

    -k,--keybase=NOTE             Keybase sound was recorded in. MIDI note range from
                                  0-127. Refer to N64 Programming manual for more info.
                                  Default=60 (MIDI note C4)
    -d,--detune=CENTS             Additional detune value in cents (1200 cents per octave).
                                  Refer to N64 Programming manual for more info.
                                  Default=0

freq_adjust_mode = search

    Keybase and detune parameters loaded from .inst file. Setting any value
    implicitly toggles this mode. These options are incompatible with `explicit` mode.

    --inst-file=FILE              Input .inst file to search. Required.
    --inst-search=MODE            Search method to use. Required. Available options are:
                                  - "use"
                                    Finds `sound` based on trailing text of "use" value.
                                  - "sound"
                                    Finds `sound` with same name
                                  - "keymap"
                                    Finds `keymap` with same name
    --inst-val=TEXT               Text parameter of search. Required.
    --force-freq-adjust           By default, frequency adjustments will only be applied on
                                  audio of type AL_ADPCM_WAVE. This flag will force
                                  AL_RAW16_WAVE to be adjusted as well. Has no
                                  effect if --no-freq-adjust is set.
```

# Frequency Adjustment

By default files are converted without frequency adjustment, and without loop information. Some sound samples in the game need to have the specified frequency changed in order to sound correct. This is simply updating the metadata to a different frequency value, waveform data is not changed.

Frequency adjustment is measured against the base MIDI note (keybase), and optional detune parameter (1/100 note). These can be specified to `aifc2wav` with the `--keybase` and `--detune` parameters. Example

```
bin/aifc2wav --in=piano.aifc --keybase=72
```

Sometimes the exact keybase or detune amount isn't known, but is available in the .inst file. In that case, you can tell `aifc2wav` to search an .inst file for the values it needs. Specify the .inst file with `--inst-file`. Then explain how to find the needed value. This can search for a matching `sound` with the `--inst-search=sound` option (and then it will find the keymap). Or search for a matching keymap with `--inst-search=keymap`. Or you can tell it to search based on the "use" filename with `--inst-search=use`, which will probably be the same as the file being converted. Examples of each of these:

```
bin/aifc2wav --in test_data/snd/Rocket_Launch.aifc --out test_data/snd/Rocket_Launch.wav --debug --inst-file=test_data/sfx.inst --inst-search=sound --inst-val=Sound0000
bin/aifc2wav --in test_data/snd/Rocket_Launch.aifc --out test_data/snd/Rocket_Launch.wav --debug --inst-file=test_data/sfx.inst --inst-search=keymap --inst-val=Keymap0000
bin/aifc2wav --in test_data/snd/Rocket_Launch.aifc --out test_data/snd/Rocket_Launch.wav --debug --inst-file=test_data/sfx.inst --inst-search=use --inst-val="test_data/snd/Rocket_Launch.aifc"
```

By default, frequency adjustment will only be applied on .aifc files using compression (marked as type `AL_ADPCM_WAVE`). To force frequency adjustment on uncompressed .aifc files (marked as type `AL_RAW16_WAVE`), add flag `--force-freq-adjust`.

See the Programming Manual chapter 20.4 for more information and sample frequency adjustment.

# Loops

The .aifc file can contain an application chunk with loop information. By default, `aifc2wav` will not export loop metadata (start, end, count). Loop metadata can be exported to the wav "smpl" chunk format by specifying `--write-smpl`. Note that this requires specifying a frequency adjust amount (keybase). This can be disabled by using option `--no-freq-adjust`.

Loop waveform data can be repeated when converted to give the sound of a sustained playback. The number of times the loop should be repeated is given by the option `--loop`.
