using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Unfloader.Message.MessageType;

namespace Gebug64.Unfloader.Protocol.Unfloader
{
    /// <summary>
    /// Describes interface for UNFLoader packet.
    /// </summary>
    public interface IUnfloaderPacket : IEncapsulate
    {
        /// <summary>
        /// UNFLoader protocol parameter, 1 byte for message type.
        /// </summary>
        UnfloaderMessageType MessageType { get; }

        /// <summary>
        /// UNFLoader protocol parameter, 3 bytes for size.
        /// Number of bytes in the UNFLoader packet after the header.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Sets the inner content of the packet, without any header/tail protocol data.
        /// </summary>
        /// <param name="body">Body data.</param>
        void SetContent(byte[] body);
    }
}
