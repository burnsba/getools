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
    /// Packet contains `struct ramromfilestructure` header data, will load and start replay.
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Ramrom, Command = (byte)GebugCmdRamrom.StartDemoReplayFromPc)]
    public class GebugRamromStartDemoReplayFromPcMessage : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugRamromStartDemoReplayFromPcMessage"/> class.
        /// </summary>
        public GebugRamromStartDemoReplayFromPcMessage()
          : base(GebugMessageCategory.Ramrom)
        {
            Command = (int)GebugCmdRamrom.StartDemoReplayFromPc;
        }

        /// <summary>
        /// Byte array of `struct ramromfilestructure`, <see cref="RamromFile"/>.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, IsVariableSize = true, UseDirection = ParameterUseDirection.PcToConsole)]
        public byte[]? Header { get; set; }
    }
}
