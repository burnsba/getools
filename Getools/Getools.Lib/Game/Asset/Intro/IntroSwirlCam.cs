using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Intro
{
    /// <summary>
    /// Definition for 2nd cinema intro swirl cam.
    /// </summary>
    public class IntroSwirlCam : IntroBase
    {
        public new const int SizeOf = IntroBase.BaseSizeOf + (7 * Config.TargetWordSize);

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
        [JsonIgnore]
        public override int BaseDataSize
        {
            get
            {
                return SizeOf;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc />
        public override string ToCInlineS32Array(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.Append(prefix);
            AppendToCInlineS32Array(sb);

            return sb.ToString();
        }

        /// <inheritdoc />
        public override void Collect(IAssembleContext context)
        {
            context.AppendToDataSection(this);
        }

        /// <inheritdoc />
        public override void Assemble(IAssembleContext context)
        {
            var size = SizeOf;
            var bytes = new byte[size];
            int pos = 0;

            // base data
            BitUtility.Insert32Big(bytes, pos, (int)Type);
            pos += Config.TargetPointerSize;

            // this object data
            BitUtility.Insert32Big(bytes, pos, (int)Unknown_00);
            pos += Config.TargetPointerSize;

            BitUtility.Insert32Big(bytes, pos, (int)X);
            pos += Config.TargetPointerSize;

            BitUtility.Insert32Big(bytes, pos, (int)Y);
            pos += Config.TargetPointerSize;

            BitUtility.Insert32Big(bytes, pos, (int)Z);
            pos += Config.TargetPointerSize;

            BitUtility.Insert32Big(bytes, pos, (int)SplineScale);
            pos += Config.TargetPointerSize;

            BitUtility.Insert32Big(bytes, pos, (int)Duration);
            pos += Config.TargetPointerSize;

            BitUtility.Insert32Big(bytes, pos, (int)Flags);
            pos += Config.TargetPointerSize;

            var result = context.AssembleAppendBytes(bytes, Config.TargetWordSize);
            BaseDataOffset = result.DataStartAddress;
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
