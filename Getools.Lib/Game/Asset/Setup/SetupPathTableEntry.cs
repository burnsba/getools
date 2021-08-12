using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// SetupPathTableEntry.
    /// </summary>
    public class SetupPathTableEntry
    {
        public int Offset { get; set; }

        public UInt16 Unknown_00 { get; set; }
        public UInt16 Unknown_02 { get; set; }
        public int EntryPointer { get; set; }
        public PathTable Entry { get; set; }
        public UInt32 Unknown_08 { get; set; }
        public UInt32 Unknown_0C { get; set; }
    }
}
