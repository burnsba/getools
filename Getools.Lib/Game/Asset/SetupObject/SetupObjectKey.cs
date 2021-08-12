using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectKey : SetupObjectGenericBase
    {
        public SetupObjectKey()
            : base(Propdef.Key)
        {
        }

        public uint Key { get; set; }
    }
}
