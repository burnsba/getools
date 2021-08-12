using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectObjectiveFailCondition : SetupObjectBase, ISetupObject
    {
        public SetupObjectObjectiveFailCondition()
            : base(Propdef.ObjectiveFailCondition)
        {
        }

        public int TestVal { get; set; }
    }
}
