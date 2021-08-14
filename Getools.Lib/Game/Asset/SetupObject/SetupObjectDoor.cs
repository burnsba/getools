using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for door.
    /// Values are transformed at stage load.
    /// </summary>
    public class SetupObjectDoor : SetupObjectGenericBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectDoor"/> class.
        /// </summary>
        public SetupObjectDoor()
            : base(PropDef.Door)
        {
        }

        /// <summary>
        /// TODO: unknown.
        /// Offset 0x84.
        /// </summary>
        public UInt32 LinkedDoorOffset { get; set; }

        /// <summary>
        /// Distance door travels when opening/closing (total%).
        /// Converted to float on load (divide by <see cref="UInt16.MaxValue"/>).
        /// Offset 0x84.
        /// </summary>
        public UInt32 MaxFrac { get; set; }

        /// <summary>
        /// Distance before door loses collisions (total%) (vertical height % until Bond can walk through).
        /// Converted to float on load (divide by <see cref="UInt16.MaxValue"/>).
        /// Offset 0x88.
        /// </summary>
        public UInt32 PerimFrac { get; set; }

        /// <summary>
        /// Start moving acceleration rate (when a door is first opened/closed).
        /// Converted to float on load (divide by <see cref="UInt16.MaxValue"/>).
        /// Offset 0x8c.
        /// </summary>
        public UInt32 Accel { get; set; }

        /// <summary>
        /// Start slowing down acceleration rate (when a door is almost entirely opened/closed).
        /// Converted to float on load (divide by <see cref="UInt16.MaxValue"/>).
        /// Offset 0x90.
        /// </summary>
        public UInt32 Decel { get; set; }

        /// <summary>
        /// Maximum speed door can move on each update.
        /// Converted to float on load (divide by <see cref="UInt16.MaxValue"/>).
        /// Offset 0x94.
        /// </summary>
        public UInt32 MaxSpeed { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// See <see cref="Game.Enums.DoorFlags"/>.
        /// Offset 0x98.
        /// </summary>
        public UInt16 DoorFlags { get; set; }

        /// <summary>
        /// Type of door.
        /// See <see cref="Game.Enums.DoorTypes"/>.
        /// Offset 0x9a.
        /// </summary>
        public UInt16 DoorType { get; set; }

        /// <summary>
        /// Nonzero to lock.
        /// Offset 0x9c.
        /// </summary>
        public UInt32 KeyFlags { get; set; }

        /// <summary>
        /// Number of frames the door remains open before closing.
        /// Offset 0xa0.
        /// </summary>
        public UInt32 AutoCloseFrames { get; set; }

        /// <summary>
        /// Sound effect played when the door is opened.
        /// (Actually sets the initial open, continued opening, and final open sounds).
        /// Offset 0xa4.
        /// </summary>
        public UInt32 DoorOpenSound { get; set; }

        /// <summary>
        /// Max fraction open, aka max displacement.
        /// Offset 0xa8.
        /// </summary>
        public UInt32 Frac { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Offset 0xac.
        /// </summary>
        public UInt32 UnknownAc { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Offset 0xb0.
        /// </summary>
        public UInt32 UnknownB0 { get; set; }

        /// <summary>
        /// Current (runtime) distance travelled, aka displacement percent.
        /// Offset 0xb4.
        /// </summary>
        public float OpenPosition { get; set; }

        /// <summary>
        /// Current (runtime) speed of the door as it is opening or closing.
        /// Offset 0xb8.
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// Current open/close/other state of the door.
        /// See <see cref="Game.Enums.DoorState"/>.
        /// Offset 0xbc.
        /// </summary>
        public byte State { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Offset 0xbd.
        /// </summary>
        public byte UnknownBd { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Offset 0xbe.
        /// </summary>
        public UInt16 UnknownBe { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Offset 0xc0.
        /// </summary>
        public UInt32 UnknownC0 { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Offset 0xc4.
        /// </summary>
        public UInt16 UnknownC4 { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Offset 0xc6.
        /// </summary>
        public byte SoundType { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Offset 0xc7.
        /// </summary>
        public byte FadeTime60 { get; set; }

        /// <summary>
        /// Connected door. Opening/closing this door will also open the linkedDoor.
        /// Offset 0xc8.
        /// </summary>
        public UInt32 LinkedDoorPointer { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Offset 0xcc.
        /// </summary>
        public byte LaserFade { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Offset 0xcd.
        /// </summary>
        public byte UnknownCd { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Offset 0xce.
        /// </summary>
        public UInt16 UnknownCe { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Offset 0xd0.
        /// </summary>
        public UInt32 UnknownD0 { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Offset 0xd4.
        /// </summary>
        public UInt32 UnknownD4 { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Offset 0xd8.
        /// </summary>
        public UInt32 UnknownD8 { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Offset 0xdc.
        /// </summary>
        public UInt32 UnknownDc { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Offset 0xe0.
        /// </summary>
        public UInt32 UnknownE0 { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Offset 0xe4.
        /// </summary>
        public UInt32 UnknownE4 { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Offset 0xe8.
        /// </summary>
        public UInt32 UnknownE8 { get; set; }

        /// <summary>
        /// When the door completely opens, the current (runtime) global timer value is
        /// copied into this property.Once autoCloseFrames have elapsed
        /// (once the difference between the timer and this value has exceeded autoCloseFrames)
        /// the door will start closing.
        /// Offset 0xec.
        /// </summary>
        public UInt32 OpenedTime { get; set; }

        /// <summary>
        /// Portal number.
        /// Offset 0xf0.
        /// </summary>
        public UInt32 PortalNumber { get; set; }

        /// <summary>
        /// TODO: Unknown. Changes at runtime. Appears to be set to a pointer
        /// while the door is moving, then cleared when the door is stationary.
        /// If you reset this to 0 (NULL pointer), then the door opening
        /// sound never stops playing.
        /// Offset 0xf4.
        /// </summary>
        public UInt32 UnknownF4Pointer { get; set; }

        /// <summary>
        /// Portal Unknown.
        /// Offset 0xf8.
        /// </summary>
        public UInt32 UnknownF8 { get; set; }

        /// <summary>
        /// Copy of global timer value.
        /// Offset 0xfc.
        /// </summary>
        public UInt32 Timer { get; set; }

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
