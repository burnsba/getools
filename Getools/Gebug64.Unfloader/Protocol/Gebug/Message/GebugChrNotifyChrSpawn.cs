using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Dto;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Unfloader.Protocol.Gebug.Parameter;
using Getools.Lib;
using Getools.Lib.Game.Enums;
using Newtonsoft.Json.Linq;

namespace Gebug64.Unfloader.Protocol.Gebug.Message
{
    /// <summary>
    /// Sends all character spawn positions for this tick from console to pc.
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Chr, Command = (byte)GebugCmdChr.NotifyChrSpawn)]
    public class GebugChrNotifyChrSpawn : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugChrNotifyChrSpawn"/> class.
        /// </summary>
        public GebugChrNotifyChrSpawn()
          : base(GebugMessageCategory.Chr)
        {
            Command = (int)GebugCmdChr.NotifyChrSpawn;
        }

        /// <summary>
        /// Number of guard positions in <see cref="Data"/>.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 2, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public UInt16 Count { get; set; }

        /// <summary>
        /// Raw result from gebug message. Contains guard positions.
        /// </summary>
        [GebugParameter(ParameterIndex = 2, IsVariableSize = true, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public byte[]? Data { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            if (Source == CommunicationSource.Pc)
            {
                return $"{Category} {DebugCommand}";
            }
            else
            {
                return $"{Category} {DebugCommand} - {Count} chr(s)";
            }
        }
    }
}
