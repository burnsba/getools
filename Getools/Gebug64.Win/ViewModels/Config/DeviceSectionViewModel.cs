using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.ViewModels.Config
{
    public class DeviceSectionViewModel : ConfigViewModelBase, ISettingsViewModel
    {
        private string? _flashcart;

        public string? Flashcart
        {
            get => _flashcart;
            set
            {
                _flashcart = value;

                if (_flashcart != value)
                {
                    IsDirty = true;
                }
            }
        }

        public DeviceSectionViewModel() { }
    }
}
