using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.Event
{
    /// <summary>
    /// Event args for when the mouse is moved in the map control.
    /// The position is in scaled game units, but is shifted such that
    /// the min value is (0,0).
    /// </summary>
    public class NotifyMouseMoveTranslatedPositionEventArgs : EventArgs
    {
        /// <summary>
        /// Mouse position in scaled game units.
        /// This is translated by the min value of the level such that
        /// the smallest possible position is (0,0).
        /// </summary>
        public System.Windows.Point Position { get; set; }
    }
}
