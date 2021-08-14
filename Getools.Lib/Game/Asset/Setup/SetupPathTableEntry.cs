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

        public int Offset { get; set; }

        public UInt16 Unknown_00 { get; set; }
        public UInt16 Unknown_02 { get; set; }
        public int EntryPointer { get; set; }
        public PathTable Entry { get; set; }
        public UInt32 Unknown_08 { get; set; }
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
