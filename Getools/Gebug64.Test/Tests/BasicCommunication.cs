using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Test.Framework;
using Gebug64.Unfloader.Manage;
using Gebug64.Unfloader.Protocol.Flashcart;
using Gebug64.Unfloader.SerialPort;
using Microsoft.Extensions.DependencyInjection;
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

        [Fact]
        public void EverdriveTestCommandResponse()
        {
            var flashcart = new Everdrive(ConsoleHost.SerialPortProvider);
            var csp = new ConnectionServiceProvider(flashcart);
            csp.Start(MockConsoleHost.PcSerialPortName);

            Assert.Empty(flashcart.ReadPackets);

            var serialPort = ConsoleHost.SerialPortProvider.GetPort(MockConsoleHost.PcSerialPortName);

            // This is an UNFLoader Text packet of length 1, inside an Everdrive packet.
            serialPort.Write(new byte[] { (byte)'D', (byte)'M', (byte)'A', (byte)'@', 1, 0, 0, 1, (byte)'A', (byte)'C', (byte)'M', (byte)'P', (byte)'H' }, 0, 13);

            System.Threading.Thread.Sleep(2000);

            Assert.NotEmpty(flashcart.ReadPackets);
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
