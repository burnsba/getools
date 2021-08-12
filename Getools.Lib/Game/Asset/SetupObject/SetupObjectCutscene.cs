using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectCutscene : SetupObjectBase, ISetupObject
    {
        public SetupObjectCutscene()
            : base(Propdef.Cutscene)
        {
        }

        public int XCoord { get; set; }
        public int YCoord { get; set; }
        public int ZCoord { get; set; }
        public int LatRot { get; set; }
        public int VertRot { get; set; }
        public uint IllumPreset { get; set; }
    }
}
