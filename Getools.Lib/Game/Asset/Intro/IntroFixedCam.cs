using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    public class IntroFixedCam : IntroBase
    {
        public IntroFixedCam()
            : base(IntroType.FixedCam)
        {
        }

        public UInt32 X { get; set; }
        public UInt32 Y { get; set; }
        public UInt32 Z { get; set; }
        public UInt32 LatRot { get; set; }
        public UInt32 VertRot { get; set; }
        public UInt32 Preset { get; set; }
        public UInt32 TextId { get; set; }
        public UInt32 Text2Id { get; set; }
        public UInt32 Unknown_20 { get; set; }

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
            sb.Append(Formatters.IntegralTypes.ToHex8(X));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(Y));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(Z));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(LatRot));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(VertRot));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(Preset));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(TextId));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(Text2Id));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(Unknown_20));
        }
    }
}
