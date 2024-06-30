using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Palantir
{
    /// <summary>
    /// Describes output image format.
    /// </summary>
    public enum OutputImageFormat
    {
        /// <summary>
        /// Default / unknown value.
        /// </summary>
        DefaultUnknown,

        /// <summary>
        /// SVG format (uncompressed).
        /// </summary>
        Svg,

        /// <summary>
        /// Output is to be consumed by another part of the application.
        /// </summary>
        CsharpRaw,
    }
}
