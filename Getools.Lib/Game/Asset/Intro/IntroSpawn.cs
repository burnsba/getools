using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    /// <summary>
    /// Intro spawn definition.
    /// </summary>
    public class IntroSpawn : IntroBase
    {
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
            sb.Append(Unknown_00);
            sb.Append(", ");
            sb.Append(Unknown_04);
        }
    }
}
