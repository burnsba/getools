using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Message.MessageType
{
    public enum GebugMessageCategory
    {
        DefaultUnknown = 0,

        Ack = 1,
        Ramrom = 10,
        Replay = 15,
        SaveState = 20,
        Debug = 25,
        Objectives = 30,
        Cheat = 35,
        Memory = 40,
        Sound = 45,
        Fog = 50,
        Stage = 55,
        Bond = 60,
        Chr = 65,
        Objects = 70,
        File = 75,
        Vi = 80,
        Meta = 90,
        Misc = 95,
    }
}
