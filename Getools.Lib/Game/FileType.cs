using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game
{
    public enum FileType
    {
        /// <summary>
        /// Unset / unknown.
        /// </summary>
        DefaultUnknown,

        /// <summary>
        /// Text .c file.
        /// </summary>
        C,

        /// <summary>
        /// Binary file.
        /// </summary>
        Bin,

        /// <summary>
        /// Text JSON file.
        /// </summary>
        Json,
    }
}
