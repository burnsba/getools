using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// PathLink.
    /// </summary>
    public class PathListing
    {
        public PathListing()
        {
        }

        public PathListing(IEnumerable<int> ids)
        {
            Ids = ids.ToList();
        }

        public int Offset { get; set; }

        /// <summary>
        /// Gets or sets the variable name used in source file.
        /// </summary>
        public string VariableName { get; set; }

        public List<int> Ids { get; set; } = new List<int>();
    }
}
