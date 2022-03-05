# Tutorial: Custom music track with custom instrument

Adding a new music track with new instruments is not for the faint of heart, but I will do my best to outline the steps required.

For this example, I will be using a MIDI track from gaudio test data, located in `gaudio/test_cases/midi/entertainer_short.midi`. I copied this to the `assets/audio/midi/` folder. This example will use one external instrument; I borrowed the `44_grand_piano.aiff` sample from Super Mario 64 and resampled to 22050 Hz, converted to wav file (and renamed to `piano.wav`), and placed the file in `assets/audio/aifc_instruments`.

The general out will be:

- Convert instrument samples to .aifc
- Assemble metadata about instrument samples into the .inst file
- Adjust MIDI file to reference the instrument sample
- Add loop markers like a wizard
- Convert the MIDI file to .seq
- Update game references to use the new music

I tried to make a video version of this tutorial, which also demos the music track working correctly on everdrive. Available at [https://www.youtube.com/watch?v=01AOi7LkxUI](https://www.youtube.com/watch?v=01AOi7LkxUI).

**Convert instrument samples to .aifc**

See the outline for converting .wav to .aifc above, this is the same. The example `piano.wav` was resampled to 44100 Hz (up one octave). The original was assumed to be C4, and the new sample at C5, so named `piano_c5.wav`.

```
# from audio/assets folder
../../tools/gaudio/bin/tabledesign --in aifc_instruments/piano.wav
../../tools/gaudio/bin/wav2aifc --in=aifc_instruments/piano.wav --coef=aifc_instruments/piano.coef --out=aifc_instruments/piano.aifc
../../tools/gaudio/bin/tabledesign --in aifc_instruments/piano_c5.wav
../../tools/gaudio/bin/wav2aifc --in=aifc_instruments/piano_c5.wav --coef=aifc_instruments/piano_c5.coef --out=aifc_instruments/piano_c5.aifc
```

Note: loop points within samples are not covered in this tutorial, but gaudio supports .wav "smpl" chunk to specify loop points. See gaudio documentation for more info.

**Assemble metadata about instrument samples into the .inst file**

The last instrument in the .inst file is `Instrument0074`. A new instrument will need to be added, which I'll call `ent_inst_0075`. The instrument will need two child sound records, one for the base piano sound and another for the higher frequency sample. And each sound record will need a keymap and envelope record. The naming is not really particular, the only that matters is that references are unique within the .inst file.

The sequence player restricts the audio sample to within one octave of the keybase. This is why the original piano sample was resampled to 44100 Hz above. Each one will require it's own keymap. The base sample will have a keybase of C4, with max value of B4. The second sample will have a keybase of C5 with max value of B5. The programmer manual says that keymap ranges should not overlap, as the sequence player will choose the first keymap valid for the range.

The .inst file should have the following text added

```
# Instrument0074 is the same, add new records below

# keymap for new sound c4
keymap ent_keymap_c4 {
    metaCtlWriteOrder = 115;
    velocityMin = 0;
    velocityMax = 127;
    keyMin = 0;
    keyMax = 71;
    keyBase = 60;
    detune = 0;
}

# keymap for new sound c5
keymap ent_keymap_c5 {
    metaCtlWriteOrder = 116;
    velocityMin = 0;
    velocityMax = 127;
    keyMin = 72;
    keyMax = 83;
    keyBase = 72;
    detune = 0;
}

# here's a new sound
sound ent_sound_c4 {
    metaCtlWriteOrder = 132;
    use ("aifc_instruments/piano.aifc");

    pan = 127;
    volume = 127;
    envelope = Envelope0000;
    keymap = ent_keymap_c4;
}

# here's a new sound
sound ent_sound_c5 {
    metaCtlWriteOrder = 133;
    use ("aifc_instruments/piano_c5.aifc");

    pan = 127;
    volume = 127;
    envelope = Envelope0000;
    keymap = ent_keymap_c5;
}

# here's the new instrument
instrument ent_inst_0075 {
    volume = 127;
    pan = 64;
    priority = 5;
    bendRange = 200;

    # sound for everything below B4
    sound [0] = ent_sound_c4;
    # sound for C5 - B5
    sound [1] = ent_sound_c5;
}

bank Bank0000 {
    sampleRate = 22050;
    instrument [0] = Instrument0000;
    instrument [1] = Instrument0001;
    # ...
    # skipped a bunch of lines to keep this short
    # ...
    instrument [73] = Instrument0073;
    # Instrument0074 hasn't changed
    instrument [74] = Instrument0074;

    # this line is new:
    instrument [75] = ent_inst_0075;
}

```

Rebuild the instruments container. If there is a syntax error in the .inst file, this will fail, so this is a good thing to check now.

```
bin/gic --in=instruments.inst --out=instruments --sort-meta
```

**MIDI processing**

Use your favorite MIDI editor to adjust the instrument used in the MIDI file to the sound sample just added. The example MIDI uses channel zero. The new instrument is index 75. The gaudio `miditool` can be used to adjust the instrument (below).

It is required that the channel of an event is the same as the track number. The gaudio `miditool` can update events to force the channel number to be the same as the track number.

```
cp ../../tools/gaudio/test_cases/midi/entertainer_short.midi midi/
# make sure all event channel are the same as track number
../../tools/gaudio/bin/miditool --in=midi/entertainer_short.midi --action=make-channel-track
# change the program control event in track 1 (channel 1) to the new instrument
../../tools/gaudio/bin/miditool --in=midi/entertainer_short.midi --action=set-channel-instrument --channel=1 --instrument=75
# which should give the following output:
destination track not resolved, assuming midi track 0
Changing MIDI "Program Change" event in track index 1, from 0 to 75
```

Note: gaudio tools only support MIDI events as used by N64. If the MIDI file contains "key signature" or "time signature" or "text" events, or any other unsupported events, this will result in error.


**MIDI track loop**

MIDI loops are specified using custom controller values. Each loop will be described using three events, a start event, count event, and end event. The start and end loop event will have one parameter which is the loop number. The loop count event has one parameter which is the loop count.

Controller 102: loop start, value is loop number  
Controller 103: loop end, value is loop number
Controller 104: loop count (0-127), value is loop count
Controller 105: loop count (128-255), value is loop count

Loop count values must be between 0-127. Controller event 105 will add 128 to the loop count.

The gaudio `miditool` can be used to insert a basic loop around a track. This will create a start event before the first Note On event, and an end event after the last Note Off (or implicit Note Off) event. The loop count will be set to the max allowed value.

```
../../tools/gaudio/bin/miditool --in=midi/entertainer_short.midi --action=add-note-loop --loop-number=0 --track=1
```

**Convert the MIDI file to .seq**
Once MIDI file is in the final state, it can be converted to seq format.

```
../../tools/gaudio/bin/midi2cseq --in=midi/entertainer_short.midi --out=seq/entertainer_short.seq
```

**Update game references to use the new music**

The following are changes that should be applied to the game source code.

It's important that the array indeces for the following operations match. This example will be adding a new music track as array index 62, before the "end something" record.

Edit `src/bondconstants.h` and add a new enum in `enum MUSIC_TRACKS` for the new track. This will be placed at the end of the list, but before the `M_END_SOMETHING` enum. The end of the definition should now look like

```
    M_SURFACE2X,
    M_SURFACE2END,
    M_STATUEPART,
    M_ENTERTAINER,     // this is the new entry
    M_END_SOMETHING
```

Add a new entry at the end of `music_names` file for the new music track, before "End_Something". Reminder, this is the list of files used to compile the soundbank. The end of the file should look like

```
# previously 60-62, now 60-63
Surface2end    
Statuepart

# here is the new line:
entertainer_short

# same as before:
End_Something
```

Edit `src/music.h` and update the `NUM_MUSIC_TRACKS` definition to add one track. It should now look like

```
#define NUM_MUSIC_TRACKS  64
```

Edit `src/music.c` and update the global `g_musicDefaultTrackVolume`. Between index 61 (statuepart) and index 62, add an entry for the new track. The end of the array should now look like

```
/**
* Index 61, M_STATUEPART.
*/
0x6665,

/**
* new Index 62, M_ENTERTAINER.
*/
0x6665,

/**
* old Index 62, M_END_SOMETHING.
*/
0x7332,
```

Change the stage music mapping in `music_setup_entries` from `src/game/music_0D2720.c` to use the new music track. Here I'm changing runway:

```
struct music_setup music_setup_entries[] = {
    ...
    { LEVELID_FACILITY,    M_FACILITY,       0xFFFF,   M_FACILITYX },
    { LEVELID_RUNWAY,      M_ENTERTAINER,    0xFFFF,   M_RUNWAYPLANE },
    { LEVELID_SURFACE,     M_SURFACE1,       0xFFFF,   M_WIND },
    ...
```

**Build the project**

Work is done. Time to test.

```
# starting in assets/audio
cd ../..
make audioclean
cd assets/audio
./build_instruments_from_aifc.sh
./build_soundbank.sh
cd ../..
make -j4
```

**Tips**

- `music_names` and `sfx_names` are used both for extraction and rebuilding, so be careful if you changed one of those and are trying to reset to a working state.
- The Nintendo 64 imposes an upper limit on the keyMax value of one octave more than the keyBase. This is a hard upper limit on how high frequency audio can detune, any value above this will simply play at the max value instead.
- Keymap ranges should not overlap. The manual states the sequence player will choose the first matching keymap if multiple are available.
- The sequence player does not seem to like if the MIDI event channel is different from the track number. This resulted in bad DMA request / hard crash for me.
- The programming manual states MIDI/sequence loop value of zero will loop forever, but a value of zero seems to only play once.
