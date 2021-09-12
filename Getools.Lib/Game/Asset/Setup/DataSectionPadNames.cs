using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Getools.Lib.BinPack;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Pad names section.
    /// </summary>
    public class DataSectionPadNames : SetupDataSection
    {
        private const string _defaultVariableName = "padnames";

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSectionPadNames"/> class.
        /// </summary>
        public DataSectionPadNames()
            : base(SetupSectionId.SectionPadNames, _defaultVariableName)
        {
        }

        /// <summary>
        /// Gets or sets the pad names list.
        /// </summary>
        public List<RodataString> PadNames { get; set; } = new List<RodataString>();

        /// <inheritdoc />
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
            if (PadNames.Any())
            {
                sw.WriteLine($"{GetDeclarationTypeName()} = {{");

                Utility.AllButLast(
                        PadNames,
                        x => sw.WriteLine(x.ToCValue(Config.DefaultIndent) + ","),
                        x => sw.WriteLine(x.ToCValueOrNull(Config.DefaultIndent)));

                sw.WriteLine("};");
            }
            else
            {
                sw.WriteLine("/* no padnames */");
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
            return PadNames.Count;
        }

        /// <inheritdoc />
        public override int GetPrequelDataSize()
        {
            return 0;
        }

        /// <inheritdoc />
        public override void Collect(IAssembleContext context)
        {
            foreach (var entry in PadNames)
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
