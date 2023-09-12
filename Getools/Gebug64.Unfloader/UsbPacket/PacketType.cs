using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.UsbPacket
{
    /// <summary>
    /// UNFLoader USB packet type, as implemented on N64 ROM.
    /// </summary>
    public enum PacketType
    {
        Text = 1,
        Binary = 2,
        Header = 3,
        Screenshot = 4,
        HeartBeat = 5,
        RmonBinary = 0x69,

        Unknown = 999,
    }
}
