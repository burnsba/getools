using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Gebug.Message.MessageType
{
    /// <summary>
    /// Native cheat methods.
    /// </summary>
    public enum GebugCmdCheat
    {
        /// <summary>
        /// Unset / unknown.
        /// </summary>
        DefaultUnknown = 0,

        /// <summary>
        /// Turns a cheat on or off.
        /// </summary>
        SetCheatStatus = 10,

        /// <summary>
        /// Disables all runtime cheats.
        /// </summary>
        DisableAll = 12,
    }
}
