using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Intro
{
    /// <summary>
    /// Specifies start weapon for either or both hands.
    /// </summary>
    public class IntroStartWeapon : IntroBase
    {
        /// <summary>
        /// The overall allocated size of this object in bytes, including child elements.
        /// </summary>
        public new const int SizeOf = IntroBase.BaseSizeOf + (3 * Config.TargetWordSize);

        /// <summary>
        /// Initializes a new instance of the <see cref="IntroStartWeapon"/> class.
        /// </summary>
        public IntroStartWeapon()
            : base(IntroType.StartWeapon)
        {
        }

        /// <summary>
        /// Gets or sets right hand weapon.
        /// Set to -1 if not used.
        /// Struct offset 0x0.
        /// See <see cref="Game.Enums.ItemIds"/> for values.
        /// </summary>
        public Int32 Right { get; set; }

        /// <summary>
        /// Gets or sets left hand weapon.
        /// Set to -1 if not used.
        /// Struct offset 0x4.
        /// See <see cref="Game.Enums.ItemIds"/> for values.
        public Int32 Left { get; set; }

        /// <summary>
        /// Gets or sets the order of intro start weapon declarations.
        /// (This is the index of these entries).
        /// </summary>
        public UInt32 SetNum { get; set; }

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
            BitUtility.Insert32Big(bytes, pos, (int)Right);
            pos += Config.TargetPointerSize;

            BitUtility.Insert32Big(bytes, pos, (int)Left);
            pos += Config.TargetPointerSize;

            BitUtility.Insert32Big(bytes, pos, (int)SetNum);
            pos += Config.TargetPointerSize;

            var result = context.AssembleAppendBytes(bytes, Config.TargetWordSize);
            BaseDataOffset = result.DataStartAddress;
        }

        /// <inheritdoc />
        protected override void AppendToCInlineS32Array(StringBuilder sb)
        {
            base.AppendToCInlineS32Array(sb);

            sb.Append(", ");
            sb.Append(Right);
            sb.Append(", ");
            sb.Append(Left);
            sb.Append(", ");
            sb.Append(SetNum);
        }
    }
}
