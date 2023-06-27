using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Bg
{
    /// <summary>
    /// This object cooresponds to an entire bg file.
    /// </summary>
    public class BgFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BgFile"/> class.
        /// </summary>
        public BgFile()
        {
        }

        /// <summary>
        /// Header.
        /// </summary>
        public BgFileHeader Header { get; set; }

        public BgFileRoomDataTable RoomDataTable { get; set; }

        public BgFileVisibilityCommandTable GlobalVisibilityCommands { get; set; }

        public BgFilePortalDataTable PortalDataTable { get; set; }

        //public List<BgFilePortal> Portals { get; set; }

        //public List<BgFilePointTableEntry> BgFilePointTable { get; set; }

        ///// <summary>
        ///// Contains entries for both primary and secondary.
        ///// </summary>
        //public List<BgFileRoomDisplayListEntry> DisplayListTable { get; set; }
    }
}
