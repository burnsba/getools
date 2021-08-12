using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    public class IntroWatchTime : IntroBase
    {
        public IntroWatchTime()
            : base(IntroType.WatchTime)
        {
        }

        public UInt32 Hour { get; set; }
        public UInt32 Minute { get; set; }
    }
}
