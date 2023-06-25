using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Message.Packet
{
    public interface IPacket
    {
        string PacketDescription { get; }

        string GetAdditionalDescription();

        string ToProtocolPacket();
    }
}
