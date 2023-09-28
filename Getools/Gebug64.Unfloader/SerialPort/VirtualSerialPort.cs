using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.SerialPort
{
    public class VirtualSerialPort : ISerialPort
    {
        private List<byte> _writeBuffer = new List<byte>();
        private List<byte> _readBuffer = new List<byte>();
        private VirtualSerialPort? _connectedOther = null;

        public int BytesToRead { get; set; }
        public int BytesToWrite { get; set; }
        public bool IsOpen { get; set; }
        public int ReadTimeout { get; set; }
        public int WriteTimeout { get; set; }
        public string PortName { get; init; }
        public System.IO.Stream BaseStream => new MemoryStream(new byte[1]);
        public bool DtrEnable { get; set; }
        public bool RtsEnable { get; set; }

        public event System.IO.Ports.SerialDataReceivedEventHandler DataReceived;
        public event System.IO.Ports.SerialErrorReceivedEventHandler ErrorReceived;

        public VirtualSerialPort(string port)
        {
            PortName = port;
        }

        public void Connect(ISerialPort other)
        {
            _connectedOther = (VirtualSerialPort)other;
        }

        public void Close() { IsOpen = false; }
        public void DiscardInBuffer() { }
        public void DiscardOutBuffer() { }

        protected void Dispose(bool disposing) { }

        public void Open() { IsOpen = true; }

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

        public void Write(byte[] buffer, int offset, int count)
        {
            if (_connectedOther != null)
            {
                _writeBuffer.AddRange(buffer.Skip(offset).Take(count));
                _connectedOther._readBuffer.AddRange(_writeBuffer);
                _connectedOther.BytesToRead = _connectedOther._readBuffer.Count;

                if (_connectedOther.DataReceived != null)
                {
                    _connectedOther.DataReceived.Invoke(null, null);
                }
            }

            _writeBuffer.Clear();
        }

        public void Dispose()
        {
            //
        }
    }
}
