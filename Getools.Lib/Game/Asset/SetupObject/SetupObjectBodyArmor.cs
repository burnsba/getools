using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for body armor.
    /// </summary>
    public class SetupObjectBodyArmor : SetupObjectGenericBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectBodyArmor"/> class.
        /// </summary>
        public SetupObjectBodyArmor()
            : base(PropDef.Armour)
        {
        }

        /// <summary>
        /// Gets or sets body armor strength.
        /// </summary>
        public int ArmorStrength { get; set; }

        /// <summary>
        /// Gets or sets body armor percent.
        /// </summary>
        public int ArmorPercent { get; set; }

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
            sb.Append(ArmorStrength);

            sb.Append(", ");
            sb.Append(ArmorPercent);
        }
    }
}
