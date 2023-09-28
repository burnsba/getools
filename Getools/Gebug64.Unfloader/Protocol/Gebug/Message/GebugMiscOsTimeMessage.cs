using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Unfloader.Protocol.Gebug.Parameter;

namespace Gebug64.Unfloader.Protocol.Gebug.Message
{
    [ProtocolCommand(Category = GebugMessageCategory.Misc, Command = (byte)GebugCmdMisc.OsTime)]
    public class GebugMiscOsTimeMessage : GebugMessage, IActivatorGebugMessage
    {
        [GebugParameter(ParameterIndex = 0, Size = 4, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public int Count { get; set; }

        public GebugMiscOsTimeMessage()
          : base(GebugMessageCategory.Misc)
        {
            Command = (int)GebugCmdMisc.OsTime;
        }
    }
}
