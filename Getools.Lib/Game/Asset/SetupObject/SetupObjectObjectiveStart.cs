using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition to mark start of an objective definition.
    /// </summary>
    public class SetupObjectObjectiveStart : SetupObjectBase, ISetupObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectObjectiveStart"/> class.
        /// </summary>
        public SetupObjectObjectiveStart()
            : base(PropDef.ObjectiveStart)
        {
        }

        /// <summary>
        /// Gets or sets objective number.
        /// Struct offset 0x0.
        /// </summary>
        public int ObjectiveNumber { get; set; }

        /// <summary>
        /// Gets or sets text block id.
        /// Struct offset 0x4.
        /// </summary>
        public int TextId { get; set; }

        /// <summary>
        /// Gets or sets minimum difficulty this appears for.
        /// Struct offset 0x8.
        /// </summary>
        public int MinDifficulty { get; set; }

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
            sb.Append(ObjectiveNumber);
            sb.Append(", ");
            sb.Append(TextId);
            sb.Append(", ");
            sb.Append(MinDifficulty);
        }
    }
}
