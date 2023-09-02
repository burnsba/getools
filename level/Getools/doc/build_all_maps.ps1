# run from folder: asset

mkdir -Force "svgmap"
mkdir -Force "svgmap/e"
mkdir -Force "svgmap/u"
mkdir -Force "svgmap/j"

$outFolder = "svgmap/"

$app = '..\Getools\bin\Debug\net6.0\Getools.exe'

# Dam
& $app make_map --bg="bg/bg_dam_all_p.bin" --stan="stan/Tbg_dam_all_p_stanZ.bin" --setup="setup/UsetupdamZ.bin" --output-file="${outFolder}u/Dam.svg" --scale=0.23363999

# Facility
& $app make_map --bg="bg/bg_ark_all_p.bin" --stan="stan/Tbg_ark_all_p_stanZ.bin" --setup="setup/UsetuparkZ.bin" --output-file="${outFolder}u/Facility.svg" --scale=1.20648

# Runway
& $app make_map --bg="bg/bg_run_all_p.bin" --stan="stan/Tbg_run_all_p_stanZ.bin" --setup="setup/UsetuprunZ.bin" --output-file="${outFolder}u/Runway.svg" --scale=0.089571431

# Surface 1
& $app make_map --bg="bg/bg_sevx_all_p.bin" --stan="stan/Tbg_sevx_all_p_stanZ.bin" --setup="setup/UsetupsevxZ.bin" --output-file="${outFolder}u/Surface 1.svg" --scale=0.45445713

# Bunker 1
& $app make_map --bg="bg/bg_sev_all_p.bin" --stan="stan/Tbg_sev_all_p_stanZ.bin" --setup="setup/UsetupsevbunkerZ.bin" --output-file="${outFolder}u/Bunker 1.svg" --scale=0.53931433

# Silo
& $app make_map --bg="bg/bg_silo_all_p.bin" --stan="stan/Tbg_silo_all_p_stanZ.bin" --setup="setup/u/UsetupsiloZ.bin" --output-file="${outFolder}u/Silo.svg" --scale=0.47256002
& $app make_map --bg="bg/bg_silo_all_p.bin" --stan="stan/Tbg_silo_all_p_stanZ.bin" --setup="setup/e/UsetupsiloZ.bin" --output-file="${outFolder}e/Silo.svg" --scale=0.47256002
& $app make_map --bg="bg/bg_silo_all_p.bin" --stan="stan/Tbg_silo_all_p_stanZ.bin" --setup="setup/j/UsetupsiloZ.bin" --output-file="${outFolder}j/Silo.svg" --scale=0.47256002

# Frigate
& $app make_map --bg="bg/bg_dest_all_p.bin" --stan="stan/Tbg_dest_all_p_stanZ.bin" --setup="setup/u/UsetupdestZ.bin" --output-file="${outFolder}u/Frigate.svg" --scale=0.44757429
& $app make_map --bg="bg/bg_dest_all_p.bin" --stan="stan/Tbg_dest_all_p_stanZ.bin" --setup="setup/e/UsetupdestZ.bin" --output-file="${outFolder}e/Frigate.svg" --scale=0.44757429
& $app make_map --bg="bg/bg_dest_all_p.bin" --stan="stan/Tbg_dest_all_p_stanZ.bin" --setup="setup/j/UsetupdestZ.bin" --output-file="${outFolder}j/Frigate.svg" --scale=0.44757429

# Surface 2
& $app make_map --bg="bg/bg_sevx_all_p.bin" --stan="stan/Tbg_sevx_all_p_stanZ.bin" --setup="setup/UsetupsevxbZ.bin" --output-file="${outFolder}u/Surface 2.svg" --scale=0.45445713

# Bunker 2
& $app make_map --bg="bg/bg_sevb_all_p.bin" --stan="stan/Tbg_sevb_all_p_stanZ.bin" --setup="setup/UsetupsevbZ.bin" --output-file="${outFolder}u/Bunker 2.svg" --scale=0.53931433

# Statue
& $app make_map --bg="bg/bg_stat_all_p.bin" --stan="stan/Tbg_stat_all_p_stanZ.bin" --setup="setup/u/UsetupstatueZ.bin" --output-file="${outFolder}u/Statue.svg" --scale=0.107202865
& $app make_map --bg="bg/bg_stat_all_p.bin" --stan="stan/Tbg_stat_all_p_stanZ.bin" --setup="setup/e/UsetupstatueZ.bin" --output-file="${outFolder}e/Statue.svg" --scale=0.107202865
& $app make_map --bg="bg/bg_stat_all_p.bin" --stan="stan/Tbg_stat_all_p_stanZ.bin" --setup="setup/j/UsetupstatueZ.bin" --output-file="${outFolder}j/Statue.svg" --scale=0.107202865

