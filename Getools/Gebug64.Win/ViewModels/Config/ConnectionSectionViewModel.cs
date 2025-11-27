using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.ViewModels.Config
{
    /// <summary>
    /// Connection settings.
    /// </summary>
    public class ConnectionSectionViewModel : ConfigViewModelBase, ISettingsViewModel
    {
        private string? _serialPort;
        private string? _fakeConsoleType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionSectionViewModel"/> class.
        /// </summary>
        public ConnectionSectionViewModel()
        {
        }

        /// <summary>
        /// Currently selected serial port.
        /// </summary>
        public string? SerialPort
        {
            get => _serialPort;
            set
            {
                _serialPort = value;

                if (_serialPort != value)
                {
                    IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Currently selected fake console.
        /// </summary>
        public string? FakeConsoleType
        {
            get => _fakeConsoleType;
            set
            {
                _fakeConsoleType = value;

                if (_fakeConsoleType != value)
                {
                    IsDirty = true;
                }
            }
        }
    }
}
