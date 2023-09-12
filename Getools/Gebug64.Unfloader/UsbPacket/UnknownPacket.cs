using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Gebug64.Unfloader.UsbPacket
{
    public class UnknownPacket : Packet
    {
        public UnknownPacket(byte[] data)
            : base(PacketType.Unknown, data)
        { }
    }
}
