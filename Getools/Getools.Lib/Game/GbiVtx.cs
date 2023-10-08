using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game
{
    /// <summary>
    /// LibUltra `Vtx_tn` (set up for use with normals).
    /// </summary>
    public class GbiVtx
    {
        /// <summary>
        /// x, y, z.
        /// </summary>
        public Coord3dshort? Ob { get; set; }

        /// <summary>
        /// Flag.
        /// </summary>
        public ushort Flag { get; set; }

        /// <summary>
        /// Texture coordinate.
        /// </summary>
        public Coord2dshort? TextureCoordinate { get; set; }

        /// <summary>
        /// Normal.
        /// </summary>
        public Coord3dsbyte? Normal { get; set; }

        /// <summary>
        /// Alpha.
        /// </summary>
        public byte Alpha { get; set; }
    }
}
