using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;

namespace Getools.Lib.Extensions
{
    public static class BoundingBoxdExtensions
    {
        public static BoundingBoxd Scale(this BoundingBoxd bb, double scaleFactor)
        {
            return new BoundingBoxd()
            {
                MinX = bb.MinX * scaleFactor,
                MaxX = bb.MaxX * scaleFactor,
                MinY = bb.MinY * scaleFactor,
                MaxY = bb.MaxY * scaleFactor,
                MinZ = bb.MinZ * scaleFactor,
                MaxZ = bb.MaxZ * scaleFactor,
            };
        }
    }
}
