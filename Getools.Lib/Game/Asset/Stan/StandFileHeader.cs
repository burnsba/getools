using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Getools.Lib.Error;

namespace Getools.Lib.Game.Asset.Stan
{
    /// <summary>
    /// Header object declare at top of stan.
    /// Subset of <see cref="StandFile"/>.
    /// </summary>
    public class StandFileHeader
    {
        /// <summary>
        /// C file, header section type name, non-beta. Should match known struct type.
        /// </summary>
        public const string HeaderCTypeName = "StandFileHeader";

        /// <summary>
        /// Initializes a new instance of the <see cref="StandFileHeader"/> class.
        /// </summary>
        public StandFileHeader()
        {
        }

        /// <summary>
        /// Initial unknown field. Appears to always be 4 zero'd bytes.
        /// </summary>
        public int? Unknown1 { get; set; }

        /// <summary>
        /// Offset to first tile from start of stan.
        /// This value is ignored when generating output, and actualy offset
        /// calculation is used instead.
        /// </summary>
        public int FirstTileOffset { get; set; }

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

            BitUtility.InsertPointer32Big(results, index, FirstTileOffset);
            index += Config.TargetPointerSize;

            Array.Copy(UnknownHeaderData.ToArray(), 0, results, index, UnknownHeaderData.Count);
            index += UnknownHeaderData.Count;

            return results;
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

        /// <summary>
        /// Reads from current position in stream. Loads object from
        /// stream as it would be read from a binary file using beta structs.
        /// </summary>
        /// <param name="br">Stream to read.</param>
        /// <param name="name">Sets the header <see cref="Name"/>.</param>
        /// <returns>New object.</returns>
        internal static StandFileHeader ReadFromBetaBinFile(BinaryReader br, string name)
        {
            var result = new StandFileHeader();

            result.Unknown1 = br.ReadInt32();
            if (result.Unknown1 == 0)
            {
                result.Unknown1 = null;
            }

            result.FirstTileOffset = (int)BitUtility.Swap((uint)br.ReadInt32());

            var remaining = result.FirstTileOffset - br.BaseStream.Position;
            if (remaining < 0)
            {
                throw new BadFileFormatException($"Error reading stan header, invalid first tile offset: \"{result.FirstTileOffset}\"");
            }

            for (int i = 0; i < remaining; i++)
            {
                result.UnknownHeaderData.Add(br.ReadByte());
            }

            result.Name = name;

            return result;
        }

        /// <summary>
        /// Converts this object to a byte array using normal structs and writes
        /// it to the current stream position.
        /// </summary>
        /// <param name="stream">Stream to write to.</param>
        internal void AppendToBinaryStream(BinaryWriter stream)
        {
            var bytes = ToByteArray();
            stream.Write(bytes);
        }

        /// <summary>
        /// Converts this object to a byte array using beta structs and writes
        /// it to the current stream position.
        /// </summary>
        /// <param name="stream">Stream to write to.</param>
        internal void BetaAppendToBinaryStream(BinaryWriter stream)
        {
            // no changes for beta
            AppendToBinaryStream(stream);
        }
    }
}
