using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;

namespace Gebug64.Win.Game
{
    /// <summary>
    /// Backer class for a pad or pad3d object.
    /// </summary>
    public class Pad3d : Pad
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Pad3d"/> class.
        /// </summary>
        public Pad3d()
            : base()
        {
        }

        /// <summary>
        /// Bounding box, if pad3d.
        /// </summary>
        public BoundingBoxd Bbox { get; set; } = new BoundingBoxd();
    }
}
