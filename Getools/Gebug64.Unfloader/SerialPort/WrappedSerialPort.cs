using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsSerialPort = System.IO.Ports.SerialPort;

namespace Gebug64.Unfloader.SerialPort
{
    /// <summary>
    /// Wrapper for native <see cref="System.IO.Ports.SerialPort"/>.
    /// </summary>
    public class WrappedSerialPort : ISerialPort
    {
        private readonly OsSerialPort _serialPort;

        /// <summary>
        /// Initializes a new instance of the <see cref="WrappedSerialPort"/> class.
        /// </summary>
        /// <param name="port">Port to connect to.</param>
        public WrappedSerialPort(string port)
        {
            _serialPort = new OsSerialPort(port);

            _serialPort.DataReceived += SerialPort_DataReceived;
            _serialPort.ErrorReceived += SerialPort_ErrorReceived;
        }

        /// <inheritdoc />
        public event System.IO.Ports.SerialDataReceivedEventHandler? DataReceived;

        /// <inheritdoc />
        public event System.IO.Ports.SerialErrorReceivedEventHandler? ErrorReceived;

        /// <inheritdoc />
        public int BytesToRead => _serialPort.BytesToRead;

        /// <inheritdoc />
        public int BytesToWrite => _serialPort.BytesToWrite;

        /// <inheritdoc />
        public bool IsOpen => _serialPort.IsOpen;

        /// <inheritdoc />
        public int ReadTimeout
        {
            get { return _serialPort.ReadTimeout; }
            set { _serialPort.ReadTimeout = value; }
        }

        /// <inheritdoc />
        public int WriteTimeout
        {
            get { return _serialPort.WriteTimeout; }
            set { _serialPort.WriteTimeout = value; }
        }

        /// <inheritdoc />
        public string PortName
        {
            get { return _serialPort.PortName; }
            set { _serialPort.PortName = value; }
        }

        /// <inheritdoc />
        public System.IO.Stream BaseStream => _serialPort.BaseStream;

        /// <inheritdoc />
        public bool DtrEnable
        {
            get { return _serialPort.DtrEnable; }
            set { _serialPort.DtrEnable = value; }
        }

        /// <inheritdoc />
        public bool RtsEnable
        {
            get { return _serialPort.RtsEnable; }
            set { _serialPort.RtsEnable = value; }
        }

        /// <inheritdoc />
        public void Connect(ISerialPort other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Close() => _serialPort.Close();

        /// <inheritdoc />
        public void DiscardInBuffer() => _serialPort.DiscardInBuffer();

        /// <inheritdoc />
        public void DiscardOutBuffer() => _serialPort.DiscardOutBuffer();

        /// <inheritdoc />
        public void Open() => _serialPort.Open();

        /// <inheritdoc />
        public void Read(byte[] buffer, int offset, int count)
        {
            _serialPort.Read(buffer, offset, count);
        }

        /// <inheritdoc />
        public void Write(byte[] buffer, int offset, int count)
        {
            _serialPort.Write(buffer, offset, count);
        }

        /// <inheritdoc />
        public void Dispose() => _serialPort.Dispose();

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        protected void Dispose(bool disposing) => _serialPort.Dispose();

        private void SerialPort_ErrorReceived(object sender, System.IO.Ports.SerialErrorReceivedEventArgs e)
        {
            ErrorReceived?.Invoke(sender, e);
        }

        private void SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            DataReceived?.Invoke(sender, e);
        }
    }
}
