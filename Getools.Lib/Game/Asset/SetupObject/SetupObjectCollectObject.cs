using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for "collect object" objective.
    /// </summary>
    public class SetupObjectCollectObject : SetupObjectBase, ISetupObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectCollectObject"/> class.
        /// </summary>
        public SetupObjectCollectObject()
            : base(PropDef.ObjectiveCollectObject)
        {
        }

        /// <summary>
        /// Object to collect.
        /// Struct offset 0x0.
        /// </summary>
        public uint ObjectId { get; set; }

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
            sb.Append(ObjectId);
        }
    }
}
