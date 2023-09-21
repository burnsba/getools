using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Gebug64.Unfloader.SerialPort
{
    public class SerialPortProvider
    {
        private IServiceProvider _serviceProvider;
        private readonly Dictionary<string, ISerialPort> _serialPorts = new();

        public SerialPortProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ISerialPort CreatePort(string port)
        {
            if (_serialPorts.ContainsKey(port))
            {
                return _serialPorts[port];
            }

            var factory = _serviceProvider.GetService<SerialPortFactory>();
            var commPort = factory.Create(port);

            _serialPorts.Add(port, commPort);

            return commPort;
        }

        public ISerialPort GetPort(string port)
        {
            return _serialPorts[port];
        }

        public void ConnectPorts(ISerialPort first, ISerialPort second)
        {
            first.Connect(second);
            second.Connect(first);
        }
    }
}
