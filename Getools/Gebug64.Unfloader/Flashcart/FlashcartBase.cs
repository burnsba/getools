﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Message;
using Gebug64.Unfloader.UsbPacket;

namespace Gebug64.Unfloader.Flashcart
{
    public abstract class FlashcartBase : IFlashcart
    {
        private const int DefaultWriteLength = 32768;
        protected bool _isInit = false;

        protected object _lock = new object();

        protected SerialPort? _serialPort = null;
        protected List<byte> _readData = new List<byte>();

        protected Stopwatch? _sinceDataReceived = null;
        protected Stopwatch? _sinceRomMessageReceived = null;

        public ConcurrentQueue<IGebugMessage> MessagesFromConsole { get; set; } = new ConcurrentQueue<IGebugMessage>();

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

        public abstract void Send(IGebugMessage message);

        public void Init(string portName)
        {
            _serialPort = new SerialPort(portName);
            _serialPort.DataReceived += DataReceived;
            _serialPort.Open();
            _serialPort.ReadTimeout = 2000;
            _serialPort.WriteTimeout = 2000;

            _isInit = true;
        }

        public void Disconnect()
        {
            if (!object.ReferenceEquals(null, _serialPort))
            {
                _serialPort.DataReceived -= DataReceived;

                if (_serialPort.IsOpen)
                {
                    try
                    {
                        _serialPort.DtrEnable = false;
                        _serialPort.RtsEnable = false;
                        _serialPort.DiscardInBuffer();
                        _serialPort.DiscardOutBuffer();
                        _serialPort.BaseStream.Close();
                        _serialPort.Close();
                    }
                    catch (System.ObjectDisposedException)
                    { }
                }
            }

            _serialPort = null;
            _isInit = false;
            _serialPort = null;

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
        }

        public void Dispose()
        {
            Disconnect();
        }

        internal abstract void SendTest();

        internal abstract bool IsTestCommandResponse(IPacket packet);

        public abstract void SendRom(byte[] filedata, Nullable<CancellationToken> token = null);

        protected void Write(byte[] data)
        {
            Write(data, 0, data.Length);
        }

        protected void Write(byte[] data, int offset, int length)
        {
            if (!_isInit)
            {
                throw new InvalidOperationException($"Call {nameof(Init)} first");
            }

            if (length < 1)
            {
                throw new ArgumentException($"{nameof(length)} must be positive integer");
            }

            if (offset < 0)
            {
                throw new ArgumentException($"{nameof(offset)} must be non negative integer");
            }

            if (object.ReferenceEquals(null, data))
            {
                throw new NullReferenceException($"{nameof(data)} is null");
            }

            int remaining = length;

            while (remaining > 0)
            {
                var writeLength = DefaultWriteLength;
                if (writeLength > length)
                {
                    writeLength = length;
                }

                if (!_serialPort!.IsOpen)
                {
                    throw new InvalidOperationException("serial port is not open");
                }

                _serialPort!.Write(data, offset, writeLength);

                remaining -= writeLength;
                offset += writeLength;
            }
        }

        protected abstract void ProcessReadData();

        private void DataReceived(object s, SerialDataReceivedEventArgs e)
        {
            byte[] data = new byte[_serialPort!.BytesToRead];
            _serialPort.Read(data, 0, data.Length);
            lock (_lock)
            {
                _readData.AddRange(data);
            }

            _sinceDataReceived = Stopwatch.StartNew();

            if (!_isInit)
            {
                return;
            }

            ProcessReadData();
        }
    }
}
