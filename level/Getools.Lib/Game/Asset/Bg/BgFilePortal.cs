using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Bg
{
    public class BgFilePortal : IGetoolsLibObject
    {
        public byte NumberPoints { get; set; }

        public byte[] Padding { get; set; } = new byte[3];

        public List<Coord3df> Points { get; set; }

        /// <summary>
        /// Gets Getools.Lib reference id for the section/filler section.
        /// </summary>
        [JsonIgnore]
        public Guid MetaId { get; private set; } = Guid.NewGuid();
    }
}
