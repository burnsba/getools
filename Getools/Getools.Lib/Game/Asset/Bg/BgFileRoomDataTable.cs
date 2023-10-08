using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Bg
{
    /// <summary>
    /// Room section.
    /// </summary>
    public class BgFileRoomDataTable : IGetoolsLibObject
    {
        /// <summary>
        /// Entries.
        /// </summary>
        public List<BgFileRoomDataEntry> Entries { get; set; } = new List<BgFileRoomDataEntry>();

        /// <summary>
        /// Gets Getools.Lib reference id for the section/filler section.
        /// </summary>
        [JsonIgnore]
        public Guid MetaId { get; private set; } = Guid.NewGuid();
    }
}
