using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Getools.Lib.BinPack;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Path list / path links section.
    /// </summary>
    public class DataSectionPathList : SetupDataSection
    {
        private const string _defaultVariableName = "pathlist";

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSectionPathList"/> class.
        /// </summary>
        public DataSectionPathList()
            : base(SetupSectionId.SectionPathLink, _defaultVariableName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSectionPathList"/> class.
        /// </summary>
        /// <param name="typeId">Type.</param>
        protected DataSectionPathList(SetupSectionId typeId)
            : base(typeId, _defaultVariableName)
        {
        }

        /// <summary>
        /// Gets or sets the path link data.
        /// Each entry should contain any necessary "prequel" data that
        /// would be listed before this main entry.
        /// </summary>
        public List<SetupPathLinkEntry> PathLinkEntries { get; set; } = new List<SetupPathLinkEntry>();

        /// <inheritdoc />
        public override int BaseDataSize
        {
            get
            {
                return
                    GetPrequelDataSize() +
                    (GetEntriesCount() * SetupPathLinkEntry.SizeOf);
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc />
        public override string GetDeclarationTypeName()
        {
            return $"{SetupPathLinkEntry.CTypeName} {VariableName}[]";
        }

        /// <inheritdoc />
        public override void WritePrequelData(StreamWriter sw)
        {
            if (IsUnreferenced)
            {
                foreach (var entry in PathLinkEntries
                    .Where(x =>
                        object.ReferenceEquals(null, x.Neighbors)
                        && object.ReferenceEquals(null, x.Indeces)
                        && x.IsNull))
                {
                    sw.WriteLine(entry.ToCDeclaration());
                }

                if (PathLinkEntries.Any(x =>
                    object.ReferenceEquals(null, x.Neighbors)
                    && object.ReferenceEquals(null, x.Indeces)
                    && x.IsNull))
                {
                    sw.WriteLine();
                }
            }

            foreach (var entry in PathLinkEntries.Where(x => x.Neighbors != null).OrderBy(x => x.NeighborsPointer))
            {
                sw.Write(entry.Neighbors.ToCDeclaration());
            }

            if (PathLinkEntries.Where(x => x.Neighbors != null).Any())
            {
                sw.WriteLine();
            }

            foreach (var entry in PathLinkEntries.Where(x => x.Indeces != null).OrderBy(x => x.IndexPointer))
            {
                sw.Write(entry.Indeces.ToCDeclaration());
            }

            if (PathLinkEntries.Where(x => x.Indeces != null).Any())
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
                PathLinkEntries,
                x => x.ToCInlineDeclaration(Config.DefaultIndent));

            sw.WriteLine("};");
        }

        /// <inheritdoc />
        public override void DeserializeFix(int startingIndex = 0)
        {
            int index = startingIndex;

            if (IsUnreferenced)
            {
                foreach (var entry in PathLinkEntries)
                {
                    entry.DeserializeFix();

                    if (!object.ReferenceEquals(null, entry.Neighbors) && string.IsNullOrEmpty(entry.Neighbors.VariableName))
                    {
                        entry.Neighbors.VariableName = $"path_neighbors_not_used_{index}";
                    }

                    if (!object.ReferenceEquals(null, entry.Indeces) && string.IsNullOrEmpty(entry.Indeces.VariableName))
                    {
                        entry.Indeces.VariableName = $"path_indeces_not_used_{index}";
                    }

                    if (object.ReferenceEquals(null, entry.Neighbors) && object.ReferenceEquals(null, entry.Indeces) && entry.IsNull)
                    {
                        entry.VariableName = $"path_not_used_{index}";
                    }

                    index++;
                }
            }
            else
            {
                foreach (var entry in PathLinkEntries)
                {
                    entry.DeserializeFix();

                    if (!object.ReferenceEquals(null, entry.Neighbors) && string.IsNullOrEmpty(entry.Neighbors.VariableName))
                    {
                        entry.Neighbors.VariableName = $"path_neighbors_{index}";
                    }

                    if (!object.ReferenceEquals(null, entry.Indeces) && string.IsNullOrEmpty(entry.Indeces.VariableName))
                    {
                        entry.Indeces.VariableName = $"path_indeces_{index}";
                    }

                    index++;
                }
            }
        }

        /// <inheritdoc />
        public override int GetEntriesCount()
        {
            return PathLinkEntries.Count;
        }

        /// <inheritdoc />
        public override int GetPrequelDataSize()
        {
            var neighborsSize = PathLinkEntries.Where(x => x.Neighbors != null).Sum(x => x.Neighbors.Ids.Count) * Config.TargetWordSize;
            var indecesSize = PathLinkEntries.Where(x => x.Indeces != null).Sum(x => x.Indeces.Ids.Count) * Config.TargetWordSize;
            return neighborsSize + indecesSize;
        }

        /// <inheritdoc />
        public override void Collect(IAssembleContext context)
        {
            foreach (var entry in PathLinkEntries)
            {
                context.AppendToDataSection(entry.Neighbors);
            }

            foreach (var entry in PathLinkEntries)
            {
                context.AppendToDataSection(entry.Indeces);
            }

            foreach (var entry in PathLinkEntries)
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
