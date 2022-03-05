# Tutorial: Goldeneye custom sound effect

Note: N64 playback applies a number of effects (like reverb), so the resulting wav files will not sound the same on PC as in the game.

To convert an .aifc to .wav, use the gaudio tool `aifc2wav`. See gaudio documentation for further details on available command line options.

Note that the sound effects are generally stored at various sampling rates. This means the sound effect will need to be "detuned" (or retuned rather) to the correct frequency. This information is captured in the .inst file, under the "detune" and "keybase" properties in a keymap. `aifc2wav` happens to support extracting this information from an .inst file when converting to wav. Here is an example to convert the rocket launch sound effect to wav. This example is run from the audio/assets folder, and it uses the sound effect names from the "sfx_names" file:

```
../../tools/gaudio/bin/aifc2wav --in aifc_sfx/Rocket_Launch.aifc --out aifc_sfx/Rocket_Launch.wav --inst-file=aifc_sfx/sfx.inst --inst-search=use --inst-val="aifc_sfx/Rocket_Launch.aifc"
```

Wav files can be converted to .aifc files for use in the project. These must be signed 16 bit mono wav files. In order to convert from .wav to .aifc using the standard compression algorithm, suitable "coefficents" must be found. The gaudio tool `tabledesign` can do this; note this depends on the GNU Scientific Library being installed. Following is an example to replace the Rocket Launch sound effect with a file called "ont.wav". This example is run from assets/audio folder, and uses a file called "ont.wav" in the assets/audio/wav folder.

**Step 1:**  

Convert wav file to 16 bit mono. Take note of the sampling frequency. In this example, the file has a sample rate of 22050 Hz, approximately 2.4 seconds long.

**Step 2:**  

Search for compression coefficients using `tabledesign`:  

```
../../tools/gaudio/bin/tabledesign --in wav/ont.wav
```

A file named "ont.coef" should now exist in the `wav/` folder.

**Step 3:**  

Convert from wav to aifc, and place output in `aifc_sfx/` folder  

```
../../tools/gaudio/bin/wav2aifc --in=wav/ont.wav --coef=wav/ont.coef --out=aifc_sfx/ont.aifc
```

**Step 4:**  

Update .inst file (file path)  

Search the sfx.inst file for a line that says `use ("aifc_sfx/Rocket_Launch.aifc");`. Change the file path to the .aifc file just created. It should now look like

```
sound Sound0000 {
    metaCtlWriteOrder = 1;
    use ("aifc_sfx/ont.aifc");

    pan = 64;
    volume = 100;
    envelope = Envelope0000;
    keymap = Keymap0000;
}
```

**Step 5:**  

Update .inst file (detune)  

The game plays back sound effects at a rate of 22050 Hz. If the sample rate of the audio file is different from playback, then a "detune" parameter needs to be specified in the .inst file. This is specified by the "keybase" (one MIDI note), and "detune" (one hundredth of a note). The formula to convert between sample rate <-> keybase and detune is

```
hw_sample_rate / 2^( (60 - (keybase + detune/100))/12 )
```

The "keybase" and "detune" parameter are specified in the `keymap` defined in the `sound`. In the example above, the `sound` object is named `Sound0000`, and the `keymap` object is named `Keymap0000`. Find the `Keymap0000` in the .inst file:

```
keymap Keymap0000 {
    metaCtlWriteOrder = 38;
    velocityMin = 0;
    velocityMax = 0;
    keyMin = 1;
    keyMax = 0;
    keyBase = 54;
    detune = 40;
}
```

The Rocket Launch sound effect is applying a keybase of 54 and detune of 40. Applying this in the above formula

```
22050 / 2^( (60 - (54 + 40/100))/12 ) = 15956
```

So the Rocket Launch sound effect sampling rate is 15956 Hz. Back to the "ont" example, this has a sample rate of 22050 Hz. Because this is the same as playback, set the keybase to the "natural" value (60=middle C4), and detune to 0. It should now look like

```
keymap Keymap0000 {
    metaCtlWriteOrder = 38;
    velocityMin = 0;
    velocityMax = 0;
    keyMin = 1;
    keyMax = 0;
    keyBase = 60;
    detune = 0;
}
```

**Step 6:**  

Update .inst (playback time)

The .inst file describes how long the it takes to play the sound effect, and the volume it should be played at. There three durations that need to be specified and two volumes. There is attack time, which is the amount of time it takes to ramp from 0 to the "attack volume." There is decay time, which is the amount of time it takes to go from the "attack volume" to "decay volume." Then there is the release time, which is the amount of time it takes to go from the "decay volume" to zero.

```
[start] -> (attack time) to [attack volume] -> (decay time) to [decay volume] -> (release time) to zero volume
```

The .inst file specifies these times and volumes in the `envelope` of a sound. Times are in units of micro seconds. Volume can range from 0 (silent) to 127 (full volume).

Find the `envelope` for the rocket launch sound effect. This is `Envelope0000`. Looking at the current settings, the sound effect instantly starts at full volume. It plays for 0.933830 seconds ending at full volume. Then it fades to zero volume over 0.002 seconds. Since the audio sample in this example is 2.4 seconds long, change the decayTime to 2300000 (microseconds). The `envelope` should now look like

```
envelope Envelope0000 {
    metaCtlWriteOrder = 127;
    attackTime = 0;
    attackVolume = 127;
    decayTime = 2300000;
    decayVolume = 127;
    releaseTime = 2000;
}
```

**Step 7:**  

compile  

Save the .inst changes above. Run the `build_sfx_from_aifc.sh` script. This should warn about checksums not matching. Rebuild the project.

```
cd ../..
make audioclean
cd assets/audio
./build_sfx_from_aifc.sh
make -j4
```

Demo of the above on everdrive on real console: [https://www.youtube.com/watch?v=vO2vf10bBu8](https://www.youtube.com/watch?v=vO2vf10bBu8)
