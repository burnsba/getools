using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.Config
{
    /// <summary>
    /// Connection settings.
    /// </summary>
    public class ConnectionSectionSettings
    {
        /// <summary>
        /// Currently selected serial port.
        /// </summary>
        public string? SerialPort { get; set; }

        /// <summary>
        /// Currently selected fake console.
        /// </summary>
        public string? FakeConsoleType { get; set; }
    }
}
