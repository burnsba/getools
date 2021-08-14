using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// SetupPathTableEntry.
    /// </summary>
    public class SetupPathTableEntry
    {
        /// <summary>
        /// C file, type name. Should match known struct type.
        /// </summary>
        public const string CTypeName = "struct s_pathTbl";

        /// <summary>
        /// Gets or sets the offset this entry was read from.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x0.
        /// </summary>
        public UInt16 Unknown_00 { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x2.
        /// </summary>
        public UInt16 Unknown_02 { get; set; }

        /// <summary>
        /// Pointer address of <see cref="Entry"/>.
        /// Struct offset 0x4.
        /// </summary>
        public int EntryPointer { get; set; }

        /// <summary>
        /// Value pointed to from <see cref="EntryPointer"/>.
        /// </summary>
        public PathTable Entry { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x8.
        /// </summary>
        public UInt32 Unknown_08 { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0xc.
        /// </summary>
        public UInt32 Unknown_0C { get; set; }

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

            sb.Append($"{Formatters.IntegralTypes.ToHex4(Unknown_00)}");
            sb.Append($", ");
            sb.Append($"{Formatters.IntegralTypes.ToHex4(Unknown_02)}");
            sb.Append($", ");
            sb.Append($"{Formatters.Strings.ToCPointerOrNull(Entry?.VariableName)}");
            sb.Append($", ");
            sb.Append($"{Formatters.IntegralTypes.ToHex8(Unknown_08)}");
            sb.Append($", ");
            sb.Append($"{Formatters.IntegralTypes.ToHex8(Unknown_0C)}");

            sb.Append(" }");

            return sb.ToString();
        }
    }
}
