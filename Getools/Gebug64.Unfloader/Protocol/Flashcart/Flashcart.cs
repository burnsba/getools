using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Parse;
using Gebug64.Unfloader.SerialPort;

namespace Gebug64.Unfloader.Protocol.Flashcart
{
    public abstract class Flashcart : IFlashcart
    {
        private readonly SerialPortProvider _portProvider;
        private ISerialPort? _serialPort;
        private List<byte> _readData = new List<byte>();
        private object _readLock = new object();
        private ConcurrentQueue<IFlashcartPacket> _readPackets = new ConcurrentQueue<IFlashcartPacket>();

        public bool IsConnected => _serialPort?.IsOpen ?? false;

        protected abstract FlashcartPacketParseResult TryParse(List<byte> data);
        protected abstract IFlashcartPacket MakePacket(byte[] data);

        public ConcurrentQueue<IFlashcartPacket> ReadPackets => _readPackets;

        public Flashcart(SerialPortProvider portProvider)
        {
            _portProvider = portProvider;
        }

        public void Connect(string port)
        {
            if (IsConnected)
            {
                Disconnect();
            }

            _serialPort = _portProvider.CreatePort(port);
            _serialPort.DataReceived += DataReceived;
            _serialPort.Open();
        }

        public void Disconnect()
        {
            if (!object.ReferenceEquals(_serialPort, null))
            {
                _serialPort.DataReceived -= DataReceived;

                _serialPort.Close();
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();

                _serialPort = null;
            }
        }

        public void SendRom() { }

        public void Send(byte[] data)
        {
            Send(MakePacket(data));
        }

        public void Send(IFlashcartPacket packet)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException($"Call {nameof(Connect)} first");
            }

            var data = packet.GetOuterPacket();
            _serialPort.Write(data, 0, data.Length);
        }

        public bool TestInMenu() { return false; }

        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] data = new byte[_serialPort!.BytesToRead];
            _serialPort.Read(data, 0, data.Length);
            lock (_readLock)
            {
                _readData.AddRange(data);
            }

            Task.Run(() => TryReadPacket());
        }

        private void TryReadPacket()
        {
            List<byte> readCopy;
            List<IFlashcartPacket> toQueue = new();

            lock (_readLock)
            {
                readCopy = _readData.ToArray().ToList();
            }

            int removeSize = 0;

            while (readCopy.Count > 0)
            {
                var parse = TryParse(readCopy);
                if (parse.ParseStatus == PacketParseStatus.Success)
                {
                    readCopy.RemoveRange(0, parse.TotalBytesRead);
                    removeSize += parse.TotalBytesRead;

                    toQueue.Add(parse.Packet!);
                }
                else
                {
                    // not a flashcart packet, or haven't received enough data.
                    break;
                }
            }

            lock (_readLock)
            {
                _readData.RemoveRange(0, removeSize);
            }

            foreach (var packet in toQueue)
            {
                _readPackets.Enqueue(packet);
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
