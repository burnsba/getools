using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// PathTable.
    /// </summary>
    public class PathTable
    {
        /// <summary>
        /// C file, type name. Should match known struct type.
        /// </summary>
        public const string CTypeName = "s32";

        public PathTable()
        {
        }

        public PathTable(IEnumerable<int> ids)
        {
            Ids = ids.ToList();
        }

        public int Offset { get; set; }

        /// <summary>
        /// Gets or sets the variable name used in source file.
        /// </summary>
        public string VariableName { get; set; }

        public List<int> Ids { get; set; } = new List<int>();

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

            sb.Append($"{prefix}{CTypeName} {VariableName}[] = {{ ");

            sb.Append(string.Join(", ", Ids));

            sb.AppendLine(" };");

            return sb.ToString();
        }
    }
}
