using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    public class IntroStartWeapon : IntroBase
    {
        public IntroStartWeapon()
            : base(IntroType.StartWeapon)
        {
        }

        public Int32 Left { get; set; }
        public Int32 Right { get; set; }
        public UInt32 SetNum { get; set; }

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
            sb.Append(Left);
            sb.Append(", ");
            sb.Append(Right);
            sb.Append(", ");
            sb.Append(SetNum);
        }
    }
}
