# Gaudio miditool

Adjust MIDI events within MIDI file

Usage:

```
bin/miditool --in file --action=ACTION [args]
```

Options:

```
    --help                        print this help
    -n,--in=FILE                  input MIDI file
    -a,--action=TEXT              Action to perform
    -c,--channel=INT              Channel value.
    -t,--track=INT                Track value.
    -i,--instrument=INT           Instrument value.
    --loop-number=INT             Loop number value.
    -q,--quiet                    suppress output
    -v,--verbose                  more output

Available actions:
    parse                         Parse all tracks in file and write parse output to stdout.
    parse-track                   Parse single track in file and write parse output to stdout.
                                  Parameters used:
                                  --track
    make-channel-track            Set any event channel to the same as the track number.
    remove-loop                   Remove loop with specified loop number. Parameters used:
                                  --track
                                  --loop-number
    add-note-loop                 Creates a new loop, starting before the first Note On, and
                                  ending after the last Note Off of the track. Parameters used:
                                  --track
                                  --loop-number
    set-channel-instrument        Iterate all events in the file, and for any Program Change event that sets
                                  the instrument for a channel, change it instead to the values
                                  supplied. Parameters used:
                                  --channel
                                  --instrument
```

# Parse

Sometimes it is helpful to understand how gaudio interprets a MIDI file. You can use the `parse` or `parse-track` action of `miditool` to print how a MIDI file or track is parsed. For example:

```
bin/miditool --in=test_cases/midi/entertainer_short.midi --action=parse-track --track=1
```

Output begins
 ```
destination track not resolved, assuming midi track 0
Print MIDI track # 1
id=0, offset=0, abs=0, delta=0 (0x000000), chan=-1, command=0xff51, [0]=3, [1]=363636
id=1, offset=7, abs=39056, delta=39056 (0x82b110), chan=-1, command=0xff2f
id=2, offset=0, abs=5, delta=5 (0x000005), chan=0, command=0x00c0, [0]=0
id=3, offset=3, abs=10, delta=5 (0x000005), chan=0, command=0x00b0, [0]=7, [1]=115
id=4, offset=7, abs=1415, delta=1405 (0x008a7d), chan=0, command=0x0090, [0]=74, [1]=86
id=5, offset=12, abs=1425, delta=10 (0x00000a), chan=0, command=0x0090, [0]=86, [1]=107
id=6, offset=16, abs=1628, delta=203 (0x00814b), chan=0, command=0x0080, [0]=74, [1]=0
...
```

# Sync channel to track

It is required that any MIDI event command channel match the track it is on. This can fixed with action `make-channel-track`. This overwrites the file in-place. For example:

```
bin/miditool --in=test_data/seq/entertainer_short.midi --action=make-channel-track
```

# Loops

See the technical documentation or Programming Manual section 20.5 for how loops are implemented in MIDI tracks. `miditool` can be used to create a simple loop in a track. This will create a loop start event before any Note On at the beginning of the track, and create a loop end event after the last Note Off (or implicit Note Off). If you need to place a loop marker at an arbitrary location you will have to use another tool. This overwrites the file in-place. Example of creating an easy loop:

```
bin/miditool --in=test_data/seq/entertainer_short.midi --action=add-note-loop --track=1 --loop-number=0
```

Which gives output
```
destination track not resolved, assuming midi track 0
creating loop 0 start event at abs time 1415
creating loop 0 end event at abs time 39136
```

A valid loop can also be removed with `miditool` using action `remove-loop`. For example, after running the above command which added a loop:

```
bin/miditool --in=test_data/seq/entertainer_short.midi --action=remove-loop --track=1 --loop-number=0
```

Which gives output
```
destination track not resolved, assuming midi track 0
Found 3 events to remove from track 1
```

# Program Change

The `miditool` can be used to set the Program Change event within a channel (track). For example, to set the Program Change event to instrument 75 in track 1:

```
bin/miditool --in=test_data/seq/entertainer_short.midi --action=set-channel-instrument --channel=1 --instrument=75
```

Which gives output
```
destination track not resolved, assuming midi track 0
Changing MIDI "Program Change" event in track index 1, from 0 to 75
```
