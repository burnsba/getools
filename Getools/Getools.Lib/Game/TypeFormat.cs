using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game
{
    /// <summary>
    /// Object/struct formats supported by the library.
    /// </summary>
    public enum TypeFormat
    {
        /// <summary>
        /// Unset / unknown.
        /// </summary>
        DefaultUnknown,

        /// <summary>
        /// Release build data format.
        /// </summary>
        Normal,

        /// <summary>
        /// Beta format.
        /// </summary>
        Beta,
    }
}
