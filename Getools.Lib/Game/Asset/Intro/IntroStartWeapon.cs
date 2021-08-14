using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    /// <summary>
    /// Specifies start weapon for either or both hands.
    /// </summary>
    public class IntroStartWeapon : IntroBase
    {
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
            sb.Append(Right);
            sb.Append(", ");
            sb.Append(Left);
            sb.Append(", ");
            sb.Append(SetNum);
        }
    }
}
