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
    /// Command to enable or disable native cheats.
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Cheat, Command = (byte)GebugCmdCheat.SetCheatStatus)]
    public class GebugCheatSetCheatStatusMessage : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugCheatSetCheatStatusMessage"/> class.
        /// </summary>
        public GebugCheatSetCheatStatusMessage()
          : base(GebugMessageCategory.Cheat)
        {
            Command = (int)GebugCmdCheat.SetCheatStatus;
        }

        /// <summary>
        /// Enable or disable cheat.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 1, UseDirection = ParameterUseDirection.PcToConsole)]
        public byte Enable { get; set; }

        /// <summary>
        /// Cheat to enable or disable. See <see cref="Getools.Lib.Game.EnumModel.CheatIdX"/>.
        /// </summary>
        [GebugParameter(ParameterIndex = 1, Size = 1, UseDirection = ParameterUseDirection.PcToConsole)]
        public byte CheatId { get; set; }
    }
}
