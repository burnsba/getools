using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    /// <summary>
    /// Marker for end of intro section in setup.
    /// </summary>
    public class IntroEndIntro : IntroBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntroEndIntro"/> class.
        /// </summary>
        public IntroEndIntro()
            : base(IntroType.EndIntro)
        {
        }
    }
}
