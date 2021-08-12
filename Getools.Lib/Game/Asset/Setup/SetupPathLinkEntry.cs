using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// PathLink.
    /// </summary>
    /// <remarks>
    /// Record ends with (UInt32)0.
    /// </remarks>
    public class SetupPathLinkEntry
    {
        public const UInt32 RecordDelimiter = 0;

        public int Offset { get; set; }

        public int NeighborsPointer { get; set; }
        public PathListing Neighbors { get; set; }

        public int IndexPointer { get; set; }
        public PathListing Indeces { get; set; }
    }
}
