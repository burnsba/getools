using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Enums
{
    /// <summary>
    /// Native enum GAMEMODE.
    /// </summary>
    public enum GameMode
    {
        /// <summary>
        /// Intro flag.
        /// </summary>
        Intro = -1,

        /// <summary>
        /// Solo game mode.
        /// </summary>
        Solo = 0,

        /// <summary>
        /// Multiplayer.
        /// </summary>
        Multi,

        /// <summary>
        /// Cheats.
        /// </summary>
        Cheats,
    }
}
