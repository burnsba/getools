using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectDoor : SetupObjectGenericBase
    {
        public SetupObjectDoor()
            : base(Propdef.Door)
        {
        }

        public UInt32 LinkedDoorOffset { get; set; }
        public UInt32 MaxFrac { get; set; }
        public UInt32 PerimFrac { get; set; }
        public UInt32 Accel { get; set; }
        public UInt32 Decel { get; set; }
        public UInt32 MaxSpeed { get; set; }
        public UInt16 DoorFlags { get; set; }
        public UInt16 DoorType { get; set; }
        public UInt32 KeyFlags { get; set; }
        public UInt32 AutoCloseFrames { get; set; }
        public UInt32 DoorOpenSound { get; set; }
        public UInt32 Frac { get; set; }
        public UInt32 UnknownAc { get; set; }
        public UInt32 UnknownB0 { get; set; }
        public float OpenPosition { get; set; }
        public float Speed { get; set; }
        public byte State { get; set; }
        public byte UnknownBd { get; set; }
        public UInt16 UnknownBe { get; set; }
        public UInt32 UnknownC0 { get; set; }
        public UInt16 UnknownC4 { get; set; }
        public byte SoundType { get; set; }
        public byte FadeTime60 { get; set; }
        public UInt32 LinkedDoorPointer { get; set; }
        public byte LaserFade { get; set; }
        public byte UnknownCd { get; set; }
        public UInt16 UnknownCe { get; set; }
        public UInt32 UnknownD0 { get; set; }
        public UInt32 UnknownD4 { get; set; }
        public UInt32 UnknownD8 { get; set; }
        public UInt32 UnknownDc { get; set; }
        public UInt32 UnknownE0 { get; set; }
        public UInt32 UnknownE4 { get; set; }
        public UInt32 UnknownE8 { get; set; }
        public UInt32 OpenedTime { get; set; }
        public UInt32 PortalNumber { get; set; }
        public UInt32 UnknownF4Pointer { get; set; }
        public UInt32 UnknownF8 { get; set; }
        public UInt32 Timer { get; set; }

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
            sb.Append(Formatters.IntegralTypes.ToHex8(LinkedDoorOffset));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(MaxFrac));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(PerimFrac));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(Accel));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(Decel));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(MaxSpeed));
            sb.Append(", ");
            sb.Append(Config.CMacro_WordFromShortShort(DoorFlags, DoorType));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(KeyFlags));
            sb.Append(", ");
            sb.Append(AutoCloseFrames);
            sb.Append(", ");
            sb.Append(DoorOpenSound);
            sb.Append(", ");
            sb.Append(Formatters.FloatingPoint.ToFloatCLiteral(Frac));
            sb.Append(", ");
            sb.Append(UnknownAc);
            sb.Append(", ");
            sb.Append(UnknownB0);
            sb.Append(", ");
            sb.Append(OpenPosition);
            sb.Append(", ");
            sb.Append(Formatters.FloatingPoint.ToFloatCLiteral(Speed));
            sb.Append(", ");
            sb.Append(Config.CMacro_WordFromShortShort(UnknownBd, UnknownBe));
            sb.Append(", ");
            sb.Append(UnknownC0);
            sb.Append(", ");
            sb.Append(Config.CMacro_WordFromShortByteByte(UnknownC4, SoundType, FadeTime60));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(LinkedDoorPointer));
            sb.Append(", ");
            sb.Append(Config.CMacro_WordFromByteByteShort(LaserFade, UnknownCd, UnknownCe));
            sb.Append(", ");
            sb.Append(UnknownD0);
            sb.Append(", ");
            sb.Append(UnknownD4);
            sb.Append(", ");
            sb.Append(UnknownD8);
            sb.Append(", ");
            sb.Append(UnknownDc);
            sb.Append(", ");
            sb.Append(UnknownE0);
            sb.Append(", ");
            sb.Append(UnknownE4);
            sb.Append(", ");
            sb.Append(UnknownE8);
            sb.Append(", ");
            sb.Append(OpenedTime);
            sb.Append(", ");
            sb.Append(PortalNumber);
            sb.Append(", ");
            sb.Append(UnknownF4Pointer);
            sb.Append(", ");
            sb.Append(UnknownF8);
            sb.Append(", ");
            sb.Append(Timer);
        }
    }
}
