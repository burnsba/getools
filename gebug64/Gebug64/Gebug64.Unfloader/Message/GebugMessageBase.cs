using Gebug64.Unfloader.Message.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Message
{
    public abstract class GebugMessageBase : IGebugMessage
    {
        private readonly Guid _id = Guid.NewGuid();

        public Guid Id => _id;

        public CommunicationSource Source { get; private set; }

        public IPacket Packet { get; protected set; }

        public IPacket? OriginationPacket { get; protected set; }

        public string MessageDescription { get; protected set; }

        public string GetFriendlyLogText()
        {
            string addtional = Packet.GetAdditionalDescription() ?? string.Empty;

            var sb = new StringBuilder();
            sb.Append($"Received [{MessageDescription}]: [{Packet.PacketDescription}]");

            if (!string.IsNullOrEmpty(addtional))
            {
                sb.Append(" ");
                sb.Append(addtional);
            }

            return sb.ToString();
        }
    }
}
