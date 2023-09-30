using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Gebug64.Unfloader.Protocol.Unfloader.Message.MessageType;

namespace Gebug64.Unfloader.Protocol.Unfloader.Message
{
    /// <summary>
    /// UNFLoader binary type packet. This is the format used by gebug romhack packets.
    /// </summary>
    public class BinaryPacket : UnfloaderPacket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryPacket"/> class.
        /// </summary>
        /// <param name="data">Inner packet (body) data without header/tail protocol data.</param>
        public BinaryPacket(byte[] data)
            : base(UnfloaderMessageType.Binary, data)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"size={Size}, type={MessageType}";
        }
    }
}
