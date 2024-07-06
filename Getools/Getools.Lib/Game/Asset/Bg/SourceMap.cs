using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.Bg
{
    /// <summary>
    /// Filenames embedded in retail version of game.
    /// </summary>
    public class SourceMap
    {
        /// <summary>
        /// Gets file info relative to root asset/obseg/bg location.
        /// </summary>
        /// <param name="version">Region version.</param>
        /// <param name="levelId">Level.</param>
        /// <returns>File info.</returns>
        /// <exception cref="NotImplementedException">Thrown on invalid level id.</exception>
        public static FileMap GetFileMap(Enums.Version version, int levelId)
        {
            switch (levelId)
            {
                case (int)LevelId.Dam: return new FileMap { Dir = string.Empty, Filename = "bg_dam_all_p" };
                case (int)LevelId.Facility: return new FileMap { Dir = string.Empty, Filename = "bg_ark_all_p" };
                case (int)LevelId.Runway: return new FileMap { Dir = string.Empty, Filename = "bg_run_all_p" };
                case (int)LevelId.Surface: return new FileMap { Dir = string.Empty, Filename = "bg_sevx_all_p" };
                case (int)LevelId.Bunker1: return new FileMap { Dir = string.Empty, Filename = "bg_sev_all_p" };
                case (int)LevelId.Silo: return new FileMap { Dir = string.Empty, Filename = "bg_silo_all_p" };
                case (int)LevelId.Frigate: return new FileMap { Dir = string.Empty, Filename = "bg_dest_all_p" };
                case (int)LevelId.Surface2: return new FileMap { Dir = string.Empty, Filename = "bg_sevx_all_p" };
                case (int)LevelId.Bunker2: return new FileMap { Dir = string.Empty, Filename = "bg_sevb_all_p" };
                case (int)LevelId.Statue: return new FileMap { Dir = string.Empty, Filename = "bg_stat_all_p" };
                case (int)LevelId.Archives: return new FileMap { Dir = string.Empty, Filename = "bg_arch_all_p" };
                case (int)LevelId.Streets:
                {
                    switch (version)
                    {
                        case Enums.Version.Ntsc: return new FileMap { Dir = string.Empty, Filename = "bg_pete_all_p" };
                        case Enums.Version.Pal: return new FileMap { Dir = "e", Filename = "bg_pete_all_p" };
                        case Enums.Version.NtscJ: return new FileMap { Dir = string.Empty, Filename = "bg_pete_all_p" };
                    }
                }

                break;

                case (int)LevelId.Depot: return new FileMap { Dir = string.Empty, Filename = "bg_depo_all_p" };
                case (int)LevelId.Train: return new FileMap { Dir = string.Empty, Filename = "bg_tra_all_p" };
                case (int)LevelId.Jungle:
                {
                    switch (version)
                    {
                        case Enums.Version.Ntsc: return new FileMap { Dir = string.Empty, Filename = "bg_jun_all_p" };
                        case Enums.Version.Pal: return new FileMap { Dir = "e", Filename = "bg_jun_all_p" };
                        case Enums.Version.NtscJ: return new FileMap { Dir = string.Empty, Filename = "bg_jun_all_p" };
                    }
                }

                break;

                case (int)LevelId.Control: return new FileMap { Dir = string.Empty, Filename = "bg_arec_all_p" };
                case (int)LevelId.Caverns: return new FileMap { Dir = string.Empty, Filename = "bg_cave_all_p" };
                case (int)LevelId.Cradle: return new FileMap { Dir = string.Empty, Filename = "bg_crad_all_p" };
                case (int)LevelId.Aztec: return new FileMap { Dir = string.Empty, Filename = "bg_azt_all_p" };
                case (int)LevelId.Egypt: return new FileMap { Dir = string.Empty, Filename = "bg_cryp_all_p" };
                case (int)LevelId.Cuba: return new FileMap { Dir = string.Empty, Filename = "bg_len_all_p" };
            }

            throw new NotImplementedException();
        }
    }
}
