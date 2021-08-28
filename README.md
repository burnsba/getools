# getools suite
Goldeneye 007 command line tool suite for N64 hacking. Built in C#, dotnet core 3.1.

Current work in progress.

# getools

Command line application. Wrapper for main library.

Available actions:

- convert_stan: convert stan file between/from various file formats

# getools.lib

The core code is contained in this library project.

## convert_stan

Run with 

    getools.exe convert_stan
    
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
      --input-data-is-beta        (Default: false) Flag for input to use beta data structures/formats.
      --output-data-is-beta       (Default: false) Flag for output to use beta data structures/formats.
      --help                      Display this help screen.
    
    The following values are supported for input "FTYPE":
    C, Json, Bin
    
    The following values are supported for output "FTYPE"
    C, Json

Example powershell script to convert all .bin stans to .c files:

    $stans = "Tbg_ame_all_p_stanZ", "Tbg_arch_all_p_stanZ", "Tbg_arec_all_p_stanZ", "Tbg_ark_all_p_stanZ", "Tbg_ash_all_p_stanZ", "Tbg_azt_all_p_stanZ", "Tbg_cave_all_p_stanZ", "Tbg_crad_all_p_stanZ", "Tbg_cryp_all_p_stanZ", "Tbg_dam_all_p_stanZ", "Tbg_depo_all_p_stanZ", "Tbg_dest_all_p_stanZ", "Tbg_dish_all_p_stanZ", "Tbg_imp_all_p_stanZ", "Tbg_jun_all_p_stanZ", "Tbg_len_all_p_stanZ", "Tbg_oat_all_p_stanZ", "Tbg_pete_all_p_stanZ", "Tbg_ref_all_p_stanZ", "Tbg_run_all_p_stanZ","Tbg_sev_all_p_stanZ", "Tbg_sevb_all_p_stanZ", "Tbg_sevx_all_p_stanZ", "Tbg_silo_all_p_stanZ", "Tbg_stat_all_p_stanZ", "Tbg_tra_all_p_stanZ" 

    $stans | ForEach-Object { .\Getools.exe convert_stan --input-file="$_.bin" --output-file="$_.c" }

    .\Getools.exe convert_stan --input-file="Tbg_cat_all_p_stanZ.bin" --output-file="Tbg_cat_all_p_stanZ.c" --input-data-is-beta=true --output-data-is-beta=true
    
Convert .bin format to .c file:

    getools convert_stan --input-file Tbg_jun_all_p_stanZ.bin --output-file Tbg_jun_all_p_stanZ.c
    
Convert beta .bin format to (non-beta) .c file:

    getools convert_stan --input-file Tbg_cat_all_p_stanZ.bin --input-data-is-beta True --output-file Tbg_cat_all_p_stanZ.c
  
Parse .c file with beta types, convert to json:

    getools convert_stan --input-file Tbg_cat_all_p_stanZ.c --input-data-is-beta True --output-file out --output-file-type json


## convert_setup

Run with 

    getools.exe convert_setup
    
Example powershell script to convert all .bin setups to .c files:

    # version invariant setups
    $assetFolder = "../../../../asset/setup/"
    $setups = "Ump_setupameZ", "Ump_setuparkZ", "Ump_setupashZ", "Ump_setupcaveZ", "Ump_setupcradZ", "Ump_setupcrypZ", "Ump_setupdishZ", "Ump_setupimpZ", "Ump_setupoatZ", "Ump_setuprefZ", "Ump_setupsevbZ", "Ump_setupstatueZ", "UsetuparchZ", "UsetuparkZ", "UsetupaztZ", "UsetupcaveZ", "UsetupcontrolZ", "UsetupcrypZ", "UsetupdamZ", "UsetupdepoZ", "UsetuppeteZ", "UsetuprunZ", "UsetupsevbunkerZ", "UsetupsevbZ", "UsetupsevxbZ", "UsetupsevxZ"
    $setups | ForEach-Object { .\Getools.exe convert_setup --input-file="${assetFolder}${_}.bin" --output-file="${assetFolder}${_}.c" }
    
    # version=US setups
    $assetFolder = "../../../../asset/setup/u/"
    $setups = "Ump_setuparchZ", "UsetupcradZ", "UsetupdestZ", "UsetupjunZ", "UsetuplenZ", "UsetupsiloZ", "UsetupstatueZ", "UsetuptraZ"
    $setups | ForEach-Object { .\Getools.exe convert_setup --input-file="${assetFolder}${_}.bin" --output-file="${assetFolder}${_}.c" }