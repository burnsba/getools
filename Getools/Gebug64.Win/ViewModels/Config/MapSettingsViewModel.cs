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
        private Dictionary<Enum.UiMapLayer, bool> _showUiLayer = new();
        private bool _autoLoadLevel;
        private bool _followBond;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapSettingsViewModel"/> class.
        /// </summary>
        public MapSettingsViewModel()
        {
            _showUiLayer = System.Enum.GetValues<Enum.UiMapLayer>()
                .Where(x => x > Enum.UiMapLayer.DefaultUnknown)
                .OrderBy(x => x)
                .ToDictionary(key => key, val => true);
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

        /// <summary>
        /// Property to manage map layer visibility.
        /// Used in auto mapper construction.
        /// Use <see cref="SetMapLayerVisibility"/> to set values at runtime.
        /// </summary>
        public Dictionary<Enum.UiMapLayer, bool> ShowMapLayer
        {
            get => _showUiLayer;
            set
            {
                _showUiLayer = value;
            }
        }

        /// <summary>
        /// Whether stage should chnage automatically, or user should manually change the stage.
        /// </summary>
        public bool AutoLoadLevel
        {
            get => _autoLoadLevel;

            set
            {
                _autoLoadLevel = value;

                if (_autoLoadLevel != value)
                {
                    IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Whether the map should automatically scroll to Bond's position.
        /// </summary>
        public bool FollowBond
        {
            get => _followBond;

            set
            {
                _followBond = value;

                if (_followBond != value)
                {
                    IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Sets the UI layer visibility to the given value.
        /// </summary>
        /// <param name="key">Layer.</param>
        /// <param name="val">Visibility.</param>
        public void SetMapLayerVisibility(Enum.UiMapLayer key, bool val)
        {
            if (_showUiLayer.ContainsKey(key))
            {
                if (_showUiLayer[key] != val)
                {
                    _showUiLayer[key] = val;

                    Workspace.Instance.SaveAppSettings();
                }
            }
        }
    }
}
