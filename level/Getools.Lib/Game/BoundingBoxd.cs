using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game
{
    public class BoundingBoxd
    {
        /// <summary>
        /// Gets or sets min x coordinate of bounding box.
        /// </summary>
        public double MinX { get; set; }

        /// <summary>
        /// Gets or sets max x coordinate of bounding box.
        /// </summary>
        public double MaxX { get; set; }

        /// <summary>
        /// Gets or sets min y coordinate of bounding box.
        /// </summary>
        public double MinY { get; set; }

        /// <summary>
        /// Gets or sets max y coordinate of bounding box.
        /// </summary>
        public double MaxY { get; set; }

        /// <summary>
        /// Gets or sets min z coordinate of bounding box.
        /// </summary>
        public double MinZ { get; set; }

        /// <summary>
        /// Gets or sets max z coordinate of bounding box.
        /// </summary>
        public double MaxZ { get; set; }
    }
}
