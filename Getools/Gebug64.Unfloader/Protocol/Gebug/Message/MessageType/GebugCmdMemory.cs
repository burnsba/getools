using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Gebug.Message.MessageType
{
    /// <summary>
    /// Memory related methods.
    /// </summary>
    public enum GebugCmdMemory
    {
        /// <summary>
        /// Unset / unknown.
        /// </summary>
        DefaultUnknown = 0,

        /// <summary>
        /// PC send request to console to add a new memory watch.
        /// </summary>
        AddWatch = 14,

        /// <summary>
        /// Console send bulk read of all current memory watches to PC.
        /// </summary>
        WatchBulkRead = 15,

        /// <summary>
        /// PC send request to console to remove an existing memory watch.
        /// </summary>
        RemoveWatch = 16,
    }
}
