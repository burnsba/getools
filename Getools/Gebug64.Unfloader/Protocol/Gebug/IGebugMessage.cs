using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;

namespace Gebug64.Unfloader.Protocol.Gebug
{
    public interface IGebugMessage
    {
        GebugMessageCategory Category { get; }
        int Command { get; }

        List<GebugPacket> ToSendPackets();
    }
}
