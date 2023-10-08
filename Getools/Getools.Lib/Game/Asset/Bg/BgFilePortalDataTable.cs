using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Bg
{
    /// <summary>
    /// Portal section.
    /// </summary>
    public class BgFilePortalDataTable : IGetoolsLibObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BgFilePortalDataTable"/> class.
        /// </summary>
        public BgFilePortalDataTable()
        {
        }

        /// <summary>
        /// Entries.
        /// </summary>
        public List<BgFilePortalDataEntry> Entries { get; set; } = new List<BgFilePortalDataEntry>();

        /// <summary>
        /// Gets Getools.Lib reference id for the section/filler section.
        /// </summary>
        [JsonIgnore]
        public Guid MetaId { get; private set; } = Guid.NewGuid();
    }
}
