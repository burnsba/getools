using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for safe item.
    /// </summary>
    public class SetupObjectSafeItem : SetupObjectBase, ISetupObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectSafeItem"/> class.
        /// </summary>
        public SetupObjectSafeItem()
            : base(PropDef.SafeItem)
        {
        }

        /// <summary>
        /// Item.
        /// </summary>
        public int Item { get; set; }

        /// <summary>
        /// Safe.
        /// </summary>
        public int Safe { get; set; }

        /// <summary>
        /// Door
        /// </summary>
        public int Door { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// Maybe end of record marker.
        /// </summary>
        public int Empty { get; set; }

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
            sb.Append(Item);
            sb.Append(", ");
            sb.Append(Safe);
            sb.Append(", ");
            sb.Append(Door);
            sb.Append(", ");
            sb.Append(Empty);
        }
    }
}
