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
    /// Meta command to disable all active cheats.
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Cheat, Command = (byte)GebugCmdCheat.DisableAll)]
    public class GebugCheatDisableAllMessage : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugCheatDisableAllMessage"/> class.
        /// </summary>
        public GebugCheatDisableAllMessage()
          : base(GebugMessageCategory.Cheat)
        {
            Command = (int)GebugCmdCheat.DisableAll;
        }
    }
}
