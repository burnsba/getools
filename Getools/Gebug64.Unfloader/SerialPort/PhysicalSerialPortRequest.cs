using System;
using System.Collections.Generic;
using System.Text;

namespace Gebug64.Unfloader.SerialPort
{
    public class PhysicalSerialPortRequest : ISerialPortRequest
    {
        public PhysicalSerialPortRequest(string portName)
        {
            PortName = portName;
        }

        public string PortName { get; set; }

        public string DisplayName
        {
            get
            {
                return PortName;
            }
        }
    }
}
