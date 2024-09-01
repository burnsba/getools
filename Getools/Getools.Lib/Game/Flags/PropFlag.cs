using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Flags
{
    /// <summary>
    /// Game prop flag constants.
    /// </summary>
    public class PropFlag
    {
        /// <summary>
        /// Fall to Ground.
        /// </summary>
        public const uint PROPFLAG_RENDERPOSTBG = 0x00000001;

        /// <summary>
        /// In Air Rotated 90 Deg Upside-Down.
        /// </summary>
        public const uint PROPFLAG_ONSCREEN = 0x00000002;

        /// <summary>
        /// In Air Upside-Down.
        /// </summary>
        public const uint PROPFLAG_ENABLED = 0x00000004;

        /// <summary>
        /// In Air.
        /// </summary>
        public const uint PROPFLAG_00000008 = 0x00000008;

        /// <summary>
        /// Scale to Pad Bounds.
        /// </summary>
        public const uint PROPFLAG_00000010 = 0x00000010;

        /// <summary>
        /// Scale X to Pad Bounds.
        /// </summary>
        public const uint PropFlag1_XToPresetBounds = 0x0020;

        /// <summary>
        /// Scale Y to Pad Bounds.
        /// </summary>
        public const uint PropFlag1_YToPresetBounds = 0x0040;

        /// <summary>
        /// Scale Z to Pad Bounds.
        /// </summary>
        public const uint PropFlag1_ZToPresetBounds = 0x0080;

        /// <summary>
        /// Force Collisions.
        /// </summary>
        public const uint PROPFLAG_00000100 = 0x00000100;

        /// <summary>
        /// Glass Env Mapping Style.
        /// </summary>
        public const uint PROPFLAG_00000200 = 0x00000200;

        /// <summary>
        /// Ignore Stan Colour.
        /// </summary>
        public const uint PROPFLAG_ILLUMINATED = 0x00000400;

        /// <summary>
        /// Free Standing Glass.
        /// </summary>
        public const uint PROPFLAG_00000800 = 0x00000800;

        /// <summary>
        /// Absolute Position.
        /// </summary>
        public const uint PropFlag1_AbsolutePosition = 0x1000;

        /// <summary>
        /// Item Not Dropped.
        /// </summary>
        public const uint PROPFLAG_AIUNDROPPABLE = 0x00002000;

        /// <summary>
        /// Assigned to Actor.
        /// </summary>
        public const uint PROPFLAG_ASSIGNEDTOCHR = 0x00004000;

        /// <summary>
        /// Embedded Object.
        /// </summary>
        public const uint PROPFLAG_INSIDEANOTHEROBJ = 0x00008000;

        /// <summary>
        /// unknown.
        /// </summary>
        public const uint PROPFLAG_FORCE_MORTAL = 0x00010000;

        /// <summary>
        /// Invincible.
        /// </summary>
        public const uint PROPFLAG_INVINCIBLE = 0x00020000;

        /// <summary>
        /// Allow Pickup (chr_type).
        /// </summary>
        public const uint PROPFLAG_00040000 = 0x00040000;

        /// <summary>
        /// Collect Object by Interaction Button Only.
        /// </summary>
        public const uint PROPFLAG_00080000 = 0x00080000;

        /// <summary>
        /// Item Not Collectable.
        /// </summary>
        public const uint PROPFLAG_UNCOLLECTABLE = 0x00100000;

        /// <summary>
        /// Bounce and Destroy If Shot.
        /// </summary>
        public const uint PROPFLAG_00200000 = 0x00200000;

        /// <summary>
        /// unknown.
        /// </summary>
        public const uint PROPFLAG_00400000 = 0x00400000;

        /// <summary>
        /// unknown.
        /// </summary>
        public const uint PROPFLAG_00800000 = 0x00800000;

        /// <summary>
        /// Embedded Object.
        /// </summary>
        public const uint PROPFLAG_01000000 = 0x01000000;

        /// <summary>
        /// Cannot Activate Door/Object.
        /// </summary>
        public const uint PROPFLAG_CANNOT_ACTIVATE = 0x02000000;

        /// <summary>
        /// AI Sees Through Door/Object.
        /// </summary>
        public const uint PROPFLAG_04000000 = 0x04000000;

        /// <summary>
        /// Open Away From Player.
        /// </summary>
        public const uint PROPFLAG_DOOR_TWOWAY = 0x08000000;

        /// <summary>
        /// Left-Handed weapon.
        /// </summary>
        public const uint PROPFLAG_WEAPON_LEFTHANDED = 0x10000000;

        /// <summary>
        /// Glass Has Portal.
        /// </summary>
        public const uint PROPFLAG_GLASS_HASPORTAL = 0x10000000;

        /// <summary>
        /// Area Behind Door Invisible.
        /// </summary>
        public const uint PROPFLAG_CULL_BEHIND_DOOR = 0x10000000;

        /// <summary>
        /// Monitor Fixed.
        /// </summary>
        public const uint PROPFLAG_FIXED_MONITOR = 0x10000000;

        /// <summary>
        /// Disable security camera.
        /// </summary>
        public const uint PROPFLAG_CCTV_DISABLED = 0x10000000;

        /// <summary>
        /// drone gun.
        /// </summary>
        public const uint PROPFLAG_IS_DRONE_GUN = 0x10000000;

        /// <summary>
        /// Open Backwards.
        /// </summary>
        public const uint PROPFLAG_DOOR_OPENTOFRONT = 0x20000000;

        /// <summary>
        /// Special Function.
        /// </summary>
        public const uint PROPFLAG_SPECIAL_FUNC = 0x20000000;

        /// <summary>
        /// Conceal Weapon.
        /// </summary>
        public const uint PROPFLAG_CONCEAL_GUN = 0x20000000;

        /// <summary>
        /// unknown.
        /// </summary>
        public const uint PROPFLAG_MONITOR_RENDERPOSTBG = 0x40000000;

        /// <summary>
        /// Area Behind Door Visible.
        /// </summary>
        public const uint PROPFLAG_NO_PORTAL_CLOSE = 0x40000000;

        /// <summary>
        /// No Ammo on pickup.
        /// </summary>
        public const uint PROPFLAG_NO_AMMO = 0x40000000;

        /// <summary>
        /// Open By Default/Weapon Paired for Player.
        /// </summary>
        public const uint PROPFLAG_80000000 = 0x80000000;

        /// <summary>
        /// unknown.
        /// </summary>
        public const uint PROPFLAG_IS_DOUBLE = 0x80000000;
    }
}
