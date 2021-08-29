using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition to dual wield items.
    /// </summary>
    public class SetupObjectLinkItems : SetupObjectBase, ISetupObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectLinkItems"/> class.
        /// </summary>
        public SetupObjectLinkItems()
            : base(PropDef.LinkItems)
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
        }
    }
}
