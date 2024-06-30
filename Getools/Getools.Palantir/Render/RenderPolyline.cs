using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;

namespace Getools.Palantir.Render
{
    /// <summary>
    /// General container for rendering a sequence of lines, such as patrol path.
    /// </summary>
    public class RenderPolyline
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderPolyline"/> class.
        /// </summary>
        public RenderPolyline()
        {
            Bbox = new BoundingBoxd()
            {
                MinX = double.MaxValue,
                MinY = double.MaxValue,
                MinZ = double.MaxValue,
                MaxX = double.MinValue,
                MaxY = double.MinValue,
                MaxZ = double.MinValue,
            };
        }

        /// <summary>
        /// Gets or sets the order index of the item.
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// Gets or sets the points defining the lines. These should be listed in order.
        /// </summary>
        public List<Coord3dd> Points { get; set; } = new List<Coord3dd>();

        /// <summary>
        /// Gets or sets the bounding box of the entire sequence of lines.
        /// </summary>
        public BoundingBoxd Bbox { get; set; }
    }
}
