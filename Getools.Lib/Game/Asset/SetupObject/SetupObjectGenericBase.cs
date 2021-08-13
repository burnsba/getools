using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public abstract class SetupObjectGenericBase : SetupObjectBase
    {
        public SetupObjectGenericBase()
        {
        }

        public SetupObjectGenericBase(Propdef type)
            : base(type)
        {
        }

        public UInt16 ObjectId { get; set; }
        public UInt16 Preset { get; set; }
        public UInt32 Flags1 { get; set; }
        public UInt32 Flags2 { get; set; }
        public UInt32 PointerPositionData { get; set; }
        public UInt32 PointerObjInstanceController { get; set; }
        public UInt32 Unknown18 { get; set; }
        public UInt32 Unknown1c { get; set; }
        public UInt32 Unknown20 { get; set; }
        public UInt32 Unknown24 { get; set; }
        public UInt32 Unknown28 { get; set; }
        public UInt32 Unknown2c { get; set; }
        public UInt32 Unknown30 { get; set; }
        public UInt32 Unknown34 { get; set; }
        public UInt32 Unknown38 { get; set; }
        public UInt32 Unknown3c { get; set; }
        public UInt32 Unknown40 { get; set; }
        public UInt32 Unknown44 { get; set; }
        public UInt32 Unknown48 { get; set; }
        public UInt32 Unknown4c { get; set; }
        public UInt32 Unknown50 { get; set; }
        public UInt32 Unknown54 { get; set; }
        public UInt32 Xpos { get; set; }
        public UInt32 Ypos { get; set; }
        public UInt32 Zpos { get; set; }
        public UInt32 Bitflags { get; set; }
        public UInt32 PointerCollisionBlock { get; set; }
        public UInt32 Unknown6c { get; set; }
        public UInt32 Unknown70 { get; set; }
        public UInt16 Health { get; set; }
        public UInt16 MaxHealth { get; set; }
        public UInt32 Unknown78 { get; set; }
        public UInt32 Unknown7c { get; set; }

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
            sb.AppendFormat(Config.CMacro_WordFromShorts_Format, ObjectId, Preset);
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(Flags1));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(Flags2));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(PointerPositionData));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(PointerObjInstanceController));
            sb.Append(", ");
            sb.Append(Unknown18);
            sb.Append(", ");
            sb.Append(Unknown1c);
            sb.Append(", ");
            sb.Append(Unknown20);
            sb.Append(", ");
            sb.Append(Unknown24);
            sb.Append(", ");
            sb.Append(Unknown28);
            sb.Append(", ");
            sb.Append(Unknown2c);
            sb.Append(", ");
            sb.Append(Unknown30);
            sb.Append(", ");
            sb.Append(Unknown34);
            sb.Append(", ");
            sb.Append(Unknown38);
            sb.Append(", ");
            sb.Append(Unknown3c);
            sb.Append(", ");
            sb.Append(Unknown40);
            sb.Append(", ");
            sb.Append(Unknown44);
            sb.Append(", ");
            sb.Append(Unknown48);
            sb.Append(", ");
            sb.Append(Unknown4c);
            sb.Append(", ");
            sb.Append(Unknown50);
            sb.Append(", ");
            sb.Append(Unknown54);
            sb.Append(", ");
            sb.Append(Xpos);
            sb.Append(", ");
            sb.Append(Ypos);
            sb.Append(", ");
            sb.Append(Zpos);
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(Bitflags));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(PointerCollisionBlock));
            sb.Append(", ");
            sb.Append(Unknown6c);
            sb.Append(", ");
            sb.Append(Unknown70);
            sb.Append(", ");
            sb.AppendFormat(Config.CMacro_WordFromShorts_Format, Health, MaxHealth);
            sb.Append(", ");
            sb.Append(Unknown78);
            sb.Append(", ");
            sb.Append(Unknown7c);
        }
    }
}
