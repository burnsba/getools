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

        /// <summary>
        /// Sends all character spawn positions for this tick from console to pc.
        /// </summary>
        NotifyChrSpawn = 14,

        /// <summary>
        /// Remove all body armor and all but 0.01 HP from guard.
        /// </summary>
        GhostHp = 21,

        /// <summary>
        /// Set guard HP to zero. This removes any current body armor.
        /// </summary>
        MaxHp = 22,
    }
}
