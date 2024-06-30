using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.ViewModels.Config
{
    /// <summary>
    /// Map settings.
    /// </summary>
    public class MapSettingsViewModel : ConfigViewModelBase, ISettingsViewModel
    {
        private string? _setupBinFolder;
        private string? _stanBinFolder;
        private string? _bgBinFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapSettingsViewModel"/> class.
        /// </summary>
        public MapSettingsViewModel()
        {
        }

        /// <summary>
        /// Folder containing setup files.
        /// </summary>
        public string? SetupBinFolder
        {
            get => _setupBinFolder;

            set
            {
                _setupBinFolder = value;

                if (_setupBinFolder != value)
                {
                    IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Folder containing stan files.
        /// </summary>
        public string? StanBinFolder
        {
            get => _stanBinFolder;

            set
            {
                _stanBinFolder = value;

                if (_stanBinFolder != value)
                {
                    IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Folder containing bg files.
        /// </summary>
        public string? BgBinFolder
        {
            get => _bgBinFolder;

            set
            {
                _bgBinFolder = value;

                if (_bgBinFolder != value)
                {
                    IsDirty = true;
                }
            }
        }
    }
}
