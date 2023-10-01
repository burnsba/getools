// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild
#pragma warning disable SA1502 // Element should not be on a single line
#pragma warning disable SA1208 // System using directives should be placed before other using directives
#pragma warning disable SA1516 // Elements should be separated by blank line
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1513 // Closing brace should be followed by blank line
#pragma warning disable SA1601 // Partial elements should be documented
#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1308 // Variable names should not be prefixed
#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable SA1128 // Put constructor initializers on their own line
#pragma warning disable SA1500 // Braces for multi-line statements should not share line
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
    /// <summary>
    /// struct ramromfilestructure
    /// </summary>
    public partial class RamromReplay : KaitaiStruct
    {
        public static RamromReplay FromFile(string fileName)
        {
            return new RamromReplay(new KaitaiStream(fileName));
        }

        public RamromReplay(KaitaiStream p__io, KaitaiStruct p__parent = null, RamromReplay p__root = null) : base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root ?? this;
            _read();
        }
        private void _read()
        {
            _randomSeed = m_io.ReadU8be();
            _randomizer = m_io.ReadU8be();
            _stageNum = m_io.ReadU4be();
            _difficulty = m_io.ReadU4be();
            _sizeCmds = m_io.ReadU4be();
            _saveFile = new SaveData(m_io, this, m_root);
            _padding = m_io.ReadU2be();
            _totaltimeMs = m_io.ReadS4be();
            _fileSize = m_io.ReadS4be();
            _mode = m_io.ReadU4be();
            _slotNum = m_io.ReadU4be();
            _numPlayers = m_io.ReadU4be();
            _scenario = m_io.ReadU4be();
            _mpstageSel = m_io.ReadU4be();
            _gameLength = m_io.ReadU4be();
            _mpWeaponSet = m_io.ReadU4be();
            _mpChar = new List<int>((int) (4));
            for (var i = 0; i < 4; i++)
            {
                _mpChar.Add(m_io.ReadS4be());
            }
            _mpHandi = new List<int>((int) (4));
            for (var i = 0; i < 4; i++)
            {
                _mpHandi.Add(m_io.ReadS4be());
            }
            _mpContstyle = new List<int>((int) (4));
            for (var i = 0; i < 4; i++)
            {
                _mpContstyle.Add(m_io.ReadS4be());
            }
            _aimOption = m_io.ReadU4be();
            _mpFlags = new List<int>((int) (4));
            for (var i = 0; i < 4; i++)
            {
                _mpFlags.Add(m_io.ReadS4be());
            }
            _padding2 = m_io.ReadU4be();
            _seqData = new List<RamromIter>();
            {
                var i = 0;
                while (!m_io.IsEof) {
                    _seqData.Add(new RamromIter(SizeCmds, m_io, this, m_root));
                    i++;
                }
            }
        }

        /// <summary>
        /// struct save_data
        /// </summary>
        public partial class SaveData : KaitaiStruct
        {
            public static SaveData FromFile(string fileName)
            {
                return new SaveData(new KaitaiStream(fileName));
            }

            public SaveData(KaitaiStream p__io, RamromReplay p__parent = null, RamromReplay p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _chksum1 = m_io.ReadU4be();
                _chksum2 = m_io.ReadU4be();
                _completionBitflags = m_io.ReadU1();
                _flag007 = m_io.ReadU1();
                _musicVol = m_io.ReadU1();
                _sfxVol = m_io.ReadU1();
                _options = m_io.ReadU2be();
                _unlockedCheats1 = m_io.ReadU1();
                _unlockedCheats2 = m_io.ReadU1();
                _unlockedCheats3 = m_io.ReadU1();
                _unused = m_io.ReadU1();
                _times = new List<byte>((int) ((19 * 4)));
                for (var i = 0; i < (19 * 4); i++)
                {
                    _times.Add(m_io.ReadU1());
                }
            }
            private uint _chksum1;
            private uint _chksum2;
            private byte _completionBitflags;
            private byte _flag007;
            private byte _musicVol;
            private byte _sfxVol;
            private ushort _options;
            private byte _unlockedCheats1;
            private byte _unlockedCheats2;
            private byte _unlockedCheats3;
            private byte _unused;
            private List<byte> _times;
            private RamromReplay m_root;
            private RamromReplay m_parent;
            public uint Chksum1 { get { return _chksum1; } }
            public uint Chksum2 { get { return _chksum2; } }
            public byte CompletionBitflags { get { return _completionBitflags; } }
            public byte Flag007 { get { return _flag007; } }
            public byte MusicVol { get { return _musicVol; } }
            public byte SfxVol { get { return _sfxVol; } }
            public ushort Options { get { return _options; } }
            public byte UnlockedCheats1 { get { return _unlockedCheats1; } }
            public byte UnlockedCheats2 { get { return _unlockedCheats2; } }
            public byte UnlockedCheats3 { get { return _unlockedCheats3; } }
            public byte Unused { get { return _unused; } }
            public List<byte> Times { get { return _times; } }
            public RamromReplay M_Root { get { return m_root; } }
            public RamromReplay M_Parent { get { return m_parent; } }
        }
        public partial class RamromSeed : KaitaiStruct
        {
            public static RamromSeed FromFile(string fileName)
            {
                return new RamromSeed(new KaitaiStream(fileName));
            }

            public RamromSeed(KaitaiStream p__io, RamromReplay.RamromIter p__parent = null, RamromReplay p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _speedFrames = m_io.ReadU1();
                _count = m_io.ReadU1();
                _randseed = m_io.ReadU1();
                _check = m_io.ReadU1();
            }
            private byte _speedFrames;
            private byte _count;
            private byte _randseed;
            private byte _check;
            private RamromReplay m_root;
            private RamromReplay.RamromIter m_parent;
            public byte SpeedFrames { get { return _speedFrames; } }
            public byte Count { get { return _count; } }
            public byte Randseed { get { return _randseed; } }
            public byte Check { get { return _check; } }
            public RamromReplay M_Root { get { return m_root; } }
            public RamromReplay.RamromIter M_Parent { get { return m_parent; } }
        }
        public partial class RamromBlockbuf : KaitaiStruct
        {
            public static RamromBlockbuf FromFile(string fileName)
            {
                return new RamromBlockbuf(new KaitaiStream(fileName));
            }

            public RamromBlockbuf(KaitaiStream p__io, RamromReplay.RamromIter p__parent = null, RamromReplay p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _stickX = m_io.ReadS1();
                _stickY = m_io.ReadS1();
                _buttonLow = m_io.ReadU1();
                _buttonHigh = m_io.ReadU1();
            }
            private sbyte _stickX;
            private sbyte _stickY;
            private byte _buttonLow;
            private byte _buttonHigh;
            private RamromReplay m_root;
            private RamromReplay.RamromIter m_parent;
            public sbyte StickX { get { return _stickX; } }
            public sbyte StickY { get { return _stickY; } }
            public byte ButtonLow { get { return _buttonLow; } }
            public byte ButtonHigh { get { return _buttonHigh; } }
            public RamromReplay M_Root { get { return m_root; } }
            public RamromReplay.RamromIter M_Parent { get { return m_parent; } }
        }
        public partial class RamromIter : KaitaiStruct
        {
            public RamromIter(uint p_sizeCmds, KaitaiStream p__io, RamromReplay p__parent = null, RamromReplay p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _sizeCmds = p_sizeCmds;
                _read();
            }
            private void _read()
            {
                _head = new RamromSeed(m_io, this, m_root);
                _data = new List<RamromBlockbuf>((int) ((Head.Count * SizeCmds)));
                for (var i = 0; i < (Head.Count * SizeCmds); i++)
                {
                    _data.Add(new RamromBlockbuf(m_io, this, m_root));
                }
            }
            private RamromSeed _head;
            private List<RamromBlockbuf> _data;
            private uint _sizeCmds;
            private RamromReplay m_root;
            private RamromReplay m_parent;
            public RamromSeed Head { get { return _head; } }
            public List<RamromBlockbuf> Data { get { return _data; } }
            public uint SizeCmds { get { return _sizeCmds; } }
            public RamromReplay M_Root { get { return m_root; } }
            public RamromReplay M_Parent { get { return m_parent; } }
        }
        private ulong _randomSeed;
        private ulong _randomizer;
        private uint _stageNum;
        private uint _difficulty;
        private uint _sizeCmds;
        private SaveData _saveFile;
        private ushort _padding;
        private int _totaltimeMs;
        private int _fileSize;
        private uint _mode;
        private uint _slotNum;
        private uint _numPlayers;
        private uint _scenario;
        private uint _mpstageSel;
        private uint _gameLength;
        private uint _mpWeaponSet;
        private List<int> _mpChar;
        private List<int> _mpHandi;
        private List<int> _mpContstyle;
        private uint _aimOption;
        private List<int> _mpFlags;
        private uint _padding2;
        private List<RamromIter> _seqData;
        private RamromReplay m_root;
        private KaitaiStruct m_parent;
        public ulong RandomSeed { get { return _randomSeed; } }
        public ulong Randomizer { get { return _randomizer; } }

        /// <summary>
        /// enum LEVELID
        /// </summary>
        public uint StageNum { get { return _stageNum; } }

        /// <summary>
        /// enum DIFFICULTY
        /// </summary>
        public uint Difficulty { get { return _difficulty; } }
        public uint SizeCmds { get { return _sizeCmds; } }
        public SaveData SaveFile { get { return _saveFile; } }
        public ushort Padding { get { return _padding; } }
        public int TotaltimeMs { get { return _totaltimeMs; } }
        public int FileSize { get { return _fileSize; } }

        /// <summary>
        /// enum GAMEMODE
        /// </summary>
        public uint Mode { get { return _mode; } }
        public uint SlotNum { get { return _slotNum; } }
        public uint NumPlayers { get { return _numPlayers; } }
        public uint Scenario { get { return _scenario; } }
        public uint MpstageSel { get { return _mpstageSel; } }
        public uint GameLength { get { return _gameLength; } }
        public uint MpWeaponSet { get { return _mpWeaponSet; } }
        public List<int> MpChar { get { return _mpChar; } }
        public List<int> MpHandi { get { return _mpHandi; } }
        public List<int> MpContstyle { get { return _mpContstyle; } }
        public uint AimOption { get { return _aimOption; } }
        public List<int> MpFlags { get { return _mpFlags; } }
        public uint Padding2 { get { return _padding2; } }
        public List<RamromIter> SeqData { get { return _seqData; } }
        public RamromReplay M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}

#pragma warning restore SA1500 // Braces for multi-line statements should not share line
#pragma warning restore SA1128 // Put constructor initializers on their own line
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore SA1300 // Element should begin with upper-case letter
#pragma warning restore SA1308 // Variable names should not be prefixed
#pragma warning restore SA1201 // Elements should appear in the correct order
#pragma warning restore SA1601 // Partial elements should be documented
#pragma warning restore SA1513 // Closing brace should be followed by blank line
#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore SA1516 // Elements should be separated by blank line
#pragma warning restore SA1208 // System using directives should be placed before other using directives
#pragma warning restore SA1502 // Element should not be on a single line