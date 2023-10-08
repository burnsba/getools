using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Test.Framework;
using Gebug64.Unfloader.Manage;
using Gebug64.Unfloader.Protocol.Flashcart;
using Gebug64.Unfloader.Protocol.Flashcart.Message;
using Gebug64.Unfloader.Protocol.Gebug;
using Gebug64.Unfloader.Protocol.Gebug.Message;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Unfloader.Protocol.Unfloader;
using Gebug64.Unfloader.Protocol.Unfloader.Message;
using Gebug64.Unfloader.Protocol.Unfloader.Message.MessageType;
using Gebug64.Unfloader.SerialPort;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.ObjectModel;
using Xunit;

namespace Gebug64.Test.Tests
{
    public class BasicCommunication
    {
        private ILogger _logger;

        protected MockConsoleHost ConsoleHost { get; set; }

        public BasicCommunication(MockConsoleHost consoleHost)
        {
            ConsoleHost = consoleHost;

            _logger = new Getools.Utility.Logging.Logger();
        }

        /// <summary>
        /// Disable service provider manage thread.
        /// Write a UNFLoader packet directly into the raw serial port.
        /// Check that any response exists.
        /// </summary>
        [Fact]
        public void EverdriveTest_MockConsoleHost_Echo_Direct_ToFlashcart()
        {
            var flashcart = new Everdrive(ConsoleHost.SerialPortProvider, _logger);
            var csp = new ConnectionServiceProvider(flashcart, _logger);
            csp.ManagerActive = false;
            csp.Start(MockConsoleHost.PcSerialPortName);

            Assert.Empty(flashcart.ReadPackets);

            ConsoleHost.EchoData = true;

            var serialPort = ConsoleHost.SerialPortProvider.GetPort(MockConsoleHost.PcSerialPortName);

            // This is an UNFLoader Text packet of length 1, inside an Everdrive packet.
            serialPort.Write(new byte[] { (byte)'D', (byte)'M', (byte)'A', (byte)'@', 1, 0, 0, 1, (byte)'A', (byte)'C', (byte)'M', (byte)'P', (byte)'H' }, 0, 13);

            TimeoutAssert.NotEmpty(flashcart.ReadPackets, TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// Disable service provider manage thread.
        /// Generate a "OsSyncPrintf" event on the console, receieve a text packet.
        /// </summary>
        [Fact]
        public void EverdriveTest_MockConsoleHost_Printf_ToFlashcart()
        {
            var flashcart = new Everdrive(ConsoleHost.SerialPortProvider, _logger);
            var csp = new ConnectionServiceProvider(flashcart, _logger);
            csp.ManagerActive = false;
            csp.Start(MockConsoleHost.PcSerialPortName);

            Assert.Empty(flashcart.ReadPackets);

            string msg = "bossEntry done.\n";
            ConsoleHost.SyncPrint(msg);

            TimeoutAssert.NotEmpty(flashcart.ReadPackets, TimeSpan.FromSeconds(1));

            // test will fail if this fails
            var everdrivePacket = (EverdrivePacket)ReadFlascartPacket<EverdrivePacket>(flashcart);

            Assert.NotNull(everdrivePacket.InnerType);
            Assert.Equal(typeof(TextPacket), everdrivePacket.InnerType);
            Assert.NotNull(everdrivePacket.InnerData);

            var textPacket = (TextPacket)everdrivePacket.InnerData;
            Assert.Equal(msg, textPacket.Content);
            Assert.Equal(UnfloaderMessageType.Text, textPacket.MessageType);
        }

        /// <summary>
        /// Disable service provider manage thread.
        /// Write an everdrive 'cmdt' command directly into the PC serial port.
        /// Check that a <see cref="EverdriveCmdTestResponse"/> is in the incoming flashcart packets.
        /// </summary>
        [Fact]
        public void EverdriveTest_CommandTest_Direct_ToFlashcart()
        {
            var flashcart = new Everdrive(ConsoleHost.SerialPortProvider, _logger);
            var csp = new ConnectionServiceProvider(flashcart, _logger);
            csp.ManagerActive = false;
            csp.Start(MockConsoleHost.PcSerialPortName);

            Assert.Empty(flashcart.ReadPackets);

            ConsoleHost.InEverdriveMenu = true;

            var serialPort = ConsoleHost.SerialPortProvider.GetPort(MockConsoleHost.PcSerialPortName);

            serialPort.Write(new byte[] { (byte)'c', (byte)'m', (byte)'d', (byte)'t', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0, 16);

            TimeoutAssert.NotEmpty(flashcart.ReadPackets, TimeSpan.FromSeconds(1));

            ReadFlascartPacket<EverdriveCmdTestResponse>(flashcart);
        }

        /// <summary>
        /// Using service provider manage thread.
        /// Generate a "OsSyncPrintf" event on the console, receieve a text packet.
        /// Add a subscriber to the service provider that will on text packets,
        /// and toggle a flag once received.
        /// </summary>
        [Fact]
        public void EverdriveTest_MockConsoleHost_Printf()
        {
            var flashcart = new Everdrive(ConsoleHost.SerialPortProvider, _logger);
            var csp = new ConnectionServiceProvider(flashcart, _logger);
            csp.Start(MockConsoleHost.PcSerialPortName);

            Assert.Empty(flashcart.ReadPackets);

            bool unfloaderTextReceived = false;

            Action<IUnfloaderPacket> callback = packet =>
            {
                unfloaderTextReceived = true;
            };

            Func<IUnfloaderPacket, bool> filter = packet =>
            {
                return packet.MessageType == UnfloaderMessageType.Text;
            };

            csp.Subscribe(callback, 1, filter);

            string msg = "bossEntry done.\n";
            ConsoleHost.SyncPrint(msg);

            TimeoutAssert.True(ref unfloaderTextReceived, TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// Using service provider manage thread.
        /// Send gebug message ping and receive response (service provider subscription).
        /// </summary>
        [Fact]
        public void EverdriveTest_Gebug_Ping()
        {
            var flashcart = new Everdrive(ConsoleHost.SerialPortProvider, _logger);
            var csp = new ConnectionServiceProvider(flashcart, _logger);
            csp.Start(MockConsoleHost.PcSerialPortName);

            Assert.Empty(flashcart.ReadPackets);

            GebugMessage sendMesssage = new GebugMetaPingMessage();

            ushort sendMessageId = sendMesssage.MessageId;
            bool messageReceived = false;

            Action<IGebugMessage> callback = packet =>
            {
                messageReceived = true;
            };

            Func<IGebugMessage, bool> filter = message =>
            {
                var firstPacket = message.FirstPacket!;

                return message.Category == GebugMessageCategory.Meta
                    && message.Command == (int)GebugCmdMeta.Ping
                    && (firstPacket.Flags & (ushort)GebugMessageFlags.IsAck) > 0
                    && message.AckId == sendMessageId
                    ;
            };

            csp.Subscribe(callback, 1, filter);

            csp.SendMessage(sendMesssage);

            TimeoutAssert.True(ref messageReceived, TimeSpan.FromSeconds(1), "Did not receive packet matching filter");
        }

        /// <summary>
        /// Using service provider manage thread.
        /// Send gebug message version request and receive response (service provider subscription).
        /// </summary>
        [Fact]
        public void EverdriveTest_Gebug_Version()
        {
            var flashcart = new Everdrive(ConsoleHost.SerialPortProvider, _logger);
            var csp = new ConnectionServiceProvider(flashcart, _logger);
            csp.Start(MockConsoleHost.PcSerialPortName);

            Assert.Empty(flashcart.ReadPackets);

            GebugMessage sendMesssage = new GebugMetaVersionMessage();

            ushort sendMessageId = sendMesssage.MessageId;
            bool messageReceived = false;

            Action<IGebugMessage> callback = packet =>
            {
                messageReceived = true;
            };

            Func<IGebugMessage, bool> filter = message =>
            {
                var firstPacket = message.FirstPacket!;
                var versionMessage = (GebugMetaVersionMessage)message;

                return message.Category == GebugMessageCategory.Meta
                    && message.Command == (int)GebugCmdMeta.Version
                    && (firstPacket.Flags & (ushort)GebugMessageFlags.IsAck) > 0
                    && message.AckId == sendMessageId
                    && versionMessage != null
                    // just some arbitrary numbers used in the send host
                    && versionMessage.VersionA == 0x11223344
                    && versionMessage.VersionB == 0x22222222
                    && versionMessage.VersionC == 0x44444444
                    && versionMessage.VersionD == 0x00888888
                    ;
            };

            csp.Subscribe(callback, 1, filter);

            csp.SendMessage(sendMesssage);

            TimeoutAssert.True(ref messageReceived, TimeSpan.FromSeconds(1), "Did not receive packet matching filter");
        }

        /// <summary>
        /// Tests variable length property.
        /// Tests multi-packet message.
        /// No send/receieve.
        /// </summary>
        [Theory]
        [InlineData(2, 42)] // variable length 0xff
        [InlineData(36, 42)] // variable length 0xfe
        [InlineData(320, 240)] // variable length 0xfd
        public void Gebug_VariableLengthParameter(int expectedWidth, int expectedHeight)
        {
            var sendMesssage = new GebugViFramebufferMessage();

            // Console data will be in big endien format, but PC native format is little endien.
            sendMesssage.Width = (ushort)expectedWidth;
            sendMesssage.Height = (ushort)expectedHeight;

            int size = expectedWidth * expectedHeight;
            var data = new byte[size];
            for (int i = 0; i < size; i++)
            {
                // arbitrary values, just something non zero.
                data[i] = (byte)(i % 255);
            }

            sendMesssage.Data = data;

            var bodyLength = sendMesssage.Data.Length + 4;
            int variableSizeByte = 0;

            // `data` is IsVariableSize, so figure out additional protocol overhead.
            if (sendMesssage.Data.Length > ushort.MaxValue)
            {
                variableSizeByte = 0xfd;
                bodyLength += 4;
            }
            else if (sendMesssage.Data.Length > byte.MaxValue)
            {
                variableSizeByte = 0xfe;
                bodyLength += 2;
            }
            else
            {
                variableSizeByte = 0xff;
                bodyLength += 1;
            }

            var dividePacketSize = GebugPacket.ProtocolMaxBodySizeMulti;

            int totalNumPackets = (bodyLength / (dividePacketSize)) + 1;

            // An even multiple of the buffer size will be off by one due to the addition above.
            if ((bodyLength % (dividePacketSize)) == 0)
            {
                totalNumPackets--;
            }

            // However, want to count a size of zero as exactly one packet.
            if (totalNumPackets == 0)
            {
                totalNumPackets = 1;
            }

            var packets = sendMesssage.ToSendPackets(Unfloader.Protocol.Gebug.Parameter.ParameterUseDirection.ConsoleToPc);

            // Test

            Assert.NotNull(packets);
            Assert.NotEmpty(packets);

            var firstPacket = packets.First();
            var msgId = firstPacket.MessageId;

            ushort packetNumber = 1;
            int bodyOffset = 0;

            ushort width = (ushort)(firstPacket.Body[bodyOffset++] << 8);
            width |= (ushort)(firstPacket.Body[bodyOffset++] << 0);

            Assert.Equal(expectedWidth, width);

            ushort height = (ushort)(firstPacket.Body[bodyOffset++] << 8);
            height |= (ushort)(firstPacket.Body[bodyOffset++] << 0);

            Assert.Equal(expectedHeight, height);

            Assert.Equal(variableSizeByte, firstPacket.Body[bodyOffset++]);

            if (variableSizeByte == 0xfd)
            {
                uint packetDataSize = (uint)(firstPacket.Body[bodyOffset++] << 24);
                packetDataSize |= (uint)(firstPacket.Body[bodyOffset++] << 16);
                packetDataSize |= (uint)(firstPacket.Body[bodyOffset++] << 8);
                packetDataSize |= (uint)(firstPacket.Body[bodyOffset++] << 0);
                Assert.Equal((uint)size, packetDataSize);
            }
            else if (variableSizeByte == 0xfe)
            {
                ushort packetDataSize = (ushort)(firstPacket.Body[bodyOffset++] << 8);
                packetDataSize |= (ushort)(firstPacket.Body[bodyOffset++] << 0);
                Assert.Equal(size, packetDataSize);
            }
            else
            {
                byte packetDataSize = firstPacket.Body[bodyOffset++];
                Assert.Equal(size, packetDataSize);
            }

            int dataIndex = 0;

            foreach (var packet in packets)
            {
                Assert.Equal(GebugMessageCategory.Vi, packet.Category);
                Assert.Equal((int)GebugCmdVi.GrabFramebuffer, packet.Command);

                if ((packet.Flags & (ushort)GebugMessageFlags.IsMultiMessage) > 0)
                {
                    Assert.Equal(packetNumber, packet.PacketNumber);
                    Assert.Equal((ushort)totalNumPackets, packet.TotalNumberPackets);
                }
                else
                {
                    Assert.Null(packet.PacketNumber);
                    Assert.Null(packet.TotalNumberPackets);
                }

                Assert.Equal(3, packet.NumberParameters);
                Assert.Equal(msgId, packet.MessageId);

                if (packet.PacketNumber > 1)
                {
                    bodyOffset = 0;
                }

                for (; bodyOffset < packet.Body.Length; bodyOffset++)
                {
                    Assert.Equal(dataIndex % 255, packet.Body[bodyOffset]);
                    dataIndex++;
                }
                
                packetNumber++;
            }
        }

        /// <summary>
        /// Tests variable length property.
        /// Tests multi-packet message.
        /// Send gebug message vi GrabFramebuffer request and receive response (service provider subscription).
        /// </summary>
        [Fact]
        public void EverdriveTest_Gebug_GetFramebuffer()
        {
            var flashcart = new Everdrive(ConsoleHost.SerialPortProvider, _logger);
            var csp = new ConnectionServiceProvider(flashcart, _logger);
            csp.Start(MockConsoleHost.PcSerialPortName);

            Assert.Empty(flashcart.ReadPackets);

            GebugMessage sendMesssage = new GebugViFramebufferMessage();

            ushort sendMessageId = sendMesssage.MessageId;
            bool messageReceived = false;

            GebugViFramebufferMessage? receiveMessage = null;

            Action<IGebugMessage> callback = packet =>
            {
                messageReceived = true;

                receiveMessage = (GebugViFramebufferMessage)packet;
            };

            Func<IGebugMessage, bool> filter = message =>
            {
                var firstPacket = message.FirstPacket!;
                var versionMessage = (GebugViFramebufferMessage)message;

                return message.Category == GebugMessageCategory.Vi
                    && message.Command == (int)GebugCmdVi.GrabFramebuffer
                    && (firstPacket.Flags & (ushort)GebugMessageFlags.IsAck) > 0
                    && message.AckId == sendMessageId
                    && versionMessage != null
                    // just some arbitrary numbers used in the send host
                    && versionMessage.Width == 36
                    && versionMessage.Height == 42
                    ;
            };

            csp.Subscribe(callback, 1, filter);

            csp.SendMessage(sendMesssage);

            TimeoutAssert.True(ref messageReceived, TimeSpan.FromSeconds(3), "Did not receive packet matching filter");

            int size = receiveMessage!.Width * receiveMessage.Height;

            Assert.NotNull(receiveMessage.Data);
            Assert.Equal(size, receiveMessage.Data.Length);

            for (int i = 0; i < size; i++)
            {
                Assert.Equal(i % 255, receiveMessage.Data[i]);
            }
        }

        /// <summary>
        /// Using service provider manage thread.
        /// </summary>
        [Fact]
        public void EverdriveTest_TestInMenu()
        {
            var flashcart = new Everdrive(ConsoleHost.SerialPortProvider, _logger);
            var csp = new ConnectionServiceProvider(flashcart, _logger);
            csp.Start(MockConsoleHost.PcSerialPortName);

            bool actualReply;

            ConsoleHost.InEverdriveMenu = true;
            actualReply = csp.TestInMenu();

            Assert.True(actualReply);

            ConsoleHost.InEverdriveMenu = false;
            actualReply = csp.TestInMenu();

            Assert.False(actualReply);
        }

        /// <summary>
        /// Using service provider manage thread.
        /// </summary>
        [Fact]
        public void EverdriveTest_TestInRom()
        {
            var flashcart = new Everdrive(ConsoleHost.SerialPortProvider, _logger);
            var csp = new ConnectionServiceProvider(flashcart, _logger);
            csp.Start(MockConsoleHost.PcSerialPortName);

            bool actualReply;

            ConsoleHost.InEverdriveMenu = true;
            actualReply = csp.TestInRom();

            Assert.False(actualReply);

            ConsoleHost.InEverdriveMenu = false;
            actualReply = csp.TestInRom();

            Assert.True(actualReply);
        }

        private IFlashcartPacket ReadFlascartPacket<T>(IFlashcart flashcart)
        {
            var sw = Stopwatch.StartNew();
            IFlashcartPacket flashcartPacket;
            while (!flashcart.ReadPackets.TryDequeue(out flashcartPacket!))
            {
                if (sw.Elapsed.TotalSeconds > 1)
                {
                    Assert.True(false, "Timeout trying to dequeue flashcart");
                }
            }

            if (!typeof(T).IsAssignableFrom(flashcartPacket.GetType()))
            {
                var packetType = flashcartPacket.GetType().Name;
                Assert.True(false, $"Got packet from console, but packet type is {packetType} instead of {typeof(T).Name}");
            }

            return flashcartPacket;
        }

        public class Startup
        {
            public void ConfigureServices(IServiceCollection services)
            {
                var typeGetter = new SerialPortFactoryTypeGetter(typeof(VirtualSerialPort));
                services.AddSingleton<SerialPortFactoryTypeGetter>(typeGetter);

                services.AddTransient<SerialPortFactory>();
                services.AddTransient<MockConsoleHost>();
            }
        }
    }
}
