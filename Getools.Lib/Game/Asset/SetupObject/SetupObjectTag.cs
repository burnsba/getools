using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectTag : SetupObjectBase, ISetupObject
    {
        public SetupObjectTag()
            : base(Propdef.Tag)
        {
        }

        public ushort TagId { get; set; }
        public ushort Value { get; set; }
        public uint Unknown_04 { get; set; }
        public uint Unknown_08 { get; set; }

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
            sb.AppendFormat(
                Config.CMacro_WordFromShorts_Format,
                TagId.ToString(),
                Formatters.IntegralTypes.ToHex4(Value));
            sb.Append(", ");
            sb.Append(Unknown_04);
            sb.Append(", ");
            sb.Append(Unknown_08);
        }
    }
}
