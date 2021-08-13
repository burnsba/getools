using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectAmmoBox : SetupObjectGenericBase
    {
        public SetupObjectAmmoBox()
            : base(Propdef.AmmoBox)
        {
        }

        public ushort Unused_00 { get; set; }
        public short Ammo9mm { get; set; }
        public ushort Unused_04 { get; set; }
        public short Ammo9mm2 { get; set; }
        public ushort Unused_08 { get; set; }
        public short AmmoRifle { get; set; }
        public ushort Unused_0c { get; set; }
        public short AmmoShotgun { get; set; }
        public ushort Unused_10 { get; set; }
        public short AmmoHgrenade { get; set; }
        public ushort Unused_14 { get; set; }
        public short AmmoRockets { get; set; }
        public ushort Unused_18 { get; set; }
        public short AmmoRemoteMine { get; set; }
        public ushort Unused_1c { get; set; }
        public short AmmoProximityMine { get; set; }
        public ushort Unused_20 { get; set; }
        public short AmmoTimedMine { get; set; }
        public ushort Unused_24 { get; set; }
        public short AmmoThrowing { get; set; }
        public ushort Unused_28 { get; set; }
        public short AmmoGrenadeLauncher { get; set; }
        public ushort Unused_2c { get; set; }
        public short AmmoMagnum { get; set; }
        public ushort Unused_30 { get; set; }
        public short AmmoGolden { get; set; }

        public override string ToCInlineS32Array(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.Append(prefix);
            AppendToCInlineS32Array(sb);

            return sb.ToString();
        }

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
