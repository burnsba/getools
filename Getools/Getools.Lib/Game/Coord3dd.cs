using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Formatters;
using Newtonsoft.Json;

namespace Getools.Lib.Game
{
    /// <summary>
    /// 3d double point.
    /// </summary>
    public class Coord3dd
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Coord3dd"/> class.
        /// </summary>
        public Coord3dd()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Coord3dd"/> class.
        /// </summary>
        /// <param name="x">X.</param>
        /// <param name="y">Y.</param>
        /// <param name="z">Z.</param>
        public Coord3dd(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Coord3dd"/> class.
        /// </summary>
        /// <param name="x">X.</param>
        /// <param name="y">Y.</param>
        /// <param name="z">Z.</param>
        public Coord3dd(Single x, Single y, Single z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Zero constant point.
        /// </summary>
        public static Coord3dd Zero => new Coord3dd(0, 0, 0);

        /// <summary>
        /// Max value point.
        /// </summary>
        public static Coord3dd MaxValue => new Coord3dd(double.MaxValue, double.MaxValue, double.MaxValue);

        /// <summary>
        /// Min value point.
        /// </summary>
        public static Coord3dd MinValue => new Coord3dd(double.MinValue, double.MinValue, double.MinValue);

        /// <summary>
        /// Gets or sets x position.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets y position.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Gets or sets z position.
        /// </summary>
        public double Z { get; set; }

        /// <summary>
        /// True if <see cref="X"/>, <see cref="Y"/>, and <see cref="Z"/> are all finite.
        /// </summary>
        public bool IsFinite => double.IsFinite(X) && double.IsFinite(Y) && double.IsFinite(Z);

        public static Coord3dd operator *(Coord3dd p, double d)
        {
            return new Coord3dd(p.X * d, p.Y * d, p.Z * d);
        }

        public static Coord3dd operator /(Coord3dd p, double d)
        {
            return new Coord3dd(p.X / d, p.Y / d, p.Z / d);
        }

        /// <summary>
        /// Creates a copy of the point.
        /// </summary>
        /// <returns>Copy.</returns>
        public Coord3dd Clone()
        {
            return new Coord3dd(X, Y, Z);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{FloatingPoint.ToFloatString(X)}, {FloatingPoint.ToFloatString(Y)}, {FloatingPoint.ToFloatString(Z)}";
        }

        /// <summary>
        /// Compare object.
        /// </summary>
        /// <param name="obj">Other.</param>
        /// <returns>Compares <see cref="X"/>, <see cref="Y"/>, and <see cref="Z"/> of both points. If all are equivalent or all are non-finite or NaN the points are considered requal.</returns>
        public override bool Equals(object? obj)
        {
            var other = obj as Coord3dd;

            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            bool xequals = false;
            bool yequals = false;
            bool zequals = false;

            if (double.IsNaN(X) && double.IsNaN(other.X))
            {
                xequals = true;
            }
            else if (double.IsInfinity(X) && double.IsInfinity(other.X))
            {
                xequals = true;
            }
            else if (double.IsNegativeInfinity(X) && double.IsNegativeInfinity(other.X))
            {
                xequals = true;
            }
            else
            {
                xequals = X == other.X;
            }

            if (double.IsNaN(Y) && double.IsNaN(other.Y))
            {
                yequals = true;
            }
            else if (double.IsInfinity(Y) && double.IsInfinity(other.Y))
            {
                yequals = true;
            }
            else if (double.IsNegativeInfinity(Y) && double.IsNegativeInfinity(other.Y))
            {
                yequals = true;
            }
            else
            {
                yequals = Y == other.Y;
            }

            if (double.IsNaN(Z) && double.IsNaN(other.Z))
            {
                zequals = true;
            }
            else if (double.IsInfinity(Z) && double.IsInfinity(other.Z))
            {
                zequals = true;
            }
            else if (double.IsNegativeInfinity(Z) && double.IsNegativeInfinity(other.Z))
            {
                zequals = true;
            }
            else
            {
                zequals = Z == other.Z;
            }

            return xequals && yequals && zequals;
        }

        /// <summary>
        /// Project to 2d.
        /// </summary>
        /// <returns>XY point.</returns>
        public Coord2dd To2DXY()
        {
            return new Coord2dd(X, Y);
        }

        /// <summary>
        /// Project to 2d.
        /// </summary>
        /// <returns>XZ point.</returns>
        public Coord2dd To2DXZ()
        {
            return new Coord2dd(X, Z);
        }

        /// <summary>
        /// Project to 2d.
        /// </summary>
        /// <returns>YZ point.</returns>
        public Coord2dd To2DYZ()
        {
            return new Coord2dd(Y, Z);
        }

        /// <summary>
        /// Hash code.
        /// </summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ (Y.GetHashCode() + 100) ^ (Z.GetHashCode() + 10000);
        }
    }
}
