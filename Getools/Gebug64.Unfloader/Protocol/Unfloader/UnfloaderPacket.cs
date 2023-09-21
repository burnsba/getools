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
    public abstract class UnfloaderPacket : IUnfloaderPacket
    {
        public const int ProtocolHeaderSize = 4;

        protected byte[] _data;

        public UnfloaderMessageType MessageType { get; set; }
        public int Size { get; set; }

        public UnfloaderPacket(UnfloaderMessageType dataType, byte[] data)
        {
            MessageType = dataType;
            Size = data?.Length ?? 0;
            _data = data;
        }

        public void SetContent(byte[] body)
        {
            _data = body;
        }

        public byte[] GetInnerPacket()
        {
            if (object.ReferenceEquals(null, _data))
            {
                throw new NullReferenceException($"Body content not set");
            }

            return _data;
        }

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

        public static UnfloaderPacketParseResult TryParse(List<byte> data)
        {
            var result = new UnfloaderPacketParseResult()
            {
                ParseStatus = PacketParseStatus.DefaultUnknown,
            };

            int readOffset = 0;
            int packetSize = 0;

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
    }
}
