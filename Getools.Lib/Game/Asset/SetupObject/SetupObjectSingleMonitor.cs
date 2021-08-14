using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition to describe a tv monitor.
    /// </summary>
    public class SetupObjectSingleMonitor : SetupObjectGenericBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectSingleMonitor"/> class.
        /// </summary>
        public SetupObjectSingleMonitor()
            : base(PropDef.SingleMonitor)
        {
        }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x0.
        /// </summary>
        public uint CurNumCmdsFromStartRotation { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x4.
        /// </summary>
        public uint LoopCounter { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x8.
        /// </summary>
        public uint ImgnumOrPtrheader { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0xc.
        /// </summary>
        public uint Rotation { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x10.
        /// </summary>
        public uint CurHzoom { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x14.
        /// </summary>
        public uint CurHzoomTime { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x18.
        /// </summary>
        public uint FinalHzoomTime { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x1c.
        /// </summary>
        public uint InitialHzoom { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x20.
        /// </summary>
        public uint FinalHzoom { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x24.
        /// </summary>
        public uint CurVzoom { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x28.
        /// </summary>
        public uint CurVzoomTime { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x2c.
        /// </summary>
        public uint FinalVzoomTime { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x30.
        /// </summary>
        public uint InitialVzoom { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x34.
        /// </summary>
        public uint FinalVzoom { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x38.
        /// </summary>
        public uint CurHpos { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x3c.
        /// </summary>
        public uint CurHscrollTime { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x40.
        /// </summary>
        public uint FinalHscrollTime { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x44.
        /// </summary>
        public uint InitialHpos { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x48.
        /// </summary>
        public uint FinalHpos { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x4c.
        /// </summary>
        public uint CurVpos { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x50.
        /// </summary>
        public uint CurVscrollTime { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x54.
        /// </summary>
        public uint FinalVscrollTime { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x58.
        /// </summary>
        public uint InitialVpos { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x5c.
        /// </summary>
        public uint FinalVpos { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x60.
        /// </summary>
        public byte CurRed { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x61.
        /// </summary>
        public byte InitialRed { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x62.
        /// </summary>
        public byte FinalRed { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x63.
        /// </summary>
        public byte CurGreen { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x64.
        /// </summary>
        public byte InitialGreen { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x65.
        /// </summary>
        public byte FinalGreen { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x66.
        /// </summary>
        public byte CurBlue { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x67.
        /// </summary>
        public byte InitialBlue { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x68.
        /// </summary>
        public byte FinalBlue { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x69.
        /// </summary>
        public byte CurAlpha { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x6a.
        /// </summary>
        public byte InitialAlpha { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x6b.
        /// </summary>
        public byte FinalAlpha { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x6c.
        /// </summary>
        public uint CurColorTransitionTime { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x70.
        /// </summary>
        public uint FinalColorTransitionTime { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x74.
        /// </summary>
        public uint BackwardMonLink { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x78.
        /// </summary>
        public uint ForwardMonLink { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset: 0x7c.
        /// </summary>
        public uint AnimationNum { get; set; }

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
            sb.Append(CurNumCmdsFromStartRotation);
            sb.Append(", ");
            sb.Append(LoopCounter);
            sb.Append(", ");
            sb.Append(ImgnumOrPtrheader);
            sb.Append(", ");
            sb.Append(Rotation);
            sb.Append(", ");
            sb.Append(CurHzoom);
            sb.Append(", ");
            sb.Append(CurHzoomTime);
            sb.Append(", ");
            sb.Append(FinalHzoomTime);
            sb.Append(", ");
            sb.Append(InitialHzoom);
            sb.Append(", ");
            sb.Append(FinalHzoom);
            sb.Append(", ");
            sb.Append(CurVzoom);
            sb.Append(", ");
            sb.Append(CurVzoomTime);
            sb.Append(", ");
            sb.Append(FinalVzoomTime);
            sb.Append(", ");
            sb.Append(InitialVzoom);
            sb.Append(", ");
            sb.Append(FinalVzoom);
            sb.Append(", ");
            sb.Append(CurHpos);
            sb.Append(", ");
            sb.Append(CurHscrollTime);
            sb.Append(", ");
            sb.Append(FinalHscrollTime);
            sb.Append(", ");
            sb.Append(InitialHpos);
            sb.Append(", ");
            sb.Append(FinalHpos);
            sb.Append(", ");
            sb.Append(CurVpos);
            sb.Append(", ");
            sb.Append(CurVscrollTime);
            sb.Append(", ");
            sb.Append(FinalVscrollTime);
            sb.Append(", ");
            sb.Append(InitialVpos);
            sb.Append(", ");
            sb.Append(FinalVpos);
            sb.Append(", ");
            sb.AppendFormat(
                Config.CMacro_WordFromByteByteByteByte(
                    CurRed,
                    InitialRed,
                    FinalRed,
                    CurGreen));
            sb.Append(", ");
            sb.AppendFormat(
                Config.CMacro_WordFromByteByteByteByte(
                    InitialGreen,
                    FinalGreen,
                    CurBlue,
                    InitialBlue));
            sb.Append(", ");
            sb.AppendFormat(
                Config.CMacro_WordFromByteByteByteByte(
                    FinalBlue,
                    CurAlpha,
                    InitialAlpha,
                    FinalAlpha));
            sb.Append(", ");
            sb.Append(CurColorTransitionTime);
            sb.Append(", ");
            sb.Append(FinalColorTransitionTime);
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(BackwardMonLink));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(ForwardMonLink));
            sb.Append(", ");
            sb.Append(AnimationNum);
        }
    }
}
