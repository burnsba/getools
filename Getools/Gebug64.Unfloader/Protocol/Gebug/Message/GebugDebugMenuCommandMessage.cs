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
    /// Execute debug menu command, according to gebug romhack switch statement value.
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Debug, Command = (byte)GebugCmdDebug.DebugMenuProcessor)]
    public class GebugDebugMenuCommandMessage : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugDebugMenuCommandMessage"/> class.
        /// </summary>
        public GebugDebugMenuCommandMessage()
          : base()
        {
        }

        /// <summary>
        /// Menu command to execute.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 1, UseDirection = ParameterUseDirection.PcToConsole)]
        public byte MenuCommand { get; set; }
    }
}
