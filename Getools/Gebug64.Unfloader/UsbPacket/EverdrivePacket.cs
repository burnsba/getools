using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.UsbPacket
{
    public class EverdrivePacket : PacketBase
    {
        public const int ProtocolHeaderSize = 8;
        public const int ProtocolTailSize = 4;

        /// <summary>
        /// Size of packet is offset by header/tail byte length.
        /// </summary>
        public const int ProtocolByteLength = ProtocolHeaderSize + ProtocolTailSize;

        public EverdrivePacket(PacketType packetType, byte[] data)
            : base(packetType, data)
        { }

        public override byte[] GetOuterData()
        {
            var toSend = new List<byte>
            {
                (byte)'D',
                (byte)'M',
                (byte)'A',
                (byte)'@'
            };

            var length = _data?.Length ?? 0;

            // UNFloader/everdrive requires "2 byte aligned" data.
            // Add an extra zero byte if sending an odd number of bytes.
            bool pad = false;

            if ((length & 1) == 1)
            {
                pad = true;
                length++;
            }

            toSend.Add((byte)DataType);

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

            toSend.Add((byte)'C');
            toSend.Add((byte)'M');
            toSend.Add((byte)'P');
            toSend.Add((byte)'H');

            return toSend.ToArray();
        }

        internal static PacketParseContext Parse(byte[] data)
        {
            var result = new PacketParseContext()
            {
                ParseResult = PacketParseResult.DefaultUnknown,
            };

            if (object.ReferenceEquals(null, data))
            {
                throw new NullReferenceException();
            }

            if (data.Length < ProtocolByteLength)
            {
                result.ParseResult = PacketParseResult.DataTooShort;
                return result;
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

                if (readOffset > data.Length - ProtocolHeaderSize)
                {
                    result.ParseResult = PacketParseResult.HeaderNotFound;
                    return result;
                }
            }

            if (!foundStart)
            {
                result.ParseResult = PacketParseResult.HeaderNotFound;
                return result;
            }

            int packetSize = 0;

            PacketType dataType = (PacketType)(int)data[readOffset++];

            packetSize |= data[readOffset++] << 16;
            packetSize |= data[readOffset++] << 8;
            packetSize |= data[readOffset++] << 0;

            // Tail size isn't included in everdrive protocol `size` parameter.
            if (readOffset + packetSize > data.Length)
            {
                result.ParseResult = PacketParseResult.SizeMismatch;
                return result;
            }

            result.BytePrefix = readOffset - ProtocolHeaderSize;

            var readData = new byte[packetSize];
            Array.Copy(data, readOffset, readData, 0, packetSize);

            readOffset += packetSize;

            bool foundEnd = IsProtocolTail(data, readOffset);

            if (!foundEnd)
            {
                result.ParseResult = PacketParseResult.InvalidTail;
                return result;
            }

            readOffset += ProtocolTailSize;
            result.TotalBytesRead = readOffset;

            if (dataType == PacketType.Text)
            {
                result.Packet = new TextPacket(readData.ToArray());
            }
            else if (dataType == PacketType.HeartBeat)
            {
                result.Packet = new HeartbeartPacket(readData.ToArray());
            }
            else
            {
                result.Packet = new EverdrivePacket(dataType, readData.ToArray());
            }

            result.ParseResult = PacketParseResult.Success;

            return result;
        }

        ///// <summary>
        ///// Receive message from N64.
        ///// </summary>
        ///// <param name="data"></param>
        ///// <returns></returns>
        //public static PacketParseResult Unwrap(byte[] data, out Packet? result)
        //{
        //    result = null;

        //    if (ReferenceEquals(data, null))
        //    {
        //        return PacketParseResult.DataTooShort;
        //    }

        //    if (data.Length < ProtocolByteLength)
        //    {
        //        result = new UnknownPacket(data);
        //        return PacketParseResult.DataTooShort;
        //    }

        //    int readOffset = 0;
        //    byte b;
        //    int state = 0;
        //    bool foundStart = false;

        //    while (true)
        //    {
        //        b = data[readOffset++];

        //        switch (state)
        //        {
        //            case 0:
        //                if (b == 'D')
        //                {
        //                    state++;
        //                }
        //                else
        //                {
        //                    state = 0;
        //                }
        //                break;

        //            case 1:
        //                if (b == 'M')
        //                {
        //                    state++;
        //                }
        //                else
        //                {
        //                    state = 0;
        //                }
        //                break;

        //            case 2:
        //                if (b == 'A')
        //                {
        //                    state++;
        //                }
        //                else
        //                {
        //                    state = 0;
        //                }
        //                break;

        //            case 3:
        //                if (b == '@')
        //                {
        //                    state++;
        //                }
        //                else
        //                {
        //                    state = 0;
        //                }
        //                break;
        //        }

        //        if (state == 4)
        //        {
        //            foundStart = true;
        //            break;
        //        }

        //        if (readOffset > data.Length - ProtocolHeaderSize)
        //        {
        //            result = new UnknownPacket(data);

        //            return PacketParseResult.HeaderNotFound;
        //        }
        //    }

        //    if (!foundStart)
        //    {
        //        result = new UnknownPacket(data);

        //        return PacketParseResult.HeaderNotFound;
        //    }

        //    int packetSize = 0;

        //    PacketType dataType = (PacketType)(int)data[readOffset++];

        //    packetSize |= data[readOffset++] << 16;
        //    packetSize |= data[readOffset++] << 8;
        //    packetSize |= data[readOffset++] << 0;

        //    if (readOffset - ProtocolHeaderSize + packetSize > data.Length - ProtocolTailSize)
        //    {
        //        result = new UnknownPacket(data);

        //        return PacketParseResult.SizeMismatch;
        //    }

        //    var readData = new byte[packetSize];
        //    Array.Copy(data, readOffset, readData, 0, packetSize);

        //    readOffset += packetSize;

        //    bool foundEnd = IsProtocolTail(data, readOffset);

        //    if (!foundEnd)
        //    {
        //        result = new UnknownPacket(data);

        //        return PacketParseResult.InvalidTail;
        //    }

        //    if (dataType == PacketType.Text)
        //    {
        //        result = new TextPacket(readData.ToArray());
        //    }
        //    else if (dataType == PacketType.HeartBeat)
        //    {
        //        result = new HeartbeartPacket(readData.ToArray());
        //    }
        //    else
        //    {
        //        result = new Packet(dataType, readData.ToArray());
        //    }

        //    return PacketParseResult.Success;
        //}

        private static bool IsProtocolTail(byte[] data, int index)
        {
            if (index + 4 > data.Length)
            {
                return false;
            }

            if (data[index + 0] == 'C'
                && data[index + 1] == 'M'
                && data[index + 2] == 'P'
                && data[index + 3] == 'H')
            {
                return true;
            }

            return false;
        }
    }
}
