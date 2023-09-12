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
    /// Pad list section.
    /// </summary>
    public class DataSectionPadList : SetupDataSection
    {
        private const string _defaultVariableName = "padlist";

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSectionPadList"/> class.
        /// </summary>
        public DataSectionPadList()
            : base(SetupSectionId.SectionPadList, _defaultVariableName)
        {
        }

        /// <summary>
        /// Gets or sets the path pad listing.
        /// Each entry should contain any necessary "prequel" data that
        /// would be listed before this main entry.
        /// </summary>
        public List<Pad> PadList { get; set; } = new List<Pad>();

        /// <inheritdoc />
        [JsonIgnore]
        public override int BaseDataSize
        {
            get
            {
                return
                    GetPrequelDataSize() +
                    (GetEntriesCount() * Pad.SizeOf);
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc />
        public override string GetDeclarationTypeName()
        {
            return $"{Pad.CTypeName} {VariableName}[]";
        }

        /// <inheritdoc />
        public override void WritePrequelData(StreamWriter sw)
        {
            // nothing to do
        }

        /// <inheritdoc />
        public override void WriteSectionData(StreamWriter sw)
        {
            sw.WriteLine($"{GetDeclarationTypeName()} = {{");

            Utility.ApplyCommaList(
                sw.WriteLine,
                PadList,
                x => x.ToCInlineDeclaration(Config.DefaultIndent));

            sw.WriteLine("};");
        }

        /// <inheritdoc />
        public override void DeserializeFix(int startingIndex = 0)
        {
            // nothing to do
        }

        /// <inheritdoc />
        public override int GetEntriesCount()
        {
            return PadList.Count;
        }

        /// <inheritdoc />
        public override int GetPrequelDataSize()
        {
            return 0;
        }

        /// <inheritdoc />
        public override void Collect(IAssembleContext context)
        {
            // no prequel data, passthrough to item
            foreach (var entry in PadList)
            {
                entry.Collect(context);
            }
        }

        /// <inheritdoc />
        public override void Assemble(IAssembleContext context)
        {
            // nothing to do
        }
    }
}
