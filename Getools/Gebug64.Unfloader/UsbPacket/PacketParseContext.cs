using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.UsbPacket
{
    internal class PacketParseContext
    {
        public int BytePrefix { get; set; }

        public int TotalBytesRead { get; set; }

        public PacketParseResult ParseResult { get; set; } = PacketParseResult.DefaultUnknown;

        public IPacket? Packet { get; set; } = null;
    }
}
