using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectWeapon : SetupObjectGenericBase
    {
        public SetupObjectWeapon()
            : base(Propdef.Weapon)
        {
        }

        public byte GunPickup { get; set; }
        public byte LinkedItem { get; set; }
        public UInt16 Timer { get; set; }
        public UInt32 PointerLinkedItem { get; set; }

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
            sb.AppendFormat(
                Config.CMacro_WordFromByteByteShort(
                    Formatters.IntegralTypes.ToHex2(GunPickup),
                    Formatters.IntegralTypes.ToHex2(LinkedItem),
                    Formatters.IntegralTypes.ToHex4(Timer)));
            sb.Append(", ");
            sb.Append(PointerLinkedItem);
        }
    }
}
