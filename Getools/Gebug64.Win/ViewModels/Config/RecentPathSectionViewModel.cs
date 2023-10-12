using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.ViewModels.Config
{
    /// <summary>
    /// Configuration section about recent file/folder paths.
    /// </summary>
    public class RecentPathSectionViewModel : ConfigViewModelBase, ISettingsViewModel
    {
        private string? _sendRomFolder;
        private string? _ramromPcReplayFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecentPathSectionViewModel"/> class.
        /// </summary>
        public RecentPathSectionViewModel()
        {
            RecentSendRom.CollectionChanged += (a, b) => IsDirty = true;
        }

        /// <summary>
        /// The most recently used folder location when sending a ROM to flashcart.
        /// </summary>
        public string? SendRomFolder
        {
            get => _sendRomFolder;
            set
            {
                _sendRomFolder = value;

                if (_sendRomFolder != value)
                {
                    IsDirty = true;
                }
            }
        }

        /// <summary>
        /// The most recently used folder location when selecting a ramrom replay.
        /// </summary>
        public string? RamromPcReplayFolder
        {
            get => _ramromPcReplayFolder;
            set
            {
                _ramromPcReplayFolder = value;

                if (_ramromPcReplayFolder != value)
                {
                    IsDirty = true;
                }
            }
        }

        /// <summary>
        /// List of most recently sent ROMs.
        /// </summary>
        public ObservableCollection<string> RecentSendRom { get; set; } = new ObservableCollection<string>();
    }
}
