// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

/*
 * Changes to auto generated code:
 *
 * - Add pragmas.
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
    public partial class StanBeta : KaitaiStruct
    {
        public static StanBeta FromFile(string fileName)
        {
            return new StanBeta(new KaitaiStream(fileName));
        }

        public StanBeta(KaitaiStream p__io, KaitaiStruct p__parent = null, StanBeta p__root = null) : base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root ?? this;
            _read();
        }
        private void _read()
        {
            _headerBlock = new StandFileHeader(m_io, this, m_root);
            _tiles = new List<BetaStandTile>();
            {
                var i = 0;
                BetaStandTile M_;
                do {
                    M_ = new BetaStandTile(m_io, this, m_root);
                    _tiles.Add(M_);
                    i++;
                } while (!(M_.DebugName.Offset == 0));
            }
            _footer = new StandTileFooter(m_io, this, m_root);
        }
        public partial class BetaStandTileTail : KaitaiStruct
        {
            public static BetaStandTileTail FromFile(string fileName)
            {
                return new BetaStandTileTail(new KaitaiStream(fileName));
            }

            public BetaStandTileTail(KaitaiStream p__io, StanBeta.BetaStandTile p__parent = null, StanBeta p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _pointCount = m_io.ReadU1();
                _firstPoint = m_io.ReadU1();
                _secondPoint = m_io.ReadU1();
                _thirdPoint = m_io.ReadU1();
                _points = new List<BetaStandTilePoint>((int) (PointCount));
                for (var i = 0; i < PointCount; i++)
                {
                    _points.Add(new BetaStandTilePoint(m_io, this, m_root));
                }
            }
            private byte _pointCount;
            private byte _firstPoint;
            private byte _secondPoint;
            private byte _thirdPoint;
            private List<BetaStandTilePoint> _points;
            private StanBeta m_root;
            private StanBeta.BetaStandTile m_parent;
            public byte PointCount { get { return _pointCount; } }
            public byte FirstPoint { get { return _firstPoint; } }
            public byte SecondPoint { get { return _secondPoint; } }
            public byte ThirdPoint { get { return _thirdPoint; } }
            public List<BetaStandTilePoint> Points { get { return _points; } }
            public StanBeta M_Root { get { return m_root; } }
            public StanBeta.BetaStandTile M_Parent { get { return m_parent; } }
        }
        public partial class BetaStandTilePoint : KaitaiStruct
        {
            public static BetaStandTilePoint FromFile(string fileName)
            {
                return new BetaStandTilePoint(new KaitaiStream(fileName));
            }

            public BetaStandTilePoint(KaitaiStream p__io, StanBeta.BetaStandTileTail p__parent = null, StanBeta p__root = null) : base(p__io)
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
                _link = m_io.ReadU4be();
            }
            private float _x;
            private float _y;
            private float _z;
            private uint _link;
            private StanBeta m_root;
            private StanBeta.BetaStandTileTail m_parent;
            public float X { get { return _x; } }
            public float Y { get { return _y; } }
            public float Z { get { return _z; } }
            public uint Link { get { return _link; } }
            public StanBeta M_Root { get { return m_root; } }
            public StanBeta.BetaStandTileTail M_Parent { get { return m_parent; } }
        }
        public partial class StandFileHeader : KaitaiStruct
        {
            public static StandFileHeader FromFile(string fileName)
            {
                return new StandFileHeader(new KaitaiStream(fileName));
            }

            public StandFileHeader(KaitaiStream p__io, StanBeta p__parent = null, StanBeta p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _unknown1 = m_io.ReadU4be();
                _tileOffset = m_io.ReadU4be();
                _unknownHeaderData = m_io.ReadBytes((TileOffset - M_Io.Pos));
            }
            private uint _unknown1;
            private uint _tileOffset;
            private byte[] _unknownHeaderData;
            private StanBeta m_root;
            private StanBeta m_parent;
            public uint Unknown1 { get { return _unknown1; } }
            public uint TileOffset { get { return _tileOffset; } }
            public byte[] UnknownHeaderData { get { return _unknownHeaderData; } }
            public StanBeta M_Root { get { return m_root; } }
            public StanBeta M_Parent { get { return m_parent; } }
        }
        public partial class StandTileFooter : KaitaiStruct
        {
            public static StandTileFooter FromFile(string fileName)
            {
                return new StandTileFooter(new KaitaiStream(fileName));
            }

            public StandTileFooter(KaitaiStream p__io, StanBeta p__parent = null, StanBeta p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _unstric = System.Text.Encoding.GetEncoding("ASCII").GetString(m_io.ReadBytesTerm(0, false, true, true));
                _stringPad = m_io.ReadBytes(KaitaiStream.Mod((4 - M_Io.Pos), 4));
                _unknown3 = m_io.ReadU4be();
                _unknown4 = m_io.ReadU4be();
                _unknown5 = m_io.ReadU4be();
                _unknownRemaining = m_io.ReadBytesFull();
            }
            private string _unstric;
            private byte[] _stringPad;
            private uint _unknown3;
            private uint _unknown4;
            private uint _unknown5;
            private byte[] _unknownRemaining;
            private StanBeta m_root;
            private StanBeta m_parent;
            public string Unstric { get { return _unstric; } }
            public byte[] StringPad { get { return _stringPad; } }
            public uint Unknown3 { get { return _unknown3; } }
            public uint Unknown4 { get { return _unknown4; } }
            public uint Unknown5 { get { return _unknown5; } }
            public byte[] UnknownRemaining { get { return _unknownRemaining; } }
            public StanBeta M_Root { get { return m_root; } }
            public StanBeta M_Parent { get { return m_parent; } }
        }
        public partial class BetaStandTile : KaitaiStruct
        {
            public static BetaStandTile FromFile(string fileName)
            {
                return new BetaStandTile(new KaitaiStream(fileName));
            }

            public BetaStandTile(KaitaiStream p__io, StanBeta p__parent = null, StanBeta p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _debugName = new StringPointer(m_io, this, m_root);
                _flags = m_io.ReadBitsIntBe(4);
                _red = m_io.ReadBitsIntBe(4);
                _green = m_io.ReadBitsIntBe(4);
                _blue = m_io.ReadBitsIntBe(4);
                m_io.AlignToByte();
                _betaUnknown = m_io.ReadU2be();
                if (DebugName.Offset > 0) {
                    _tail = new BetaStandTileTail(m_io, this, m_root);
                }
            }
            private StringPointer _debugName;
            private ulong _flags;
            private ulong _red;
            private ulong _green;
            private ulong _blue;
            private ushort _betaUnknown;
            private BetaStandTileTail _tail;
            private StanBeta m_root;
            private StanBeta m_parent;
            public StringPointer DebugName { get { return _debugName; } }
            public ulong Flags { get { return _flags; } }
            public ulong Red { get { return _red; } }
            public ulong Green { get { return _green; } }
            public ulong Blue { get { return _blue; } }
            public ushort BetaUnknown { get { return _betaUnknown; } }
            public BetaStandTileTail Tail { get { return _tail; } }
            public StanBeta M_Root { get { return m_root; } }
            public StanBeta M_Parent { get { return m_parent; } }
        }
        public partial class StringPointer : KaitaiStruct
        {
            public static StringPointer FromFile(string fileName)
            {
                return new StringPointer(new KaitaiStream(fileName));
            }

            public StringPointer(KaitaiStream p__io, StanBeta.BetaStandTile p__parent = null, StanBeta p__root = null) : base(p__io)
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
            private StanBeta m_root;
            private StanBeta.BetaStandTile m_parent;
            public uint Offset { get { return _offset; } }
            public StanBeta M_Root { get { return m_root; } }
            public StanBeta.BetaStandTile M_Parent { get { return m_parent; } }
        }
        private StandFileHeader _headerBlock;
        private List<BetaStandTile> _tiles;
        private StandTileFooter _footer;
        private StanBeta m_root;
        private KaitaiStruct m_parent;
        public StandFileHeader HeaderBlock { get { return _headerBlock; } }
        public List<BetaStandTile> Tiles { get { return _tiles; } }
        public StandTileFooter Footer { get { return _footer; } }
        public StanBeta M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}
