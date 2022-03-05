# Gaudio tbl2aifc

Extracts data from .tbl and .ctl files into .inst and .aifc files.

See [gic](gic.md) documentation for details about .inst file format.

Usage:

```
bin/tbl2aifc --ctl file --tbl file
```

Options:

```
    --help                        print this help
    -c,--ctl=FILE                 .ctl input file (required)
    -t,--tbl=FILE                 .tbl input file (required)
    -d,--dir=PATH                 output directory. Default=snd/
    -p,--prefix=STRING            string to prepend to output aifc files.
                                  Default=snd_
    --inst=FILE                   output .inst filename. Default=snd.inst
    --no-aifc                     don't generate .aifc files
    --no-inst                     don't generate .inst file
    -n,--names=FILE               sound names. One name per line. Lines starting with # ignored.
                                  Names applied in order read, if the list is too short
                                  subsequent items will be given numeric id (0001, 0002, ...).
                                  Non alphanumeric characters ignored.
                                  Do not include filename extension.
    -q,--quiet                    suppress output
    -v,--verbose                  more output
```

# Names

An optional "names" file can be specified with `--names`. `tbl2aifc` will use this to name the files it extracts from the .tbl file. This should contain filenames without extension. If a directory should be specified, use the `--dir` option (or abuse the `--prefix` option).

If a "names" file is not used, it might be helpful to specify a file prefix with `--p`.

If a "names" file is not used (or is too short), gaudio will include the instrument number (unique id) and sound number (unique id) in the filename. This is according to the order of objects parsed from the .ctl file. For example, extracting the retail instruments.ctl/.tbl results in files named as

```
snd_inst_0000_0000.aifc
snd_inst_0001_0001.aifc
snd_inst_0001_0002.aifc
snd_inst_0001_0003.aifc
snd_inst_0001_0004.aifc
snd_inst_0002_0005.aifc
snd_inst_0003_0006.aifc
...
snd_inst_0072_0101.aifc
snd_inst_0072_0102.aifc
snd_inst_0073_0103.aifc
snd_inst_0074_0104.aifc
snd_inst_0074_0105.aifc
```

This corresponds to

- instrument 0, sound 0
- instrument 1, sound 1
- instrument 1, sound 2
- instrument 1, sound 3
- instrument 1, sound 4
- instrument 2, sound 5
- instrument 3, sound 6
- ...
