using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GzipSharpLib
{
    /// <remarks>
    /// gzip.h
    /// </remarks>
    internal enum CompressionMethod
    {
        Stored = 0,
        Compressed = 1,
        Packed = 2,
        Lzhed = 3,
        /* methods 4-7 reserved */
        Deflated = 8,

        //MaxMethods = 9,

        Rare1172Deflated = 10,

        DefaultUnknown = 253,
        Error = 254,
        PrintTotals = 255,
    }
}
