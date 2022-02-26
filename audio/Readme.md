# Gaudio Tool Suite

This is a collection of command line tools for working with N64 audio, as implemented in Goldeneye 007.

The "g" in gaudio stands for either Goldeneye or GNU.

These tools were written for a Linux based operating system.

# Tools

- **aifc2wav**: Convert from N64 .aifc to .wav
- **cseq2midi**: Convert from N64 MIDI format to standard MIDI
- **gic**: Gaudio instrument compiler. Build .ctl and .tbl from .inst file and source .aifc files.
- **midi2cseq**: Convert from standard MIDI to N64 MIDI format
- **sbksplit**: Parse single .sbk file and split into separate .seq.rz files (no gzip decompression performed)
- **sbc**: Compile existing gzip music tracks (.seq.rz) into single .sbk
- **tabledesign**: Evaulate audio file and build .aifc codebook
- **tbl2aifc**: Convert .ctl and .tbl file into .inst file and .aifc files.
- **wav2aifc**: Convert .wav to compressed .aifc using supplied codebook.

  
See individual program help page for more info.

# Project Structure

```
gaudio
├── bin: where built executables end up
├── doc: help files, technical documentation, runtime comparison
├── obj: intermediate build objects
├── src: project source code
│   ├── app: command line executables source
│   ├── base: general shared code, e.g., hash table
│   ├── lib: audio processing code
│   └── test: automated test code
└── test_data: files used in automated tests
```

The project compiles four static libraries from source code:

- **libgaudiobase**: general shared code (linked list, debugging, etc)
- **libgaudiohash**: everything hash related (int hashtable, string hashtable, md5)
- **libgaudio**: base audio implementation classes (wav, aifc, midi)
- **libgaudiox**: translation between audio formats

These libraries are placed in the `obj` folder.

# Building

The tabledesign application depends on the GNU Scientific Library. On debian-like systems, this can be installed with  

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

This should only report some dead assignments and dead increments.