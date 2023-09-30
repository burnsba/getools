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
    /// Command to show debug menu.
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Debug, Command = (byte)GebugCmdDebug.ShowDebugMenu)]
    public class GebugDebugMenuOpenMessage : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugDebugMenuOpenMessage"/> class.
        /// </summary>
        public GebugDebugMenuOpenMessage()
          : base(GebugMessageCategory.Debug)
        {
            Command = (int)GebugCmdDebug.ShowDebugMenu;
        }

        /// <summary>
        /// Open value (true/false).
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 1, UseDirection = ParameterUseDirection.PcToConsole)]
        public byte Open { get; set; }
    }
}
