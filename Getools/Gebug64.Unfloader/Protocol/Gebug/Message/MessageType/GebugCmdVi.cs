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
        /// Native `void viSetFov(f32 fovx, f32 fovy)`.
        /// </summary>
        SetFov = 20,

        /// <summary>
        /// Native `void viSetZRange(f32 near, f32 far)`.
        /// </summary>
        SetZRange = 22,

        /// <summary>
        /// Native `void currentPlayerSetScreenSize(f32 width, f32 height)`.
        /// </summary>
        CurrentPlayerSetScreenSize = 40,

        /// <summary>
        /// Native `void currentPlayerSetScreenPosition(f32 left, f32 top)`.
        /// </summary>
        CurrentPlayerSetScreenPosition = 42,

        /// <summary>
        /// Native `void currentPlayerSetPerspective(f32 near, f32 fovy, f32 aspect)`.
        /// </summary>
        CurrentPlayerSetPerspective = 44,
    }
}
