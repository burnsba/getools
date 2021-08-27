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
        /// <summary>
        /// Each entry ends with this value.
        /// </summary>
        public const UInt32 RecordDelimiter = 0;

        /// <summary>
        /// Size of the point struct in bytes.
        /// </summary>
        public const int SizeOf = 16;

        /// <summary>
        /// C file, type name. Should match known struct type.
        /// </summary>
        public const string CTypeName = "struct s_pathLink";

        /// <summary>
        /// Gets or sets the offset this entry was read from.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Gets or sets address of the <see cref="Neighbors"/> list being pointed to.
        /// Struct offset 0x0.
        /// </summary>
        public int NeighborsPointer { get; set; }

        /// <summary>
        /// List of ids pointed to from <see cref="NeighborsPointer"/>.
        /// </summary>
        public PathListing Neighbors { get; set; }

        /// <summary>
        /// Gets or sets address of the <see cref="Indeces"/> list being pointed to.
        /// Struct offset 0x4.
        /// </summary>
        public int IndexPointer { get; set; }

        /// <summary>
        /// List of ids pointed to from <see cref="IndexPointer"/>.
        /// </summary>
        public PathListing Indeces { get; set; }

        /// <summary>
        /// Some setups have a single NULL entry before the path link section,
        /// instead of two "not used" arrays. Mark this as a NULL
        /// entry with this property.
        /// </summary>
        public bool IsNull { get; set; }

        /// <summary>
        /// Gets or sets the variable name used in source file.
        /// </summary>
        public string VariableName { get; set; }

        /// <summary>
        /// Builds a string to describe the current object
        /// as a complete declaraction in c, using normal structs. Includes type, variable
        /// name and trailing semi-colon.
        /// </summary>
        /// <param name="prefix">Prefix or indentation.</param>
        /// <returns>String of object.</returns>
        public string ToCDeclaration(string prefix = "")
        {
            if (!(object.ReferenceEquals(null, Indeces) && object.ReferenceEquals(null, Neighbors) && IsNull))
            {
                throw new NotImplementedException($"{nameof(SetupPathLinkEntry)} {nameof(ToCDeclaration)} not implemented for non-null variable");
            }

            var sb = new StringBuilder();

            sb.AppendLine($"{prefix}{CTypeName} {VariableName} = NULL;");

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
