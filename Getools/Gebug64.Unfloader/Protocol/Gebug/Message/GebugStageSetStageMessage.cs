using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Unfloader.Protocol.Gebug.Parameter;

namespace Gebug64.Unfloader.Protocol.Gebug.Message
{
    [ProtocolCommand(Category = GebugMessageCategory.Stage, Command = (byte)GebugCmdStage.SetStage)]
    public class GebugStageSetStageMessage : GebugMessage, IActivatorGebugMessage
    {
        [GebugParameter(ParameterIndex = 0, Size = 1, UseDirection = ParameterUseDirection.PcToConsole)]
        public byte LevelId { get; set; }

        public GebugStageSetStageMessage()
          : base(GebugMessageCategory.Stage)
        {
            Command = (int)GebugCmdStage.SetStage;
        }
    }
}
