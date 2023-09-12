using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Flags
{
    public static class SetupChrFlags
    {
        public const int GUARD_SETUP_FLAG_SUNGLASSES = 0x1;

        /// <summary>
        /// 50% chance of sunglasses
        /// </summary>
        public const int GUARD_SETUP_FLAG_SUNGLASSES_50 = 0x2;

        public const int GUARD_SETUP_FLAG_CHR_CLONE = 0x4;

        public const int GUARD_SETUP_FLAG_CHR_INVINCIBLE = 0x8;
    }
}
