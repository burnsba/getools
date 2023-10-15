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
    /// Request osGetMemSize().
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Misc, Command = (byte)GebugCmdMisc.OsMemSize)]
    public class GebugMiscOsGetMemSizeMessage : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugMiscOsGetMemSizeMessage"/> class.
        /// </summary>
        public GebugMiscOsGetMemSizeMessage()
          : base(GebugMessageCategory.Misc)
        {
            Command = (int)GebugCmdMisc.OsMemSize;
        }

        /// <summary>
        /// OS memory size value in bytes.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 4, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public uint Size { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            if (Source == CommunicationSource.Pc)
            {
                return $"{Category} {DebugCommand}";
            }
            else
            {
                return $"{Category} {DebugCommand} 0x{Size:x8}";
            }
        }
    }
}
