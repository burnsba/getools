using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectDrone : SetupObjectGenericBase
    {
        public SetupObjectDrone()
            : base(Propdef.Drone)
        {
        }

        public byte[] Data { get; set; } = new byte[104];
    }
}
