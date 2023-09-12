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
    /// 2d float point.
    /// </summary>
    public class Coord2df
    {
        /// <summary>
        /// Size of the struct in bytes.
        /// </summary>
        public const int SizeOf = 8;

        /// <summary>
        /// Initializes a new instance of the <see cref="Coord2df"/> class.
        /// </summary>
        public Coord2df()
        {
        }

        public Coord2df(double x, double y)
        {
            X = (Single)x;
            Y = (Single)y;
        }

        public Coord2df(Single x, Single y)
        {
            X = (Single)x;
            Y = (Single)y;
        }

        /// <summary>
        /// Gets or sets x position.
        /// </summary>
        public Single X { get; set; }

        /// <summary>
        /// Gets or sets y position.
        /// </summary>
        public Single Y { get; set; }

        public bool IsFinite => double.IsFinite(X) && double.IsFinite(Y);

        public static Coord2df operator *(Coord2df p, double d)
        {
            return new Coord2df(p.X * d, p.Y * d);
        }

        /// <summary>
        /// Reads from current position in stream. Loads object from
        /// stream as it would be read from a binary file using normal structs.
        /// </summary>
        /// <param name="br">Stream to read.</param>
        /// <returns>New object.</returns>
        public static Coord3df ReadFromBinFile(BinaryReader br)
        {
            var result = new Coord3df();

            result.X = BitUtility.CastToFloat(BitUtility.Read32Big(br));
            result.Y = BitUtility.CastToFloat(BitUtility.Read32Big(br));

            return result;
        }

        /// <summary>
        /// Builds a string to describe the current object
        /// as a complete declaraction in c, using normal structs.
        /// Does not include type, variable name, or trailing semi-colon.
        /// </summary>
        /// <param name="prefix">Prefix or indentation.</param>
        /// <returns>String of object.</returns>
        public string ToCInlineDeclaration(string prefix = "")
        {
            return $"{prefix}{{{FloatingPoint.ToFloatCLiteral(X)}, {FloatingPoint.ToFloatCLiteral(Y)}}}";
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{FloatingPoint.ToFloatString(X)}, {FloatingPoint.ToFloatString(Y)}";
        }

        public override bool Equals(object? obj)
        {
            var other = obj as Coord2df;

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
