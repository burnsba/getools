using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for tinted glass.
    /// </summary>
    public class SetupObjectGlassTinted : SetupObjectGenericBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectGlassTinted"/> class.
        /// </summary>
        public SetupObjectGlassTinted()
            : base(PropDef.TintedGlass)
        {
        }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public int Unknown04 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public int Unknown08 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public int Unknown0c { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public int Unknown10 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public int Unknown14 { get; set; }

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
            sb.Append(Unknown04);
            sb.Append(", ");
            sb.Append(Unknown08);
            sb.Append(", ");
            sb.Append(Unknown0c);
            sb.Append(", ");
            sb.Append(Unknown10);
            sb.Append(", ");
            sb.Append(Unknown14);
        }
    }
}
