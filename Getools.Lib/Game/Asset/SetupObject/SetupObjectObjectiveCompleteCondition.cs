using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectObjectiveCompleteCondition : SetupObjectBase, ISetupObject
    {
        public SetupObjectObjectiveCompleteCondition()
            : base(Propdef.ObjectiveCompleteCondition)
        {
        }

        public int TestVal { get; set; }

        public override string ToCInlineS32Array(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.Append(prefix);
            AppendToCInlineS32Array(sb);

            return sb.ToString();
        }

        protected override void AppendToCInlineS32Array(StringBuilder sb)
        {
            base.AppendToCInlineS32Array(sb);

            sb.Append(", ");
            sb.Append(TestVal);
        }
    }
}
