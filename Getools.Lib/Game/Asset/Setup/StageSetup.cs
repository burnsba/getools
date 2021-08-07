using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Complete setup.
    /// </summary>
    public class StageSetup
    {
        public List<PathTable> PathTables { get; set; }

        public int PathTablesOffset { get; set; }

        public List<PathTable> PathLists { get; set; }

        public int PathListsOffset { get; set; }

        public List<int> Intros { get; set; }

        public int IntrosOffset { get; set; }

        public List<int> Objects { get; set; }

        public int ObjectsOffset { get; set; }

        public List<PathSet> PathSets { get; set; }

        public int PathSetsOffset { get; set; }

        public List<int> AiLists { get; set; }

        public int AiListsOffset { get; set; }

        public List<Pad> PadList1 { get; set; }

        public int PadList1Offset { get; set; }

        public List<Pad3d> Pad3dList { get; set; }

        public int Pad3dOffset { get; set; }

        public List<Pad> PadList2 { get; set; }

        public List<int> LinkedPathSets1 { get; set; }

        public List<int> LinkedPathSets2 { get; set; }

        public List<int> LinkedPathTables { get; set; }

        public List<int> PathSetEntries { get; set; }

        public List<int> UnknownList { get; set; }
    }
}
