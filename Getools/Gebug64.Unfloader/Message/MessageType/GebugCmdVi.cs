using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Message.MessageType
{
    public enum GebugCmdVi
    {
        DefaultUnknown = 0,

        GrabFramebuffer = 10,
        SetFov = 20,
        SetZRange = 22,
        CurrentPlayerSetScreenSize = 40,
        CurrentPlayerSetScreenPosition = 42,
        CurrentPlayerSetPerspective = 44,
    }
}
