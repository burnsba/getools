using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectWeapon : SetupObjectGenericBase
    {
        public SetupObjectWeapon()
            : base(Propdef.Weapon)
        {
        }

        public byte GunPickup { get; set; }
        public byte LinkedItem { get; set; }
        public UInt16 Timer { get; set; }
        public UInt32 PointerLinkedItem { get; set; }
    }
}
