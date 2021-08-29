using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for objective enter room.
    /// </summary>
    public class SetupObjectObjectiveEnterRoom : SetupObjectBase, ISetupObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectObjectiveEnterRoom"/> class.
        /// </summary>
        public SetupObjectObjectiveEnterRoom()
            : base(PropDef.ObjectiveEnterRoom)
        {
        }

        /// <summary>
        /// Room
        /// </summary>
        public int Room { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// </summary>
        public int Unknown04 { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// </summary>
        public int Unknown08 { get; set; }

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
            sb.Append(Room);
            sb.Append(", ");
            sb.Append(Unknown04);
            sb.Append(", ");
            sb.Append(Unknown08);
        }
    }
}
