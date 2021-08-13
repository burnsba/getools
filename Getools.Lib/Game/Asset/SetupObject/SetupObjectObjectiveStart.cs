using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectObjectiveStart : SetupObjectBase, ISetupObject
    {
        public SetupObjectObjectiveStart()
            : base(Propdef.ObjectiveStart)
        {
        }

        public int ObjectiveNumber { get; set; }
        public int TextId { get; set; }
        public int MinDifficulty { get; set; }

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
            sb.Append(ObjectiveNumber);
            sb.Append(", ");
            sb.Append(TextId);
            sb.Append(", ");
            sb.Append(MinDifficulty);
        }
    }
}
