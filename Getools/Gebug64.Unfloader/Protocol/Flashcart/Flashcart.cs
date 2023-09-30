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
    /// <summary>
    /// Describes physical flashcart device.
    /// This is the lowest level of communication to the gebug romhack.
    /// </summary>
    public abstract class Flashcart : IFlashcart
    {
        /// <summary>
        /// Serial port provider.
        /// </summary>
        private readonly SerialPortProvider _portProvider;

        /// <summary>
        /// Serial port instance.
        /// </summary>
        private ISerialPort? _serialPort;

        /// <summary>
        /// Container for incoming read data.
        /// </summary>
        private List<byte> _readData = new List<byte>();

        /// <summary>
        /// Thread lock for <see cref="_readData"/>.
        /// </summary>
        private Mutex _readLock = new();

        /// <summary>
        /// This is the internal queue for incoming data that has been parsed
        /// into a <see cref="IFlashcartPacket"/>.
        /// </summary>
        private ConcurrentQueue<IFlashcartPacket> _readPackets = new ConcurrentQueue<IFlashcartPacket>();

        private Stopwatch? _sinceDataReceived = null;

        private Stopwatch? _sinceFlashcartPacketReceived = null;

        /// <summary>
        /// Logger.
        /// </summary>
        protected readonly ILogger _logger;

        /// <summary>
        /// Internal message bus to notify when a well formed packet has been received. This
        /// allows the <see cref="TestInMenu"/> method to work without interferring with
        /// other messages.
        /// </summary>
        protected MessageBus<IFlashcartPacket> _flashcartPacketBus = new();

        /// <summary>
        /// Flag to disable processing incoming messages.
        /// At one point this was required for long running communcation, such as
        /// uploading a ROM.
        /// </summary>
        protected bool _disableProcessIncoming = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Flashcart"/> class.
        /// </summary>
        /// <param name="portProvider">Serial port provider.</param>
        /// <param name="logger">Logger.</param>
        public Flashcart(SerialPortProvider portProvider, ILogger logger)
        {
            _portProvider = portProvider;
            _logger = logger;

            _flashcartPacketBus.Subscribe(SubscriptionEnqueuePacket);
        }

        /// <inheritdoc />
        public bool IsConnected => _serialPort?.IsOpen ?? false;

        /// <inheritdoc />
        public ConcurrentQueue<IFlashcartPacket> ReadPackets => _readPackets;

        /// <inheritdoc />
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

        /// <inheritdoc />
        public TimeSpan SinceFlashcartPacketReceived
        {
            get
            {
                if (_sinceFlashcartPacketReceived == null)
                {
                    return TimeSpan.MaxValue;
                }

                return _sinceFlashcartPacketReceived.Elapsed;
            }
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

            if (_sinceFlashcartPacketReceived != null)
            {
                _sinceFlashcartPacketReceived.Stop();
            }

            _sinceFlashcartPacketReceived = null;

            _readData.Clear();
            _readPackets.Clear();
        }

        /// <inheritdoc />
        public abstract void SendRom(byte[] filedata, CancellationToken? token = null);

        /// <inheritdoc />
        public void Send(byte[] data)
        {
            Send(MakePacket(data));
        }

        /// <inheritdoc />
        public void Send(IFlashcartPacket packet)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException($"Call {nameof(Connect)} first");
            }

            var data = packet.GetOuterPacket();
            _serialPort!.Write(data, 0, data.Length);
        }

        /// <inheritdoc />
        public virtual bool TestInMenu() => false;

        /// <inheritdoc />
        public void Dispose()
        {
            Disconnect();
        }

        /// <summary>
        /// Reads bytes from incoming source and attempts to read as many as rquired to
        /// parse a single packet.
        /// Implementation is device specific.
        /// </summary>
        /// <param name="data">Data to parse.</param>
        /// <returns>Parse result.</returns>
        protected abstract FlashcartPacketParseResult TryParse(List<byte> data);

        /// <summary>
        /// Wraps binary data to create a packet.
        /// Implementation is device specific.
        /// </summary>
        /// <param name="data">Data to create packet.</param>
        /// <returns>Packet.</returns>
        protected abstract IFlashcartPacket MakePacket(byte[] data);

        /// <summary>
        /// Internal method to write binary data without wrapping in a packet.
        /// </summary>
        /// <param name="data">Raw data to send.</param>
        /// <exception cref="InvalidOperationException">Throw if not connected.</exception>
        protected void WriteRaw(byte[] data)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException($"Call {nameof(Connect)} first");
            }

            _serialPort!.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Background parse method. Attempts to empty the incoming
        /// read data and parse into packets.
        /// </summary>
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
                // make a copy of the incoming data to allow permutation in place.
                readCopy = _readData.ToArray().ToList();

                int removeSize = 0;

                // This will read through the incoming data, trying to parse as it goes.
                // Every successful packet parse will remove the corresponding
                // bytes from the start of the collection.
                // Save each packet to a queue to send out when the parsing finishes.
                // Break on empty / failure.
                while (readCopy.Count > 0)
                {
                    if (!IsConnected)
                    {
                        return;
                    }

                    var parse = TryParse(readCopy);
                    if (parse.ParseStatus == PacketParseStatus.Success)
                    {
                        _sinceFlashcartPacketReceived = Stopwatch.StartNew();

                        readCopy.RemoveRange(0, parse.TotalBytesRead);
                        removeSize += parse.TotalBytesRead;

                        // Save the parsed packet to send out once parsing is done.
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

            // Notify listeners about the new packets.
            foreach (var packet in toQueue)
            {
                _flashcartPacketBus.Publish(packet);
            }
        }

        /// <summary>
        /// Serial port data received event.
        /// Adds incoming data to the read buffer, then starts the background task
        /// to parse the incoming data.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Args.</param>
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

        /// <summary>
        /// Default message bus handler. Simply make the packet available to outside sources.
        /// </summary>
        /// <param name="packet">Packet.</param>
        private void SubscriptionEnqueuePacket(IFlashcartPacket packet)
        {
            _readPackets.Enqueue(packet);
        }
    }
}
