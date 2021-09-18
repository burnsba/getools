using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.BinPack
{
    /// <summary>
    /// Small enum to track where binary data should be organized.
    /// </summary>
    public enum MipsElfSection
    {
        /// <summary>
        /// Default / unknown.
        /// </summary>
        DefaultUnknown,

        /// <summary>
        /// The .data section.
        /// </summary>
        Data,

        /// <summary>
        /// The .rodata section.
        /// </summary>
        Rodata,
    }
}
