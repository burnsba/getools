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
    /// Transfer replay to expansion pak memory, then start ramrom replay.
    /// PC should first check if xpak is installed.
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Ramrom, Command = (byte)GebugCmdRamrom.ReplayFromExpansionPak)]
    public class GebugRamromReplayFromXpakMessage : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugRamromReplayFromXpakMessage"/> class.
        /// </summary>
        public GebugRamromReplayFromXpakMessage()
          : base()
        {
        }

        /// <summary>
        /// Ramrom replay. Should contain file contents of `struct ramromfilestructure` with all iteration data.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, IsVariableSize = true, UseDirection = ParameterUseDirection.PcToConsole)]
        public byte[]? Data { get; set; }
    }
}
