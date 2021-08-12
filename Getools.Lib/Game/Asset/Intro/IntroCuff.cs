using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    public class IntroCuff : IntroBase
    {
        public IntroCuff()
            : base(IntroType.Cuff)
        {
        }

        public UInt32 Cuff { get; set; }
    }
}
