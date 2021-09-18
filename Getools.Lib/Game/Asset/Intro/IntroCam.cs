using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Intro
{
    /// <summary>
    /// Used by Bond during the intro swirl.
    /// </summary>
    /// <remarks>
    /// Discord comment:
    /// > Animation is used by Bond during the intro swirl process. It's actually a bunch
    /// > of preset values for each animation. To get around this limitations, some setups
    /// > such as Silo will instead not set an animation used for the intro swirl, but
    /// > will have a dedicated AI list that'll just watch for when the intro swirl has
    /// > triggered, and then trigger an animation that isn't selectable by the intro
    /// > block animation's range. Sometimes this is used because the intro swirl is
    /// > quite long, so some manual timing is required via AI list.
    /// - carnivorous Aug 14, 2021
    /// </remarks>
    public class IntroCam : IntroBase
    {
        /// <summary>
        /// The overall allocated size of this object in bytes, including child elements.
        /// </summary>
        public new const int SizeOf = IntroBase.BaseSizeOf + (1 * Config.TargetWordSize);

        /// <summary>
        /// Initializes a new instance of the <see cref="IntroCam"/> class.
        /// </summary>
        public IntroCam()
            : base(IntroType.IntroCam)
        {
        }

        /// <summary>
        /// Gets or sets animation id.
        /// </summary>
        public UInt32 Animation { get; set; }

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
            BitUtility.Insert32Big(bytes, pos, (int)Animation);
            pos += Config.TargetPointerSize;

            var result = context.AssembleAppendBytes(bytes, Config.TargetWordSize);
            BaseDataOffset = result.DataStartAddress;
        }

        /// <inheritdoc />
        protected override void AppendToCInlineS32Array(StringBuilder sb)
        {
            base.AppendToCInlineS32Array(sb);

            sb.Append(", ");
            sb.Append(Animation);
        }
    }
}
