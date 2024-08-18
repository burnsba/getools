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
    /// Message from console to pc when a level is selected, or automatically advanced.
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Stage, Command = (byte)GebugCmdStage.NotifyLevelSelected)]
    public class GebugStageNotifyLevelSelected : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugStageNotifyLevelSelected"/> class.
        /// </summary>
        public GebugStageNotifyLevelSelected()
          : base(GebugMessageCategory.Stage)
        {
            Command = (int)GebugCmdStage.NotifyLevelSelected;
        }

        /// <summary>
        /// Selected stage. See <see cref="Getools.Lib.Game.EnumModel.LevelIdX"/>.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 2, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public UInt16 LevelId { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            Getools.Lib.Game.Enums.LevelId lid = (Getools.Lib.Game.Enums.LevelId)LevelId;

            return $"{Category} {DebugCommand} {lid}";
        }
    }
}
