using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;

namespace Gebug64.Unfloader.Protocol.Gebug.Message
{
    public class GebugMetaPingMessage : GebugMessage
    {
        public GebugMetaPingMessage()
          : base(GebugMessageCategory.Meta)
        {
            Command = (int)GebugCmdMeta.Ping;
        }
    }
}
