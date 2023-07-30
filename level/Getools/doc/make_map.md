## make_map

Run with

    getools.exe make_map

Command line help output:

    USAGE:

      --stan               stan filename (uncompressed)
      --stan-beta          (Default: false) Flag for input stan to use beta data structures/formats.
      --setup              setup filename.
      --bg                 bg filename.
      --scale              (Default: 1) Level scale.
      --slice-z            Slice stage at singular value (plane), perpendicular offset from ground
                           (internal y value).
      --min-z              Lower boundary of bounding box to determine points of interest (internal y
                           value).
      --max-z              Upper boundary of bounding box to determine points of interest (internal y
                           value).
      -o, --output-file    Output filename (with extension).
      --help               Display this help screen.

## Level scale

Level scale is required to generate an accurate map. Scales for the default game stages are as follows:

    LEVELID_BUNKER1   0.53931433  
    LEVELID_SILO      0.47256002  
    LEVELID_STATUE    0.107202865
    LEVELID_CONTROL   0.49886572  
    LEVELID_ARCHIVES  0.50678575  
    LEVELID_TRAIN     0.15019713  
    LEVELID_FRIGATE   0.44757429  
    LEVELID_BUNKER2   0.53931433  
    LEVELID_AZTEC     0.35300568  
    LEVELID_STREETS   0.34187999  
    LEVELID_DEPOT     0.21847887  
    LEVELID_COMPLEX   0.94285715  
    LEVELID_EGYPT     0.25608     
    LEVELID_DAM       0.23363999  
    LEVELID_FACILITY  1.20648     
    LEVELID_RUNWAY    0.089571431
    LEVELID_SURFACE   0.45445713  
    LEVELID_JUNGLE    0.094662853
    LEVELID_TEMPLE    0.47142857  
    LEVELID_CAVERNS   0.26824287  
    LEVELID_CITADEL   0.76852286  
    LEVELID_CRADLE    0.23571429  
    LEVELID_SHO       0.528       
    LEVELID_SURFACE2  0.45445713  
    LEVELID_ELD       0.94285715  
    LEVELID_BASEMENT  0.65999997  
    LEVELID_STACK     0.65999997  
    LEVELID_LUE       0.94285715  
    LEVELID_LIBRARY   0.65999997  
    LEVELID_RIT       0.94285715  
    LEVELID_CAVES     0.14142857  
    LEVELID_EAR       0.94285715  
    LEVELID_LEE       0.94285715  
    LEVELID_LIP       0.94285715  
    LEVELID_CUBA      0.094662853
    LEVELID_WAX       0.94285715  
    LEVELID_PAM       0.94285715  
    LEVELID_MAX       0.94285715  

## Example usage

Generate Train NTSC SVG map:

    Getools.exe make_map --bg="bg/bg_tra_all_p.bin" --stan="stan/Tbg_tra_all_p_stanZ.bin" --setup="setup/u/UsetuptraZ.bin" --output-file="u/Train.svg" --scale=0.15019713

Generate Citadel SVG map (beta stan, missing setup):

    Getools.exe make_map --bg="bg/bg_cat_all_p.bin" --stan="stan/Tbg_cat_all_p_stanZ.bin" --stan-beta --output-file="u/Citadel.svg" --scale=0.76852286

Generate beta "rit" map (bg file only):

    Getools.exe make_map --bg="bg/bg_rit_all_p.bin"  --output-file="u/Rit.svg" --scale=0.94285715

See the file [build_all_maps](build_all_maps.ps1) for how to create SVG maps for all stages. This assumes all setup, stan, and bg data files have already been extracted and uncompressed.
