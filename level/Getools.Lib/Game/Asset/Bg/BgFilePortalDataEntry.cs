using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.BinPack;

namespace Getools.Lib.Game.Asset.Bg
{
    public class BgFilePortalDataEntry
    {
        public PointerVariable PortalPointer { get; set; }

        public BgFilePortal Portal { get; set; }

        public byte ConnectedRoom1 { get; set; }

        public byte ConnectedRoom2 { get; set; }

        public ushort ControlFlags { get; set; }

        /// <summary>
        /// When loading a binary file, this will be the index of the portal seen so far (0,1,2,...).
        /// </summary>
        public int OrderIndex { get; set; }
    }
}
