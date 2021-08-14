using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for weapon.
    /// </summary>
    public class SetupObjectWeapon : SetupObjectGenericBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectWeapon"/> class.
        /// </summary>
        public SetupObjectWeapon()
            : base(PropDef.Collectable)
        {
        }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x0.
        /// </summary>
        public byte GunPickup { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x1.
        /// </summary>
        public byte LinkedItem { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x2.
        /// </summary>
        public UInt16 Timer { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x4.
        /// </summary>
        public UInt32 PointerLinkedItem { get; set; }

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
