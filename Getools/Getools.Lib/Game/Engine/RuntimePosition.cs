using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Engine
{
    /// <summary>
    /// Object position in scaled units with bounding box and rotation.
    /// This class should be used to draw/render an object on screen.
    /// </summary>
    public class RuntimePosition
    {
        /// <summary>
        /// Rotation to apply to object, in context of screen.
        /// </summary>
        public double RotationDegrees { get; set; }

        /// <summary>
        /// Width (x), height (y), depth (z).
        /// </summary>
        public Coord3dd ModelSize { get; set; } = new Coord3dd(0, 0, 0);

        /// <summary>
        /// Width (x), height (y), depth (z).
        /// </summary>
        public Coord3dd HalfModelSize { get; set; } = new Coord3dd(0, 0, 0);

        /// <summary>
        /// Origin point.
        /// </summary>
        public Coord3dd Origin { get; set; } = new Coord3dd(0, 0, 0);
    }
}
