using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.ViewModels.Config
{
    /// <summary>
    /// Memory window settings.
    /// </summary>
    public class MemorySettingsViewModel : ConfigViewModelBase, ISettingsViewModel
    {
        private string? _mapBuildFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemorySettingsViewModel"/> class.
        /// </summary>
        public MemorySettingsViewModel()
        {
        }

        /// <summary>
        /// Path to build output file giving memory locations of every ELF component.
        /// </summary>
        public string? MapBuildFile
        {
            get => _mapBuildFile;

            set
            {
                _mapBuildFile = value;

                if (_mapBuildFile != value)
                {
                    IsDirty = true;
                }
            }
        }
    }
}
