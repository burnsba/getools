using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using Getools.Lib.Error;

namespace Getools.Lib.Game.Asset.Stan
{
    /// <summary>
    /// Footer object declare at bottom of stan.
    /// Subset of <see cref="StandFile"/>.
    /// </summary>
    public class StandFileFooter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StandFileFooter"/> class.
        /// </summary>
        public StandFileFooter()
        {
        }

        /// <summary>
        /// Unknown 32 bit field (1). Seems to always be zero.
        /// </summary>
        public int? Unknown1 { get; set; }

        /// <summary>
        /// Unknown 32 bit field (2). Seems to always be zero.
        /// </summary>
        public int? Unknown2 { get; set; }

        /// <summary>
        /// Seems to always be an 8 byte string consisting of "unstric".
        /// </summary>
        public string C { get; set; } = "unstric";

        /// <summary>
        /// Unknown 32 bit field (3). Seems to always be zero.
        /// </summary>
        public int? Unknown3 { get; set; }

        /// <summary>
        /// Unknown 32 bit field (4). Seems to always be zero.
        /// </summary>
        public int? Unknown4 { get; set; }

        /// <summary>
        /// Unknown 32 bit field (5). Seems to always be zero.
        /// </summary>
        public int? Unknown5 { get; set; }

        /// <summary>
        /// Unknown 32 bit field (6). Seems to always be zero.
        /// </summary>
        public int? Unknown6 { get; set; }

        /// <summary>
        /// Name of the footer object (c declaration variable name).
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
        /// <param name="prefix">Prefix or indentation.</param>
        /// <returns>String of object.</returns>
        public string ToCDeclaration(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{prefix}{Config.Stan.FooterCTypeName} {Name} = {{");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}{Formatters.IntegralTypes.ToCPointerString(Unknown1)},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}{Formatters.IntegralTypes.ToCPointerString(Unknown2)},");

            if (!string.IsNullOrEmpty(C))
            {
                sb.AppendLine($"{prefix}{Config.DefaultIndent}\"{C}\",");
            }
            else
            {
                sb.AppendLine($"{prefix}{Config.DefaultIndent}{Formatters.IntegralTypes.ToCPointerString(null)},");
            }

            sb.AppendLine($"{prefix}{Config.DefaultIndent}{Formatters.IntegralTypes.ToCPointerString(Unknown3)},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}{Formatters.IntegralTypes.ToCPointerString(Unknown4)},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}{Formatters.IntegralTypes.ToCPointerString(Unknown5)},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}{Formatters.IntegralTypes.ToCPointerString(Unknown6)}");

            sb.AppendLine($"{prefix}}};");

            return sb.ToString();
        }

        /// <summary>
        /// Builds a string to describe the current object
        /// as a complete declaraction in c, using beta structs. Includes type, variable
        /// name and trailing semi-colon.
        /// </summary>
        /// <param name="prefix">Prefix or indentation.</param>
        /// <returns>String of object.</returns>
        public string ToBetaCDeclaration(string prefix = "")
        {
            // no change for beta
            return ToCDeclaration(prefix);
        }

        /// <summary>
        /// Converts the current object to a byte array, as it would
        /// exist in a regular binary format.
        /// </summary>
        /// <param name="currentStreamPosition">Current stream position is needed
        /// to calculate how many bytes of padding to add to align properly.</param>
        /// <returns>Byte array of object.</returns>
        public byte[] ToByteArray(int currentStreamPosition)
        {
            int stringStart = currentStreamPosition + (Config.TargetPointerSize * 2);
            int stringEnd = stringStart + C.Length - 1; // adjust for zero
            int next16 = 0;
            if ((stringEnd % 16) != 0)
            {
                next16 = ((int)(stringEnd / 16) + 1) * 16;
            }
            else
            {
                next16 = (int)(stringEnd / 16) * 16;
            }

            var stringLength = next16 - stringStart;

            var results = new byte[stringLength + (Config.TargetPointerSize * 6)];

            int index = 0;

            BitUtility.InsertPointer32Big(results, index, Unknown1);
            index += Config.TargetPointerSize;

            BitUtility.InsertPointer32Big(results, index, Unknown2);
            index += Config.TargetPointerSize;

            BitUtility.InsertString(results, index, C, stringLength);
            index += stringLength;

            BitUtility.InsertPointer32Big(results, index, Unknown3);
            index += Config.TargetPointerSize;

            BitUtility.InsertPointer32Big(results, index, Unknown4);
            index += Config.TargetPointerSize;

            BitUtility.InsertPointer32Big(results, index, Unknown5);
            index += Config.TargetPointerSize;

            BitUtility.InsertPointer32Big(results, index, Unknown6);
            index += Config.TargetPointerSize;

            return results;
        }

        /// <summary>
        /// Reads from current position in stream. Loads object from
        /// stream as it would be read from a binary file using normal structs.
        /// </summary>
        /// <param name="br">Stream to read.</param>
        /// <returns>New object.</returns>
        internal static StandFileFooter ReadFromBinFile(BinaryReader br)
        {
            var result = new StandFileFooter();

            result.Unknown1 = br.ReadInt32();
            if (result.Unknown1 == 0)
            {
                result.Unknown1 = null;
            }

            result.Unknown2 = br.ReadInt32();
            if (result.Unknown2 == 0)
            {
                result.Unknown2 = null;
            }

            int stringStart = (int)br.BaseStream.Position;

            var strBytes = new List<byte>();
            int stringLength = 0;
            int safety = 32;
            while (true)
            {
                Byte b = br.ReadByte();

                if (b == 0)
                {
                    br.BaseStream.Seek(-1, SeekOrigin.Current);
                    break;
                }

                strBytes.Add(b);

                stringLength++;

                if (stringLength >= safety)
                {
                    throw new BadFileFormatException("Could not find terminating character when reading stan footer string");
                }
            }

            var unstricString = System.Text.Encoding.ASCII.GetString(strBytes.ToArray());
            result.C = unstricString;

            int stringEnd = stringStart + stringLength;
            if ((stringEnd % 16) != 0)
            {
                int next16 = ((int)(stringEnd / 16) + 1) * 16;
                var seek = next16 - stringEnd;
                br.BaseStream.Seek(seek, SeekOrigin.Current);
            }

            result.Unknown3 = br.ReadInt32();
            if (result.Unknown3 == 0)
            {
                result.Unknown3 = null;
            }

            result.Unknown4 = br.ReadInt32();
            if (result.Unknown4 == 0)
            {
                result.Unknown4 = null;
            }

            result.Unknown5 = br.ReadInt32();
            if (result.Unknown5 == 0)
            {
                result.Unknown5 = null;
            }

            result.Unknown6 = br.ReadInt32();
            if (result.Unknown6 == 0)
            {
                result.Unknown6 = null;
            }

            result.Name = $"{Config.Stan.DefaultDeclarationName_StandFileFooter}";

            return result;
        }

        /// <summary>
        /// Reads from current position in stream. Loads object from
        /// stream as it would be read from a binary file using beta structs.
        /// </summary>
        /// <param name="br">Stream to read.</param>
        /// <returns>New object.</returns>
        internal static StandFileFooter ReadFromBetaBinFile(BinaryReader br)
        {
            // footer is the same here.
            return StandFileFooter.ReadFromBinFile(br);
        }

        /// <summary>
        /// Converts this object to a byte array using normal structs and writes
        /// it to the current stream position.
        /// </summary>
        /// <param name="stream">Stream to write to.</param>
        internal void AppendToBinaryStream(BinaryWriter stream)
        {
            var bytes = ToByteArray((int)stream.BaseStream.Position);
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
