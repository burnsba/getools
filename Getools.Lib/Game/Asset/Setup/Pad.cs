using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Pad. Each room in the game is composed of one or more pads.
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

        /// <summary>
        /// Initializes a new instance of the <see cref="Pad"/> class.
        /// </summary>
        public Pad()
        {
        }

        /// <summary>
        /// Gets or sets position coordinate.
        /// Struct offset 0x0.
        /// </summary>
        public Coord3df Position { get; set; }

        /// <summary>
        /// Gets or sets "up" coordinate.
        /// Struct offset 0xc.
        /// </summary>
        public Coord3df Up { get; set; }

        /// <summary>
        /// Gets or sets "look" coordinate.
        /// Struct offset 0x18.
        /// </summary>
        public Coord3df Look { get; set; }

        /// <summary>
        /// Gets or sets name string/pointer.
        /// Struct offset 0x24.
        /// </summary>
        public StringPointer Name { get; set; }

        /// <summary>
        /// TODO: Unknown fields.
        /// Struct offset 0x28.
        /// Seems to always be zero, to indicate end of pad in setup.
        /// </summary>
        public int Unknown { get; set; }

        /// <summary>
        /// Gets or sets the variable name used in source file.
        /// </summary>
        public string VariableName { get; set; }

        /// <summary>
        /// Reads from current position in stream. Loads object from
        /// stream as it would be read from a binary file using normal structs.
        /// </summary>
        /// <param name="br">Stream to read.</param>
        /// <returns>New object.</returns>
        public static Pad ReadFromBinFile(BinaryReader br)
        {
            var result = new Pad();

            result.Position = Coord3df.ReadFromBinFile(br);
            result.Up = Coord3df.ReadFromBinFile(br);
            result.Look = Coord3df.ReadFromBinFile(br);

            result.Name = BitUtility.Read16Big(br);
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

        /// <summary>
        /// Common implementation, returns comma seperated string of struct contents.
        /// Does not include brackets or semi-colon.
        /// </summary>
        /// <param name="sb">String builder to append to.</param>
        /// <param name="prefix">Optional prefix to prepend.</param>
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
