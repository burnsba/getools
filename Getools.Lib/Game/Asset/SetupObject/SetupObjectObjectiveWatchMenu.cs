using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for watch menu text definition.
    /// </summary>
    public class SetupObjectObjectiveWatchMenu : SetupObjectBase, ISetupObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectObjectiveWatchMenu"/> class.
        /// </summary>
        public SetupObjectObjectiveWatchMenu()
            : base(PropDef.WatchMenuObjectiveText)
        {
        }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x0.
        /// </summary>
        public int MenuOption { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x4.
        /// </summary>
        public int TextId { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x8.
        /// </summary>
        public int End { get; set; }

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
            sb.Append(MenuOption);
            sb.Append(", ");
            sb.Append(TextId);
            sb.Append(", ");
            sb.Append(End);
        }
    }
}
