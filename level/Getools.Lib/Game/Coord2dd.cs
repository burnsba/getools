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
    /// 2d double point.
    /// </summary>
    public class Coord2dd
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Coord2dd"/> class.
        /// </summary>
        public Coord2dd()
        {
        }

        public Coord2dd(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Coord2dd(Single x, Single y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Gets or sets x position.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets y position.
        /// </summary>
        public double Y { get; set; }

        public bool IsFinite => double.IsFinite(X) && double.IsFinite(Y);

        public static Coord2dd operator *(Coord2dd p, double d)
        {
            return new Coord2dd(p.X * d, p.Y * d);
        }

        public Coord2dd Clone()
        {
            return new Coord2dd(X, Y);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{FloatingPoint.ToFloatString(X)}, {FloatingPoint.ToFloatString(Y)}";
        }

        public override bool Equals(object? obj)
        {
            var other = obj as Coord2dd;

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

            return xequals && yequals;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ (Y.GetHashCode() + 100);
        }
    }
}
