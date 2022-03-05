# Gaudio gic

Gaudio instrument compiler.

Reads an .inst file and generates .tbl and .ctl files from specification and referenced .aifc files.

Full compatibility with the SDK `ic` program is not a design requirement, but any valid .inst file supported by `ic` should also work with `gic`.

Usage:

```
bin/gic --in file
```

Options:

```
    --help                        print this help
    -n,--in=FILE                  input .inst file source
    -o,--out=FILE                 output file prefix. Optional. If not provided, will
                                  reuse the input file name but change extension.
    -r,--sample-rate              overwrite bank sample rate
    --sort-natural                write envelope and keymap according to parent
                                  ALSound order. Default value. Incompatible with sort-meta.
    --sort-meta                   write envelope and keymap according to metaCtlWriteOrder
                                  property read from .inst file. Incompatible with sort-natural.
    -q,--quiet                    suppress output
    -v,--verbose                  more output
```

# Sort Order

When the .tbl/.ctl files are first read by `tbl2aifc`, the file offset is recorded in the .inst file under the property `metaCtlWriteOrder`. When command line option `--sort-meta` is used,  `gic` will use the `metaCtlWriteOrder` property to build the .tbl and .ctl files according to the order specified. This is required to rebuild a byte exact .ctl/.tbl file. Otherwise, `gic` will create the .ctl and .tbl files according to the order of items parsed from the .inst file, the so called "natural" sort order (`--sort-natural`).

# Instrument File

This section assumes you have already read the N64 Programming Manual section 18.1 about the instrument compiler.

The .inst file format uses a C-like syntax. Type names, property names, and object references are case sensitive and must match exactly or a fatal syntax error occurs.

 Objects are declared in a hierarchy (or rather, are declared in a flat sequence but are logically structured in a hierarchy): a bank object, which contains the sample rate, and one or more instruments. Each instrument has properties, and contains one or more sounds. The sound specifies the file on disk containing audio waveform data, and references envelope and keymap parameters.

A short example of an .inst file is as follows, from `src/test_cases/inst_parse/0001.inst`:

```
envelope Envelope0000 {
    attackVolume = 127;
}
keymap Keymap0000 {
    keyMin = 1;
}
sound Sound0000 {
    use ("sound_effect_0001.aifc");
    envelope = Envelope0000;
    keymap = Keymap0000;
}
instrument Instrument0000 {
    sound [0] = Sound0000;
}
bank Bank0000 {
    instrument [0] = Instrument0000;
}
```

Gaudio adds support for a new property called `metaCtlWriteOrder`; this is valid for `sound`, `keymap`, and `envelope`. This defines a sort order for how this section will be written in the .ctl file (keymap section, envelope section, etc). This can be any integer value, and is used to sort objects (from smallest to largest). This is used in combination with the `--sort-meta` flag to recreate a .ctl/.tbl identical to the starting files.

The `use` property specifies file location on disk of the audio waveform, relative to where `gic` is invoked.

Array indeces are allowed to be sparse. Items will be sorted from smallest to largest.

Multiple `sound`s can "use" the same audio sample file. Multiple `sound`s can reference the same `keymap` or `envelope`. Gaudio will automatically resolve references and not duplicate data unnecessarily.

Objects can be declared in any order. For example, a `keymap` can be declared before it is referenced in a `sound` block, or after.

The parser is very tolerant of whitespace. For example,

```
sound
[
                2
]
=
Sound0138
;
```

is the same as `sound[2]=Sound0138;`.

A `#` character can be used anywhere to indicate that the rest of the line is a comment and should be ignored.

Duplicate declarations (instance names) are not allowed.

 ## Supported types and properties

Type names, property names, and object references are case sensitive.

Support types are: "bank", "instrument", "sound", "keymap", "envelope".

Properties support by `bank`: "instrument", "sampleRate"      

Properties supported by `instrument`: "volume", "pan", "priority", "flags", "tremType",  
 "tremRate", "tremDepth", "tremDelay", "vibType", "vibRate", "vibDepth", "vibDelay", "bendRange", "sound"    

Properties supported by `sound`: "use", "pan", "volume", "envelope", "keymap", "metaCtlWriteOrder"  

Properties supported by `keymap`: "velocityMin", "velocityMax", "keyMin", "keyMax", "keyBase", "detune", "metaCtlWriteOrder"  

Properties supported by `envelope`: "attackTime", "attackVolume", "decayTime", "decayVolume", "releaseTime", "metaCtlWriteOrder"


See the Programming Manual 18.1, or `naudio.h` for more details on what each property does.

The `naudio_parse_inst.c` file contains the authoritative list of supported types, properties, and values.
