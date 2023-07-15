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

        public static void IncludePoint(this BoundingBoxd bb, Coord3dd point)
        {
            if (point.X > bb.MaxX)
            {
                bb.MaxX = point.X;
            }

            if (point.Y > bb.MaxY)
            {
                bb.MaxY = point.Y;
            }

            if (point.Z > bb.MaxZ)
            {
                bb.MaxZ = point.Z;
            }

            if (point.X < bb.MinX)
            {
                bb.MinX = point.X;
            }

            if (point.Y < bb.MinY)
            {
                bb.MinY = point.Y;
            }

            if (point.Z < bb.MinZ)
            {
                bb.MinZ = point.Z;
            }
        }
    }
}
