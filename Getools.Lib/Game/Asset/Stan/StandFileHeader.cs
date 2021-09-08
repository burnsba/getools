using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Error;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Stan
{
    /// <summary>
    /// Header object declare at top of stan.
    /// Subset of <see cref="StandFile"/>.
    /// </summary>
    public class StandFileHeader : IBinData
    {
        /// <summary>
        /// C file, header section type name, non-beta. Should match known struct type.
        /// </summary>
        public const string HeaderCTypeName = "StandFileHeader";

        private Guid _metaId = Guid.NewGuid();

        /// <summary>
        /// Initializes a new instance of the <see cref="StandFileHeader"/> class.
        /// </summary>
        public StandFileHeader()
        {
        }

        /// <inheritdoc />
        [JsonIgnore]
        public Guid MetaId => _metaId;

        /// <summary>
        /// Initial unknown field. Appears to always be 4 zero'd bytes.
        /// </summary>
        public int? Unknown1 { get; set; }

        /// <summary>
        /// Gets or sets pointer to the first tile.
        /// </summary>
        public PointerVariable FirstTilePointer { get; set; }

        /// <summary>
        /// Unknown data after <see cref="FirstTileOffset"/> but before the first tile.
        /// Probably ints or pointers, but stored here as bytes.
        /// </summary>
        public List<Byte> UnknownHeaderData { get; set; } = new List<byte>();

        /// <summary>
        /// Name of the header object (c declaration variable name). Convention is that this matches the file name.
        /// </summary>
        public string Name { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public int ByteAlignment => Config.TargetWordSize;

        /// <inheritdoc />
        [JsonIgnore]
        public int BaseDataOffset { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public int BaseDataSize => GetDataSizeOf();

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Builds a string to describe the current object
        /// as a complete declaraction in c, using normal structs. Includes type, variable
        /// name and trailing semi-colon.
        /// </summary>
        /// <param name="filePointerDeclaration">String giving the variable name
        /// to the first tile, as a pointer (should be prefixed with "&").</param>
        /// <param name="prefix">Prefix or indentation.</param>
        /// <returns>String of object.</returns>
        public string ToCDeclaration(string filePointerDeclaration, string prefix = "")
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{prefix}{StandFileHeader.HeaderCTypeName} {Name} = {{");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}{Formatters.IntegralTypes.ToCPointerString(Unknown1)},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}{filePointerDeclaration},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}{Formatters.IntegralTypes.ToCInlineByteArray(UnknownHeaderData)}");
            sb.AppendLine($"{prefix}}};");

            return sb.ToString();
        }

        /// <summary>
        /// Converts the current object to a byte array, as it would
        /// exist in a regular binary format.
        /// </summary>
        /// <returns>Byte array of object.</returns>
        public byte[] ToByteArray()
        {
            var results = new byte[4 + 4 + UnknownHeaderData.Count];

            int index = 0;

            BitUtility.InsertPointer32Big(results, index, Unknown1);
            index += Config.TargetPointerSize;

            // Address will be calculated later
            BitUtility.InsertPointer32Big(results, index, 0);
            index += Config.TargetPointerSize;

            Array.Copy(UnknownHeaderData.ToArray(), 0, results, index, UnknownHeaderData.Count);
            index += UnknownHeaderData.Count;

            return results;
        }

        public void Collect(IAssembleContext context)
        {
            context.AppendToDataSection(this);
        }

        public void Assemble(IAssembleContext context)
        {
            var aac = context.AssembleAppendBytes(ToByteArray(), Config.TargetWordSize);
            BaseDataOffset = aac.DataStartAddress;

            context.RegisterPointer(FirstTilePointer);
        }

        /// <summary>
        /// Returns the binary size of the object in bytes,
        /// according to the current format.
        /// </summary>
        /// <returns>Size in bytes.</returns>
        public int GetDataSizeOf()
        {
            return (2 * Config.TargetPointerSize) + UnknownHeaderData.Count;
        }
    }
}
