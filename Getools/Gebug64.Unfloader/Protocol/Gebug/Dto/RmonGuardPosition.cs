using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;
using Getools.Lib.Game.Enums;
using static System.Net.Mime.MediaTypeNames;

namespace Gebug64.Unfloader.Protocol.Gebug.Dto
{
    /// <summary>
    /// Container to describe character position data.
    /// </summary>
    public class RmonGuardPosition
    {
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
        public double ModelRotationDegrees => Subroty * 180.0 / Math.PI;
    }
}
