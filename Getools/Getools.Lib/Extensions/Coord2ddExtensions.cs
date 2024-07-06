using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;

namespace Getools.Lib.Extensions
{
    /// <summary>
    /// Extension methods for 2d points.
    /// </summary>
    public static class Coord2ddExtensions
    {
        /// <summary>
        /// Convert a collection of 2d points into an array of single points.
        /// </summary>
        /// <param name="list">Source points.</param>
        /// <returns>1d list.</returns>
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

        /// <summary>
        /// Convert a single point to a rectangle.
        /// </summary>
        /// <param name="p">Origin point in middle of rectangle.</param>
        /// <param name="halfOffsetWidth">Half of the rectangle width.</param>
        /// <param name="halfOffsetHeight">Half of the rectangle height.</param>
        /// <returns>List of points defining a rectangle, clockwise from top left.</returns>
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
        /// Convert a single point to a rectangle with rotation and translation.
        /// </summary>
        /// <param name="p">Origin point in middle of rectangle.</param>
        /// <param name="halfOffsetWidth">Half of the rectangle width.</param>
        /// <param name="halfOffsetHeight">Half of the rectangle height.</param>
        /// <param name="angle">Angle in radians.</param>
        /// <param name="translateX">Trnaslation x amount.</param>
        /// <param name="translatey">Translation y amount.</param>
        /// <returns>List of points defining a rectangle, clockwise from top left (before rotation).</returns>
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

        /// <summary>
        /// Create a new point, scaled by an amount.
        /// </summary>
        /// <param name="p">Source point.</param>
        /// <param name="scaleFactor">Scalar amount.</param>
        /// <returns>New point.</returns>
        public static Coord2dd Scale(this Coord2dd p, double scaleFactor)
        {
            return new Coord2dd(p.X * scaleFactor, p.Y * scaleFactor);
        }
    }
}
