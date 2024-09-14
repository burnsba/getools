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
    /// Get framebuffer message.
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Vi, Command = (byte)GebugCmdVi.GrabFramebuffer)]
    public class GebugViFramebufferMessage : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugViFramebufferMessage"/> class.
        /// </summary>
        public GebugViFramebufferMessage()
          : base()
        {
        }

        /// <summary>
        /// Result from `viGetX()`.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 2, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public ushort Width { get; set; }

        /// <summary>
        /// Result from `viGetY()`.
        /// </summary>
        [GebugParameter(ParameterIndex = 1, Size = 2, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public ushort Height { get; set; }

        /// <summary>
        /// Framebuffer data. Data should be processed 16 bits at a time, read as N64 native 5551 RGBA format.
        /// </summary>
        [GebugParameter(ParameterIndex = 2, IsVariableSize = true, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public byte[]? Data { get; set; }
    }
}
