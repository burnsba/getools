using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for glass.
    /// </summary>
    public class SetupObjectGlass : SetupObjectGenericBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectGlass"/> class.
        /// </summary>
        public SetupObjectGlass()
            : base(PropDef.Glass)
        {
        }
    }
}
