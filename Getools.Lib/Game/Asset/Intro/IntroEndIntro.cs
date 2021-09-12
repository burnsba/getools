using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;

namespace Getools.Lib.Game.Asset.Intro
{
    /// <summary>
    /// Marker for end of intro section in setup.
    /// </summary>
    public class IntroEndIntro : IntroBase
    {
        public const int SizeOf = IntroBase.BaseSizeOf;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntroEndIntro"/> class.
        /// </summary>
        public IntroEndIntro()
            : base(IntroType.EndIntro)
        {
        }

        /// <inheritdoc />
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

            var result = context.AssembleAppendBytes(bytes, Config.TargetWordSize);
            BaseDataOffset = result.DataStartAddress;
        }
    }
}
