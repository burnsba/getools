using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Enums
{
    /// <summary>
    /// Door state.
    /// Cooresponds to type `enum DOORSTATE`.
    /// </summary>
    public enum DoorState
    {
        /// <summary>
        /// Stationary.
        /// </summary>
        Stationary = 0,

        /// <summary>
        /// Opening.
        /// </summary>
        Opening = 1,

        /// <summary>
        /// Closing.
        /// </summary>
        Closing = 2,

        /// <summary>
        /// Waiting.
        /// </summary>
        Waiting = 3,
    }
}
