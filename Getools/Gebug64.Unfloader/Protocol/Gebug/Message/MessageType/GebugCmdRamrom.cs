using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Gebug.Message.MessageType
{
    /// <summary>
    /// Native demo/replay methods.
    /// </summary>
    public enum GebugCmdRamrom
    {
        /// <summary>
        /// Unset / unknown.
        /// </summary>
        DefaultUnknown = 0,

        /// <summary>
        /// Packet contains `struct ramromfilestructure` header data, load and start replay.
        /// </summary>
        StartDemoReplayFromPc = 10,

        /// <summary>
        /// Demo replay from PC has been started. Request next iteration block.
        /// </summary>
        ReplayRequestNextIteration = 12,

        /// <summary>
        /// Start demo replay from native `ramrom_table` entry.
        /// </summary>
        ReplayNativeDemo = 30,
    }
}
