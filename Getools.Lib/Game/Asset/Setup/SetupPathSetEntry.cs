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

        /// <summary>
        /// Gets or sets the offset this entry was read from.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Gets or sets address of the <see cref="Entry"/> being pointed to.
        /// Struct offset 0x0.
        /// </summary>
        public uint EntryPointer { get; set; }

        /// <summary>
        /// List of ids pointed to from <see cref="EntryPointer"/>.
        /// </summary>
        public PathSet Entry { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// Struct offset 0x4.
        /// </summary>
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
