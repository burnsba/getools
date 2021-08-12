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
        public float MaxFrac { get; set; }
        public float PerimFrac { get; set; }
        public float Accel { get; set; }
        public float Decel { get; set; }
        public float MaxSpeed { get; set; }
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
    }
}
