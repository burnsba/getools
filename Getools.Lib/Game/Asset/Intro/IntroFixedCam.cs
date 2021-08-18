using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    /// <summary>
    /// Intro cam (first cinema).
    /// </summary>
    public class IntroFixedCam : IntroBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntroFixedCam"/> class.
        /// </summary>
        public IntroFixedCam()
            : base(IntroType.FixedCam)
        {
        }

        /// <summary>
        /// Gets or sets x coord.
        /// TODO: determine if this is float, and/or scaled at stage load.
        /// </summary>
        public UInt32 X { get; set; }

        /// <summary>
        /// Gets or sets y coord.
        /// TODO: determine if this is float, and/or scaled at stage load.
        /// </summary>
        public UInt32 Y { get; set; }

        /// <summary>
        /// Gets or sets z coord.
        /// TODO: determine if this is float, and/or scaled at stage load.
        /// </summary>
        public UInt32 Z { get; set; }

        /// <summary>
        /// Gets or sets lateral rotation.
        /// TODO: determine if this is float, and/or scaled at stage load.
        /// </summary>
        public UInt32 LatRot { get; set; }

        /// <summary>
        /// Gets or sets vertical rotation.
        /// TODO: determine if this is float, and/or scaled at stage load.
        /// </summary>
        public UInt32 VertRot { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// </summary>
        public UInt32 Preset { get; set; }

        /// <summary>
        /// Gets or sets id of text shown during cinema.
        /// </summary>
        public UInt32 TextId { get; set; }

        /// <summary>
        /// Gets or sets id of 2nd text shown during cinema.
        /// </summary>
        public UInt32 Text2Id { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// </summary>
        public UInt32 Unknown_20 { get; set; }

        /// <inheritdoc />
        public override string ToCInlineS32Array(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.Append(prefix);
            AppendToCInlineS32Array(sb);

            return sb.ToString();
        }

        /// <inheritdoc />
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
