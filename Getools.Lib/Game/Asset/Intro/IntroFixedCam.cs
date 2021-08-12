using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    public class IntroFixedCam : IntroBase
    {
        public IntroFixedCam()
            : base(IntroType.FixedCam)
        {
        }

        public UInt32 X { get; set; }
        public UInt32 Y { get; set; }
        public UInt32 Z { get; set; }
        public UInt32 Unknown_0c { get; set; }
        public UInt32 Unknown_10 { get; set; }
        public UInt32 Unknown_14 { get; set; }
        public UInt32 Unknown_18 { get; set; }
        public UInt32 Unknown_1c { get; set; }
        public UInt32 Unknown_20 { get; set; }
    }
}
