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
    /// PC send request to console to remove an existing memory watch.
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Memory, Command = (byte)GebugCmdMemory.RemoveWatch)]
    public class GebugMemoryRemoveWatch : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugMemoryRemoveWatch"/> class.
        /// </summary>
        public GebugMemoryRemoveWatch()
          : base()
        {
        }

        /// <summary>
        /// Unique id of memory watch. PC needs to manage this.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 1, UseDirection = ParameterUseDirection.PcToConsole)]
        public byte Id { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Category} {DebugCommand} id {Id}";
        }
    }
}
