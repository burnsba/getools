using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Unfloader.Protocol.Gebug.Parameter;

namespace Gebug64.Unfloader.Protocol.Gebug.Message
{
    [ProtocolCommand(Category = GebugMessageCategory.Vi, Command = (byte)GebugCmdVi.GrabFramebuffer)]
    public class GebugViFramebufferMessage : GebugMessage, IActivatorGebugMessage
    {
        [GebugParameter(ParameterIndex = 0, Size = 2, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public ushort Width { get; set; }

        [GebugParameter(ParameterIndex = 1, Size = 2, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public ushort Height { get; set; }

        [GebugParameter(ParameterIndex = 2, IsVariableSize = true, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public byte[] Data { get; set; }

        public GebugViFramebufferMessage()
          : base(GebugMessageCategory.Vi)
        {
            Command = (int)GebugCmdVi.GrabFramebuffer;
        }
    }
}
