using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;

namespace Getools.Palantir.Render
{
    /// <summary>
    /// General container for rendering lines from stage data.
    /// </summary>
    internal class RenderLine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderLine"/> class.
        /// </summary>
        public RenderLine()
        {
            Bbox = new BoundingBoxd();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderLine"/> class.
        /// </summary>
        /// <param name="p1">Line start point.</param>
        /// <param name="p2">Line end point.</param>
        public RenderLine(Coord3dd p1, Coord3dd p2)
        {
            P1 = p1;
            P2 = p2;
            Bbox = Getools.Lib.Math.Geometry.GetBounds(p1, p2);
        }

        /// <summary>
        /// If the line is a waypoint, this is the waypoint index from the setup file.
        /// </summary>
        public int WaypointIndex { get; set; }

        /// <summary>
        /// If this line is a waypoint, this is the path table index from the seutp file.
        /// </summary>
        public int TableIndex { get; set; }

        /// <summary>
        /// Gets or sets the pad id of the first point.
        /// </summary>
        public int Pad1Id { get; set; }

        /// <summary>
        /// Gets or sets the pad id of the second point.
        /// </summary>
        public int Pad2Id { get; set; }

        /// <summary>
        /// Gets or sets the room of the first point.
        /// </summary>
        public int Pad1RoomId { get; set; }

        /// <summary>
        /// Gets or sets the room of the second point.
        /// </summary>
        public int Pad2RoomId { get; set; }

        /// <summary>
        /// Gets or sets the stage coordinates of the first point.
        /// </summary>
        public Coord3dd P1 { get; set; } = Coord3dd.Zero.Clone();

        /// <summary>
        /// Gets or sets the stage coordinates of the second point.
        /// </summary>
        public Coord3dd P2 { get; set; } = Coord3dd.Zero.Clone();

        /// <summary>
        /// Gets or sets the bounding box surrounding the points.
        /// This should be manually managed external to this class.
        /// </summary>
        public BoundingBoxd Bbox { get; set; }
    }
}
