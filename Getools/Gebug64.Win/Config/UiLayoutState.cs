using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.Config
{
    /// <summary>
    /// Container to manage user interface layout state.
    /// </summary>
    public class UiLayoutState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UiLayoutState"/> class.
        /// </summary>
        public UiLayoutState()
        {
        }

        /// <summary>
        /// List of windows that are currently being managed.
        /// </summary>
        public List<UiWindowState> Windows { get; set; } = new List<UiWindowState>();
    }
}
