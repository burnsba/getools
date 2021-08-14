using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// SetupAiListEntry.
    /// </summary>
    public class SetupAiListEntry
    {
        /// <summary>
        /// C file, type name. Should match known struct type.
        /// </summary>
        public const string CTypeName = "struct ailist";

        public UInt32 EntryPointer { get; set; }
        public UInt32 Id { get; set; }

        public AiFunction Function { get; set; }

        public int OrderIndex { get; set; } = 0;

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

            sb.Append($"{Formatters.Strings.ToCPointerOrNull(Function?.VariableName)}");
            sb.Append($", ");
            sb.Append($"{Formatters.IntegralTypes.ToHex8(Id)}");

            sb.Append(" }");

            return sb.ToString();
        }
    }
}
