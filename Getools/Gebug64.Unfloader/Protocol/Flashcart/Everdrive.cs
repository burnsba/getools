using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Flashcart.Message;
using Gebug64.Unfloader.Protocol.Parse;
using Gebug64.Unfloader.Protocol.Unfloader;
using Gebug64.Unfloader.Protocol.Unfloader.Message;
using Gebug64.Unfloader.SerialPort;

namespace Gebug64.Unfloader.Protocol.Flashcart
{
    public class Everdrive : Flashcart
    {
        private const int WriteRomTargetAddress = 0x10000000;

        private const string Command_Test_Send = "cmdt";
        private const string Command_Test_Response = "cmdr";
        private const string Command_WriteRom = "cmdW";
        private const string Command_PifBoot_Send = "cmds";

        public const int ProtocolHeaderSize = 4;
        public const int ProtocolTailSize = 4;

        /// <summary>
        /// Size of packet is offset by header/tail byte length.
        /// </summary>
        public const int ProtocolByteLength = ProtocolHeaderSize + ProtocolTailSize;

        public Everdrive(SerialPortProvider portProvider)
          : base(portProvider)
        {
        }

        protected override FlashcartPacketParseResult TryParse(List<byte> data)
        {
            var result = new FlashcartPacketParseResult()
            {
                ParseStatus = PacketParseStatus.DefaultUnknown,
            };

            if (object.ReferenceEquals(null, data))
            {
                throw new NullReferenceException();
            }

            if (data.Count < ProtocolHeaderSize)
            {
                result.ParseStatus = PacketParseStatus.Error;
                result.ErrorReason = PacketParseReason.DataTooShort;
                return result;
            }

            // check for everdrive specific system messages first.
            EverdriveCmd? systemCmd = null;
            if (data.Count <= 16)
            {
                systemCmd = EverdriveCmd.ParseSystemCommand(data.ToArray());
            }

            // If the everdrive system parse command succeeded above, it will have used at most 16 bytes.
            // Take that and return as the result.
            if (systemCmd != null)
            {
                var systemBytesRead = data.Count;
                if (systemBytesRead > 16)
                {
                    systemBytesRead = 16;
                }

                result.TotalBytesRead = systemBytesRead;
                result.Packet = systemCmd;
                result.ParseStatus = PacketParseStatus.Success;

                return result;
            }

            // Not a system command, so it should have the tail.
            if (data.Count < ProtocolByteLength)
            {
                result.ParseStatus = PacketParseStatus.Error;
                result.ErrorReason = PacketParseReason.DataTooShort;
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

                if (readOffset > data.Count - ProtocolHeaderSize)
                {
                    result.ParseStatus = PacketParseStatus.Error;
                    result.ErrorReason = PacketParseReason.HeaderNotFound;
                    return result;
                }
            }

            if (!foundStart)
            {
                result.ParseStatus = PacketParseStatus.Error;
                result.ErrorReason = PacketParseReason.HeaderNotFound;
                return result;
            }

            // normally the everdrive shouldn't care about the inner UNFLoader packet,
            // except that we need to know how many bytes to read to check for the tail
            // of the everdrive packet.

            var remain = data.Skip(readOffset).ToList();
            UnfloaderPacketParseResult unfloaderParse = UnfloaderPacket.TryParse(remain);

            if (unfloaderParse.ParseStatus == PacketParseStatus.Success)
            {
                readOffset += unfloaderParse.TotalBytesRead;
                bool foundEnd = IsProtocolTail(data, readOffset);

                if (!foundEnd)
                {
                    result.ParseStatus = PacketParseStatus.Error;
                    result.ErrorReason = PacketParseReason.InvalidTail;
                    return result;
                }

                readOffset += ProtocolTailSize;
                result.TotalBytesRead = readOffset;
            }
            else
            {
                result.ParseStatus = PacketParseStatus.Error;
                result.ErrorReason = PacketParseReason.InvalidTail;
                return result;
            }

            result.Packet = new EverdrivePacket(unfloaderParse.Packet!.GetOuterPacket())
            {
                InnerData = unfloaderParse.Packet!,
                InnerType = unfloaderParse.Packet.GetType(),
            };

            result.ParseStatus = PacketParseStatus.Success;

            return result;
        }

        protected override IFlashcartPacket MakePacket(byte[] data)
        {
            return new EverdrivePacket(data);
        }

        private static bool IsProtocolTail(List<byte> data, int index)
        {
            if (index + 4 > data.Count)
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
