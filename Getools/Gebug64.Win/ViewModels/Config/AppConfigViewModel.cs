using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.ViewModels.Config
{
    public class AppConfigViewModel : ConfigViewModelBase, ISettingsViewModel
    {
        public DeviceSectionViewModel Device { get; set; }

        public ConnectionSectionViewModel Connection { get; set; }

        public ObservableCollection<string> RecentSendRom { get; set; } = new ObservableCollection<string>();

        public string FramebufferGrabSavePath { get; set; }

        public AppConfigViewModel()
        {
            RecentSendRom.CollectionChanged += (a, b) => IsDirty = true;
        }
    }
}
