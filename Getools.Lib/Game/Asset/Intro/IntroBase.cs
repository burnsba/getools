using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    public abstract class IntroBase : GameObjectHeaderBase, IIntro
    {
        public IntroBase()
        {
        }

        public IntroBase(IntroType type)
        {
            Type = type;
        }

        public IntroType Type
        {
            get
            {
                return (IntroType)TypeRaw;
            }

            set
            {
                TypeRaw = (byte)value;
            }
        }
    }
}
