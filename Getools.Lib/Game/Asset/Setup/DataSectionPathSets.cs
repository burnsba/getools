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
    /// Path sets section.
    /// </summary>
    public class DataSectionPathSets : SetupDataSection
    {
        private const string _defaultVariableName = "paths";

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSectionPathSets"/> class.
        /// </summary>
        public DataSectionPathSets()
            : base(SetupSectionId.SectionPathSets, _defaultVariableName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSectionPathSets"/> class.
        /// </summary>
        /// <param name="typeId">Type.</param>
        protected DataSectionPathSets(SetupSectionId typeId)
            : base(typeId, _defaultVariableName)
        {
        }

        /// <summary>
        /// Gets or sets the path sets data.
        /// Each entry should contain any necessary "prequel" data that
        /// would be listed before this main entry.
        /// </summary>
        public List<SetupPathSetEntry> PathSets { get; set; } = new List<SetupPathSetEntry>();

        /// <inheritdoc />
        [JsonIgnore]
        public override int BaseDataSize
        {
            get
            {
                return
                    GetPrequelDataSize() +
                    (GetEntriesCount() * SetupPathSetEntry.SizeOf);
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc />
        public override string GetDeclarationTypeName()
        {
            return $"{SetupPathSetEntry.CTypeName} {VariableName}[]";
        }

        /// <inheritdoc />
        public override void WritePrequelData(StreamWriter sw)
        {
            foreach (var entry in PathSets.Where(x => x.Entry != null).OrderBy(x => x.EntryPointer.PointedToOffset))
            {
                sw.Write(entry.Entry.ToCDeclaration());
            }

            if (PathSets.Where(x => x.Entry != null).Any())
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
                PathSets,
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
                baseNameFormat = "path_set_not_used_{0}";
            }
            else
            {
                baseNameFormat = "path_set_{0}";
            }

            foreach (var entry in PathSets)
            {
                entry.DeserializeFix();

                if (!object.ReferenceEquals(null, entry.Entry))
                {
                    if (string.IsNullOrEmpty(entry.Entry.VariableName))
                    {
                        entry.Entry.VariableName = string.Format(baseNameFormat, index);
                    }

                    if (entry.EntryPointer.IsNull || entry.EntryPointer.PointedToOffset == 0)
                    {
                        entry.EntryPointer.AssignPointer(entry.Entry);
                    }
                }

                index++;
            }
        }

        /// <inheritdoc />
        public override int GetEntriesCount()
        {
            return PathSets.Count;
        }

        /// <inheritdoc />
        public override int GetPrequelDataSize()
        {
            return PathSets.Where(x => x.Entry != null).Sum(x => x.Entry.Ids.Count) * Config.TargetWordSize;
        }

        /// <inheritdoc />
        public override void Collect(IAssembleContext context)
        {
            foreach (var entry in PathSets)
            {
                context.AppendToDataSection(entry.Entry);
            }

            foreach (var entry in PathSets)
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
