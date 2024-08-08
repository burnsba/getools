using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Win.Game;

namespace Gebug64.Win.Event
{
    /// <summary>
    /// Events args to describe what the mouse is over in the map control.
    /// </summary>
    public class NotifyMouseOverGameObjectEventArgs : EventArgs
    {
        /// <summary>
        /// List of game objects under the mouse.
        /// </summary>
        public List<GameObject> MouseOverObjects { get; set; } = new List<GameObject>();
    }
}
