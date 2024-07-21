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
    /// Call native fileUnlockStageInFolderAtDifficulty(s32 foldernum, LEVEL_SOLO_SEQUENCE stage, DIFFICULTY difficulty, s32 newtime).
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.File, Command = (byte)GebugCmdFile.UnlockStageDifficulty)]
    public class GebugFileUnlockStageDifficultyMessage : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugFileUnlockStageDifficultyMessage"/> class.
        /// </summary>
        public GebugFileUnlockStageDifficultyMessage()
          : base(GebugMessageCategory.File)
        {
            Command = (int)GebugCmdFile.UnlockStageDifficulty;
        }

        /// <summary>
        /// enum LEVEL_SOLO_SEQUENCE stage.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 1, UseDirection = ParameterUseDirection.PcToConsole)]
        public byte Stage { get; set; }

        /// <summary>
        /// <see cref="Getools.Lib.Game.Enums.Difficulty"/>.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 1, UseDirection = ParameterUseDirection.PcToConsole)]
        public byte Difficulty { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            if (Source == CommunicationSource.Pc)
            {
                return $"{Category} {DebugCommand}";
            }
            else
            {
                return $"{Category} {DebugCommand}";
            }
        }
    }
}
