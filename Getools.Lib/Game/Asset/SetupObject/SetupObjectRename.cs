using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for renaming an inventory item.
    /// </summary>
    public class SetupObjectRename : SetupObjectBase, ISetupObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectRename"/> class.
        /// </summary>
        public SetupObjectRename()
            : base(PropDef.Rename)
        {
        }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x0.
        /// </summary>
        public uint ObjectOffset { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x4.
        /// </summary>
        public uint InventoryId { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x8.
        /// </summary>
        public uint Text1 { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0xc.
        /// </summary>
        public uint Text2 { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x10.
        /// </summary>
        public uint Text3 { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x14.
        /// </summary>
        public uint Text4 { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x18.
        /// </summary>
        public uint Text5 { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x1c.
        /// </summary>
        public uint Unknown1c { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x20.
        /// </summary>
        public uint Unknown20 { get; set; }

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
            sb.Append(Formatters.IntegralTypes.ToHex8(ObjectOffset));
            sb.Append(", ");
            sb.Append(InventoryId);
            sb.Append(", ");
            sb.Append(Text1);
            sb.Append(", ");
            sb.Append(Text2);
            sb.Append(", ");
            sb.Append(Text3);
            sb.Append(", ");
            sb.Append(Text4);
            sb.Append(", ");
            sb.Append(Text5);
            sb.Append(", ");
            sb.Append(Unknown1c);
            sb.Append(", ");
            sb.Append(Unknown20);
        }
    }
}
