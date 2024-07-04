using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;
using Getools.Lib.Game.Asset.SetupObject;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Engine
{
    /// <summary>
    /// General container to describe position of an prop as a single point.
    /// </summary>
    public class PropPointPosition : PointPosition
    {
        /// <summary>
        /// Gets or sets propdef.
        /// </summary>
        public PropDef Type { get; set; }

        /// <summary>
        /// Gets or sets prop id (which prop).
        /// </summary>
        public PropId Prop { get; set; }

        /// <summary>
        /// Gets or sets associated setup object.
        /// </summary>
        public ISetupObject? SetupObject { get; set; }

        /// <summary>
        /// Gets or sets model bounding box.
        /// </summary>
        public BoundingBoxd ModelBbox { get; set; } = new BoundingBoxd();
    }
}
