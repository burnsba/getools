using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Gebug64.Unfloader.SerialPort
{
    /// <summary>
    /// This is the last class in the chain to create a serial port using dependency injection.
    /// This class can be transient.
    /// </summary>
    public class SerialPortFactory
    {
        private readonly SerialPortFactoryTypeGetter _typeGetter;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialPortFactory"/> class.
        /// </summary>
        /// <param name="typeGetter">Resolver for type that will be created.</param>
        public SerialPortFactory(SerialPortFactoryTypeGetter typeGetter)
        {
            _typeGetter = typeGetter;
        }

        /// <summary>
        /// Calls <see cref="Activator.CreateInstance"/> to create an instance
        /// of the serial port described by the <see cref="SerialPortFactoryTypeGetter"/>.
        /// </summary>
        /// <param name="port">Port for newly created serial port to use.</param>
        /// <returns>Instance of serial port.</returns>
        public ISerialPort Create(string port)
        {
            return (ISerialPort)Activator.CreateInstance(_typeGetter.Type, port)!;
        }
    }
}
