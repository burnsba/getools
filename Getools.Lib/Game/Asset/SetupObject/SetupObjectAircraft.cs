using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectAircraft : SetupObjectGenericBase
    {
        public SetupObjectAircraft()
            : base(Propdef.Aircraft)
        {
        }

        public byte[] Data { get; set; } = new byte[52];
    }
}
