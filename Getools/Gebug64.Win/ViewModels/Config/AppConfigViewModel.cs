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

        public RecentPathSectionViewModel RecentPath { get; set; }

        public string FramebufferGrabSavePath { get; set; }

        public AppConfigViewModel()
        {
        }
    }
}
