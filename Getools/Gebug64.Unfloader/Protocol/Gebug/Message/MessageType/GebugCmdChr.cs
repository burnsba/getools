using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Gebug.Message.MessageType
{
    /// <summary>
    /// Gebug methods for guards and characters.
    /// </summary>
    public enum GebugCmdChr
    {
        /// <summary>
        /// Unset / unknown.
        /// </summary>
        DefaultUnknown = 0,

        /// <summary>
        /// Sends all "active" character positions and some light info from console to pc (active means the model is not null).
        /// </summary>
        SendAllGuardInfo = 10,
    }
}
