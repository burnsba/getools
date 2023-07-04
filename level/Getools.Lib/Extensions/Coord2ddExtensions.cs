using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;

namespace Getools.Lib.Extensions
{
    public static class Coord2ddExtensions
    {
        public static double[] To1dArray(this IEnumerable<Coord2dd> list)
        {
            var results = new List<double>();

            foreach (var coord in list)
            {
                results.Add(coord.X);
                results.Add(coord.Y);
            }

            return results.ToArray();
        }

        public static List<Coord2dd> ToRect(this Coord2dd p, double halfOffsetWidth, double halfOffsetHeight)
        {
            var rect = new List<Coord2dd>();

            // order matters here, if this is being converted to svg line.
            rect.Add(new Coord2dd(p.X - halfOffsetWidth, p.Y + halfOffsetHeight));
            rect.Add(new Coord2dd(p.X + halfOffsetWidth, p.Y + halfOffsetHeight));
            rect.Add(new Coord2dd(p.X + halfOffsetWidth, p.Y - halfOffsetHeight));
            rect.Add(new Coord2dd(p.X - halfOffsetWidth, p.Y - halfOffsetHeight));

            return rect;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="halfOffsetWidth"></param>
        /// <param name="halfOffsetHeight"></param>
        /// <param name="angle">Angle in radians.</param>
        /// <returns></returns>
        public static List<Coord2dd> ToRectRot(this Coord2dd p, double halfOffsetWidth, double halfOffsetHeight, double angle, double translateX = 0, double translatey = 0)
        {
            var rect = new List<Coord2dd>();

            var sin = System.Math.Sin(angle);
            var cos = System.Math.Cos(angle);

            // order matters here, if this is being converted to svg line.
            rect.Add(new Coord2dd(p.X - halfOffsetWidth + translateX, p.Y + halfOffsetHeight + translatey));
            rect.Add(new Coord2dd(p.X + halfOffsetWidth + translateX, p.Y + halfOffsetHeight + translatey));
            rect.Add(new Coord2dd(p.X + halfOffsetWidth + translateX, p.Y - halfOffsetHeight + translatey));
            rect.Add(new Coord2dd(p.X - halfOffsetWidth + translateX, p.Y - halfOffsetHeight + translatey));

            foreach (var rectPoint in rect)
            {
                rectPoint.X -= p.X;
                rectPoint.Y -= p.Y;

                var offsetx = (rectPoint.X * cos) - (rectPoint.Y * sin);
                var offsety = (rectPoint.X * sin) + (rectPoint.Y * cos);

                rectPoint.X = p.X + offsetx;
                rectPoint.Y = p.Y + offsety;
            }

            return rect;
        }
    }
}
