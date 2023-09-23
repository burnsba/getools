using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Gebug
{
    [Flags]
    public enum GebugMessageFlags
    {
        IsMultiMessage = 0x1,
    }
}
