## convert_setup

Run with

    getools.exe convert_setup

Command line help output:

    USAGE:

      -i, --input-file            Required. Input filename.
      --input-file-type=FTYPE     Describes file type, such as whether this is a binary file or json.
                                  Attempts to guess the format based on file extension if not set.
      -o, --output-file           Required. Output filename.
      --output-file-type=FTYPE    Describes file type, such as whether this is a binary file or json.
                                  Attempts to guess the format based on file extension if not set.
      -d, --dname                 Container object declaration name, used when converting to
                                  code/source. Defaults to input filename without extension if not set.
      --help                      Display this help screen.

    The following values are supported for input "FTYPE":
    Json, Bin

    The following values are supported for output "FTYPE"
    C, Json

## Example usage

Example powershell script to convert all .bin setups to .c files:

    # version invariant setups
    $assetFolder = "../../../../asset/setup/"
    $setups = "Ump_setupameZ", "Ump_setuparkZ", "Ump_setupashZ", "Ump_setupcaveZ", "Ump_setupcradZ", "Ump_setupcrypZ", "Ump_setupdishZ", "Ump_setupimpZ", "Ump_setupoatZ", "Ump_setuprefZ", "Ump_setupsevbZ", "Ump_setupstatueZ", "UsetuparchZ", "UsetuparkZ", "UsetupaztZ", "UsetupcaveZ", "UsetupcontrolZ", "UsetupcrypZ", "UsetupdamZ", "UsetupdepoZ", "UsetuppeteZ", "UsetuprunZ", "UsetupsevbunkerZ", "UsetupsevbZ", "UsetupsevxbZ", "UsetupsevxZ"
    $setups | ForEach-Object { .\Getools.exe convert_setup --input-file="${assetFolder}${_}.bin" --output-file="${assetFolder}${_}.c" }

    # version=US setups
    $assetFolder = "../../../../asset/setup/u/"
    $setups = "Ump_setuparchZ", "UsetupcradZ", "UsetupdestZ", "UsetupjunZ", "UsetuplenZ", "UsetupsiloZ", "UsetupstatueZ", "UsetuptraZ"
    $setups | ForEach-Object { .\Getools.exe convert_setup --input-file="${assetFolder}${_}.bin" --output-file="${assetFolder}${_}.c" }
