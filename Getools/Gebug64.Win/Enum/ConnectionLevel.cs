using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.Enum
{
    /// <summary>
    /// Current connection level.
    /// </summary>
    public enum ConnectionLevel
    {
        /// <summary>
        /// Not connected.
        /// </summary>
        NotConnected,

        /// <summary>
        /// Connected, but only to flashcart.
        /// </summary>
        Flashcart,

        /// <summary>
        /// Connected, currently running gebug romhack.
        /// </summary>
        Rom,
    }
}
