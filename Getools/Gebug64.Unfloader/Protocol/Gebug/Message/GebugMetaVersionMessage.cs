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
    /// Version information of gebug romhack.
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Meta, Command = (byte)GebugCmdMeta.Version)]
    public class GebugMetaVersionMessage : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugMetaVersionMessage"/> class.
        /// </summary>
        public GebugMetaVersionMessage()
          : base(GebugMessageCategory.Meta)
        {
            Command = (int)GebugCmdMeta.Version;
        }

        /// <summary>
        /// Version info word 1.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 4, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public int VersionA { get; set; }

        /// <summary>
        /// Version info word 2.
        /// </summary>
        [GebugParameter(ParameterIndex = 1, Size = 4, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public int VersionB { get; set; }

        /// <summary>
        /// Version info word 3.
        /// </summary>
        [GebugParameter(ParameterIndex = 2, Size = 4, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public int VersionC { get; set; }

        /// <summary>
        /// Version info word 4.
        /// </summary>
        [GebugParameter(ParameterIndex = 3, Size = 4, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public int VersionD { get; set; }
    }
}
