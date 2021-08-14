using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    /// <summary>
    /// Definition for 2nd cinema intro swirl cam.
    /// </summary>
    public class IntroSwirlCam : IntroBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntroSwirlCam"/> class.
        /// </summary>
        public IntroSwirlCam()
            : base(IntroType.SwirlCam)
        {
        }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 Unknown_00 { get; set; }

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
        /// Gets or sets spline scale.
        /// </summary>
        public UInt32 SplineScale { get; set; }

        /// <summary>
        /// Gets or sets Duration.
        /// TODO: determine units (frames?).
        /// </summary>
        public UInt32 Duration { get; set; }

        /// <summary>
        /// Gets or sets flags.
        /// TODO: determine available flags.
        /// </summary>
        public UInt32 Flags { get; set; }

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
