using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Message.Packet;

namespace Gebug64.Unfloader.Message
{
    public interface IGebugMessage
    {
        Guid Id { get; }

        CommunicationSource Source { get; }

        IPacket Packet { get; }

        IPacket? OriginationPacket { get; }

        string MessageDescription { get; }

        string GetFriendlyLogText();
    }
}
