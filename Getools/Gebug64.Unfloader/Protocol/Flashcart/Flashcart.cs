using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug;
using Gebug64.Unfloader.Protocol.Parse;
using Gebug64.Unfloader.SerialPort;

namespace Gebug64.Unfloader.Protocol.Flashcart
{
    public abstract class Flashcart : IFlashcart
    {
        private readonly SerialPortProvider _portProvider;
        private ISerialPort? _serialPort;
        private List<byte> _readData = new List<byte>();
        private Mutex _readLock = new();
        private ConcurrentQueue<IFlashcartPacket> _readPackets = new ConcurrentQueue<IFlashcartPacket>();

        public bool IsConnected => _serialPort?.IsOpen ?? false;

        protected MessageBus<IFlashcartPacket> _flashcartPacketBus = new();

        protected abstract FlashcartPacketParseResult TryParse(List<byte> data);
        protected abstract IFlashcartPacket MakePacket(byte[] data);

        public ConcurrentQueue<IFlashcartPacket> ReadPackets => _readPackets;

        public Flashcart(SerialPortProvider portProvider)
        {
            _portProvider = portProvider;

            _flashcartPacketBus.Subscribe(SubscriptionEnqueuePacket);
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

        public abstract void SendRom(byte[] filedata, Nullable<CancellationToken> token = null);

        // deprecated this ?
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
            _serialPort!.Write(data, 0, data.Length);
        }

        public virtual bool TestInMenu() { return false; }

        protected void WriteRaw(byte[] data)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException($"Call {nameof(Connect)} first");
            }

            _serialPort!.Write(data, 0, data.Length);
        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] data = new byte[_serialPort!.BytesToRead];
            _serialPort.Read(data, 0, data.Length);

            _readLock.WaitOne();

            try
            {
                _readData.AddRange(data);
            }
            finally
            {
                _readLock.ReleaseMutex();
            }

            Task.Run(() => TryReadPacket());
        }

        private void TryReadPacket()
        {
            List<byte> readCopy;
            List<IFlashcartPacket> toQueue = new();

            // I was previously locking the buffer, copying the data, unlock, then parsing,
            // then relock, and remove data in range.
            // But there are occasionally race condition errors if two (or more) packets
            // show up but the first takes longer to parse than the second.
            // So just lock the whole thing while parsing.
            while (true)
            {
                if (!_readLock.WaitOne(10))
                {
                    // If another thread clears the read buffer while this one is waiting
                    // then there's nothing to do.
                    if (!_readData.Any())
                    {
                        return;
                    }
                }
                else
                {
                    break;
                }
            }

            try
            {
                readCopy = _readData.ToArray().ToList();

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

                _readData.RemoveRange(0, removeSize);
            }
            finally
            {
                _readLock.ReleaseMutex();
            }

            foreach (var packet in toQueue)
            {
                _flashcartPacketBus.Publish(packet);
            }
        }

        public void Dispose()
        {
            Disconnect();
        }

        private void SubscriptionEnqueuePacket(IFlashcartPacket packet)
        {
            _readPackets.Enqueue(packet);
        }
    }
}
