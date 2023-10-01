using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Gebug.Message.MessageType
{
    /// <summary>
    /// Gebug methods for video information (vi.c, fr.c, viewport.c, etc).
    /// </summary>
    public enum GebugCmdVi
    {
        /// <summary>
        /// Unset / unknown.
        /// </summary>
        DefaultUnknown = 0,

        /// <summary>
        /// Dumps the framebuffer back to PC.
        /// </summary>
        GrabFramebuffer = 10,

        /// <summary>
        /// Native `void viSetZRange(f32 near, f32 far)`.
        /// </summary>
        SetZRange = 22,

        /// <summary>
        /// Native `void viSetViewSize(s16 x, s16 y)`.
        /// </summary>
        SetViewSize = 24,

        /// <summary>
        /// Native `void viSetViewPosition(s16 left, s16 top)`.
        /// </summary>
        SetViewPosition = 26,
    }
}
