using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Gebug.Message.MessageType
{
    /// <summary>
    /// Gebug miscellaneous commands.
    /// </summary>
    public enum GebugCmdMisc
    {
        /// <summary>
        /// Unset / unknown.
        /// </summary>
        DefaultUnknown = 0,

        /// <summary>
        /// Requests current ROM OS time.
        /// </summary>
        OsTime = 1,

        /// <summary>
        /// Requests current ROM OS memory size in bytes.
        /// </summary>
        OsMemSize = 2,

        /// <summary>
        /// Set grenade roll chance in ROM.
        /// </summary>
        SetGrenadeChance = 3,
    }
}
