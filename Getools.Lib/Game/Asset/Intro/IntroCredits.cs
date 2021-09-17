using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Intro
{
    /// <summary>
    /// Credits.
    /// </summary>
    public class IntroCredits : IntroBase
    {
        public const int SizeOf = IntroBase.BaseSizeOf + (1 * Config.TargetPointerSize);

        /// <summary>
        /// Initializes a new instance of the <see cref="IntroCredits"/> class.
        /// </summary>
        public IntroCredits()
            : base(IntroType.Credits)
        {
        }

        ///// <summary>
        ///// Pointer to credits data.
        ///// </summary>
        //public int DataOffset { get; set; }

        public PointerVariable CreditsDataPointer { get; set; }

        /// <summary>
        /// Credits data.
        /// </summary>
        public CreditsContainer Credits { get; set; }

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
            int pointerOffset = 0;

            // base data
            BitUtility.Insert32Big(bytes, pos, (int)Type);
            pos += Config.TargetPointerSize;

            // this object data
            // pointer value will be resolved when linking
            pointerOffset = pos;
            BitUtility.Insert32Big(bytes, pos, 0);
            pos += Config.TargetPointerSize;

            var result = context.AssembleAppendBytes(bytes, Config.TargetWordSize);
            BaseDataOffset = result.DataStartAddress;

            CreditsDataPointer.BaseDataOffset = BaseDataOffset + pointerOffset;

            context.RegisterPointer(CreditsDataPointer);
        }

        /// <inheritdoc />
        protected override void AppendToCInlineS32Array(StringBuilder sb)
        {
            base.AppendToCInlineS32Array(sb);

            if (!object.ReferenceEquals(null, Credits))
            {
                sb.Append(", ");
                sb.Append(Formatters.Strings.ToCPointerOrNull(Credits.VariableName));
            }
            else
            {
                sb.Append(", ");
                sb.Append(CreditsDataPointer.AddressOfVariableName);
            }
        }
    }
}
