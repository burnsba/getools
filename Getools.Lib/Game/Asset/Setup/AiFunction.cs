using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Setup
{
    public class AiFunction
    {
        /// <summary>
        /// C file, type name. Should match known struct type.
        /// </summary>
        public const string CTypeName = "u32";

        public AiFunction()
        {
        }

        public int Offset { get; set; }

        public byte[] Data { get; set; }

        /// <summary>
        /// Gets or sets the variable name used in source file.
        /// </summary>
        public string VariableName { get; set; }

        public int OrderIndex { get; set; } = 0;

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

            var s32count = Data.Length / 4;
            int dataOffset = 0;
            for (int i = 0; i < s32count - 1; i++, dataOffset += 4)
            {
                sb.Append(Formatters.IntegralTypes.ToHex8(BitUtility.Read32Big(Data, dataOffset)) + ", ");
            }

            if (s32count > 0)
            {
                sb.Append(Formatters.IntegralTypes.ToHex8(BitUtility.Read32Big(Data, dataOffset)));
            }

            sb.AppendLine(" };");

            return sb.ToString();
        }
    }
}
