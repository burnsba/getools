using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;

namespace Getools.Lib.Math
{
    public static class Geometry
    {
        /// <summary>
        /// For a list of points that describe a mesh, intersect at the given height
        /// and return all points on the boundary.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="z"></param>
        /// <returns></returns>
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
        /// For a list of points that describe a mesh, intersect at the given height
        /// and return all points on the boundary.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="z"></param>
        /// <returns></returns>
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
        /// <param name="p1x"></param>
        /// <param name="p1y"></param>
        /// <param name="p2x"></param>
        /// <param name="p2y"></param>
        /// <param name="atx"></param>
        /// <returns></returns>
        public static double FindPointOnSegment(double p1x, double p1y, double p2x, double p2y, double atx)
        {
            // (y - y1) / (y2 - y1) = (x - x1) / (x2 - x1)
            // y = (y2 - y1) * (x - x1) / (x2 - x1) + y1

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
        /// <param name="points"></param>
        /// <returns></returns>
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
            List<Coord2dd> H = new List<Coord2dd>(new Coord2dd[2 * n]);

            points.Sort((a, b) =>
                 a.X == b.X ? a.Y.CompareTo(b.Y) : a.X.CompareTo(b.X));

            // Build lower hull
            for (int i = 0; i < n; ++i)
            {
                while (k >= 2 && HullCross(H[k - 2], H[k - 1], points[i]) <= 0)
                {
                    k--;
                }

                H[k++] = points[i];
            }

            // Build upper hull
            for (int i = n - 2, t = k + 1; i >= 0; i--)
            {
                while (k >= t && HullCross(H[k - 2], H[k - 1], points[i]) <= 0)
                {
                    k--;
                }

                H[k++] = points[i];
            }

            return H.Take(k - 1).ToList();
        }

        private static double HullCross(Coord2dd O, Coord2dd A, Coord2dd B)
        {
            return ((A.X - O.X) * (B.Y - O.Y)) - ((A.Y - O.Y) * (B.X - O.X));
        }
    }
}
