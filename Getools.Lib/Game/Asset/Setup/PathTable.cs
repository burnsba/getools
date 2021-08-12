﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// PathTable.
    /// </summary>
    public class PathTable
    {
        public PathTable()
        {
        }

        public PathTable(IEnumerable<uint> ids)
        {
            Ids = ids.ToList();
        }

        public int Offset { get; set; }

        public List<uint> Ids { get; set; } = new List<uint>();
    }
}
