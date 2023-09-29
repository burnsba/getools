using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Gebug64.Unfloader.SerialPort
{
    /// <summary>
    /// This is a high level class to manage creation of serial ports.
    /// This is designed to be a dependency injection singleton instance.
    /// This class will either return an existing serial port object
    /// if one already exists (keyed to the port), or create a new one.
    /// This is also where serial ports can be connected together.
    /// </summary>
    public class SerialPortProvider
    {
        private readonly Dictionary<string, ISerialPort> _serialPorts = new();
        private SerialPortFactory _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialPortProvider"/> class.
        /// </summary>
        /// <param name="factory">Factory to create serial port instances.</param>
        public SerialPortProvider(SerialPortFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// If this is the first time requesting a serial port for this port,
        /// a new instance is created and returned. Otherwise the existing
        /// instance associated to that port is returned.
        /// </summary>
        /// <param name="port">Port to associate with a serial port.</param>
        /// <returns>Serial port for port.</returns>
        public ISerialPort CreatePort(string port)
        {
            if (_serialPorts.ContainsKey(port))
            {
                return _serialPorts[port];
            }

            var commPort = _factory.Create(port);

            _serialPorts.Add(port, commPort);

            return commPort;
        }

        /// <summary>
        /// Returns the serial port instance associated with a port.
        /// </summary>
        /// <param name="port">Port associated with a serial port.</param>
        /// <returns>Serial port instance.</returns>
        /// <exception cref="KeyNotFoundException">Will be thrown if there is no existing serial port for this port.</exception>
        public ISerialPort GetPort(string port)
        {
            return _serialPorts[port];
        }

        /// <summary>
        /// Connects the output of each port to the input of the other.
        /// </summary>
        /// <param name="first">First serial port.</param>
        /// <param name="second">Second serial port.</param>
        public void ConnectPorts(ISerialPort first, ISerialPort second)
        {
            first.Connect(second);
            second.Connect(first);
        }
    }
}
