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
    /// bondtypes.h struct bbox.
    /// </summary>
    public class BoundingBoxf : IBinData, IGetoolsLibObject
    {
        /// <summary>
        /// C file type name.
        /// </summary>
        public const string CTypeName = "struct bbox";

        /// <summary>
        /// Size of the <see cref="BoundingBoxf"/> struct in bytes.
        /// </summary>
        public const int SizeOf = 24;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundingBoxf"/> class.
        /// </summary>
        public BoundingBoxf()
        {
        }

        /// <summary>
        /// Gets or sets min x coordinate of bounding box.
        /// Game struct property, offset 0x0.
        /// </summary>
        public Single MinX { get; set; }

        /// <summary>
        /// Gets or sets max x coordinate of bounding box.
        /// Game struct property, offset 0x4.
        /// </summary>
        public Single MaxX { get; set; }

        /// <summary>
        /// Gets or sets min y coordinate of bounding box.
        /// Game struct property, offset 0x8.
        /// </summary>
        public Single MinY { get; set; }

        /// <summary>
        /// Gets or sets max y coordinate of bounding box.
        /// Game struct property, offset 0xc.
        /// </summary>
        public Single MaxY { get; set; }

        /// <summary>
        /// Gets or sets min z coordinate of bounding box.
        /// Game struct property, offset 0x10.
        /// </summary>
        public Single MinZ { get; set; }

        /// <summary>
        /// Gets or sets max z coordinate of bounding box.
        /// Game struct property, offset 0x4.
        /// </summary>
        public Single MaxZ { get; set; }

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
        public static BoundingBoxf ReadFromBinFile(BinaryReader br)
        {
            var result = new BoundingBoxf();

            result.MinX = BitUtility.CastToFloat(BitUtility.Read32Big(br));
            result.MaxX = BitUtility.CastToFloat(BitUtility.Read32Big(br));
            result.MinY = BitUtility.CastToFloat(BitUtility.Read32Big(br));
            result.MaxY = BitUtility.CastToFloat(BitUtility.Read32Big(br));
            result.MinZ = BitUtility.CastToFloat(BitUtility.Read32Big(br));
            result.MaxZ = BitUtility.CastToFloat(BitUtility.Read32Big(br));

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
            var sb = new StringBuilder();

            sb.Append(prefix);
            sb.Append("{");
            sb.Append(FloatingPoint.ToFloatCLiteral(MinX));
            sb.Append(", ");
            sb.Append(FloatingPoint.ToFloatCLiteral(MaxX));
            sb.Append(", ");
            sb.Append(FloatingPoint.ToFloatCLiteral(MinY));
            sb.Append(", ");
            sb.Append(FloatingPoint.ToFloatCLiteral(MaxY));
            sb.Append(", ");
            sb.Append(FloatingPoint.ToFloatCLiteral(MinZ));
            sb.Append(", ");
            sb.Append(FloatingPoint.ToFloatCLiteral(MaxZ));
            sb.Append("}");

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

            BitUtility.Insert32Big(bytes, pos, BitUtility.CastToInt32(MinX));
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, BitUtility.CastToInt32(MaxX));
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, BitUtility.CastToInt32(MinY));
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, BitUtility.CastToInt32(MaxY));
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, BitUtility.CastToInt32(MinZ));
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, BitUtility.CastToInt32(MaxZ));
            pos += Config.TargetWordSize;

            var result = context.AssembleAppendBytes(bytes, Config.TargetWordSize);
            BaseDataOffset = result.DataStartAddress;
        }
    }
}
