using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;

namespace Gebug64.Unfloader.Protocol.Gebug.Message
{
    [ProtocolCommand(Category = GebugMessageCategory.Meta, Command = (byte)GebugCmdMeta.Ping)]
    public class GebugMetaPingMessage : GebugMessage, IActivatorGebugMessage
    {
        public GebugMetaPingMessage()
          : base(GebugMessageCategory.Meta)
        {
            Command = (int)GebugCmdMeta.Ping;
        }
    }
}
