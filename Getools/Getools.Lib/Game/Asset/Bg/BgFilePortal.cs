using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Bg
{
    /// <summary>
    /// Portal definition in BG file.
    /// </summary>
    public class BgFilePortal : IGetoolsLibObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BgFilePortal"/> class.
        /// </summary>
        public BgFilePortal()
        {
        }

        /// <summary>
        /// Number of points in the portal.
        /// </summary>
        public byte NumberPoints { get; set; }

        /// <summary>
        /// Unused padding value after <see cref="NumberPoints"/>.
        /// </summary>
        public byte[] Padding { get; set; } = new byte[3];

        /// <summary>
        /// Points that define the portall.
        /// </summary>
        public List<Coord3df> Points { get; set; } = new();

        /// <summary>
        /// Gets Getools.Lib reference id for the section/filler section.
        /// </summary>
        [JsonIgnore]
        public Guid MetaId { get; private set; } = Guid.NewGuid();
    }
}
