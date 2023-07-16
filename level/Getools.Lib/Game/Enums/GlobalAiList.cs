using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Enums
{
    public enum GlobalAiList
    {
        /// <summary>
        /// Try aiming at bond, otherwise do nothing.
        /// </summary>
        GAILIST_AIM_AT_BOND = 0,

        /// <summary>
        /// Dead or Removed AI.
        /// Use when AI has no more to do (or use YIELD_FOREVER)
        /// </summary>
        GAILIST_DEAD_AI,

        /// <summary>
        /// Stand Guard and Kill Time or patrol (Not typicaly used for patrolling).
        /// While killing time, play Idle animations
        /// On detecting Bond, Send a clone OR Run to Bond and Attack.
        /// This AI List is used by nearly all guards either as default or as a result
        /// of detecting Bond or finnishing their assigned behaivior.
        /// </summary>
        GAILIST_STANDARD_GUARD,

        /// <summary>
        /// Play one random idle animation
        /// </summary>
        GAILIST_PLAY_IDLE_ANIMATION,

        /// <summary>
        /// Bash that Keyboard once with a random animation
        /// </summary>
        GAILIST_BASH_KEYBOARD,

        /// <summary>
        /// Stand Guard Statically (No Clones, No animations) or patrol.
        /// On detecting Bond (sight/near-miss only), Act like a Standard Guard.
        /// </summary>
        GAILIST_SIMPLE_GUARD_DEAF,

        /// <summary>
        /// Attack Bond once via 1 random animation
        /// </summary>
        GAILIST_ATTACK_BOND,

        /// <summary>
        /// Stand Guard Statically (No Clones, No animations) or patrol (Typical use of this type).
        /// On detecting Bond, Act like a Standard Guard.
        /// </summary>
        GAILIST_SIMPLE_GUARD,

        /// <summary>
        /// Run to bond and fire if seen, otherwise wait.
        /// </summary>
        GAILIST_RUN_TO_BOND,

        /// <summary>
        /// Stand Guard Statically (No Clones, No animations) or patrol.
        /// On detecting Bond, Run to padpreset1 and activate alarm.
        /// Act like a Standard Guard thereafter.
        /// </summary>
        GAILIST_SIMPLE_GUARD_ALARM_RAISER,

        /// <summary>
        /// Startle character then Run To Bond
        /// </summary>
        GAILIST_STARTLE_AND_RUN_TO_BOND,

        /// <summary>
        /// If Calling Chr NOT been seen, Send Clone after Bond, otherwise Act like a
        /// Standard Guard
        /// </summary>
        GAILIST_TRY_CLONE_SEND_OR_RUN_TO_BOND,

        /// <summary>
        /// Run to bond then act like a Standard Guard
        /// </summary>
        GAILIST_STANDARD_CLONE,

        /// <summary>
        /// Persistently chase Bond and Attack (halt randomly)
        /// </summary>
        GAILIST_PERSISTENTLY_CHASE_AND_ATTACK_BOND,

        /// <summary>
        ///  Wait for one second then return
        /// </summary>
        GAILIST_WAIT_ONE_SECOND,

        /// <summary>
        /// Exit level and set BG AI to nothing
        /// </summary>
        GAILIST_END_LEVEL,

        /// <summary>
        /// Draw TT33, Aim and fire.
        /// Act like a Standard Guard thereafter
        /// </summary>
        GAILIST_DRAW_TT33_AND_ATTCK_BOND,

        /// <summary>
        /// Remove Calling chr and set AI to nothing
        /// </summary>
        GAILIST_REMOVE_CHR,
    }
}
