using Gebug64.FakeConsole.Lib.Flashcart;
using Gebug64.FakeConsole.Lib.SerialPort;
using Gebug64.Unfloader.Protocol;
using Gebug64.Unfloader.Protocol.Flashcart;
using Gebug64.Unfloader.Protocol.Flashcart.Message;
using Gebug64.Unfloader.Protocol.Gebug;
using Gebug64.Unfloader.Protocol.Gebug.Message;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Unfloader.Protocol.Unfloader;
using Gebug64.Unfloader.Protocol.Unfloader.Message;
using Gebug64.Unfloader.SerialPort;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Gebug64.FakeConsole.Lib.FakeConsole
{
    public abstract class FakeConsoleBase : IFakeConsole
    {
        /// <summary>
        /// Baseline memory size.
        /// </summary>
        private const int BaseMemSizeBytes = 0x00400000;

        /// <summary>
        /// Size in bytes if expansion pak is installed.
        /// </summary>
        private const int ExpansionPakMemSizeBytes = 0x00800000;

        private ISerialPort _serialPort;
        private IFakeFlashcart _flashcart;
        private Action? _serialPortNotifyDataReady = null;
        private Action<IEncapsulate>? _serialPortEnqueue = null;

        public FakeConsoleBase(ISerialPort serialPort, IFakeFlashcart flashcart)
        {
            _serialPort = serialPort;
            _flashcart = flashcart;

            if (serialPort is FakeSerialPort fsp)
            {
                fsp.ReadEventHandler = ReadEventHandler;
                fsp.WriteEventHandler = WriteEventHandler;

                _serialPortNotifyDataReady = () => fsp.NotifyDataReadToSend(this);
                _serialPortEnqueue = fsp.SendEnqueue;
            }
        }

        protected virtual byte[] MutatePacket(GebugPacket packet)
        {
            return packet.ToByteArray();
        }

        protected virtual byte[] MutatePacket(IUnfloaderPacket packet)
        {
            return packet.GetOuterPacket();
        }

        // read bytes from fake console back to pc
        private void ReadEventHandler(byte[] buffer, int offset, int count)
        {
            _serialPort.Read(buffer, offset, count);
        }

        // write from pc to fake console
        private void WriteEventHandler(byte[] buffer, int offset, int count)
        {
            var dataList = buffer.ToList();
            var result = _flashcart.TryParse(dataList);

            IfReceiveThenSend(result);
        }

        private void IfReceiveThenSend(FlashcartPacketParseResult receive)
        {
            IEncapsulate? sendPacket = null;

            if (receive.ParseStatus == Unfloader.Protocol.Parse.PacketParseStatus.Success)
            {
                if (receive.Packet is EverdriveCmdTestSend)
                {
                    sendPacket = new EverdriveCmdTestResponse();
                }
                else
                {
                    if (receive.Packet is EverdrivePacket)
                    {
                        if (receive.Packet.InnerData is UnfloaderPacket unfPacket)
                        {
                            if (unfPacket is BinaryPacket bpacket)
                            {
                                var gebugParseResult = GebugPacket.TryParse(unfPacket.GetInnerPacket().ToList());

                                IfReceiveThenSend(gebugParseResult);
                                return;
                            }
                        }
                    }
                }
            }

            if (object.ReferenceEquals(null, sendPacket))
            {
                return;
            }

            if (!object.ReferenceEquals(null, _serialPortEnqueue))
            {
                _serialPortEnqueue(sendPacket);
            }

            if (!object.ReferenceEquals(null, _serialPortNotifyDataReady))
            {
                _serialPortNotifyDataReady();
            }
        }

        private void IfReceiveThenSend(GebugPacketParseResult receive)
        {
            List<GebugPacket> sendPackets = new List<GebugPacket>();

            if (receive.ParseStatus == Unfloader.Protocol.Parse.PacketParseStatus.Success
                && !object.ReferenceEquals(null, receive.Packet))
            {
                var packet = receive.Packet;

                if (packet.Category == GebugMessageCategory.Meta)
                {
                    if (packet.Command == (byte)GebugCmdMeta.Ping)
                    {
                        var msg = new GebugMetaPingMessage();
                        msg.ReplyTo(packet);
                        sendPackets = msg.ToSendPackets(Unfloader.Protocol.Gebug.Parameter.ParameterUseDirection.ConsoleToPc);
                    }
                    else if (packet.Command == (byte)GebugCmdMeta.Version)
                    {
                        var msg = new GebugMetaVersionMessage()
                        {
                            // sonic cheat code sound effects
                            VersionA = 1965,
                            VersionB = 9,
                            VersionC = 17,
                            // still 4294967295 available values
                            VersionD = 1,
                            // size value here determines whether gebug app thinks expansion pack is installed
                            Size = ExpansionPakMemSizeBytes,
                        };
                        msg.ReplyTo(packet);
                        sendPackets = msg.ToSendPackets(Unfloader.Protocol.Gebug.Parameter.ParameterUseDirection.ConsoleToPc);
                    }
                }
                else if (packet.Category == GebugMessageCategory.Misc)
                {
                    if (packet.Command == (byte)GebugCmdMisc.OsTime)
                    {
                        var msg = new GebugMiscOsTimeMessage();
                        msg.ReplyTo(packet);
                        msg.Count =
                            (uint)(DateTime.Now.Second * 1000000)
                            + (uint)(DateTime.Now.Millisecond * 1000)
                            + (uint)DateTime.Now.Microsecond;
                        sendPackets = msg.ToSendPackets(Unfloader.Protocol.Gebug.Parameter.ParameterUseDirection.ConsoleToPc);
                    }
                    else if (packet.Command == (byte)GebugCmdMisc.OsMemSize)
                    {
                        var msg = new GebugMiscOsGetMemSizeMessage();
                        msg.ReplyTo(packet);
                        msg.Size = ExpansionPakMemSizeBytes;
                        sendPackets = msg.ToSendPackets(Unfloader.Protocol.Gebug.Parameter.ParameterUseDirection.ConsoleToPc);
                    }
                }
            }

            if (!sendPackets.Any())
            {
                return;
            }

            if (!object.ReferenceEquals(null, _serialPortEnqueue))
            {
                foreach (var p in sendPackets)
                {
                    var gebugBytes = MutatePacket(p);
                    IUnfloaderPacket unfpacket = new BinaryPacket(gebugBytes);

                    var unfBytes = MutatePacket(unfpacket);
                    var flashcartPacket = _flashcart.PackageToSend(unfBytes);
                    _serialPortEnqueue(flashcartPacket);
                }
            }

            if (!object.ReferenceEquals(null, _serialPortNotifyDataReady))
            {
                _serialPortNotifyDataReady();
            }
        }
    }
}
