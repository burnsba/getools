﻿using System;
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
        /// C headers to #include when building .c files.
        /// </summary>
        public static List<string> IncludeHeaders = new List<string>()
        {
            "ultra64.h",
            "stagesetup.h",
        };

        public int PathTableDataOffset { get; set; }

        public List<PathTable> PathTableData { get; set; } = new List<PathTable>();

        public int PathTablesOffset { get; set; }

        public List<SetupPathTableEntry> PathTables { get; set; } = new List<SetupPathTableEntry>();

        public int PathLinkDataOffset { get; set; }

        public List<PathListing> PathLinkData { get; set; } = new List<PathListing>();

        public int PathLinksOffset { get; set; }

        public List<SetupPathLinkEntry> PathLinkEntries { get; set; } = new List<SetupPathLinkEntry>();

        public int IntrosOffset { get; set; }

        public List<IIntro> Intros { get; set; } = new List<IIntro>();

        public int ObjectsOffset { get; set; }

        public List<ISetupObject> Objects { get; set; } = new List<ISetupObject>();

        public int PathSetsDataOffset { get; set; }

        public List<PathSet> PathSetsData { get; set; } = new List<PathSet>();

        public int PathSetsOffset { get; set; }

        public List<SetupPathSetEntry> PathSets { get; set; } = new List<SetupPathSetEntry>();

        public int AiDataOffset { get; set; }

        public byte[] AiData { get; set; }

        public int AiListOffset { get; set; }

        public List<SetupAiListEntry> AiLists { get; set; } = new List<SetupAiListEntry>();

        public int PadListOffset { get; set; }

        public List<Pad> PadList { get; set; } = new List<Pad>();

        public int Pad3dListOffset { get; set; }

        public List<Pad3d> Pad3dList { get; set; } = new List<Pad3d>();

        public int PadNamesOffset { get; set; }

        public List<StringPointer> PadNames { get; set; } = new List<StringPointer>();

        public int Pad3dNamesOffset { get; set; }

        public List<StringPointer> Pad3dNames { get; set; } = new List<StringPointer>();

        public byte[] RodataPrequelFiller { get; set; }

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
        }
    }
}
