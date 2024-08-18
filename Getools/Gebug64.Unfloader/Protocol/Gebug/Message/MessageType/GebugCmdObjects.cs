using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Gebug.Message.MessageType
{
    /// <summary>
    /// Gebug methods for setup objects.
    /// </summary>
    public enum GebugCmdObjects
    {
        /// <summary>
        /// Unset / unknown.
        /// </summary>
        DefaultUnknown = 0,

        /// <summary>
        /// Sends notification from console to pc for all explosions created this tick.
        /// </summary>
        NotifyExplosionCreate = 4,
    }
}
