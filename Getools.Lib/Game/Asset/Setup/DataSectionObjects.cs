using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Game.Asset.SetupObject;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Objects / propdef section.
    /// </summary>
    public class DataSectionObjects : SetupDataSection
    {
        private const string _defaultVariableName = "objlist";

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSectionObjects"/> class.
        /// </summary>
        public DataSectionObjects()
            : base(SetupSectionId.SectionObjects, _defaultVariableName)
        {
        }

        /// <summary>
        /// Gets or sets the object prop declaration data.
        /// Each entry should contain any necessary "prequel" data that
        /// would be listed before this main entry.
        /// </summary>
        public List<ISetupObject> Objects { get; set; } = new List<ISetupObject>();

        /// <inheritdoc />
        public override int BaseDataSize
        {
            get
            {
                return
                    GetPrequelDataSize() +
                    Objects.Sum(x => x.BaseDataSize);
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc />
        public override string GetDeclarationTypeName()
        {
            return $"{SetupObjectBase.CTypeName} {VariableName}[]";
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
                Objects,
                (x, index) =>
                {
                    var s = $"{Config.DefaultIndent}/* {nameof(ISetupObject.Type)} = {x.Type}; index = {index} */";
                    s += Environment.NewLine;
                    s += x.ToCInlineS32Array(Config.DefaultIndent);
                    return s;
                });

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
            return Objects.Count;
        }

        /// <inheritdoc />
        public override int GetPrequelDataSize()
        {
            return 0;
        }

        /// <inheritdoc />
        public override void Collect(IAssembleContext context)
        {
            foreach (var entry in Objects)
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
