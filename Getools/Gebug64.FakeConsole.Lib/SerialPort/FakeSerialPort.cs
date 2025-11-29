using Gebug64.Unfloader.Protocol;
using Gebug64.Unfloader.SerialPort;
using Getools.Lib.Game.Asset.Intro;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.FakeConsole.Lib.SerialPort
{
    /// <summary>
    /// Mock serial port used for testing.
    /// </summary>
    public class FakeSerialPort : ISerialPort
    {
        private Queue<IEncapsulate> _sendQueue = new Queue<IEncapsulate>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeSerialPort"/> class.
        /// </summary>
        /// <param name="port">Port to connect to.</param>
        public FakeSerialPort(string port)
        {
            PortName = port;
        }

        public Action<byte[],int,int>? ReadEventHandler { get; set; }
        public Action<byte[],int,int>? WriteEventHandler { get; set; }

        /// <inheritdoc />
        public event System.IO.Ports.SerialDataReceivedEventHandler? DataReceived;

        /// <inheritdoc />
        public event System.IO.Ports.SerialErrorReceivedEventHandler? ErrorReceived;

        /// <inheritdoc />
        public int BytesToRead { get; set; }

        /// <inheritdoc />
        public int BytesToWrite { get; set; }

        /// <inheritdoc />
        public bool IsOpen { get; set; }

        /// <inheritdoc />
        public int ReadTimeout { get; set; }

        /// <inheritdoc />
        public int WriteTimeout { get; set; }

        /// <inheritdoc />
        public string PortName { get; init; }

        /// <inheritdoc />
        public System.IO.Stream BaseStream => new MemoryStream(new byte[1]);

        /// <inheritdoc />
        public bool DtrEnable { get; set; }

        /// <inheritdoc />
        public bool RtsEnable { get; set; }

        /// <inheritdoc />
        public void Connect(ISerialPort other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Close() => IsOpen = false;

        /// <inheritdoc />
        public void DiscardInBuffer()
        {
        }

        /// <inheritdoc />
        public void DiscardOutBuffer()
        {
        }

        /// <inheritdoc />
        public void Open() => IsOpen = true;

        /// <inheritdoc />
        public void Read(byte[] buffer, int offset, int count)
        {
            var adjust = 0;

            while (_sendQueue.Any())
            {
                var item = _sendQueue.Dequeue();
                int itemLength = item.GetOuterPacket().Length;

                Array.Copy(item.GetOuterPacket(), 0, buffer, offset, itemLength);

                offset += itemLength;
                adjust += itemLength;
            }

            BytesToRead -= adjust;
        }

        /// <inheritdoc />
        public void Write(byte[] buffer, int offset, int count)
        {
            if (!object.ReferenceEquals(null, WriteEventHandler))
            {
                WriteEventHandler(buffer, offset, count);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        public void SendEnqueue(IEncapsulate packet)
        {
            _sendQueue.Enqueue(packet);
            BytesToRead += packet.GetOuterPacket().Length;
        }

        public void NotifyDataReadToSend(object sender)
        {
            ConstructorInfo constructor = typeof(SerialDataReceivedEventArgs).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(SerialData) },
                null)!;

            SerialDataReceivedEventArgs eventArgs =
                (SerialDataReceivedEventArgs)constructor.Invoke(new object[] { SerialData.Eof });

            DataReceived?.Invoke(sender, eventArgs);
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        protected void Dispose(bool disposing)
        {
        }
    }
}
