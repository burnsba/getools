using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader
{
    /// <summary>
    /// Describes where message originated.
    /// </summary>
    public enum CommunicationSource
    {
        /// <summary>
        /// Communication from PC.
        /// </summary>
        Pc,

        /// <summary>
        /// Communication from N64.
        /// </summary>
        N64,
    }
}
