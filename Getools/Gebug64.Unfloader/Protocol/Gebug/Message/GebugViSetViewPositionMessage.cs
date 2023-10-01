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
    /// Call native `void viSetViewPosition(s16 left, s16 top)`.
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Vi, Command = (byte)GebugCmdVi.SetViewPosition)]
    public class GebugViSetViewPositionMessage : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugViSetViewPositionMessage"/> class.
        /// </summary>
        public GebugViSetViewPositionMessage()
          : base(GebugMessageCategory.Vi)
        {
            Command = (int)GebugCmdVi.SetViewPosition;
        }

        /// <summary>
        /// First parameter.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 2, UseDirection = ParameterUseDirection.PcToConsole)]
        public short Left { get; set; }

        /// <summary>
        /// Second parameter.
        /// </summary>
        [GebugParameter(ParameterIndex = 1, Size = 2, UseDirection = ParameterUseDirection.PcToConsole)]
        public short Top { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Category} {DebugCommand} ({Left}, {Top})";
        }
    }
}
