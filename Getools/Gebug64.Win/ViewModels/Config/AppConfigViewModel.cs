using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.ViewModels.Config
{
    /// <summary>
    /// Runtime app settings.
    /// </summary>
    public class AppConfigViewModel : ConfigViewModelBase, ISettingsViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppConfigViewModel"/> class.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public AppConfigViewModel()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }

        /// <summary>
        /// Device section settings.
        /// </summary>
        public DeviceSectionViewModel Device { get; set; }

        /// <summary>
        /// Connection section settings.
        /// </summary>
        public ConnectionSectionViewModel Connection { get; set; }

        /// <summary>
        /// Recent paths settings.
        /// </summary>
        public RecentPathSectionViewModel RecentPath { get; set; }

        /// <summary>
        /// Map settings.
        /// </summary>
        public MapSettingsViewModel Map { get; set; }

        /// <summary>
        /// UI layout state.
        /// </summary>
        public UiLayoutStateViewModel LayoutState { get; set; }

        /// <summary>
        /// Path to save framebuffer grabs to.
        /// </summary>
        public string FramebufferGrabSavePath { get; set; }
    }
}
