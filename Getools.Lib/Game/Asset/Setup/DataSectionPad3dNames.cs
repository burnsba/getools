using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Getools.Lib.BinPack;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Pad3d names section.
    /// </summary>
    public class DataSectionPad3dNames : SetupDataSection
    {
        private const string _defaultVariableName = "pad3dnames";

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSectionPad3dNames"/> class.
        /// </summary>
        public DataSectionPad3dNames()
            : base(SetupSectionId.SectionPad3dNames, _defaultVariableName)
        {
        }

        /// <summary>
        /// Gets or sets the pad3d names list.
        /// </summary>
        public List<RodataString> Pad3dNames { get; set; } = new List<RodataString>();

        /// <inheritdoc />
        [JsonIgnore]
        public override int BaseDataSize
        {
            get
            {
                return
                    GetPrequelDataSize() +
                    (GetEntriesCount() * Config.TargetPointerSize);
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc />
        public override string GetDeclarationTypeName()
        {
            return $"char *{VariableName}[]";
        }

        /// <inheritdoc />
        public override void WritePrequelData(StreamWriter sw)
        {
            // nothing to do
        }

        /// <inheritdoc />
        public override void WriteSectionData(StreamWriter sw)
        {
            if (Pad3dNames.Any())
            {
                sw.WriteLine($"{GetDeclarationTypeName()} = {{");

                Utility.AllButLast(
                        Pad3dNames,
                        x => sw.WriteLine(Formatters.Strings.ToCValue(x.Value, Config.DefaultIndent) + ","),
                        x => sw.WriteLine(Formatters.Strings.ToCValueOrNull(x.Value, Config.DefaultIndent)));

                sw.WriteLine("};");
            }
            else
            {
                sw.WriteLine("/* no pad3dnames */");
            }
        }

        /// <inheritdoc />
        public override void DeserializeFix(int startingIndex = 0)
        {
            // nothing to do
        }

        /// <inheritdoc />
        public override int GetEntriesCount()
        {
            return Pad3dNames.Count;
        }

        /// <inheritdoc />
        public override int GetPrequelDataSize()
        {
            return 0;
        }

        /// <inheritdoc />
        public override void Collect(IAssembleContext context)
        {
            foreach (var entry in Pad3dNames)
            {
                context.AppendToDataSection(entry);
            }
        }

        /// <inheritdoc />
        public override void Assemble(IAssembleContext context)
        {
            // nothing to do
        }
    }
}
