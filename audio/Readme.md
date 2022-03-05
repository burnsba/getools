# Gaudio Tool Suite

This is a collection of command line tools for working with N64 audio, as implemented in Goldeneye 007.

The "g" in gaudio stands for either Goldeneye or GNU.

These tools were written in C (and some bash) for a Linux based operating system.

-----

This assumes you have five base audio files from Goldeneye:

- instruments.ctl: metadata for sound samples used in music playback
- instruments.tbl: sound samples used in music playback
- sfx.ctl: metadata for sound effects
- sfx.tbl: sound effect samples
- music.sbk: soundbank, compiled music tracks

Gaudio can extract audio files and metadata from the above into more common music formats, and then rebuild the above files for use in a romhack or custom build.

One of the goals of gaudio is to preserve as much information as possible when converting between formats, and to implement fully reversible conversion whenever possible (end up with byte-exact copy of input). Converting to MIDI and back to soundbank is fully reversible. Converting from .ctl/.tbl to .aifc+.inst is fully reversible. Converting from .aifc to .wav is not.

# Tools

- **[aifc2wav](doc/app_manual/aifc2wav.md)**: Convert from N64 .aifc to .wav
- **[cseq2midi](doc/app_manual/cseq2midi.md)**: Convert from N64 MIDI format to standard MIDI
- **[gic](doc/app_manual/gic.md)**: Gaudio instrument compiler. Build .ctl and .tbl from .inst file and source .aifc files.
- **[midi2cseq](doc/app_manual/midi2cseq.md)**: Convert from standard MIDI to N64 MIDI format
- **[miditool](doc/app_manual/miditool.md)**: Adjust events within MIDI file
- **[sbksplit](doc/app_manual/sbksplit.md)**: Parse single .sbk file and split into separate .seq.rz files (no gzip decompression performed)
- **[sbc](doc/app_manual/sbc.md)**: Compile existing gzip music tracks (.seq.rz) into single .sbk
- **[tabledesign](doc/app_manual/tabledesign.md)**: Evaulate audio file and build .aifc codebook
- **[tbl2aifc](doc/app_manual/tbl2aifc.md)**: Convert .ctl and .tbl file into .inst file and .aifc files.
- **[wav2aifc](doc/app_manual/wav2aifc.md)**: Convert .wav to compressed .aifc using supplied codebook.


See individual program help page for more info (docs folder).

See the [technical documentation](doc/tech/Technical.md) for an overview of the conversion "lifecycle".

# Documentation

Documentation can be read online at github project website, or browsed locally, starting with the [doc/readme.md](doc/readme.md) file.

# Project Structure

The project is organized in the following structure:

```
gaudio
├── bin: where built executables end up
├── doc: help files, technical documentation, runtime comparison
├── obj: intermediate build objects
├── shell: shell scripts
├── src: project source code
│   ├── app: command line executables source
│   ├── base: general shared code, e.g., hash table
│   ├── lib: audio processing code. All the good stuff happens here.
│   └── test: automated test code
└── test_cases: files used in automated tests
```

The project compiles four static libraries from source code:

- **libgaudiobase**: general shared code (linked list, debugging, etc)
- **libgaudiohash**: everything hash related (int hashtable, string hashtable, md5)
- **libgaudio**: base audio implementation classes (wav, aifc, midi)
- **libgaudiox**: translation between audio formats

These libraries are placed in the `obj` folder.

# Building

The tabledesign application depends on the GNU Scientific Library (GSL), but otherwise there are no external dependencies. On debian-like systems, GSL can be installed with  

```
sudo apt-get install libgsl-dev
```

This is not required to build the other applications. The makefile should automatically determine if the library is available or not.


Running `make` without any arguments (or `make all`) should build everything:

```
make
```

If there were no compilation errors, executable files will end up in the `bin` folder. After the tools build, you can verify the results by running the automated tests:

```
bin/test
```

or

```
make check
```

Test status is printed as it runs, it should end with a line similar to the following:

```
118 tests run, 118 pass, 0 fail
```

If clang is installed, static analysis can be performed with

```
make clean
scan-build-11 make
```

Note: debian command is `scan-build-11`.

This should (hopefully) only report some dead assignments and dead increments.

# License

Gaudio is released under the terms of the GNU General Public License. 
