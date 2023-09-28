using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Unfloader.Protocol.Gebug.Parameter;

namespace Gebug64.Unfloader.Protocol.Gebug
{
    public interface IGebugMessage
    {
        GebugMessageCategory Category { get; }
        int Command { get; }

        ushort MessageId { get; }
        ushort AckId { get; }

        DateTime InstantiateTime { get; }

        CommunicationSource Source { get; }

        GebugPacket? FirstPacket { get; }

        List<GebugPacket> ToSendPackets(ParameterUseDirection sendDirection);
    }
}
