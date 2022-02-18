using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list marker for end of objective.
    /// </summary>
    public class SetupObjectObjectiveEnd : SetupObjectBase, ISetupObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectObjectiveEnd"/> class.
        /// </summary>
        public SetupObjectObjectiveEnd()
            : base(PropDef.ObjectiveEnd)
        {
        }
    }
}
