using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;

namespace Gebug64.Unfloader.Protocol.Gebug
{
    public class GebugPacket
    {
        public const int MaxPacketSize = 480;

        public GebugMessageCategory Category { get; set; }
        public int Command { get; set; }
        public int Flags { get; set; }
        public int Size { get; set; }
        public int PacketNumber { get; set; }
        public int TotalNumberPackets { get; set; }
        public byte[] Body { get; set; }

        public byte[] ToByteArray()
        {
            return new byte[1];
        }

        public static GebugPacketParseResult TryParse(List<byte> data)
        {
            return new GebugPacketParseResult();
        }
    }
}
