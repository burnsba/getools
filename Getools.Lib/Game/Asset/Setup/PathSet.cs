using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// PathSet.
    /// </summary>
    public class PathSet
    {
        public PathSet()
        {
        }

        public PathSet(IEnumerable<uint> ids)
        {
            Ids = ids.ToList();
        }

        public int Offset { get; set; }

        public List<uint> Ids { get; set; } = new List<uint>();
    }
}
