using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectVehicle : SetupObjectGenericBase
    {
        public SetupObjectVehicle()
            : base(Propdef.Vehicle)
        {
        }

        public byte[] Data { get; set; } = new byte[136];
    }
}
