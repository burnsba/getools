using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    public class IntroSwirlCam : IntroBase
    {
        public IntroSwirlCam()
            : base(IntroType.SwirlCam)
        {
        }

        public UInt32 Unknown_00 { get; set; }
        public UInt32 X { get; set; }
        public UInt32 Y { get; set; }
        public UInt32 Z { get; set; }
        public UInt32 SplineScale { get; set; }
        public UInt32 Duration { get; set; }
        public UInt32 Flags { get; set; }

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
            sb.Append(Formatters.IntegralTypes.ToHex8(X));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(Y));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(Z));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(SplineScale));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(Duration));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(Flags));
        }
    }
}
