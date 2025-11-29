using System;
using System.Collections.Generic;
using System.Text;

namespace Gebug64.Unfloader.SerialPort
{
    public class InstanceSerialPortRequest : ISerialPortRequest
    {
        public InstanceSerialPortRequest(ISerialPort port)
        {
            Port = port;
        }

        public ISerialPort Port { get; set; }

        public string DisplayName
        {
            get
            {
                return Port.PortName;
            }
        }
    }
}
