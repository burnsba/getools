using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Gebug.Message.MessageType
{
    /// <summary>
    /// Native debug methods.
    /// </summary>
    public enum GebugCmdDebug
    {
        /// <summary>
        /// Unset / unknown.
        /// </summary>
        DefaultUnknown = 0,

        /// <summary>
        /// Sets `g_BossIsDebugMenuOpen` to the value of the parameter.
        /// </summary>
        ShowDebugMenu = 1,

        /// <summary>
        /// Calls `debug_menu_case_processer` with the supplied value.
        /// </summary>
        DebugMenuProcessor = 99,
    }
}
