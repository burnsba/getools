using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Parse;

namespace Gebug64.Unfloader.Protocol.Gebug
{
    /// <summary>
    /// Parse result context.
    /// </summary>
    public class GebugPacketParseResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugPacketParseResult"/> class.
        /// </summary>
        public GebugPacketParseResult()
        {
        }

        /// <summary>
        /// Gets or sets the associated packet. Null if <see cref="ParseStatus"/> is not <see cref="PacketParseStatus.Success"/>.
        /// </summary>
        public GebugPacket? Packet { get; set; }

        /// <summary>
        /// Gets or sets whether the packet was parsed successfully.
        /// </summary>
        public PacketParseStatus ParseStatus { get; set; }

        /// <summary>
        /// Gets or sets the number of bytes consumed by the packet.
        /// </summary>
        public int TotalBytesRead { get; set; }
    }
}
