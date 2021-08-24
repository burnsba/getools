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

        /// <summary>
        /// Gets or sets the file offset that the main <see cref="PathTablesVariableName"/>
        /// declaration is located at.
        /// </summary>
        public int PathTablesOffset { get; set; }

        /// <summary>
        /// Gets or sets the path tables data.
        /// Each entry should contain any necessary "prequel" data that
        /// would be listed before this main entry.
        /// </summary>
        public List<SetupPathTableEntry> PathTables { get; set; } = new List<SetupPathTableEntry>();

        /// <summary>
        /// Gets or sets the variable name for this section.
        /// This will be used as a pointer in the main setup struct declaration,
        /// then used later in the file for the associated data declaration
        /// (the data being pointed to).
        /// </summary>
        public string PathTablesVariableName { get; set; } = "pathtbl";

        /// <summary>
        /// Gets or sets the file offset that the main <see cref="PathListVariableName"/>
        /// declaration is located at.
        /// </summary>
        public int PathLinksOffset { get; set; }

        /// <summary>
        /// Gets or sets the path link data.
        /// Each entry should contain any necessary "prequel" data that
        /// would be listed before this main entry.
        /// </summary>
        public List<SetupPathLinkEntry> PathLinkEntries { get; set; } = new List<SetupPathLinkEntry>();

        /// <summary>
        /// Gets or sets the variable name for this section.
        /// This will be used as a pointer in the main setup struct declaration,
        /// then used later in the file for the associated data declaration
        /// (the data being pointed to).
        /// </summary>
        public string PathListVariableName { get; set; } = "pathlist";

        /// <summary>
        /// Gets or sets the file offset that the main <see cref="IntroListVariableName"/>
        /// declaration is located at.
        /// </summary>
        public int IntrosOffset { get; set; }

        /// <summary>
        /// Gets or sets the intro data.
        /// Each entry should contain any necessary "prequel" data that
        /// would be listed before this main entry.
        /// </summary>
        public List<IIntro> Intros { get; set; } = new List<IIntro>();

        /// <summary>
        /// Gets or sets the variable name for this section.
        /// This will be used as a pointer in the main setup struct declaration,
        /// then used later in the file for the associated data declaration
        /// (the data being pointed to).
        /// </summary>
        public string IntroListVariableName { get; set; } = "intro";

        /// <summary>
        /// Gets or sets the file offset that the main <see cref="ObjectListVariableName"/>
        /// declaration is located at.
        /// </summary>
        public int ObjectsOffset { get; set; }

        /// <summary>
        /// Gets or sets the object prop declaration data.
        /// Each entry should contain any necessary "prequel" data that
        /// would be listed before this main entry.
        /// </summary>
        public List<ISetupObject> Objects { get; set; } = new List<ISetupObject>();

        /// <summary>
        /// Gets or sets the variable name for this section.
        /// This will be used as a pointer in the main setup struct declaration,
        /// then used later in the file for the associated data declaration
        /// (the data being pointed to).
        /// </summary>
        public string ObjectListVariableName { get; set; } = "objlist";

        /// <summary>
        /// Gets or sets the file offset that the main <see cref="PathSetsVariableName"/>
        /// declaration is located at.
        /// </summary>
        public int PathSetsOffset { get; set; }

        /// <summary>
        /// Gets or sets the path sets data.
        /// Each entry should contain any necessary "prequel" data that
        /// would be listed before this main entry.
        /// </summary>
        public List<SetupPathSetEntry> PathSets { get; set; } = new List<SetupPathSetEntry>();

        /// <summary>
        /// Gets or sets the variable name for this section.
        /// This will be used as a pointer in the main setup struct declaration,
        /// then used later in the file for the associated data declaration
        /// (the data being pointed to).
        /// </summary>
        public string PathSetsVariableName { get; set; } = "paths";

        /// <summary>
        /// Gets or sets the file offset that the main <see cref="AiListsVariableName"/>
        /// declaration is located at.
        /// </summary>
        public int AiListOffset { get; set; }

        /// <summary>
        /// Gets or sets the ai script listings.
        /// Each entry should contain any necessary "prequel" data that
        /// would be listed before this main entry.
        /// </summary>
        public List<SetupAiListEntry> AiLists { get; set; } = new List<SetupAiListEntry>();

        /// <summary>
        /// Gets or sets the variable name for this section.
        /// This will be used as a pointer in the main setup struct declaration,
        /// then used later in the file for the associated data declaration
        /// (the data being pointed to).
        /// </summary>
        public string AiListsVariableName { get; set; } = "ailists";

        /// <summary>
        /// Gets or sets the file offset that the main <see cref="PadListVariableName"/>
        /// declaration is located at.
        /// </summary>
        public int PadListOffset { get; set; }

        /// <summary>
        /// Gets or sets the pad listing.
        /// Each entry should contain any necessary "prequel" data that
        /// would be listed before this main entry.
        /// </summary>
        public List<Pad> PadList { get; set; } = new List<Pad>();

        /// <summary>
        /// Gets or sets the variable name for this section.
        /// This will be used as a pointer in the main setup struct declaration,
        /// then used later in the file for the associated data declaration
        /// (the data being pointed to).
        /// </summary>
        public string PadListVariableName { get; set; } = "padlist";

        /// <summary>
        /// Gets or sets the file offset that the main <see cref="Pad3dListVariableName"/>
        /// declaration is located at.
        /// </summary>
        public int Pad3dListOffset { get; set; }

        /// <summary>
        /// Gets or sets the pad3d listing.
        /// Each entry should contain any necessary "prequel" data that
        /// would be listed before this main entry.
        /// </summary>
        public List<Pad3d> Pad3dList { get; set; } = new List<Pad3d>();

        /// <summary>
        /// Gets or sets the variable name for this section.
        /// This will be used as a pointer in the main setup struct declaration,
        /// then used later in the file for the associated data declaration
        /// (the data being pointed to).
        /// </summary>
        public string Pad3dListVariableName { get; set; } = "pad3dlist";

        /// <summary>
        /// Gets or sets the file offset that the main <see cref="PadNamesVariableName"/>
        /// declaration is located at.
        /// </summary>
        public int PadNamesOffset { get; set; }

        /// <summary>
        /// Gets or sets the pad names list.
        /// </summary>
        public List<StringPointer> PadNames { get; set; } = new List<StringPointer>();

        /// <summary>
        /// Gets or sets the variable name for this section.
        /// This will be used as a pointer in the main setup struct declaration,
        /// then used later in the file for the associated data declaration
        /// (the data being pointed to).
        /// </summary>
        public string PadNamesVariableName { get; set; } = "padnames";

        /// <summary>
        /// Gets or sets the file offset that the main <see cref="Pad3dNamesVariableName"/>
        /// declaration is located at.
        /// </summary>
        public int Pad3dNamesOffset { get; set; }

        /// <summary>
        /// Gets or sets the pad3d names list.
        /// </summary>
        public List<StringPointer> Pad3dNames { get; set; } = new List<StringPointer>();

        /// <summary>
        /// Gets or sets the variable name for this section.
        /// This will be used as a pointer in the main setup struct declaration,
        /// then used later in the file for the associated data declaration
        /// (the data being pointed to).
        /// </summary>
        public string Pad3dNamesVariableName { get; set; } = "pad3dnames";

        /// <summary>
        /// Iterates over the collection after it has been deserialized
        /// and sets any remaining unset indeces or offsets.
        /// Updates variable names based on the indeces.
        /// </summary>
        public void DeserializeFix()
        {
            int index;

            index = 0;
            foreach (var entry in PathLinkEntries)
            {
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

            index = 0;
            foreach (var entry in PathSets)
            {
                if (!object.ReferenceEquals(null, entry.Entry) && string.IsNullOrEmpty(entry.Entry.VariableName))
                {
                    entry.Entry.VariableName = $"path_set_{index}";
                }

                index++;
            }

            index = 0;
            foreach (var entry in PathTables)
            {
                if (!object.ReferenceEquals(null, entry.Entry) && string.IsNullOrEmpty(entry.Entry.VariableName))
                {
                    entry.Entry.VariableName = $"path_table_{index}";
                }

                index++;
            }

            index = 0;
            foreach (var entry in AiLists)
            {
                entry.OrderIndex = index;
                index++;
            }

            index = 0;
            foreach (var entry in AiLists.OrderBy(x => x.EntryPointer))
            {
                if (!object.ReferenceEquals(null, entry.Function))
                {
                    entry.Function.OrderIndex = index;

                    if (string.IsNullOrEmpty(entry.Function.VariableName))
                    {
                        entry.Function.VariableName = $"ai_{entry.Function.OrderIndex}";
                    }

                    index++;
                }
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
            sw.WriteLine($"{Pad.CTypeName} {PadListVariableName}[];");
            sw.WriteLine($"{Pad3d.CTypeName} {Pad3dListVariableName}[];");
            sw.WriteLine($"s32 {ObjectListVariableName}[];");
            sw.WriteLine($"s32 {IntroListVariableName}[];");
            sw.WriteLine($"{SetupPathLinkEntry.CTypeName} {PathListVariableName}[];");

            if (Pad3dNames.Any())
            {
                sw.WriteLine($"char *{Pad3dNamesVariableName}[];");
            }
            else
            {
                sw.WriteLine("/* no pad3dnames */");
            }

            sw.WriteLine($"{SetupPathTableEntry.CTypeName} {PathTablesVariableName}[];");

            if (PadNames.Any())
            {
                sw.WriteLine($"char *{PadNamesVariableName}[];");
            }
            else
            {
                sw.WriteLine("/* no padnames */");
            }

            sw.WriteLine($"{SetupPathSetEntry.CTypeName} {PathSetsVariableName}[];");
            sw.WriteLine($"{SetupAiListEntry.CTypeName} {AiListsVariableName}[];");

            sw.WriteLine();

            sw.WriteLine($"{CTypeName} setup = {{");
            sw.WriteLine($"{Config.DefaultIndent}{Formatters.Strings.ToCPointerOrNull(PathTablesVariableName)},");
            sw.WriteLine($"{Config.DefaultIndent}{Formatters.Strings.ToCPointerOrNull(PathListVariableName)},");
            sw.WriteLine($"{Config.DefaultIndent}{Formatters.Strings.ToCPointerOrNull(IntroListVariableName)},");
            sw.WriteLine($"{Config.DefaultIndent}{Formatters.Strings.ToCPointerOrNull(ObjectListVariableName)},");
            sw.WriteLine($"{Config.DefaultIndent}{Formatters.Strings.ToCPointerOrNull(PathSetsVariableName)},");
            sw.WriteLine($"{Config.DefaultIndent}{Formatters.Strings.ToCPointerOrNull(AiListsVariableName)},");
            sw.WriteLine($"{Config.DefaultIndent}{Formatters.Strings.ToCPointerOrNull(PadListVariableName)},");
            sw.WriteLine($"{Config.DefaultIndent}{Formatters.Strings.ToCPointerOrNull(Pad3dListVariableName)},");

            if (PadNamesOffset > 0 && PadNames.Any())
            {
                sw.WriteLine($"{Config.DefaultIndent}{Formatters.Strings.ToCPointerOrNull(PadNamesVariableName)},");
            }
            else
            {
                sw.WriteLine($"{Config.DefaultIndent}NULL,");
            }

            if (Pad3dNamesOffset > 0 && Pad3dNames.Any())
            {
                sw.WriteLine($"{Config.DefaultIndent}{Formatters.Strings.ToCPointerOrNull(Pad3dNamesVariableName)}");
            }
            else
            {
                sw.WriteLine($"{Config.DefaultIndent}NULL,");
            }

            sw.WriteLine("};");

            sw.WriteLine();

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

            sw.WriteLine($"{Pad.CTypeName} {PadListVariableName}[] = {{");

            for (int i = 0; i < PadList.Count - 1; i++)
            {
                sw.WriteLine(PadList[i].ToCInlineDeclaration(Config.DefaultIndent) + ",");
            }

            if (PadList.Any())
            {
                sw.WriteLine(PadList.Last().ToCInlineDeclaration(Config.DefaultIndent));
            }

            sw.WriteLine("};");

            /*
             * End pad list
             */

            sw.WriteLine();
            sw.WriteLine();

            /******************************************************************************************
             * Begin pad3d list
             */

            sw.WriteLine($"{Pad3d.CTypeName} {Pad3dListVariableName}[] = {{");

            Utility.ApplyCommaList(
                sw.WriteLine,
                Pad3dList,
                x => x.ToCInlineDeclaration(Config.DefaultIndent));

            sw.WriteLine("};");

            /*
             * End pad3d list
             */

            sw.WriteLine();
            sw.WriteLine();

            /******************************************************************************************
             * Begin object list
             */

            sw.WriteLine($"s32 {ObjectListVariableName}[] = {{");

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

            sw.WriteLine();
            sw.WriteLine();

            /*
             * End object list
             */

            /******************************************************************************************
             * Begin intro definitions
             */

            sw.WriteLine($"s32 {IntroListVariableName}[] = {{");

            Utility.ApplyCommaList(
                sw.WriteLine,
                Intros,
                (x, index) =>
                {
                    var s = $"{Config.DefaultIndent}/* {nameof(IIntro.Type)} = {x.Type}; index = {index} */";
                    s += Environment.NewLine;
                    s += x.ToCInlineS32Array(Config.DefaultIndent);
                    return s;
                });

            sw.WriteLine("};");

            sw.WriteLine();
            sw.WriteLine();

            /*
             * End intro definitions
             */

            /******************************************************************************************
             * Begin path links
             */

            // declare arrays used in path listings
            foreach (var entry in PathLinkEntries.Where(x => x.Neighbors != null))
            {
                sw.Write(entry.Neighbors.ToCDeclaration());
            }

            sw.WriteLine();

            foreach (var entry in PathLinkEntries.Where(x => x.Indeces != null))
            {
                sw.Write(entry.Indeces.ToCDeclaration());
            }

            sw.WriteLine();

            ///// done with data, onto setup struct data

            sw.WriteLine($"{SetupPathLinkEntry.CTypeName} {PathListVariableName}[] = {{");

            Utility.ApplyCommaList(
                sw.WriteLine,
                PathLinkEntries,
                x => x.ToCInlineDeclaration(Config.DefaultIndent));

            sw.WriteLine("};");

            sw.WriteLine();
            sw.WriteLine();

            /*
             * End path links
             */

            /******************************************************************************************
             * Begin pad3d names
             */

            if (Pad3dNames.Any())
            {
                sw.WriteLine($"char *{Pad3dNamesVariableName}[] = {{");

                Utility.AllButLast(
                    Pad3dNames,
                    x => sw.WriteLine(x.ToCValue(Config.DefaultIndent) + ","),
                    x => sw.WriteLine(x.ToCValueOrNull(Config.DefaultIndent)));

                sw.WriteLine("};");
            }
            else
            {
                sw.WriteLine("/* no pad3dnames */");
            }

            sw.WriteLine();
            sw.WriteLine();

            /*
             * End pad3d names
             */

            /******************************************************************************************
             * Begin path tables
             */

            // declare arrays used in path tables
            foreach (var entry in PathTables.Where(x => x.Entry != null))
            {
                sw.Write(entry.Entry.ToCDeclaration());
            }

            sw.WriteLine();

            ///// done with data, onto setup struct data

            sw.WriteLine($"{SetupPathTableEntry.CTypeName} {PathTablesVariableName}[] = {{");

            Utility.ApplyCommaList(
                sw.WriteLine,
                PathTables,
                x => x.ToCInlineDeclaration(Config.DefaultIndent));

            sw.WriteLine("};");

            sw.WriteLine();
            sw.WriteLine();

            /*
             * End path tables
             */

            /******************************************************************************************
             * Begin pad names
             */

            if (PadNames.Any())
            {
                sw.WriteLine($"char *{PadNamesVariableName}[] = {{");

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

            sw.WriteLine();
            sw.WriteLine();

            /*
             * End pad names
             */

            /******************************************************************************************
             * Begin path sets
             */

            // declare arrays used in path sets
            foreach (var entry in PathSets.Where(x => x.Entry != null))
            {
                sw.Write(entry.Entry.ToCDeclaration());
            }

            sw.WriteLine();

            ///// done with data, onto setup struct data

            sw.WriteLine($"{SetupPathSetEntry.CTypeName} {PathSetsVariableName}[] = {{");

            Utility.ApplyCommaList(
                sw.WriteLine,
                PathSets,
                x => x.ToCInlineDeclaration(Config.DefaultIndent));

            sw.WriteLine("};");

            sw.WriteLine();
            sw.WriteLine();

            /*
             * End path sets
             */

            /******************************************************************************************
             * Begin ai lists
             */

            // scope
            {
                // Facility has a duplicate ailist entry (to the function), so check for duplicates
                // on the ai function data before declaration.
                var declared = new HashSet<string>();

                // declare arrays used in ai script data
                // data needs to be sorted by address the ai script appears
                foreach (var entry in AiLists.Where(x => x.Function != null).OrderBy(x => x.EntryPointer))
                {
                    if (!declared.Contains(entry.Function.VariableName))
                    {
                        sw.Write(entry.Function.ToCDeclaration());
                    }

                    declared.Add(entry.Function.VariableName);
                }

                sw.WriteLine();

                sw.WriteLine($"{SetupAiListEntry.CTypeName} {AiListsVariableName}[] = {{");

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

                sw.WriteLine();
                sw.WriteLine();
            }

            /*
             * End ai lists
             */
        }
    }
}
