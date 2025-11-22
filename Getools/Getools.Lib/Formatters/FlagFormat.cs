using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Formatters
{
    /// <summary>
    /// Converts game flags into friendly strings.
    /// </summary>
    public class FlagFormat
    {
        /// <summary>
        /// Convert mask of flags into names.
        /// </summary>
        /// <param name="flags">Bitmask of <see cref="Game.Flags.ChrFlags"/>.</param>
        /// <returns>Friendly names of flags set.</returns>
        public static List<string> ResolveChrFlagFriendlyName(uint flags)
        {
            return ResolveFlagCommon(ChrFlagFriendlyName, flags);
        }

        /// <summary>
        /// Convert mask of flags into names.
        /// </summary>
        /// <param name="flags">Bitmask of <see cref="Game.Flags.ChrHidden"/>.</param>
        /// <returns>Friendly names of flags set.</returns>
        public static List<string> ResolveChrHiddenFriendlyName(uint flags)
        {
            return ResolveFlagCommon(ChrHiddenFriendlyName, flags);
        }

        /// <summary>
        /// Convert mask of flags into names.
        /// </summary>
        /// <param name="flags">Bitmask of <see cref="Game.Flags.PropFlag"/>.</param>
        /// <returns>Friendly names of flags set.</returns>
        public static List<string> ResolvePropFlagFriendlyName(uint flags)
        {
            return ResolveFlagCommon(PropFlagFriendlyName, flags);
        }

        /// <summary>
        /// Convert mask of flags into names.
        /// </summary>
        /// <param name="flags">Bitmask of <see cref="Game.Flags.ActionAttackAttackType"/>.</param>
        /// <returns>Friendly names of flags set.</returns>
        public static List<string> ResolveChrAttackTypeFriendlyName(uint flags)
        {
            return ResolveFlagCommon(ChrAttackTypeFriendlyName, flags);
        }

        /// <summary>
        /// Convert single flag into name.
        /// </summary>
        /// <param name="flag">Single <see cref="Game.Flags.ChrFlags"/>.</param>
        /// <returns>Friendly name of flag.</returns>
        public static string ChrFlagFriendlyName(uint flag)
        {
            switch (flag)
            {
                case 0: return "NONE";
                case 0x00000001: return "INIT";
                case 0x00000002: return "CLONE";
                case 0x00000004: return "NEAR MISS";
                case 0x00000008: return "HAS BEEN ON SCREEN";
                case 0x00000010: return "INVINCIBLE";
                case 0x00000020: return "FORCE NO BLOOD";
                case 0x00000040: return "CAN SHOOT CHRS";
                case 0x00000080: return "00000080";
                case 0x00000100: return "WAS DAMAGED";
                case 0x00000200: return "00000200";
                case 0x00000400: return "HIDDEN";
                case 0x00000800: return "NO AUTOAIM";
                case 0x00001000: return "LOCK Y POS";
                case 0x00002000: return "NO SHADOW";
                case 0x00004000: return "IGNORE ANIM TRANSLATION";
                case 0x00008000: return "IMPACT ALWAYS";
                case 0x00010000: return "00010000";
                case 0x00020000: return "00020000";
                case 0x00040000: return "00040000";
                case 0x00080000: return "INCREASE RUNNING SPEED";
                case 0x00100000: return "COUNT DEATH AS CIVILIAN";
                case 0x00200000: return "WAS HIT";
                case 0x00400000: return "00400000";
                case 0x00800000: return "CULL USING HITBOX";
                case 0x01000000: return "01000000";
                case 0x02000000: return "02000000";
                case 0x04000000: return "04000000";
                case 0x08000000: return "08000000";
                case 0x10000000: return "10000000";
                case 0x20000000: return "20000000";
                case 0x40000000: return "40000000";
                case 0x80000000: return "80000000";

                default:
                    return "UNKNOWN";
            }
        }

        /// <summary>
        /// Convert single flag into name.
        /// </summary>
        /// <param name="flag">Single <see cref="Game.Flags.ChrHidden"/>.</param>
        /// <returns>Friendly name of flag.</returns>
        public static string ChrHiddenFriendlyName(uint flag)
        {
            switch (flag)
            {
                case 0: return "NONE";
                case 0x0001: return "DROP HELD ITEMS";
                case 0x0002: return "ALERT GUARD RELATED";
                case 0x0004: return "FIRE WEAPON LEFT";
                case 0x0008: return "FIRE WEAPON RIGHT";
                case 0x0010: return "OFFSCREEN PATROL";
                case 0x0020: return "REMOVE";
                case 0x0040: return "TIMER ACTIVE";
                case 0x0080: return "FIRE TRACER";
                case 0x0100: return "MOVING";
                case 0x0200: return "BACKGROUND AI";
                case 0x0400: return "0400";
                case 0x0800: return "FREEZE";
                case 0x1000: return "RAND FLINCH 1";
                case 0x2000: return "RAND FLINCH 2";
                case 0x4000: return "RAND FLINCH 4";
                case 0x8000: return "RAND FLINCH 8";

                default:
                    return "UNKNOWN";
            }
        }

        /// <summary>
        /// Convert single flag into name.
        /// </summary>
        /// <param name="flag">Single <see cref="Game.Flags.PropFlag"/>.</param>
        /// <returns>Friendly name of flag.</returns>
        public static string PropFlagFriendlyName(uint flag)
        {
            switch (flag)
            {
                case 0: return "NONE";
                case 0x00000001: return "RENDER POST BG";
                case 0x00000002: return "ON SCREEN";
                case 0x00000004: return "ENABLED";
                case 0x00000008: return "IN AIR";
                case 0x00000010: return "Scale to Pad Bounds";
                case 0x00000020: return "Scale X to Pad Bounds";
                case 0x00000040: return "Scale Y to Pad Bounds";
                case 0x00000080: return "Scale Z to Pad Bounds";
                case 0x00000100: return "FORCE COLLISIONS";
                case 0x00000200: return "GLASS ENV MAPPING";
                case 0x00000400: return "ILLUMINATED";
                case 0x00000800: return "FREE STANDING GLASS";
                case 0x00001000: return "ABSOLUTE POSITION";
                case 0x00002000: return "AI UNDROPPABLE";
                case 0x00004000: return "ASSIGNED TO CHR";
                case 0x00008000: return "INSIDE ANOTHER OBJ";
                case 0x00010000: return "FORCE MORTAL";
                case 0x00020000: return "INVINCIBLE";
                case 0x00040000: return "ALLOW PICKUP";
                case 0x00080000: return "COLLECT INTERACT ONLY";
                case 0x00100000: return "UNCOLLECTABLE";
                case 0x00200000: return "BOUNCE AND DESTROY";
                case 0x00400000: return "00400000";
                case 0x00800000: return "00800000";
                case 0x01000000: return "EMBEDDED OBJECT";
                case 0x02000000: return "CANNOT ACTIVATE";
                case 0x04000000: return "AI SEES THROUGH";
                case 0x08000000: return "DOOR TWOWAY";

                case 0x10000000: return "WEAPON LEFTHANDED";
                ////case 0x10000000: return "GLASS HASPORTAL";
                ////case 0x10000000: return "CULL BEHIND DOOR";
                ////case 0x10000000: return "FIXED MONITOR";
                ////case 0x10000000: return "CCTV DISABLED";
                ////case 0x10000000: return "IS DRONE GUN";

                case 0x20000000: return "DOOR OPENTOFRONT";
                ////case 0x20000000: return "SPECIAL FUNC";
                ////case 0x20000000: return "CONCEAL GUN";

                case 0x40000000: return "NO PORTAL CLOSE";
                ////case 0x40000000: return "NO AMMO";

                case 0x80000000: return "80000000";
                ////case 0x80000000: return "IS DOUBLE";

                default:
                    return "UNKNOWN";
            }
        }

        /// <summary>
        /// Convert single flag into name.
        /// </summary>
        /// <param name="flag">Single <see cref="Game.Flags.ActionAttackAttackType"/>.</param>
        /// <returns>Friendly name of flag.</returns>
        public static string ChrAttackTypeFriendlyName(uint flag)
        {
            //// compass might need specially handling? But I don't think it's used?

            switch (flag)
            {
                case 0: return "NONE";
                case 0x0001: return "TARGET BOND";
                case 0x0002: return "TARGET FRONT OF CHR";
                case 0x0004: return "TARGET CHR";
                case 0x0008: return "TARGET PAD";
                case 0x0010: return "TARGET COMPASS";
                case 0x0020: return "TARGET AIM ONLY";
                case 0x0040: return "TARGET DONTTURN";

                default:
                    return "UNKNOWN";
            }
        }

        private static List<string> ResolveFlagCommon(Func<uint, string> func, uint flags)
        {
            uint i = 1;
            int count;
            List<string> results = new List<string>();

            for (count = 0; count < 32; count++)
            {
                uint maybe = flags & i;
                if (maybe != 0)
                {
                    results.Add(func(maybe));
                }

                i <<= 1;
            }

            if (!results.Any())
            {
                results.Add(func(0));
            }

            return results;
        }
    }
}
