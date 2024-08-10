using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;

namespace Gebug64.Win.ViewModels.Game
{
    /// <summary>
    /// Backer class for a pad or pad3d object.
    /// </summary>
    public class Pad : GameObject, IMapSelectedObjectViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Pad"/> class.
        /// </summary>
        public Pad()
            : base()
        {
        }

        /// <summary>
        /// Pos property.
        /// </summary>
        public Coord3dd Pos { get; set; } = Coord3dd.Zero.Clone();
    }
}
