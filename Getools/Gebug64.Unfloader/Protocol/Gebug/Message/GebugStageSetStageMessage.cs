using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Unfloader.Protocol.Gebug.Parameter;

namespace Gebug64.Unfloader.Protocol.Gebug.Message
{
    /// <summary>
    /// Command to load a stage.
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Stage, Command = (byte)GebugCmdStage.SetStage)]
    public class GebugStageSetStageMessage : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugStageSetStageMessage"/> class.
        /// </summary>
        public GebugStageSetStageMessage()
          : base()
        {
        }

        /// <summary>
        /// Stage to load. See <see cref="Getools.Lib.Game.EnumModel.LevelIdX"/>.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 1, UseDirection = ParameterUseDirection.PcToConsole)]
        public byte LevelId { get; set; }
    }
}
