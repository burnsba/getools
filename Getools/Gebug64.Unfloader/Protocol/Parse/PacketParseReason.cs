﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Parse
{
    /// <summary>
    /// Description of why a packet failed to parse.
    /// </summary>
    public enum PacketParseReason
    {
        /// <summary>
        /// Unset default value.
        /// </summary>
        DefaultUnknown,

        /// <summary>
        /// There needs to be at least 12 bytes to convert to a packet.
        /// </summary>
        DataTooShort,

        /// <summary>
        /// Received closing header, but <see cref="UsbPacket.Size"/> differs from the number of bytes read.
        /// </summary>
        SizeMismatch,

        /// <summary>
        /// Could not find header magic bytes.
        /// </summary>
        HeaderNotFound,

        /// <summary>
        /// Could not find closing header magic bytes.
        /// </summary>
        InvalidTail,
    }
}
