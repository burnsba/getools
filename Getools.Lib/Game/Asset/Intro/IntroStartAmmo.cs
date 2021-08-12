using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    public class IntroStartAmmo : IntroBase
    {
        public IntroStartAmmo()
            : base(IntroType.StartAmmo)
        {
        }

        public UInt32 AmmoType { get; set; }
        public UInt32 Quantity { get; set; }
        public UInt32 Set { get; set; }
    }
}
