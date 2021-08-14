using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// PathLink.
    /// </summary>
    public class SetupPathSetEntry
    {
        /// <summary>
        /// C file, type name. Should match known struct type.
        /// </summary>
        public const string CTypeName = "struct s_pathSet";

        public int Offset { get; set; }

        public uint EntryPointer { get; set; }
        public PathSet Entry { get; set; }
        public uint Unknown_04 { get; set; }

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

            sb.Append($"{Formatters.Strings.ToCPointerOrNull(Entry?.VariableName)}");
            sb.Append($", ");
            sb.Append($"{Formatters.IntegralTypes.ToHex8(Unknown_04)}");

            sb.Append(" }");

            return sb.ToString();
        }
    }
}
