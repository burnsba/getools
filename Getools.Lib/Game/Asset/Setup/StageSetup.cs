using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Getools.Lib.Error;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Complete setup.
    /// </summary>
    public class StageSetup
    {
        private int _rodataOffset = -1;

        public StageSetup()
        { }

        private enum ParseSection
        {
            DefaultUnknown,
            PathTables,
            PathLists,
            Intro,
            ObjectList,
            PathSets,
            AiLists,
            PadList,
            Pad3dList,
            Rodata,
        }

        public List<PathTable> PathTables { get; set; } = new List<PathTable>();

        public int PathTablesOffset { get; set; }

        public List<PathLink> PathLists { get; set; } = new List<PathLink>();

        public int PathListsOffset { get; set; }

        public List<int> Intros { get; set; } = new List<int>();

        public int IntrosOffset { get; set; }

        public List<int> Objects { get; set; } = new List<int>();

        public int ObjectsOffset { get; set; }

        public List<PathSet> PathSets { get; set; } = new List<PathSet>();

        public int PathSetsOffset { get; set; }

        public List<int> AiLists { get; set; } = new List<int>();

        public int AiListsOffset { get; set; }

        public List<Pad> PadList1 { get; set; } = new List<Pad>();

        public int PadList1Offset { get; set; }

        public List<Pad3d> Pad3dList { get; set; } = new List<Pad3d>();

        public int Pad3dListOffset { get; set; }

        public List<Byte> UnknownHeaderData { get; set; } = new List<byte>();

        public List<Pad> PadList2 { get; set; }

        public List<int> LinkedPathSets1 { get; set; }

        public List<int> LinkedPathSets2 { get; set; }

        public List<int> LinkedPathTables { get; set; }

        public List<int> PathSetEntries { get; set; }

        public List<int> UnknownList { get; set; }

        public static StageSetup ReadFromBinFile(BinaryReader br)
        {
            var result = new StageSetup();

            result.PathTablesOffset = BitUtility.Read32Big(br);

            if (result.PathTablesOffset < 1)
            {
                throw new BadFileFormatException($"Error reading {nameof(PathTablesOffset)} value: \"{result.PathTablesOffset}\"");
            }

            result.PathListsOffset = BitUtility.Read32Big(br);

            if (result.PathListsOffset < 1)
            {
                throw new BadFileFormatException($"Error reading {nameof(PathListsOffset)} value: \"{result.PathListsOffset}\"");
            }

            result.IntrosOffset = BitUtility.Read32Big(br);

            if (result.IntrosOffset < 1)
            {
                throw new BadFileFormatException($"Error reading {nameof(IntrosOffset)} value: \"{result.IntrosOffset}\"");
            }

            result.ObjectsOffset = BitUtility.Read32Big(br);

            if (result.ObjectsOffset < 1)
            {
                throw new BadFileFormatException($"Error reading {nameof(ObjectsOffset)} value: \"{result.ObjectsOffset}\"");
            }

            result.PathSetsOffset = BitUtility.Read32Big(br);

            if (result.PathSetsOffset < 1)
            {
                throw new BadFileFormatException($"Error reading {nameof(PathSetsOffset)} value: \"{result.PathSetsOffset}\"");
            }

            result.AiListsOffset = BitUtility.Read32Big(br);

            if (result.AiListsOffset < 1)
            {
                throw new BadFileFormatException($"Error reading {nameof(AiListsOffset)} value: \"{result.AiListsOffset}\"");
            }

            result.PadList1Offset = BitUtility.Read32Big(br);

            if (result.PadList1Offset < 1)
            {
                throw new BadFileFormatException($"Error reading {nameof(PadList1Offset)} value: \"{result.PadList1Offset}\"");
            }

            result.Pad3dListOffset = BitUtility.Read32Big(br);

            if (result.Pad3dListOffset < 1)
            {
                throw new BadFileFormatException($"Error reading {nameof(Pad3dListOffset)} value: \"{result.Pad3dListOffset}\"");
            }

            /*
            * Done with header pointers.
            * There should be two null pointers next, but use the address of the first data section to figure out
            * how much to read in case there's anything extra.
            */

            var remaining = result.PadList1Offset - br.BaseStream.Position;
            if (remaining < 0)
            {
                throw new BadFileFormatException($"Error reading setup header, invalid start offset: \"{result.PadList1Offset}\"");
            }

            for (int i = 0; i < remaining; i++)
            {
                result.UnknownHeaderData.Add(br.ReadByte());
            }

            /*
             * Done with header.
             * On to data.
             */
            var sectionParseOrder = new List<SectionOffsetOrder>();
            sectionParseOrder.Add(new SectionOffsetOrder(result.PathTablesOffset, ParseSection.PathTables));
            sectionParseOrder.Add(new SectionOffsetOrder(result.PathListsOffset, ParseSection.PathLists));
            sectionParseOrder.Add(new SectionOffsetOrder(result.IntrosOffset, ParseSection.Intro));
            sectionParseOrder.Add(new SectionOffsetOrder(result.ObjectsOffset, ParseSection.ObjectList));
            sectionParseOrder.Add(new SectionOffsetOrder(result.PathSetsOffset, ParseSection.PathSets));
            sectionParseOrder.Add(new SectionOffsetOrder(result.AiListsOffset, ParseSection.AiLists));
            sectionParseOrder.Add(new SectionOffsetOrder(result.PadList1Offset, ParseSection.PadList));
            sectionParseOrder.Add(new SectionOffsetOrder(result.Pad3dListOffset, ParseSection.Pad3dList));

            sectionParseOrder = sectionParseOrder.OrderBy(x => x.Offset).ToList();

            for (int i = 0; i < sectionParseOrder.Count; i++)
            {
                var section = sectionParseOrder[i];
                var nextSection = SectionOffsetOrder.UnsetRodata;

                if (i < (sectionParseOrder.Count - 1))
                {
                    nextSection = sectionParseOrder[i + 1];
                }

                switch (section.Section)
                {
                    case ParseSection.PadList:
                        result.ReadFromBinFile_PadList(br, nextSection);
                        if (result.PadList1.Any())
                        {
                            result.EnsureRodataOffsetSet(result.PadList1.First().NameRodataOffset);
                        }

                        break;

                    case ParseSection.Pad3dList:
                        result.ReadFromBinFile_Pad3dList(br, nextSection);
                        if (result.Pad3dList.Any())
                        {
                            result.EnsureRodataOffsetSet(result.Pad3dList.First().NameRodataOffset);
                        }

                        break;

                    default:
                        result.ReadFromBinFile_Unsupported(br, nextSection);
                        break;
                }
            }

            /*
            * now that .data is loaded, read the .rodata and resolve
            * strings back into their proper containers.
            */
            result.ReadFromBinFile_Rodata(br);

            return result;
        }

        private void ReadFromBinFile_PadList(BinaryReader br, SectionOffsetOrder nextSection)
        {
            int nextSectionStart = GetNextSectionStart(nextSection);
            long position = br.BaseStream.Position;

            while (position < br.BaseStream.Length - 1 && position < nextSectionStart)
            {
                int bytesToRead = (int)(nextSectionStart - position);

                if (bytesToRead < Pad.SizeOf)
                {
                    // not enough bytes to read the next entry, assume this is padding/alignment filler.
                    if (bytesToRead > 0)
                    {
                        br.ReadBytes((int)bytesToRead);
                    }

                    break;
                }

                var pad = Pad.ReadFromBinFile(br);

                PadList1.Add(pad);

                position += Pad.SizeOf;
            }
        }

        private void ReadFromBinFile_Pad3dList(BinaryReader br, SectionOffsetOrder nextSection)
        {
            int nextSectionStart = GetNextSectionStart(nextSection);
            long position = br.BaseStream.Position;

            while (position < br.BaseStream.Length - 1 && position < nextSectionStart)
            {
                int bytesToRead = (int)(nextSectionStart - position);

                if (bytesToRead < Pad3d.SizeOf)
                {
                    // not enough bytes to read the next entry, assume this is padding/alignment filler.
                    if (bytesToRead > 0)
                    {
                        br.ReadBytes((int)bytesToRead);
                    }

                    break;
                }

                var pad = Pad3d.ReadFromBinFile(br);

                Pad3dList.Add(pad);

                position += Pad3d.SizeOf;
            }
        }

        // read bytes and discard until the next section.
        private void ReadFromBinFile_Unsupported(BinaryReader br, SectionOffsetOrder nextSection)
        {
            if (nextSection.Section == ParseSection.Rodata)
            {
                throw new InvalidOperationException("Rodata section should be handled explicitly");
            }

            int nextSectionStart = GetNextSectionStart(nextSection);
            long position = br.BaseStream.Position;

            // reading one byte at a time is not ideal, but it's the "safe" way.
            while (position < br.BaseStream.Length - 1 && position < nextSectionStart)
            {
                br.ReadByte();
                position += 1;
            }
        }

        private void ReadFromBinFile_Rodata(BinaryReader br)
        {
            var rostrings = new List<string>();
            var rodataOffsets = new List<int>();

            var rodataStrings = BitUtility.ReadRodataStrings(br, 16);

            var needToSetCount = PadList1.Count + Pad3dList.Count;

            if (needToSetCount != rodataStrings.Count)
            {
                throw new BadFileFormatException($"Error reading setup: .rodata strings count (={rodataStrings.Count}) does not match pad count (={needToSetCount}). #pad={PadList1.Count}, #pad3d={Pad3dList.Count}");
            }

            /*
             * Iterate list of pads, find matching rodata string.
             * Then Iterate list of pad3ds, find matching rodata string.
             * Feel like this is suboptimal, but didn't benchmark anything.
             * Would some kind of join be faster?
             */

            foreach (var p in PadList1)
            {
                var stringy = rodataStrings.FirstOrDefault(x => x.Offset == p.NameRodataOffset);
                if (object.ReferenceEquals(null, stringy))
                {
                    throw new BadFileFormatException($"Error reading setup: could not find matching .rodata string for pad, string offset=0x{p.NameRodataOffset:x4}");
                }

                p.Name = stringy.Value;
            }

            foreach (var p3d in Pad3dList)
            {
                var stringy = rodataStrings.FirstOrDefault(x => x.Offset == p3d.NameRodataOffset);
                if (object.ReferenceEquals(null, stringy))
                {
                    throw new BadFileFormatException($"Error reading setup: could not find matching .rodata string for pad3d, string offset=0x{p3d.NameRodataOffset:x4}");
                }

                p3d.Name = stringy.Value;
            }
        }

        private int GetNextSectionStart(SectionOffsetOrder nextSection)
        {
            // If the next section is .rodata, then there should have been a string encountered
            // along the way to mark .rodata start. Otherwise read to end of stream.
            int nextSectionStart = nextSection.Offset;
            if (nextSection.Section == ParseSection.Rodata)
            {
                nextSectionStart = _rodataOffset;
            }

            if (nextSectionStart < 0)
            {
                nextSectionStart = Int32.MaxValue;
            }

            return nextSectionStart;
        }

        private void EnsureRodataOffsetSet(int newValue)
        {
            if (_rodataOffset == -1 || newValue < _rodataOffset)
            {
                if (newValue > 0)
                {
                    _rodataOffset = newValue;
                }
            }
        }

        private class SectionOffsetOrder
        {
            public static SectionOffsetOrder UnsetRodata { get; } = new SectionOffsetOrder(-1, ParseSection.Rodata);

            public int Offset { get; set; }

            public ParseSection Section { get; set; }

            public SectionOffsetOrder(int offset, ParseSection section)
            {
                Offset = offset;
                Section = section;
            }
        }
    }
}
