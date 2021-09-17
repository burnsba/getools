using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Single ai script function.
    /// </summary>
    public class AiFunction : IBinData, IGetoolsLibObject
    {
        /// <summary>
        /// C file, type name. Should match known struct type.
        /// </summary>
        public const string CTypeName = "u32";

        /// <summary>
        /// Initializes a new instance of the <see cref="AiFunction"/> class.
        /// </summary>
        public AiFunction()
        {
        }

        /// <summary>
        /// Gets or sets the ai script definition.
        /// Eventually this will be replaced with strongly typed data.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Gets or sets the variable name used in source file.
        /// </summary>
        public string VariableName { get; set; }

        /// <summary>
        /// Gets or sets the index of this ai script, in the list of ai scripts
        /// defined in the setup file.
        /// </summary>
        public int OrderIndex { get; set; } = 0;

        /// <inheritdoc />
        [JsonIgnore]
        public int ByteAlignment => Config.TargetWordSize;

        /// <inheritdoc />
        [JsonIgnore]
        public int BaseDataOffset { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public int BaseDataSize => Data.Length;

        /// <inheritdoc />
        [JsonIgnore]
        public Guid MetaId { get; private set; } = Guid.NewGuid();

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

        /// <inheritdoc />
        public void Collect(IAssembleContext context)
        {
            // Leaving this not implemented.
            // Collect should be called by the DataSectionAiList because the entries
            // and prequel entries need to be placed in the correct order as a complete
            // group.
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Assemble(IAssembleContext context)
        {
            var result = context.AssembleAppendBytes(Data, Config.TargetWordSize);
            BaseDataOffset = result.DataStartAddress;
        }
    }
}
