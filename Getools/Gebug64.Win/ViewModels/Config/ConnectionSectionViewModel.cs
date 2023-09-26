using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.ViewModels.Config
{
    public class ConnectionSectionViewModel : ConfigViewModelBase, ISettingsViewModel
    {
        private string? _serialPort;

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

        public ConnectionSectionViewModel() { }
    }
}
