using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;

namespace Getools.Lib.Extensions
{
    public static class Coord3ddExtensions
    {
        public static Coord3dd Scale(this Coord3dd p, double scaleFactor)
        {
            return new Coord3dd(p.X * scaleFactor, p.Y * scaleFactor, p.Z * scaleFactor);
        }
    }
}
