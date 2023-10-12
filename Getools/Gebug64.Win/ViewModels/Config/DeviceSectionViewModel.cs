using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.ViewModels.Config
{
    /// <summary>
    /// Device settings.
    /// </summary>
    public class DeviceSectionViewModel : ConfigViewModelBase, ISettingsViewModel
    {
        private string? _flashcart;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceSectionViewModel"/> class.
        /// </summary>
        public DeviceSectionViewModel()
        {
        }

        /// <summary>
        /// Name of the selected flashcart.
        /// </summary>
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
    }
}
