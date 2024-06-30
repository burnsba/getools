using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset
{
    public class StageScale
    {
        public const double Dam = 0.23363999;
        public const double Facility = 1.20648;
        public const double Runway = 0.089571431;
        public const double Surface1 = 0.45445713;
        public const double Bunker1 = 0.53931433;
        public const double Silo = 0.47256002;
        public const double Frigate = 0.44757429;
        public const double Surface2 = 0.45445713;
        public const double Bunker2 = 0.53931433;
        public const double Statue = 0.107202865;
        public const double Archives = 0.50678575;
        public const double Streets = 0.34187999;
        public const double Depot = 0.21847887;
        public const double Train = 0.15019713;
        public const double Jungle = 0.094662853;
        public const double Control = 0.49886572;
        public const double Caverns = 0.26824287;
        public const double Cradle = 0.23571429;
        public const double Aztec = 0.35300568;
        public const double Egypt = 0.25608;
        public const double Cuba = 0.094662853;

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
