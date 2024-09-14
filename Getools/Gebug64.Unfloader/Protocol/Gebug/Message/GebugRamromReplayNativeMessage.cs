using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Unfloader.Protocol.Gebug.Parameter;
using Getools.Lib.Game.Asset.Ramrom;

namespace Gebug64.Unfloader.Protocol.Gebug.Message
{
    /// <summary>
    /// Request to start a replay demo from the native `ramrom_table` list.
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Ramrom, Command = (byte)GebugCmdRamrom.ReplayNativeDemo)]
    public class GebugRamromReplayNativeMessage : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugRamromReplayNativeMessage"/> class.
        /// </summary>
        public GebugRamromReplayNativeMessage()
          : base()
        {
        }

        /// <summary>
        /// Which replay demo to start.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 1, UseDirection = ParameterUseDirection.PcToConsole)]
        public byte Index { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            if (Source == CommunicationSource.Pc)
            {
                return $"{Category} {DebugCommand} {Index}";
            }
            else
            {
                return $"{Category} {DebugCommand}";
            }
        }
    }
}
