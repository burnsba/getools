using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;

namespace Gebug64.Win.ViewModels.Game
{
    /// <summary>
    /// Backer class for a prop, or prop-like object.
    /// </summary>
    public class Prop : GameObject, IMapSelectedObjectViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Prop"/> class.
        /// </summary>
        public Prop()
            : base()
        {
        }

        /// <summary>
        /// Prop->pos property.
        /// </summary>
        public Coord3dd PropPos { get; set; } = Coord3dd.Zero.Clone();
    }
}
