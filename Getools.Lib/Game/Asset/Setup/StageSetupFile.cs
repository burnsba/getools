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

        public int PathTableDataOffset { get; set; }

        public List<PathTable> PathTableData { get; set; } = new List<PathTable>();

        public int PathTablesOffset { get; set; }

        public List<SetupPathTableEntry> PathTables { get; set; } = new List<SetupPathTableEntry>();

        public string PathTablesVariableName { get; set; } = "pathtbl";

        public int PathLinkDataOffset { get; set; }

        public List<PathListing> PathLinkData { get; set; } = new List<PathListing>();

        public int PathLinksOffset { get; set; }

        public List<SetupPathLinkEntry> PathLinkEntries { get; set; } = new List<SetupPathLinkEntry>();

        public string PathListVariableName { get; set; } = "pathlist";

        public int IntrosOffset { get; set; }

        public List<IIntro> Intros { get; set; } = new List<IIntro>();

        public string IntroListVariableName { get; set; } = "intro";

        public int ObjectsOffset { get; set; }

        public List<ISetupObject> Objects { get; set; } = new List<ISetupObject>();

        public string ObjectListVariableName { get; set; } = "objlist";

        public int PathSetsDataOffset { get; set; }

        public List<PathSet> PathSetsData { get; set; } = new List<PathSet>();

        public int PathSetsOffset { get; set; }

        public List<SetupPathSetEntry> PathSets { get; set; } = new List<SetupPathSetEntry>();

        public string PathSetsVariableName { get; set; } = "paths";

        public int AiDataOffset { get; set; }

        public byte[] AiData { get; set; }

        public int AiListOffset { get; set; }

        public List<SetupAiListEntry> AiLists { get; set; } = new List<SetupAiListEntry>();

        public string AiListsVariableName { get; set; } = "ailists";

        public int PadListOffset { get; set; }

        public List<Pad> PadList { get; set; } = new List<Pad>();

        public string PadListVariableName { get; set; } = "padlist";

        public int Pad3dListOffset { get; set; }

        public List<Pad3d> Pad3dList { get; set; } = new List<Pad3d>();

        public string Pad3dListVariableName { get; set; } = "pad3dlist";

        public int PadNamesOffset { get; set; }

        public List<StringPointer> PadNames { get; set; } = new List<StringPointer>();

        public string PadNamesVariableName { get; set; } = "padnames";

        public int Pad3dNamesOffset { get; set; }

        public List<StringPointer> Pad3dNames { get; set; } = new List<StringPointer>();

        public string Pad3dNamesVariableName { get; set; } = "pad3dnames";

        public byte[] RodataPrequelFiller { get; set; }

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
            sw.WriteLine($"char *{Pad3dNamesVariableName}[];");
            sw.WriteLine($"{SetupPathTableEntry.CTypeName} {PathTablesVariableName}[];");
            sw.WriteLine($"char *{PadNamesVariableName}[];");
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
            sw.WriteLine($"{Config.DefaultIndent}{Formatters.Strings.ToCPointerOrNull(PadNamesVariableName)},");
            sw.WriteLine($"{Config.DefaultIndent}{Formatters.Strings.ToCPointerOrNull(Pad3dNamesVariableName)}");
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

            sw.WriteLine($"char *{Pad3dNamesVariableName}[] = {{");

            Utility.AllButLast(
                Pad3dNames,
                x => sw.WriteLine(x.ToCValue(Config.DefaultIndent) + ","),
                x => sw.WriteLine(x.ToCValueOrNull(Config.DefaultIndent)));

            sw.WriteLine("};");

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

            sw.WriteLine($"char *{PadNamesVariableName}[] = {{");

            Utility.AllButLast(
                PadNames,
                x => sw.WriteLine(x.ToCValue(Config.DefaultIndent) + ","),
                x => sw.WriteLine(x.ToCValueOrNull(Config.DefaultIndent)));

            sw.WriteLine("};");

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

            // declare arrays used in ai script data
            // data needs to be sorted by address the ai script appears
            foreach (var entry in AiLists.Where(x => x.Function != null).OrderBy(x => x.EntryPointer))
            {
                sw.Write(entry.Function.ToCDeclaration());
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

            /*
             * End ai lists
             */
        }
    }
}
