using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Getools.Lib;

namespace Gebug64.Unfloader.Protocol.Gebug
{
    /// <summary>
    /// Gebug romhack packet.
    /// </summary>
    public record GebugPacket
    {
        /// <summary>
        /// Max size of send packet, with all protocol overhead.
        /// This should equate to the buffer on console.
        /// </summary>
        /// <remarks>
        /// 508 = margin
        /// does the console buffer need to include overhead for everdrive + unfloader? => 508 - 12 = 496.
        /// </remarks>
        public const int HardMaxPacketSize = 496;

        /// <summary>
        /// Number of bytes used by protocol header, for a single packet message.
        /// </summary>
        public const int ProtocolOverheadSingle = 12;

        /// <summary>
        /// Max number of bytes that the body of a single packet message can use before requiring
        /// a multi-packet message.
        /// </summary>
        public const int ProtocolMaxBodySizeSingle = HardMaxPacketSize - ProtocolOverheadSingle;

        /// <summary>
        /// Number of bytes used by protocol header, for a multi packet message.
        /// This includes bytes counted by <see cref="ProtocolOverheadSingle"/>.
        /// </summary>
        public const int ProtocolOverheadMulti = 16;

        /// <summary>
        /// Max number of bytes that the body of a multi-packet message can use.
        /// </summary>
        public const int ProtocolMaxBodySizeMulti = HardMaxPacketSize - ProtocolOverheadMulti;

        /// <summary>
        /// Size parameter is the number of bytes remaining to the end of the packet,
        /// after the size parameter. This is the offset to the start of the packet.
        /// </summary>
        private const int BytesBeforeSizeOffset = 6;

        /// <summary>
        /// This is the number of bytes after the size parameter to the start of the body,
        /// for single packet message.
        /// </summary>
        /// <remarks>
        /// NumberParameters = 2
        /// MessageId = 2
        /// AckId = 2
        /// </remarks>
        private const int HeaderBytesAlwaysAfterSizeBeforeBody = 6;

        /// <summary>
        /// Initializes a new instance of the <see cref="GebugPacket"/> class.
        /// </summary>
        /// <param name="category">Packet category.</param>
        /// <param name="command">Command.</param>
        /// <param name="flags">Associated packet flags.</param>
        /// <param name="numberParameters">Number of parameters included in message body.</param>
        /// <param name="messageId">Associated message id.</param>
        /// <param name="ackId">ACK Id or zero.</param>
        /// <param name="packetNumber">If multi-packet message, this is the current packet number. Otherwise this should be null.</param>
        /// <param name="totalNumberPackets">If multi-packet message, this is the total number of packets. Otherwise this should be null.</param>
        /// <param name="body">Body data for this packet.</param>
        public GebugPacket(
            GebugMessageCategory category,
            byte command,
            ushort flags,
            ushort numberParameters,
            ushort messageId,
            ushort ackId,
            ushort? packetNumber,
            ushort? totalNumberPackets,
            byte[] body)
        {
            if (object.ReferenceEquals(null, body))
            {
                body = new byte[0];
            }

            if (body.Length > HardMaxPacketSize)
            {
                throw new InvalidOperationException($"Packet size of {body.Length} bytes exceeds max supported packet length of {HardMaxPacketSize} bytes.");
            }

            Category = category;
            Command = command;
            Flags = flags;
            MessageId = messageId;
            AckId = ackId;
            NumberParameters = numberParameters;
            PacketNumber = packetNumber;
            TotalNumberPackets = totalNumberPackets;
            Body = body;

            Size = HeaderBytesAlwaysAfterSizeBeforeBody;

            Size += (ushort)(PacketNumber.HasValue ? 2 : 0);
            Size += (ushort)(TotalNumberPackets.HasValue ? 2 : 0);

            Size += (ushort)Body.Length;

            // Validation
            if ((Flags & (ushort)GebugMessageFlags.IsMultiMessage) > 0)
            {
                if (!PacketNumber.HasValue)
                {
                    throw new InvalidOperationException($"IsMultiMessage flag was wet, but {nameof(PacketNumber)} is null.");
                }

                if (!TotalNumberPackets.HasValue)
                {
                    throw new InvalidOperationException($"IsMultiMessage flag was wet, but {nameof(TotalNumberPackets)} is null.");
                }
            }
        }

        /// <summary>
        /// Gets or sets message category.
        /// Size: 1 byte.
        /// </summary>
        public GebugMessageCategory Category { get; init; }

        /// <summary>
        /// Gets or sets message command.
        /// Command interpretation depends on <see cref="Category"/>.
        /// Size: 1 byte.
        /// </summary>
        public byte Command { get; init; }

        /// <summary>
        /// Gets or sets message flags.
        /// Size: 2 bytes.
        /// </summary>
        public ushort Flags { get; init; }

        /// <summary>
        /// Gets or sets the size of this packet.
        /// The size is the number of bytes after the <see cref="Size"/> value to the end of the packet.
        /// Size: 2 bytes.
        /// </summary>
        public ushort Size { get; init; }

        /// <summary>
        /// Gets or sets the number of parameters included in the messaage.
        /// This specific packet may only inlcude part of one parameter, but <see cref="NumberParameters"/>
        /// is for the entire message.
        /// Size: 2 bytes.
        /// </summary>
        public ushort NumberParameters { get; init; }

        /// <summary>
        /// Each packet in the message should share the same message id.
        /// This should be unique enough to avoid ambiguous replies.
        /// </summary>
        public ushort MessageId { get; init; }

