using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Intro
{
    /// <summary>
    /// Intro spawn definition.
    /// </summary>
    public class IntroSpawn : IntroBase
    {
        /// <summary>
        /// The overall allocated size of this object in bytes, including child elements.
        /// </summary>
        public new const int SizeOf = IntroBase.BaseSizeOf + (2 * Config.TargetWordSize);

        /// <summary>
        /// Initializes a new instance of the <see cref="IntroSpawn"/> class.
        /// </summary>
        public IntroSpawn()
            : base(IntroType.Spawn)
        {
        }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 Unknown_00 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 Unknown_04 { get; set; }

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

            BitUtility.Insert32Big(bytes, pos, (int)Unknown_04);
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
            sb.Append(Unknown_04);
        }
    }
}
