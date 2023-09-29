using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.SerialPort
{
    /// <summary>
    /// This class is used to resolve the type of serial port that should be instantiated
    /// at runtime. This will allow using a mock serial port in the test project,
    /// or using a real serial port the application.
    /// This is designed to be used as a dependency injection singleton class.
    /// </summary>
    public class SerialPortFactoryTypeGetter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerialPortFactoryTypeGetter"/> class.
        /// </summary>
        /// <param name="type">Type of serial port.</param>
        public SerialPortFactoryTypeGetter(Type type)
        {
            Type = type;
        }

        /// <summary>
        /// Gets the type of serial port.
        /// </summary>
        public Type Type { get; init; }
    }
}
