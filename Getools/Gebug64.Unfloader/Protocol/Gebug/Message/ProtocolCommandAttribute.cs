using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;

namespace Gebug64.Unfloader.Protocol.Gebug.Message
{
    internal class ProtocolCommandAttribute : Attribute
    {
        public GebugMessageCategory Category { get; set; }
        public byte Command { get; set; }
    }
}
