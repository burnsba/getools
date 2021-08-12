using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectObjectiveEnd : SetupObjectBase, ISetupObject
    {
        public SetupObjectObjectiveEnd()
            : base(Propdef.EndObjective)
        {
        }
    }
}
