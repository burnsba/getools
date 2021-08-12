using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    public class IntroSpawn : IntroBase
    {
        public IntroSpawn()
            : base(IntroType.Spawn)
        {
        }

        public UInt32 Unknown_00 { get; set; }
        public UInt32 Unknown_04 { get; set; }
    }
}
