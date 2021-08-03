using System;
using System.Collections.Generic;
using System.IO;
using Getools.Lib.Converters;

namespace Getools
{
    class Program
    {
        static void Main(string[] args)
        {
            //var path = "../../../test.c";

            //var stan = StanConverters.ParseFromC(path);

            //StanConverters.WriteToC(stan, "test_write.c");
            //StanConverters.WriteToBin(stan, "test_write.bin");

            //using (var br = new BinaryReader(new FileStream("Tbg_depo_all_p_stanZ.bin", FileMode.Open)))
            //{
            //    var stan_round2 = Getools.Lib.Game.Asset.Stan.StandFile.ReadFromBinFile(br, "Tbg_depo_all_p_stanZ");
            //    StanConverters.WriteToC(stan_round2, "Tbg_depo_all_p_stanZ.c");
            //    StanConverters.WriteToBin(stan_round2, "Tbg_depo_all_p_stanZ_2.bin");
            //}

            //using (var br = new BinaryReader(new FileStream("Tbg_depo_all_p_stanZ_2.bin", FileMode.Open)))
            //{
            //    var stan = Getools.Lib.Game.Asset.Stan.StandFile.ReadFromBinFile(br, "Tbg_depo_all_p_stanZ");
            //    StanConverters.WriteToC(stan, "Tbg_depo_all_p_stanZ_3.c");
            //    StanConverters.WriteToBin(stan, "Tbg_depo_all_p_stanZ_3.bin");
            //}

            var files = new List<string>()
            {
                "Tbg_azt_all_p_stanZ",
                //"Tbg_cat_all_p_stanZ",
                "Tbg_crad_all_p_stanZ",
                "Tbg_dam_all_p_stanZ",
                "Tbg_dest_all_p_stanZ",
                "Tbg_pete_all_p_stanZ",
                "Tbg_run_all_p_stanZ",
                "Tbg_silo_all_p_stanZ",
                "Tbg_stat_all_p_stanZ",
                "Tbg_tra_all_p_stanZ",
            };

            foreach (var file in files)
            {
                using (var br = new BinaryReader(new FileStream(file + ".bin", FileMode.Open)))
                {
                    var stan = Getools.Lib.Game.Asset.Stan.StandFile.ReadFromBinFile(br, file);
                    StanConverters.WriteToC(stan, file + ".c");
                    StanConverters.WriteToBin(stan, file + "_check.bin");
                }
            }
        }
    }
}
