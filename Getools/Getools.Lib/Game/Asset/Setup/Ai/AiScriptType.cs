using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    public enum AiScriptType
    {
        DefaultUnknown,

        /// <summary>
        /// Global AI List info.
        /// </summary>
        /// <remarks>
        /// Global AI lists can be assigned as the initial AI list for characters or
        /// assigned manually by other AI lists. They are not started automatically.
        /// Global AI lists can be referenced using constants such as GAILIST_AIM_AT_BOND
        /// </remarks>
        Global,

        /// <summary>
        /// Entity AI List info.
        /// </summary>
        /// <remarks>
        /// These are similar to global AI lists, but they are specific to individual
        /// stages. They are defined in each stage's setup file and use constants such
        /// as AILIST_SCIENTIST (no G prefix). They do not run automatically; they must
        /// be assigned as the initial function for an Entity or invoked by another
        /// function.
        /// Multiple Entities can use the same AI List - each Entity is treated as an
        /// independent thread with their own instance of unique data.
        /// </remarks>
        Entity,

        /// <summary>
        /// Background AI List info.
        /// </summary>
        /// <remarks>
        /// AI lists in this range will be started automatically when the stage is
        /// started using gameplay (but not run when using the cinema menu). They are
        /// commonly used for overarching stage logic that isn't specific to a character,
        /// such as monitoring objectives, or waiting for the player to enter a room then
        /// triggering a radio message.
        /// BG AI Lists cannot run Entity commands due to the lack of level presence.
        /// Attempting to do so will crash the game.
        /// </remarks>
        Background,
    }
}
