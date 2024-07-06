using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset
{
    /// <summary>
    /// Stage sscale for retail version of the game.
    /// </summary>
    public class StageScale
    {
        /// <summary>
        /// Dam stage scale.
        /// </summary>
        public const double Dam = 0.23363999;

        /// <summary>
        /// Facility stage scale.
        /// </summary>
        public const double Facility = 1.20648;

        /// <summary>
        /// Runway stage scale.
        /// </summary>
        public const double Runway = 0.089571431;

        /// <summary>
        /// Surface 1 stage scale.
        /// </summary>
        public const double Surface1 = 0.45445713;

        /// <summary>
        /// Bunker 1 stage scale.
        /// </summary>
        public const double Bunker1 = 0.53931433;

        /// <summary>
        /// Silo stage scale.
        /// </summary>
        public const double Silo = 0.47256002;

        /// <summary>
        /// Frigate stage scale.
        /// </summary>
        public const double Frigate = 0.44757429;

        /// <summary>
        /// Surface 2 stage scale.
        /// </summary>
        public const double Surface2 = 0.45445713;

        /// <summary>
        /// Bunker 2 stage scale.
        /// </summary>
        public const double Bunker2 = 0.53931433;

        /// <summary>
        /// Statue stage scale.
        /// </summary>
        public const double Statue = 0.107202865;

        /// <summary>
        /// Archives stage scale.
        /// </summary>
        public const double Archives = 0.50678575;

        /// <summary>
        /// Streets stage scale.
        /// </summary>
        public const double Streets = 0.34187999;

        /// <summary>
        /// Depot stage scale.
        /// </summary>
        public const double Depot = 0.21847887;

        /// <summary>
        /// Train stage scale.
        /// </summary>
        public const double Train = 0.15019713;

        /// <summary>
        /// Jungle stage scale.
        /// </summary>
        public const double Jungle = 0.094662853;

        /// <summary>
        /// Control stage scale.
        /// </summary>
        public const double Control = 0.49886572;

        /// <summary>
        /// Caverns stage scale.
        /// </summary>
        public const double Caverns = 0.26824287;

        /// <summary>
        /// Cradle stage scale.
        /// </summary>
        public const double Cradle = 0.23571429;

        /// <summary>
        /// Aztec stage scale.
        /// </summary>
        public const double Aztec = 0.35300568;

        /// <summary>
        /// Egypt stage scale.
        /// </summary>
        public const double Egypt = 0.25608;

        /// <summary>
        /// Cuba stage scale.
        /// </summary>
        public const double Cuba = 0.094662853;

        /// <summary>
        /// Gets the stage scale for a level.
        /// </summary>
        /// <param name="levelId">Level.</param>
        /// <returns>Stage scale.</returns>
        /// <exception cref="NotImplementedException">Thrown on invalid level id.</exception>
        public static double GetStageScale(int levelId)
        {
            switch (levelId)
            {
                case (int)LevelId.Dam: return Dam;
                case (int)LevelId.Facility: return Facility;
                case (int)LevelId.Runway: return Runway;
                case (int)LevelId.Surface: return Surface1;
                case (int)LevelId.Bunker1: return Bunker1;
                case (int)LevelId.Silo: return Silo;
                case (int)LevelId.Frigate: return Frigate;
                case (int)LevelId.Surface2: return Surface2;
                case (int)LevelId.Bunker2: return Bunker2;
                case (int)LevelId.Statue: return Statue;
                case (int)LevelId.Archives: return Archives;
                case (int)LevelId.Streets: return Streets;
                case (int)LevelId.Depot: return Depot;
                case (int)LevelId.Train: return Train;
                case (int)LevelId.Jungle: return Jungle;
                case (int)LevelId.Control: return Control;
                case (int)LevelId.Caverns: return Caverns;
                case (int)LevelId.Cradle: return Cradle;
                case (int)LevelId.Aztec: return Aztec;
                case (int)LevelId.Egypt: return Egypt;
                case (int)LevelId.Cuba: return Cuba;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
