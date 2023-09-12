using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;

namespace Getools.Lib.Extensions
{
    public static class Coord2dfExtensions
    {
        public static Single[] To1dArray(this IEnumerable<Coord2df> list)
        {
            var results = new List<Single>();

            foreach (var coord in list)
            {
                results.Add(coord.X);
                results.Add(coord.Y);
            }

            return results.ToArray();
        }

        public static Coord2dd ToCoord2dd(this Coord2df p)
        {
            return new Coord2dd(p.X, p.Y);
        }
    }
}
