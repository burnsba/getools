using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.Windows
{
    /// <summary>
    /// Interface to describe windows or window-like controls that should be saved to/loaded from config file.
    /// </summary>
    public interface ILayoutWindow
    {
        /// <summary>
        /// Full type name. Used to save settings to config file, then load back and instantiate instance at runtime.
        /// </summary>
        string TypeName { get; }
    }
}
