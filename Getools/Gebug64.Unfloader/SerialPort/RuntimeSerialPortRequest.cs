using System;
using System.Collections.Generic;
using System.Text;

namespace Gebug64.Unfloader.SerialPort
{
    public class RuntimeSerialPortRequest : ISerialPortRequest
    {
        public RuntimeSerialPortRequest(Type type)
        {
            Type = type;
        }

        public Type Type { get; set; }

        public string DisplayName
        {
            get
            {
                return Type.FullName;
            }
        }
    }
}
