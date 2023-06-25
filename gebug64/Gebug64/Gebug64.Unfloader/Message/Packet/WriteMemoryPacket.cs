using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Message.Packet
{
    public class WriteMemoryPacket : IPacket
    {
        public string PacketDescription => "write memory";

        public uint Address { get; set; }

        public int Length { get; set; }

        public string GetAdditionalDescription()
        {
            return $"0x{Address.ToString("x")} : {Length}";
        }

        public string ToProtocolPacket() => throw new InvalidOperationException("packet cannot be sent to console");
    }
}
