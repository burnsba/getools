using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectObjectiveStart : SetupObjectBase, ISetupObject
    {
        public SetupObjectObjectiveStart()
            : base(Propdef.ObjectiveStart)
        {
        }

        public int ObjectiveNumber { get; set; }
        public int TextId { get; set; }
        public int MinDifficulty { get; set; }
    }
}
