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
    /// Console send bulk read of all current memory watches to PC.
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Memory, Command = (byte)GebugCmdMemory.WatchBulkRead)]
    public class GebugMemoryWatchBulkRead : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugMemoryWatchBulkRead"/> class.
        /// </summary>
        public GebugMemoryWatchBulkRead()
          : base()
        {
            WatchResults = new List<GebugMesgMemoryWatchItem>();
        }

        /// <summary>
        /// Number of memory watch values in <see cref="Data"/>.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 1, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public byte Count { get; set; }

        /// <summary>
        /// Raw result from gebug message. Contains memory watch values.
        /// </summary>
        [GebugParameter(ParameterIndex = 1, IsVariableSizeList = true, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public List<GebugMesgMemoryWatchItem> WatchResults { get; set; }

        /// <summary>
        /// Gets length of <see cref="WatchResults"/>.
        /// </summary>
        /// <returns><see cref="Count"/>.</returns>
        public override int GetVariableSizeListCount()
        {
            return Count;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Category} {DebugCommand} {Count} watches";
        }
    }
}
