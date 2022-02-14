programs:

aifc2wav
cseq2midi
sbksplit
tbl2aifc

clang code analysis: scan-build-11 make

The tabledesign application depends on the GNU Scientific Library. On debian-like systems, this can be installed with  

```
sudo apt-get intstall libgsl-dev
```

-----

.inst parse support:

- instrument-sounds and bank->instruments are sorted by array index.
- Array indeces are required. The devkit example seems to make these optional.
- expanded support:
--- comments
--- metaCtlWriteOrder (envelope, keymap, sound)
--- sampleRate (bank)

-----

Add "further reading" or "references" section for the file spec stuff.

-----


Pan values range from 0 to 127, with 0 being full left, 64 center pan, and 127 full right.

Volumes are from 0 to 127, with 0 meaning there will be no sound, and 127 being full volume. 

Note: Keymaps are used only by the sequence player. They are ignored by the sound player. 

Note: The Nintendo 64 imposes an upper limit on the keyMax value of one octave more than the keyBase.

-----

```
for file in test_data/seq/*.seq.rz
do
    OUTPUT_FILENAME=$(echo "${file}" | sed -e 's/seq\.rz$/seq/')
    if [ "${file}" = "${OUTPUT_FILENAME}" ]; then
        echo "cannot determine what to rename file: ${file}"
        break
    fi
    GZ=../gzipsrc/gzip ../1172inflate.sh "${file}" "${OUTPUT_FILENAME}"
done
```

for file in test_data/seq/Aztec.*.rz ; do echo "${file}" | sed -e 's/seq\.rz$/seq/' ; done

-----

todo:



- final valgrind check
- readme writeup
- if only one input is required, and one input provided, assume it's the `--in` parameter


feature roadmap:

- decomp extract script
--- extract sbk -> cseq -> midi
--- extract instruments -> .tbl, .ctl -> .inst, .aifc -> .wav
--- extract sounds -> .tbl, .ctl -> .inst, .aifc -> .wav

- decomp build script
--- pack cseq into cseq.rz and combine into .sbk
--- sound .inst + .aifc -> .tbl, .ctl
--- instrument .inst + .aifc -> .tbl, .ctl

- calculate encode codebook
--- encode/decode algorithm writeup

- app: midi2cseq -- convert midi to cseq

- app: ??? -- list controllers used by which midi tracks
- app: cseq2wav -- convert midi to wav but automatically apply the correct instrument sounds

- add parse flag to `ALBankFile_new_from_inst` to ignore unreferenced elements (currently fatal error)
--- should print a list of text_id
- parser should also check for instance declarations with no properties set