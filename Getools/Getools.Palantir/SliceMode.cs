using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Palantir
{
    /// <summary>
    /// How the stage data should be filtered.
    /// </summary>
    internal enum SliceMode
    {
        /// <summary>
        /// Take only objects that intersect at this Y value.
        /// </summary>
        Slice,

        /// <summary>
        /// Take objects that are between a min and max Y value.
        /// </summary>
        BoundingBox,

        /// <summary>
        /// Take all objects.
        /// </summary>
        Unbound,
    }
}
