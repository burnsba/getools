using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Unfloader.Protocol.Gebug.Parameter;

namespace Gebug64.Unfloader.Protocol.Gebug
{
    /// <summary>
    /// Interface to describe gebug romhack message.
    /// A message can be composed of multiple packets.
    /// </summary>
    public interface IGebugMessage
    {
        /// <summary>
        /// Message category.
        /// Size: 1 byte.
        /// </summary>
        GebugMessageCategory Category { get; }

        /// <summary>
        /// Message command.
        /// Command interpretation depends on <see cref="Category"/>.
        /// Size: 1 byte.
        /// </summary>
        int Command { get; }

        /// <summary>
        /// Each packet in the message should share the same message id.
        /// This should be unique enough to avoid ambiguous replies.
        /// </summary>
        ushort MessageId { get; }

        /// <summary>
        /// When <see cref="GebugMessageFlags.IsAck"/> is set, this will contain
        /// the <see cref="MessageId"/> of the message being responded to.
        /// Otherwise the value in this field doesn't matter.
        /// </summary>
        ushort AckId { get; }

        /// <summary>
        /// Gets the time this message was created.
        /// </summary>
        DateTime InstantiateTime { get; }

        /// <summary>
        /// Origination source of the message.
        /// </summary>
        CommunicationSource Source { get; }

        /// <summary>
        /// Access first packet in the message.
        /// </summary>
        GebugPacket? FirstPacket { get; }

        /// <summary>
        /// Convert the message into one or more packets that can be sent.
        /// </summary>
        /// <param name="sendDirection">Specifies send direction, which is required to know which parameters should be included or not.</param>
        /// <returns>One or more packets.</returns>
        /// <remarks>
        /// The communcation layer can only send packets, not messages.
        /// </remarks>
        List<GebugPacket> ToSendPackets(ParameterUseDirection sendDirection);

        /// <summary>
        /// Set appropriate values such as <see cref="AckId"/> in this message to reply to a message.
        /// </summary>
        /// <param name="source">Message being replied to.</param>
        void ReplyTo(IGebugMessage source);
    }
}
