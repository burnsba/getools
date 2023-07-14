using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;

namespace Getools.Palantir.Render
{
    internal class RenderLine
    {
        public RenderLine()
        {
            Bbox = new BoundingBoxd();
        }

        public RenderLine(Coord3dd p1, Coord3dd p2)
        {
            P1 = p1;
            P2 = p2;
            Bbox = Getools.Lib.Math.Geometry.GetBounds(p1, p2);
        }

        public int WaypointIndex { get; set; }
        public int TableIndex { get; set; }
        public int Pad1Id { get; set; }
        public int Pad2Id { get; set; }
        public int Pad1RoomId { get; set; }
        public int Pad2RoomId { get; set; }
        public Coord3dd P1 { get; set; } = Coord3dd.Zero.Clone();
        public Coord3dd P2 { get; set; } = Coord3dd.Zero.Clone();
        public BoundingBoxd Bbox { get; set; }
    }
}
