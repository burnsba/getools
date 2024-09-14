using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.Enum
{
    /// <summary>
    /// How to display memory watch data.
    /// </summary>
    public enum MemoryDisplayFormat
    {
        /// <summary>
        /// Unset / unknown.
        /// </summary>
        DefaultUnknown = 0,

        /// <summary>
        /// Show as decimal, no prefix.
        /// </summary>
        Decimal,

        /// <summary>
        /// Show as hex, leading "0x" refix.
        /// </summary>
        Hex,

        /// <summary>
        /// Floating point number with two digits after decimal.
        /// </summary>
        Float0_00,

        /// <summary>
        /// Floating point number with four digits after decimal.
        /// </summary>
        Float0_0000,

        /// <summary>
        /// Floating point number, convert to string with default digits after decimal.
        /// </summary>
        Float0_x,
    }
}
