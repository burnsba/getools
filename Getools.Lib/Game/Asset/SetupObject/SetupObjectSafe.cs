using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for safe.
    /// </summary>
    public class SetupObjectSafe : SetupObjectGenericBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectSafe"/> class.
        /// </summary>
        public SetupObjectSafe()
            : base(PropDef.Safe)
        {
        }
    }
}
