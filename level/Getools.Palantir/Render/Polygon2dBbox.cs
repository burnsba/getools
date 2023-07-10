using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;

namespace Getools.Palantir.Render
{
    public class Polygon2dBbox1d
    {
        public int OrderIndex { get; set; }

        public int Room { get; set; }

        /// <summary>
        /// Points that define polygon.
        /// </summary>
        public List<Coord2dd> Points { get; set; } = new List<Coord2dd>();

        /// <summary>
        /// Dimension min value for bbox.
        /// </summary>
        public double DMin { get; set; }

        /// <summary>
        /// Dimension max value for bbox.
        /// </summary>
        public double DMax { get; set; }

        public GameCoordinateSystem CoordinateSystem { get; set; }
    }
}
