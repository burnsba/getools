using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Getools.Lib;

namespace Gebug64.Unfloader.Protocol.Gebug
{
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

        public const int ProtocolMaxBodySizeSingle = HardMaxPacketSize - ProtocolOverheadSingle;

        /// <summary>
        /// Number of bytes used by protocol header, for a multi packet message.
        /// This includes bytes counted by <see cref="ProtocolOverheadSingle"/>.
        /// </summary>
        public const int ProtocolOverheadMulti = 16;

        public const int ProtocolMaxBodySizeMulti = HardMaxPacketSize - ProtocolOverheadMulti;

        private const int BytesBeforeSizeOffset = 6;

        // NumberParameters = 2
        // MessageId = 2
        // AckId = 2
        private const int HeaderBytesAlwaysAfterSizeBeforeBody = 6;

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

        public static GebugPacketParseResult TryParse(List<byte> data)
        {
            var dataArr = data.ToArray();

            var result = new GebugPacketParseResult()
            {
                ParseStatus = Parse.PacketParseStatus.DefaultUnknown,
                Packet = null,
                TotalBytesRead = 0,
            };

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

            var expectedBodyLength = offset - sizeAdjust + size;

            if (expectedBodyLength > dataArr.Length)
            {
                result.ParseStatus = Parse.PacketParseStatus.Error;
                return result;
            }

            var actualBodyLength = dataArr.Length - offset;

            var body = new byte[actualBodyLength];
            Array.Copy(dataArr, offset, body, 0, actualBodyLength);

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

        public static ushort GetRandomMessageId()
        {
            var messageIdBytes = Guid.NewGuid().ToByteArray();
            ushort messageId = (ushort)(messageIdBytes[0] << 8);
            messageId |= (ushort)(messageIdBytes[1]);

            return messageId;
        }
    }
}
