using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    public interface IIntro : IGameObjectHeader
    {
        public IntroType Type { get; set; }
    }
}
