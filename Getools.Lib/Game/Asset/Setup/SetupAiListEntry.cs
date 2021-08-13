using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// SetupAiListEntry.
    /// </summary>
    public class SetupAiListEntry
    {
        //public int Offset { get; set; }

        public UInt32 EntryPointer { get; set; }
        public UInt32 Id { get; set; }

        public AiFunction Function { get; set; }
    }
}
