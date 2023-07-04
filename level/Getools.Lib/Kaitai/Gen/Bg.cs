// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using Kaitai;
using System.Collections.Generic;

namespace Getools.Lib.Kaitai.Gen
{
    public partial class Bg : KaitaiStruct
    {
        public static Bg FromFile(string fileName)
        {
            return new Bg(new KaitaiStream(fileName));
        }

        public Bg(KaitaiStream p__io, KaitaiStruct p__parent = null, Bg p__root = null) : base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root ?? this;
            _read();
        }
        private void _read()
        {
            _headerBlock = new BgFileHeader(m_io, this, m_root);
        }
        public partial class BgFilePortalDataEntry : KaitaiStruct
        {
            public static BgFilePortalDataEntry FromFile(string fileName)
            {
                return new BgFilePortalDataEntry(new KaitaiStream(fileName));
            }

            public BgFilePortalDataEntry(KaitaiStream p__io, Bg.BgFileHeader p__parent = null, Bg p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_portal = false;
                _read();
            }
            private void _read()
            {
                _portalPointer = m_io.ReadU4be();
                _connectedRoom1 = m_io.ReadU1();
                _connectedRoom2 = m_io.ReadU1();
                _controlFlags = m_io.ReadU2be();
            }
            private bool f_portal;
            private BgFilePortal _portal;
            public BgFilePortal Portal
            {
                get
                {
                    if (f_portal)
                        return _portal;
                    KaitaiStream io = M_Root.M_Io;
                    long _pos = io.Pos;
                    io.Seek((PortalPointer & 16777215));
                    _portal = new BgFilePortal(io, this, m_root);
                    io.Seek(_pos);
                    f_portal = true;
                    return _portal;
                }
            }
            private uint _portalPointer;
            private byte _connectedRoom1;
            private byte _connectedRoom2;
            private ushort _controlFlags;
            private Bg m_root;
            private Bg.BgFileHeader m_parent;
            public uint PortalPointer { get { return _portalPointer; } }
            public byte ConnectedRoom1 { get { return _connectedRoom1; } }
            public byte ConnectedRoom2 { get { return _connectedRoom2; } }
            public ushort ControlFlags { get { return _controlFlags; } }
            public Bg M_Root { get { return m_root; } }
            public Bg.BgFileHeader M_Parent { get { return m_parent; } }
        }
        public partial class BgFileHeader : KaitaiStruct
        {
            public static BgFileHeader FromFile(string fileName)
            {
                return new BgFileHeader(new KaitaiStream(fileName));
            }

            public BgFileHeader(KaitaiStream p__io, Bg p__parent = null, Bg p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_roomDataTableIgnore = false;
                f_roomDataTable = false;
                f_globalVisibilityCommands = false;
                f_portalDataTable = false;
                _read();
            }
            private void _read()
            {
                _unknown1 = m_io.ReadU4be();
                _roomDataTablePointer = m_io.ReadU4be();
                _portalDataTablePointer = m_io.ReadU4be();
                _globalVisibilityCommandsPointer = m_io.ReadU4be();
                _unknown2 = m_io.ReadU4be();
            }
            private bool f_roomDataTableIgnore;
            private List<BgFileRoomDataEntry> _roomDataTableIgnore;
            public List<BgFileRoomDataEntry> RoomDataTableIgnore
            {
                get
                {
                    if (f_roomDataTableIgnore)
                        return _roomDataTableIgnore;
                    KaitaiStream io = M_Root.M_Io;
                    long _pos = io.Pos;
                    io.Seek((RoomDataTablePointer & 16777215));
                    _roomDataTableIgnore = new List<BgFileRoomDataEntry>();
                    {
                        var i = 0;
                        BgFileRoomDataEntry M_;
                        do
                        {
                            M_ = new BgFileRoomDataEntry(io, this, m_root);
                            _roomDataTableIgnore.Add(M_);
                            i++;
                        } while (!(M_.PointTablePointer == 0));
                    }
                    io.Seek(_pos);
                    f_roomDataTableIgnore = true;
                    return _roomDataTableIgnore;
                }
            }
            private bool f_roomDataTable;
            private List<BgFileRoomDataEntry> _roomDataTable;
            public List<BgFileRoomDataEntry> RoomDataTable
            {
                get
                {
                    if (f_roomDataTable)
                        return _roomDataTable;
                    KaitaiStream io = M_Root.M_Io;
                    long _pos = io.Pos;
                    io.Seek(((RoomDataTablePointer & 16777215) + 24));
                    _roomDataTable = new List<BgFileRoomDataEntry>();
                    {
                        var i = 0;
                        BgFileRoomDataEntry M_;
                        do
                        {
                            M_ = new BgFileRoomDataEntry(io, this, m_root);
                            _roomDataTable.Add(M_);
                            i++;
                        } while (!(((M_.PointTablePointer == 0) && (M_.PrimaryDisplayListPointer == 0))));
                    }
                    io.Seek(_pos);
                    f_roomDataTable = true;
                    return _roomDataTable;
                }
            }
            private bool f_globalVisibilityCommands;
            private List<VisibilityCommand> _globalVisibilityCommands;
            public List<VisibilityCommand> GlobalVisibilityCommands
            {
                get
                {
                    if (f_globalVisibilityCommands)
                        return _globalVisibilityCommands;
                    KaitaiStream io = M_Root.M_Io;
                    long _pos = io.Pos;
                    io.Seek((GlobalVisibilityCommandsPointer & 16777215));
                    _globalVisibilityCommands = new List<VisibilityCommand>();
                    {
                        var i = 0;
                        VisibilityCommand M_;
                        do
                        {
                            M_ = new VisibilityCommand(io, this, m_root);
                            _globalVisibilityCommands.Add(M_);
                            i++;
                        } while (!(M_Root.M_Io.Pos == (PortalDataTablePointer & 16777215)));
                    }
                    io.Seek(_pos);
                    f_globalVisibilityCommands = true;
                    return _globalVisibilityCommands;
                }
            }
            private bool f_portalDataTable;
            private List<BgFilePortalDataEntry> _portalDataTable;
            public List<BgFilePortalDataEntry> PortalDataTable
            {
                get
                {
                    if (f_portalDataTable)
                        return _portalDataTable;
                    KaitaiStream io = M_Root.M_Io;
                    long _pos = io.Pos;
                    io.Seek((PortalDataTablePointer & 16777215));
                    _portalDataTable = new List<BgFilePortalDataEntry>();
                    {
                        var i = 0;
                        BgFilePortalDataEntry M_;
                        do
                        {
                            M_ = new BgFilePortalDataEntry(io, this, m_root);
                            _portalDataTable.Add(M_);
                            i++;
                        } while (!(M_.PortalPointer == 0));
                    }
                    io.Seek(_pos);
                    f_portalDataTable = true;
                    return _portalDataTable;
                }
            }
            private uint _unknown1;
            private uint _roomDataTablePointer;
            private uint _portalDataTablePointer;
            private uint _globalVisibilityCommandsPointer;
            private uint _unknown2;
            private Bg m_root;
            private Bg m_parent;
            public uint Unknown1 { get { return _unknown1; } }
            public uint RoomDataTablePointer { get { return _roomDataTablePointer; } }
            public uint PortalDataTablePointer { get { return _portalDataTablePointer; } }
            public uint GlobalVisibilityCommandsPointer { get { return _globalVisibilityCommandsPointer; } }
            public uint Unknown2 { get { return _unknown2; } }
            public Bg M_Root { get { return m_root; } }
            public Bg M_Parent { get { return m_parent; } }
        }
        public partial class BgFileRoomDataEntry : KaitaiStruct
        {
            public static BgFileRoomDataEntry FromFile(string fileName)
            {
                return new BgFileRoomDataEntry(new KaitaiStream(fileName));
            }

