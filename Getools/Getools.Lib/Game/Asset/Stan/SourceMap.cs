using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.Stan
{
    public class SourceMap
    {
        public static FileMap GetFileMap(int levelId)
        {
            switch (levelId)
            {
                case (int)LevelId.Dam: return new FileMap { Dir = string.Empty, Filename = "Tbg_dam_all_p_stanZ" };
                case (int)LevelId.Facility: return new FileMap { Dir = string.Empty, Filename = "Tbg_ark_all_p_stanZ" };
                case (int)LevelId.Runway: return new FileMap { Dir = string.Empty, Filename = "Tbg_run_all_p_stanZ" };
                case (int)LevelId.Surface: return new FileMap { Dir = string.Empty, Filename = "Tbg_sevx_all_p_stanZ" };
                case (int)LevelId.Bunker1: return new FileMap { Dir = string.Empty, Filename = "Tbg_sev_all_p_stanZ" };
                case (int)LevelId.Silo: return new FileMap { Dir = string.Empty, Filename = "Tbg_silo_all_p_stanZ" };
                case (int)LevelId.Frigate: return new FileMap { Dir = string.Empty, Filename = "Tbg_dest_all_p_stanZ" };
                case (int)LevelId.Surface2: return new FileMap { Dir = string.Empty, Filename = "Tbg_sevx_all_p_stanZ" };
                case (int)LevelId.Bunker2: return new FileMap { Dir = string.Empty, Filename = "Tbg_sevb_all_p_stanZ" };
                case (int)LevelId.Statue: return new FileMap { Dir = string.Empty, Filename = "Tbg_stat_all_p_stanZ" };
                case (int)LevelId.Archives: return new FileMap { Dir = string.Empty, Filename = "Tbg_arch_all_p_stanZ" };
                case (int)LevelId.Streets: return new FileMap { Dir = string.Empty, Filename = "Tbg_pete_all_p_stanZ" };
                case (int)LevelId.Depot: return new FileMap { Dir = string.Empty, Filename = "Tbg_depo_all_p_stanZ" };
                case (int)LevelId.Train: return new FileMap { Dir = string.Empty, Filename = "Tbg_tra_all_p_stanZ" };
                case (int)LevelId.Jungle: return new FileMap { Dir = string.Empty, Filename = "Tbg_jun_all_p_stanZ" };
                case (int)LevelId.Control: return new FileMap { Dir = string.Empty, Filename = "Tbg_arec_all_p_stanZ" };
                case (int)LevelId.Caverns: return new FileMap { Dir = string.Empty, Filename = "Tbg_cave_all_p_stanZ" };
                case (int)LevelId.Cradle: return new FileMap { Dir = string.Empty, Filename = "Tbg_crad_all_p_stanZ" };
                case (int)LevelId.Aztec: return new FileMap { Dir = string.Empty, Filename = "Tbg_azt_all_p_stanZ" };
                case (int)LevelId.Egypt: return new FileMap { Dir = string.Empty, Filename = "Tbg_cryp_all_p_stanZ" };
                case (int)LevelId.Cuba: return new FileMap { Dir = string.Empty, Filename = "Tbg_len_all_p_stanZ" };
            }

            throw new NotImplementedException();
        }
    }
}
