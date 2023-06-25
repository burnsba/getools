using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Message.Packet
{
    public class ScreenshotRequestPacket : IPacket
    {
        public string PacketDescription => "screenshot";

        public string GetAdditionalDescription() => string.Empty;

        public string ToProtocolPacket() => throw new InvalidOperationException("packet cannot be sent to console");
    }
}
