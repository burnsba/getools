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
        public BgFileHeader? Header { get; set; }

        /// <summary>
        /// Room section.
        /// </summary>
        public BgFileRoomDataTable? RoomDataTable { get; set; }

        /// <summary>
        /// Visibility commands section.
        /// </summary>
        public BgFileVisibilityCommandTable? GlobalVisibilityCommands { get; set; }

        /// <summary>
        /// Portal section.
        /// </summary>
        public BgFilePortalDataTable? PortalDataTable { get; set; }
    }
}
