using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Gebug.Message.MessageType
{
    /// <summary>
    /// Gebug moethds to manage the stage.
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
    }
}
