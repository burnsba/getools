using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Unfloader.Protocol.Gebug.Parameter;

namespace Gebug64.Unfloader.Protocol.Gebug.Message
{
    [ProtocolCommand(Category = GebugMessageCategory.Cheat, Command = (byte)GebugCmdCheat.SetCheatStatus)]
    public class GebugCheatSetCheatStatusMessage : GebugMessage, IActivatorGebugMessage
    {
        [GebugParameter(ParameterIndex = 0, Size = 1, UseDirection = ParameterUseDirection.PcToConsole)]
        public byte Enable { get; set; }

        [GebugParameter(ParameterIndex = 1, Size = 1, UseDirection = ParameterUseDirection.PcToConsole)]
        public byte CheatId { get; set; }

        public GebugCheatSetCheatStatusMessage()
          : base(GebugMessageCategory.Cheat)
        {
            Command = (int)GebugCmdCheat.SetCheatStatus;
        }
    }
}
