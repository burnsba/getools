using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Enums
{
    /// <summary>
    /// Door flags.
    /// </summary>
    public enum DoorFlags
    {
        /// <summary>
        /// Clears the door away as it slides, stopping visual problems when opening into a wall.
        /// Applies to:
        /// Slider (left/right) default,
        /// Shutter (up/down) default,
        /// special (swinging) default,
        /// special (eye) default,
        /// special (iris) default.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Slider (left/right) unknown,
        /// Shutter (up/down) unknown,
        /// </summary>
        Unknown = 4,
    }
}
