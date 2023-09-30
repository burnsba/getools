using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;

namespace Gebug64.Unfloader.Protocol.Gebug.Message
{
    /// <summary>
    /// Ping.
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Meta, Command = (byte)GebugCmdMeta.Ping)]
    public class GebugMetaPingMessage : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugMetaPingMessage"/> class.
        /// </summary>
        public GebugMetaPingMessage()
          : base(GebugMessageCategory.Meta)
        {
            Command = (int)GebugCmdMeta.Ping;
        }
    }
}
