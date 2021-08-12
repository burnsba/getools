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
    }
}
