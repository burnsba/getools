using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.UsbPacket
{
    /// <summary>
    /// UNFLoader USB packet, as implemented on N64 ROM.
    /// This contains some additional protocol header data not required
    /// when interfacing with flash hardware directly.
    /// </summary>
    public class Packet
    {
        // data without usb header info
        protected byte[]? _data;

        /// <summary>
        /// Size of packet is offset by header/tail byte length.
        /// </summary>
        public const int ProtocolByteLength = 12;

        public PacketType DataType { get; set; }

        public int Size { get; set; }

        public Packet() { }

        public Packet(PacketType dataType, byte[] data)
        {
            DataType = dataType;
            Size = data?.Length ?? 0;
            _data = data;
        }

        public byte[]? GetData() => _data;

        public byte[] Wrap()
        {
            var toSend = new List<byte>
            {
                (byte)'D',
                (byte)'M',
                (byte)'A',
                (byte)'@'
            };

            var length = _data?.Length ?? 0;

            toSend.Add((byte)(length >> 16));
            toSend.Add((byte)(length >> 8));
            toSend.Add((byte)(length >> 0));

            toSend.Add((byte)DataType);

            if (!ReferenceEquals(null, _data))
            {
                toSend.AddRange(_data);
            }

            toSend.Add((byte)'C');
            toSend.Add((byte)'M');
            toSend.Add((byte)'P');
            toSend.Add((byte)'H');

            return toSend.ToArray();
        }

        /// <summary>
        /// Receive message from N64.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static PacketParseResult Unwrap(byte[] data, out Packet? result)
        {
            result = null;

            if (ReferenceEquals(data, null))
            {
                return PacketParseResult.DataTooShort;
            }

            if (data.Length < 12)
            {
                result = new UnknownPacket(data);
                return PacketParseResult.DataTooShort;
            }

            int readOffset = 0;
            byte b;
            int state = 0;
            bool foundStart = false;

            while (true)
            {
                b = data[readOffset++];

                switch (state)
                {
                    case 0:
                        if (b == 'D')
                        {
                            state++;
                        }
                        else
                        {
                            state = 0;
                        }
                        break;

                    case 1:
                        if (b == 'M')
                        {
                            state++;
                        }
                        else
                        {
                            state = 0;
                        }
                        break;

                    case 2:
                        if (b == 'A')
                        {
                            state++;
                        }
                        else
                        {
                            state = 0;
                        }
                        break;

                    case 3:
                        if (b == '@')
                        {
                            state++;
                        }
                        else
                        {
                            state = 0;
                        }
                        break;
                }

                if (state == 4)
                {
                    foundStart = true;
                    break;
                }

                if (readOffset > data.Length - 8)
                {
                    result = new UnknownPacket(data);

                    return PacketParseResult.HeaderNotFound;
                }
            }

            if (!foundStart)
            {
                result = new UnknownPacket(data);

                return PacketParseResult.HeaderNotFound;
            }

            int packetSize = 0;

            PacketType dataType = (PacketType)(int)data[readOffset++];

            packetSize |= data[readOffset++] << 16;
            packetSize |= data[readOffset++] << 8;
            packetSize |= data[readOffset++] << 0;

            var readData = new List<byte>();
            state = 0;
            bool foundEnd = false;
            int readPacketSize = 0;

            while (true)
            {
                b = data[readOffset++];
                readPacketSize++;
                readData.Add(b);

                switch (state)
                {
                    case 0:
                        if (b == 'C')
                        {
                            state++;
                        }
                        else
                        {
                            state = 0;
                        }
                        break;

                    case 1:
                        if (b == 'M')
                        {
                            state++;
                        }
                        else
                        {
                            state = 0;
                        }
                        break;

                    case 2:
                        if (b == 'P')
                        {
                            state++;
                        }
                        else
                        {
                            state = 0;
                        }
                        break;

                    case 3:
                        if (b == 'H')
                        {
                            state++;
                        }
                        else
                        {
                            state = 0;
                        }
                        break;
                }

                if (state == 4)
                {
                    readPacketSize -= 4;

                    readData.RemoveAt(readData.Count - 1);
                    readData.RemoveAt(readData.Count - 1);
                    readData.RemoveAt(readData.Count - 1);
                    readData.RemoveAt(readData.Count - 1);

                    foundEnd = true;
                    break;
                }

                if (readOffset == data.Length)
                {
                    break;
                }
            }

            if (!foundEnd)
            {
                result = new UnknownPacket(data);

                return PacketParseResult.InvalidTail;
            }

            if (readData.Count != readPacketSize)
            {
                // sanity check, this will only happen from bad programming logic.
                throw new InvalidOperationException();
            }

            if (readPacketSize != packetSize)
            {
                result = new UnknownPacket(data);

                return PacketParseResult.SizeMismatch;
            }

            if (dataType == PacketType.Text)
            {
                result = new TextPacket(readData.ToArray());
            }
            else if (dataType == PacketType.HeartBeat)
            {
                result = new HeartbeartPacket(readData.ToArray());
            }
            else
            {
                result = new Packet(dataType, readData.ToArray());
            }

            return PacketParseResult.Success;
        }

        public override string ToString()
        {
            return $"size={Size}, type={DataType}";
        }
    }
}
