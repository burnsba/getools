using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Unfloader.Protocol.Gebug.Parameter;

namespace Gebug64.Unfloader.Protocol.Gebug.Message
{
    [ProtocolCommand(Category = GebugMessageCategory.Meta, Command = (byte)GebugCmdMeta.Version)]
    public class GebugMetaVersionMessage : GebugMessage, IActivatorGebugMessage
    {
        [GebugParameter(ParameterIndex = 0, Size = 4, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public int VersionA { get; set; }

        [GebugParameter(ParameterIndex = 1, Size = 4, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public int VersionB { get; set; }

        [GebugParameter(ParameterIndex = 2, Size = 4, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public int VersionC { get; set; }

        [GebugParameter(ParameterIndex = 3, Size = 4, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public int VersionD { get; set; }

        public GebugMetaVersionMessage()
          : base(GebugMessageCategory.Meta)
        {
            Command = (int)GebugCmdMeta.Version;
        }
    }
}