            public BgFileRoomDataEntry(KaitaiStream p__io, Bg.BgFileHeader p__parent = null, Bg p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _pointTablePointer = m_io.ReadU4be();
                _primaryDisplayListPointer = m_io.ReadU4be();
                _secondaryDisplayListPointer = m_io.ReadU4be();
                _coord = new Coord3d(m_io, this, m_root);
            }
            private uint _pointTablePointer;
            private uint _primaryDisplayListPointer;
            private uint _secondaryDisplayListPointer;
            private Coord3d _coord;
            private Bg m_root;
            private Bg.BgFileHeader m_parent;
            public uint PointTablePointer { get { return _pointTablePointer; } }
            public uint PrimaryDisplayListPointer { get { return _primaryDisplayListPointer; } }
            public uint SecondaryDisplayListPointer { get { return _secondaryDisplayListPointer; } }
            public Coord3d Coord { get { return _coord; } }
            public Bg M_Root { get { return m_root; } }
            public Bg.BgFileHeader M_Parent { get { return m_parent; } }
        }
        public partial class BgFilePortal : KaitaiStruct
        {
            public static BgFilePortal FromFile(string fileName)
            {
                return new BgFilePortal(new KaitaiStream(fileName));
            }

            public BgFilePortal(KaitaiStream p__io, Bg.BgFilePortalDataEntry p__parent = null, Bg p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _numberPoints = m_io.ReadU1();
                _padding = new List<byte>((int)(3));
                for (var i = 0; i < 3; i++)
                {
                    _padding.Add(m_io.ReadU1());
                }
                _points = new List<Coord3d>((int)(NumberPoints));
                for (var i = 0; i < NumberPoints; i++)
                {
                    _points.Add(new Coord3d(m_io, this, m_root));
                }
            }
            private byte _numberPoints;
            private List<byte> _padding;
            private List<Coord3d> _points;
            private Bg m_root;
            private Bg.BgFilePortalDataEntry m_parent;
            public byte NumberPoints { get { return _numberPoints; } }
            public List<byte> Padding { get { return _padding; } }
            public List<Coord3d> Points { get { return _points; } }
            public Bg M_Root { get { return m_root; } }
            public Bg.BgFilePortalDataEntry M_Parent { get { return m_parent; } }
        }
        public partial class Coord3d : KaitaiStruct
        {
            public static Coord3d FromFile(string fileName)
            {
                return new Coord3d(new KaitaiStream(fileName));
            }

            public Coord3d(KaitaiStream p__io, KaitaiStruct p__parent = null, Bg p__root = null) : base(p__io)
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
            private Bg m_root;
            private KaitaiStruct m_parent;
            public float X { get { return _x; } }
            public float Y { get { return _y; } }
            public float Z { get { return _z; } }
            public Bg M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class VisibilityCommand : KaitaiStruct
        {
            public static VisibilityCommand FromFile(string fileName)
            {
                return new VisibilityCommand(new KaitaiStream(fileName));
            }

            public VisibilityCommand(KaitaiStream p__io, Bg.BgFileHeader p__parent = null, Bg p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _command = m_io.ReadU4be();
            }
            private uint _command;
            private Bg m_root;
            private Bg.BgFileHeader m_parent;
            public uint Command { get { return _command; } }
            public Bg M_Root { get { return m_root; } }
            public Bg.BgFileHeader M_Parent { get { return m_parent; } }
        }
        private BgFileHeader _headerBlock;
        private Bg m_root;
        private KaitaiStruct m_parent;
        public BgFileHeader HeaderBlock { get { return _headerBlock; } }
        public Bg M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}
