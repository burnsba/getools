using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Bg
{
    /// <summary>
    /// Point table section.
    /// </summary>
    public class BgFilePointTableEntry
    {
        /// <summary>
        /// Verteces.
        /// </summary>
        public List<GbiVtx> Verteces { get; set; } = new List<GbiVtx>();
    }
}
