using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;

namespace Getools.Lib.Math
{
    /// <summary>
    /// Geometry methods.
    /// </summary>
    public static class Geometry
    {
        /// <summary>
        /// For a list of points that describe a mesh, intersect a plane at the given axis
        /// and return a new set of points on on the plane-mesh intersection.
        /// </summary>
        /// <param name="source">Points that describe a mesh.</param>
        /// <param name="z">Z axis value for intersect (this is not the game native height axis).</param>
        /// <returns>List of intersect points.</returns>
        public static List<Coord3dd> PlaneIntersectZ(List<Coord3dd> source, double z)
        {
            var result = new HashSet<Coord3dd>();

            var numberPoints = source.Count;

            if (numberPoints == 0)
            {
                return result.ToList();
            }

            for (int i = 0; i < numberPoints; i++)
            {
                for (int j = i + 1; j < numberPoints; j++)
                {
                    var p1 = source[i];
                    var p2 = source[j];

                    if (!p1.IsFinite || !p2.IsFinite)
                    {
                        continue;
                    }

                    // If both points are either above or below the plane, there's
                    // no intersection, so continue.
                    if (p1.Z < z && p2.Z < z)
                    {
                        continue;
                    }
                    else if (p1.Z > z && p2.Z > z)
                    {
                        continue;
                    }

                    // If both points are on the plane, this forms an edge, so take each point.
                    if (p1.Z == p2.Z)
                    {
                        result.Add(new Coord3dd(p1.X, p1.Y, z));
                        result.Add(new Coord3dd(p2.X, p2.Y, z));
                        continue;
                    }

                    // If the two points form a vertical line then the location is already known.
                    if (p1.X == p2.X && p1.Y == p2.Y)
                    {
                        result.Add(new Coord3dd(p1.X, p1.Y, z));
                        continue;
                    }

                    // Project to Y plane, solve for X.
                    var x = FindPointOnSegment(p1.Z, p1.X, p2.Z, p2.X, z);

                    // Project to X plane, solve for Y.
                    var y = FindPointOnSegment(p1.Z, p1.Y, p2.Z, p2.Y, z);

                    if (double.IsFinite(x) && double.IsFinite(y))
                    {
                        result.Add(new Coord3dd(x, y, z));
                        continue;
                    }
                }
            }

            return result.ToList();
        }

        /// <summary>
        /// For a list of points that describe a mesh, intersect a plane at the given height
        /// and return a new set of points on on the plane-mesh intersection.
        /// </summary>
        /// <param name="source">Points that describe a mesh.</param>
        /// <param name="y">Y axis value for intersect (game native vertical axis).</param>
        /// <returns>List of intersect points.</returns>
        public static List<Coord3dd> PlaneIntersectY(List<Coord3dd> source, double y)
        {
            var result = new HashSet<Coord3dd>();

            var numberPoints = source.Count;

            if (numberPoints == 0)
            {
                return result.ToList();
            }

            for (int i = 0; i < numberPoints; i++)
            {
                for (int j = i + 1; j < numberPoints; j++)
                {
                    var p1 = source[i];
                    var p2 = source[j];

                    if (!p1.IsFinite || !p2.IsFinite)
                    {
                        continue;
                    }

                    // If both points are either above or below the plane, there's
                    // no intersection, so continue.
                    if (p1.Y < y && p2.Y < y)
                    {
                        continue;
                    }
                    else if (p1.Y > y && p2.Y > y)
                    {
                        continue;
                    }

                    // If both points are on the plane, this forms an edge, so take each point.
                    if (p1.Y == p2.Y)
                    {
                        result.Add(new Coord3dd(p1.X, y, p1.Z));
                        result.Add(new Coord3dd(p2.X, y, p2.Z));
                        continue;
                    }

                    // If the two points form a vertical line then the location is already known.
                    if (p1.X == p2.X && p1.Z == p2.Z)
                    {
                        result.Add(new Coord3dd(p1.X, y, p1.Z));
                        continue;
                    }

                    var x = FindPointOnSegment(p1.Y, p1.X, p2.Y, p2.X, y);
                    var z = FindPointOnSegment(p1.Y, p1.Z, p2.Y, p2.Z, y);

                    if (double.IsFinite(x) && double.IsFinite(z))
                    {
                        result.Add(new Coord3dd(x, y, z));
                        continue;
                    }
                }
            }

            return result.ToList();
        }

