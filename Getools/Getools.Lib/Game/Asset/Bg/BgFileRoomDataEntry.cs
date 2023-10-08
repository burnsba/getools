using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.BinPack;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Bg
{
    /// <summary>
    /// Entry in BG room section.
    /// </summary>
    public class BgFileRoomDataEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BgFileRoomDataEntry"/> class.
        /// </summary>
        public BgFileRoomDataEntry()
        {
        }

        /// <summary>
        /// Pointer to table.
        /// </summary>
        public PointerVariable? PointTablePointer { get; set; }

        /// <summary>
        /// List of points in the room.
        /// </summary>
        public List<GbiVtx> Points { get; set; } = new List<GbiVtx>();

        /// <summary>
        /// Primary display list pointer.
        /// </summary>
        public PointerVariable? PrimaryDisplayList { get; set; }

        /// <summary>
        /// Secondary display list pointer.
        /// </summary>
        public PointerVariable? SecondaryDisplayList { get; set; }

        /// <summary>
        /// Center coordinate.
        /// </summary>
        public Coord3df? Coord { get; set; }

        /// <summary>
        /// When loading a binary file, this will be the index of the roomdata seen so far (0,1,2,...).
        /// </summary>
        public int OrderIndex { get; set; }
    }
}
