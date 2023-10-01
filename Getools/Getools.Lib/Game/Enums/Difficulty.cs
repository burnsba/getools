using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Enums
{
    /// <summary>
    /// Native enum DIFFICULTY.
    /// </summary>
    public enum Difficulty
    {
        /// <summary>
        /// Multiplayer flag.
        /// </summary>
        Multi = -1,

        /// <summary>
        /// Agent.
        /// </summary>
        Agent = 0,

        /// <summary>
        /// Secret agent.
        /// </summary>
        SecretAgent,

        /// <summary>
        /// 00 Agent.
        /// </summary>
        Difficulty00,

        /// <summary>
        /// 007.
        /// </summary>
        Difficulty007,

        /// <summary>
        /// Count/max value.
        /// </summary>
        Max,
    }
}
