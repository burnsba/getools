using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for setting/changing a guard attribute.
    /// </summary>
    public class SetupObjectSetGuardAttribute : SetupObjectBase, ISetupObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectSetGuardAttribute"/> class.
        /// </summary>
        public SetupObjectSetGuardAttribute()
            : base(PropDef.SetGuardAttribute)
        {
        }

        /// <summary>
        /// Gets or sets guard id.
        /// Struct offset 0x0.
        /// </summary>
        public uint GuardId { get; set; }

        /// <summary>
        /// Gets or sets guard attribute value.
        /// Struct offset 0x4.
        /// </summary>
        public int Attribute { get; set; }

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
            sb.Append(GuardId);
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(Attribute));
        }
    }
}
