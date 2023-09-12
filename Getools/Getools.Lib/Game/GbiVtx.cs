using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game
{
    public class GbiVtx
    {
        public Coord3dshort Ob { get; set; }

        // unsigned short
        public ushort Flag { get; set; }

        // short
        public Coord2dshort TextureCoordinate { get; set; }

        // signed char
        public Coord3dsbyte Normal { get; set; }

        // unsigned char
        public byte Alpha { get; set; }
    }
}
