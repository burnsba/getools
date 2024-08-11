using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Gebug.Message.MessageType
{
    /// <summary>
    /// Gebug methods for Bond.
    /// </summary>
    public enum GebugCmdBond
    {
        /// <summary>
        /// Unset / unknown.
        /// </summary>
        DefaultUnknown = 0,

        /// <summary>
        /// Sends Bond position from console to pc.
        /// </summary>
        SendPosition = 14,

        /// <summary>
        /// Teleport Bond to position.
        /// </summary>
        TeleportToPosition = 17,
    }
}
