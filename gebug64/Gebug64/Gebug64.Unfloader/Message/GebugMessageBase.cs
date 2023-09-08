using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.UsbPacket;

namespace Gebug64.Unfloader.Message
{
    public abstract class GebugMessageBase : IGebugMessage
    {
        private readonly Guid _id = Guid.NewGuid();

        public Guid Id => _id;

        public CommunicationSource Source { get; set; }

        public Packet UsbPacket { get; set; }

        public DateTime InstantiateTime { get; set; }
    }
}
