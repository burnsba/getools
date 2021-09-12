using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Getools.Lib.BinPack;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Path table section.
    /// </summary>
    public class DataSectionPathTable : SetupDataSection
    {
        private const string _defaultVariableName = "pathtbl";

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSectionPathTable"/> class.
        /// </summary>
        public DataSectionPathTable()
            : base(SetupSectionId.SectionPathTable, _defaultVariableName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSectionPathTable"/> class.
        /// </summary>
        /// <param name="typeId">Type.</param>
        protected DataSectionPathTable(SetupSectionId typeId)
            : base(typeId, _defaultVariableName)
        {
        }

        /// <summary>
        /// Gets or sets the path tables data.
        /// Each entry should contain any necessary "prequel" data that
        /// would be listed before this main entry.
        /// </summary>
        public List<SetupPathTableEntry> PathTables { get; set; } = new List<SetupPathTableEntry>();

        /// <inheritdoc />
        public override int BaseDataSize
        {
            get
            {
                return
                    GetPrequelDataSize() +
                    (GetEntriesCount() * SetupPathTableEntry.SizeOf);
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc />
        public override string GetDeclarationTypeName()
        {
            return $"{SetupPathTableEntry.CTypeName} {VariableName}[]";
        }

        /// <inheritdoc />
        public override void WritePrequelData(StreamWriter sw)
        {
            foreach (var entry in PathTables.Where(x => x.Entry != null).OrderBy(x => x.EntryPointer))
            {
                sw.Write(entry.Entry.ToCDeclaration());
            }

            if (PathTables.Where(x => x.Entry != null).Any())
            {
                sw.WriteLine();
            }
        }

        /// <inheritdoc />
        public override void WriteSectionData(StreamWriter sw)
        {
            sw.WriteLine($"{GetDeclarationTypeName()} = {{");

            Utility.ApplyCommaList(
                sw.WriteLine,
                PathTables,
                x => x.ToCInlineDeclaration(Config.DefaultIndent));

            sw.WriteLine("};");
        }

        /// <inheritdoc />
        public override void DeserializeFix(int startingIndex = 0)
        {
            int index = startingIndex;
            string baseNameFormat = null;

            if (IsUnreferenced)
            {
                baseNameFormat = "path_table_not_used_{0}";
            }
            else
            {
                baseNameFormat = "path_table_{0}";
            }

            foreach (var entry in PathTables)
            {
                entry.DeserializeFix();

                if (!object.ReferenceEquals(null, entry.Entry) && string.IsNullOrEmpty(entry.Entry.VariableName))
                {
                    entry.Entry.VariableName = string.Format(baseNameFormat, index);
                }

                index++;
            }
        }

        /// <inheritdoc />
        public override int GetEntriesCount()
        {
            return PathTables.Count;
        }

        /// <inheritdoc />
        public override int GetPrequelDataSize()
        {
            return PathTables.Where(x => x.Entry != null).Sum(x => x.Entry.Ids.Count) * Config.TargetWordSize;
        }

        /// <inheritdoc />
        public override void Collect(IAssembleContext context)
        {
            foreach (var entry in PathTables)
            {
                context.AppendToDataSection(entry.Entry);
            }

            foreach (var entry in PathTables)
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
