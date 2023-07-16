using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Game.Asset.Setup.Ai;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Single ai script function.
    /// </summary>
    public class AiFunction : IBinData, IGetoolsLibObject
    {
        private AiCommandBlock? _aiblock = null;

        /// <summary>
        /// C file, type name. Should match known struct type.
        /// </summary>
        public const string CTypeName = "u8";

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

            if (VariableName == "ai_9")
            {
                int a = 9;
            }

            sb.AppendLine();
            var ai = GetParsedAiBlock();

            sb.Append(ai.ToCMacro(prefix + "    "));
            sb.AppendLine();

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

        public AiCommandBlock GetParsedAiBlock()
        {
            if (object.ReferenceEquals(null, _aiblock))
            {
                _aiblock = AiCommandBuilder.ParseBytes(Data);
            }

            return _aiblock;
        }
    }
}
