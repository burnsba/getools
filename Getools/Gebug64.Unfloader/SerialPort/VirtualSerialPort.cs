using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.SerialPort
{
    /// <summary>
    /// Mock serial port used for testing.
    /// </summary>
    public class VirtualSerialPort : ISerialPort
    {
        private List<byte> _writeBuffer = new List<byte>();
        private List<byte> _readBuffer = new List<byte>();
        private VirtualSerialPort? _connectedOther = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualSerialPort"/> class.
        /// </summary>
        /// <param name="port">Port to connect to.</param>
        public VirtualSerialPort(string port)
        {
            PortName = port;
        }

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
            _connectedOther = (VirtualSerialPort)other;
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
            Array.Copy(_readBuffer.ToArray(), 0, buffer, offset, count);
            _readBuffer.RemoveRange(0, count);

            BytesToRead -= count;
            if (BytesToRead < 0)
            {
                BytesToRead = 0;
            }
        }

        /// <inheritdoc />
        public void Write(byte[] buffer, int offset, int count)
        {
            if (_connectedOther != null)
            {
                _writeBuffer.AddRange(buffer.Skip(offset).Take(count));
                _connectedOther._readBuffer.AddRange(_writeBuffer);
                _connectedOther.BytesToRead = _connectedOther._readBuffer.Count;

                if (_connectedOther.DataReceived != null)
                {
                    _connectedOther.DataReceived.Invoke(null!, null!);
                }
            }

            _writeBuffer.Clear();
        }

        /// <inheritdoc />
        public void Dispose()
        {
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
