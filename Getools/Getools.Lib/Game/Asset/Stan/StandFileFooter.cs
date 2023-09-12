using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Error;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Stan
{
    /// <summary>
    /// Footer object declare at bottom of stan.
    /// Subset of <see cref="StandFile"/>.
    /// </summary>
    public class StandFileFooter : IBinData
    {
        /// <summary>
        /// Number of bytes for the "unstric" string.
        /// Includes terminating zeros.
        /// </summary>
        public const int UnstricStringLength = 8;

        /// <summary>
        /// C file, footer variable declaration name.
        /// </summary>
        public const string DefaultDeclarationName = "footer";

        /// <summary>
        /// C file, footer section type name, non-beta. Should match known struct type.
        /// </summary>
        public const string FooterCTypeName = "StandFileFooter";

        private const int _numberWordsProperties = 4;

        private Guid _metaId = Guid.NewGuid();

        /// <summary>
        /// Initializes a new instance of the <see cref="StandFileFooter"/> class.
        /// </summary>
        public StandFileFooter()
        {
        }

        /// <inheritdoc />
        [JsonIgnore]
        public Guid MetaId => _metaId;

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
        public string Name { get; set; } = DefaultDeclarationName;

        /// <inheritdoc />
        [JsonIgnore]
        public int ByteAlignment => Config.TargetWordSize;

        /// <inheritdoc />
        [JsonIgnore]
        public int BaseDataOffset { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public int BaseDataSize => GetDataSizeOf();

        /// <summary>
        /// Calculates the .data section size of this object,
        /// according to the current format.
        /// </summary>
        /// <returns>Size in bytes.</returns>
        public static int GetDataSizeOf()
        {
            return (_numberWordsProperties * Config.TargetPointerSize) + UnstricStringLength;
        }

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

            sb.AppendLine($"{prefix}{StandFileFooter.FooterCTypeName} {Name} = {{");

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
            int stringStart = currentStreamPosition;
            int stringEnd = stringStart + C.Length - 1; // adjust for zero
            int next8 = BitUtility.Align8(stringEnd);

            var stringLength = next8 - stringStart;

            var endPosition = BitUtility.Align16(currentStreamPosition + stringLength + (Config.TargetPointerSize * _numberWordsProperties));
            var footerSize = endPosition - currentStreamPosition;

            var results = new byte[footerSize];

            int index = 0;

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

        /// <inheritdoc />
        public void Collect(IAssembleContext context)
        {
            context.AppendToDataSection(this);
        }

        /// <inheritdoc />
        public void Assemble(IAssembleContext context)
        {
            int stringEnd = C.Length - 1; // adjust for zero
            int next8 = BitUtility.Align8(stringEnd);

            var stringLength = next8;

            var footerSize = stringLength + (Config.TargetPointerSize * _numberWordsProperties);

            var results = new byte[footerSize];

            int index = 0;

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

            var aac = context.AssembleAppendBytes(results, Config.TargetWordSize);
            BaseDataOffset = aac.DataStartAddress;
        }
    }
}
