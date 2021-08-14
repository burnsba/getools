using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    /// <summary>
    /// Starting ammo definition.
    /// </summary>
    public class IntroStartAmmo : IntroBase
    {
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
            sb.Append(AmmoType);
            sb.Append(", ");
            sb.Append(Quantity);
            sb.Append(", ");
            sb.Append(Set);
        }
    }
}
