using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectSetGuardAttribute : SetupObjectBase, ISetupObject
    {
        public SetupObjectSetGuardAttribute()
            : base(Propdef.SetGuardAttribute)
        {
        }

        public uint GuardId { get; set; }
        public int Attribute { get; set; }

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
            sb.Append(GuardId);
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(Attribute));
        }
    }
}
