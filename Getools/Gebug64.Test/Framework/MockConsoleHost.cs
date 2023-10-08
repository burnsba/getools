using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Flashcart;
using Gebug64.Unfloader.Protocol.Flashcart.Message;
using Gebug64.Unfloader.Protocol.Gebug;
using Gebug64.Unfloader.Protocol.Gebug.Message;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Unfloader.Protocol.Parse;
using Gebug64.Unfloader.Protocol.Unfloader;
using Gebug64.Unfloader.SerialPort;
using Microsoft.Extensions.DependencyInjection;

namespace Gebug64.Test.Framework
{
    public class MockConsoleHost
    {
        public const string ConsoleSerialPortName = "CONSOLE_COM1";
        public const string PcSerialPortName = "PC_COM1";

        private List<byte> _readBuffer = new List<byte>();
        private object _lock = new();

        public ISerialPort ConsoleSerialPort { get; set; }
        public SerialPortProvider SerialPortProvider { get; set; }

        public bool EchoData { get; set; } = false;
        public bool InEverdriveMenu { get; set; } = false;

        public MockConsoleHost(SerialPortFactory factory)
        {
            SerialPortProvider = new SerialPortProvider(factory);

            var consolePort = SerialPortProvider.CreatePort(ConsoleSerialPortName);
            var pcPort = SerialPortProvider.CreatePort(PcSerialPortName);
            SerialPortProvider.ConnectPorts(consolePort, pcPort);

            ConsoleSerialPort = consolePort;

            ConsoleSerialPort.DataReceived += SerialPort_DataReceived;
        }

        /// <summary>
        /// Send a UNFLoader text packet containing the supplied text.
        /// </summary>
        /// <param name="text"></param>
        public void SyncPrint(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new NullReferenceException();
            }

            if (text.Length > 255)
            {
                throw new Exception("Fix text, can't support string length > 255");
            }

            byte len = (byte)text.Length;

            var datas = new List<byte[]>();
            
            // everdrive packet header
            datas.Add(new byte[] { (byte)'D', (byte)'M', (byte)'A', (byte)'@' });
            
            // UNFLoader packet, type: text, length
            datas.Add(new byte[] { 1, 0, 0, len });

            // actual data to send
            datas.Add(System.Text.Encoding.ASCII.GetBytes(text));

            // everdrive packet tail
            datas.Add(new byte[] { (byte)'C', (byte)'M', (byte)'P', (byte)'H' });

            var tosend = datas.SelectMany(x => x).ToArray();

            ConsoleSerialPort.Write(tosend, 0, tosend.Length);
        }

        private void SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            byte[] data = new byte[ConsoleSerialPort!.BytesToRead];
            ConsoleSerialPort.Read(data, 0, data.Length);

            lock (_lock)
            {
                _readBuffer.AddRange(data);
            }

