using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.Config
{
    /// <summary>
    /// Configuration settings related to the map.
    /// </summary>
    public class MapSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapSettings"/> class.
        /// </summary>
        public MapSettings()
        {
            ShowMapLayer = System.Enum.GetValues<Enum.UiMapLayer>()
                .Where(x => x > Enum.UiMapLayer.DefaultUnknown)
                .OrderBy(x => x)
                .ToDictionary(key => key, val => true);
        }

        /// <summary>
        /// Folder containing setup files.
        /// </summary>
        public string? SetupBinFolder { get; set; }

        /// <summary>
        /// Folder containing stan files.
        /// </summary>
        public string? StanBinFolder { get; set; }

        /// <summary>
        /// Folder containing bg files.
        /// </summary>
        public string? BgBinFolder { get; set; }

        /// <summary>
        /// Flags to show or hide map layer.
        /// </summary>
        public Dictionary<Enum.UiMapLayer, bool> ShowMapLayer { get; set; }

        /// <summary>
        /// Whether stage should chnage automatically, or user should manually change the stage.
        /// </summary>
        public bool AutoLoadLevel { get; set; }

        /// <summary>
        /// Whether the map should automatically scroll to Bond's position.
        /// </summary>
        public bool FollowBond { get; set; }
    }
}
