using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectGuard : SetupObjectBase, ISetupObject
    {
        public SetupObjectGuard()
            : base(Propdef.Guard)
        {
        }

        public ushort ObjectId { get; set; }
        public ushort Preset { get; set; }
        public ushort BodyId { get; set; }
        public ushort ActionPathAssignment { get; set; }
        public uint PresetToTrigger { get; set; }
        public ushort Unknown10 { get; set; }
        public ushort Health { get; set; }
        public ushort ReactionTime { get; set; }
        public ushort Head { get; set; }
        public uint PointerRuntimeData { get; set; }

        public override string ToCInlineS32Array(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.Append(prefix);
            AppendToCInlineS32Array(sb);

            return sb.ToString();
        }

        protected override void AppendToCInlineS32Array(StringBuilder sb)
        {
            base.AppendToCInlineS32Array(sb);

            sb.Append(", ");
            sb.AppendFormat(
                Config.CMacro_WordFromShorts_Format,
                ObjectId,
                Preset);
            sb.Append(", ");
            sb.AppendFormat(
                Config.CMacro_WordFromShorts_Format,
                BodyId,
                ActionPathAssignment);
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(PresetToTrigger));
            sb.Append(", ");
            sb.AppendFormat(
                Config.CMacro_WordFromShorts_Format,
                Unknown10,
                Health);
            sb.Append(", ");
            sb.AppendFormat(
                Config.CMacro_WordFromShorts_Format,
                ReactionTime,
                Formatters.IntegralTypes.ToHex4(Head));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(PointerRuntimeData));
        }
    }
}
