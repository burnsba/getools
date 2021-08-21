using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list objective to copy item.
    /// </summary>
    public class SetupObjectiveCopyItem : SetupObjectBase, ISetupObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectiveCopyItem"/> class.
        /// </summary>
        public SetupObjectiveCopyItem()
            : base(PropDef.ObjectiveCopy_Item)
        {
        }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public uint TagId { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// Struct offset 0x4.
        /// </summary>
        public ushort Unknown_04 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// Struct offset 0x6.
        /// </summary>
        public ushort Unknown_06 { get; set; }

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
            sb.Append(TagId);
            sb.Append(", ");
            sb.AppendFormat(
                Config.CMacro_WordFromShorts_Format,
                Formatters.IntegralTypes.ToHex4(Unknown_04),
                Formatters.IntegralTypes.ToHex4(Unknown_06));
        }
    }
}
