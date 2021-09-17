using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Intro
{
    /// <summary>
    /// Intro definition to set the watch time.
    /// </summary>
    public class IntroWatchTime : IntroBase
    {
        public const int SizeOf = IntroBase.BaseSizeOf + (2 * Config.TargetWordSize);

        /// <summary>
        /// Initializes a new instance of the <see cref="IntroWatchTime"/> class.
        /// </summary>
        public IntroWatchTime()
            : base(IntroType.WatchTime)
        {
        }

        /// <summary>
        /// Gets or sets intro watch starting hour.
        /// </summary>
        public UInt32 Hour { get; set; }

        /// <summary>
        /// Gets or sets intro watch starting minute.
        /// </summary>
        public UInt32 Minute { get; set; }

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
            BitUtility.Insert32Big(bytes, pos, (int)Hour);
            pos += Config.TargetPointerSize;

            BitUtility.Insert32Big(bytes, pos, (int)Minute);
            pos += Config.TargetPointerSize;

            var result = context.AssembleAppendBytes(bytes, Config.TargetWordSize);
            BaseDataOffset = result.DataStartAddress;
        }

        /// <inheritdoc />
        protected override void AppendToCInlineS32Array(StringBuilder sb)
        {
            base.AppendToCInlineS32Array(sb);

            sb.Append(", ");
            sb.Append(Hour);
            sb.Append(", ");
            sb.Append(Minute);
        }
    }
}
