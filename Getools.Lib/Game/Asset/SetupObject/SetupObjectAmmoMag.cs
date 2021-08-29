using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for ammo magazine.
    /// </summary>
    public class SetupObjectAmmoMag : SetupObjectGenericBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectAmmoMag"/> class.
        /// </summary>
        public SetupObjectAmmoMag()
            : base(PropDef.AmmoMag)
        {
        }

        /// <summary>
        /// Ammo type.
        /// See <see cref="Getools.Lib.Game.Enums.AmmoType"/>.
        /// </summary>
        public int AmmoType { get; set; }

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
            sb.Append(AmmoType);
        }
    }
}
