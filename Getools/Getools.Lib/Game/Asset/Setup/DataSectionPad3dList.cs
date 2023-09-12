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
    /// Pad3d list section.
    /// </summary>
    public class DataSectionPad3dList : SetupDataSection
    {
        private const string _defaultVariableName = "pad3dlist";

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSectionPad3dList"/> class.
        /// </summary>
        public DataSectionPad3dList()
            : base(SetupSectionId.SectionPad3dList, _defaultVariableName)
        {
        }

        /// <summary>
        /// Gets or sets the path pad3d listing.
        /// Each entry should contain any necessary "prequel" data that
        /// would be listed before this main entry.
        /// </summary>
        public List<Pad3d> Pad3dList { get; set; } = new List<Pad3d>();

        /// <inheritdoc />
        [JsonIgnore]
        public override int BaseDataSize
        {
            get
            {
                return
                    GetPrequelDataSize() +
                    (GetEntriesCount() * Pad3d.SizeOf);
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc />
        public override string GetDeclarationTypeName()
        {
            return $"{Pad3d.CTypeName} {VariableName}[]";
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
                Pad3dList,
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
            return Pad3dList.Count;
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
            foreach (var entry in Pad3dList)
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
