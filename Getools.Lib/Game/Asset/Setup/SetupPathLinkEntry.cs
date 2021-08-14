using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// PathLink.
    /// </summary>
    /// <remarks>
    /// Record ends with (UInt32)0.
    /// </remarks>
    public class SetupPathLinkEntry
    {
        public const UInt32 RecordDelimiter = 0;

        /// <summary>
        /// C file, type name. Should match known struct type.
        /// </summary>
        public const string CTypeName = "struct s_pathLink";

        public int Offset { get; set; }

        public int NeighborsPointer { get; set; }
        public PathListing Neighbors { get; set; }

        public int IndexPointer { get; set; }
        public PathListing Indeces { get; set; }

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

            sb.Append($"{Formatters.Strings.ToCPointerOrNull(Neighbors?.VariableName)}");
            sb.Append($", ");
            sb.Append($"{Formatters.Strings.ToCPointerOrNull(Indeces?.VariableName)}");
            sb.Append($", ");
            sb.Append($"{RecordDelimiter}");

            sb.Append(" }");

            return sb.ToString();
        }
    }
}
