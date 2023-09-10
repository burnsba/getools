using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.Mvvm
{
    /// <summary>
    /// Base class for window ViewModel.
    /// </summary>
    public abstract class WindowViewModelBase : ViewModelBase
    {
        /// <summary>
        /// Closes the window.
        /// </summary>
        /// <param name="window">Window to close.</param>
        protected void CloseWindow(ICloseable window)
        {
            if (window != null)
            {
                window.Close();
            }
        }
    }
}
