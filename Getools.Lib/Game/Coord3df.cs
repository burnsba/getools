using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
            return $"{prefix}{{{X}, {Y}, {Z}}}";
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

            sb.AppendLine($"{prefix}{X}, {Y}, {Z}");

            sb.AppendLine($"{prefix}}};");

            return sb.ToString();
        }
    }
}
