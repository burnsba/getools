using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;

namespace Getools.Palantir.Render
{
    internal class RenderPolyline
    {
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

        public int OrderIndex { get; set; }
        public List<Coord3dd> Points { get; set; } = new List<Coord3dd>();
        public BoundingBoxd Bbox { get; set; }
    }
}
