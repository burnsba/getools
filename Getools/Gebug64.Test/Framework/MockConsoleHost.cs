using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public MockConsoleHost(SerialPortFactory factory)
        {
            SerialPortProvider = new SerialPortProvider(factory);

            var consolePort = SerialPortProvider.CreatePort(ConsoleSerialPortName);
            var pcPort = SerialPortProvider.CreatePort(PcSerialPortName);
            SerialPortProvider.ConnectPorts(consolePort, pcPort);

            ConsoleSerialPort = consolePort;

            ConsoleSerialPort.DataReceived += SerialPort_DataReceived;
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
            System.Threading.Thread.Sleep(1000);

            byte[] data;

            lock (_lock)
            {
                data = _readBuffer.ToArray();
            }

            ConsoleSerialPort.Write(data, 0, data.Length);

            lock (_lock)
            {
                _readBuffer.RemoveRange(0, data.Length);
            }
        }
    }
}
