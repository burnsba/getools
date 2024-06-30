using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Palantir.Enums
{
    /// <summary>
    /// Describes what a base polygon is capturing.
    /// </summary>
    public enum PolygonSource
    {
        /// <summary>
        /// Default.
        /// </summary>
        DefaultUnknown = 0,

        /// <summary>
        /// BG.
        /// </summary>
        Bg,

        /// <summary>
        /// Tile.
        /// </summary>
        Stan,

        /// <summary>
        /// Object / char.
        /// </summary>
        GameObject,
    }
}
