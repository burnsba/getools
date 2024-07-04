using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Engine
{
    /// <summary>
    /// Prop position in scaled units with bounding box and rotation.
    /// </summary>
    public class RuntimePropPosition
    {
        public PropPointPosition Source { get; set; }

        public double RotationDegrees { get; set; }

        /// <summary>
        /// Width (x), height (y), depth (z).
        /// </summary>
        public Coord3dd ModelSize { get; set; } = new Coord3dd(0, 0, 0);

        /// <summary>
        /// Width (x), height (y), depth (z).
        /// </summary>
        public Coord3dd HalfModelSize { get; set; } = new Coord3dd(0, 0, 0);

        public Coord3dd Origin { get; set; }
    }
}
