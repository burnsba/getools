using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Bg
{
    /// <summary>
    /// Visibility section.
    /// </summary>
    public class BgFileVisibilityCommandTable : IGetoolsLibObject
    {
        /// <summary>
        /// List of entries.
        /// </summary>
        public List<GlobalVisibilityCommand> Entries { get; set; } = new List<GlobalVisibilityCommand>();

        /// <summary>
        /// Gets Getools.Lib reference id for the section/filler section.
        /// </summary>
        [JsonIgnore]
        public Guid MetaId { get; private set; } = Guid.NewGuid();
    }
}
