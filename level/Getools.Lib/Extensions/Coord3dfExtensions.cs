using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;

namespace Getools.Lib.Extensions
{
    public static class Coord3dfExtensions
    {
        public static Coord3dd ToCoord3dd(this Coord3df p)
        {
            return new Coord3dd(p.X, p.Y, p.Z);
        }
    }
}
