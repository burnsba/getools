using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Parameter;

namespace Gebug64.Unfloader.Protocol.Gebug.Message
{
    /// <summary>
    /// Child of <see cref="GebugMemoryWatchBulkRead"/>.
    /// </summary>
    public class GebugMesgMemoryWatchItem
    {
        /// <summary>
        /// Id of corresponding memory watch.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 1, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public byte Id { get; set; }

        /// <summary>
        /// Variable length array of bytes readfor memory watch.
        /// </summary>
        [GebugParameter(ParameterIndex = 1, IsVariableSize = true, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public byte[]? Data { get; set; }
    }
}