# Archives
& $app make_map --bg="bg/bg_arch_all_p.bin" --stan="stan/Tbg_arch_all_p_stanZ.bin" --setup="setup/UsetuparchZ.bin" --output-file="${outFolder}u/Archives.svg" --scale=0.50678575

# Streets
& $app make_map --bg="bg/bg_pete_all_p.bin" --stan="stan/Tbg_pete_all_p_stanZ.bin" --setup="setup/UsetuppeteZ.bin" --output-file="${outFolder}u/Streets.svg" --scale=0.34187999
& $app make_map --bg="bg/e/bg_pete_all_p.bin" --stan="stan/Tbg_pete_all_p_stanZ.bin" --setup="setup/UsetuppeteZ.bin" --output-file="${outFolder}e/Streets.svg" --scale=0.34187999

# Depot
& $app make_map --bg="bg/bg_depo_all_p.bin" --stan="stan/Tbg_depo_all_p_stanZ.bin" --setup="setup/UsetupdepoZ.bin" --output-file="${outFolder}u/Depot.svg" --scale=0.21847887

# Train
& $app make_map --bg="bg/bg_tra_all_p.bin" --stan="stan/Tbg_tra_all_p_stanZ.bin" --setup="setup/u/UsetuptraZ.bin" --output-file="${outFolder}u/Train.svg" --scale=0.15019713
& $app make_map --bg="bg/bg_tra_all_p.bin" --stan="stan/Tbg_tra_all_p_stanZ.bin" --setup="setup/e/UsetuptraZ.bin" --output-file="${outFolder}e/Train.svg" --scale=0.15019713
& $app make_map --bg="bg/bg_tra_all_p.bin" --stan="stan/Tbg_tra_all_p_stanZ.bin" --setup="setup/j/UsetuptraZ.bin" --output-file="${outFolder}j/Train.svg" --scale=0.15019713

# Jungle
& $app make_map --bg="bg/bg_jun_all_p.bin" --stan="stan/Tbg_jun_all_p_stanZ.bin" --setup="setup/u/UsetupjunZ.bin" --output-file="${outFolder}u/Jungle.svg" --scale=0.094662853
& $app make_map --bg="bg/e/bg_jun_all_p.bin" --stan="stan/Tbg_jun_all_p_stanZ.bin" --setup="setup/e/UsetupjunZ.bin" --output-file="${outFolder}e/Jungle.svg" --scale=0.094662853
& $app make_map --bg="bg/bg_jun_all_p.bin" --stan="stan/Tbg_jun_all_p_stanZ.bin" --setup="setup/j/UsetupjunZ.bin" --output-file="${outFolder}j/Jungle.svg" --scale=0.094662853

# Control
& $app make_map --bg="bg/bg_arec_all_p.bin" --stan="stan/Tbg_arec_all_p_stanZ.bin" --setup="setup/UsetupcontrolZ.bin" --output-file="${outFolder}u/Control.svg" --scale=0.49886572

# Caverns
& $app make_map --bg="bg/bg_cave_all_p.bin" --stan="stan/Tbg_cave_all_p_stanZ.bin" --setup="setup/UsetupcaveZ.bin" --output-file="${outFolder}u/Caverns.svg" --scale=0.26824287

# Cradle
& $app make_map --bg="bg/bg_crad_all_p.bin" --stan="stan/Tbg_crad_all_p_stanZ.bin" --setup="setup/u/UsetupcradZ.bin" --output-file="${outFolder}u/Cradle.svg" --scale=0.23571429
& $app make_map --bg="bg/bg_crad_all_p.bin" --stan="stan/Tbg_crad_all_p_stanZ.bin" --setup="setup/e/UsetupcradZ.bin" --output-file="${outFolder}e/Cradle.svg" --scale=0.23571429
& $app make_map --bg="bg/bg_crad_all_p.bin" --stan="stan/Tbg_crad_all_p_stanZ.bin" --setup="setup/j/UsetupcradZ.bin" --output-file="${outFolder}j/Cradle.svg" --scale=0.23571429

# Aztec
& $app make_map --bg="bg/bg_azt_all_p.bin" --stan="stan/Tbg_azt_all_p_stanZ.bin" --setup="setup/UsetupaztZ.bin" --output-file="${outFolder}u/Aztec.svg" --scale=0.35300568

# Egypt
& $app make_map --bg="bg/bg_cryp_all_p.bin" --stan="stan/Tbg_cryp_all_p_stanZ.bin" --setup="setup/UsetupcrypZ.bin" --output-file="${outFolder}u/Egypt.svg" --scale=0.25608

# ------

