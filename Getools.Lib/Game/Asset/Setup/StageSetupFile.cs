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
                // DataFormats.C,
                // DataFormats.Json,
                DataFormats.Bin,
            };

        /// <summary>
        /// Formats available to output a <see cref="StageSetupFile"/>.
        /// </summary>
        public static List<DataFormats> SupportedOutputFormats = new List<DataFormats>()
            {
                DataFormats.C,

                // DataFormats.Json,
                // DataFormats.Bin,
            };

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

        private StageSetupFile()
        {
        }

        /// <summary>
        /// Gets or sets the list of all known sections in the setup file.
        /// This includes non-main sections.
        /// </summary>
        public List<SetupDataSection> Sections { get; set; } = new List<SetupDataSection>();

        /// <summary>
        /// Gets the first known referenced section of type <see cref="DataSectionAiList"/>, or null.
        /// </summary>
        public DataSectionAiList SectionAiLists => Sections.OfType<DataSectionAiList>().Where(x => x.IsUnreferenced == false).FirstOrDefault();

        /// <summary>
        /// Gets the first known referenced section of type <see cref="DataSectionIntros"/>, or null.
        /// </summary>
        public DataSectionIntros SectionIntros => Sections.OfType<DataSectionIntros>().Where(x => x.IsUnreferenced == false).FirstOrDefault();

        /// <summary>
        /// Gets the first known referenced section of type <see cref="DataSectionObjects"/>, or null.
        /// </summary>
        public DataSectionObjects SectionObjects => Sections.OfType<DataSectionObjects>().Where(x => x.IsUnreferenced == false).FirstOrDefault();

        /// <summary>
        /// Gets the first known referenced section of type <see cref="DataSectionPad3dList"/>, or null.
        /// </summary>
        public DataSectionPad3dList SectionPad3dList => Sections.OfType<DataSectionPad3dList>().Where(x => x.IsUnreferenced == false).FirstOrDefault();

        /// <summary>
        /// Gets the first known referenced section of type <see cref="DataSectionPad3dNames"/>, or null.
        /// </summary>
        public DataSectionPad3dNames SectionPad3dNames => Sections.OfType<DataSectionPad3dNames>().Where(x => x.IsUnreferenced == false).FirstOrDefault();

        /// <summary>
        /// Gets the first known referenced section of type <see cref="DataSectionPadList"/>, or null.
        /// </summary>
        public DataSectionPadList SectionPadList => Sections.OfType<DataSectionPadList>().Where(x => x.IsUnreferenced == false).FirstOrDefault();

        /// <summary>
        /// Gets the first known referenced section of type <see cref="DataSectionPadNames"/>, or null.
        /// </summary>
        public DataSectionPadNames SectionPadNames => Sections.OfType<DataSectionPadNames>().Where(x => x.IsUnreferenced == false).FirstOrDefault();

        /// <summary>
        /// Gets the first known referenced section of type <see cref="DataSectionPathList"/>, or null.
        /// </summary>
        public DataSectionPathList SectionPathList => Sections.OfType<DataSectionPathList>().Where(x => x.IsUnreferenced == false).FirstOrDefault();

        /// <summary>
        /// Gets the first known referenced section of type <see cref="DataSectionPathSets"/>, or null.
        /// </summary>
        public DataSectionPathSets SectionPathSets => Sections.OfType<DataSectionPathSets>().Where(x => x.IsUnreferenced == false).FirstOrDefault();

        /// <summary>
        /// Gets the first known referenced section of type <see cref="DataSectionPathTable"/>, or null.
        /// </summary>
        public DataSectionPathTable SectionPathTables => Sections.OfType<DataSectionPathTable>().Where(x => x.IsUnreferenced == false).FirstOrDefault();

        /// <summary>
        /// Gets the first known referenced section of type <see cref="RefSectionRodata"/>, or null.
        /// </summary>
        public RefSectionRodata Rodata => Sections.OfType<RefSectionRodata>().FirstOrDefault();

        /// <summary>
        /// Gets all unreferenced sections.
        /// </summary>
        public IEnumerable<UnrefSectionUnknown> FillerBlocks => Sections.OfType<UnrefSectionUnknown>();

        /// <summary>
        /// Creates a new <see cref="StageSetupFile"/> without any sections.
        /// </summary>
        /// <returns>New object.</returns>
        public static StageSetupFile NewEmpty()
        {
            return new StageSetupFile();
        }

        /// <summary>
        /// Creates a new <see cref="StageSetupFile"/> with the "default" main section order.
        /// </summary>
        /// <returns>New object.</returns>
        /// <remarks>
        /// This is the majority default order, I think one or two setups violate this order.
        /// </remarks>
        public static StageSetupFile NewDefault()
        {
            var ssf = new StageSetupFile();

            ssf.Sections.Add(new DataSectionPadList());
            ssf.Sections.Add(new DataSectionPad3dList());
            ///// credits data
            ssf.Sections.Add(new DataSectionObjects());
            ssf.Sections.Add(new DataSectionIntros());
            ssf.Sections.Add(new DataSectionPathList());
            ssf.Sections.Add(new DataSectionPad3dNames());
            ssf.Sections.Add(new DataSectionPathTable());
            ssf.Sections.Add(new DataSectionPadNames());
            ssf.Sections.Add(new DataSectionPathSets());
            ssf.Sections.Add(new DataSectionAiList());

            return ssf;
        }

        /// <summary>
        /// Will slice the section and remove it from <see cref="Sections"/>.
        /// If the <paramref name="length"/> is -1 or equal to the length of the section,
        /// the entire section is remove and returned. If the length is only partial,
        /// the remaining bytes are split into a new <see cref="UnrefSectionUnknown"/>
        /// and placed back into the sections list.
        /// </summary>
        /// <param name="offset">Starting address of block to claim. Must be within an existing unclaimed section.</param>
        /// <param name="length">Number of bytes to claim. Must not exceed end of the unclaimed section.</param>
        /// <returns>Section extracted from unclaimed section.</returns>
        public UnrefSectionUnknown ClaimUnrefSectionBytes(int offset, int length)
        {
            if (length == 0 || length < -1)
            {
                throw new ArgumentException("Length must be a positive value or -1");
            }

            var section = FillerBlocks.Where(x => x.Offset <= offset).OrderByDescending(x => x.Offset).FirstOrDefault();

            if (object.ReferenceEquals(null, section))
            {
                throw new InvalidOperationException($"Could not find filler block associated to offset {offset}");
            }

            if (!(section.Offset <= offset && offset < section.Offset + section.Length))
            {
                throw new InvalidOperationException($"Offset paramter={offset} is outside the bounds of the nearest filler block.");
            }

            if (offset + length > section.Offset + section.Length)
            {
                throw new InvalidOperationException($"Data slice will exceed the length of the filler block.");
            }

            int takeLength = length;
            if (takeLength == -1)
            {
                takeLength = section.Length;
            }

            int prequel = offset - section.Offset;
            int remaining = section.Length - takeLength;

            var unknownSectionIndex = Sections.FindIndex(x => x.MetaId == section.MetaId);
            Sections.RemoveAt(unknownSectionIndex);

            if (prequel > 0)
            {
                var prequelBlock = new UnrefSectionUnknown(section.GetDataBytes().Take(prequel).ToArray());
                prequelBlock.Offset = section.Offset;
                Sections.Add(prequelBlock);
            }

            if (remaining > 0)
            {
                var remainBlock = new UnrefSectionUnknown(section.GetDataBytes().Skip(prequel + takeLength).Take(remaining).ToArray());
                remainBlock.Offset = section.Offset + prequel + takeLength;
                Sections.Add(remainBlock);
            }

            var returnBlock = new UnrefSectionUnknown(section.GetDataBytes().Skip(prequel).Take(takeLength).ToArray());
            returnBlock.Offset = section.Offset + prequel;

            return returnBlock;
        }

        /// <summary>
        /// Inserts a section before the first section of <paramref name="type"/>.
        /// If the section type is <see cref="SetupSectionId.DefaultUnknown"/>, it is inserted
        /// at index 0. Otherwise will throw if a section with the specified type can't be found.
        /// </summary>
        /// <param name="section">Section to insert.</param>
        /// <param name="type">Will insert the new section before the first section of this type.</param>
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

        /// <summary>
        /// Inserts a section before the first section of <paramref name="type"/>
        /// that begins at the exact offset.
        /// If the section type is <see cref="SetupSectionId.DefaultUnknown"/>, it is inserted
        /// at index 0. Otherwise will throw if a section with the specified type
        /// and exact offset can't be found.
        /// </summary>
        /// <param name="section">Section to insert.</param>
        /// <param name="type">Will insert the new section before the first section of this type.</param>
        /// <param name="offset">Exact offset of section to find.</param>
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

        /// <summary>
        /// Searches all main referenced sections and returns the first section
        /// that starts before the given offset.
        /// </summary>
        /// <param name="offset">Offset to search before.</param>
        /// <returns>Section, or throws exception.</returns>
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

        /// <summary>
        /// Searches for any (main or not, unreferenced or not) section that
        /// begins after the given offset. Will then return the number
        /// of bytes to the next section. If no subsequent section
        /// can be found, returns <paramref name="defaultReturn"/>.
        /// </summary>
        /// <param name="offset">Searches for a section that begins after this offset.</param>
        /// <param name="defaultReturn">Value to return if no section is found.</param>
        /// <returns>Bytes remaining or <paramref name="defaultReturn"/>.</returns>
        public int BytesToNextAnySection(int offset, int defaultReturn)
        {
            var nextSection = Sections
                .Where(x => x.Offset > offset)
                .OrderBy(x => x.Offset)
                .FirstOrDefault();

            if (object.ReferenceEquals(null, nextSection))
            {
                return defaultReturn;
            }

            return nextSection.Offset - offset;
        }

        /// <summary>
        /// Sorts the sections by offset.
        /// </summary>
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

            WriteForwardDeclaration(sw, SetupSectionId.SectionPadList);
            WriteForwardDeclaration(sw, SetupSectionId.SectionPad3dList);
            WriteForwardDeclaration(sw, SetupSectionId.SectionObjects);
            WriteForwardDeclaration(sw, SetupSectionId.SectionIntro);
            WriteForwardDeclaration(sw, SetupSectionId.SectionPathLink);
            WriteForwardDeclaration(sw, SetupSectionId.SectionPad3dNames);
            WriteForwardDeclaration(sw, SetupSectionId.SectionPathTable);
            WriteForwardDeclaration(sw, SetupSectionId.SectionPadNames);
            WriteForwardDeclaration(sw, SetupSectionId.SectionPathSets);
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
            sw.WriteLine($"{Config.DefaultIndent}{GetSectionPointer(SetupSectionId.SectionPadNames)},");
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
