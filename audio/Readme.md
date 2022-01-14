programs:

aifc2wav
sbksplit
seq2midi
tbl2aifc

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