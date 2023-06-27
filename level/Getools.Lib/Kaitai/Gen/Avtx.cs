// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using Kaitai;
using System.Collections.Generic;

namespace Getools.Lib.Kaitai.Gen
{
    public partial class Avtx : KaitaiStruct
    {
        public static Avtx FromFile(string fileName)
        {
            return new Avtx(new KaitaiStream(fileName));
        }

        public static Avtx FromBytes(byte[] bytes)
        {
            return new Avtx(new KaitaiStream(bytes));
        }

        public Avtx(KaitaiStream p__io, KaitaiStruct p__parent = null, Avtx p__root = null) : base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root ?? this;
            _read();
        }
        private void _read()
        {
            _verteces = new List<Vtx>();
            {
                var i = 0;
                while (!m_io.IsEof) {
                    _verteces.Add(new Vtx(m_io, this, m_root));
                    i++;
                }
            }
        }
        public partial class Dummy : KaitaiStruct
        {
            public static Dummy FromFile(string fileName)
            {
                return new Dummy(new KaitaiStream(fileName));
            }

            public Dummy(KaitaiStream p__io, KaitaiStruct p__parent = null, Avtx p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _ignore = m_io.ReadU1();
            }
            private byte _ignore;
            private Avtx m_root;
            private KaitaiStruct m_parent;
            public byte Ignore { get { return _ignore; } }
            public Avtx M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class TextureNormal : KaitaiStruct
        {
            public static TextureNormal FromFile(string fileName)
            {
                return new TextureNormal(new KaitaiStream(fileName));
            }

            public TextureNormal(KaitaiStream p__io, Avtx.Vtx p__parent = null, Avtx p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _x = m_io.ReadS1();
                _y = m_io.ReadS1();
                _z = m_io.ReadS1();
            }
            private sbyte _x;
            private sbyte _y;
            private sbyte _z;
            private Avtx m_root;
            private Avtx.Vtx m_parent;
            public sbyte X { get { return _x; } }
            public sbyte Y { get { return _y; } }
            public sbyte Z { get { return _z; } }
            public Avtx M_Root { get { return m_root; } }
            public Avtx.Vtx M_Parent { get { return m_parent; } }
        }
        public partial class TextureCoord : KaitaiStruct
        {
            public static TextureCoord FromFile(string fileName)
            {
                return new TextureCoord(new KaitaiStream(fileName));
            }

            public TextureCoord(KaitaiStream p__io, Avtx.Vtx p__parent = null, Avtx p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _u = m_io.ReadS2be();
                _v = m_io.ReadS2be();
            }
            private short _u;
            private short _v;
            private Avtx m_root;
            private Avtx.Vtx m_parent;
            public short U { get { return _u; } }
            public short V { get { return _v; } }
            public Avtx M_Root { get { return m_root; } }
            public Avtx.Vtx M_Parent { get { return m_parent; } }
        }
        public partial class Coord3dShort : KaitaiStruct
        {
            public static Coord3dShort FromFile(string fileName)
            {
                return new Coord3dShort(new KaitaiStream(fileName));
            }

            public Coord3dShort(KaitaiStream p__io, Avtx.Vtx p__parent = null, Avtx p__root = null) : base(p__io)
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
            }
            private short _x;
            private short _y;
            private short _z;
            private Avtx m_root;
            private Avtx.Vtx m_parent;
            public short X { get { return _x; } }
            public short Y { get { return _y; } }
            public short Z { get { return _z; } }
            public Avtx M_Root { get { return m_root; } }
            public Avtx.Vtx M_Parent { get { return m_parent; } }
        }
        public partial class Vtx : KaitaiStruct
        {
            public static Vtx FromFile(string fileName)
            {
                return new Vtx(new KaitaiStream(fileName));
            }

            public Vtx(KaitaiStream p__io, Avtx p__parent = null, Avtx p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _ob = new Coord3dShort(m_io, this, m_root);
                _flag = m_io.ReadU2be();
                _textureCoord = new TextureCoord(m_io, this, m_root);
                _normal = new TextureNormal(m_io, this, m_root);
                _alpha = m_io.ReadU1();
            }
            private Coord3dShort _ob;
            private ushort _flag;
            private TextureCoord _textureCoord;
            private TextureNormal _normal;
            private byte _alpha;
            private Avtx m_root;
            private Avtx m_parent;
            public Coord3dShort Ob { get { return _ob; } }
            public ushort Flag { get { return _flag; } }
            public TextureCoord TextureCoord { get { return _textureCoord; } }
            public TextureNormal Normal { get { return _normal; } }
            public byte Alpha { get { return _alpha; } }
            public Avtx M_Root { get { return m_root; } }
            public Avtx M_Parent { get { return m_parent; } }
        }
        private List<Vtx> _verteces;
        private Avtx m_root;
        private KaitaiStruct m_parent;
        public List<Vtx> Verteces { get { return _verteces; } }
        public Avtx M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}