        /// <summary>
        /// For two points that describe a line, given an x value, find the corresponding y value.
        /// </summary>
        /// <remarks>
        /// https://stackoverflow.com/a/17693189/1462295
        /// </remarks>
        /// <param name="p1x">Point 1 x.</param>
        /// <param name="p1y">Point 1 y.</param>
        /// <param name="p2x">Point 2 x.</param>
        /// <param name="p2y">Point 2 y.</param>
        /// <param name="atx">X value to find corresponding y value.</param>
        /// <returns>Y value.</returns>
        public static double FindPointOnSegment(double p1x, double p1y, double p2x, double p2y, double atx)
        {
            /***
            * (y - y1) / (y2 - y1) = (x - x1) / (x2 - x1)
            * y = (y2 - y1) * (x - x1) / (x2 - x1) + y1
            */

            if (p2y == p1y)
            {
                return p1y;
            }

            if (p2x == p1x)
            {
                return double.NaN;
            }

            return ((p2y - p1y) * (atx - p1x) / (p2x - p1x)) + p1y;
        }

        /// <summary>
        /// Given a list of points, create a convex hull polygon.
        /// </summary>
        /// <remarks>
        /// https://stackoverflow.com/a/46371357/1462295
        /// </remarks>
        /// <param name="points">Points to create hull from.</param>
        /// <returns>Convex hull.</returns>
        public static List<Coord2dd> GetConvexHull(List<Coord2dd> points)
        {
            if (points == null)
            {
                return new List<Coord2dd>();
            }

            if (points.Count() <= 1)
            {
                return points;
            }

            int n = points.Count(), k = 0;
            List<Coord2dd> hull = new List<Coord2dd>(new Coord2dd[2 * n]);

            points.Sort((a, b) =>
                 a.X == b.X ? a.Y.CompareTo(b.Y) : a.X.CompareTo(b.X));

            // Build lower hull
            for (int i = 0; i < n; ++i)
            {
                while (k >= 2 && HullCross(hull[k - 2], hull[k - 1], points[i]) <= 0)
                {
                    k--;
                }

                hull[k++] = points[i];
            }

            // Build upper hull
            for (int i = n - 2, t = k + 1; i >= 0; i--)
            {
                while (k >= t && HullCross(hull[k - 2], hull[k - 1], points[i]) <= 0)
                {
                    k--;
                }

                hull[k++] = points[i];
            }

            return hull.Take(k - 1).ToList();
        }

        /// <summary>
        /// Gets the bounding min/max values to create a rectangular bounding box around the given points.
        /// </summary>
        /// <param name="p1">Point 1.</param>
        /// <param name="p2">Point 2.</param>
        /// <returns>Bounding box.</returns>
        public static BoundingBoxd GetBounds(Coord3dd p1, Coord3dd p2)
        {
            var bb = new BoundingBoxd();

            bb.MinX = p1.X < p2.X ? p1.X : p2.X;
            bb.MinY = p1.Y < p2.Y ? p1.Y : p2.Y;
            bb.MinZ = p1.Z < p2.Z ? p1.Z : p2.Z;

            bb.MaxX = p1.X > p2.X ? p1.X : p2.X;
            bb.MaxY = p1.Y > p2.Y ? p1.Y : p2.Y;
            bb.MaxZ = p1.Z > p2.Z ? p1.Z : p2.Z;

            return bb;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "<Justification>")]
        private static double HullCross(Coord2dd O, Coord2dd A, Coord2dd B)
        {
            return ((A.X - O.X) * (B.Y - O.Y)) - ((A.Y - O.Y) * (B.X - O.X));
        }
    }
}
