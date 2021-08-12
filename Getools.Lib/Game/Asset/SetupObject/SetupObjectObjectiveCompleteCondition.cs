using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectObjectiveCompleteCondition : SetupObjectBase, ISetupObject
    {
        public SetupObjectObjectiveCompleteCondition()
            : base(Propdef.ObjectiveCompleteCondition)
        {
        }

        public int TestVal { get; set; }
    }
}
