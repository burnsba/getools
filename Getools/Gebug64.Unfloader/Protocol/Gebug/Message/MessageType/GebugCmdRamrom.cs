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
        /// Only header data is sent, each iteration is streamed from PC.
        /// </summary>
        StartDemoReplayFromPc = 10,

        /// <summary>
        /// Demo replay from PC has been started. Request next iteration block.
        /// </summary>
        ReplayRequestNextIteration = 12,

        /// <summary>
        /// Transfer replay to expansion pak memory, then start ramrom replay.
        /// PC should first check if xpak is installed.
        /// </summary>
        ReplayFromExpansionPak = 13,

        /// <summary>
        /// Start demo replay from native `ramrom_table` entry.
        /// </summary>
        ReplayNativeDemo = 30,
    }
}
