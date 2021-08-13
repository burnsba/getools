using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectCutscene : SetupObjectBase, ISetupObject
    {
        public SetupObjectCutscene()
            : base(Propdef.Cutscene)
        {
        }

        public int XCoord { get; set; }
        public int YCoord { get; set; }
        public int ZCoord { get; set; }
        public int LatRot { get; set; }
        public int VertRot { get; set; }
        public uint IllumPreset { get; set; }

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
            sb.Append(XCoord);
            sb.Append(", ");
            sb.Append(YCoord);
            sb.Append(", ");
            sb.Append(ZCoord);
            sb.Append(", ");
            sb.Append(LatRot);
            sb.Append(", ");
            sb.Append(VertRot);
            sb.Append(", ");
            sb.Append(IllumPreset);
        }
    }
}
