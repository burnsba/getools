using Getools.Lib.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Palantir.Render
{
    public class RenderPosition
    {
        public int OrderIndex { get; set; }

        public int PadId { get; set; }

        public int Room { get; set; }

        public Coord3dd Origin { get; set; } = Coord3dd.Zero.Clone();

        public Coord3dd Up { get; set; } = Coord3dd.Zero.Clone();

        public Coord3dd Look { get; set; } = Coord3dd.Zero.Clone();

        public GameCoordinateSystem CoordinateSystem { get; set; }

        public BoundingBoxd? Bbox { get; set; } = null;
    }
}
