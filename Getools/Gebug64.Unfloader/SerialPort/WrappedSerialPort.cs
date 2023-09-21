using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsSerialPort = System.IO.Ports.SerialPort;

namespace Gebug64.Unfloader.SerialPort
{
    public class WrappedSerialPort : ISerialPort
    {
        private readonly OsSerialPort _serialPort;

        public int BytesToRead => _serialPort.BytesToRead;
        public int BytesToWrite => _serialPort.BytesToWrite;
        public bool IsOpen => _serialPort.IsOpen;
        public int ReadTimeout
        {
            get { return _serialPort.ReadTimeout; }
            set { _serialPort.ReadTimeout = value; }
        }
        public int WriteTimeout
        {
            get { return _serialPort.WriteTimeout; }
            set { _serialPort.WriteTimeout = value; }
        }
        public string PortName
        {
            get { return _serialPort.PortName; }
            set { _serialPort.PortName = value; }
        }

        public event System.IO.Ports.SerialDataReceivedEventHandler DataReceived;
        public event System.IO.Ports.SerialErrorReceivedEventHandler ErrorReceived;

        public WrappedSerialPort(string port)
        {
            _serialPort = new OsSerialPort(port);

            _serialPort.DataReceived += DataReceived;
            _serialPort.ErrorReceived += ErrorReceived;
        }

        public void Connect(ISerialPort other)
        {
            throw new NotImplementedException();
        }

        public void Close() { _serialPort.Close(); }
        public void DiscardInBuffer() { _serialPort.DiscardInBuffer(); }
        public void DiscardOutBuffer() { _serialPort.DiscardOutBuffer(); }

        protected void Dispose(bool disposing) { _serialPort.Dispose(); }

        public void Open() { _serialPort.Open(); }

        public void Read(byte[] buffer, int offset, int count)
        {
            _serialPort.Read(buffer, offset, count);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _serialPort.Write(buffer, offset, count);
        }

        public void Dispose() { _serialPort.Dispose(); }
    }
}
