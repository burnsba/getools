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
    public partial class Stan : KaitaiStruct
    {
        public static Stan FromFile(string fileName)
        {
            return new Stan(new KaitaiStream(fileName));
        }

        public Stan(KaitaiStream p__io, KaitaiStruct p__parent = null, Stan p__root = null) : base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root ?? this;
            _read();
        }
        private void _read()
        {
            _headerBlock = new StandFileHeader(m_io, this, m_root);
            _tiles = new List<StandTile>();
            {
                var i = 0;
                StandTile M_;
                do
                {
                    M_ = new StandTile(m_io, this, m_root);
                    _tiles.Add(M_);
                    i++;
                } while (!(((M_.InternalName == 0) && (M_.PointCount == 0))));
            }
            _footer = new StandTileFooter(m_io, this, m_root);
        }
        public partial class StandFileHeader : KaitaiStruct
        {
            public static StandFileHeader FromFile(string fileName)
            {
                return new StandFileHeader(new KaitaiStream(fileName));
            }

            public StandFileHeader(KaitaiStream p__io, Stan p__parent = null, Stan p__root = null) : base(p__io)
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
            private Stan m_root;
            private Stan m_parent;
            public uint Unknown1 { get { return _unknown1; } }
            public uint TileOffset { get { return _tileOffset; } }
            public byte[] UnknownHeaderData { get { return _unknownHeaderData; } }
            public Stan M_Root { get { return m_root; } }
            public Stan M_Parent { get { return m_parent; } }
        }
        public partial class StandTilePoint : KaitaiStruct
        {
            public static StandTilePoint FromFile(string fileName)
            {
                return new StandTilePoint(new KaitaiStream(fileName));
            }

            public StandTilePoint(KaitaiStream p__io, Stan.StandTile p__parent = null, Stan p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _x = m_io.ReadS2be();
                _y = m_io.ReadS2be();
                _z = m_io.ReadS2be();
                _link = m_io.ReadU2be();
            }
            private short _x;
            private short _y;
            private short _z;
            private ushort _link;
            private Stan m_root;
            private Stan.StandTile m_parent;
            public short X { get { return _x; } }
            public short Y { get { return _y; } }
            public short Z { get { return _z; } }
            public ushort Link { get { return _link; } }
            public Stan M_Root { get { return m_root; } }
            public Stan.StandTile M_Parent { get { return m_parent; } }
        }
        public partial class StandTile : KaitaiStruct
        {
            public static StandTile FromFile(string fileName)
            {
                return new StandTile(new KaitaiStream(fileName));
            }

            public StandTile(KaitaiStream p__io, Stan p__parent = null, Stan p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _internalName = m_io.ReadBitsIntBe(24);
                m_io.AlignToByte();
                _room = m_io.ReadU1();
                _flags = m_io.ReadBitsIntBe(4);
                _red = m_io.ReadBitsIntBe(4);
                _green = m_io.ReadBitsIntBe(4);
                _blue = m_io.ReadBitsIntBe(4);
                _pointCount = m_io.ReadBitsIntBe(4);
                _firstPoint = m_io.ReadBitsIntBe(4);
                _secondPoint = m_io.ReadBitsIntBe(4);
                _thirdPoint = m_io.ReadBitsIntBe(4);
                m_io.AlignToByte();
                _points = new List<StandTilePoint>((int)(PointCount));
                for (var i = 0; i < (int)PointCount; i++)
                {
                    _points.Add(new StandTilePoint(m_io, this, m_root));
                }
            }
            private ulong _internalName;
            private byte _room;
            private ulong _flags;
            private ulong _red;
            private ulong _green;
            private ulong _blue;
            private ulong _pointCount;
            private ulong _firstPoint;
            private ulong _secondPoint;
            private ulong _thirdPoint;
            private List<StandTilePoint> _points;
            private Stan m_root;
            private Stan m_parent;
            public ulong InternalName { get { return _internalName; } }
            public byte Room { get { return _room; } }
            public ulong Flags { get { return _flags; } }
            public ulong Red { get { return _red; } }
            public ulong Green { get { return _green; } }
            public ulong Blue { get { return _blue; } }
            public ulong PointCount { get { return _pointCount; } }
            public ulong FirstPoint { get { return _firstPoint; } }
            public ulong SecondPoint { get { return _secondPoint; } }
            public ulong ThirdPoint { get { return _thirdPoint; } }
            public List<StandTilePoint> Points { get { return _points; } }
            public Stan M_Root { get { return m_root; } }
            public Stan M_Parent { get { return m_parent; } }
        }
        public partial class StandTileFooter : KaitaiStruct
        {
            public static StandTileFooter FromFile(string fileName)
            {
                return new StandTileFooter(new KaitaiStream(fileName));
            }

            public StandTileFooter(KaitaiStream p__io, Stan p__parent = null, Stan p__root = null) : base(p__io)
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
            private Stan m_root;
            private Stan m_parent;
            public string Unstric { get { return _unstric; } }
            public byte[] StringPad { get { return _stringPad; } }
            public uint Unknown3 { get { return _unknown3; } }
            public uint Unknown4 { get { return _unknown4; } }
            public uint Unknown5 { get { return _unknown5; } }
            public byte[] UnknownRemaining { get { return _unknownRemaining; } }
            public Stan M_Root { get { return m_root; } }
            public Stan M_Parent { get { return m_parent; } }
        }
        private StandFileHeader _headerBlock;
        private List<StandTile> _tiles;
        private StandTileFooter _footer;
        private Stan m_root;
        private KaitaiStruct m_parent;
        public StandFileHeader HeaderBlock { get { return _headerBlock; } }
        public List<StandTile> Tiles { get { return _tiles; } }
        public StandTileFooter Footer { get { return _footer; } }
        public Stan M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}
