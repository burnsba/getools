using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Formatters;
using Newtonsoft.Json;

namespace Getools.Lib.Game
{
    /// <summary>
    /// 3d float point.
    /// Cooresponds to `struct coord3d`.
    /// </summary>
    public class Coord3df : IBinData, IGetoolsLibObject
    {
        /// <summary>
        /// C file type name.
        /// </summary>
        public const string CTypeName = "struct coord3d";

        /// <summary>
        /// Size of the struct in bytes.
        /// </summary>
        public const int SizeOf = 12;

        /// <summary>
        /// Initializes a new instance of the <see cref="Coord3df"/> class.
        /// </summary>
        public Coord3df()
        {
        }

        /// <summary>
        /// Gets or sets x position.
        /// </summary>
        public Single X { get; set; }

        /// <summary>
        /// Gets or sets y position.
        /// </summary>
        public Single Y { get; set; }

        /// <summary>
        /// Gets or sets z position.
        /// </summary>
        public Single Z { get; set; }

        /// <summary>
        /// Gets or sets the variable name used in source file.
        /// </summary>
        public string VariableName { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public int ByteAlignment => Config.TargetWordSize;

        /// <inheritdoc />
        [JsonIgnore]
        public int BaseDataOffset { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public int BaseDataSize => SizeOf;

        /// <inheritdoc />
        [JsonIgnore]
        public Guid MetaId { get; private set; } = Guid.NewGuid();

        /// <summary>
        /// Reads from current position in stream. Loads object from
        /// stream as it would be read from a binary file using normal structs.
        /// </summary>
        /// <param name="br">Stream to read.</param>
        /// <returns>New object.</returns>
        public static Coord3df ReadFromBinFile(BinaryReader br)
        {
            var result = new Coord3df();

            result.X = BitUtility.CastToFloat(BitUtility.Read32Big(br));
            result.Y = BitUtility.CastToFloat(BitUtility.Read32Big(br));
            result.Z = BitUtility.CastToFloat(BitUtility.Read32Big(br));

            return result;
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
            return $"{prefix}{{{FloatingPoint.ToFloatCLiteral(X)}, {FloatingPoint.ToFloatCLiteral(Y)}, {FloatingPoint.ToFloatCLiteral(Z)}}}";
        }

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

            sb.AppendLine($"{prefix}{CTypeName} {VariableName} = {{");

            sb.AppendLine($"{prefix}{FloatingPoint.ToFloatCLiteral(X)}, {FloatingPoint.ToFloatCLiteral(Y)}, {FloatingPoint.ToFloatCLiteral(Z)}");

            sb.AppendLine($"{prefix}}};");

            return sb.ToString();
        }

        /// <inheritdoc />
        public void Collect(IAssembleContext context)
        {
            context.AppendToDataSection(this);
        }

        /// <inheritdoc />
        public void Assemble(IAssembleContext context)
        {
            var size = SizeOf;
            var bytes = new byte[size];
            int pos = 0;

            BitUtility.Insert32Big(bytes, pos, BitUtility.CastToInt32(X));
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, BitUtility.CastToInt32(Y));
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, BitUtility.CastToInt32(Z));
            pos += Config.TargetWordSize;

            var result = context.AssembleAppendBytes(bytes, Config.TargetWordSize);
            BaseDataOffset = result.DataStartAddress;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{FloatingPoint.ToFloatString(X)}, {FloatingPoint.ToFloatString(Y)}, {FloatingPoint.ToFloatString(Z)}";
        }
    }
}
