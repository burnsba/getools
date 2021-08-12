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
    }
}
