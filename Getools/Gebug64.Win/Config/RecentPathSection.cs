using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.Config
{
    /// <summary>
    /// Configuration section about recent file/folder paths.
    /// </summary>
    public class RecentPathSection
    {
        /// <summary>
        /// The most recently used folder location when sending a ROM to flashcart.
        /// </summary>
        public string? SendRomFolder { get; set; }

        /// <summary>
        /// The most recently used folder location when selecting a ramrom replay.
        /// </summary>
        public string? RamromPcReplayFolder { get; set; }

        /// <summary>
        /// List of most recently sent ROMs.
        /// </summary>
        public List<string> RecentSendRom { get; set; } = new List<string>();
    }
}
