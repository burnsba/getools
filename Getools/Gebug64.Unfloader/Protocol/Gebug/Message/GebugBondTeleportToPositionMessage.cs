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
    /// Teleport Bond to position.
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Bond, Command = (byte)GebugCmdBond.TeleportToPosition)]
    public class GebugBondTeleportToPositionMessage : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugBondTeleportToPositionMessage"/> class.
        /// </summary>
        public GebugBondTeleportToPositionMessage()
          : base(GebugMessageCategory.Bond)
        {
            Command = (int)GebugCmdBond.TeleportToPosition;
        }

        /// <summary>
        /// Teleport target x position.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 4, UseDirection = ParameterUseDirection.PcToConsole)]
        public Single PosX { get; set; }

        /// <summary>
        /// Teleport target z position.
        /// </summary>
        [GebugParameter(ParameterIndex = 1, Size = 4, UseDirection = ParameterUseDirection.PcToConsole)]
        public Single PosZ { get; set; }

        /// <summary>
        /// Target stan id. This should be <see cref="Getools.Lib.Game.Asset.Stan.StandTile.InternalName"/>.
        /// </summary>
        [GebugParameter(ParameterIndex = 2, Size = 4, UseDirection = ParameterUseDirection.PcToConsole)]
        public Int32 StanId { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            var stanText = StanId.ToString("X6");
            return $"{Category} {DebugCommand} {PosX:0.0000}, {PosZ:0.0000} stan 0x{stanText}";
        }
    }
}
