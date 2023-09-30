using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Unfloader.Message.MessageType
{
    /// <summary>
    /// UNFLoader USB packet type, as implemented on N64 ROM.
    /// </summary>
    /// <remarks>
    /// Values provided by UNFLoader usb.h .
    /// </remarks>
    public enum UnfloaderMessageType
    {
        /// <summary>
        /// Text.
        /// </summary>
        Text = 1,

        /// <summary>
        /// Binary packet. All gebug romhack packets use this format.
        /// </summary>
        Binary = 2,

        /// <summary>
        /// Not supported.
        /// </summary>
        Header = 3,

        /// <summary>
        /// Not supported.
        /// </summary>
        Screenshot = 4,

        /// <summary>
        /// UNFLoader version information.
        /// </summary>
        HeartBeat = 5,

        /// <summary>
        /// Not supported.
        /// </summary>
        RmonBinary = 0x69,
    }
}
