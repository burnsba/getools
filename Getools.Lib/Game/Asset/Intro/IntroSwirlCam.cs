using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    public class IntroSwirlCam : IntroBase
    {
        public IntroSwirlCam()
            : base(IntroType.SwirlCam)
        {
        }

        public UInt32 Unknown_00 { get; set; }
        public UInt32 X { get; set; }
        public UInt32 Y { get; set; }
        public UInt32 Z { get; set; }
        public UInt32 Left { get; set; }
        public UInt32 Right { get; set; }
        public UInt32 Unknown_18 { get; set; }
    }
}
