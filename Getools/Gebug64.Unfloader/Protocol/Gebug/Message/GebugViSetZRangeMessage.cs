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
    /// Call native `void viSetZRange(f32 near, f32 far)`.
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Vi, Command = (byte)GebugCmdVi.SetZRange)]
    public class GebugViSetZRangeMessage : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugViSetZRangeMessage"/> class.
        /// </summary>
        public GebugViSetZRangeMessage()
          : base()
        {
        }

        /// <summary>
        /// First parameter.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 4, UseDirection = ParameterUseDirection.PcToConsole)]
        public Single Near { get; set; }

        /// <summary>
        /// Second parameter.
        /// </summary>
        [GebugParameter(ParameterIndex = 1, Size = 4, UseDirection = ParameterUseDirection.PcToConsole)]
        public Single Far { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Category} {DebugCommand} ({Near}, {Far})";
        }
    }
}
