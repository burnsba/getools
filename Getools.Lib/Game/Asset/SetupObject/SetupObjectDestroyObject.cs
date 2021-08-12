using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectDestroyObject : SetupObjectBase, ISetupObject
    {
        public SetupObjectDestroyObject()
            : base(Propdef.DestroyObject)
        {
        }

        public uint ObjectId { get; set; }
    }
}
