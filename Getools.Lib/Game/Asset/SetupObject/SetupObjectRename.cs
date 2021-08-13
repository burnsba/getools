using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectRename : SetupObjectBase, ISetupObject
    {
        public SetupObjectRename()
            : base(Propdef.Rename)
        {
        }

        public uint ObjectOffset { get; set; }
        public uint InventoryId { get; set; }
        public uint Text1 { get; set; }
        public uint Text2 { get; set; }
        public uint Text3 { get; set; }
        public uint Text4 { get; set; }
        public uint Text5 { get; set; }
        public uint Unknown20 { get; set; }
        public uint Unknown24 { get; set; }

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
            sb.Append(Formatters.IntegralTypes.ToHex8(ObjectOffset));
            sb.Append(", ");
            sb.Append(InventoryId);
            sb.Append(", ");
            sb.Append(Text1);
            sb.Append(", ");
            sb.Append(Text2);
            sb.Append(", ");
            sb.Append(Text3);
            sb.Append(", ");
            sb.Append(Text4);
            sb.Append(", ");
            sb.Append(Text5);
            sb.Append(", ");
            sb.Append(Unknown20);
            sb.Append(", ");
            sb.Append(Unknown24);
        }
    }
}
