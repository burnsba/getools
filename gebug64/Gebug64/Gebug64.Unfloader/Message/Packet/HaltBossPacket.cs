using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Message.Packet
{
    public class HaltBossPacket : IPacket
    {
        public string PacketDescription => "halt boss";

        public string GetAdditionalDescription() => string.Empty;

        public string ToProtocolPacket() => throw new InvalidOperationException("packet cannot be sent to console");
    }
}
