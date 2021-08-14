using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    /// <summary>
    /// Bond's cuffs.
    /// </summary>
    public class IntroCuff : IntroBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntroCuff"/> class.
        /// </summary>
        public IntroCuff()
            : base(IntroType.Cuff)
        {
        }

        /// <summary>
        /// Gets or sets cuffs.
        /// See <see cref="Game.Enums.CuffType"/>.
        /// </summary>
        public UInt32 Cuff { get; set; }

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
            sb.Append(Cuff);
        }
    }
}
