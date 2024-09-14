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
    /// Call native `void viSetViewSize(s16 x, s16 y)`.
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Vi, Command = (byte)GebugCmdVi.SetViewSize)]
    public class GebugViSetViewSizeMessage : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugViSetViewSizeMessage"/> class.
        /// </summary>
        public GebugViSetViewSizeMessage()
          : base()
        {
        }

        /// <summary>
        /// First parameter.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 2, UseDirection = ParameterUseDirection.PcToConsole)]
        public short Width { get; set; }

        /// <summary>
        /// Second parameter.
        /// </summary>
        [GebugParameter(ParameterIndex = 1, Size = 2, UseDirection = ParameterUseDirection.PcToConsole)]
        public short Height { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Category} {DebugCommand} ({Width}, {Height})";
        }
    }
}
