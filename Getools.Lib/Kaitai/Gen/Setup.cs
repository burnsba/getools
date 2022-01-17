// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

/*
 * Changes to auto generated code:
 *
 * - Add pragmas.
 * - Modify SectionBlock._read, needs to use if statements since can't switch on variable
 * ---- public partial class SectionBlock : KaitaiStruct
 */
#pragma warning disable SA1003
#pragma warning disable SA1008
#pragma warning disable SA1009
#pragma warning disable SA1119
#pragma warning disable SA1128
#pragma warning disable SA1201
#pragma warning disable SA1300
#pragma warning disable SA1302
#pragma warning disable SA1308
#pragma warning disable SA1312
#pragma warning disable SA1500
#pragma warning disable SA1502
#pragma warning disable SA1503
#pragma warning disable SA1513
#pragma warning disable SA1515
#pragma warning disable SA1516
#pragma warning disable SA1600
#pragma warning disable SA1601
#pragma warning disable SA1602
#pragma warning disable SA1507
// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using System.Collections.Generic;
using Kaitai;

namespace Getools.Lib.Kaitai.Gen
{
    public partial class Setup : KaitaiStruct
    {
        public static Setup FromFile(string fileName)
        {
            return new Setup(new KaitaiStream(fileName));
        }


        public enum SectionId
        {
            PadSection = 0,
            Pad3dSection = 10,
            ObjectSection = 20,
            IntroSection = 30,
            SetsPrequel = 38,
            PathLinksSection = 40,
            Pad3dNames = 50,
            PathTableSection = 60,
            PathSetsSection = 70,
            PadNames = 80,
            AiListSection = 90,
        }

        public enum Propdef
        {
            Nothing = 0,
            Door = 1,
            DoorScale = 2,
            Standard = 3,
            Key = 4,
            Alarm = 5,
            Cctv = 6,
            AmmoMag = 7,
            Weapon = 8,
            Guard = 9,
            SingleMonitor = 10,
            MultiMonitor = 11,
            HangingMonitor = 12,
            Autogun = 13,
            LinkItems = 14,
            Hat = 17,
            SetGuardAttribute = 18,
            LinkProps = 19,
            AmmoBox = 20,
            BodyArmor = 21,
            Tag = 22,
            ObjectiveStart = 23,
            EndObjective = 24,
            DestroyObject = 25,
            ObjectiveCompleteCondition = 26,
            ObjectiveFailCondition = 27,
            CollectObject = 28,
            ObjectivePhotographItem = 30,
            ObjectiveEnterRoom = 32,
            ObjectiveThrowInRoom = 33,
            ObjectiveCopyItem = 34,
            WatchMenuObjectiveText = 35,
            GasProp = 36,
            Rename = 37,
            Lock = 38,
            Vehicle = 39,
            Aircraft = 40,
            Glass = 42,
            Safe = 43,
            SafeItem = 44,
            Tank = 45,
            Cutscene = 46,
            GlassTinted = 47,
            EndProps = 48,
        }

        public enum Introdef
        {
            Spawn = 0,
            StartWeapon = 1,
            StartAmmo = 2,
            SwirlCam = 3,
            IntroCam = 4,
            Cuff = 5,
            FixedCam = 6,
            WatchTime = 7,
            Credits = 8,
            EndIntro = 9,
        }

        public enum IntroCreditsAlignment
        {
            Right = 0,
            Left = 1,
            Center = 2,
            Previous = 65535,
        }
        public Setup(KaitaiStream p__io, KaitaiStruct p__parent = null, Setup p__root = null) : base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root ?? this;
            _read();
        }
        private void _read()
        {
            _pointers = new StageSetupStruct(m_io, this, m_root);
            _contents = new List<SectionBlock>();
            {
                var i = 0;
                while (!m_io.IsEof)
                {
                    _contents.Add(new SectionBlock(m_io, this, m_root));
                    i++;
                }
            }
        }
        public partial class SetupIntroSwirlCamBody : KaitaiStruct
        {
            public static SetupIntroSwirlCamBody FromFile(string fileName)
            {
                return new SetupIntroSwirlCamBody(new KaitaiStream(fileName));
            }

