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
    /// PC send request to console to add a new memory watch.
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Memory, Command = (byte)GebugCmdMemory.AddWatch)]
    public class GebugMemoryAddWatch : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugMemoryAddWatch"/> class.
        /// </summary>
        public GebugMemoryAddWatch()
          : base()
        {
        }

        /// <summary>
        /// Unique id of memory watch. PC needs to manage this.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 1, UseDirection = ParameterUseDirection.PcToConsole)]
        public byte Id { get; set; }

        /// <summary>
        /// Size in bytes of address to watch.
        /// </summary>
        [GebugParameter(ParameterIndex = 1, Size = 1, UseDirection = ParameterUseDirection.PcToConsole)]
        public byte Size { get; set; }

        /// <summary>
        /// Unused.
        /// </summary>
        [GebugParameter(ParameterIndex = 2, Size = 2, UseDirection = ParameterUseDirection.PcToConsole)]
        public UInt16 Padding { get; set; }

        /// <summary>
        /// Address that console should watch.
        /// </summary>
        [GebugParameter(ParameterIndex = 3, Size = 4, UseDirection = ParameterUseDirection.PcToConsole)]
        public UInt32 Address { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Category} {DebugCommand} id {Id}: 0x{Address:X8} ({Size} bytes)";
        }
    }
}
