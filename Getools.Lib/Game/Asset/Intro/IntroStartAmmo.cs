using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;

namespace Getools.Lib.Game.Asset.Intro
{
    /// <summary>
    /// Starting ammo definition.
    /// </summary>
    public class IntroStartAmmo : IntroBase
    {
        public const int SizeOf = IntroBase.BaseSizeOf + (3 * Config.TargetWordSize);

        /// <summary>
        /// Initializes a new instance of the <see cref="IntroStartAmmo"/> class.
        /// </summary>
        public IntroStartAmmo()
            : base(IntroType.StartAmmo)
        {
        }

        /// <summary>
        /// Gets or sets ammo type.
        /// Struct offset 0x0.
        /// See <see cref="Game.Enums.AmmoType"/> for available types.
        /// </summary>
        public UInt32 AmmoType { get; set; }

        /// <summary>
        /// Gets or sets ammo type.
        /// Struct offset 0x4.
        /// See <see cref="Game.Enums.AmmoType"/> for available types.
        /// </summary>
        public UInt32 Quantity { get; set; }

        /// <summary>
        /// Index of ammo intro entry.
        /// </summary>
        public UInt32 Set { get; set; }

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
            BitUtility.Insert32Big(bytes, pos, (int)AmmoType);
            pos += Config.TargetPointerSize;

            BitUtility.Insert32Big(bytes, pos, (int)Quantity);
            pos += Config.TargetPointerSize;

            BitUtility.Insert32Big(bytes, pos, (int)Set);
            pos += Config.TargetPointerSize;

            var result = context.AssembleAppendBytes(bytes, Config.TargetWordSize);
            BaseDataOffset = result.DataStartAddress;
        }

        /// <inheritdoc />
        protected override void AppendToCInlineS32Array(StringBuilder sb)
        {
            base.AppendToCInlineS32Array(sb);

            sb.Append(", ");
            sb.Append(AmmoType);
            sb.Append(", ");
            sb.Append(Quantity);
            sb.Append(", ");
            sb.Append(Set);
        }
    }
}
