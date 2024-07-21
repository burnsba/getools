using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Gebug.Message.MessageType
{
    /// <summary>
    /// Gebug methods for file and file2 methods.
    /// </summary>
    public enum GebugCmdFile
    {
        /// <summary>
        /// Unset / unknown.
        /// </summary>
        DefaultUnknown = 0,

        /// <summary>
        /// Call native fileUnlockStageInFolderAtDifficulty(s32 foldernum, LEVEL_SOLO_SEQUENCE stage, DIFFICULTY difficulty, s32 newtime).
        /// </summary>
        UnlockStageDifficulty = 10,
    }
}
