using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for locked door.
    /// </summary>
    public class SetupObjectLock : SetupObjectBase, ISetupObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectLock"/> class.
        /// </summary>
        public SetupObjectLock()
            : base(PropDef.LockDoor)
        {
        }

        /// <summary>
        /// Door.
        /// </summary>
        public int Door { get; set; }

        /// <summary>
        /// Lock.
        /// </summary>
        public int Lock { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// Maybe end of record marker.
        /// </summary>
        public int Empty { get; set; }

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
            sb.Append(Door);
            sb.Append(", ");
            sb.Append(Lock);
            sb.Append(", ");
            sb.Append(Empty);
        }
    }
}
