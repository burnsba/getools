using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Getools.Lib.BinPack;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// AI list section.
    /// </summary>
    public class DataSectionAiList : SetupDataSection
    {
        private const string _defaultVariableName = "ailists";

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSectionAiList"/> class.
        /// </summary>
        public DataSectionAiList()
            : base(SetupSectionId.SectionAiList, _defaultVariableName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSectionAiList"/> class.
        /// </summary>
        /// <param name="typeId">Type.</param>
        protected DataSectionAiList(SetupSectionId typeId)
            : base(typeId, _defaultVariableName)
        {
        }

        /// <summary>
        /// Gets or sets the ai script listings.
        /// Each entry should contain any necessary "prequel" data that
        /// would be listed before this main entry.
        /// </summary>
        public List<SetupAiListEntry> AiLists { get; set; } = new List<SetupAiListEntry>();

        /// <inheritdoc />
        public override int BaseDataSize
        {
            get
            {
                return
                    GetPrequelDataSize() +
                    (GetEntriesCount() * SetupAiListEntry.SizeOf);
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc />
        public override string GetDeclarationTypeName()
        {
            return $"{SetupAiListEntry.CTypeName} {VariableName}[]";
        }

        /// <inheritdoc />
        public override void WritePrequelData(StreamWriter sw)
        {
            // Facility has a duplicate ailist entry (to the function), so check for duplicates
            // on the ai function data before declaration.
            var declared = new HashSet<string>();

            // Declare arrays used in ai script data.
            // Data needs to be sorted by address the ai script appears (for referenced AI functions).
            if (IsUnreferenced)
            {
                foreach (var entry in AiLists.Where(x => x.Function != null))
                {
                    if (!declared.Contains(entry.Function.VariableName))
                    {
                        sw.Write(entry.Function.ToCDeclaration());
                    }

                    declared.Add(entry.Function.VariableName);
                }
            }
            else
            {
                foreach (var entry in AiLists.Where(x => x.Function != null).OrderBy(x => x.EntryPointer.PointedToOffset))
                {
                    if (!declared.Contains(entry.Function.VariableName))
                    {
                        sw.Write(entry.Function.ToCDeclaration());
                    }

                    declared.Add(entry.Function.VariableName);
                }
            }

            if (AiLists.Where(x => x.Function != null).Any())
            {
                sw.WriteLine();
            }
        }

        /// <inheritdoc />
        public override void WriteSectionData(StreamWriter sw)
        {
            if (!IsUnreferenced)
            {
                sw.WriteLine($"{GetDeclarationTypeName()} = {{");

                // ai variables need to appear in the "natural" order
                Utility.ApplyCommaList(
                    sw.WriteLine,
                    AiLists.OrderBy(x => x.OrderIndex).ToList(),
                    (x, index) =>
                    {
                        var s = $"{Config.DefaultIndent}/* index = {index} */";
                        s += Environment.NewLine;
                        s += x.ToCInlineDeclaration(Config.DefaultIndent);
                        return s;
                    });

                sw.WriteLine("};");
            }
        }

        /// <inheritdoc />
        public override void DeserializeFix(int startingIndex = 0)
        {
            int index = startingIndex;
            string baseNameFormat = null;

            if (IsUnreferenced)
            {
                baseNameFormat = "ai_not_used_{0}";
            }
            else
            {
                baseNameFormat = "ai_{0}";
            }

            foreach (var entry in AiLists)
            {
                entry.OrderIndex = index;
                index++;
            }

            foreach (var entry in AiLists.OrderBy(x => x.EntryPointer))
            {
                entry.DeserializeFix();

                if (!object.ReferenceEquals(null, entry.Function))
                {
                    entry.Function.OrderIndex = entry.OrderIndex;

                    if (string.IsNullOrEmpty(entry.Function.VariableName))
                    {
                        entry.Function.VariableName = string.Format(baseNameFormat, entry.Function.OrderIndex);
                    }
                }
            }
        }

        /// <inheritdoc />
        public override int GetEntriesCount()
        {
            return AiLists.Count;
        }

        /// <inheritdoc />
        public override int GetPrequelDataSize()
        {
            return AiLists.Where(x => x.Function != null).Sum(x => x.Function.Data.Length);
        }

        /// <inheritdoc />
        public override void Collect(IAssembleContext context)
        {
            foreach (var entry in AiLists)
            {
                context.AppendToDataSection(entry.Function);
            }

            foreach (var entry in AiLists)
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
