using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    /// <summary>
    /// Credits.
    /// </summary>
    public class IntroCredits : IntroBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntroCredits"/> class.
        /// </summary>
        public IntroCredits()
            : base(IntroType.Credits)
        {
        }

        /// <summary>
        /// Pointer to credits data.
        /// </summary>
        public int DataOffset { get; set; }

        /// <summary>
        /// Credits data.
        /// </summary>
        public CreditsContainer Credits { get; set; }

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

            if (!object.ReferenceEquals(null, Credits))
            {
                sb.Append(", ");
                sb.Append(Formatters.Strings.ToCPointerOrNull(Credits.VariableName));
            }
            else
            {
                sb.Append(", ");
                sb.Append(DataOffset);
            }
        }
    }
}
