using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectSingleMonitor : SetupObjectGenericBase
    {
        public SetupObjectSingleMonitor()
            : base(Propdef.SingleMonitor)
        {
        }

        public uint CurNumCmdsFromStartRotation { get; set; }
        public uint LoopCounter { get; set; }
        public uint ImgnumOrPtrheader { get; set; }
        public uint Rotation { get; set; }
        public uint CurHzoom { get; set; }
        public uint CurHzoomTime { get; set; }
        public uint FinalHzoomTime { get; set; }
        public uint InitialHzoom { get; set; }
        public uint FinalHzoom { get; set; }
        public uint CurVzoom { get; set; }
        public uint CurVzoomTime { get; set; }
        public uint FinalVzoomTime { get; set; }
        public uint InitialVzoom { get; set; }
        public uint FinalVzoom { get; set; }
        public uint CurHpos { get; set; }
        public uint CurHscrollTime { get; set; }
        public uint FinalHscrollTime { get; set; }
        public uint InitialHpos { get; set; }
        public uint FinalHpos { get; set; }
        public uint CurVpos { get; set; }
        public uint CurVscrollTime { get; set; }
        public uint FinalVscrollTime { get; set; }
        public uint InitialVpos { get; set; }
        public uint FinalVpos { get; set; }
        public byte CurRed { get; set; }
        public byte InitialRed { get; set; }
        public byte FinalRed { get; set; }
        public byte CurGreen { get; set; }
        public byte InitialGreen { get; set; }
        public byte FinalGreen { get; set; }
        public byte CurBlue { get; set; }
        public byte InitialBlue { get; set; }
        public byte FinalBlue { get; set; }
        public byte CurAlpha { get; set; }
        public byte InitialAlpha { get; set; }
        public byte FinalAlpha { get; set; }
        public uint CurColorTransitionTime { get; set; }
        public uint FinalColorTransitionTime { get; set; }
        public uint BackwardMonLink { get; set; }
        public uint ForwardMonLink { get; set; }
        public uint AnimationNum { get; set; }

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
