using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib;
using Getools.Lib.Game;
using Getools.Lib.Game.Enums;
using Getools.Lib.Game.Flags;
using static System.Net.Mime.MediaTypeNames;

namespace Gebug64.Unfloader.Protocol.Gebug.Dto
{
    /// <summary>
    /// Container to describe character position data.
    /// </summary>
    public class RmonGuardPosition
    {
        /// <summary>
        /// Size of the point struct in bytes.
        /// </summary>
        public const int SizeOf = 0x38;

        /// <summary>
        /// In-game chrnum id.
        /// </summary>
        public UInt16 Chrnum { get; set; }

        /// <summary>
        /// Index source from g_ChrSlots.
        /// </summary>
        public byte ChrSlotIndex { get; set; }

        /// <summary>
        /// Guard current action.
        /// </summary>
        public GuardActType ActionType { get; set; }

        /// <summary>
        /// Prop->pos property.
        /// </summary>
        public Coord3dd PropPos { get; set; } = Coord3dd.Zero.Clone();

        /// <summary>
        /// If <see cref="ActionType"/> is <see cref="GuardActType.ActGoPos"/>, this is guard->act_gopos->targetpos.
        /// If <see cref="ActionType"/> is <see cref="GuardActType.ActPatrol"/>, this is guard->act_patrol->waydata->pos.
        /// Otherwise this is zero.
        /// </summary>
        public Coord3dd TargetPos { get; set; } = Coord3dd.Zero.Clone();

        /// <summary>
        /// getsubroty(chr->model) result.
        /// </summary>
        public Single Subroty { get; set; } = 0;

        /// <summary>
        /// Canonical "damage", this is how much damage the chr has taken.
        /// </summary>
        public Single Damage { get; set; } = 0;

        /// <summary>
        /// Canonical "maxdamage", this is how much damage the chr can take before dying.
        /// </summary>
        public Single MaxDamage { get; set; } = 0;

        /// <summary>
        /// Canonical "shotbondsum", current intolerance.
        /// </summary>
        public Single Intolerance { get; set; } = 0;

        /// <summary>
        /// Remaining body armor.
        /// </summary>
        public double BodyArmorRemain => (Damage < 0) ? (double)(-1 * Damage) : 0;

        /// <summary>
        /// Remaining hit points. Ignores bordy armor.
        /// </summary>
        public double HpRemain
        {
            get
            {
                if (Damage < 0)
                {
                    // If there is body armor ignore that.
                    return (double)MaxDamage;
                }
                else if (Damage > 0 && Damage < MaxDamage)
                {
                    return (double)(MaxDamage - Damage);
                }

                return 0;
            }
        }

        /// <summary>
        /// Gets rotation of model in degrees.
        /// </summary>
        public double ModelRotationDegrees => WpfRotationTransform(Subroty * 180.0 / Math.PI);

        /// <summary>
        /// Untranslated animation (e.g., idle starts at 0x1c).
        /// </summary>
        public UInt32 Anim { get; set; }

        /// <summary>
        /// Bitmask of current character flags. <see cref="Getools.Lib.Game.Flags.ChrFlags"/>.
        /// </summary>
        public UInt32 ChrFlags { get; set; }

        /// <summary>
        /// Bitmask of current prop flags. <see cref="Getools.Lib.Game.Flags.PropFlag"/>.
        /// </summary>
        public byte PropFlags { get; set; }

        /// <summary>
        /// Bitmask of current character flags2.
        /// </summary>
        public byte ChrFlags2 { get; set; }

        /// <summary>
        /// Bitmask of current chr->hidden flags. <see cref="Getools.Lib.Game.Flags.ChrHidden"/>.
        /// </summary>
        public UInt16 ChrHidden { get; set; }

        /// <summary>
        /// Parses position data from <see cref="Gebug64.Unfloader.Protocol.Gebug.Message.GebugChrSendAllGuardInfoMessage"/>.
        /// </summary>
        /// <param name="fullBody">Byte array containing message data.</param>
        /// <param name="bodyOffset">Byte offset for start of <see cref="RmonGuardPosition"/>.</param>
        /// <returns>Parsed object.</returns>
        public static RmonGuardPosition TryParse(byte[] fullBody, int bodyOffset)
        {
            ushort chrnum = (ushort)BitUtility.Read16Big(fullBody, bodyOffset);
            bodyOffset += 2;

            byte chrSlotIndex = fullBody[bodyOffset++];

            GuardActType action = (GuardActType)fullBody[bodyOffset++];
            if (!Enum.IsDefined(typeof(GuardActType), action))
            {
                action = GuardActType.ActInvalidData;
            }

            double x = (double)BitUtility.CastToFloat((int)BitUtility.Read32Big(fullBody, bodyOffset));
            bodyOffset += 4;

            double y = (double)BitUtility.CastToFloat((int)BitUtility.Read32Big(fullBody, bodyOffset));
            bodyOffset += 4;

            double z = (double)BitUtility.CastToFloat((int)BitUtility.Read32Big(fullBody, bodyOffset));
            bodyOffset += 4;

            var propPos = new Getools.Lib.Game.Coord3dd(x, y, z);

            x = (double)BitUtility.CastToFloat((int)BitUtility.Read32Big(fullBody, bodyOffset));
            bodyOffset += 4;

            y = (double)BitUtility.CastToFloat((int)BitUtility.Read32Big(fullBody, bodyOffset));
            bodyOffset += 4;

            z = (double)BitUtility.CastToFloat((int)BitUtility.Read32Big(fullBody, bodyOffset));
            bodyOffset += 4;

            var targetPos = new Getools.Lib.Game.Coord3dd(x, y, z);

            Single rot = BitUtility.CastToFloat((int)BitUtility.Read32Big(fullBody, bodyOffset));
            bodyOffset += 4;

            Single damage = BitUtility.CastToFloat((int)BitUtility.Read32Big(fullBody, bodyOffset));
            bodyOffset += 4;

            Single maxdamage = BitUtility.CastToFloat((int)BitUtility.Read32Big(fullBody, bodyOffset));
            bodyOffset += 4;

            Single intolerance = BitUtility.CastToFloat((int)BitUtility.Read32Big(fullBody, bodyOffset));
            bodyOffset += 4;

            UInt32 anim = (UInt32)BitUtility.Read32Big(fullBody, bodyOffset);
            bodyOffset += 4;

            UInt32 chrflags = (UInt32)BitUtility.Read32Big(fullBody, bodyOffset);
            bodyOffset += 4;

            byte propFlags = fullBody[bodyOffset++];
            byte chrFlags2 = fullBody[bodyOffset++];

            UInt16 chrHidden = (UInt16)BitUtility.Read16Big(fullBody, bodyOffset);
            bodyOffset += 2;

            var guard = new RmonGuardPosition()
            {
                Chrnum = chrnum,
                ChrSlotIndex = chrSlotIndex,
                ActionType = action,
                PropPos = propPos,
                TargetPos = targetPos,
                Subroty = rot,
                Damage = damage,
                MaxDamage = maxdamage,
                Intolerance = intolerance,
                Anim = anim,
                ChrFlags = chrflags,
                PropFlags = propFlags,
                ChrFlags2 = chrFlags2,
                ChrHidden = chrHidden,
            };

            return guard;
        }

        private double WpfRotationTransform(double degrees)
        {
            return -1 * degrees;
        }
    }
}
