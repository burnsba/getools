using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Win.Game;

namespace Gebug64.Win.Event
{
    public class NotifyMouseOverGameObjectEventArgs : EventArgs
    {
        public List<GameObject> MouseOverObjects { get; set; }
    }
}
