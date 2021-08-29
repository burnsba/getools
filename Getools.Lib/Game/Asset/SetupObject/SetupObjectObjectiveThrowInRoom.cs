using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for objective to throw item in room.
    /// </summary>
    public class SetupObjectObjectiveThrowInRoom : SetupObjectBase, ISetupObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectObjectiveThrowInRoom"/> class.
        /// </summary>
        public SetupObjectObjectiveThrowInRoom()
            : base(PropDef.ObjectiveDepositObjectInRoom)
        {
        }

        /// <summary>
        /// Weapon slot index
        /// </summary>
        public int WeaponSlotIndex { get; set; }

        /// <summary>
        /// Preset.
        /// </summary>
        public int Preset { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// </summary>
        public int Unknown08 { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// </summary>
        public int Unknown0c { get; set; }

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
            sb.Append(WeaponSlotIndex);
            sb.Append(", ");
            sb.Append(Preset);
            sb.Append(", ");
            sb.Append(Unknown08);
            sb.Append(", ");
            sb.Append(Unknown0c);
        }
    }
}
