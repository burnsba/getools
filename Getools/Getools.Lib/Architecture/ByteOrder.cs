using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Architecture
{
    public enum ByteOrder
    {
        SystemNative,

        /// <summary>
        /// MIPS (N64).
        /// </summary>
        BigEndien,

        /// <summary>
        /// Intel, ARM.
        /// </summary>
        LittleEndien,
    }
}
