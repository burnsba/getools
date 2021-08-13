using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    public class IntroSpawn : IntroBase
    {
        public IntroSpawn()
            : base(IntroType.Spawn)
        {
        }

        public UInt32 Unknown_00 { get; set; }
        public UInt32 Unknown_04 { get; set; }

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
            sb.Append(Unknown_00);
            sb.Append(", ");
            sb.Append(Unknown_04);
        }
    }
}