            public SetupIntroSwirlCamBody(KaitaiStream p__io, Setup.SetupIntroRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _unknown00 = m_io.ReadU4be();
                _x = m_io.ReadU4be();
                _y = m_io.ReadU4be();
                _z = m_io.ReadU4be();
                _splineScale = m_io.ReadU4be();
                _duration = m_io.ReadU4be();
                _flags = m_io.ReadU4be();
            }
            private uint _unknown00;
            private uint _x;
            private uint _y;
            private uint _z;
            private uint _splineScale;
            private uint _duration;
            private uint _flags;
            private Setup m_root;
            private Setup.SetupIntroRecord m_parent;
            public uint Unknown00 { get { return _unknown00; } }
            public uint X { get { return _x; } }
            public uint Y { get { return _y; } }
            public uint Z { get { return _z; } }
            public uint SplineScale { get { return _splineScale; } }
            public uint Duration { get { return _duration; } }
            public uint Flags { get { return _flags; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupIntroRecord M_Parent { get { return m_parent; } }
        }
        public partial class ObjectList : KaitaiStruct
        {
            public static ObjectList FromFile(string fileName)
            {
                return new ObjectList(new KaitaiStream(fileName));
            }

            public ObjectList(KaitaiStream p__io, Setup.SectionBlock p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_type = false;
                _read();
            }
            private void _read()
            {
                _data = new List<SetupObjectRecord>();
                {
                    var i = 0;
                    SetupObjectRecord M_;
                    do
                    {
                        M_ = new SetupObjectRecord(m_io, this, m_root);
                        _data.Add(M_);
                        i++;
                    } while (!(M_.Header.Type == Setup.Propdef.EndProps));
                }
            }
            private bool f_type;
            private SectionId _type;
            public SectionId Type
            {
                get
                {
                    if (f_type)
                        return _type;
                    _type = (SectionId)(Setup.SectionId.ObjectSection);
                    f_type = true;
                    return _type;
                }
            }
            private List<SetupObjectRecord> _data;
            private Setup m_root;
            private Setup.SectionBlock m_parent;
            public List<SetupObjectRecord> Data { get { return _data; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SectionBlock M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectStandardBody : KaitaiStruct
        {
            public static SetupObjectStandardBody FromFile(string fileName)
            {
                return new SetupObjectStandardBody(new KaitaiStream(fileName));
            }

            public SetupObjectStandardBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectBase = new SetupGenericObject(m_io, this, m_root);
            }
            private SetupGenericObject _objectBase;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public SetupGenericObject ObjectBase { get { return _objectBase; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectVehicleBody : KaitaiStruct
        {
            public static SetupObjectVehicleBody FromFile(string fileName)
            {
                return new SetupObjectVehicleBody(new KaitaiStream(fileName));
            }

            public SetupObjectVehicleBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectBase = new SetupGenericObject(m_io, this, m_root);
                _bytes = m_io.ReadBytes(48);
            }
            private SetupGenericObject _objectBase;
            private byte[] _bytes;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public SetupGenericObject ObjectBase { get { return _objectBase; } }
            public byte[] Bytes { get { return _bytes; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class Pad3dList : KaitaiStruct
        {
            public static Pad3dList FromFile(string fileName)
            {
                return new Pad3dList(new KaitaiStream(fileName));
            }

            public Pad3dList(KaitaiStream p__io, Setup.SectionBlock p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_type = false;
                _read();
            }
            private void _read()
            {
                _data = new List<Pad3d>();
                {
                    var i = 0;
                    Pad3d M_;
                    do
                    {
                        M_ = new Pad3d(m_io, this, m_root);
                        _data.Add(M_);
                        i++;
                    } while (!(M_.Plink.Offset == 0));
                }
            }
            private bool f_type;
            private SectionId _type;
            public SectionId Type
            {
                get
                {
                    if (f_type)
                        return _type;
                    _type = (SectionId)(Setup.SectionId.Pad3dSection);
                    f_type = true;
                    return _type;
                }
            }
            private List<Pad3d> _data;
            private Setup m_root;
            private Setup.SectionBlock m_parent;
            public List<Pad3d> Data { get { return _data; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SectionBlock M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectKeyBody : KaitaiStruct
        {
            public static SetupObjectKeyBody FromFile(string fileName)
            {
                return new SetupObjectKeyBody(new KaitaiStream(fileName));
            }

            public SetupObjectKeyBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectBase = new SetupGenericObject(m_io, this, m_root);
                _key = m_io.ReadU4be();
            }
            private SetupGenericObject _objectBase;
            private uint _key;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public SetupGenericObject ObjectBase { get { return _objectBase; } }
            public uint Key { get { return _key; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectHatBody : KaitaiStruct
        {
            public static SetupObjectHatBody FromFile(string fileName)
            {
                return new SetupObjectHatBody(new KaitaiStream(fileName));
            }

            public SetupObjectHatBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectBase = new SetupGenericObject(m_io, this, m_root);
            }
            private SetupGenericObject _objectBase;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public SetupGenericObject ObjectBase { get { return _objectBase; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SectionBlock : KaitaiStruct
        {
            public static SectionBlock FromFile(string fileName)
            {
                return new SectionBlock(new KaitaiStream(fileName));
            }

            public SectionBlock(KaitaiStream p__io, Setup p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                if (M_Root.M_Io.Pos == M_Root.Pointers.ObjectListOffset)
                    _body = new ObjectList(m_io, this, m_root);
                else if (M_Root.M_Io.Pos == M_Root.Pointers.Pad3dNamesOffset)
                    _body = new Pad3dNamesList(m_io, this, m_root);
                else if (M_Root.M_Io.Pos == M_Root.Pointers.AiListOffset)
                    _body = new AiList(m_io, this, m_root);
                else if (M_Root.M_Io.Pos == M_Root.Pointers.Pad3dListOffset)
                    _body = new Pad3dList(m_io, this, m_root);
                else if (M_Root.M_Io.Pos == M_Root.Pointers.IntrosOffset)
                    _body = new IntroList(m_io, this, m_root);
                else if (M_Root.M_Io.Pos == M_Root.Pointers.PathLinksOffset)
                    _body = new PathLinksSection(m_io, this, m_root);
                else if (M_Root.M_Io.Pos == M_Root.Pointers.PadNamesOffset)
                    _body = new PadNamesList(m_io, this, m_root);
                else if (M_Root.M_Io.Pos == M_Root.Pointers.PadListOffset)
                    _body = new PadList(m_io, this, m_root);
                else if (M_Root.M_Io.Pos == M_Root.Pointers.PathSetsOffset)
                    _body = new PathSetsSection(m_io, this, m_root);
                else if (M_Root.M_Io.Pos == M_Root.Pointers.PathTablesOffset)
                    _body = new PathTableSection(m_io, this, m_root);
                else
                    _body = new FillerBlock((uint)M_Root.M_Io.Pos, m_io, this, m_root);
            }
            private KaitaiStruct _body;
            private Setup m_root;
            private Setup m_parent;
            public KaitaiStruct Body { get { return _body; } }
            public Setup M_Root { get { return m_root; } }
            public Setup M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectAmmoMagBody : KaitaiStruct
        {
            public static SetupObjectAmmoMagBody FromFile(string fileName)
            {
                return new SetupObjectAmmoMagBody(new KaitaiStream(fileName));
            }

            public SetupObjectAmmoMagBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectBase = new SetupGenericObject(m_io, this, m_root);
                _ammoType = m_io.ReadS4be();
            }
            private SetupGenericObject _objectBase;
            private int _ammoType;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public SetupGenericObject ObjectBase { get { return _objectBase; } }
            public int AmmoType { get { return _ammoType; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectSetGuardAttributeBody : KaitaiStruct
        {
            public static SetupObjectSetGuardAttributeBody FromFile(string fileName)
            {
                return new SetupObjectSetGuardAttributeBody(new KaitaiStream(fileName));
            }

            public SetupObjectSetGuardAttributeBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _guardId = m_io.ReadU4be();
                _attribute = m_io.ReadU4be();
            }
            private uint _guardId;
            private uint _attribute;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public uint GuardId { get { return _guardId; } }
            public uint Attribute { get { return _attribute; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class Bbox : KaitaiStruct
        {
            public static Bbox FromFile(string fileName)
            {
                return new Bbox(new KaitaiStream(fileName));
            }

            public Bbox(KaitaiStream p__io, Setup.Pad3d p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _xmin = m_io.ReadF4be();
                _xmax = m_io.ReadF4be();
                _ymin = m_io.ReadF4be();
                _ymax = m_io.ReadF4be();
                _zmin = m_io.ReadF4be();
                _zmax = m_io.ReadF4be();
            }
            private float _xmin;
            private float _xmax;
            private float _ymin;
            private float _ymax;
            private float _zmin;
            private float _zmax;
            private Setup m_root;
            private Setup.Pad3d m_parent;
            public float Xmin { get { return _xmin; } }
            public float Xmax { get { return _xmax; } }
            public float Ymin { get { return _ymin; } }
            public float Ymax { get { return _ymax; } }
            public float Zmin { get { return _zmin; } }
            public float Zmax { get { return _zmax; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.Pad3d M_Parent { get { return m_parent; } }
        }
        public partial class IntroCreditEntry : KaitaiStruct
        {
            public static IntroCreditEntry FromFile(string fileName)
            {
                return new IntroCreditEntry(new KaitaiStream(fileName));
            }

            public IntroCreditEntry(KaitaiStream p__io, Setup.SetupIntroCreditsBody p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _textId1 = m_io.ReadU2be();
                _textId2 = m_io.ReadU2be();
                _textPosition1 = m_io.ReadS2be();
                _textAlignment1 = ((Setup.IntroCreditsAlignment)m_io.ReadU2be());
                _textPosition2 = m_io.ReadS2be();
                _textAlignment2 = ((Setup.IntroCreditsAlignment)m_io.ReadU2be());
            }
            private ushort _textId1;
            private ushort _textId2;
            private short _textPosition1;
            private IntroCreditsAlignment _textAlignment1;
            private short _textPosition2;
            private IntroCreditsAlignment _textAlignment2;
            private Setup m_root;
            private Setup.SetupIntroCreditsBody m_parent;
            public ushort TextId1 { get { return _textId1; } }
            public ushort TextId2 { get { return _textId2; } }
            public short TextPosition1 { get { return _textPosition1; } }
            public IntroCreditsAlignment TextAlignment1 { get { return _textAlignment1; } }
            public short TextPosition2 { get { return _textPosition2; } }
            public IntroCreditsAlignment TextAlignment2 { get { return _textAlignment2; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupIntroCreditsBody M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectivePhotographItemBody : KaitaiStruct
        {
            public static SetupObjectivePhotographItemBody FromFile(string fileName)
            {
                return new SetupObjectivePhotographItemBody(new KaitaiStream(fileName));
            }

            public SetupObjectivePhotographItemBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectTagId = m_io.ReadU4be();
                _unknown04 = m_io.ReadU4be();
                _unknown08 = m_io.ReadU4be();
            }
            private uint _objectTagId;
            private uint _unknown04;
            private uint _unknown08;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public uint ObjectTagId { get { return _objectTagId; } }
            public uint Unknown04 { get { return _unknown04; } }
            public uint Unknown08 { get { return _unknown08; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class PathTableSection : KaitaiStruct
        {
            public static PathTableSection FromFile(string fileName)
            {
                return new PathTableSection(new KaitaiStream(fileName));
            }

            public PathTableSection(KaitaiStream p__io, Setup.SectionBlock p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_type = false;
                _read();
            }
            private void _read()
            {
                _data = new List<PathTableEntry>();
                {
                    var i = 0;
                    PathTableEntry M_;
                    do
                    {
                        M_ = new PathTableEntry(m_io, this, m_root);
                        _data.Add(M_);
                        i++;
                    } while (!(((M_.PadId == System.UInt32.MaxValue))));
                }
            }
            private bool f_type;
            private SectionId _type;
            public SectionId Type
            {
                get
                {
                    if (f_type)
                        return _type;
                    _type = (SectionId)(Setup.SectionId.PathTableSection);
                    f_type = true;
                    return _type;
                }
            }
            private List<PathTableEntry> _data;
            private Setup m_root;
            private Setup.SectionBlock m_parent;
            public List<PathTableEntry> Data { get { return _data; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SectionBlock M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectEndObjectiveBody : KaitaiStruct
        {
            public static SetupObjectEndObjectiveBody FromFile(string fileName)
            {
                return new SetupObjectEndObjectiveBody(new KaitaiStream(fileName));
            }

            public SetupObjectEndObjectiveBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _noValue = m_io.ReadBytes(0);
            }
            private byte[] _noValue;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public byte[] NoValue { get { return _noValue; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupIntroStartAmmoBody : KaitaiStruct
        {
            public static SetupIntroStartAmmoBody FromFile(string fileName)
            {
                return new SetupIntroStartAmmoBody(new KaitaiStream(fileName));
            }

            public SetupIntroStartAmmoBody(KaitaiStream p__io, Setup.SetupIntroRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _ammoType = m_io.ReadU4be();
                _quantity = m_io.ReadU4be();
                _set = m_io.ReadU4be();
            }
            private uint _ammoType;
            private uint _quantity;
            private uint _set;
            private Setup m_root;
            private Setup.SetupIntroRecord m_parent;
            public uint AmmoType { get { return _ammoType; } }
            public uint Quantity { get { return _quantity; } }
            public uint Set { get { return _set; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupIntroRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectSafeBody : KaitaiStruct
        {
            public static SetupObjectSafeBody FromFile(string fileName)
            {
                return new SetupObjectSafeBody(new KaitaiStream(fileName));
            }

            public SetupObjectSafeBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectBase = new SetupGenericObject(m_io, this, m_root);
            }
            private SetupGenericObject _objectBase;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public SetupGenericObject ObjectBase { get { return _objectBase; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectDoorBody : KaitaiStruct
        {
            public static SetupObjectDoorBody FromFile(string fileName)
            {
                return new SetupObjectDoorBody(new KaitaiStream(fileName));
            }

            public SetupObjectDoorBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectBase = new SetupGenericObject(m_io, this, m_root);
                _linkedDoorOffset = m_io.ReadU4be();
                _maxFrac = m_io.ReadU4be();
                _perimFrac = m_io.ReadU4be();
                _accel = m_io.ReadU4be();
                _decel = m_io.ReadU4be();
                _maxSpeed = m_io.ReadU4be();
                _doorFlags = m_io.ReadU2be();
                _doorType = m_io.ReadU2be();
                _keyFlags = m_io.ReadU4be();
                _autoCloseFrames = m_io.ReadU4be();
                _doorOpenSound = m_io.ReadU4be();
                _frac = m_io.ReadU4be();
                _unknownAc = m_io.ReadU4be();
                _unknownB0 = m_io.ReadU4be();
                _openPosition = m_io.ReadF4be();
                _speed = m_io.ReadF4be();
                _state = m_io.ReadU1();
                _unknownBd = m_io.ReadU1();
                _unknownBe = m_io.ReadU2be();
                _unknownC0 = m_io.ReadU4be();
                _unknownC4 = m_io.ReadU2be();
                _soundType = m_io.ReadU1();
                _fadeTime60 = m_io.ReadU1();
                _linkedDoorPointer = m_io.ReadU4be();
                _laserFade = m_io.ReadU1();
                _unknownCd = m_io.ReadU1();
                _unknownCe = m_io.ReadU2be();
                _unknownD0 = m_io.ReadU4be();
                _unknownD4 = m_io.ReadU4be();
                _unknownD8 = m_io.ReadU4be();
                _unknownDc = m_io.ReadU4be();
                _unknownE0 = m_io.ReadU4be();
                _unknownE4 = m_io.ReadU4be();
                _unknownE8 = m_io.ReadU4be();
                _openedTime = m_io.ReadU4be();
                _portalNumber = m_io.ReadU4be();
                _unknownF4Pointer = m_io.ReadU4be();
                _unknownF8 = m_io.ReadU4be();
                _timer = m_io.ReadU4be();
            }
            private SetupGenericObject _objectBase;
            private uint _linkedDoorOffset;
            private uint _maxFrac;
            private uint _perimFrac;
            private uint _accel;
            private uint _decel;
            private uint _maxSpeed;
            private ushort _doorFlags;
            private ushort _doorType;
            private uint _keyFlags;
            private uint _autoCloseFrames;
            private uint _doorOpenSound;
            private uint _frac;
            private uint _unknownAc;
            private uint _unknownB0;
            private float _openPosition;
            private float _speed;
            private byte _state;
            private byte _unknownBd;
            private ushort _unknownBe;
            private uint _unknownC0;
            private ushort _unknownC4;
            private byte _soundType;
            private byte _fadeTime60;
            private uint _linkedDoorPointer;
            private byte _laserFade;
            private byte _unknownCd;
            private ushort _unknownCe;
            private uint _unknownD0;
            private uint _unknownD4;
            private uint _unknownD8;
            private uint _unknownDc;
            private uint _unknownE0;
            private uint _unknownE4;
            private uint _unknownE8;
            private uint _openedTime;
            private uint _portalNumber;
            private uint _unknownF4Pointer;
            private uint _unknownF8;
            private uint _timer;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public SetupGenericObject ObjectBase { get { return _objectBase; } }
            public uint LinkedDoorOffset { get { return _linkedDoorOffset; } }
            public uint MaxFrac { get { return _maxFrac; } }
            public uint PerimFrac { get { return _perimFrac; } }
            public uint Accel { get { return _accel; } }
            public uint Decel { get { return _decel; } }
            public uint MaxSpeed { get { return _maxSpeed; } }
            public ushort DoorFlags { get { return _doorFlags; } }
            public ushort DoorType { get { return _doorType; } }
            public uint KeyFlags { get { return _keyFlags; } }
            public uint AutoCloseFrames { get { return _autoCloseFrames; } }
            public uint DoorOpenSound { get { return _doorOpenSound; } }
            public uint Frac { get { return _frac; } }
            public uint UnknownAc { get { return _unknownAc; } }
            public uint UnknownB0 { get { return _unknownB0; } }
            public float OpenPosition { get { return _openPosition; } }
            public float Speed { get { return _speed; } }
            public byte State { get { return _state; } }
            public byte UnknownBd { get { return _unknownBd; } }
            public ushort UnknownBe { get { return _unknownBe; } }
            public uint UnknownC0 { get { return _unknownC0; } }
            public ushort UnknownC4 { get { return _unknownC4; } }
            public byte SoundType { get { return _soundType; } }
            public byte FadeTime60 { get { return _fadeTime60; } }
            public uint LinkedDoorPointer { get { return _linkedDoorPointer; } }
            public byte LaserFade { get { return _laserFade; } }
            public byte UnknownCd { get { return _unknownCd; } }
            public ushort UnknownCe { get { return _unknownCe; } }
            public uint UnknownD0 { get { return _unknownD0; } }
            public uint UnknownD4 { get { return _unknownD4; } }
            public uint UnknownD8 { get { return _unknownD8; } }
            public uint UnknownDc { get { return _unknownDc; } }
            public uint UnknownE0 { get { return _unknownE0; } }
            public uint UnknownE4 { get { return _unknownE4; } }
            public uint UnknownE8 { get { return _unknownE8; } }
            public uint OpenedTime { get { return _openedTime; } }
            public uint PortalNumber { get { return _portalNumber; } }
            public uint UnknownF4Pointer { get { return _unknownF4Pointer; } }
            public uint UnknownF8 { get { return _unknownF8; } }
            public uint Timer { get { return _timer; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class PathLinkEntry : KaitaiStruct
        {
            public static PathLinkEntry FromFile(string fileName)
            {
                return new PathLinkEntry(new KaitaiStream(fileName));
            }

            public PathLinkEntry(KaitaiStream p__io, Setup.PathLinksSection p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_padNeighborIds = false;
                f_padIndexIds = false;
                _read();
            }
            private void _read()
            {
                _padNeighborOffset = m_io.ReadU4be();
                _padIndexOffset = m_io.ReadU4be();
                _empty = m_io.ReadU4be();
            }
            private bool f_padNeighborIds;
            private List<FfListItem> _padNeighborIds;
            public List<FfListItem> PadNeighborIds
            {
                get
                {
                    if (f_padNeighborIds)
                        return _padNeighborIds;
                    if (PadNeighborOffset > 0)
                    {
                        KaitaiStream io = M_Root.M_Io;
                        long _pos = io.Pos;
                        io.Seek(PadNeighborOffset);
                        _padNeighborIds = new List<FfListItem>();
                        {
                            var i = 0;
                            FfListItem M_;
                            do
                            {
                                M_ = new FfListItem(io, this, m_root);
                                _padNeighborIds.Add(M_);
                                i++;
                            } while (!(M_.Value == 4294967295));
                        }
                        io.Seek(_pos);
                        f_padNeighborIds = true;
                    }
                    return _padNeighborIds;
                }
            }
            private bool f_padIndexIds;
            private List<FfListItem> _padIndexIds;
            public List<FfListItem> PadIndexIds
            {
                get
                {
                    if (f_padIndexIds)
                        return _padIndexIds;
                    if (PadIndexOffset > 0)
                    {
                        KaitaiStream io = M_Root.M_Io;
                        long _pos = io.Pos;
                        io.Seek(PadIndexOffset);
                        _padIndexIds = new List<FfListItem>();
                        {
                            var i = 0;
                            FfListItem M_;
                            do
                            {
                                M_ = new FfListItem(io, this, m_root);
                                _padIndexIds.Add(M_);
                                i++;
                            } while (!(M_.Value == 4294967295));
                        }
                        io.Seek(_pos);
                        f_padIndexIds = true;
                    }
                    return _padIndexIds;
                }
            }
            private uint _padNeighborOffset;
            private uint _padIndexOffset;
            private uint _empty;
            private Setup m_root;
            private Setup.PathLinksSection m_parent;
            public uint PadNeighborOffset { get { return _padNeighborOffset; } }
            public uint PadIndexOffset { get { return _padIndexOffset; } }
            public uint Empty { get { return _empty; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.PathLinksSection M_Parent { get { return m_parent; } }
        }
        public partial class SetupGenericObject : KaitaiStruct
        {
            public static SetupGenericObject FromFile(string fileName)
            {
                return new SetupGenericObject(new KaitaiStream(fileName));
            }

            public SetupGenericObject(KaitaiStream p__io, KaitaiStruct p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectId = m_io.ReadU2be();
                _preset = m_io.ReadU2be();
                _flags1 = m_io.ReadU4be();
                _flags2 = m_io.ReadU4be();
                _pointerPositionData = m_io.ReadU4be();
                _pointerObjInstanceController = m_io.ReadU4be();
                _unknown18 = m_io.ReadU4be();
                _unknown1c = m_io.ReadU4be();
                _unknown20 = m_io.ReadU4be();
                _unknown24 = m_io.ReadU4be();
                _unknown28 = m_io.ReadU4be();
                _unknown2c = m_io.ReadU4be();
                _unknown30 = m_io.ReadU4be();
                _unknown34 = m_io.ReadU4be();
                _unknown38 = m_io.ReadU4be();
                _unknown3c = m_io.ReadU4be();
                _unknown40 = m_io.ReadU4be();
                _unknown44 = m_io.ReadU4be();
                _unknown48 = m_io.ReadU4be();
                _unknown4c = m_io.ReadU4be();
                _unknown50 = m_io.ReadU4be();
                _unknown54 = m_io.ReadU4be();
                _xpos = m_io.ReadU4be();
                _ypos = m_io.ReadU4be();
                _zpos = m_io.ReadU4be();
                _bitflags = m_io.ReadU4be();
                _pointerCollisionBlock = m_io.ReadU4be();
                _unknown6c = m_io.ReadU4be();
                _unknown70 = m_io.ReadU4be();
                _health = m_io.ReadU2be();
                _maxHealth = m_io.ReadU2be();
                _unknown78 = m_io.ReadU4be();
                _unknown7c = m_io.ReadU4be();
            }
            private ushort _objectId;
            private ushort _preset;
            private uint _flags1;
            private uint _flags2;
            private uint _pointerPositionData;
            private uint _pointerObjInstanceController;
            private uint _unknown18;
            private uint _unknown1c;
            private uint _unknown20;
            private uint _unknown24;
            private uint _unknown28;
            private uint _unknown2c;
            private uint _unknown30;
            private uint _unknown34;
            private uint _unknown38;
            private uint _unknown3c;
            private uint _unknown40;
            private uint _unknown44;
            private uint _unknown48;
            private uint _unknown4c;
            private uint _unknown50;
            private uint _unknown54;
            private uint _xpos;
            private uint _ypos;
            private uint _zpos;
            private uint _bitflags;
            private uint _pointerCollisionBlock;
            private uint _unknown6c;
            private uint _unknown70;
            private ushort _health;
            private ushort _maxHealth;
            private uint _unknown78;
            private uint _unknown7c;
            private Setup m_root;
            private KaitaiStruct m_parent;
            public ushort ObjectId { get { return _objectId; } }
            public ushort Preset { get { return _preset; } }
            public uint Flags1 { get { return _flags1; } }
            public uint Flags2 { get { return _flags2; } }
            public uint PointerPositionData { get { return _pointerPositionData; } }
            public uint PointerObjInstanceController { get { return _pointerObjInstanceController; } }
            public uint Unknown18 { get { return _unknown18; } }
            public uint Unknown1c { get { return _unknown1c; } }
            public uint Unknown20 { get { return _unknown20; } }
            public uint Unknown24 { get { return _unknown24; } }
            public uint Unknown28 { get { return _unknown28; } }
            public uint Unknown2c { get { return _unknown2c; } }
            public uint Unknown30 { get { return _unknown30; } }
            public uint Unknown34 { get { return _unknown34; } }
            public uint Unknown38 { get { return _unknown38; } }
            public uint Unknown3c { get { return _unknown3c; } }
            public uint Unknown40 { get { return _unknown40; } }
            public uint Unknown44 { get { return _unknown44; } }
            public uint Unknown48 { get { return _unknown48; } }
            public uint Unknown4c { get { return _unknown4c; } }
            public uint Unknown50 { get { return _unknown50; } }
            public uint Unknown54 { get { return _unknown54; } }
            public uint Xpos { get { return _xpos; } }
            public uint Ypos { get { return _ypos; } }
            public uint Zpos { get { return _zpos; } }
            public uint Bitflags { get { return _bitflags; } }
            public uint PointerCollisionBlock { get { return _pointerCollisionBlock; } }
            public uint Unknown6c { get { return _unknown6c; } }
            public uint Unknown70 { get { return _unknown70; } }
            public ushort Health { get { return _health; } }
            public ushort MaxHealth { get { return _maxHealth; } }
            public uint Unknown78 { get { return _unknown78; } }
            public uint Unknown7c { get { return _unknown7c; } }
            public Setup M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectLockBody : KaitaiStruct
        {
            public static SetupObjectLockBody FromFile(string fileName)
            {
                return new SetupObjectLockBody(new KaitaiStream(fileName));
            }

            public SetupObjectLockBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _door = m_io.ReadS4be();
                _lock = m_io.ReadS4be();
                _empty = m_io.ReadS4be();
            }
            private int _door;
            private int _lock;
            private int _empty;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public int Door { get { return _door; } }
            public int Lock { get { return _lock; } }
            public int Empty { get { return _empty; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectiveEnterRoomBody : KaitaiStruct
        {
            public static SetupObjectiveEnterRoomBody FromFile(string fileName)
            {
                return new SetupObjectiveEnterRoomBody(new KaitaiStream(fileName));
            }

            public SetupObjectiveEnterRoomBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _room = m_io.ReadS4be();
                _unknown04 = m_io.ReadS4be();
                _unknown08 = m_io.ReadS4be();
            }
            private int _room;
            private int _unknown04;
            private int _unknown08;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public int Room { get { return _room; } }
            public int Unknown04 { get { return _unknown04; } }
            public int Unknown08 { get { return _unknown08; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupIntroEndIntroBody : KaitaiStruct
        {
            public static SetupIntroEndIntroBody FromFile(string fileName)
            {
                return new SetupIntroEndIntroBody(new KaitaiStream(fileName));
            }

            public SetupIntroEndIntroBody(KaitaiStream p__io, Setup.SetupIntroRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _noValue = m_io.ReadBytes(0);
            }
            private byte[] _noValue;
            private Setup m_root;
            private Setup.SetupIntroRecord m_parent;
            public byte[] NoValue { get { return _noValue; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupIntroRecord M_Parent { get { return m_parent; } }
        }
        public partial class Pad : KaitaiStruct
        {
            public static Pad FromFile(string fileName)
            {
                return new Pad(new KaitaiStream(fileName));
            }

            public Pad(KaitaiStream p__io, Setup.PadList p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _pos = new Coord3d(m_io, this, m_root);
                _up = new Coord3d(m_io, this, m_root);
                _look = new Coord3d(m_io, this, m_root);
                _plink = new StringPointer(m_io, this, m_root);
                _unknown = m_io.ReadU4be();
            }
            private Coord3d _pos;
            private Coord3d _up;
            private Coord3d _look;
            private StringPointer _plink;
            private uint _unknown;
            private Setup m_root;
            private Setup.PadList m_parent;
            public Coord3d Pos { get { return _pos; } }
            public Coord3d Up { get { return _up; } }
            public Coord3d Look { get { return _look; } }
            public StringPointer Plink { get { return _plink; } }
            public uint Unknown { get { return _unknown; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.PadList M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectAmmoBoxBody : KaitaiStruct
        {
            public static SetupObjectAmmoBoxBody FromFile(string fileName)
            {
                return new SetupObjectAmmoBoxBody(new KaitaiStream(fileName));
            }

            public SetupObjectAmmoBoxBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectBase = new SetupGenericObject(m_io, this, m_root);
                _unused00 = m_io.ReadU2be();
                _ammo9mm = m_io.ReadS2be();
                _unused04 = m_io.ReadU2be();
                _ammo9mm2 = m_io.ReadS2be();
                _unused08 = m_io.ReadU2be();
                _ammoRifle = m_io.ReadS2be();
                _unused0c = m_io.ReadU2be();
                _ammoShotgun = m_io.ReadS2be();
                _unused10 = m_io.ReadU2be();
                _ammoHgrenade = m_io.ReadS2be();
                _unused14 = m_io.ReadU2be();
                _ammoRockets = m_io.ReadS2be();
                _unused18 = m_io.ReadU2be();
                _ammoRemoteMine = m_io.ReadS2be();
                _unused1c = m_io.ReadU2be();
                _ammoProximityMine = m_io.ReadS2be();
                _unused20 = m_io.ReadU2be();
                _ammoTimedMine = m_io.ReadS2be();
                _unused24 = m_io.ReadU2be();
                _ammoThrowing = m_io.ReadS2be();
                _unused28 = m_io.ReadU2be();
                _ammoGrenadeLauncher = m_io.ReadS2be();
                _unused2c = m_io.ReadU2be();
                _ammoMagnum = m_io.ReadS2be();
                _unused30 = m_io.ReadU2be();
                _ammoGolden = m_io.ReadS2be();
            }
            private SetupGenericObject _objectBase;
            private ushort _unused00;
            private short _ammo9mm;
            private ushort _unused04;
            private short _ammo9mm2;
            private ushort _unused08;
            private short _ammoRifle;
            private ushort _unused0c;
            private short _ammoShotgun;
            private ushort _unused10;
            private short _ammoHgrenade;
            private ushort _unused14;
            private short _ammoRockets;
            private ushort _unused18;
            private short _ammoRemoteMine;
            private ushort _unused1c;
            private short _ammoProximityMine;
            private ushort _unused20;
            private short _ammoTimedMine;
            private ushort _unused24;
            private short _ammoThrowing;
            private ushort _unused28;
            private short _ammoGrenadeLauncher;
            private ushort _unused2c;
            private short _ammoMagnum;
            private ushort _unused30;
            private short _ammoGolden;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public SetupGenericObject ObjectBase { get { return _objectBase; } }
            public ushort Unused00 { get { return _unused00; } }
            public short Ammo9mm { get { return _ammo9mm; } }
            public ushort Unused04 { get { return _unused04; } }
            public short Ammo9mm2 { get { return _ammo9mm2; } }
            public ushort Unused08 { get { return _unused08; } }
            public short AmmoRifle { get { return _ammoRifle; } }
            public ushort Unused0c { get { return _unused0c; } }
            public short AmmoShotgun { get { return _ammoShotgun; } }
            public ushort Unused10 { get { return _unused10; } }
            public short AmmoHgrenade { get { return _ammoHgrenade; } }
            public ushort Unused14 { get { return _unused14; } }
            public short AmmoRockets { get { return _ammoRockets; } }
            public ushort Unused18 { get { return _unused18; } }
            public short AmmoRemoteMine { get { return _ammoRemoteMine; } }
            public ushort Unused1c { get { return _unused1c; } }
            public short AmmoProximityMine { get { return _ammoProximityMine; } }
            public ushort Unused20 { get { return _unused20; } }
            public short AmmoTimedMine { get { return _ammoTimedMine; } }
            public ushort Unused24 { get { return _unused24; } }
            public short AmmoThrowing { get { return _ammoThrowing; } }
            public ushort Unused28 { get { return _unused28; } }
            public short AmmoGrenadeLauncher { get { return _ammoGrenadeLauncher; } }
            public ushort Unused2c { get { return _unused2c; } }
            public short AmmoMagnum { get { return _ammoMagnum; } }
            public ushort Unused30 { get { return _unused30; } }
            public short AmmoGolden { get { return _ammoGolden; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectTankBody : KaitaiStruct
        {
            public static SetupObjectTankBody FromFile(string fileName)
            {
                return new SetupObjectTankBody(new KaitaiStream(fileName));
            }

            public SetupObjectTankBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectBase = new SetupGenericObject(m_io, this, m_root);
                _bytes = m_io.ReadBytes(96);
            }
            private SetupGenericObject _objectBase;
            private byte[] _bytes;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public SetupGenericObject ObjectBase { get { return _objectBase; } }
            public byte[] Bytes { get { return _bytes; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectAircraftBody : KaitaiStruct
        {
            public static SetupObjectAircraftBody FromFile(string fileName)
            {
                return new SetupObjectAircraftBody(new KaitaiStream(fileName));
            }

            public SetupObjectAircraftBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectBase = new SetupGenericObject(m_io, this, m_root);
                _bytes = m_io.ReadBytes(52);
            }
            private SetupGenericObject _objectBase;
            private byte[] _bytes;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public SetupGenericObject ObjectBase { get { return _objectBase; } }
            public byte[] Bytes { get { return _bytes; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectHangingMonitorBody : KaitaiStruct
        {
            public static SetupObjectHangingMonitorBody FromFile(string fileName)
            {
                return new SetupObjectHangingMonitorBody(new KaitaiStream(fileName));
            }

            public SetupObjectHangingMonitorBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectBase = new SetupGenericObject(m_io, this, m_root);
            }
            private SetupGenericObject _objectBase;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public SetupGenericObject ObjectBase { get { return _objectBase; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class PathLinksSection : KaitaiStruct
        {
            public static PathLinksSection FromFile(string fileName)
            {
                return new PathLinksSection(new KaitaiStream(fileName));
            }

            public PathLinksSection(KaitaiStream p__io, Setup.SectionBlock p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_type = false;
                _read();
            }
            private void _read()
            {
                _data = new List<PathLinkEntry>();
                {
                    var i = 0;
                    PathLinkEntry M_;
                    do
                    {
                        M_ = new PathLinkEntry(m_io, this, m_root);
                        _data.Add(M_);
                        i++;
                    } while (!(M_.PadNeighborOffset == 0));
                }
            }
            private bool f_type;
            private SectionId _type;
            public SectionId Type
            {
                get
                {
                    if (f_type)
                        return _type;
                    _type = (SectionId)(Setup.SectionId.PathLinksSection);
                    f_type = true;
                    return _type;
                }
            }
            private List<PathLinkEntry> _data;
            private Setup m_root;
            private Setup.SectionBlock m_parent;
            public List<PathLinkEntry> Data { get { return _data; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SectionBlock M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectCutsceneBody : KaitaiStruct
        {
            public static SetupObjectCutsceneBody FromFile(string fileName)
            {
                return new SetupObjectCutsceneBody(new KaitaiStream(fileName));
            }

            public SetupObjectCutsceneBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _xcoord = m_io.ReadU4be();
                _ycoord = m_io.ReadU4be();
                _zcoord = m_io.ReadU4be();
                _latRot = m_io.ReadU4be();
                _vertRot = m_io.ReadU4be();
                _illumPreset = m_io.ReadU4be();
            }
            private uint _xcoord;
            private uint _ycoord;
            private uint _zcoord;
            private uint _latRot;
            private uint _vertRot;
            private uint _illumPreset;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public uint Xcoord { get { return _xcoord; } }
            public uint Ycoord { get { return _ycoord; } }
            public uint Zcoord { get { return _zcoord; } }
            public uint LatRot { get { return _latRot; } }
            public uint VertRot { get { return _vertRot; } }
            public uint IllumPreset { get { return _illumPreset; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectGlassTintedBody : KaitaiStruct
        {
            public static SetupObjectGlassTintedBody FromFile(string fileName)
            {
                return new SetupObjectGlassTintedBody(new KaitaiStream(fileName));
            }

            public SetupObjectGlassTintedBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectBase = new SetupGenericObject(m_io, this, m_root);
                _unknown04 = m_io.ReadS4be();
                _unknown08 = m_io.ReadS4be();
                _unknown0c = m_io.ReadS4be();
                _unknown10 = m_io.ReadS4be();
                _unknown14 = m_io.ReadS4be();
            }
            private SetupGenericObject _objectBase;
            private int _unknown04;
            private int _unknown08;
            private int _unknown0c;
            private int _unknown10;
            private int _unknown14;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public SetupGenericObject ObjectBase { get { return _objectBase; } }
            public int Unknown04 { get { return _unknown04; } }
            public int Unknown08 { get { return _unknown08; } }
            public int Unknown0c { get { return _unknown0c; } }
            public int Unknown10 { get { return _unknown10; } }
            public int Unknown14 { get { return _unknown14; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class StageSetupStruct : KaitaiStruct
        {
            public static StageSetupStruct FromFile(string fileName)
            {
                return new StageSetupStruct(new KaitaiStream(fileName));
            }

            public StageSetupStruct(KaitaiStream p__io, Setup p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _pathTablesOffset = m_io.ReadU4be();
                _pathLinksOffset = m_io.ReadU4be();
                _introsOffset = m_io.ReadU4be();
                _objectListOffset = m_io.ReadU4be();
                _pathSetsOffset = m_io.ReadU4be();
                _aiListOffset = m_io.ReadU4be();
                _padListOffset = m_io.ReadU4be();
                _pad3dListOffset = m_io.ReadU4be();
                _padNamesOffset = m_io.ReadU4be();
                _pad3dNamesOffset = m_io.ReadU4be();
            }
            private uint _pathTablesOffset;
            private uint _pathLinksOffset;
            private uint _introsOffset;
            private uint _objectListOffset;
            private uint _pathSetsOffset;
            private uint _aiListOffset;
            private uint _padListOffset;
            private uint _pad3dListOffset;
            private uint _padNamesOffset;
            private uint _pad3dNamesOffset;
            private Setup m_root;
            private Setup m_parent;
            public uint PathTablesOffset { get { return _pathTablesOffset; } }
            public uint PathLinksOffset { get { return _pathLinksOffset; } }
            public uint IntrosOffset { get { return _introsOffset; } }
            public uint ObjectListOffset { get { return _objectListOffset; } }
            public uint PathSetsOffset { get { return _pathSetsOffset; } }
            public uint AiListOffset { get { return _aiListOffset; } }
            public uint PadListOffset { get { return _padListOffset; } }
            public uint Pad3dListOffset { get { return _pad3dListOffset; } }
            public uint PadNamesOffset { get { return _padNamesOffset; } }
            public uint Pad3dNamesOffset { get { return _pad3dNamesOffset; } }
            public Setup M_Root { get { return m_root; } }
            public Setup M_Parent { get { return m_parent; } }
        }
        public partial class AiList : KaitaiStruct
        {
            public static AiList FromFile(string fileName)
            {
                return new AiList(new KaitaiStream(fileName));
            }

            public AiList(KaitaiStream p__io, Setup.SectionBlock p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_type = false;
                _read();
            }
            private void _read()
            {
                _data = new List<SetupAiScript>();
                {
                    var i = 0;
                    SetupAiScript M_;
                    do
                    {
                        M_ = new SetupAiScript(m_io, this, m_root);
                        _data.Add(M_);
                        i++;
                    } while (!(M_.Pointer == 0));
                }
            }
            private bool f_type;
            private SectionId _type;
            public SectionId Type
            {
                get
                {
                    if (f_type)
                        return _type;
                    _type = (SectionId)(Setup.SectionId.AiListSection);
                    f_type = true;
                    return _type;
                }
            }
            private List<SetupAiScript> _data;
            private Setup m_root;
            private Setup.SectionBlock m_parent;
            public List<SetupAiScript> Data { get { return _data; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SectionBlock M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectCctvBody : KaitaiStruct
        {
            public static SetupObjectCctvBody FromFile(string fileName)
            {
                return new SetupObjectCctvBody(new KaitaiStream(fileName));
            }

            public SetupObjectCctvBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectBase = new SetupGenericObject(m_io, this, m_root);
                _bytes = m_io.ReadBytes(108);
            }
            private SetupGenericObject _objectBase;
            private byte[] _bytes;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public SetupGenericObject ObjectBase { get { return _objectBase; } }
            public byte[] Bytes { get { return _bytes; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectAlarmBody : KaitaiStruct
        {
            public static SetupObjectAlarmBody FromFile(string fileName)
            {
                return new SetupObjectAlarmBody(new KaitaiStream(fileName));
            }

            public SetupObjectAlarmBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectBase = new SetupGenericObject(m_io, this, m_root);
            }
            private SetupGenericObject _objectBase;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public SetupGenericObject ObjectBase { get { return _objectBase; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectEndProps : KaitaiStruct
        {
            public static SetupObjectEndProps FromFile(string fileName)
            {
                return new SetupObjectEndProps(new KaitaiStream(fileName));
            }

            public SetupObjectEndProps(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _noValue = m_io.ReadBytes(0);
            }
            private byte[] _noValue;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public byte[] NoValue { get { return _noValue; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class FillerBlock : KaitaiStruct
        {
            public FillerBlock(uint p_startPos, KaitaiStream p__io, Setup.SectionBlock p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _startPos = p_startPos;
                f_len = false;
                f_csharpFillerType = false;
                _read();
            }
            private void _read()
            {
                _data = new List<byte[]>();
                {
                    var i = 0;
                    byte[] M_;
                    do
                    {
                        M_ = m_io.ReadBytes(1);
                        _data.Add(M_);
                        i++;
                    } while (!(((M_Root.M_Io.Pos == M_Root.M_Io.Size) || (M_Root.M_Io.Pos == M_Root.Pointers.PathTablesOffset) || (M_Root.M_Io.Pos == M_Root.Pointers.PathLinksOffset) || (M_Root.M_Io.Pos == M_Root.Pointers.IntrosOffset) || (M_Root.M_Io.Pos == M_Root.Pointers.ObjectListOffset) || (M_Root.M_Io.Pos == M_Root.Pointers.PathSetsOffset) || (M_Root.M_Io.Pos == M_Root.Pointers.AiListOffset) || (M_Root.M_Io.Pos == M_Root.Pointers.PadListOffset) || (M_Root.M_Io.Pos == M_Root.Pointers.Pad3dListOffset) || (M_Root.M_Io.Pos == M_Root.Pointers.PadNamesOffset) || (M_Root.M_Io.Pos == M_Root.Pointers.Pad3dNamesOffset))));
                }
            }
            private bool f_len;
            private int _len;
            public int Len
            {
                get
                {
                    if (f_len)
                        return _len;
                    _len = (int)(Data.Count);
                    f_len = true;
                    return _len;
                }
            }
            private bool f_csharpFillerType;
            private sbyte _csharpFillerType;
            public sbyte CsharpFillerType
            {
                get
                {
                    if (f_csharpFillerType)
                        return _csharpFillerType;
                    _csharpFillerType = (sbyte)(0);
                    f_csharpFillerType = true;
                    return _csharpFillerType;
                }
            }
            private List<byte[]> _data;
            private uint _startPos;
            private Setup m_root;
            private Setup.SectionBlock m_parent;
            public List<byte[]> Data { get { return _data; } }
            public uint StartPos { get { return _startPos; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SectionBlock M_Parent { get { return m_parent; } }
        }
        public partial class IntroNotSupported : KaitaiStruct
        {
            public IntroNotSupported(Introdef p_type, KaitaiStream p__io, Setup.SetupIntroRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _type = p_type;
                _read();
            }
            private void _read()
            {
                _pos = m_io.ReadU4be();
                if (!(Pos == M_Io.Pos))
                {
                    throw new ValidationNotEqualError(M_Io.Pos, Pos, M_Io, "/types/intro_not_supported/seq/0");
                }
            }
            private uint _pos;
            private Introdef _type;
            private Setup m_root;
            private Setup.SetupIntroRecord m_parent;
            public uint Pos { get { return _pos; } }
            public Introdef Type { get { return _type; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupIntroRecord M_Parent { get { return m_parent; } }
        }
        public partial class ObjectHeaderData : KaitaiStruct
        {
            public static ObjectHeaderData FromFile(string fileName)
            {
                return new ObjectHeaderData(new KaitaiStream(fileName));
            }

            public ObjectHeaderData(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _extraScale = m_io.ReadU2be();
                _state = m_io.ReadU1();
                _type = ((Setup.Propdef)m_io.ReadU1());
            }
            private ushort _extraScale;
            private byte _state;
            private Propdef _type;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public ushort ExtraScale { get { return _extraScale; } }
            public byte State { get { return _state; } }
            public Propdef Type { get { return _type; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupIntroRecord : KaitaiStruct
        {
            public static SetupIntroRecord FromFile(string fileName)
            {
                return new SetupIntroRecord(new KaitaiStream(fileName));
            }

            public SetupIntroRecord(KaitaiStream p__io, Setup.IntroList p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _header = new SetupIntroHeaderData(m_io, this, m_root);
                switch (Header.Type)
                {
                    case Setup.Introdef.IntroCam:
                        {
                            _body = new SetupIntroIntroCamBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Introdef.SwirlCam:
                        {
                            _body = new SetupIntroSwirlCamBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Introdef.WatchTime:
                        {
                            _body = new SetupIntroWatchTimeBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Introdef.EndIntro:
                        {
                            _body = new SetupIntroEndIntroBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Introdef.StartWeapon:
                        {
                            _body = new SetupIntroStartWeaponBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Introdef.StartAmmo:
                        {
                            _body = new SetupIntroStartAmmoBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Introdef.Cuff:
                        {
                            _body = new SetupIntroCuffBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Introdef.FixedCam:
                        {
                            _body = new SetupIntroFixedCamBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Introdef.Credits:
                        {
                            _body = new SetupIntroCreditsBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Introdef.Spawn:
                        {
                            _body = new SetupIntroSpawnBody(m_io, this, m_root);
                            break;
                        }
                    default:
                        {
                            _body = new IntroNotSupported(Header.Type, m_io, this, m_root);
                            break;
                        }
                }
            }
            private SetupIntroHeaderData _header;
            private KaitaiStruct _body;
            private Setup m_root;
            private Setup.IntroList m_parent;
            public SetupIntroHeaderData Header { get { return _header; } }
            public KaitaiStruct Body { get { return _body; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.IntroList M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectMissionObjectiveBody : KaitaiStruct
        {
            public static SetupObjectMissionObjectiveBody FromFile(string fileName)
            {
                return new SetupObjectMissionObjectiveBody(new KaitaiStream(fileName));
            }

            public SetupObjectMissionObjectiveBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectiveNumber = m_io.ReadU4be();
                _textId = m_io.ReadU4be();
                _minDifficulty = m_io.ReadU4be();
            }
            private uint _objectiveNumber;
            private uint _textId;
            private uint _minDifficulty;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public uint ObjectiveNumber { get { return _objectiveNumber; } }
            public uint TextId { get { return _textId; } }
            public uint MinDifficulty { get { return _minDifficulty; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupIntroCreditsBody : KaitaiStruct
        {
            public static SetupIntroCreditsBody FromFile(string fileName)
            {
                return new SetupIntroCreditsBody(new KaitaiStream(fileName));
            }

            public SetupIntroCreditsBody(KaitaiStream p__io, Setup.SetupIntroRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_creditData = false;
                _read();
            }
            private void _read()
            {
                _dataOffset = m_io.ReadS4be();
            }
            private bool f_creditData;
            private List<IntroCreditEntry> _creditData;
            public List<IntroCreditEntry> CreditData
            {
                get
                {
                    if (f_creditData)
                        return _creditData;
                    KaitaiStream io = M_Root.M_Io;
                    long _pos = io.Pos;
                    io.Seek(DataOffset);
                    _creditData = new List<IntroCreditEntry>();
                    {
                        var i = 0;
                        IntroCreditEntry M_;
                        do
                        {
                            M_ = new IntroCreditEntry(io, this, m_root);
                            _creditData.Add(M_);
                            i++;
                        } while (!(((M_.TextId1 == 0) && (M_.TextId2 == 0))));
                    }
                    io.Seek(_pos);
                    f_creditData = true;
                    return _creditData;
                }
            }
            private int _dataOffset;
            private Setup m_root;
            private Setup.SetupIntroRecord m_parent;
            public int DataOffset { get { return _dataOffset; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupIntroRecord M_Parent { get { return m_parent; } }
        }
        public partial class Pad3dNamesList : KaitaiStruct
        {
            public static Pad3dNamesList FromFile(string fileName)
            {
                return new Pad3dNamesList(new KaitaiStream(fileName));
            }

            public Pad3dNamesList(KaitaiStream p__io, Setup.SectionBlock p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_type = false;
                _read();
            }
            private void _read()
            {
                _data = new List<StringPointer>();
                {
                    var i = 0;
                    StringPointer M_;
                    do
                    {
                        M_ = new StringPointer(m_io, this, m_root);
                        _data.Add(M_);
                        i++;
                    } while (!(M_.Offset == 0));
                }
            }
            private bool f_type;
            private SectionId _type;
            public SectionId Type
            {
                get
                {
                    if (f_type)
                        return _type;
                    _type = (SectionId)(Setup.SectionId.Pad3dNames);
                    f_type = true;
                    return _type;
                }
            }
            private List<StringPointer> _data;
            private Setup m_root;
            private Setup.SectionBlock m_parent;
            public List<StringPointer> Data { get { return _data; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SectionBlock M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectCollectObjectBody : KaitaiStruct
        {
            public static SetupObjectCollectObjectBody FromFile(string fileName)
            {
                return new SetupObjectCollectObjectBody(new KaitaiStream(fileName));
            }

            public SetupObjectCollectObjectBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectId = m_io.ReadU4be();
            }
            private uint _objectId;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public uint ObjectId { get { return _objectId; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectObjectiveCompleteConditionBody : KaitaiStruct
        {
            public static SetupObjectObjectiveCompleteConditionBody FromFile(string fileName)
            {
                return new SetupObjectObjectiveCompleteConditionBody(new KaitaiStream(fileName));
            }

            public SetupObjectObjectiveCompleteConditionBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _testval = m_io.ReadU4be();
            }
            private uint _testval;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public uint Testval { get { return _testval; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectWeaponBody : KaitaiStruct
        {
            public static SetupObjectWeaponBody FromFile(string fileName)
            {
                return new SetupObjectWeaponBody(new KaitaiStream(fileName));
            }

            public SetupObjectWeaponBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectBase = new SetupGenericObject(m_io, this, m_root);
                _gunPickup = m_io.ReadU1();
                _linkedItem = m_io.ReadU1();
                _timer = m_io.ReadU2be();
                _pointerLinkedItem = m_io.ReadU4be();
            }
            private SetupGenericObject _objectBase;
            private byte _gunPickup;
            private byte _linkedItem;
            private ushort _timer;
            private uint _pointerLinkedItem;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public SetupGenericObject ObjectBase { get { return _objectBase; } }
            public byte GunPickup { get { return _gunPickup; } }
            public byte LinkedItem { get { return _linkedItem; } }
            public ushort Timer { get { return _timer; } }
            public uint PointerLinkedItem { get { return _pointerLinkedItem; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectRecord : KaitaiStruct
        {
            public static SetupObjectRecord FromFile(string fileName)
            {
                return new SetupObjectRecord(new KaitaiStream(fileName));
            }

            public SetupObjectRecord(KaitaiStream p__io, Setup.ObjectList p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _header = new ObjectHeaderData(m_io, this, m_root);
                switch (Header.Type)
                {
                    case Setup.Propdef.Key:
                        {
                            _body = new SetupObjectKeyBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.CollectObject:
                        {
                            _body = new SetupObjectCollectObjectBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.WatchMenuObjectiveText:
                        {
                            _body = new SetupObjectWatchMenuObjectiveBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.Safe:
                        {
                            _body = new SetupObjectSafeBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.ObjectiveStart:
                        {
                            _body = new SetupObjectMissionObjectiveBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.MultiMonitor:
                        {
                            _body = new SetupObjectMultiMonitorBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.ObjectiveCompleteCondition:
                        {
                            _body = new SetupObjectObjectiveCompleteConditionBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.SafeItem:
                        {
                            _body = new SetupObjectSafeItemBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.Alarm:
                        {
                            _body = new SetupObjectAlarmBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.LinkItems:
                        {
                            _body = new SetupObjectLinkItemsBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.Standard:
                        {
                            _body = new SetupObjectStandardBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.Tag:
                        {
                            _body = new SetupObjectTagBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.Vehicle:
                        {
                            _body = new SetupObjectVehicleBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.Door:
                        {
                            _body = new SetupObjectDoorBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.HangingMonitor:
                        {
                            _body = new SetupObjectHangingMonitorBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.ObjectiveCopyItem:
                        {
                            _body = new SetupObjectiveCopyItemBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.Rename:
                        {
                            _body = new SetupObjectRenameBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.Cctv:
                        {
                            _body = new SetupObjectCctvBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.GasProp:
                        {
                            _body = new SetupObjectGasPropBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.AmmoBox:
                        {
                            _body = new SetupObjectAmmoBoxBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.Weapon:
                        {
                            _body = new SetupObjectWeaponBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.ObjectiveEnterRoom:
                        {
                            _body = new SetupObjectiveEnterRoomBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.SingleMonitor:
                        {
                            _body = new SetupObjectSingleMonitorBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.LinkProps:
                        {
                            _body = new SetupObjectLinkPropsBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.AmmoMag:
                        {
                            _body = new SetupObjectAmmoMagBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.GlassTinted:
                        {
                            _body = new SetupObjectGlassTintedBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.Hat:
                        {
                            _body = new SetupObjectHatBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.BodyArmor:
                        {
                            _body = new SetupObjectBodyArmorBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.Glass:
                        {
                            _body = new SetupObjectGlassBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.Tank:
                        {
                            _body = new SetupObjectTankBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.EndObjective:
                        {
                            _body = new SetupObjectEndObjectiveBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.Guard:
                        {
                            _body = new SetupObjectGuardBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.Autogun:
                        {
                            _body = new SetupObjectAutogunBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.DestroyObject:
                        {
                            _body = new SetupObjectDestroyObjectBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.Lock:
                        {
                            _body = new SetupObjectLockBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.Cutscene:
                        {
                            _body = new SetupObjectCutsceneBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.ObjectivePhotographItem:
                        {
                            _body = new SetupObjectivePhotographItemBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.EndProps:
                        {
                            _body = new SetupObjectEndProps(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.Aircraft:
                        {
                            _body = new SetupObjectAircraftBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.ObjectiveFailCondition:
                        {
                            _body = new SetupObjectObjectiveFailConditionBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.ObjectiveThrowInRoom:
                        {
                            _body = new SetupObjectiveThrowInRoomBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.DoorScale:
                        {
                            _body = new SetupObjectDoorScaleBody(m_io, this, m_root);
                            break;
                        }
                    case Setup.Propdef.SetGuardAttribute:
                        {
                            _body = new SetupObjectSetGuardAttributeBody(m_io, this, m_root);
                            break;
                        }
                    default:
                        {
                            _body = new NotSupported(Header.Type, m_io, this, m_root);
                            break;
                        }
                }
            }
            private ObjectHeaderData _header;
            private KaitaiStruct _body;
            private Setup m_root;
            private Setup.ObjectList m_parent;
            public ObjectHeaderData Header { get { return _header; } }
            public KaitaiStruct Body { get { return _body; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.ObjectList M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectGasPropBody : KaitaiStruct
        {
            public static SetupObjectGasPropBody FromFile(string fileName)
            {
                return new SetupObjectGasPropBody(new KaitaiStream(fileName));
            }

            public SetupObjectGasPropBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectBase = new SetupGenericObject(m_io, this, m_root);
            }
            private SetupGenericObject _objectBase;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public SetupGenericObject ObjectBase { get { return _objectBase; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectDoorScaleBody : KaitaiStruct
        {
            public static SetupObjectDoorScaleBody FromFile(string fileName)
            {
                return new SetupObjectDoorScaleBody(new KaitaiStream(fileName));
            }

            public SetupObjectDoorScaleBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _modifier = m_io.ReadS4be();
            }
            private int _modifier;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public int Modifier { get { return _modifier; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectMultiMonitorBody : KaitaiStruct
        {
            public static SetupObjectMultiMonitorBody FromFile(string fileName)
            {
                return new SetupObjectMultiMonitorBody(new KaitaiStream(fileName));
            }

            public SetupObjectMultiMonitorBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectBase = new SetupGenericObject(m_io, this, m_root);
                _bytes = m_io.ReadBytes(468);
            }
            private SetupGenericObject _objectBase;
            private byte[] _bytes;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public SetupGenericObject ObjectBase { get { return _objectBase; } }
            public byte[] Bytes { get { return _bytes; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectiveCopyItemBody : KaitaiStruct
        {
            public static SetupObjectiveCopyItemBody FromFile(string fileName)
            {
                return new SetupObjectiveCopyItemBody(new KaitaiStream(fileName));
            }

            public SetupObjectiveCopyItemBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectTagId = m_io.ReadU4be();
                _unknown04 = m_io.ReadU2be();
                _unknown06 = m_io.ReadU2be();
            }
            private uint _objectTagId;
            private ushort _unknown04;
            private ushort _unknown06;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public uint ObjectTagId { get { return _objectTagId; } }
            public ushort Unknown04 { get { return _unknown04; } }
            public ushort Unknown06 { get { return _unknown06; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupAiScript : KaitaiStruct
        {
            public static SetupAiScript FromFile(string fileName)
            {
                return new SetupAiScript(new KaitaiStream(fileName));
            }

            public SetupAiScript(KaitaiStream p__io, Setup.AiList p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _pointer = m_io.ReadU4be();
                _id = m_io.ReadU4be();
            }
            private uint _pointer;
            private uint _id;
            private Setup m_root;
            private Setup.AiList m_parent;
            public uint Pointer { get { return _pointer; } }
            public uint Id { get { return _id; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.AiList M_Parent { get { return m_parent; } }
        }
        public partial class SetupIntroSpawnBody : KaitaiStruct
        {
            public static SetupIntroSpawnBody FromFile(string fileName)
            {
                return new SetupIntroSpawnBody(new KaitaiStream(fileName));
            }

            public SetupIntroSpawnBody(KaitaiStream p__io, Setup.SetupIntroRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _unknown00 = m_io.ReadU4be();
                _unknown04 = m_io.ReadU4be();
            }
            private uint _unknown00;
            private uint _unknown04;
            private Setup m_root;
            private Setup.SetupIntroRecord m_parent;
            public uint Unknown00 { get { return _unknown00; } }
            public uint Unknown04 { get { return _unknown04; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupIntroRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectLinkPropsBody : KaitaiStruct
        {
            public static SetupObjectLinkPropsBody FromFile(string fileName)
            {
                return new SetupObjectLinkPropsBody(new KaitaiStream(fileName));
            }

            public SetupObjectLinkPropsBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _offset1 = m_io.ReadS4be();
                _offset2 = m_io.ReadS4be();
                _unknown08 = m_io.ReadS4be();
            }
            private int _offset1;
            private int _offset2;
            private int _unknown08;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public int Offset1 { get { return _offset1; } }
            public int Offset2 { get { return _offset2; } }
            public int Unknown08 { get { return _unknown08; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class PathSetsSection : KaitaiStruct
        {
            public static PathSetsSection FromFile(string fileName)
            {
                return new PathSetsSection(new KaitaiStream(fileName));
            }

            public PathSetsSection(KaitaiStream p__io, Setup.SectionBlock p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_type = false;
                _read();
            }
            private void _read()
            {
                _data = new List<PathSetEntry>();
                {
                    var i = 0;
                    PathSetEntry M_;
                    do
                    {
                        M_ = new PathSetEntry(m_io, this, m_root);
                        _data.Add(M_);
                        i++;
                    } while (!(M_.Pointer == 0));
                }
            }
            private bool f_type;
            private SectionId _type;
            public SectionId Type
            {
                get
                {
                    if (f_type)
                        return _type;
                    _type = (SectionId)(Setup.SectionId.PathSetsSection);
                    f_type = true;
                    return _type;
                }
            }
            private List<PathSetEntry> _data;
            private Setup m_root;
            private Setup.SectionBlock m_parent;
            public List<PathSetEntry> Data { get { return _data; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SectionBlock M_Parent { get { return m_parent; } }
        }
        public partial class SetupIntroIntroCamBody : KaitaiStruct
        {
            public static SetupIntroIntroCamBody FromFile(string fileName)
            {
                return new SetupIntroIntroCamBody(new KaitaiStream(fileName));
            }

            public SetupIntroIntroCamBody(KaitaiStream p__io, Setup.SetupIntroRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _animation = m_io.ReadU4be();
            }
            private uint _animation;
            private Setup m_root;
            private Setup.SetupIntroRecord m_parent;
            public uint Animation { get { return _animation; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupIntroRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectWatchMenuObjectiveBody : KaitaiStruct
        {
            public static SetupObjectWatchMenuObjectiveBody FromFile(string fileName)
            {
                return new SetupObjectWatchMenuObjectiveBody(new KaitaiStream(fileName));
            }

            public SetupObjectWatchMenuObjectiveBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _menuOption = m_io.ReadU4be();
                _textId = m_io.ReadU4be();
                _end = m_io.ReadU4be();
            }
            private uint _menuOption;
            private uint _textId;
            private uint _end;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public uint MenuOption { get { return _menuOption; } }
            public uint TextId { get { return _textId; } }
            public uint End { get { return _end; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectDestroyObjectBody : KaitaiStruct
        {
            public static SetupObjectDestroyObjectBody FromFile(string fileName)
            {
                return new SetupObjectDestroyObjectBody(new KaitaiStream(fileName));
            }

            public SetupObjectDestroyObjectBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectId = m_io.ReadU4be();
            }
            private uint _objectId;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public uint ObjectId { get { return _objectId; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class PadNamesList : KaitaiStruct
        {
            public static PadNamesList FromFile(string fileName)
            {
                return new PadNamesList(new KaitaiStream(fileName));
            }

            public PadNamesList(KaitaiStream p__io, Setup.SectionBlock p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_type = false;
                _read();
            }
            private void _read()
            {
                _data = new List<StringPointer>();
                {
                    var i = 0;
                    StringPointer M_;
                    do
                    {
                        M_ = new StringPointer(m_io, this, m_root);
                        _data.Add(M_);
                        i++;
                    } while (!(M_.Offset == 0));
                }
            }
            private bool f_type;
            private SectionId _type;
            public SectionId Type
            {
                get
                {
                    if (f_type)
                        return _type;
                    _type = (SectionId)(Setup.SectionId.PadNames);
                    f_type = true;
                    return _type;
                }
            }
            private List<StringPointer> _data;
            private Setup m_root;
            private Setup.SectionBlock m_parent;
            public List<StringPointer> Data { get { return _data; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SectionBlock M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectiveThrowInRoomBody : KaitaiStruct
        {
            public static SetupObjectiveThrowInRoomBody FromFile(string fileName)
            {
                return new SetupObjectiveThrowInRoomBody(new KaitaiStream(fileName));
            }

            public SetupObjectiveThrowInRoomBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _weaponSlotIndex = m_io.ReadS4be();
                _preset = m_io.ReadS4be();
                _unknown08 = m_io.ReadS4be();
                _unknown0c = m_io.ReadS4be();
            }
            private int _weaponSlotIndex;
            private int _preset;
            private int _unknown08;
            private int _unknown0c;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public int WeaponSlotIndex { get { return _weaponSlotIndex; } }
            public int Preset { get { return _preset; } }
            public int Unknown08 { get { return _unknown08; } }
            public int Unknown0c { get { return _unknown0c; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupIntroStartWeaponBody : KaitaiStruct
        {
            public static SetupIntroStartWeaponBody FromFile(string fileName)
            {
                return new SetupIntroStartWeaponBody(new KaitaiStream(fileName));
            }

            public SetupIntroStartWeaponBody(KaitaiStream p__io, Setup.SetupIntroRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _right = m_io.ReadS4be();
                _left = m_io.ReadS4be();
                _setNum = m_io.ReadU4be();
            }
            private int _right;
            private int _left;
            private uint _setNum;
            private Setup m_root;
            private Setup.SetupIntroRecord m_parent;
            public int Right { get { return _right; } }
            public int Left { get { return _left; } }
            public uint SetNum { get { return _setNum; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupIntroRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectGuardBody : KaitaiStruct
        {
            public static SetupObjectGuardBody FromFile(string fileName)
            {
                return new SetupObjectGuardBody(new KaitaiStream(fileName));
            }

            public SetupObjectGuardBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectId = m_io.ReadU2be();
                _preset = m_io.ReadU2be();
                _bodyId = m_io.ReadU2be();
                _actionPathAssignment = m_io.ReadU2be();
                _presetToTrigger = m_io.ReadU4be();
                _unknown10 = m_io.ReadU2be();
                _health = m_io.ReadU2be();
                _reactionTime = m_io.ReadU2be();
                _head = m_io.ReadU2be();
                _pointerRuntimeData = m_io.ReadU4be();
            }
            private ushort _objectId;
            private ushort _preset;
            private ushort _bodyId;
            private ushort _actionPathAssignment;
            private uint _presetToTrigger;
            private ushort _unknown10;
            private ushort _health;
            private ushort _reactionTime;
            private ushort _head;
            private uint _pointerRuntimeData;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public ushort ObjectId { get { return _objectId; } }
            public ushort Preset { get { return _preset; } }
            public ushort BodyId { get { return _bodyId; } }
            public ushort ActionPathAssignment { get { return _actionPathAssignment; } }
            public uint PresetToTrigger { get { return _presetToTrigger; } }
            public ushort Unknown10 { get { return _unknown10; } }
            public ushort Health { get { return _health; } }
            public ushort ReactionTime { get { return _reactionTime; } }
            public ushort Head { get { return _head; } }
            public uint PointerRuntimeData { get { return _pointerRuntimeData; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class IntroList : KaitaiStruct
        {
            public static IntroList FromFile(string fileName)
            {
                return new IntroList(new KaitaiStream(fileName));
            }

            public IntroList(KaitaiStream p__io, Setup.SectionBlock p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_type = false;
                _read();
            }
            private void _read()
            {
                _data = new List<SetupIntroRecord>();
                {
                    var i = 0;
                    SetupIntroRecord M_;
                    do
                    {
                        M_ = new SetupIntroRecord(m_io, this, m_root);
                        _data.Add(M_);
                        i++;
                    } while (!(M_.Header.Type == Setup.Introdef.EndIntro));
                }
            }
            private bool f_type;
            private SectionId _type;
            public SectionId Type
            {
                get
                {
                    if (f_type)
                        return _type;
                    _type = (SectionId)(Setup.SectionId.IntroSection);
                    f_type = true;
                    return _type;
                }
            }
            private List<SetupIntroRecord> _data;
            private Setup m_root;
            private Setup.SectionBlock m_parent;
            public List<SetupIntroRecord> Data { get { return _data; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SectionBlock M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectAutogunBody : KaitaiStruct
        {
            public static SetupObjectAutogunBody FromFile(string fileName)
            {
                return new SetupObjectAutogunBody(new KaitaiStream(fileName));
            }

            public SetupObjectAutogunBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectBase = new SetupGenericObject(m_io, this, m_root);
                _bytes = m_io.ReadBytes(88);
            }
            private SetupGenericObject _objectBase;
            private byte[] _bytes;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public SetupGenericObject ObjectBase { get { return _objectBase; } }
            public byte[] Bytes { get { return _bytes; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class Coord3d : KaitaiStruct
        {
            public static Coord3d FromFile(string fileName)
            {
                return new Coord3d(new KaitaiStream(fileName));
            }

            public Coord3d(KaitaiStream p__io, KaitaiStruct p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _x = m_io.ReadF4be();
                _y = m_io.ReadF4be();
                _z = m_io.ReadF4be();
            }
            private float _x;
            private float _y;
            private float _z;
            private Setup m_root;
            private KaitaiStruct m_parent;
            public float X { get { return _x; } }
            public float Y { get { return _y; } }
            public float Z { get { return _z; } }
            public Setup M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class NotSupported : KaitaiStruct
        {
            public NotSupported(Propdef p_type, KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _type = p_type;
                _read();
            }
            private void _read()
            {
                _pos = m_io.ReadU4be();
                if (!(Pos == M_Io.Pos))
                {
                    throw new ValidationNotEqualError(M_Io.Pos, Pos, M_Io, "/types/not_supported/seq/0");
                }
            }
            private uint _pos;
            private Propdef _type;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public uint Pos { get { return _pos; } }
            public Propdef Type { get { return _type; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class PathSetEntry : KaitaiStruct
        {
            public static PathSetEntry FromFile(string fileName)
            {
                return new PathSetEntry(new KaitaiStream(fileName));
            }

            public PathSetEntry(KaitaiStream p__io, Setup.PathSetsSection p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_data = false;
                _read();
            }
            private void _read()
            {
                _pointer = m_io.ReadU4be();
                _pathId = m_io.ReadU1();
                _flags = m_io.ReadU1();
                _pathLen = m_io.ReadU2be();
            }
            private bool f_data;
            private List<FfListItem> _data;
            public List<FfListItem> Data
            {
                get
                {
                    if (f_data)
                        return _data;
                    if (Pointer > 0)
                    {
                        KaitaiStream io = M_Root.M_Io;
                        long _pos = io.Pos;
                        io.Seek(Pointer);
                        _data = new List<FfListItem>();
                        {
                            var i = 0;
                            FfListItem M_;
                            do
                            {
                                M_ = new FfListItem(io, this, m_root);
                                _data.Add(M_);
                                i++;
                            } while (!(M_.Value == 4294967295));
                        }
                        io.Seek(_pos);
                        f_data = true;
                    }
                    return _data;
                }
            }
            private uint _pointer;
            private byte _pathId;
            private byte _flags;
            private ushort _pathLen;
            private Setup m_root;
            private Setup.PathSetsSection m_parent;
            public uint Pointer { get { return _pointer; } }
            public byte PathId { get { return _pathId; } }
            public byte Flags { get { return _flags; } }
            public ushort PathLen { get { return _pathLen; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.PathSetsSection M_Parent { get { return m_parent; } }
        }
        public partial class SetupIntroHeaderData : KaitaiStruct
        {
            public static SetupIntroHeaderData FromFile(string fileName)
            {
                return new SetupIntroHeaderData(new KaitaiStream(fileName));
            }

            public SetupIntroHeaderData(KaitaiStream p__io, Setup.SetupIntroRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _extraScale = m_io.ReadU2be();
                _state = m_io.ReadU1();
                _type = ((Setup.Introdef)m_io.ReadU1());
            }
            private ushort _extraScale;
            private byte _state;
            private Introdef _type;
            private Setup m_root;
            private Setup.SetupIntroRecord m_parent;
            public ushort ExtraScale { get { return _extraScale; } }
            public byte State { get { return _state; } }
            public Introdef Type { get { return _type; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupIntroRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupIntroFixedCamBody : KaitaiStruct
        {
            public static SetupIntroFixedCamBody FromFile(string fileName)
            {
                return new SetupIntroFixedCamBody(new KaitaiStream(fileName));
            }

            public SetupIntroFixedCamBody(KaitaiStream p__io, Setup.SetupIntroRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _x = m_io.ReadU4be();
                _y = m_io.ReadU4be();
                _z = m_io.ReadU4be();
                _latRot = m_io.ReadU4be();
                _vertRot = m_io.ReadU4be();
                _preset = m_io.ReadU4be();
                _textId = m_io.ReadU4be();
                _text2Id = m_io.ReadU4be();
                _unknown20 = m_io.ReadU4be();
            }
            private uint _x;
            private uint _y;
            private uint _z;
            private uint _latRot;
            private uint _vertRot;
            private uint _preset;
            private uint _textId;
            private uint _text2Id;
            private uint _unknown20;
            private Setup m_root;
            private Setup.SetupIntroRecord m_parent;
            public uint X { get { return _x; } }
            public uint Y { get { return _y; } }
            public uint Z { get { return _z; } }
            public uint LatRot { get { return _latRot; } }
            public uint VertRot { get { return _vertRot; } }
            public uint Preset { get { return _preset; } }
            public uint TextId { get { return _textId; } }
            public uint Text2Id { get { return _text2Id; } }
            public uint Unknown20 { get { return _unknown20; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupIntroRecord M_Parent { get { return m_parent; } }
        }
        public partial class StringPointer : KaitaiStruct
        {
            public static StringPointer FromFile(string fileName)
            {
                return new StringPointer(new KaitaiStream(fileName));
            }

            public StringPointer(KaitaiStream p__io, KaitaiStruct p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_deref = false;
                _read();
            }
            private void _read()
            {
                _offset = m_io.ReadU4be();
            }
            private bool f_deref;
            private string _deref;
            public string Deref
            {
                get
                {
                    if (f_deref)
                        return _deref;
                    KaitaiStream io = M_Root.M_Io;
                    long _pos = io.Pos;
                    io.Seek(Offset);
                    _deref = System.Text.Encoding.GetEncoding("ASCII").GetString(io.ReadBytesTerm(0, false, true, true));
                    io.Seek(_pos);
                    f_deref = true;
                    return _deref;
                }
            }
            private uint _offset;
            private Setup m_root;
            private KaitaiStruct m_parent;
            public uint Offset { get { return _offset; } }
            public Setup M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectTagBody : KaitaiStruct
        {
            public static SetupObjectTagBody FromFile(string fileName)
            {
                return new SetupObjectTagBody(new KaitaiStream(fileName));
            }

            public SetupObjectTagBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _tagId = m_io.ReadU2be();
                _value = m_io.ReadU2be();
                _unknown08 = m_io.ReadU4be();
                _unknown0c = m_io.ReadU4be();
            }
            private ushort _tagId;
            private ushort _value;
            private uint _unknown08;
            private uint _unknown0c;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public ushort TagId { get { return _tagId; } }
            public ushort Value { get { return _value; } }
            public uint Unknown08 { get { return _unknown08; } }
            public uint Unknown0c { get { return _unknown0c; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectGlassBody : KaitaiStruct
        {
            public static SetupObjectGlassBody FromFile(string fileName)
            {
                return new SetupObjectGlassBody(new KaitaiStream(fileName));
            }

            public SetupObjectGlassBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectBase = new SetupGenericObject(m_io, this, m_root);
            }
            private SetupGenericObject _objectBase;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public SetupGenericObject ObjectBase { get { return _objectBase; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectSingleMonitorBody : KaitaiStruct
        {
            public static SetupObjectSingleMonitorBody FromFile(string fileName)
            {
                return new SetupObjectSingleMonitorBody(new KaitaiStream(fileName));
            }

            public SetupObjectSingleMonitorBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectBase = new SetupGenericObject(m_io, this, m_root);
                _curNumCmdsFromStartRotation = m_io.ReadU4be();
                _loopCounter = m_io.ReadU4be();
                _imgnumOrPtrheader = m_io.ReadU4be();
                _rotation = m_io.ReadU4be();
                _curHzoom = m_io.ReadU4be();
                _curHzoomTime = m_io.ReadU4be();
                _finalHzoomTime = m_io.ReadU4be();
                _initialHzoom = m_io.ReadU4be();
                _finalHzoom = m_io.ReadU4be();
                _curVzoom = m_io.ReadU4be();
                _curVzoomTime = m_io.ReadU4be();
                _finalVzoomTime = m_io.ReadU4be();
                _initialVzoom = m_io.ReadU4be();
                _finalVzoom = m_io.ReadU4be();
                _curHpos = m_io.ReadU4be();
                _curHscrollTime = m_io.ReadU4be();
                _finalHscrollTime = m_io.ReadU4be();
                _initialHpos = m_io.ReadU4be();
                _finalHpos = m_io.ReadU4be();
                _curVpos = m_io.ReadU4be();
                _curVscrollTime = m_io.ReadU4be();
                _finalVscrollTime = m_io.ReadU4be();
                _initialVpos = m_io.ReadU4be();
                _finalVpos = m_io.ReadU4be();
                _curRed = m_io.ReadU1();
                _initialRed = m_io.ReadU1();
                _finalRed = m_io.ReadU1();
                _curGreen = m_io.ReadU1();
                _initialGreen = m_io.ReadU1();
                _finalGreen = m_io.ReadU1();
                _curBlue = m_io.ReadU1();
                _initialBlue = m_io.ReadU1();
                _finalBlue = m_io.ReadU1();
                _curAlpha = m_io.ReadU1();
                _initialAlpha = m_io.ReadU1();
                _finalAlpha = m_io.ReadU1();
                _curColorTransitionTime = m_io.ReadU4be();
                _finalColorTransitionTime = m_io.ReadU4be();
                _backwardMonLink = m_io.ReadU4be();
                _forwardMonLink = m_io.ReadU4be();
                _animationNum = m_io.ReadU4be();
            }
            private SetupGenericObject _objectBase;
            private uint _curNumCmdsFromStartRotation;
            private uint _loopCounter;
            private uint _imgnumOrPtrheader;
            private uint _rotation;
            private uint _curHzoom;
            private uint _curHzoomTime;
            private uint _finalHzoomTime;
            private uint _initialHzoom;
            private uint _finalHzoom;
            private uint _curVzoom;
            private uint _curVzoomTime;
            private uint _finalVzoomTime;
            private uint _initialVzoom;
            private uint _finalVzoom;
            private uint _curHpos;
            private uint _curHscrollTime;
            private uint _finalHscrollTime;
            private uint _initialHpos;
            private uint _finalHpos;
            private uint _curVpos;
            private uint _curVscrollTime;
            private uint _finalVscrollTime;
            private uint _initialVpos;
            private uint _finalVpos;
            private byte _curRed;
            private byte _initialRed;
            private byte _finalRed;
            private byte _curGreen;
            private byte _initialGreen;
            private byte _finalGreen;
            private byte _curBlue;
            private byte _initialBlue;
            private byte _finalBlue;
            private byte _curAlpha;
            private byte _initialAlpha;
            private byte _finalAlpha;
            private uint _curColorTransitionTime;
            private uint _finalColorTransitionTime;
            private uint _backwardMonLink;
            private uint _forwardMonLink;
            private uint _animationNum;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public SetupGenericObject ObjectBase { get { return _objectBase; } }
            public uint CurNumCmdsFromStartRotation { get { return _curNumCmdsFromStartRotation; } }
            public uint LoopCounter { get { return _loopCounter; } }
            public uint ImgnumOrPtrheader { get { return _imgnumOrPtrheader; } }
            public uint Rotation { get { return _rotation; } }
            public uint CurHzoom { get { return _curHzoom; } }
            public uint CurHzoomTime { get { return _curHzoomTime; } }
            public uint FinalHzoomTime { get { return _finalHzoomTime; } }
            public uint InitialHzoom { get { return _initialHzoom; } }
            public uint FinalHzoom { get { return _finalHzoom; } }
            public uint CurVzoom { get { return _curVzoom; } }
            public uint CurVzoomTime { get { return _curVzoomTime; } }
            public uint FinalVzoomTime { get { return _finalVzoomTime; } }
            public uint InitialVzoom { get { return _initialVzoom; } }
            public uint FinalVzoom { get { return _finalVzoom; } }
            public uint CurHpos { get { return _curHpos; } }
            public uint CurHscrollTime { get { return _curHscrollTime; } }
            public uint FinalHscrollTime { get { return _finalHscrollTime; } }
            public uint InitialHpos { get { return _initialHpos; } }
            public uint FinalHpos { get { return _finalHpos; } }
            public uint CurVpos { get { return _curVpos; } }
            public uint CurVscrollTime { get { return _curVscrollTime; } }
            public uint FinalVscrollTime { get { return _finalVscrollTime; } }
            public uint InitialVpos { get { return _initialVpos; } }
            public uint FinalVpos { get { return _finalVpos; } }
            public byte CurRed { get { return _curRed; } }
            public byte InitialRed { get { return _initialRed; } }
            public byte FinalRed { get { return _finalRed; } }
            public byte CurGreen { get { return _curGreen; } }
            public byte InitialGreen { get { return _initialGreen; } }
            public byte FinalGreen { get { return _finalGreen; } }
            public byte CurBlue { get { return _curBlue; } }
            public byte InitialBlue { get { return _initialBlue; } }
            public byte FinalBlue { get { return _finalBlue; } }
            public byte CurAlpha { get { return _curAlpha; } }
            public byte InitialAlpha { get { return _initialAlpha; } }
            public byte FinalAlpha { get { return _finalAlpha; } }
            public uint CurColorTransitionTime { get { return _curColorTransitionTime; } }
            public uint FinalColorTransitionTime { get { return _finalColorTransitionTime; } }
            public uint BackwardMonLink { get { return _backwardMonLink; } }
            public uint ForwardMonLink { get { return _forwardMonLink; } }
            public uint AnimationNum { get { return _animationNum; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectRenameBody : KaitaiStruct
        {
            public static SetupObjectRenameBody FromFile(string fileName)
            {
                return new SetupObjectRenameBody(new KaitaiStream(fileName));
            }

            public SetupObjectRenameBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectOffset = m_io.ReadU4be();
                _inventoryId = m_io.ReadU4be();
                _text1 = m_io.ReadU4be();
                _text2 = m_io.ReadU4be();
                _text3 = m_io.ReadU4be();
                _text4 = m_io.ReadU4be();
                _text5 = m_io.ReadU4be();
                _unknown20 = m_io.ReadU4be();
                _unknown24 = m_io.ReadU4be();
            }
            private uint _objectOffset;
            private uint _inventoryId;
            private uint _text1;
            private uint _text2;
            private uint _text3;
            private uint _text4;
            private uint _text5;
            private uint _unknown20;
            private uint _unknown24;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public uint ObjectOffset { get { return _objectOffset; } }
            public uint InventoryId { get { return _inventoryId; } }
            public uint Text1 { get { return _text1; } }
            public uint Text2 { get { return _text2; } }
            public uint Text3 { get { return _text3; } }
            public uint Text4 { get { return _text4; } }
            public uint Text5 { get { return _text5; } }
            public uint Unknown20 { get { return _unknown20; } }
            public uint Unknown24 { get { return _unknown24; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupIntroWatchTimeBody : KaitaiStruct
        {
            public static SetupIntroWatchTimeBody FromFile(string fileName)
            {
                return new SetupIntroWatchTimeBody(new KaitaiStream(fileName));
            }

            public SetupIntroWatchTimeBody(KaitaiStream p__io, Setup.SetupIntroRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _hour = m_io.ReadU4be();
                _minute = m_io.ReadU4be();
            }
            private uint _hour;
            private uint _minute;
            private Setup m_root;
            private Setup.SetupIntroRecord m_parent;
            public uint Hour { get { return _hour; } }
            public uint Minute { get { return _minute; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupIntroRecord M_Parent { get { return m_parent; } }
        }
        public partial class PathTableEntry : KaitaiStruct
        {
            public static PathTableEntry FromFile(string fileName)
            {
                return new PathTableEntry(new KaitaiStream(fileName));
            }

            public PathTableEntry(KaitaiStream p__io, Setup.PathTableSection p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_data = false;
                _read();
            }
            private void _read()
            {
                _padId = m_io.ReadU4be();
                _neighbors = m_io.ReadU4be();
                _groupnum = m_io.ReadU4be();
                _dist = m_io.ReadU4be();
            }
            private bool f_data;
            private List<FfListItem> _data;
            public List<FfListItem> Data
            {
                get
                {
                    if (f_data)
                        return _data;
                    if (Neighbors > 0)
                    {
                        KaitaiStream io = M_Root.M_Io;
                        long _pos = io.Pos;
                        io.Seek(Neighbors);
                        _data = new List<FfListItem>();
                        {
                            var i = 0;
                            FfListItem M_;
                            do
                            {
                                M_ = new FfListItem(io, this, m_root);
                                _data.Add(M_);
                                i++;
                            } while (!(M_.Value == 4294967295));
                        }
                        io.Seek(_pos);
                        f_data = true;
                    }
                    return _data;
                }
            }
            private uint _padId;
            private uint _neighbors;
            private uint _groupnum;
            private uint _dist;
            private Setup m_root;
            private Setup.PathTableSection m_parent;
            public uint PadId { get { return _padId; } }
            public uint Neighbors { get { return _neighbors; } }
            public uint Groupnum { get { return _groupnum; } }
            public uint Dist { get { return _dist; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.PathTableSection M_Parent { get { return m_parent; } }
        }
        public partial class Pad3d : KaitaiStruct
        {
            public static Pad3d FromFile(string fileName)
            {
                return new Pad3d(new KaitaiStream(fileName));
            }

            public Pad3d(KaitaiStream p__io, Setup.Pad3dList p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _pos = new Coord3d(m_io, this, m_root);
                _up = new Coord3d(m_io, this, m_root);
                _look = new Coord3d(m_io, this, m_root);
                _plink = new StringPointer(m_io, this, m_root);
                _unknown = m_io.ReadU4be();
                _bbox = new Bbox(m_io, this, m_root);
            }
            private Coord3d _pos;
            private Coord3d _up;
            private Coord3d _look;
            private StringPointer _plink;
            private uint _unknown;
            private Bbox _bbox;
            private Setup m_root;
            private Setup.Pad3dList m_parent;
            public Coord3d Pos { get { return _pos; } }
            public Coord3d Up { get { return _up; } }
            public Coord3d Look { get { return _look; } }
            public StringPointer Plink { get { return _plink; } }
            public uint Unknown { get { return _unknown; } }
            public Bbox Bbox { get { return _bbox; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.Pad3dList M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectLinkItemsBody : KaitaiStruct
        {
            public static SetupObjectLinkItemsBody FromFile(string fileName)
            {
                return new SetupObjectLinkItemsBody(new KaitaiStream(fileName));
            }

            public SetupObjectLinkItemsBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _offset1 = m_io.ReadS4be();
                _offset2 = m_io.ReadS4be();
            }
            private int _offset1;
            private int _offset2;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public int Offset1 { get { return _offset1; } }
            public int Offset2 { get { return _offset2; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class PadList : KaitaiStruct
        {
            public static PadList FromFile(string fileName)
            {
                return new PadList(new KaitaiStream(fileName));
            }

            public PadList(KaitaiStream p__io, Setup.SectionBlock p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_type = false;
                _read();
            }
            private void _read()
            {
                _data = new List<Pad>();
                {
                    var i = 0;
                    Pad M_;
                    do
                    {
                        M_ = new Pad(m_io, this, m_root);
                        _data.Add(M_);
                        i++;
                    } while (!(M_.Plink.Offset == 0));
                }
            }
            private bool f_type;
            private SectionId _type;
            public SectionId Type
            {
                get
                {
                    if (f_type)
                        return _type;
                    _type = (SectionId)(Setup.SectionId.PadSection);
                    f_type = true;
                    return _type;
                }
            }
            private List<Pad> _data;
            private Setup m_root;
            private Setup.SectionBlock m_parent;
            public List<Pad> Data { get { return _data; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SectionBlock M_Parent { get { return m_parent; } }
        }
        public partial class FfListItem : KaitaiStruct
        {
            public static FfListItem FromFile(string fileName)
            {
                return new FfListItem(new KaitaiStream(fileName));
            }

            public FfListItem(KaitaiStream p__io, KaitaiStruct p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _value = m_io.ReadU4be();
            }
            private uint _value;
            private Setup m_root;
            private KaitaiStruct m_parent;
            public uint Value { get { return _value; } }
            public Setup M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectObjectiveFailConditionBody : KaitaiStruct
        {
            public static SetupObjectObjectiveFailConditionBody FromFile(string fileName)
            {
                return new SetupObjectObjectiveFailConditionBody(new KaitaiStream(fileName));
            }

            public SetupObjectObjectiveFailConditionBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _testval = m_io.ReadU4be();
            }
            private uint _testval;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public uint Testval { get { return _testval; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectSafeItemBody : KaitaiStruct
        {
            public static SetupObjectSafeItemBody FromFile(string fileName)
            {
                return new SetupObjectSafeItemBody(new KaitaiStream(fileName));
            }

            public SetupObjectSafeItemBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _item = m_io.ReadS4be();
                _safe = m_io.ReadS4be();
                _door = m_io.ReadS4be();
                _empty = m_io.ReadU4be();
            }
            private int _item;
            private int _safe;
            private int _door;
            private uint _empty;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public int Item { get { return _item; } }
            public int Safe { get { return _safe; } }
            public int Door { get { return _door; } }
            public uint Empty { get { return _empty; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupIntroCuffBody : KaitaiStruct
        {
            public static SetupIntroCuffBody FromFile(string fileName)
            {
                return new SetupIntroCuffBody(new KaitaiStream(fileName));
            }

            public SetupIntroCuffBody(KaitaiStream p__io, Setup.SetupIntroRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _cuffId = m_io.ReadU4be();
            }
            private uint _cuffId;
            private Setup m_root;
            private Setup.SetupIntroRecord m_parent;
            public uint CuffId { get { return _cuffId; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupIntroRecord M_Parent { get { return m_parent; } }
        }
        public partial class SetupObjectBodyArmorBody : KaitaiStruct
        {
            public static SetupObjectBodyArmorBody FromFile(string fileName)
            {
                return new SetupObjectBodyArmorBody(new KaitaiStream(fileName));
            }

            public SetupObjectBodyArmorBody(KaitaiStream p__io, Setup.SetupObjectRecord p__parent = null, Setup p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _objectBase = new SetupGenericObject(m_io, this, m_root);
                _armorStrength = m_io.ReadS4be();
                _armorPercent = m_io.ReadS4be();
            }
            private SetupGenericObject _objectBase;
            private int _armorStrength;
            private int _armorPercent;
            private Setup m_root;
            private Setup.SetupObjectRecord m_parent;
            public SetupGenericObject ObjectBase { get { return _objectBase; } }
            public int ArmorStrength { get { return _armorStrength; } }
            public int ArmorPercent { get { return _armorPercent; } }
            public Setup M_Root { get { return m_root; } }
            public Setup.SetupObjectRecord M_Parent { get { return m_parent; } }
        }
        private StageSetupStruct _pointers;
        private List<SectionBlock> _contents;
        private Setup m_root;
        private KaitaiStruct m_parent;
        public StageSetupStruct Pointers { get { return _pointers; } }
        public List<SectionBlock> Contents { get { return _contents; } }
        public Setup M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}
