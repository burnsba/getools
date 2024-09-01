using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Flags
{
    /// <summary>
    /// Game chr flags constants.
    /// </summary>
    public static class ChrFlags
    {
        /// <summary>
        /// Initialize chr.
        /// </summary>
        public const uint CHRFLAG_INIT = 0x00000001;

        /// <summary>
        /// Clone on heard gunfire (used by GAILIST_STANDARD_GUARD).
        /// </summary>
        public const uint CHRFLAG_CLONE = 0x00000002;

        /// <summary>
        /// Chr was just nearly shot (sometimes set on direct hit) - resets every tick.
        /// </summary>
        public const uint CHRFLAG_NEAR_MISS = 0x00000004;

        /// <summary>
        /// Chr has been on screen before.
        /// </summary>
        public const uint CHRFLAG_HAS_BEEN_ON_SCREEN = 0x00000008;

        /// <summary>
        /// Invincible.
        /// </summary>
        public const uint CHRFLAG_INVINCIBLE = 0x00000010;

        /// <summary>
        /// Canonical name.
        /// </summary>
        public const uint CHRSTART_FORCE_NO_BLOOD = 0x00000020;

        /// <summary>
        /// Can shoot other guards.
        /// </summary>
        public const uint CHRFLAG_CAN_SHOOT_CHRS = 0x00000040;

        /// <summary>
        /// Unknown.
        /// </summary>
        public const uint CHRFLAG_00000080 = 0x00000080;

        /// <summary>
        /// Chr has taken damage (not invincible).
        /// </summary>
        public const uint CHRFLAG_WAS_DAMAGED = 0x00000100;

        /// <summary>
        /// Possibly is BG AI.
        /// </summary>
        public const uint CHRFLAG_00000200 = 0x00000200;

        /// <summary>
        /// Hidden.
        /// </summary>
        public const uint CHRFLAG_HIDDEN = 0x00000400;

        /// <summary>
        /// No autoaim.
        /// </summary>
        public const uint CHRFLAG_NO_AUTOAIM = 0x00000800;

        /// <summary>
        /// Lock y position (no gravity, used for dam/cradle jump).
        /// </summary>
        public const uint CHRFLAG_LOCK_Y_POS = 0x00001000;

        /// <summary>
        /// No shadow.
        /// </summary>
        public const uint CHRFLAG_NO_SHADOW = 0x00002000;

        /// <summary>
        /// Ignore animation translation.
        /// </summary>
        public const uint CHRFLAG_IGNORE_ANIM_TRANSLATION = 0x00004000;

        /// <summary>
        /// Trev on cradle sets this flag so he can be shot off the platform.
        /// </summary>
        public const uint CHRFLAG_IMPACT_ALWAYS = 0x00008000;

        /// <summary>
        /// Unknown.
        /// </summary>
        public const uint CHRFLAG_00010000 = 0x00010000;

        /// <summary>
        /// Unknown.
        /// </summary>
        public const uint CHRFLAG_00020000 = 0x00020000;

        /// <summary>
        /// Unknown.
        /// </summary>
        public const uint CHRFLAG_00040000 = 0x00040000;

        /// <summary>
        /// Increase sprinting speed (used by trevelyan).
        /// </summary>
        public const uint CHRFLAG_INCREASE_RUNNING_SPEED = 0x00080000;

        /// <summary>
        /// Count death as civilian killed.
        /// </summary>
        public const uint CHRFLAG_COUNT_DEATH_AS_CIVILIAN = 0x00100000;

        /// <summary>
        /// Chr has been hit (even if invincible).
        /// </summary>
        public const uint CHRFLAG_WAS_HIT = 0x00200000;

        /// <summary>
        /// Unknown.
        /// </summary>
        public const uint CHRFLAG_00400000 = 0x00400000;

        /// <summary>
        /// Cull chr using hitbox instead of tile/clipping (useful with lock y pos flag).
        /// </summary>
        public const uint CHRFLAG_CULL_USING_HITBOX = 0x00800000;

        /// <summary>
        /// Unknown.
        /// </summary>
        public const uint CHRFLAG_01000000 = 0x01000000;

        /// <summary>
        /// Unknown.
        /// </summary>
        public const uint CHRFLAG_02000000 = 0x02000000;

        /// <summary>
        /// unknown NoFade?
        /// </summary>
        public const uint CHRFLAG_04000000 = 0x04000000;

        /// <summary>
        /// Unknown.
        /// </summary>
        public const uint CHRFLAG_08000000 = 0x08000000;

        /// <summary>
        /// Unknown.
        /// </summary>
        public const uint CHRFLAG_10000000 = 0x10000000;

        /// <summary>
        /// Unknown.
        /// </summary>
        public const uint CHRFLAG_20000000 = 0x20000000;

        /// <summary>
        /// Unknown.
        /// </summary>
        public const uint CHRFLAG_40000000 = 0x40000000;

        /// <summary>
        /// Unknown.
        /// </summary>
        public const uint CHRFLAG_80000000 = 0x80000000;
    }
}