# credits
& $app make_map --bg="bg/bg_len_all_p.bin" --stan="stan/Tbg_len_all_p_stanZ.bin" --setup="setup/u/UsetuplenZ.bin" --output-file="${outFolder}u/Cuba.svg" --scale=0.094662853

# ------

# multiplayer (solo maps)
& $app make_map --bg="bg/bg_arch_all_p.bin" --stan="stan/Tbg_arch_all_p_stanZ.bin" --setup="setup/u/Ump_setuparchZ.bin" --output-file="${outFolder}u/Mp_Archives.svg" --scale=0.50678575
& $app make_map --bg="bg/bg_arch_all_p.bin" --stan="stan/Tbg_arch_all_p_stanZ.bin" --setup="setup/e/Ump_setuparchZ.bin" --output-file="${outFolder}e/Mp_Archives.svg" --scale=0.50678575
& $app make_map --bg="bg/bg_arch_all_p.bin" --stan="stan/Tbg_arch_all_p_stanZ.bin" --setup="setup/j/Ump_setuparchZ.bin" --output-file="${outFolder}j/Mp_Archives.svg" --scale=0.50678575
& $app make_map --bg="bg/bg_ark_all_p.bin" --stan="stan/Tbg_ark_all_p_stanZ.bin" --setup="setup/Ump_setuparkZ.bin" --output-file="${outFolder}u/Mp_Facility.svg" --scale=1.20648
& $app make_map --bg="bg/bg_cave_all_p.bin" --stan="stan/Tbg_cave_all_p_stanZ.bin" --setup="setup/Ump_setupcaveZ.bin" --output-file="${outFolder}u/Mp_Caverns.svg" --scale=0.26824287
& $app make_map --bg="bg/bg_crad_all_p.bin" --stan="stan/Tbg_crad_all_p_stanZ.bin" --setup="setup/Ump_setupcradZ.bin" --output-file="${outFolder}u/Mp_Cradle.svg" --scale=0.23571429
& $app make_map --bg="bg/bg_cryp_all_p.bin" --stan="stan/Tbg_cryp_all_p_stanZ.bin" --setup="setup/Ump_setupcrypZ.bin" --output-file="${outFolder}u/Mp_Egypt.svg" --scale=0.25608
& $app make_map --bg="bg/bg_sev_all_p.bin" --stan="stan/Tbg_sev_all_p_stanZ.bin" --setup="setup/Ump_setupsevbZ.bin" --output-file="${outFolder}u/Mp_Bunker 1.svg" --scale=0.53931433
& $app make_map --bg="bg/bg_stat_all_p.bin" --stan="stan/Tbg_stat_all_p_stanZ.bin" --setup="setup/Ump_setupstatueZ.bin" --output-file="${outFolder}u/Mp_Statue.svg" --scale=0.107202865

# multiplayer (mp maps)
& $app make_map --bg="bg/bg_ame_all_p.bin" --stan="stan/Tbg_ame_all_p_stanZ.bin" --setup="setup/Ump_setupimpZ.bin" --output-file="${outFolder}u/Mp_Basement.svg" --scale=0.65999997
& $app make_map --bg="bg/bg_ame_all_p.bin" --stan="stan/Tbg_ame_all_p_stanZ.bin" --setup="setup/Ump_setupashZ.bin" --output-file="${outFolder}u/Mp_Stack.svg" --scale=0.65999997
& $app make_map --bg="bg/bg_oat_all_p.bin" --stan="stan/Tbg_oat_all_p_stanZ.bin" --setup="setup/Ump_setupoatZ.bin" --output-file="${outFolder}u/Mp_Caves.svg" --scale=0.14142857
& $app make_map --bg="bg/bg_ame_all_p.bin" --stan="stan/Tbg_ame_all_p_stanZ.bin" --setup="setup/Ump_setupameZ.bin" --output-file="${outFolder}u/Mp_Library.svg" --scale=0.65999997
& $app make_map --bg="bg/bg_dish_all_p.bin" --stan="stan/Tbg_dish_all_p_stanZ.bin" --setup="setup/Ump_setupdishZ.bin" --output-file="${outFolder}u/Mp_Temple.svg" --scale=0.47142857
& $app make_map --bg="bg/bg_ref_all_p.bin" --stan="stan/Tbg_ref_all_p_stanZ.bin" --setup="setup/Ump_setuprefZ.bin" --output-file="${outFolder}u/Mp_Complex.svg" --scale=0.94285715

# ------

# Citadel
& $app make_map --bg="bg/bg_cat_all_p.bin" --stan="stan/Tbg_cat_all_p_stanZ.bin" --stan-beta --output-file="${outFolder}u/Citadel.svg" --scale=0.76852286
