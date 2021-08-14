using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for objective completion condition.
    /// </summary>
    public class SetupObjectObjectiveCompleteCondition : SetupObjectBase, ISetupObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectObjectiveCompleteCondition"/> class.
        /// </summary>
        public SetupObjectObjectiveCompleteCondition()
            : base(PropDef.ObjectiveCompleteCondition)
        {
        }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x0.
        /// </summary>
        public int TestVal { get; set; }

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
            sb.Append(TestVal);
        }
    }
}
