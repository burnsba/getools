## unzip

This is a command line tool to inflate a Rare compressed 1172 blob. This also works to inflate regular gzip compressed data, using the `inflate.c` implementation used by Rare.

Run with

    getools.exe unzip

Command line help output:

    USAGE:

      -i, --input-file     Required. Input filename.
      -o, --output-file    Required. Output filename.
      -t, --trace          (Default: false) Debug level TRACE.
      --help               Display this help screen.

## Example usage

Inflate a 1172 compressed stan file:

    Getools.exe unzip -i Tbg_arch_all_p_stanZ.bin.z -o Tbg_arch_all_p_stanZ.bin
