using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Flags
{
    /// <summary>
    /// Game chr->hidden constants.
    /// </summary>
    public static class ChrHidden
    {
        /// <summary>
        /// No flags.
        /// </summary>
        public const uint CHRHIDDEN_NONE = 0;

        /// <summary>
        /// Drop held items/weapons.
        /// </summary>
        public const uint CHRHIDDEN_DROP_HELD_ITEMS = 0x0001;

        /// <summary>
        /// Alert Guard.
        /// </summary>
        public const uint CHRHIDDEN_ALERT_GUARD_RELATED = 0x0002;

        /// <summary>
        /// Firing left weapon.
        /// </summary>
        public const uint CHRHIDDEN_FIRE_WEAPON_LEFT = 0x0004;

        /// <summary>
        /// Firing right weapon.
        /// </summary>
        public const uint CHRHIDDEN_FIRE_WEAPON_RIGHT = 0x0008;

        /// <summary>
        /// The only time 0x10 flag is kept is when the enemy is standing.
        /// Moving, patrolling or going to pad always clears 0x10.
        /// A guard can keep the 0x10 flag when moving, but only if it's been
        /// off screen and using the cheap movement logic where it doesn't
        /// calculate collision.It'll keep the 0x10 flag until the guard
        /// appears on screen..
        /// </summary>
        public const uint CHRHIDDEN_OFFSCREEN_PATROL = 0x0010;

        /// <summary>
        /// Remove character.
        /// </summary>
        public const uint CHRHIDDEN_REMOVE = 0x0020;

        /// <summary>
        /// Chr timer is active.
        /// </summary>
        public const uint CHRHIDDEN_TIMER_ACTIVE = 0x0040;

        /// <summary>
        /// Spawn a tracer.
        /// </summary>
        public const uint CHRHIDDEN_FIRE_TRACER = 0x0080;

        /// <summary>
        /// Moving.
        /// </summary>
        public const uint CHRHIDDEN_MOVING = 0x0100;

        /// <summary>
        /// Set when AI script is running.
        /// </summary>
        public const uint CHRHIDDEN_BACKGROUND_AI = 0x0200;

        /// <summary>
        /// Unknown.
        /// </summary>
        public const uint CHRHIDDEN_0400 = 0x0400;

        /// <summary>
        /// Freeze current animation state.
        /// </summary>
        public const uint CHRHIDDEN_FREEZE = 0x0800;

        /// <summary>
        /// Randomly set when shot.
        /// </summary>
        public const uint CHRHIDDEN_RAND_FLINCH_1 = 0x1000;

        /// <summary>
        /// Randomly set when shot.
        /// </summary>
        public const uint CHRHIDDEN_RAND_FLINCH_2 = 0x2000;

        /// <summary>
        /// Randomly set when shot.
        /// </summary>
        public const uint CHRHIDDEN_RAND_FLINCH_4 = 0x4000;

        /// <summary>
        /// Randomly set when shot.
        /// </summary>
        public const uint CHRHIDDEN_RAND_FLINCH_8 = 0x8000;
    }
}
