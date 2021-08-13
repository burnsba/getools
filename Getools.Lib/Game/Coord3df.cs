using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Getools.Lib.Formatters;

namespace Getools.Lib.Game
{
    public class Coord3df
    {
        /// <summary>
        /// C file type name.
        /// </summary>
        public const string CTypeName = "coord3d";

        public Single X { get; set; }

        public Single Y { get; set; }

        public Single Z { get; set; }

        /// <summary>
        /// Gets or sets the variable name used in source file.
        /// </summary>
        public string VariableName { get; set; }

        public Coord3df()
        { }

        public Coord3df(Single x, Single y, Single z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Coord3df ReadFromBinFile(BinaryReader br)
        {
            var result = new Coord3df();

            result.X = BitUtility.CastToFloat(BitUtility.Read32Big(br));
            result.Y = BitUtility.CastToFloat(BitUtility.Read32Big(br));
            result.Z = BitUtility.CastToFloat(BitUtility.Read32Big(br));

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
            return $"{prefix}{{{FloatingPoint.ToFloatCLiteral(X)}, {FloatingPoint.ToFloatCLiteral(Y)}, {FloatingPoint.ToFloatCLiteral(Z)}}}";
        }

        /// <summary>
        /// Builds a string to describe the current object
        /// as a complete declaraction in c, using normal structs. Includes type, variable
        /// name and trailing semi-colon.
        /// </summary>
        /// <param name="prefix">Prefix or indentation.</param>
        /// <returns>String of object.</returns>
        public string ToCDeclaration(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{prefix}{CTypeName} {VariableName} = {{");

            sb.AppendLine($"{prefix}{FloatingPoint.ToFloatCLiteral(X)}, {FloatingPoint.ToFloatCLiteral(Y)}, {FloatingPoint.ToFloatCLiteral(Z)}");

            sb.AppendLine($"{prefix}}};");

            return sb.ToString();
        }

        public override string ToString()
        {
            return $"{FloatingPoint.ToFloatString(X)}, {FloatingPoint.ToFloatString(Y)}, {FloatingPoint.ToFloatString(Z)}";
        }
    }
}
