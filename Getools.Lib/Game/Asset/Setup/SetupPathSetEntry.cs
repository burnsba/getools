using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// PathLink.
    /// </summary>
    public class SetupPathSetEntry
    {
        public int Offset { get; set; }

        public uint EntryPointer { get; set; }
        public PathSet Entry { get; set; }
        public uint Unknown_04 { get; set; }
    }
}
