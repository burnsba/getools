using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectCollectObject : SetupObjectBase, ISetupObject
    {
        public SetupObjectCollectObject()
            : base(Propdef.CollectObject)
        {
        }

        public uint ObjectId { get; set; }
    }
}
