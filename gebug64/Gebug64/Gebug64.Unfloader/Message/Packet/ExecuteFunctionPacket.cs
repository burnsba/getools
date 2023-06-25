using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Message.Packet
{
    public class ExecuteFunctionPacket : IPacket
    {
        public string PacketDescription => "execute function";

        public uint Address { get; set; }

        public int NumberArgs { get; set; }

        public List<IExecuteFunctionArg> Args { get; set; } = new List<IExecuteFunctionArg>();

        public string GetAdditionalDescription() => string.Empty;

        public string ToProtocolPacket() => throw new InvalidOperationException("packet cannot be sent to console");
    }
}
