using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.ViewModels.Config
{
    public class RecentPathSectionViewModel : ConfigViewModelBase, ISettingsViewModel
    {
        private string? _sendRomFolder;
        private string? _ramromPcReplayFolder;

        public RecentPathSectionViewModel()
        {
            RecentSendRom.CollectionChanged += (a, b) => IsDirty = true;
        }

        public ObservableCollection<string> RecentSendRom { get; set; } = new ObservableCollection<string>();

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
    }
}
