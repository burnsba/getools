using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Gebug64.Unfloader.UsbPacket
{
    public class RawPacket : PacketBase
    {
        public RawPacket(byte[] data)
            : base(PacketType.Binary, data)
        { }
    }
}
