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
    /// Request osGetCount().
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Misc, Command = (byte)GebugCmdMisc.OsTime)]
    public class GebugMiscOsTimeMessage : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugMiscOsTimeMessage"/> class.
        /// </summary>
        public GebugMiscOsTimeMessage()
          : base(GebugMessageCategory.Misc)
        {
            Command = (int)GebugCmdMisc.OsTime;
        }

        /// <summary>
        /// OS Time value.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 4, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public uint Count { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            if (Source == CommunicationSource.Pc)
            {
                return $"{Category} {DebugCommand}";
            }
            else
            {
                return $"{Category} {DebugCommand} 0x{Count:x8}";
            }
        }
    }
}
