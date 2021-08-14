using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Marker to indicate end of setup object list / prop definition section.
    /// </summary>
    public class SetupObjectEndProps : SetupObjectBase, ISetupObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectEndProps"/> class.
        /// </summary>
        public SetupObjectEndProps()
            : base(PropDef.EndProps)
        {
        }
    }
}
