using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Gebug
{
    /// <summary>
    /// Gebug message flags.
    /// </summary>
    [Flags]
    public enum GebugMessageFlags
    {
        /// <summary>
        /// Indicates this message is composed of more than one packet.
        /// </summary>
        IsMultiMessage = 0x1,

        /// <summary>
        /// Indicates this message is a reply to a prior message.
        /// </summary>
        IsAck = 0x2,
    }
}
