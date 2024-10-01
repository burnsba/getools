using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.Event
{
    public class NotifyBondMoveEventArgs : EventArgs
    {
        /// <summary>
        /// Bond's (x,z) position in scaled game coordinates
        /// </summary>
        public System.Windows.Point Position { get; set; }
    }
}
