﻿using System;
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
    /// Console request for next iteration blocks.
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Ramrom, Command = (byte)GebugCmdRamrom.ReplayRequestNextIteration)]
    public class GebugRamromIterationMessage : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugRamromIterationMessage"/> class.
        /// </summary>
        public GebugRamromIterationMessage()
          : base(GebugMessageCategory.Ramrom)
        {
            Command = (int)GebugCmdRamrom.ReplayRequestNextIteration;
        }

        /// <summary>
        /// <see cref="GebugMessage.MessageId"/> of original request to load demo from PC.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 2, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public ushort ReplayId { get; set; }

        /// <summary>
        /// Index of <see cref="Getools.Lib.Game.Asset.Ramrom.RamromFile.Iterations" /> being requested.
        /// </summary>
        [GebugParameter(ParameterIndex = 1, Size = 2, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public ushort IterationIndex { get; set; }

        [GebugParameter(ParameterIndex = 2, IsVariableSize = true, UseDirection = ParameterUseDirection.PcToConsole)]
        public byte[]? IterationData { get; set; }
    }
}