        /// <summary>
        /// When <see cref="GebugMessageFlags.IsAck"/> is set, this will contain
        /// the <see cref="MessageId"/> of the message being responded to.
        /// Otherwise the value in this field doesn't matter.
        /// </summary>
        public ushort AckId { get; init; }

        /// <summary>
        /// If this is a multi-packet message, this is the current packet number.
        /// </summary>
        public ushort? PacketNumber { get; init; }

        /// <summary>
        /// If this is a multi-packet message, this is the total number of packets in the message.
        /// </summary>
        public ushort? TotalNumberPackets { get; init; }

        /// <summary>
        /// Gets or sets the packet body.
        /// If there are no parameters, this will be empty.
        /// </summary>
        public byte[] Body { get; init; }

        private string DebugCommand => Gebug64.Unfloader.Protocol.Gebug.Message.MessageType.CommandResolver.ResolveCommand(Category, Command);

        private string DebugFlags => ((GebugMessageFlags)Flags).ToString();

        /// <summary>
        /// Reads bytes from incoming source and attempts to read as many as rquired to
        /// parse a single gebug packet.
        /// </summary>
        /// <param name="data">Data to parse.</param>
        /// <returns>Parse result.</returns>
        public static GebugPacketParseResult TryParse(List<byte> data)
        {
            var dataArr = data.ToArray();

            var result = new GebugPacketParseResult()
            {
                ParseStatus = Parse.PacketParseStatus.DefaultUnknown,
                Packet = null,
                TotalBytesRead = 0,
            };

            // If there's not enough data for a single packet,
            // abort.
            if (data.Count < ProtocolOverheadSingle)
            {
                result.ParseStatus = Parse.PacketParseStatus.Error;
                return result;
            }

            int offset = 0;

            GebugMessageCategory category = (GebugMessageCategory)data[offset++];
            byte command = data[offset++];

            ushort flags = (ushort)BitUtility.Read16Big(dataArr, offset);
            offset += 2;

            ushort size = (ushort)BitUtility.Read16Big(dataArr, offset);
            offset += 2;

            ushort numberParameters = (ushort)BitUtility.Read16Big(dataArr, offset);
            offset += 2;

            ushort messageId = (ushort)BitUtility.Read16Big(dataArr, offset);
            offset += 2;

            ushort ackId = (ushort)BitUtility.Read16Big(dataArr, offset);
            offset += 2;

            // Error check below for the size parameter specified in the packet
            // header, against the number of bytes remaining the method argument.
            // The computed side uses the current read offset, so need to adjust
            // back to the size parameter to get an accurate count.
            int sizeAdjust = HeaderBytesAlwaysAfterSizeBeforeBody;

            ushort? packetNumber = null;
            ushort? totalPackets = null;

            if ((flags & (ushort)GebugMessageFlags.IsMultiMessage) > 0)
            {
                if (offset + 4 > dataArr.Length)
                {
                    result.ParseStatus = Parse.PacketParseStatus.Error;
                    return result;
                }

                packetNumber = (ushort)BitUtility.Read16Big(dataArr, offset);
                offset += 2;

                totalPackets = (ushort)BitUtility.Read16Big(dataArr, offset);
                offset += 2;

                sizeAdjust += 4;
            }

            // expectedReadLength: current read offset, moved back to the
            // size parameter, incremented by `size`.
            var expectedReadLength = offset - sizeAdjust + size;

            // If the expected length is bigger than the amount of data available,
            // it's an error.
            if (expectedReadLength > dataArr.Length)
            {
                result.ParseStatus = Parse.PacketParseStatus.Error;
                return result;
            }

            var bodySize = size - sizeAdjust;
            if (bodySize < 0)
            {
                // This shouldn't happen ....
                throw new InvalidOperationException();
            }

            var body = new byte[bodySize];

            if (bodySize > 0)
            {
                Array.Copy(dataArr, offset, body, 0, bodySize);
            }

            result.Packet = new GebugPacket(
                category,
                command,
                flags,
                numberParameters,
                messageId,
                ackId,
                packetNumber,
                totalPackets,
                body);

            result.ParseStatus = Parse.PacketParseStatus.Success;

            return result;
        }

        /// <summary>
        /// Generates a random message id.
        /// </summary>
        /// <returns>Message id.</returns>
        public static ushort GetRandomMessageId()
        {
            var messageIdBytes = Guid.NewGuid().ToByteArray();
            ushort messageId = (ushort)(messageIdBytes[0] << 8);
            messageId |= (ushort)messageIdBytes[1];

            return messageId;
        }

        /// <summary>
        /// Converts the current packet into a byte array.
        /// </summary>
        /// <returns>Data.</returns>
        public byte[] ToByteArray()
        {
            var result = new byte[Size + BytesBeforeSizeOffset];

            int offset = 0;

            result[offset++] = (byte)Category;
            result[offset++] = (byte)Command;

            BitUtility.InsertShortBig(result, offset, Flags);
            offset += 2;

            BitUtility.InsertShortBig(result, offset, Size);
            offset += 2;

            BitUtility.InsertShortBig(result, offset, NumberParameters);
            offset += 2;

            BitUtility.InsertShortBig(result, offset, MessageId);
            offset += 2;

            BitUtility.InsertShortBig(result, offset, AckId);
            offset += 2;

            if ((Flags & (ushort)GebugMessageFlags.IsMultiMessage) > 0)
            {
                BitUtility.InsertShortBig(result, offset, PacketNumber!.Value);
                offset += 2;

                BitUtility.InsertShortBig(result, offset, TotalNumberPackets!.Value);
                offset += 2;
            }

            Array.Copy(Body, 0, result, offset, Body.Length);

            return result;
        }
    }
}
