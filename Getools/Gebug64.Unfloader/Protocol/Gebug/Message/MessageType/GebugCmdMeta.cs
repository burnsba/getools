using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Gebug.Message.MessageType
{
    /// <summary>
    /// Gebug meta commands.
    /// </summary>
    public enum GebugCmdMeta
    {
        /// <summary>
        /// Unset / unknown.
        /// </summary>
        DefaultUnknown = 0,

        /// <summary>
        /// Configure printf echo on gebug message received.
        /// </summary>
        ConfigEcho = 1,

        /// <summary>
        /// Simple ping command.
        /// </summary>
        Ping = 2,

        /// <summary>
        /// Version information about the ROM running on console.
        /// </summary>
        Version = 10,
    }
}
