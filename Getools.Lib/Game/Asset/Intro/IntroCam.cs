using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    /// <summary>
    /// Used by Bond during the intro swirl.
    /// </summary>
    /// <remarks>
    /// Discord comment:
    /// > Animation is used by Bond during the intro swirl process. It's actually a bunch
    /// > of preset values for each animation. To get around this limitations, some setups
    /// > such as Silo will instead not set an animation used for the intro swirl, but
    /// > will have a dedicated AI list that'll just watch for when the intro swirl has
    /// > triggered, and then trigger an animation that isn't selectable by the intro
    /// > block animation's range. Sometimes this is used because the intro swirl is
    /// > quite long, so some manual timing is required via AI list.
    /// - carnivorous Aug 14, 2021
    /// </remarks>
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
