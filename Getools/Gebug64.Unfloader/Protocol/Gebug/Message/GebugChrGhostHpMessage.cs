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
    /// Remove all body armor and all but 0.01 HP from guard.
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Chr, Command = (byte)GebugCmdChr.GhostHp)]
    public class GebugChrGhostHpMessage : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugChrGhostHpMessage"/> class.
        /// </summary>
        public GebugChrGhostHpMessage()
          : base(GebugMessageCategory.Chr)
        {
            Command = (int)GebugCmdChr.GhostHp;
        }

        /// <summary>
        /// In-game chrnum id.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 2, UseDirection = ParameterUseDirection.PcToConsole)]
        public UInt16 ChrNum { get; set; }

        /// <summary>
        /// Index source from g_ChrSlots.
        /// </summary>
        [GebugParameter(ParameterIndex = 2, Size = 1, UseDirection = ParameterUseDirection.PcToConsole)]
        public byte ChrSlotIndex { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Category} {DebugCommand} chrnum {ChrNum} (index {ChrSlotIndex})";
        }
    }
}
