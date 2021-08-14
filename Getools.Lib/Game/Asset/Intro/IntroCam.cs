using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    /// <summary>
    /// Intro cam (first cinema).
    /// </summary>
    public class IntroCam : IntroBase
    {
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
            sb.Append(Animation);
        }
    }
}
