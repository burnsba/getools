﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Parse;

namespace Gebug64.Unfloader.Protocol.Flashcart
{
    /// <summary>
    /// Parse result context.
    /// </summary>
    public class FlashcartPacketParseResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlashcartPacketParseResult"/> class.
        /// </summary>
        public FlashcartPacketParseResult()
        {
        }

        /// <summary>
        /// Gets or sets the associated packet. Null if <see cref="ParseStatus"/> is not <see cref="PacketParseStatus.Success"/>.
        /// </summary>
        public IFlashcartPacket? Packet { get; set; }

        /// <summary>
        /// Gets or sets whether the packet was parsed successfully.
        /// </summary>
        public PacketParseStatus ParseStatus { get; set; }

        /// <summary>
        /// Gets or sets the reason the packet failed to parse.
        /// </summary>
        public PacketParseReason ErrorReason { get; set; }

        /// <summary>
        /// Gets or sets the number of bytes consumed by the packet.
        /// </summary>
        public int TotalBytesRead { get; set; }
    }
}
