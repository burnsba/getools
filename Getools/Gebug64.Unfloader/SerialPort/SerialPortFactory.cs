using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Gebug64.Unfloader.SerialPort
{
    public class SerialPortFactory
    {
        SerialPortFactoryTypeGetter _typeGetter;

        public SerialPortFactory(SerialPortFactoryTypeGetter typeGetter)
        {
            _typeGetter = typeGetter;
        }

        public ISerialPort Create(string port)
        {
            return (ISerialPort)Activator.CreateInstance(_typeGetter.Type, port);
        }
    }
}
