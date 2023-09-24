using System;
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
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.ObjectModel;
using Xunit;

namespace Gebug64.Test.Tests
{
    public class BasicCommunication
    {
        protected MockConsoleHost ConsoleHost { get; set; }

        public BasicCommunication(MockConsoleHost consoleHost)
        {
            ConsoleHost = consoleHost;
        }

        /// <summary>
        /// Disable service provider manage thread.
        /// Write a UNFLoader packet directly into the raw serial port.
        /// Check that any response exists.
        /// </summary>
        [Fact]
        public void EverdriveTest_MockConsoleHost_Echo_Direct_ToFlashcart()
        {
            var flashcart = new Everdrive(ConsoleHost.SerialPortProvider);
            var csp = new ConnectionServiceProvider(flashcart);
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
            var flashcart = new Everdrive(ConsoleHost.SerialPortProvider);
            var csp = new ConnectionServiceProvider(flashcart);
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
            var flashcart = new Everdrive(ConsoleHost.SerialPortProvider);
            var csp = new ConnectionServiceProvider(flashcart);
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
            var flashcart = new Everdrive(ConsoleHost.SerialPortProvider);
            var csp = new ConnectionServiceProvider(flashcart);
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
            var flashcart = new Everdrive(ConsoleHost.SerialPortProvider);
            var csp = new ConnectionServiceProvider(flashcart);
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
            var flashcart = new Everdrive(ConsoleHost.SerialPortProvider);
            var csp = new ConnectionServiceProvider(flashcart);
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

        private IFlashcartPacket ReadFlascartPacket<T>(IFlashcart flashcart)
        {
            var sw = Stopwatch.StartNew();
            IFlashcartPacket flashcartPacket;
            while (!flashcart.ReadPackets.TryDequeue(out flashcartPacket))
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
                var typeGetter = new SerialPortFactoryTypeGetter() { Type = typeof(VirtualSerialPort) };
                services.AddSingleton<SerialPortFactoryTypeGetter>(typeGetter);

                services.AddTransient<SerialPortFactory>();
                services.AddTransient<MockConsoleHost>();
            }
        }
    }
}
