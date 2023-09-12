using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;

namespace Getools.Lib.Extensions
{
    public static class BoundingBoxfExtensions
    {
        public static BoundingBoxd ToBoundingBoxd(this BoundingBoxf bb)
        {
            return new BoundingBoxd()
            {
                MinX = bb.MinX,
                MaxX = bb.MaxX,
                MinY = bb.MinY,
                MaxY = bb.MaxY,
                MinZ = bb.MinZ,
                MaxZ = bb.MaxZ,
            };
        }
    }
}
