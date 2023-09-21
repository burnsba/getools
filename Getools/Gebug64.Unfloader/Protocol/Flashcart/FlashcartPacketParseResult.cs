using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Parse;

namespace Gebug64.Unfloader.Protocol.Flashcart
{
    public class FlashcartPacketParseResult
    {
        public IFlashcartPacket? Packet { get; set; }
        public PacketParseStatus ParseStatus { get; set; }
        public PacketParseReason ErrorReason { get; set; }
        public int TotalBytesRead { get; set; }

        public FlashcartPacketParseResult()
        { }
    }
}
