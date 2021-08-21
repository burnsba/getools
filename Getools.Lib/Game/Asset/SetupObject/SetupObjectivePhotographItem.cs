using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list objective to photograph item.
    /// </summary>
    public class SetupObjectivePhotographItem : SetupObjectBase, ISetupObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectivePhotographItem"/> class.
        /// </summary>
        public SetupObjectivePhotographItem()
            : base(PropDef.ObjectivePhotograph)
        {
        }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public uint TagId { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// Struct offset 0x4.
        /// </summary>
        public uint Unknown_04 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// Struct offset 0x8.
        /// </summary>
        public uint Unknown_08 { get; set; }

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
            sb.Append(TagId);
            sb.Append(", ");
            sb.Append(Unknown_04);
            sb.Append(", ");
            sb.Append(Unknown_08);
        }
    }
}
