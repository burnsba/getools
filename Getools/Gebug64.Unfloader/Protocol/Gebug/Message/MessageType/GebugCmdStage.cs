using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Gebug.Message.MessageType
{
    /// <summary>
    /// Gebug methods to manage the stage.
    /// </summary>
    public enum GebugCmdStage
    {
        /// <summary>
        /// Unset / unknown.
        /// </summary>
        DefaultUnknown = 0,

        /// <summary>
        /// Sets `g_MainStageNum` to the supplied value.
        /// </summary>
        SetStage = 10,

        /// <summary>
        /// Send notice from console to pc that a stage has been selected.
        /// </summary>
        NotifyLevelSelected = 13,

        /// <summary>
        /// Send notice from console to pc that a stage is being loaded.
        /// </summary>
        NotifyLevelLoaded = 14,
    }
}
