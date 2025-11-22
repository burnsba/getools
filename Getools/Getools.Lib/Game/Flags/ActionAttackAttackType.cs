using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Flags
{
    /// <summary>
    /// Character attack action, attack type.
    /// </summary>
    public static class ActionAttackAttackType
    {
        /// <summary>
        /// No flags.
        /// </summary>
        public const uint TARGET_NONE = 0;

        /// <summary>
        /// Set target to bond (ignores target argument)
        /// </summary>
        public const uint TARGET_BOND = 0x0001;

        /// <summary>
        /// Set target to front of chr
        /// </summary>
        public const uint TARGET_FRONT_OF_CHR = 0x0002;

        /// <summary>
        /// Set target type to chr_num
        /// </summary>
        public const uint TARGET_CHR = 0x0004;

        /// <summary>
        /// Set target type to pad
        /// </summary>
        public const uint TARGET_PAD = 0x0008;

        /// <summary>
        /// Set target to compass direction (hex) N: 0000 E: C000 S: 8000: W: 4000
        /// (not used?)
        /// </summary>
        public const uint TARGET_COMPASS = 0x0010;

        /// <summary>
        /// Aim at target instead of firing
        /// </summary>
        public const uint TARGET_AIM_ONLY = 0x0020;

        /// <summary>
        /// Limits target to 180 degrees in front of guard (cannot be used with TARGET_BOND flag)
        /// </summary>
        public const uint TARGET_DONTTURN = 0x0040;
    }
}
