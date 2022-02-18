using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for guard hat.
    /// </summary>
    public class SetupObjectHangingMonitor : SetupObjectGenericBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectHangingMonitor"/> class.
        /// </summary>
        public SetupObjectHangingMonitor()
            : base(PropDef.HangingMonitor)
        {
        }
    }
}
