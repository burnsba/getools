using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game
{
    public class Coord3dsbyte
    {
        /// <summary>
        /// Size of the struct in bytes.
        /// </summary>
        public const int SizeOf = 3;

        /// <summary>
        /// Gets or sets x position.
        /// </summary>
        public sbyte X { get; set; }

        /// <summary>
        /// Gets or sets y position.
        /// </summary>
        public sbyte Y { get; set; }

        /// <summary>
        /// Gets or sets z position.
        /// </summary>
        public sbyte Z { get; set; }
    }
}