            Task.Run(() => ProcessReadData());
        }

        private void ProcessReadData()
        {
            // delay and echo
            System.Threading.Thread.Sleep(100);

            byte[] data;

            lock (_lock)
            {
                data = _readBuffer.ToArray();
                _readBuffer.RemoveRange(0, data.Length);
            }

            if (EchoData)
            {
                ConsoleSerialPort.Write(data, 0, data.Length);
                return;
            }

            GenerateSendResponse(data);
        }

        private void SendPacket(GebugPacket packet)
        {
            var data = MakeEverdriveUnfloaderBinaryPacket(packet.ToByteArray());
            ConsoleSerialPort.Write(data, 0, data.Length);
        }

        private void SendBytes(byte[] data)
        {
            ConsoleSerialPort.Write(data, 0, data.Length);
        }

        private void GenerateSendResponse(byte[] readData)
        {
            if (InEverdriveMenu)
            {
                if (readData.Length >= 4 && readData.Length <= 16)
                {
                    if (readData[0] == 'c'
                        && readData[1] == 'm'
                        && readData[2] == 'd'
                        && readData[3] == 't')
                    {
                        SendBytes((byte[])EverdriveCmdTestResponse.CommandBytes.Clone());
                        return;
                    }
                }
            }
            else // "in ROM"
            {
                var parse = EverdrivePacket.TryParse(readData.ToList());
                if (parse.ParseStatus == Unfloader.Protocol.Parse.PacketParseStatus.Success)
                {
                    var receivePacket = parse.Packet;

                    if (typeof(IUnfloaderPacket).IsAssignableFrom(receivePacket!.InnerType))
                    {
                        IUnfloaderPacket unfPacket = (IUnfloaderPacket)receivePacket.InnerData!;

                        // if this is gebug ROM message, don't care about UNFLoader wrapper any more
                        var unfInnerData = unfPacket.GetInnerPacket();
                        var gebugParse = GebugPacket.TryParse(unfInnerData.ToList());

                        if (gebugParse.ParseStatus == PacketParseStatus.Success)
                        {
                            var replyPackets = MakeReplyPackets(gebugParse.Packet!);

                            foreach (var packet in replyPackets)
                            {
                                SendPacket(packet);

                                // simulate  delay ...
                                System.Threading.Thread.Sleep(10);
                            }

                            return;
                        }
                    }
                }
            }

            throw new NotSupportedException("Add logic in test case to send response or do nothing.");
        }

        private List<GebugPacket> MakeReplyPackets(GebugPacket packet)
        {
            switch (packet!.Category)
            {
                case GebugMessageCategory.Meta:
                    switch (packet.Command)
                    {
                        case (int)GebugCmdMeta.Ping:
                            {
                                var messageId = GebugPacket.GetRandomMessageId();

                                ushort flags = (ushort)(packet.Flags | (ushort)GebugMessageFlags.IsAck);

                                var reply = packet with
                                {
                                    Flags = flags,
                                    PacketNumber = 1,
                                    TotalNumberPackets = 1,
                                    MessageId = messageId,
                                    AckId = packet.MessageId,
                                };

                                return new List<GebugPacket>() { reply };
                            }
                        case (int)GebugCmdMeta.Version:
                            {
                                var messageId = GebugPacket.GetRandomMessageId();

                                ushort flags = (ushort)(packet.Flags | (ushort)GebugMessageFlags.IsAck);

                                var replyPacketBase = packet with
                                {
                                    Flags = flags,
                                    PacketNumber = 1,
                                    TotalNumberPackets = 1,
                                    MessageId = messageId,
                                    AckId = packet.MessageId,
                                };

                                var replyMessage = (GebugMetaVersionMessage)GebugMessage.FromPacket(
                                    replyPacketBase,
                                    // PcToConsole: Load header contents, but skip reading property values from body byte array.
                                    Unfloader.Protocol.Gebug.Parameter.ParameterUseDirection.PcToConsole);
                                    
                                replyMessage.VersionA = 0x11223344;
                                replyMessage.VersionB = 0x22222222;
                                replyMessage.VersionC = 0x44444444;
                                replyMessage.VersionD = 0x00888888;

                                // Need to specify ConsoleToPc to load correct properties.
                                // Make sure data is in big endien format.
                                var reply = replyMessage.ToSendPackets(Unfloader.Protocol.Gebug.Parameter.ParameterUseDirection.ConsoleToPc);
                                return reply!;
                            }
                            break;
                    }
                    break;

                case GebugMessageCategory.Vi:
                    {
                        switch (packet.Command)
                        {
                            case (int)GebugCmdVi.GrabFramebuffer:
                                {
                                    var messageId = GebugPacket.GetRandomMessageId();

                                    ushort flags = (ushort)(packet.Flags | (ushort)GebugMessageFlags.IsAck);

                                    var replyPacketBase = packet with
                                    {
                                        Flags = flags,
                                        PacketNumber = 1,
                                        TotalNumberPackets = 1,
                                        MessageId = messageId,
                                        AckId = packet.MessageId,
                                    };

                                    var replyMessage = (GebugViFramebufferMessage)GebugMessage.FromPacket(
                                        replyPacketBase,
                                        // PcToConsole: Load header contents, but skip reading property values from body byte array.
                                        Unfloader.Protocol.Gebug.Parameter.ParameterUseDirection.PcToConsole);

                                    // Arbitrary values, just want a multi-packet message (larger than single packet size)
                                    replyMessage.Width = (ushort)36;
                                    replyMessage.Height = (ushort)42;

                                    int size = 36 * 42;
                                    var data = new byte[size];
                                    for (int i = 0; i < size; i++)
                                    {
                                        data[i] = (byte)(i % 255);
                                    }

                                    replyMessage.Data = data;

                                    // Need to specify ConsoleToPc to load correct properties.
                                    // Make sure data is in big endien format.
                                    var reply = replyMessage.ToSendPackets(Unfloader.Protocol.Gebug.Parameter.ParameterUseDirection.ConsoleToPc);
                                    return reply!;
                                }
                                break;
                        }
                    }
                    break;
            }

            throw new NotImplementedException();
        }

        private byte[] MakeEverdriveUnfloaderBinaryPacket(byte[] data)
        {
            var datas = new List<byte[]>();

            // everdrive packet header
            datas.Add(new byte[] { (byte)'D', (byte)'M', (byte)'A', (byte)'@' });

            // UNFLoader packet, type: binary, length
            datas.Add(new byte[]
                {
                    2,
                    (byte)(data.Length >> 16),
                    (byte)(data.Length >> 8),
                    (byte)(data.Length >> 0)
                });

            // actual data to send
            datas.Add(data);

            // everdrive packet tail
            datas.Add(new byte[] { (byte)'C', (byte)'M', (byte)'P', (byte)'H' });

            return datas.SelectMany(x => x).ToArray();
        }
    }
}
