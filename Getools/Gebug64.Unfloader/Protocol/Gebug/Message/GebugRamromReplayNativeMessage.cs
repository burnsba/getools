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
    /// Console request to start a native replay demo.
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Ramrom, Command = (byte)GebugCmdRamrom.ReplayNativeDemo)]
    public class GebugRamromReplayNativeMessage : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugRamromReplayNativeMessage"/> class.
        /// </summary>
        public GebugRamromReplayNativeMessage()
          : base(GebugMessageCategory.Ramrom)
        {
            Command = (int)GebugCmdRamrom.ReplayNativeDemo;
        }

        /// <summary>
        /// Which replay demo to start.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 1, UseDirection = ParameterUseDirection.PcToConsole)]
        public byte Index { get; set; }
    }
}
