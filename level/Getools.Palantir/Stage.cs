using Getools.Lib.Game.Asset.Bg;
using Getools.Lib.Game.Asset.Setup;
using Getools.Lib.Game.Asset.Stan;
using Getools.Lib.Kaitai.Gen;

namespace Getools.Palantir
{
    public class Stage
    {
        public double LevelScale { get; set; }

        public BgFile Bg { get; set; }
        
        public StageSetupFile Setup { get; set; }

        public StandFile Stan { get; set; }
    }
}