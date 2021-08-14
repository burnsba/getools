using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for tag.
    /// </summary>
    public class SetupObjectTag : SetupObjectBase, ISetupObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectTag"/> class.
        /// </summary>
        public SetupObjectTag()
            : base(PropDef.Tag)
        {
        }

        /// <summary>
        /// TODO: Unknown.
        /// Struct offset 0x0.
        /// </summary>
        public ushort TagId { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// Struct offset 0x2.
        /// </summary>
        public ushort Value { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// Struct offset 0x4.
        /// </summary>
        public uint Unknown_04 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// Struct offset 0x8.
        /// </summary>
        public uint Unknown_08 { get; set; }

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
            sb.AppendFormat(
                Config.CMacro_WordFromShorts_Format,
                TagId.ToString(),
                Formatters.IntegralTypes.ToHex4(Value));
            sb.Append(", ");
            sb.Append(Unknown_04);
            sb.Append(", ");
            sb.Append(Unknown_08);
        }
    }
}
