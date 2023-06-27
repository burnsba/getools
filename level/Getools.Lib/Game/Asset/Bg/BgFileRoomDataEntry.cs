using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.BinPack;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Bg
{
    public class BgFileRoomDataEntry
    {
        public PointerVariable PointTablePointer { get; set; }

        public List<GbiVtx> Points { get; set; }

        public PointerVariable PrimaryDisplayList { get; set; }

        public PointerVariable SecondaryDisplayList { get; set; }

        public Coord3df Coord { get; set; }

        /// <summary>
        /// When loading a binary file, this will be the index of the roomdata seen so far (0,1,2,...).
        /// </summary>
        public int OrderIndex { get; set; }
    }
}
