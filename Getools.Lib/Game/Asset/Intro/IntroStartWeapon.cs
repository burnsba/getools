using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    public class IntroStartWeapon : IntroBase
    {
        public IntroStartWeapon()
            : base(IntroType.StartWeapon)
        {
        }

        public UInt32 Left { get; set; }
        public UInt32 Right { get; set; }
        public UInt32 SetNum { get; set; }
    }
}
