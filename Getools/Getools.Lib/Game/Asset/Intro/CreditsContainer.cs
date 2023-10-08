using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Game.Enums;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Intro
{
    /// <summary>
    /// Container object for credits data. TO be used to convert collection into .c array.
    /// </summary>
    public class CreditsContainer : IBinData, IGetoolsLibObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreditsContainer"/> class.
        /// </summary>
        public CreditsContainer()
        {
        }

        /// <summary>
        /// Credits entries.
        /// </summary>
        public List<IntroCreditEntry> CreditsEntries { get; set; } = new List<IntroCreditEntry>();

        /// <summary>
        /// Gets or sets the variable name used in source file.
        /// </summary>
        public string? VariableName { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public int ByteAlignment => Config.TargetWordSize;

        /// <inheritdoc />
        [JsonIgnore]
        public int BaseDataOffset { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public virtual int BaseDataSize { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public Guid MetaId { get; private set; } = Guid.NewGuid();

        /// <summary>
        /// Builds a string to describe the current object
        /// as a complete declaraction in c, using normal structs. Includes type, variable
        /// name and trailing semi-colon.
        /// </summary>
        /// <param name="prefix">Prefix or indentation.</param>
        /// <param name="printIndex">Whether or not to include a index of the items printed.</param>
        /// <returns>String of object.</returns>
        public string ToCDeclaration(string prefix = "", bool printIndex = false)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{prefix}{IntroCreditEntry.CTypeName} {VariableName}[] = {{");

            if (!printIndex)
            {
                Utility.AllButLast(
                    CreditsEntries,
                    x => sb.AppendLine($"{prefix}{Config.DefaultIndent}{x.ToCInlineDeclaration()}, "),
                    x => sb.AppendLine($"{prefix}{Config.DefaultIndent}{x.ToCInlineDeclaration()}"));
            }
            else
            {
                Utility.AllButLast(
                    CreditsEntries,
                    (x, index) =>
                    {
                        sb.AppendLine($"{prefix}{Config.DefaultIndent}/* index = {index} */");
                        sb.AppendLine($"{prefix}{Config.DefaultIndent}{x.ToCInlineDeclaration()}, ");
                    },
                    (x, index) =>
                    {
                        sb.AppendLine($"{prefix}{Config.DefaultIndent}/* index = {index} */");
                        sb.AppendLine($"{prefix}{Config.DefaultIndent}{x.ToCInlineDeclaration()}");
                    });
            }

            sb.AppendLine($"{prefix}}};");

            return sb.ToString();
        }

        /// <inheritdoc />
        public void Collect(IAssembleContext context)
        {
            foreach (var entry in CreditsEntries)
            {
                entry.Collect(context);
            }
        }

        /// <inheritdoc />
        public void Assemble(IAssembleContext context)
        {
            // nothing to do
        }
    }
}
