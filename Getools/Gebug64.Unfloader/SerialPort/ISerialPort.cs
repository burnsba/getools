using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.SerialPort
{
    public interface ISerialPort : IDisposable
    {
        int BytesToRead { get; }
        int BytesToWrite { get; }
        bool IsOpen { get; }
        int ReadTimeout { get; set; }
        int WriteTimeout { get; set; }
        string PortName { get; }
        System.IO.Stream BaseStream { get; }
        bool DtrEnable { get; set; }
        bool RtsEnable { get; set; }

        event System.IO.Ports.SerialDataReceivedEventHandler DataReceived;
        event System.IO.Ports.SerialErrorReceivedEventHandler ErrorReceived;

        void Close();
        void DiscardInBuffer();
        void DiscardOutBuffer();

        void Open();

        void Connect(ISerialPort other);

        void Read(byte[] buffer, int offset, int count);
        void Write(byte[] buffer, int offset, int count);
    }
}
