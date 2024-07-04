using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;

namespace Getools.Lib.Game.Engine
{
    /// <summary>
    /// General container to describe position of an object as a single point.
    /// </summary>
    public class PointPosition
    {
        /// <summary>
        /// Gets or sets the order index of the item.
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// Gets or sets the pad id of the item.
        /// </summary>
        public int PadId { get; set; }

        /// <summary>
        /// Gets or sets the room of the item.
        /// </summary>
        public int Room { get; set; }

        /// <summary>
        /// Gets or sets the origin (associated pad origin).
        /// </summary>
        public Coord3dd Origin { get; set; } = Coord3dd.Zero.Clone();

        /// <summary>
        /// Gets or sets the up vector (associated pad up vector).
        /// </summary>
        public Coord3dd Up { get; set; } = Coord3dd.Zero.Clone();

        /// <summary>
        /// Gets or sets the look vector (associated pad look vector).
        /// </summary>
        public Coord3dd Look { get; set; } = Coord3dd.Zero.Clone();

        /// <summary>
        /// If the object pad is a pad3d, this is the associated bounding box.
        /// </summary>
        public BoundingBoxd? Bbox { get; set; } = null;
    }
}
