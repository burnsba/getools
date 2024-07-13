using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Win.Config;

namespace Gebug64.Win.ViewModels.Config
{
    /// <summary>
    /// Viewmodel to manage user interface layout state.
    /// </summary>
    public class UiLayoutStateViewModel : ConfigViewModelBase, ISettingsViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UiLayoutStateViewModel"/> class.
        /// </summary>
        public UiLayoutStateViewModel()
        {
        }

        /// <summary>
        /// List of windows that are currently being managed.
        /// </summary>
        public List<UiWindowState> Windows { get; set; } = new List<UiWindowState>();
    }
}
