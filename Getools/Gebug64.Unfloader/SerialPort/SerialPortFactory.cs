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
        IServiceProvider _serviceProvider;
        SerialPortFactoryTypeGetter _typeGetter;

        public SerialPortFactory(IServiceProvider serviceProvider, SerialPortFactoryTypeGetter typeGetter)
        {
            _serviceProvider = serviceProvider;
            _typeGetter = typeGetter;
        }

        public ISerialPort Create(string port)
        {
            return (ISerialPort)ActivatorUtilities.CreateInstance(_serviceProvider, _typeGetter.Type, port);
        }
    }
}
