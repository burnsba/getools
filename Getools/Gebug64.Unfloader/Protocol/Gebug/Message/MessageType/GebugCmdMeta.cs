using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Gebug.Message.MessageType
{
    public enum GebugCmdMeta
    {
        DefaultUnknown = 0,

        ConfigEcho = 1,
        Ping = 2,
        Version = 10,
    }
}
