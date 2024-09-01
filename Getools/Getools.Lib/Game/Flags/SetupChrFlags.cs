using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Flags
{
    /// <summary>
    /// Character flags, as used in the setup file (not runtime).
    /// </summary>
    public static class SetupChrFlags
    {
        /// <summary>
        /// Chr has sunglasses.
        /// </summary>
        public const int GUARD_SETUP_FLAG_SUNGLASSES = 0x1;

        /// <summary>
        /// 50% chance of sunglasses
        /// </summary>
        public const int GUARD_SETUP_FLAG_SUNGLASSES_50 = 0x2;

        /// <summary>
        /// Chr will clone.
        /// </summary>
        public const int GUARD_SETUP_FLAG_CHR_CLONE = 0x4;

        /// <summary>
        /// Chr is invincible.
        /// </summary>
        public const int GUARD_SETUP_FLAG_CHR_INVINCIBLE = 0x8;
    }
}
