using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    public class IntroWatchTime : IntroBase
    {
        public IntroWatchTime()
            : base(IntroType.WatchTime)
        {
        }

        public UInt32 Hour { get; set; }
        public UInt32 Minute { get; set; }

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
            sb.Append(Hour);
            sb.Append(", ");
            sb.Append(Minute);
        }
    }
}
