using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug;
using Gebug64.Unfloader.Protocol.Parse;
using Gebug64.Unfloader.SerialPort;
using Microsoft.Extensions.Logging;

namespace Gebug64.Unfloader.Protocol.Flashcart
{
    public abstract class Flashcart : IFlashcart
    {
        private readonly SerialPortProvider _portProvider;
        private ISerialPort? _serialPort;
        private List<byte> _readData = new List<byte>();
        private Mutex _readLock = new();
        private ConcurrentQueue<IFlashcartPacket> _readPackets = new ConcurrentQueue<IFlashcartPacket>();
        private Stopwatch? _sinceDataReceived = null;
        private Stopwatch? _sinceRomMessageReceived = null;

        protected MessageBus<IFlashcartPacket> _flashcartPacketBus = new();
        protected readonly ILogger _logger;
        protected bool _disableProcessIncoming = false;

        public bool IsConnected => _serialPort?.IsOpen ?? false;

        protected abstract FlashcartPacketParseResult TryParse(List<byte> data);
        protected abstract IFlashcartPacket MakePacket(byte[] data);

        public ConcurrentQueue<IFlashcartPacket> ReadPackets => _readPackets;

        public TimeSpan SinceDataReceived
        {
            get
            {
                if (_sinceDataReceived == null)
                {
                    return TimeSpan.MaxValue;
                }

                return _sinceDataReceived.Elapsed;
            }
        }

        public TimeSpan SinceRomMessageReceived
        {
            get
            {
                if (_sinceRomMessageReceived == null)
                {
                    return TimeSpan.MaxValue;
                }

                return _sinceRomMessageReceived.Elapsed;
            }
        }

        public Flashcart(SerialPortProvider portProvider, ILogger logger)
        {
            _portProvider = portProvider;
            _logger = logger;

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
            _serialPort.ReadTimeout = 1000;
            _serialPort.WriteTimeout = 1000;
            _serialPort.Open();
        }

        public void Disconnect()
        {
            if (!object.ReferenceEquals(_serialPort, null))
            {
                _serialPort.DataReceived -= DataReceived;

                _serialPort.DtrEnable = false;
                _serialPort.RtsEnable = false;
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();

                // have to call close last
                _serialPort.Close();

                _serialPort = null;
            }

            if (_sinceDataReceived != null)
            {
                _sinceDataReceived.Stop();
            }

            _sinceDataReceived = null;

            if (_sinceRomMessageReceived != null)
            {
                _sinceRomMessageReceived.Stop();
            }

            _sinceRomMessageReceived = null;

            _readData.Clear();
            _readPackets.Clear();
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

            if (!IsConnected)
            {
                return;
            }

            _sinceDataReceived = Stopwatch.StartNew();

            if (!_disableProcessIncoming)
            {
                Task.Run(() => TryReadPacket());
            }
        }

        protected void TryReadPacket()
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
                if (!IsConnected)
                {
                    return;
                }

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
                    if (!IsConnected)
                    {
                        return;
                    }

                    var parse = TryParse(readCopy);
                    if (parse.ParseStatus == PacketParseStatus.Success)
                    {
                        _sinceRomMessageReceived = Stopwatch.StartNew();

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

            if (!IsConnected)
            {
                return;
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
