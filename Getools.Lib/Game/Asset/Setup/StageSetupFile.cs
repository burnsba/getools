using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Getools.Lib.Error;
using Getools.Lib.Game.Asset.Intro;
using Getools.Lib.Game.Asset.SetupObject;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// This object cooresponds to an entire setup file.
    /// </summary>
    public class StageSetupFile
    {
        private static List<SetupSectionId> _defaultDataOrder = new List<SetupSectionId>()
        {
            SetupSectionId.Header,
            SetupSectionId.SectionPadList,
            SetupSectionId.SectionPad3dList,
            SetupSectionId.CreditsData,
            SetupSectionId.SectionObjects,
            SetupSectionId.SectionIntro,
            SetupSectionId.UnreferencedPathLinkPointer,
            SetupSectionId.UnreferencedPathLinkEntry,
            SetupSectionId.PathLinkEntries,
            SetupSectionId.SectionPathLink,
            SetupSectionId.SectionPad3dNames,
            SetupSectionId.PathTableEntries,
            SetupSectionId.SectionPathTable,
            SetupSectionId.SectionPadNames,
            SetupSectionId.PathSetEntries,
            SetupSectionId.SectionPathSets,
            SetupSectionId.UnreferencedAiFunctions,
            SetupSectionId.AiFunctionEntries,
            SetupSectionId.SectionAiList,
        };

        /// <summary>
        /// C file, type name. Should match known struct type.
        /// </summary>
        public const string CTypeName = "struct stagesetup";

        /// <summary>
        /// C headers to #include when building .c files.
        /// </summary>
        public static List<string> IncludeHeaders = new List<string>()
        {
            "ultra64.h",
            "bondtypes.h",
        };

        /// <summary>
        /// Formats available for reading in a <see cref="StageSetupFile"/>.
        /// </summary>
        public static List<DataFormats> SupportedInputFormats = new List<DataFormats>()
            {
                DataFormats.C,
                DataFormats.Json,
                DataFormats.Bin,
            };

        /// <summary>
        /// Formats available to output a <see cref="StageSetupFile"/>.
        /// </summary>
        public static List<DataFormats> SupportedOutputFormats = new List<DataFormats>()
            {
                DataFormats.C,
                DataFormats.Json,
                //DataFormats.Bin,
                //DataFormats.BetaBin,
            };

        private StageSetupFile()
        { }

        public static StageSetupFile NewEmpty()
        {
            return new StageSetupFile();
        }

        public static StageSetupFile NewDefault()
        {
            var ssf = new StageSetupFile();

            ssf.Sections.Add(new DataSectionPadList());
            ssf.Sections.Add(new DataSectionPad3dList());
            // credits data
            ssf.Sections.Add(new DataSectionObjects());
            ssf.Sections.Add(new DataSectionIntros());
            // path link null, neighbors, indeces
            ssf.Sections.Add(new DataSectionPathList());
            ssf.Sections.Add(new DataSectionPad3dNames());
            ssf.Sections.Add(new DataSectionPathTable());
            ssf.Sections.Add(new DataSectionPadNames());
            ssf.Sections.Add(new DataSectionPathSets());
            ssf.Sections.Add(new DataSectionAiList());

            return ssf;
        }

        ///// <summary>
        ///// Gets or sets the file offset that the main <see cref="PathTablesVariableName"/>
        ///// declaration is located at.
        ///// </summary>
        //public int PathTablesOffset { get; set; }

        ///// <summary>
        ///// For multiplayer maps, the <see cref="PathTables"/> will only contain
        ///// the default "end of list" entry, but there is still an unreferenced "not used (-1)" entry
        ///// before that section. Those items are listed here.
        ///// </summary>
        //public List<SetupPathTableEntry> UnreferencedPathTables { get; set; } = new List<SetupPathTableEntry>();

        ///// <summary>
        ///// Gets or sets the path tables data.
        ///// Each entry should contain any necessary "prequel" data that
        ///// would be listed before this main entry.
        ///// </summary>
        //public List<SetupPathTableEntry> PathTables { get; set; } = new List<SetupPathTableEntry>();

        ///// <summary>
        ///// Gets or sets the variable name for this section.
        ///// This will be used as a pointer in the main setup struct declaration,
        ///// then used later in the file for the associated data declaration
        ///// (the data being pointed to).
        ///// </summary>
        //public string PathTablesVariableName { get; set; } = "pathtbl";

        ///// <summary>
        ///// Gets or sets the file offset that the main <see cref="PathListVariableName"/>
        ///// declaration is located at.
        ///// </summary>
        //public int PathLinksOffset { get; set; }

        ///// <summary>
        ///// For multiplayer maps, the <see cref="PathLinkEntries"/> will only contain
        ///// the default "end of list" entry, but there is still an unreferenced "not used (-1)" entry
        ///// before that section. Those items are listed here.
        ///// </summary>
        //public List<SetupPathLinkEntry> UnreferencedPathLinkEntries { get; set; } = new List<SetupPathLinkEntry>();

        ///// <summary>
        ///// Gets or sets the path link data.
        ///// Each entry should contain any necessary "prequel" data that
        ///// would be listed before this main entry.
        ///// </summary>
        //public List<SetupPathLinkEntry> PathLinkEntries { get; set; } = new List<SetupPathLinkEntry>();

        ///// <summary>
        ///// Gets or sets the variable name for this section.
        ///// This will be used as a pointer in the main setup struct declaration,
        ///// then used later in the file for the associated data declaration
        ///// (the data being pointed to).
        ///// </summary>
        //public string PathListVariableName { get; set; } = "pathlist";

        ///// <summary>
        ///// Gets or sets the file offset that the main <see cref="IntroListVariableName"/>
        ///// declaration is located at.
        ///// </summary>
        //public int IntrosOffset { get; set; }

        ///// <summary>
        ///// Gets or sets the intro data.
        ///// Each entry should contain any necessary "prequel" data that
        ///// would be listed before this main entry.
        ///// </summary>
        //public List<IIntro> Intros { get; set; } = new List<IIntro>();

        ///// <summary>
        ///// Gets or sets the variable name for this section.
        ///// This will be used as a pointer in the main setup struct declaration,
        ///// then used later in the file for the associated data declaration
        ///// (the data being pointed to).
        ///// </summary>
        //public string IntroListVariableName { get; set; } = "intro";

        ///// <summary>
        ///// Gets or sets the file offset that the main <see cref="ObjectListVariableName"/>
        ///// declaration is located at.
        ///// </summary>
        //public int ObjectsOffset { get; set; }

        ///// <summary>
        ///// Gets or sets the object prop declaration data.
        ///// Each entry should contain any necessary "prequel" data that
        ///// would be listed before this main entry.
        ///// </summary>
        //public List<ISetupObject> Objects { get; set; } = new List<ISetupObject>();

        ///// <summary>
        ///// Gets or sets the variable name for this section.
        ///// This will be used as a pointer in the main setup struct declaration,
        ///// then used later in the file for the associated data declaration
        ///// (the data being pointed to).
        ///// </summary>
        //public string ObjectListVariableName { get; set; } = "objlist";

        ///// <summary>
        ///// Gets or sets the file offset that the main <see cref="PathSetsVariableName"/>
        ///// declaration is located at.
        ///// </summary>
        //public int PathSetsOffset { get; set; }

        ///// <summary>
        ///// For multiplayer maps, the <see cref="PathSets"/> will only contain
        ///// the default "end of list" entry, but there is still an unreferenced "not used (-1)" entry
        ///// before that section. Those items are listed here.
        ///// </summary>
        //public List<SetupPathSetEntry> UnreferencedPathSets { get; set; } = new List<SetupPathSetEntry>();

        ///// <summary>
        ///// Gets or sets the path sets data.
        ///// Each entry should contain any necessary "prequel" data that
        ///// would be listed before this main entry.
        ///// </summary>
        //public List<SetupPathSetEntry> PathSets { get; set; } = new List<SetupPathSetEntry>();

        ///// <summary>
        ///// Gets or sets the variable name for this section.
        ///// This will be used as a pointer in the main setup struct declaration,
        ///// then used later in the file for the associated data declaration
        ///// (the data being pointed to).
        ///// </summary>
        //public string PathSetsVariableName { get; set; } = "paths";

        ///// <summary>
        ///// Gets or sets the file offset that the main <see cref="AiListsVariableName"/>
        ///// declaration is located at.
        ///// </summary>
        //public int AiListOffset { get; set; }

        ///// <summary>
        ///// For multiplayer maps, the <see cref="AiLists"/> will only contain
        ///// the default "end of list" entry, but there is still an unreferenced "not used (0x04...)" entry
        ///// before that section. Those items are listed here.
        ///// </summary>
        //public List<SetupAiListEntry> UnreferencedAiLists { get; set; } = new List<SetupAiListEntry>();

        ///// <summary>
        ///// Gets or sets the ai script listings.
        ///// Each entry should contain any necessary "prequel" data that
        ///// would be listed before this main entry.
        ///// </summary>
        //public List<SetupAiListEntry> AiLists { get; set; } = new List<SetupAiListEntry>();

        ///// <summary>
        ///// Gets or sets the variable name for this section.
        ///// This will be used as a pointer in the main setup struct declaration,
        ///// then used later in the file for the associated data declaration
        ///// (the data being pointed to).
        ///// </summary>
        //public string AiListsVariableName { get; set; } = "ailists";

        ///// <summary>
        ///// Gets or sets the file offset that the main <see cref="PadListVariableName"/>
        ///// declaration is located at.
        ///// </summary>
        //public int PadListOffset { get; set; }

        ///// <summary>
        ///// Gets or sets the pad listing.
        ///// Each entry should contain any necessary "prequel" data that
        ///// would be listed before this main entry.
        ///// </summary>
        //public List<Pad> PadList { get; set; } = new List<Pad>();

        ///// <summary>
        ///// Gets or sets the variable name for this section.
        ///// This will be used as a pointer in the main setup struct declaration,
        ///// then used later in the file for the associated data declaration
        ///// (the data being pointed to).
        ///// </summary>
        //public string PadListVariableName { get; set; } = "padlist";

        ///// <summary>
        ///// Gets or sets the file offset that the main <see cref="Pad3dListVariableName"/>
        ///// declaration is located at.
        ///// </summary>
        //public int Pad3dListOffset { get; set; }

        ///// <summary>
        ///// Gets or sets the pad3d listing.
        ///// Each entry should contain any necessary "prequel" data that
        ///// would be listed before this main entry.
        ///// </summary>
        //public List<Pad3d> Pad3dList { get; set; } = new List<Pad3d>();

        ///// <summary>
        ///// Gets or sets the variable name for this section.
        ///// This will be used as a pointer in the main setup struct declaration,
        ///// then used later in the file for the associated data declaration
        ///// (the data being pointed to).
        ///// </summary>
        //public string Pad3dListVariableName { get; set; } = "pad3dlist";

        ///// <summary>
        ///// Gets or sets the file offset that the main <see cref="PadNamesVariableName"/>
        ///// declaration is located at.
        ///// </summary>
        //public int PadNamesOffset { get; set; }

        ///// <summary>
        ///// Gets or sets the pad names list.
        ///// </summary>
        //public List<StringPointer> PadNames { get; set; } = new List<StringPointer>();

        ///// <summary>
        ///// Gets or sets the variable name for this section.
        ///// This will be used as a pointer in the main setup struct declaration,
        ///// then used later in the file for the associated data declaration
        ///// (the data being pointed to).
        ///// </summary>
        //public string PadNamesVariableName { get; set; } = "padnames";

        ///// <summary>
        ///// Gets or sets the file offset that the main <see cref="Pad3dNamesVariableName"/>
        ///// declaration is located at.
        ///// </summary>
        //public int Pad3dNamesOffset { get; set; }

        ///// <summary>
        ///// Gets or sets the pad3d names list.
        ///// </summary>
        //public List<StringPointer> Pad3dNames { get; set; } = new List<StringPointer>();

        ///// <summary>
        ///// Gets or sets the variable name for this section.
        ///// This will be used as a pointer in the main setup struct declaration,
        ///// then used later in the file for the associated data declaration
        ///// (the data being pointed to).
        ///// </summary>
        //public string Pad3dNamesVariableName { get; set; } = "pad3dnames";

        /// <summary>
        /// How the data sections and filler sections are organized.
        /// Should be set when reading a .bin file.
        /// Otherwise use the "default" order.
        /// </summary>
        //internal List<SetupSectionId> DataOrder { get; set; } = _defaultDataOrder;

        public List<SetupDataSection> Sections { get; set; } = new List<SetupDataSection>();

        public DataSectionAiList SectionAiLists => Sections.OfType<DataSectionAiList>().Where(x => x.IsUnreferenced == false).FirstOrDefault();
        public DataSectionIntros SectionIntros => Sections.OfType<DataSectionIntros>().Where(x => x.IsUnreferenced == false).FirstOrDefault();
        public DataSectionObjects SectionObjects => Sections.OfType<DataSectionObjects>().Where(x => x.IsUnreferenced == false).FirstOrDefault();
        public DataSectionPad3dList SectionPad3dList => Sections.OfType<DataSectionPad3dList>().Where(x => x.IsUnreferenced == false).FirstOrDefault();
        public DataSectionPad3dNames SectionPad3dNames => Sections.OfType<DataSectionPad3dNames>().Where(x => x.IsUnreferenced == false).FirstOrDefault();
        public DataSectionPadList SectionPadList => Sections.OfType<DataSectionPadList>().Where(x => x.IsUnreferenced == false).FirstOrDefault();
        public DataSectionPadNames SectionPadNames => Sections.OfType<DataSectionPadNames>().Where(x => x.IsUnreferenced == false).FirstOrDefault();
        public DataSectionPathList SectionPathList => Sections.OfType<DataSectionPathList>().Where(x => x.IsUnreferenced == false).FirstOrDefault();
        public DataSectionPathSets SectionPathSets => Sections.OfType<DataSectionPathSets>().Where(x => x.IsUnreferenced == false).FirstOrDefault();
        public DataSectionPathTable SectionPathTables => Sections.OfType<DataSectionPathTable>().Where(x => x.IsUnreferenced == false).FirstOrDefault();

        public void AddSectionBefore(SetupDataSection section, SetupSectionId type)
        {
            if (type == SetupSectionId.DefaultUnknown)
            {
                Sections.Insert(0, section);
            }
            else
            {
                int index = Sections.FindIndex(0, x => x.TypeId == type);

                if (index >= 0)
                {
                    Sections.Insert(index, section);
                }
                else
                {
                    throw new KeyNotFoundException($"Could not find section with type={type}");
                }
            }
        }

        public void AddSectionBefore(SetupDataSection section, SetupSectionId type, int offset)
        {
            if (type == SetupSectionId.DefaultUnknown)
            {
                Sections.Insert(0, section);
            }
            else
            {
                int index = Sections.FindIndex(0, x => x.TypeId == type && x.Offset == offset);

                if (index >= 0)
                {
                    Sections.Insert(index, section);
                }
                else
                {
                    throw new KeyNotFoundException($"Could not find section with type={type} and offset={offset}");
                }
            }
        }

        public int PreviousSectionOffset(int offset)
        {
            var section = Sections
                .Where(x =>
                    x.IsMainSection == true
                    && x.IsUnreferenced == false
                    && x.Offset < offset)
                .OrderByDescending(x => x.Offset)
                .FirstOrDefault();

            if (object.ReferenceEquals(null, section))
            {
                throw new NullReferenceException($"Could not find main section prior to offset={offset}");
            }

            return section.Offset;
        }

        public int BytesToNextAnySection(int offset, int currentSectionSize)
        {
            var nextSection = Sections
                .Where(x => x.Offset > offset)
                .OrderBy(x => x.Offset)
                .FirstOrDefault();

            if (object.ReferenceEquals(null, nextSection))
            {
                return currentSectionSize;
            }

            return nextSection.Offset - offset;
        }

        public void SortSectionsByOffset()
        {
            Sections = Sections.OrderBy(x => x.Offset).ToList();
        }

        /// <summary>
        /// Iterates over the collection after it has been deserialized
        /// and sets any remaining unset indeces or offsets.
        /// Updates variable names based on the indeces.
        /// </summary>
        public void DeserializeFix()
        {
            int index;

            IEnumerable<SetupDataSection> sectionsByType = null;
            var seen = new HashSet<Guid>();

            // Need to look at similar kinds of sections. Want the unreferenced items to start at index 0,
            // and then referenced items to continue incrementing the index from there. That means
            // handling most of this grouping manually. Sections that don't contain unreferenced data (e.g., Intros)
            // can be handled automatically at the end.

            /* ************************************************************************************************** */
            /* AI functions */
            /* ************************************************************************************************** */

            index = 0;
            sectionsByType = Sections.Where(x => x.TypeId == SetupSectionId.UnreferencedAiFunctions);
            foreach (var section in sectionsByType)
            {
                section.DeserializeFix(index);
                index += section.GetEntriesCount();

                seen.Add(section.MetaId);
            }

            sectionsByType = Sections.Where(x => x.TypeId == SetupSectionId.SectionAiList);
            foreach (var section in sectionsByType)
            {
                section.DeserializeFix(index);
                index += section.GetEntriesCount();

                seen.Add(section.MetaId);
            }

            /* ************************************************************************************************** */
            /* path link / path list */
            /* ************************************************************************************************** */

            index = 0;
            sectionsByType = Sections.Where(x => x.TypeId == SetupSectionId.UnreferencedPathLinkPointer);
            foreach (var section in sectionsByType)
            {
                section.DeserializeFix(index);
                index += section.GetEntriesCount();

                seen.Add(section.MetaId);
            }

            sectionsByType = Sections.Where(x => x.TypeId == SetupSectionId.UnreferencedPathLinkEntry);
            foreach (var section in sectionsByType)
            {
                section.DeserializeFix(index);
                index += section.GetEntriesCount();

                seen.Add(section.MetaId);
            }

            sectionsByType = Sections.Where(x => x.TypeId == SetupSectionId.PathLinkEntries);
            foreach (var section in sectionsByType)
            {
                section.DeserializeFix(index);
                index += section.GetEntriesCount();

                seen.Add(section.MetaId);
            }

            /* ************************************************************************************************** */
            /* path sets */
            /* ************************************************************************************************** */

            index = 0;
            sectionsByType = Sections.Where(x => x.TypeId == SetupSectionId.UnreferencedPathSetEntries);
            foreach (var section in sectionsByType)
            {
                section.DeserializeFix(index);
                index += section.GetEntriesCount();

                seen.Add(section.MetaId);
            }

            sectionsByType = Sections.Where(x => x.TypeId == SetupSectionId.SectionPathSets);
            foreach (var section in sectionsByType)
            {
                section.DeserializeFix(index);
                index += section.GetEntriesCount();

                seen.Add(section.MetaId);
            }

            /* ************************************************************************************************** */
            /* path tables */
            /* ************************************************************************************************** */

            index = 0;
            sectionsByType = Sections.Where(x => x.TypeId == SetupSectionId.UnreferencedPathTableEntries);
            foreach (var section in sectionsByType)
            {
                section.DeserializeFix(index);
                index += section.GetEntriesCount();

                seen.Add(section.MetaId);
            }

            sectionsByType = Sections.Where(x => x.TypeId == SetupSectionId.SectionPathTable);
            foreach (var section in sectionsByType)
            {
                section.DeserializeFix(index);
                index += section.GetEntriesCount();

                seen.Add(section.MetaId);
            }

            /* ************************************************************************************************** */
            /* unreferenced / unknown */
            /* ************************************************************************************************** */

            index = 0;
            sectionsByType = Sections.Where(x => x.TypeId == SetupSectionId.UnreferencedUnknown);
            foreach (var section in sectionsByType)
            {
                section.DeserializeFix(index);
                index += section.GetEntriesCount();

                seen.Add(section.MetaId);
            }

            /* ************************************************************************************************** */
            /* Everything else */
            /* ************************************************************************************************** */

            var remainingSections = Sections.Where(x => !seen.Contains(x.MetaId));
            foreach (var section in remainingSections)
            {
                section.DeserializeFix();
            }

            //int index;

            //// start with unreferenced path link entries.
            //index = 0;
            //foreach (var entry in UnreferencedPathLinkEntries)
            //{
            //    if (!object.ReferenceEquals(null, entry.Neighbors) && string.IsNullOrEmpty(entry.Neighbors.VariableName))
            //    {
            //        entry.Neighbors.VariableName = $"path_neighbors_not_used_{index}";
            //    }

            //    if (!object.ReferenceEquals(null, entry.Indeces) && string.IsNullOrEmpty(entry.Indeces.VariableName))
            //    {
            //        entry.Indeces.VariableName = $"path_indeces_not_used_{index}";
            //    }

            //    if (object.ReferenceEquals(null, entry.Neighbors) && object.ReferenceEquals(null, entry.Indeces) && entry.IsNull)
            //    {
            //        entry.VariableName = $"path_not_used_{index}";
            //    }

            //    index++;
            //}

            //// Now update referenced path link entries.
            //// Don't reset index here.
            //foreach (var entry in PathLinkEntries)
            //{
            //    if (!object.ReferenceEquals(null, entry.Neighbors) && string.IsNullOrEmpty(entry.Neighbors.VariableName))
            //    {
            //        entry.Neighbors.VariableName = $"path_neighbors_{index}";
            //    }

            //    if (!object.ReferenceEquals(null, entry.Indeces) && string.IsNullOrEmpty(entry.Indeces.VariableName))
            //    {
            //        entry.Indeces.VariableName = $"path_indeces_{index}";
            //    }

            //    index++;
            //}

            //// start with unreferenced path link entries.
            //index = 0;
            //foreach (var entry in UnreferencedPathSets)
            //{
            //    if (!object.ReferenceEquals(null, entry.Entry) && string.IsNullOrEmpty(entry.Entry.VariableName))
            //    {
            //        entry.Entry.VariableName = $"path_set_not_used_{index}";
            //    }

            //    index++;
            //}

            //// Now update referenced path link entries.
            //// Don't reset index here.
            //foreach (var entry in PathSets)
            //{
            //    if (!object.ReferenceEquals(null, entry.Entry) && string.IsNullOrEmpty(entry.Entry.VariableName))
            //    {
            //        entry.Entry.VariableName = $"path_set_{index}";
            //    }

            //    index++;
            //}

            //// start with unreferenced path table entries.
            //index = 0;
            //foreach (var entry in UnreferencedPathTables)
            //{
            //    if (!object.ReferenceEquals(null, entry.Entry) && string.IsNullOrEmpty(entry.Entry.VariableName))
            //    {
            //        entry.Entry.VariableName = $"path_table_not_used_{index}";
            //    }

            //    index++;
            //}

            //// Now update referenced path table entries.
            //// Don't reset index here.
            //foreach (var entry in PathTables)
            //{
            //    if (!object.ReferenceEquals(null, entry.Entry) && string.IsNullOrEmpty(entry.Entry.VariableName))
            //    {
            //        entry.Entry.VariableName = $"path_table_{index}";
            //    }

            //    index++;
            //}

            //// Start with unreferenced AI List entries.
            //index = 0;
            //foreach (var entry in UnreferencedAiLists)
            //{
            //    entry.OrderIndex = index;
            //    index++;
            //}

            //// Now update referenced AI List entries.
            //// Don't reset index here.
            //foreach (var entry in AiLists)
            //{
            //    entry.OrderIndex = index;
            //    index++;
            //}

            //// Start with unreferenced AI List entries.
            //index = 0;
            //foreach (var entry in UnreferencedAiLists /* unknown order */)
            //{
            //    if (!object.ReferenceEquals(null, entry.Function))
            //    {
            //        entry.Function.OrderIndex = index;

            //        if (string.IsNullOrEmpty(entry.Function.VariableName))
            //        {
            //            entry.Function.VariableName = $"ai_not_used_{entry.Function.OrderIndex}";
            //        }

            //        index++;
            //    }
            //}

            //// Now update referenced AI List entries.
            //// Don't reset index here.
            //foreach (var entry in AiLists.OrderBy(x => x.EntryPointer))
            //{
            //    if (!object.ReferenceEquals(null, entry.Function))
            //    {
            //        entry.Function.OrderIndex = index;

            //        if (string.IsNullOrEmpty(entry.Function.VariableName))
            //        {
            //            entry.Function.VariableName = $"ai_{entry.Function.OrderIndex}";
            //        }

            //        index++;
            //    }
            //}

            //// give any credits containers a variable name
            //index = 0;
            //foreach (var entry in Intros.OfType<IntroCredits>().Where(x => x.Credits != null))
            //{
            //    if (string.IsNullOrEmpty(entry.Credits.VariableName))
            //    {
            //        entry.Credits.VariableName = $"credits_data_{index}";
            //    }

            //    index++;
            //}
        }

        /// <summary>
        /// Builds the entire .c file describing setup and writes to stream at the current position.
        /// Assumes that the setup has already been organized/sorted (i.e., <see cref="DeserializeFix"/>
        /// has already been called).
        /// </summary>
        /// <param name="sw">Stream to write to</param>
        internal void WriteToCFile(StreamWriter sw)
        {
            sw.WriteLine("/*");

            foreach (var prefix in Config.COutputPrefix)
            {
                sw.WriteLine($"* {prefix}");
            }

            sw.WriteLine($"* {DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}");

            var assemblyInfo = Utility.GetAutoGeneratedAssemblyVersion();

            sw.WriteLine($"* {assemblyInfo}");
            sw.WriteLine("*/");
            sw.WriteLine();

            foreach (var filename in IncludeHeaders)
            {
                sw.WriteLine($"#include \"{filename}\"");
            }

            sw.WriteLine();

            sw.WriteLine("// forward declarations");
            
            //sw.WriteLine($"{Pad.CTypeName} {PadListVariableName}[];");
            WriteForwardDeclaration(sw, SetupSectionId.SectionPadList);

            //sw.WriteLine($"{Pad3d.CTypeName} {Pad3dListVariableName}[];");
            WriteForwardDeclaration(sw, SetupSectionId.SectionPad3dList);
            
            //sw.WriteLine($"s32 {ObjectListVariableName}[];");
            WriteForwardDeclaration(sw, SetupSectionId.SectionObjects);

            //sw.WriteLine($"s32 {IntroListVariableName}[];");
            WriteForwardDeclaration(sw, SetupSectionId.SectionIntro);

            //sw.WriteLine($"{SetupPathLinkEntry.CTypeName} {PathListVariableName}[];");
            WriteForwardDeclaration(sw, SetupSectionId.SectionPathLink);

            //if (Pad3dNames.Any())
            //{
            //    sw.WriteLine($"char *{Pad3dNamesVariableName}[];");
            //}
            //else
            //{
            //    sw.WriteLine("/* no pad3dnames */");
            //}
            WriteForwardDeclaration(sw, SetupSectionId.SectionPad3dNames);

            //sw.WriteLine($"{SetupPathTableEntry.CTypeName} {PathTablesVariableName}[];");
            WriteForwardDeclaration(sw, SetupSectionId.SectionPathTable);

            //if (PadNames.Any())
            //{
            //    sw.WriteLine($"char *{PadNamesVariableName}[];");
            //}
            //else
            //{
            //    sw.WriteLine("/* no padnames */");
            //}
            WriteForwardDeclaration(sw, SetupSectionId.SectionPadNames);

            //sw.WriteLine($"{SetupPathSetEntry.CTypeName} {PathSetsVariableName}[];");
            WriteForwardDeclaration(sw, SetupSectionId.SectionPathSets);

            //sw.WriteLine($"{SetupAiListEntry.CTypeName} {AiListsVariableName}[];");
            WriteForwardDeclaration(sw, SetupSectionId.SectionAiList);

            sw.WriteLine();

            sw.WriteLine($"{CTypeName} setup = {{");
            sw.WriteLine($"{Config.DefaultIndent}{GetSectionPointer(SetupSectionId.SectionPathTable)},");
            sw.WriteLine($"{Config.DefaultIndent}{GetSectionPointer(SetupSectionId.SectionPathLink)},");
            sw.WriteLine($"{Config.DefaultIndent}{GetSectionPointer(SetupSectionId.SectionIntro)},");
            sw.WriteLine($"{Config.DefaultIndent}{GetSectionPointer(SetupSectionId.SectionObjects)},");
            sw.WriteLine($"{Config.DefaultIndent}{GetSectionPointer(SetupSectionId.SectionPathSets)},");
            sw.WriteLine($"{Config.DefaultIndent}{GetSectionPointer(SetupSectionId.SectionAiList)},");
            sw.WriteLine($"{Config.DefaultIndent}{GetSectionPointer(SetupSectionId.SectionPadList)},");
            sw.WriteLine($"{Config.DefaultIndent}{GetSectionPointer(SetupSectionId.SectionPad3dList)},");

            //if (PadNamesOffset > 0 && PadNames.Any())
            //{
            //    sw.WriteLine($"{Config.DefaultIndent}{Formatters.Strings.ToCPointerOrNull(PadNamesVariableName)},");
            //}
            //else
            //{
            //    sw.WriteLine($"{Config.DefaultIndent}NULL,");
            //}
            sw.WriteLine($"{Config.DefaultIndent}{GetSectionPointer(SetupSectionId.SectionPadNames)},");

            //if (Pad3dNamesOffset > 0 && Pad3dNames.Any())
            //{
            //    sw.WriteLine($"{Config.DefaultIndent}{Formatters.Strings.ToCPointerOrNull(Pad3dNamesVariableName)}");
            //}
            //else
            //{
            //    sw.WriteLine($"{Config.DefaultIndent}NULL,");
            //}
            sw.WriteLine($"{Config.DefaultIndent}{GetSectionPointer(SetupSectionId.SectionPad3dNames)}");

            sw.WriteLine("};");

            sw.WriteLine();

            foreach (var section in Sections)
            {
                section.WritePrequelData(sw);
                section.WriteSectionData(sw);

                sw.WriteLine();
                sw.WriteLine();
            }

            /*
             * standard section order:
             *
             * - pad list
             * - pad3d list
             * - object list
             * - intro definitions
             * - path links
             * - pad3d names
             * - path tables
             * - pad names
             * - path sets
             * - ai lists
             */

            /******************************************************************************************
             * Begin pad list
             */

            //sw.WriteLine($"{Pad.CTypeName} {PadListVariableName}[] = {{");

            //for (int i = 0; i < PadList.Count - 1; i++)
            //{
            //    sw.WriteLine(PadList[i].ToCInlineDeclaration(Config.DefaultIndent) + ",");
            //}

            //if (PadList.Any())
            //{
            //    sw.WriteLine(PadList.Last().ToCInlineDeclaration(Config.DefaultIndent));
            //}

            //sw.WriteLine("};");

            ///*
            // * End pad list
            // */

            //sw.WriteLine();
            //sw.WriteLine();

            /******************************************************************************************
             * Begin pad3d list
             */

            //sw.WriteLine($"{Pad3d.CTypeName} {Pad3dListVariableName}[] = {{");

            //Utility.ApplyCommaList(
            //    sw.WriteLine,
            //    Pad3dList,
            //    x => x.ToCInlineDeclaration(Config.DefaultIndent));

            //sw.WriteLine("};");

            ///*
            // * End pad3d list
            // */

            //sw.WriteLine();
            //sw.WriteLine();

            /******************************************************************************************
             * Optional data section: credits data block
             */
            //if (Intros.OfType<IntroCredits>().Where(x => x.Credits != null).Any())
            //{
            //    foreach (var entry in Intros.OfType<IntroCredits>().Where(x => x.Credits != null))
            //    {
            //        sw.WriteLine(entry.Credits.ToCDeclaration());
            //    }

            //    sw.WriteLine();
            //    sw.WriteLine();
            //}

            /******************************************************************************************
             * Begin object list
             */

            //sw.WriteLine($"s32 {ObjectListVariableName}[] = {{");

            //Utility.ApplyCommaList(
            //    sw.WriteLine,
            //    Objects,
            //    (x, index) =>
            //    {
            //        var s = $"{Config.DefaultIndent}/* {nameof(ISetupObject.Type)} = {x.Type}; index = {index} */";
            //        s += Environment.NewLine;
            //        s += x.ToCInlineS32Array(Config.DefaultIndent);
            //        return s;
            //    });

            //sw.WriteLine("};");

            sw.WriteLine();
            sw.WriteLine();

            /*
             * End object list
             */

            /******************************************************************************************
             * Begin intro definitions
             */

            //sw.WriteLine($"s32 {IntroListVariableName}[] = {{");

            //Utility.ApplyCommaList(
            //    sw.WriteLine,
            //    Intros,
            //    (x, index) =>
            //    {
            //        var s = $"{Config.DefaultIndent}/* {nameof(IIntro.Type)} = {x.Type}; index = {index} */";
            //        s += Environment.NewLine;
            //        s += x.ToCInlineS32Array(Config.DefaultIndent);
            //        return s;
            //    });

            //sw.WriteLine("};");

            //sw.WriteLine();
            //sw.WriteLine();

            /*
             * End intro definitions
             */

            /******************************************************************************************
             * Begin path links
             */

            // declare arrays contained in bin but unreferenced in main table
            //foreach (var entry in UnreferencedPathLinkEntries.Where(x =>
            //    object.ReferenceEquals(null, x.Neighbors)
            //    && object.ReferenceEquals(null, x.Indeces)
            //    && x.IsNull))
            //{
            //    sw.WriteLine(entry.ToCDeclaration());
            //}

            //if (UnreferencedPathLinkEntries.Any(x =>
            //    object.ReferenceEquals(null, x.Neighbors)
            //    && object.ReferenceEquals(null, x.Indeces)
            //    && x.IsNull))
            //{
            //    sw.WriteLine();
            //}

            //foreach (var entry in UnreferencedPathLinkEntries.Where(x => x.Neighbors != null))
            //{
            //    sw.Write(entry.Neighbors.ToCDeclaration());
            //}

            //if (UnreferencedPathLinkEntries.Where(x => x.Neighbors != null).Any())
            //{
            //    sw.WriteLine();
            //}

            //foreach (var entry in UnreferencedPathLinkEntries.Where(x => x.Indeces != null))
            //{
            //    sw.Write(entry.Indeces.ToCDeclaration());
            //}

            //if (UnreferencedPathLinkEntries.Where(x => x.Indeces != null).Any())
            //{
            //    sw.WriteLine();
            //}

            //// declare arrays used in path listings
            //foreach (var entry in PathLinkEntries.Where(x => x.Neighbors != null))
            //{
            //    sw.Write(entry.Neighbors.ToCDeclaration());
            //}

            //if (PathLinkEntries.Where(x => x.Neighbors != null).Any())
            //{
            //    sw.WriteLine();
            //}

            //foreach (var entry in PathLinkEntries.Where(x => x.Indeces != null))
            //{
            //    sw.Write(entry.Indeces.ToCDeclaration());
            //}

            //if (PathLinkEntries.Where(x => x.Indeces != null).Any())
            //{
            //    sw.WriteLine();
            //}

            ///// done with data, onto setup struct data

            //sw.WriteLine($"{SetupPathLinkEntry.CTypeName} {PathListVariableName}[] = {{");

            //Utility.ApplyCommaList(
            //    sw.WriteLine,
            //    PathLinkEntries,
            //    x => x.ToCInlineDeclaration(Config.DefaultIndent));

            //sw.WriteLine("};");

            //sw.WriteLine();
            //sw.WriteLine();

            /*
             * End path links
             */

            /******************************************************************************************
             * Begin pad3d names
             */

            //if (Pad3dNames.Any())
            //{
            //    sw.WriteLine($"char *{Pad3dNamesVariableName}[] = {{");

            //    Utility.AllButLast(
            //        Pad3dNames,
            //        x => sw.WriteLine(x.ToCValue(Config.DefaultIndent) + ","),
            //        x => sw.WriteLine(x.ToCValueOrNull(Config.DefaultIndent)));

            //    sw.WriteLine("};");
            //}
            //else
            //{
            //    sw.WriteLine("/* no pad3dnames */");
            //}

            //sw.WriteLine();
            //sw.WriteLine();

            /*
             * End pad3d names
             */

            /******************************************************************************************
             * Begin path tables
             */

            //// declare arrays contained in bin but unreferenced in main table
            //foreach (var entry in UnreferencedPathTables.Where(x => x.Entry != null))
            //{
            //    sw.Write(entry.Entry.ToCDeclaration());
            //}

            //if (UnreferencedPathTables.Where(x => x.Entry != null).Any())
            //{
            //    sw.WriteLine();
            //}

            //// declare arrays used in path tables
            //foreach (var entry in PathTables.Where(x => x.Entry != null))
            //{
            //    sw.Write(entry.Entry.ToCDeclaration());
            //}

            //if (PathTables.Where(x => x.Entry != null).Any())
            //{
            //    sw.WriteLine();
            //}

            /////// done with data, onto setup struct data

            //sw.WriteLine($"{SetupPathTableEntry.CTypeName} {PathTablesVariableName}[] = {{");

            //Utility.ApplyCommaList(
            //    sw.WriteLine,
            //    PathTables,
            //    x => x.ToCInlineDeclaration(Config.DefaultIndent));

            //sw.WriteLine("};");

            sw.WriteLine();
            sw.WriteLine();

            /*
             * End path tables
             */

            /******************************************************************************************
             * Begin pad names
             */

            //if (PadNames.Any())
            //{
            //    sw.WriteLine($"char *{PadNamesVariableName}[] = {{");

            //    Utility.AllButLast(
            //        PadNames,
            //        x => sw.WriteLine(x.ToCValue(Config.DefaultIndent) + ","),
            //        x => sw.WriteLine(x.ToCValueOrNull(Config.DefaultIndent)));

            //    sw.WriteLine("};");
            //}
            //else
            //{
            //    sw.WriteLine("/* no padnames */");
            //}

            //sw.WriteLine();
            //sw.WriteLine();

            /*
             * End pad names
             */

            /******************************************************************************************
             * Begin path sets
             */

            //// declare arrays contained in bin but unreferenced in main table
            //foreach (var entry in UnreferencedPathSets.Where(x => x.Entry != null))
            //{
            //    sw.Write(entry.Entry.ToCDeclaration());
            //}

            //if (UnreferencedPathSets.Where(x => x.Entry != null).Any())
            //{
            //    sw.WriteLine();
            //}

            //// declare arrays used in path sets
            //foreach (var entry in PathSets.Where(x => x.Entry != null))
            //{
            //    sw.Write(entry.Entry.ToCDeclaration());
            //}

            //if (PathSets.Where(x => x.Entry != null).Any())
            //{
            //    sw.WriteLine();
            //}

            ///// done with data, onto setup struct data

            //sw.WriteLine($"{SetupPathSetEntry.CTypeName} {PathSetsVariableName}[] = {{");

            //Utility.ApplyCommaList(
            //    sw.WriteLine,
            //    PathSets,
            //    x => x.ToCInlineDeclaration(Config.DefaultIndent));

            //sw.WriteLine("};");

            //sw.WriteLine();
            //sw.WriteLine();

            /*
             * End path sets
             */

            /******************************************************************************************
             * Begin ai lists
             */

            //// scope
            //{
            //    // Facility has a duplicate ailist entry (to the function), so check for duplicates
            //    // on the ai function data before declaration.
            //    var declared = new HashSet<string>();

            //    // Declare arrays used in ai script data.
            //    // Data needs to be sorted by address the ai script appears (for referenced AI functions).
            //    foreach (var entry in UnreferencedAiLists.Where(x => x.Function != null))
            //    {
            //        if (!declared.Contains(entry.Function.VariableName))
            //        {
            //            sw.Write(entry.Function.ToCDeclaration());
            //        }

            //        declared.Add(entry.Function.VariableName);
            //    }

            //    foreach (var entry in AiLists.Where(x => x.Function != null).OrderBy(x => x.EntryPointer))
            //    {
            //        if (!declared.Contains(entry.Function.VariableName))
            //        {
            //            sw.Write(entry.Function.ToCDeclaration());
            //        }

            //        declared.Add(entry.Function.VariableName);
            //    }

            //    sw.WriteLine();

            //    sw.WriteLine($"{SetupAiListEntry.CTypeName} {AiListsVariableName}[] = {{");

            //    // ai variables need to appear in the "natural" order
            //    Utility.ApplyCommaList(
            //        sw.WriteLine,
            //        AiLists.OrderBy(x => x.OrderIndex).ToList(),
            //        (x, index) =>
            //        {
            //            var s = $"{Config.DefaultIndent}/* index = {index} */";
            //            s += Environment.NewLine;
            //            s += x.ToCInlineDeclaration(Config.DefaultIndent);
            //            return s;
            //        });

            //    sw.WriteLine("};");

            //    sw.WriteLine();
            //    sw.WriteLine();
            //}

            /*
             * End ai lists
             */
        }

        private void WriteForwardDeclaration(StreamWriter sw, SetupSectionId typeId, string prefix = "")
        {
            var section = Sections.Where(x => x.IsMainSection && x.TypeId == typeId && x.IsUnreferenced == false).FirstOrDefault();

            if (object.ReferenceEquals(null, section))
            {
                return;
            }

            sw.WriteLine($"{prefix}{section.GetDeclarationTypeName()};");
        }

        private string GetSectionPointer(SetupSectionId typeId)
        {
            var section = Sections.Where(x => x.IsMainSection && x.TypeId == typeId && x.IsUnreferenced == false).FirstOrDefault();

            if (object.ReferenceEquals(null, section))
            {
                // return default null pointer text
                return Formatters.Strings.ToCPointerOrNull(null);
            }

            return Formatters.Strings.ToCPointerOrNull(section.VariableName);
        }
    }
}
