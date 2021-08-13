using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    public class IntroStartAmmo : IntroBase
    {
        public IntroStartAmmo()
            : base(IntroType.StartAmmo)
        {
        }

        public UInt32 AmmoType { get; set; }
        public UInt32 Quantity { get; set; }
        public UInt32 Set { get; set; }

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
            sb.Append(AmmoType);
            sb.Append(", ");
            sb.Append(Quantity);
            sb.Append(", ");
            sb.Append(Set);
        }
    }
}
