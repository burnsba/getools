using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Flashcart;
using Gebug64.Unfloader.Protocol.Parse;
using Gebug64.Unfloader.Protocol.Unfloader.Message;
using Gebug64.Unfloader.Protocol.Unfloader.Message.MessageType;

namespace Gebug64.Unfloader.Protocol.Unfloader
{
    /// <summary>
    /// Abstract UNFLoader packet.
    /// </summary>
    public abstract class UnfloaderPacket : IUnfloaderPacket
    {
        /// <summary>
        /// Size in bytes of UNFLoader packet header.
        /// </summary>
        public const int ProtocolHeaderSize = 4;

        /// <summary>
        /// Inner packet (body) data without header/tail protocol data.
        /// </summary>
        protected byte[] _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnfloaderPacket"/> class.
        /// </summary>
        /// <param name="dataType">Type of UNFLoader packet.</param>
        /// <param name="data">Inner packet (body) data without header/tail protocol data.</param>
        public UnfloaderPacket(UnfloaderMessageType dataType, byte[] data)
        {
            if (object.ReferenceEquals(null, data))
            {
                throw new NullReferenceException();
            }

            MessageType = dataType;
            Size = data?.Length ?? 0;
            _data = data!;
        }

        /// <inheritdoc />
        public UnfloaderMessageType MessageType { get; set; }

        /// <inheritdoc />
        public int Size { get; set; }

        /// <inheritdoc />
        public Type? InnerType { get; set; }

        /// <inheritdoc />
        public object? InnerData { get; set; }

        /// <summary>
        /// Reads bytes from incoming source and attempts to read as many as rquired to
        /// parse a single UNFLoader packet.
        /// </summary>
        /// <param name="data">Data to parse.</param>
        /// <returns>Parse result.</returns>
        public static UnfloaderPacketParseResult TryParse(List<byte> data)
        {
            var result = new UnfloaderPacketParseResult()
            {
                ParseStatus = PacketParseStatus.DefaultUnknown,
            };

            int readOffset = 0;
            int packetSize = 0;

            if (data.Count < 4)
            {
                result.ParseStatus = PacketParseStatus.Error;
                result.ErrorReason = PacketParseReason.DataTooShort;
                return result;
            }

            var dataType = (UnfloaderMessageType)(int)data[readOffset++];

            packetSize |= data[readOffset++] << 16;
            packetSize |= data[readOffset++] << 8;
            packetSize |= data[readOffset++] << 0;

            // everdrive tail size isn't included in UNFLoader protocol `size` parameter.
            if (readOffset + packetSize > data.Count)
            {
                result.ParseStatus = PacketParseStatus.Error;
                result.ErrorReason = PacketParseReason.SizeMismatch;
                return result;
            }

            var readData = new byte[packetSize];
            Array.Copy(data.ToArray(), readOffset, readData, 0, packetSize);

            readOffset += packetSize;

            result.TotalBytesRead = readOffset;

            if (dataType == UnfloaderMessageType.Text)
            {
                result.Packet = new TextPacket(readData.ToArray());
            }
            else if (dataType == UnfloaderMessageType.HeartBeat)
            {
                result.Packet = new HeartbeartPacket(readData.ToArray());
            }
            else if (dataType == UnfloaderMessageType.Binary)
            {
                result.Packet = new BinaryPacket(readData.ToArray());
            }

            result.ParseStatus = PacketParseStatus.Success;
            return result;
        }

        /// <inheritdoc />
        public void SetContent(byte[] body)
        {
            _data = body;
        }

        /// <inheritdoc />
        public byte[] GetInnerPacket()
        {
            if (object.ReferenceEquals(null, _data))
            {
                throw new NullReferenceException($"Body content not set");
            }

            return _data;
        }

        /// <inheritdoc />
        public byte[] GetOuterPacket()
        {
            var toSend = new List<byte>();

            var length = _data?.Length ?? 0;

            // UNFloader/everdrive requires "2 byte aligned" data.
            // Add an extra zero byte if sending an odd number of bytes.
            bool pad = false;

            if ((length & 1) == 1)
            {
                pad = true;
                length++;
            }

            toSend.Add((byte)MessageType);

            toSend.Add((byte)(length >> 16));
            toSend.Add((byte)(length >> 8));
            toSend.Add((byte)(length >> 0));

            if (!ReferenceEquals(null, _data))
            {
                toSend.AddRange(_data);
            }

            if (pad)
            {
                toSend.Add(0);
            }

            return toSend.ToArray();
        }
    }
}
