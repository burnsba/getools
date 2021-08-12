using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    public class IntroCam : IntroBase
    {
        public IntroCam()
            : base(IntroType.IntroCam)
        {
        }

        public UInt32 Animation { get; set; }
    }
}
