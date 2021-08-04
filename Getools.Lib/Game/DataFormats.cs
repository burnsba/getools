using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game
{
    public enum DataFormats
    {
        /// <summary>
        /// Unset / unknown.
        /// </summary>
        DefaultUnknown,

        /// <summary>
        /// Text .c file, using current struct definitions.
        /// </summary>
        C,

        /// <summary>
        /// Text .c file, using beta struct definitions.
        /// </summary>
        BetaC,

        /// <summary>
        /// Binary file, using current struct definitions.
        /// </summary>
        Bin,

        /// <summary>
        /// Binary file, using beta struct definitions.
        /// </summary>
        BetaBin,

        /// <summary>
        /// Text JSON file, using current struct definitions.
        /// </summary>
        Json,

        /// <summary>
        /// Text JSON file, using beta struct definitions.
        /// </summary>
        BetaJson,
    }
}
