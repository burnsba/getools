using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Pad.
    /// </summary>
    public class Pad
    {
        /// <summary>
        /// C file, type name. Should match known struct type.
        /// </summary>
        public const string CTypeName = "struct pad";

        /// <summary>
        /// Size of the point struct in bytes.
        /// </summary>
        public const int SizeOf = 44;

        public Pad()
        {
        }

        public Coord3df Position { get; set; }

        public Coord3df Up { get; set; }

        public Coord3df Look { get; set; }

        public StringPointer Name { get; set; }

        public int NameRodataOffset { get; set; }

        public int Unknown { get; set; }

        /// <summary>
        /// Gets or sets the variable name used in source file.
        /// </summary>
        public string VariableName { get; set; }

        public static Pad ReadFromBinFile(BinaryReader br)
        {
            var result = new Pad();

            result.Position = Coord3df.ReadFromBinFile(br);
            result.Up = Coord3df.ReadFromBinFile(br);
            result.Look = Coord3df.ReadFromBinFile(br);

            result.NameRodataOffset = BitUtility.Read16Big(br);
            result.Unknown = BitUtility.Read32Big(br);

            return result;
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

            ToCDeclarationCommon(sb, prefix);

            sb.AppendLine($"{prefix}}};");

            return sb.ToString();
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
            var sb = new StringBuilder();

            sb.Append($"{prefix}{{ ");

            ToCDeclarationCommon(sb, prefix);

            sb.Append(" }");

            return sb.ToString();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1119:Statement should not use unnecessary parenthesis", Justification = "<Justification>")]
        protected virtual void ToCDeclarationCommon(StringBuilder sb, string prefix = "")
        {
            sb.Append(Position.ToCInlineDeclaration(string.Empty));
            sb.Append(", ");
            sb.Append(Up.ToCInlineDeclaration(string.Empty));
            sb.Append(", ");
            sb.Append(Look.ToCInlineDeclaration(string.Empty));
            sb.Append(", ");
            sb.Append(Name.ToCValueOrNull());
            sb.Append(", ");
            sb.Append(Unknown);
        }
    }
}
