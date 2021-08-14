using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for cutscene.
    /// </summary>
    public class SetupObjectCutscene : SetupObjectBase, ISetupObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectCutscene"/> class.
        /// </summary>
        public SetupObjectCutscene()
            : base(PropDef.Cutscene)
        {
        }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x0.
        /// </summary>
        public int XCoord { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x4.
        /// </summary>
        public int YCoord { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x8.
        /// </summary>
        public int ZCoord { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0xc.
        /// </summary>
        public int LatRot { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x10.
        /// </summary>
        public int VertRot { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x14.
        /// </summary>
        public uint IllumPreset { get; set; }

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
            sb.Append(XCoord);
            sb.Append(", ");
            sb.Append(YCoord);
            sb.Append(", ");
            sb.Append(ZCoord);
            sb.Append(", ");
            sb.Append(LatRot);
            sb.Append(", ");
            sb.Append(VertRot);
            sb.Append(", ");
            sb.Append(IllumPreset);
        }
    }
}
