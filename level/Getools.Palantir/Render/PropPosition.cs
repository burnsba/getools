using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;
using Getools.Lib.Game.Asset.SetupObject;
using Getools.Lib.Game.Enums;

namespace Getools.Palantir.Render
{
    public class PropPosition : RenderPosition
    {
        public PropDef Type { get; set; }

        public PropId Prop { get; set; }

        public ISetupObject? SetupObject { get; set; }

        public BoundingBoxd ModelBbox { get; set; } = new BoundingBoxd();
    }
}
