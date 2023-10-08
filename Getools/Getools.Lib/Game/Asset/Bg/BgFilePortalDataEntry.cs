using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.BinPack;

namespace Getools.Lib.Game.Asset.Bg
{
    /// <summary>
    /// Entry in BG portal section.
    /// </summary>
    public class BgFilePortalDataEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BgFilePortalDataEntry"/> class.
        /// </summary>
        public BgFilePortalDataEntry()
        {
        }

        /// <summary>
        /// Pointer to portal
        /// </summary>
        public PointerVariable? PortalPointer { get; set; }

        /// <summary>
        /// Portal.
        /// </summary>
        public BgFilePortal? Portal { get; set; }

        /// <summary>
        /// Room 1 id.
        /// </summary>
        public byte ConnectedRoom1 { get; set; }

        /// <summary>
        /// Room 2 id.
        /// </summary>
        public byte ConnectedRoom2 { get; set; }

        /// <summary>
        /// Flags.
        /// </summary>
        public ushort ControlFlags { get; set; }

        /// <summary>
        /// When loading a binary file, this will be the index of the portal seen so far (0,1,2,...).
        /// </summary>
        public int OrderIndex { get; set; }
    }
}
