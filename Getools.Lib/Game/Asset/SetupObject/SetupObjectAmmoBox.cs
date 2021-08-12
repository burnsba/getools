using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectAmmoBox : SetupObjectGenericBase
    {
        public SetupObjectAmmoBox()
            : base(Propdef.AmmoBox)
        {
        }

        public int Ammo9mm { get; set; }
        public int Ammo9mm2 { get; set; }
        public int AmmoRifle { get; set; }
        public int AmmoShotgun { get; set; }
        public int AmmoHgrenade { get; set; }
        public int AmmoRockets { get; set; }
        public int AmmoRemote { get; set; }
        public int AmmoProx { get; set; }
        public int AmmoTimed { get; set; }
        public int AmmoThrowing { get; set; }
        public int AmmoGlaunch { get; set; }
        public int AmmoMagnum { get; set; }
        public int AmmoGolden { get; set; }
    }
}
