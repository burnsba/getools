using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for ammo box.
    /// </summary>
    /// <remarks>
    /// Remarks on the unused fields below:
    /// > In PD the left s16 is a model number, and the right s16 is the ammo
    /// > quantity. I'm not sure if the feature is even used
    /// >
    /// > I think at some point in development you could shoot the box and
    /// > guns would fall out of them
    /// - discord 8/12/2021
    /// </remarks>
    public class SetupObjectAmmoBox : SetupObjectGenericBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectAmmoBox"/> class.
        /// </summary>
        public SetupObjectAmmoBox()
            : base(PropDef.AmmoBox)
        {
        }

        /// <summary>
        /// Unused.
        /// Offset 0x0.
        /// </summary>
        public ushort Unused_00 { get; set; }

        /// <summary>
        /// 9mm ammo amount.
        /// Offset 0x2.
        /// </summary>
        public short Ammo9mm { get; set; }

        /// <summary>
        /// Unused.
        /// Offset 0x4.
        /// </summary>
        public ushort Unused_04 { get; set; }

        /// <summary>
        /// 9mm (2) ammo amount.
        /// Offset 0x6.
        /// </summary>
        public short Ammo9mm2 { get; set; }

        /// <summary>
        /// Unused.
        /// Offset 0x8.
        /// </summary>
        public ushort Unused_08 { get; set; }

        /// <summary>
        /// Rifle ammo amount.
        /// Offset 0xa.
        /// </summary>
        public short AmmoRifle { get; set; }

        /// <summary>
        /// Unused.
        /// Offset 0xc.
        /// </summary>
        public ushort Unused_0c { get; set; }

        /// <summary>
        /// Shotgun ammo amount.
        /// Offset 0xe.
        /// </summary>
        public short AmmoShotgun { get; set; }

        /// <summary>
        /// Unused.
        /// Offset 0x10.
        /// </summary>
        public ushort Unused_10 { get; set; }

        /// <summary>
        /// Number of HE grenades.
        /// Offset 0x12.
        /// </summary>
        public short AmmoHgrenade { get; set; }

        /// <summary>
        /// Unused.
        /// Offset 0x14.
        /// </summary>
        public ushort Unused_14 { get; set; }

        /// <summary>
        /// Number of rockets.
        /// Offset 0x16.
        /// </summary>
        public short AmmoRockets { get; set; }

        /// <summary>
        /// Unused.
        /// Offset 0x18.
        /// </summary>
        public ushort Unused_18 { get; set; }

        /// <summary>
        /// Number of remote mines.
        /// Offset 0x1a.
        /// </summary>
        public short AmmoRemoteMine { get; set; }

        /// <summary>
        /// Unused.
        /// Offset 0x1c.
        /// </summary>
        public ushort Unused_1c { get; set; }

        /// <summary>
        /// Number of proximity mines.
        /// Offset 0x1e.
        /// </summary>
        public short AmmoProximityMine { get; set; }

        /// <summary>
        /// Unused.
        /// Offset 0x20.
        /// </summary>
        public ushort Unused_20 { get; set; }

        /// <summary>
        /// Number of timed mines.
        /// Offset 0x22.
        /// </summary>
        public short AmmoTimedMine { get; set; }

        /// <summary>
        /// Unused.
        /// Offset 0x24.
        /// </summary>
        public ushort Unused_24 { get; set; }

        /// <summary>
        /// Throwing ammo amount.
        /// Offset 0x26.
        /// </summary>
        public short AmmoThrowing { get; set; }

        /// <summary>
        /// Unused.
        /// Offset 0x28.
        /// </summary>
        public ushort Unused_28 { get; set; }

        /// <summary>
        /// Number of grenade launcher rounds.
        /// Offset 0x2a.
        /// </summary>
        public short AmmoGrenadeLauncher { get; set; }

        /// <summary>
        /// Unused.
        /// Offset 0x2c.
        /// </summary>
        public ushort Unused_2c { get; set; }

        /// <summary>
        /// Magnum ammo amount.
        /// Offset 0x2e.
        /// </summary>
        public short AmmoMagnum { get; set; }

        /// <summary>
        /// Unused.
        /// Offset 0x30.
        /// </summary>
        public ushort Unused_30 { get; set; }

        /// <summary>
        /// Golden gun ammo amount.
        /// Offset 0x32.
        /// </summary>
        public short AmmoGolden { get; set; }

        /// <inheritdoc />
        public override string ToCInlineS32Array(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.Append(prefix);
            AppendToCInlineS32Array(sb);

            return sb.ToString();
        }

        /// <inheritdoc />
        protected override void AppendToCInlineS32Array(StringBuilder sb)
        {
            base.AppendToCInlineS32Array(sb);

            sb.Append(", ");
            sb.Append(Config.CMacro_WordFromShortShort(
                Formatters.IntegralTypes.ToHex4(Unused_00),
                Ammo9mm.ToString()));
            sb.Append(", ");
            sb.Append(Config.CMacro_WordFromShortShort(
                Formatters.IntegralTypes.ToHex4(Unused_04),
                Ammo9mm2.ToString()));
            sb.Append(", ");
            sb.Append(Config.CMacro_WordFromShortShort(
                Formatters.IntegralTypes.ToHex4(Unused_08),
                AmmoRifle.ToString()));
            sb.Append(", ");
            sb.Append(Config.CMacro_WordFromShortShort(
                Formatters.IntegralTypes.ToHex4(Unused_0c),
                AmmoShotgun.ToString()));
            sb.Append(", ");
            sb.Append(Config.CMacro_WordFromShortShort(
                Formatters.IntegralTypes.ToHex4(Unused_10),
                AmmoHgrenade.ToString()));
            sb.Append(", ");
            sb.Append(Config.CMacro_WordFromShortShort(
                Formatters.IntegralTypes.ToHex4(Unused_14),
                AmmoRockets.ToString()));
            sb.Append(", ");
            sb.Append(Config.CMacro_WordFromShortShort(
                Formatters.IntegralTypes.ToHex4(Unused_18),
                AmmoRemoteMine.ToString()));
            sb.Append(", ");
            sb.Append(Config.CMacro_WordFromShortShort(
                Formatters.IntegralTypes.ToHex4(Unused_1c),
                AmmoProximityMine.ToString()));
            sb.Append(", ");
            sb.Append(Config.CMacro_WordFromShortShort(
                Formatters.IntegralTypes.ToHex4(Unused_20),
                AmmoTimedMine.ToString()));
            sb.Append(", ");
            sb.Append(Config.CMacro_WordFromShortShort(
                Formatters.IntegralTypes.ToHex4(Unused_24),
                AmmoThrowing.ToString()));
            sb.Append(", ");
            sb.Append(Config.CMacro_WordFromShortShort(
                Formatters.IntegralTypes.ToHex4(Unused_28),
                AmmoGrenadeLauncher.ToString()));
            sb.Append(", ");
            sb.Append(Config.CMacro_WordFromShortShort(
                Formatters.IntegralTypes.ToHex4(Unused_2c),
                AmmoMagnum.ToString()));
            sb.Append(", ");
            sb.Append(Config.CMacro_WordFromShortShort(
                Formatters.IntegralTypes.ToHex4(Unused_30),
                AmmoGolden.ToString()));
        }
    }
}
