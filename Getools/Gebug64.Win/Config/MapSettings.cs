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
    }
}
