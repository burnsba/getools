using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.ViewModels.Config
{
    /// <summary>
    /// Interface for app settings viewmodel class.
    /// </summary>
    public interface ISettingsViewModel
    {
        /// <summary>
        /// Gets or sets a value indicating that the settings at this level or below
        /// have changed and need to be written to disk.
        /// </summary>
        bool IsDirty { get; set; }

        /// <summary>
        /// Clears <see cref="IsDirty"/> flag, and the flag for all child properties.
        /// </summary>
        void ClearIsDirty();
    }
}
