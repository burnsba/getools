using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Enums
{
    /// <summary>
    /// Credits data block text alignment.
    /// </summary>
    public enum CreditTextAlignment : ushort
    {
        /// <summary>
        /// Right.
        /// </summary>
        Right = 0,

        /// <summary>
        /// Left.
        /// </summary>
        Left = 1,

        /// <summary>
        /// Center.
        /// </summary>
        Center = 2,

        /// <summary>
        /// Use previous value.
        /// </summary>
        Previous = 0xffff,
    }
}
