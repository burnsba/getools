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

        /// <summary>
        /// Gets or sets address of function being pointed to.
        /// Struct offset 0x0.
        /// </summary>
        public UInt32 EntryPointer { get; set; }

        /// <summary>
        /// Gets or sets ai script id.
        /// Struct offset 0x4.
        /// </summary>
        public UInt32 Id { get; set; }

        /// <summary>
        /// AI Script associated with this entry.
        /// </summary>
        public AiFunction Function { get; set; }

        /// <summary>
        /// Gets or sets the index of how this entry is sorted
        /// in the <see cref="StageSetupFile"/> AI Script section.
        /// This should coorespond to sorting by <see cref="Id"/>.
        /// </summary>
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
