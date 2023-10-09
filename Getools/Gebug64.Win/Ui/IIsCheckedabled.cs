using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.Ui
{
    /// <summary>
    /// Interface to define a menu item that can be checked.
    /// </summary>
    public interface IIsCheckedabled
    {
        /// <summary>
        /// Gets or sets a value indicating whether the item can be checked.
        /// </summary>
        bool IsChecked { get; set; }
    }
}
