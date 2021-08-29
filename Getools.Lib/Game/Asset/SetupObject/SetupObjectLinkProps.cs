using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition to link props.
    /// </summary>
    public class SetupObjectLinkProps : SetupObjectBase, ISetupObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectLinkProps"/> class.
        /// </summary>
        public SetupObjectLinkProps()
            : base(PropDef.LinkProps)
        {
        }

        /// <summary>
        /// Item offset 1.
        /// </summary>
        public int Offset1 { get; set; }

        /// <summary>
        /// Item offset 2.
        /// </summary>
        public int Offset2 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public int Unknown08 { get; set; }

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
            sb.Append(Offset1);
            sb.Append(", ");
            sb.Append(Offset2);
            sb.Append(", ");
            sb.Append(Unknown08);
        }
    }
}
