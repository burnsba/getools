using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.UsbPacket;

namespace Gebug64.Unfloader.Message
{
    public interface IGebugMessage
    {
        Guid Id { get; }

        DateTime InstantiateTime { get; }

        CommunicationSource Source { get; }

        Packet GetUsbPacket();
    }
}
