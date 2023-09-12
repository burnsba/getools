using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game
{
    public class Coord2dshort
    {
        /// <summary>
        /// Size of the struct in bytes.
        /// </summary>
        public const int SizeOf = 4;

        /// <summary>
        /// Gets or sets x position.
        /// </summary>
        public short X { get; set; }

        /// <summary>
        /// Gets or sets y position.
        /// </summary>
        public short Y { get; set; }
    }
}
