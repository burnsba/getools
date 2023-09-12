using Getools.Lib.Game.Asset.Bg;
using Getools.Lib.Game.Asset.Setup;
using Getools.Lib.Game.Asset.Stan;
using Getools.Lib.Kaitai.Gen;

namespace Getools.Palantir
{
    /// <summary>
    /// Container class for csharp object game data.
    /// </summary>
    public class Stage
    {
        /// <summary>
        /// Gets or sets the stage level scale.
        /// </summary>
        public double LevelScale { get; set; }

        /// <summary>
        /// Gets or sets the bg data.
        /// </summary>
        public BgFile? Bg { get; set; }

        /// <summary>
        /// Gets or sets the setup data.
        /// </summary>
        public StageSetupFile? Setup { get; set; }

        /// <summary>
        /// Gets or sets the stan data.
        /// </summary>
        public StandFile? Stan { get; set; }
    }
}