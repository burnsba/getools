using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Flashcart.Message;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
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

            var writeResponse = GenerateResponse(data);

            if (writeResponse != null)
            {
                ConsoleSerialPort.Write(writeResponse, 0, writeResponse.Length);
            }
        }

        private byte[]? GenerateResponse(byte[] readData)
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
                        return (byte[])EverdriveCmdTestResponse.CommandBytes.Clone();
                    }
                }
            }
            else // "in ROM"
            {
                if (readData.Length > 4)
                {
                    if (readData[0] == 'D'
                        && readData[1] == 'M'
                        && readData[2] == 'A'
                        && readData[3] == '@'
                        // UNFLoader packet type Binary
                        && readData[4] == 2)
                    {
                        if (readData.Length > 8)
                        {
                            switch (readData[8])
                            {
                                case (byte)GebugMessageCategory.Meta:
                                    if (readData.Length > 9)
                                    {
                                        if (readData[9] == (byte)GebugCmdMeta.Ping)
                                        {
                                            return MakeBinaryPacket(new byte[] { (byte)GebugMessageCategory.Ack });
                                        }
                                    }
                                break;
                            }
                        }
                    }
                }
            }

            return null;
        }

        private byte[] MakeBinaryPacket(byte[] data)
        {
            var datas = new List<byte[]>();

            // everdrive packet header
            datas.Add(new byte[] { (byte)'D', (byte)'M', (byte)'A', (byte)'@' });

            if (data.Length > 255)
            {
                throw new Exception("Fix test, packet length is wrong");
            }

            // UNFLoader packet, type: binary, length
            datas.Add(new byte[] { 2, 0, 0, (byte)data.Length });

            // actual data to send
            datas.Add(data);

            // everdrive packet tail
            datas.Add(new byte[] { (byte)'C', (byte)'M', (byte)'P', (byte)'H' });

            return datas.SelectMany(x => x).ToArray();
        }
    }
}
