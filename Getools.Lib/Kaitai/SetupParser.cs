using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Getools.Lib.Game;
using Getools.Lib.Game.Asset.Intro;
using Getools.Lib.Game.Asset.Setup;
using Getools.Lib.Game.Asset.SetupObject;

namespace Getools.Lib.Kaitai
{
    /// <summary>
    /// Wrapper for Kaitai parser generated code.
    /// </summary>
    public static class SetupParser
    {
        /// <summary>
        /// Reads a .bin file from disk and parses it using the Kaitai definition.
        /// </summary>
        /// <param name="path">Path to file to parse.</param>
        /// <returns>Newly created file.</returns>
        /// <remarks>
        /// All Kaitai struct objects are cleaned up / translated into
        /// cooresponding project objects.
        /// </remarks>
        public static StageSetupFile ParseBin(string path)
        {
            var kaitaiObject = Gen.Setup.FromFile(path);

            var setup = Convert(kaitaiObject);

            setup.DeserializeFix();

            return setup;
        }

        /// <summary>
        /// Converts Kaitai struct setup object to library setup object.
        /// </summary>
        /// <param name="kaitaiObject">Kaitai struct object.</param>
        /// <returns>Library object.</returns>
        private static StageSetupFile Convert(Gen.Setup kaitaiObject)
        {
            var ssf = new StageSetupFile();

            ssf.PathTablesOffset = (int)kaitaiObject.Pointers.PathTablesOffset;
            ssf.PathLinksOffset = (int)kaitaiObject.Pointers.PathLinksOffset;
            ssf.IntrosOffset = (int)kaitaiObject.Pointers.IntrosOffset;
            ssf.ObjectsOffset = (int)kaitaiObject.Pointers.ObjectListOffset;
            ssf.PathSetsOffset = (int)kaitaiObject.Pointers.PathSetsOffset;
            ssf.AiListOffset = (int)kaitaiObject.Pointers.AiListOffset;
            ssf.PadListOffset = (int)kaitaiObject.Pointers.PadListOffset;
            ssf.Pad3dListOffset = (int)kaitaiObject.Pointers.Pad3dListOffset;
            ssf.PadNamesOffset = (int)kaitaiObject.Pointers.PadNamesOffset;
            ssf.Pad3dNamesOffset = (int)kaitaiObject.Pointers.Pad3dNamesOffset;

            var fillerBlocks = new List<Gen.Setup.FillerBlock>();

            foreach (var block in kaitaiObject.Contents)
            {
                switch (block.Body)
                {
                    case Gen.Setup.PathTableSection kaitaiSection:
                        ConvertSection(ssf, kaitaiSection);
                        break;

                    case Gen.Setup.PathLinksSection kaitaiSection:
                        ConvertSection(ssf, kaitaiSection);
                        break;

                    case Gen.Setup.IntroList kaitaiSection:
                        ConvertSection(ssf, kaitaiSection);
                        break;

                    case Gen.Setup.ObjectList kaitaiSection:
                        ConvertSection(ssf, kaitaiSection);
                        break;

                    case Gen.Setup.PathSetsSection kaitaiSection:
                        ConvertSection(ssf, kaitaiSection);
                        break;

                    case Gen.Setup.AiList kaitaiSection:
                        ConvertSection(ssf, kaitaiSection);
                        break;

                    case Gen.Setup.PadList kaitaiSection:
                        ConvertSection(ssf, kaitaiSection);
                        break;

                    case Gen.Setup.Pad3dList kaitaiSection:
                        ConvertSection(ssf, kaitaiSection);
                        break;

                    case Gen.Setup.PadNamesList kaitaiSection:
                        ConvertSection(ssf, kaitaiSection);
                        break;

                    case Gen.Setup.Pad3dNamesList kaitaiSection:
                        ConvertSection(ssf, kaitaiSection);
                        break;

                    case Gen.Setup.FillerBlock filler:
                        // will be processed once known data is converted
                        fillerBlocks.Add(filler);
                        break;

                    default:
                        throw new InvalidOperationException($"Error parsing setup binary file. Unknown section block of type \"{block.GetType().Name}\"");
                }
            }

            if (ssf.AiLists.Any())
            {
                var aidataOffset = ssf.AiLists
                    .Where(x => x.EntryPointer > 0)
                    .OrderBy(x => x.EntryPointer)
                    .Select(x => x.EntryPointer)
                    .First();

                var aidataBlock = fillerBlocks.FirstOrDefault(x => x.StartPos == aidataOffset);

                if (object.ReferenceEquals(null, aidataBlock))
                {
                    throw new InvalidOperationException("AI Functions were listed in setup binary, but could not resolve associated function data");
                }

                var aidataBlockData = aidataBlock.Data.SelectMany(x => x).ToArray();

                int blockIndex = 0;

                // Facility has a duplicate ailist entry, so note the .Distinct here.
                var sortedPointers = ssf.AiLists.Where(x => x.EntryPointer > 0).Select(x => x.EntryPointer).OrderBy(x => x).Distinct().ToList();
                var numberSortedPointers = sortedPointers.Count;

                var aimap = new Dictionary<int, AiFunction>();

                foreach (var entry in ssf.AiLists.Where(x => x.EntryPointer > 0))
                {
                    // if this is a duplicate entry link the existing function and continue.
                    if (aimap.ContainsKey((int)entry.EntryPointer))
                    {
                        entry.Function = aimap[(int)entry.EntryPointer];
                        continue;
                    }

                    int functionSize = 0;
                    int currentEntrySortedIndex = sortedPointers.IndexOf(entry.EntryPointer);

                    if (currentEntrySortedIndex < numberSortedPointers - 1)
                    {
                        functionSize = (int)sortedPointers[currentEntrySortedIndex + 1] - (int)entry.EntryPointer;
                    }
                    else
                    {
                        functionSize = ssf.AiListOffset - (int)entry.EntryPointer;
                    }

                    if (functionSize < 0)
                    {
                        throw new ArgumentException($"Calculated invalid AI funciton size: {functionSize}, function entry: 0x{entry.EntryPointer:x4}");
                    }

                    blockIndex = (int)entry.EntryPointer - (int)aidataOffset;

                    if (blockIndex < 0)
                    {
                        throw new ArgumentException($"Calculated invalid AI function entry point: {blockIndex} relative to 0x{aidataOffset:x4}, function entry: 0x{entry.EntryPointer:x4}");
                    }

                    entry.Function = new AiFunction()
                    {
                        Data = new byte[functionSize],
                        Offset = (int)entry.EntryPointer,
                    };

                    aimap.Add((int)entry.EntryPointer, entry.Function);

                    Array.Copy(aidataBlockData, blockIndex, entry.Function.Data, 0, functionSize);
                }
            }

            var rodataFillerBlock = fillerBlocks.OrderByDescending(x => x.StartPos).First();
            var rodataOffset = (int)rodataFillerBlock.StartPos;
            var rodataBytes = rodataFillerBlock.Data.SelectMany(x => x).ToArray();

            // For most levels, the padnames pointer in the setup header is NULL, but there's still
            // an array of padnames with pointers to .rodata (pointing to empty strings).
            // Since the section offset is null, it isn't dereferenced, so try to look that up manually.
            // file order is
            //      struct s_pathTbl pathtbl[]
            //      padnames
            //      struct s_pathSet paths[] (first path set entry)
            if (ssf.PadNamesOffset == 0)
            {
                int nextSectionAddress = ssf.PathSetsOffset;
                if (ssf.PathSets.Any())
                {
                    var firstPathSetEntryAddress = ssf.PathSets
                        .Where(x => x.EntryPointer > 0)
                        .OrderBy(x => x.EntryPointer)
                        .Select(x => x.EntryPointer)
                        .First();

                    if (firstPathSetEntryAddress < nextSectionAddress)
                    {
                        nextSectionAddress = (int)firstPathSetEntryAddress;
                    }
                }

                // Look for a filler block after path tables and before path sets
                var padNamesData = fillerBlocks.FirstOrDefault(x => x.StartPos > ssf.PathTablesOffset && x.StartPos < nextSectionAddress);
                if (!object.ReferenceEquals(null, padNamesData))
                {
                    var padnamesBytesLength = (int)(nextSectionAddress - padNamesData.StartPos);

                    var fillerBlockBytes = padNamesData.Data.SelectMany(x => x).Take((int)padnamesBytesLength).ToArray();

                    if ((fillerBlockBytes.Length % Config.TargetPointerSize) != 0)
                    {
                        throw new InvalidOperationException($"Error trying to read padnames. {nameof(ssf.PadNamesOffset)} is null, so attempting to find padnames section based on starting address of filler block, but the closest match length is not word aligned.");
                    }

                    var pointerCount = fillerBlockBytes.Length / Config.TargetPointerSize;
                    var pointers = new List<int>();
                    int fillerBlockIndex = 0;
                    for (int i = 0; i < pointerCount; i++)
                    {
                        int pointer = BitUtility.Read32Big(fillerBlockBytes, fillerBlockIndex);

                        if (pointer > 0)
                        {
                            var stringValue = BitUtility.ReadString(rodataBytes, pointer - rodataOffset, 50);
                            ssf.PadNames.Add(new StringPointer((int)padNamesData.StartPos + fillerBlockIndex, stringValue));
                        }
                        else
                        {
                            ssf.PadNames.Add(new StringPointer(null));
                        }

                        fillerBlockIndex += Config.TargetPointerSize;
                    }
                }
            }

            // now everything that just happened for padnames, do it again but for pad3dnames.
            // file order is
            //      struct s_pathLink pathlist[]
            //      pad3dnames
            //      path_table filler
            if (ssf.Pad3dNamesOffset == 0)
            {
                var firstPathTableFiller = ssf.PathTables.Where(x => x.EntryPointer > 0).OrderBy(x => x.EntryPointer).FirstOrDefault();

                // Look for a filler block after path links and before path tables (path tables filler entry)
                var pad3dNamesData = fillerBlocks.FirstOrDefault(x => x.StartPos > ssf.PathLinksOffset && x.StartPos < firstPathTableFiller.EntryPointer);
                if (!object.ReferenceEquals(null, pad3dNamesData))
                {
                    var pad3dnamesBytesLength = (int)(firstPathTableFiller.EntryPointer - pad3dNamesData.StartPos);

                    var fillerBlockBytes = pad3dNamesData.Data.SelectMany(x => x).Take((int)pad3dnamesBytesLength).ToArray();

                    if ((fillerBlockBytes.Length % Config.TargetPointerSize) != 0)
                    {
                        throw new InvalidOperationException($"Error trying to read pad3dnames. {nameof(ssf.Pad3dNamesOffset)} is null, so attempting to find pad3dnames section based on starting address of filler block, but the closest match length is not word aligned.");
                    }

                    var pointerCount = fillerBlockBytes.Length / Config.TargetPointerSize;
                    var pointers = new List<int>();
                    int fillerBlockIndex = 0;
                    for (int i = 0; i < pointerCount; i++)
                    {
                        int pointer = BitUtility.Read32Big(fillerBlockBytes, fillerBlockIndex);

                        if (pointer > 0)
                        {
                            var stringValue = BitUtility.ReadString(rodataBytes, pointer - rodataOffset, 50);
                            ssf.Pad3dNames.Add(new StringPointer((int)pad3dNamesData.StartPos + fillerBlockIndex, stringValue));
                        }
                        else
                        {
                            ssf.Pad3dNames.Add(new StringPointer(null));
                        }

                        fillerBlockIndex += Config.TargetPointerSize;
                    }
                }
            }

            return ssf;
        }

        private static void ConvertSection(StageSetupFile parent, Gen.Setup.PathTableSection kaitaiObject)
        {
            foreach (var entry in kaitaiObject.Data)
            {
                parent.PathTables.Add(Convert(entry));
            }
        }

        private static void ConvertSection(StageSetupFile parent, Gen.Setup.PathLinksSection kaitaiObject)
        {
            foreach (var entry in kaitaiObject.Data)
            {
                parent.PathLinkEntries.Add(Convert(entry));
            }
        }

        private static void ConvertSection(StageSetupFile parent, Gen.Setup.IntroList kaitaiObject)
        {
            foreach (var entry in kaitaiObject.Data)
            {
                parent.Intros.Add(Convert(entry));
            }
        }

        private static void ConvertSection(StageSetupFile parent, Gen.Setup.ObjectList kaitaiObject)
        {
            foreach (var entry in kaitaiObject.Data)
            {
                parent.Objects.Add(Convert(entry));
            }
        }

        private static void ConvertSection(StageSetupFile parent, Gen.Setup.PathSetsSection kaitaiObject)
        {
            foreach (var entry in kaitaiObject.Data)
            {
                parent.PathSets.Add(Convert(entry));
            }
        }

        private static void ConvertSection(StageSetupFile parent, Gen.Setup.AiList kaitaiObject)
        {
            foreach (var entry in kaitaiObject.Data)
            {
                parent.AiLists.Add(Convert(entry));
            }
        }

        private static void ConvertSection(StageSetupFile parent, Gen.Setup.PadList kaitaiObject)
        {
            foreach (var entry in kaitaiObject.Data)
            {
                parent.PadList.Add(Convert(entry));
            }
        }

        private static void ConvertSection(StageSetupFile parent, Gen.Setup.Pad3dList kaitaiObject)
        {
            foreach (var entry in kaitaiObject.Data)
            {
                parent.Pad3dList.Add(Convert(entry));
            }
        }

        private static void ConvertSection(StageSetupFile parent, Gen.Setup.PadNamesList kaitaiObject)
        {
            foreach (var entry in kaitaiObject.Data)
            {
                parent.PadNames.Add(Convert(entry));
            }
        }

        private static void ConvertSection(StageSetupFile parent, Gen.Setup.Pad3dNamesList kaitaiObject)
        {
            foreach (var entry in kaitaiObject.Data)
            {
                parent.Pad3dNames.Add(Convert(entry));
            }
        }

        private static SetupPathTableEntry Convert(Gen.Setup.PathTableEntry kaitaiObject)
        {
            var spte = new SetupPathTableEntry();

            spte.Unknown_00 = kaitaiObject.Unknown00;
            spte.Unknown_02 = kaitaiObject.Unknown02;
            spte.EntryPointer = (int)kaitaiObject.UnknownPointer;
            spte.Unknown_08 = kaitaiObject.Unknown08;
            spte.Unknown_0C = kaitaiObject.Unknown0c;

            if (!object.ReferenceEquals(null, kaitaiObject.Data))
            {
                spte.Entry = new PathTable(kaitaiObject.Data.Select(x => (int)x.Value));
            }

            return spte;
        }

        private static SetupPathLinkEntry Convert(Gen.Setup.PathLinkEntry kaitaiObject)
        {
            var sple = new SetupPathLinkEntry();

            sple.NeighborsPointer = (int)kaitaiObject.PadNeighborOffset;
            sple.IndexPointer = (int)kaitaiObject.PadIndexOffset;

            if (!object.ReferenceEquals(null, kaitaiObject.PadNeighborIds))
            {
                sple.Neighbors = new PathListing(kaitaiObject.PadNeighborIds.Select(x => (int)x.Value));
            }

            if (!object.ReferenceEquals(null, kaitaiObject.PadIndexIds))
            {
                sple.Indeces = new PathListing(kaitaiObject.PadIndexIds.Select(x => (int)x.Value));
            }

            if (kaitaiObject.Empty != SetupPathLinkEntry.RecordDelimiter)
            {
                throw new NotSupportedException($"Error parsing setup binary file. {nameof(SetupPathLinkEntry)} record delimiter is \"{kaitaiObject.Empty}\", expected \"{SetupPathLinkEntry.RecordDelimiter}\".");
            }

            return sple;
        }

        private static IIntro Convert(Gen.Setup.SetupIntroRecord kaitaiObject)
        {
            IIntro intro;

            switch (kaitaiObject.Body)
            {
                case Gen.Setup.SetupIntroIntroCamBody kaitaiIntro:
                    intro = Convert(kaitaiIntro);
                    break;

                case Gen.Setup.SetupIntroCreditsBody kaitaiIntro:
                    intro = Convert(kaitaiIntro);
                    break;

                case Gen.Setup.SetupIntroCuffBody kaitaiIntro:
                    intro = Convert(kaitaiIntro);
                    break;

                case Gen.Setup.SetupIntroEndIntroBody kaitaiIntro:
                    intro = Convert(kaitaiIntro);
                    break;

                case Gen.Setup.SetupIntroFixedCamBody kaitaiIntro:
                    intro = Convert(kaitaiIntro);
                    break;

                case Gen.Setup.SetupIntroSpawnBody kaitaiIntro:
                    intro = Convert(kaitaiIntro);
                    break;

                case Gen.Setup.SetupIntroStartAmmoBody kaitaiIntro:
                    intro = Convert(kaitaiIntro);
                    break;

                case Gen.Setup.SetupIntroStartWeaponBody kaitaiIntro:
                    intro = Convert(kaitaiIntro);
                    break;

                case Gen.Setup.SetupIntroSwirlCamBody kaitaiIntro:
                    intro = Convert(kaitaiIntro);
                    break;

                case Gen.Setup.SetupIntroWatchTimeBody kaitaiIntro:
                    intro = Convert(kaitaiIntro);
                    break;

                default:
                    throw new InvalidOperationException($"Error parsing setup binary file. Unknown {nameof(Gen.Setup.SetupIntroRecord)} of type \"{kaitaiObject.Body.GetType().Name}\"");
            }

            intro.Scale = kaitaiObject.Header.ExtraScale;
            intro.Hidden2Raw = kaitaiObject.Header.State;
            ////// type is set in constructor.

            return intro;
        }

        private static IIntro Convert(Gen.Setup.SetupIntroIntroCamBody kaitaiObject)
        {
            var intro = new IntroCam();

            intro.Animation = kaitaiObject.Animation;

            return intro;
        }

        private static IIntro Convert(Gen.Setup.SetupIntroCreditsBody kaitaiObject)
        {
            return new IntroCredits();
        }

        private static IIntro Convert(Gen.Setup.SetupIntroCuffBody kaitaiObject)
        {
            var intro = new IntroCuff();

            intro.Cuff = kaitaiObject.CuffId;

            return intro;
        }

        private static IIntro Convert(Gen.Setup.SetupIntroEndIntroBody kaitaiObject)
        {
            return new IntroEndIntro();
        }

        private static IIntro Convert(Gen.Setup.SetupIntroFixedCamBody kaitaiObject)
        {
            var intro = new IntroFixedCam();

            intro.X = kaitaiObject.X;
            intro.Y = kaitaiObject.Y;
            intro.Z = kaitaiObject.Z;
            intro.LatRot = kaitaiObject.LatRot;
            intro.VertRot = kaitaiObject.VertRot;
            intro.Preset = kaitaiObject.Preset;
            intro.TextId = kaitaiObject.TextId;
            intro.Text2Id = kaitaiObject.Text2Id;
            intro.Unknown_20 = kaitaiObject.Unknown20;

            return intro;
        }

        private static IIntro Convert(Gen.Setup.SetupIntroSpawnBody kaitaiObject)
        {
            var intro = new IntroSpawn();

            intro.Unknown_00 = kaitaiObject.Unknown00;
            intro.Unknown_04 = kaitaiObject.Unknown04;

            return intro;
        }

        private static IIntro Convert(Gen.Setup.SetupIntroStartAmmoBody kaitaiObject)
        {
            var intro = new IntroStartAmmo();

            intro.AmmoType = kaitaiObject.AmmoType;
            intro.Quantity = kaitaiObject.Quantity;
            intro.Set = kaitaiObject.Set;

            return intro;
        }

        private static IIntro Convert(Gen.Setup.SetupIntroStartWeaponBody kaitaiObject)
        {
            var intro = new IntroStartWeapon();

            intro.Left = kaitaiObject.Left;
            intro.Right = kaitaiObject.Right;
            intro.SetNum = kaitaiObject.SetNum;

            return intro;
        }

        private static IIntro Convert(Gen.Setup.SetupIntroSwirlCamBody kaitaiObject)
        {
            var intro = new IntroSwirlCam();

            intro.Unknown_00 = kaitaiObject.Unknown00;
            intro.X = kaitaiObject.X;
            intro.Y = kaitaiObject.Y;
            intro.Z = kaitaiObject.Z;
            intro.Z = kaitaiObject.Z;
            intro.SplineScale = kaitaiObject.SplineScale;
            intro.Duration = kaitaiObject.Duration;
            intro.Flags = kaitaiObject.Flags;

            return intro;
        }

        private static IIntro Convert(Gen.Setup.SetupIntroWatchTimeBody kaitaiObject)
        {
            var intro = new IntroWatchTime();

            intro.Hour = kaitaiObject.Hour;
            intro.Minute = kaitaiObject.Minute;

            return intro;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectRecord kaitaiObject)
        {
            ISetupObject objectDef;

            switch (kaitaiObject.Body)
            {
                /* Alphabetical by type name, sort of */

                case Gen.Setup.SetupObjectAircraftBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectAlarmBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectAmmoBoxBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectAmmoMagBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectCctvBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectCollectObjectBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectCutsceneBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectDestroyObjectBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectDoorBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectAutogunBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectBodyArmorBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectEndObjectiveBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectEndProps kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectGasPropBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectGlassBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectGlassTintedBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectGuardBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectHangingMonitorBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectHatBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectKeyBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectLinkItemsBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectLinkPropsBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectLockBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectMissionObjectiveBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectMultiMonitorBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectObjectiveCompleteConditionBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectiveEnterRoomBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectObjectiveFailConditionBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectiveCopyItemBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectivePhotographItemBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectRenameBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectSafeBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectSafeItemBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectSetGuardAttributeBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectSingleMonitorBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectStandardBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectTagBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectTankBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectWatchMenuObjectiveBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectVehicleBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectWeaponBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                default:
                    throw new InvalidOperationException($"Error parsing setup binary file. Unknown propdef of type \"{kaitaiObject.Body.GetType().Name}\"");
            }

            objectDef.Scale = kaitaiObject.Header.ExtraScale;
            objectDef.Hidden2Raw = kaitaiObject.Header.State;
            ////// type is set in constructor.

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectAircraftBody kaitaiObject)
        {
            var objectDef = new SetupObjectAircraft();

            CopyGenericObjectBaseProperties(objectDef, kaitaiObject.ObjectBase);

            if (kaitaiObject.Bytes.Length != objectDef.Data.Length)
            {
                throw new InvalidOperationException($"Error parsing setup binary file when constructing \"{nameof(SetupObjectAircraft)}\". Parsed data length ({objectDef.Data.Length}) does not match expected value ({kaitaiObject.Bytes.Length})");
            }

            Array.Copy(kaitaiObject.Bytes, objectDef.Data, kaitaiObject.Bytes.Length);

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectAlarmBody kaitaiObject)
        {
            var objectDef = new SetupObjectAlarm();

            CopyGenericObjectBaseProperties(objectDef, kaitaiObject.ObjectBase);

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectAmmoBoxBody kaitaiObject)
        {
            var objectDef = new SetupObjectAmmoBox();

            CopyGenericObjectBaseProperties(objectDef, kaitaiObject.ObjectBase);

            objectDef.Unused_00 = kaitaiObject.Unused00;
            objectDef.Ammo9mm = kaitaiObject.Ammo9mm;
            objectDef.Unused_04 = kaitaiObject.Unused04;
            objectDef.Ammo9mm2 = kaitaiObject.Ammo9mm2;
            objectDef.Unused_08 = kaitaiObject.Unused08;
            objectDef.AmmoRifle = kaitaiObject.AmmoRifle;
            objectDef.Unused_0c = kaitaiObject.Unused0c;
            objectDef.AmmoShotgun = kaitaiObject.AmmoShotgun;
            objectDef.Unused_10 = kaitaiObject.Unused10;
            objectDef.AmmoHgrenade = kaitaiObject.AmmoHgrenade;
            objectDef.Unused_14 = kaitaiObject.Unused14;
            objectDef.AmmoRockets = kaitaiObject.AmmoRockets;
            objectDef.Unused_18 = kaitaiObject.Unused18;
            objectDef.AmmoRemoteMine = kaitaiObject.AmmoRemoteMine;
            objectDef.Unused_1c = kaitaiObject.Unused1c;
            objectDef.AmmoProximityMine = kaitaiObject.AmmoProximityMine;
            objectDef.Unused_20 = kaitaiObject.Unused20;
            objectDef.AmmoTimedMine = kaitaiObject.AmmoTimedMine;
            objectDef.Unused_24 = kaitaiObject.Unused24;
            objectDef.AmmoThrowing = kaitaiObject.AmmoThrowing;
            objectDef.Unused_28 = kaitaiObject.Unused28;
            objectDef.AmmoGrenadeLauncher = kaitaiObject.AmmoGrenadeLauncher;
            objectDef.Unused_2c = kaitaiObject.Unused2c;
            objectDef.AmmoMagnum = kaitaiObject.AmmoMagnum;
            objectDef.Unused_30 = kaitaiObject.Unused30;
            objectDef.AmmoGolden = kaitaiObject.AmmoGolden;

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectCollectObjectBody kaitaiObject)
        {
            var objectDef = new SetupObjectCollectObject();

            objectDef.ObjectId = kaitaiObject.ObjectId;

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectCutsceneBody kaitaiObject)
        {
            var objectDef = new SetupObjectCutscene();

            objectDef.XCoord = (int)kaitaiObject.Xcoord;
            objectDef.YCoord = (int)kaitaiObject.Ycoord;
            objectDef.ZCoord = (int)kaitaiObject.Zcoord;
            objectDef.LatRot = (int)kaitaiObject.LatRot;
            objectDef.VertRot = (int)kaitaiObject.VertRot;
            objectDef.IllumPreset = kaitaiObject.IllumPreset;

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectDestroyObjectBody kaitaiObject)
        {
            var objectDef = new SetupObjectDestroyObject();

            objectDef.ObjectId = kaitaiObject.ObjectId;

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectDoorBody kaitaiObject)
        {
            var objectDef = new SetupObjectDoor();

            CopyGenericObjectBaseProperties(objectDef, kaitaiObject.ObjectBase);

            objectDef.LinkedDoorOffset = kaitaiObject.LinkedDoorOffset;
            objectDef.MaxFrac = kaitaiObject.MaxFrac;
            objectDef.PerimFrac = kaitaiObject.PerimFrac;
            objectDef.Accel = kaitaiObject.Accel;
            objectDef.Decel = kaitaiObject.Decel;
            objectDef.MaxSpeed = kaitaiObject.MaxSpeed;
            objectDef.DoorFlags = kaitaiObject.DoorFlags;
            objectDef.DoorType = kaitaiObject.DoorType;
            objectDef.KeyFlags = kaitaiObject.KeyFlags;
            objectDef.AutoCloseFrames = kaitaiObject.AutoCloseFrames;
            objectDef.DoorOpenSound = kaitaiObject.DoorOpenSound;
            objectDef.Frac = kaitaiObject.Frac;
            objectDef.UnknownAc = kaitaiObject.UnknownAc;
            objectDef.UnknownB0 = kaitaiObject.UnknownB0;
            objectDef.OpenPosition = kaitaiObject.OpenPosition;
            objectDef.Speed = kaitaiObject.Speed;
            objectDef.State = kaitaiObject.State;
            objectDef.UnknownBd = kaitaiObject.UnknownBd;
            objectDef.UnknownBe = kaitaiObject.UnknownBe;
            objectDef.UnknownC0 = kaitaiObject.UnknownC0;
            objectDef.UnknownC4 = kaitaiObject.UnknownC4;
            objectDef.SoundType = kaitaiObject.SoundType;
            objectDef.FadeTime60 = kaitaiObject.FadeTime60;
            objectDef.LinkedDoorPointer = kaitaiObject.LinkedDoorPointer;
            objectDef.LaserFade = kaitaiObject.LaserFade;
            objectDef.UnknownCd = kaitaiObject.UnknownCd;
            objectDef.UnknownCe = kaitaiObject.UnknownCe;
            objectDef.UnknownD0 = kaitaiObject.UnknownD0;
            objectDef.UnknownD4 = kaitaiObject.UnknownD4;
            objectDef.UnknownD8 = kaitaiObject.UnknownD8;
            objectDef.UnknownDc = kaitaiObject.UnknownDc;
            objectDef.UnknownE0 = kaitaiObject.UnknownE0;
            objectDef.UnknownE4 = kaitaiObject.UnknownE4;
            objectDef.UnknownE8 = kaitaiObject.UnknownE8;
            objectDef.OpenedTime = kaitaiObject.OpenedTime;
            objectDef.PortalNumber = kaitaiObject.PortalNumber;
            objectDef.UnknownF4Pointer = kaitaiObject.UnknownF4Pointer;
            objectDef.UnknownF8 = kaitaiObject.UnknownF8;
            objectDef.Timer = kaitaiObject.Timer;

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectAutogunBody kaitaiObject)
        {
            var objectDef = new SetupObjectDrone();

            CopyGenericObjectBaseProperties(objectDef, kaitaiObject.ObjectBase);

            if (kaitaiObject.Bytes.Length != objectDef.Data.Length)
            {
                throw new InvalidOperationException($"Error parsing setup binary file when constructing \"{nameof(SetupObjectDrone)}\". Parsed data length ({objectDef.Data.Length}) does not match expected value ({kaitaiObject.Bytes.Length})");
            }

            Array.Copy(kaitaiObject.Bytes, objectDef.Data, kaitaiObject.Bytes.Length);

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectEndProps kaitaiObject)
        {
            var objectDef = new SetupObjectEndProps();

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectGuardBody kaitaiObject)
        {
            var objectDef = new SetupObjectGuard();

            objectDef.ObjectId = kaitaiObject.ObjectId;
            objectDef.Preset = kaitaiObject.Preset;
            objectDef.BodyId = kaitaiObject.BodyId;
            objectDef.ActionPathAssignment = kaitaiObject.ActionPathAssignment;
            objectDef.PresetToTrigger = kaitaiObject.PresetToTrigger;
            objectDef.Unknown10 = kaitaiObject.Unknown10;
            objectDef.Health = kaitaiObject.Health;
            objectDef.ReactionTime = kaitaiObject.ReactionTime;
            objectDef.Head = kaitaiObject.Head;
            objectDef.PointerRuntimeData = kaitaiObject.PointerRuntimeData;

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectHatBody kaitaiObject)
        {
            var objectDef = new SetupObjectHat();

            CopyGenericObjectBaseProperties(objectDef, kaitaiObject.ObjectBase);

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectKeyBody kaitaiObject)
        {
            var objectDef = new SetupObjectKey();

            CopyGenericObjectBaseProperties(objectDef, kaitaiObject.ObjectBase);

            objectDef.Key = kaitaiObject.Key;

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectObjectiveCompleteConditionBody kaitaiObject)
        {
            var objectDef = new SetupObjectObjectiveCompleteCondition();

            objectDef.TestVal = (int)kaitaiObject.Testval;

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectEndObjectiveBody kaitaiObject)
        {
            var objectDef = new SetupObjectObjectiveEnd();

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectObjectiveFailConditionBody kaitaiObject)
        {
            var objectDef = new SetupObjectObjectiveFailCondition();

            objectDef.TestVal = (int)kaitaiObject.Testval;

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectMissionObjectiveBody kaitaiObject)
        {
            var objectDef = new SetupObjectObjectiveStart();

            objectDef.ObjectiveNumber = (int)kaitaiObject.ObjectiveNumber;
            objectDef.TextId = (int)kaitaiObject.TextId;
            objectDef.MinDifficulty = (int)kaitaiObject.MinDifficulty;

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectWatchMenuObjectiveBody kaitaiObject)
        {
            var objectDef = new SetupObjectObjectiveWatchMenu();

            objectDef.MenuOption = (int)kaitaiObject.MenuOption;
            objectDef.TextId = (int)kaitaiObject.TextId;
            objectDef.End = (int)kaitaiObject.End;

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectRenameBody kaitaiObject)
        {
            var objectDef = new SetupObjectRename();

            objectDef.ObjectOffset = kaitaiObject.ObjectOffset;
            objectDef.InventoryId = kaitaiObject.InventoryId;
            objectDef.Text1 = kaitaiObject.Text1;
            objectDef.Text2 = kaitaiObject.Text2;
            objectDef.Text3 = kaitaiObject.Text3;
            objectDef.Text4 = kaitaiObject.Text4;
            objectDef.Text5 = kaitaiObject.Text5;
            objectDef.Unknown20 = kaitaiObject.Unknown24;

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectSetGuardAttributeBody kaitaiObject)
        {
            var objectDef = new SetupObjectSetGuardAttribute();

            objectDef.GuardId = kaitaiObject.GuardId;
            objectDef.Attribute = (int)kaitaiObject.Attribute;

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectSingleMonitorBody kaitaiObject)
        {
            var objectDef = new SetupObjectSingleMonitor();

            CopyGenericObjectBaseProperties(objectDef, kaitaiObject.ObjectBase);

            objectDef.CurNumCmdsFromStartRotation = kaitaiObject.CurNumCmdsFromStartRotation;
            objectDef.LoopCounter = kaitaiObject.LoopCounter;
            objectDef.ImgnumOrPtrheader = kaitaiObject.ImgnumOrPtrheader;
            objectDef.Rotation = kaitaiObject.Rotation;
            objectDef.CurHzoom = kaitaiObject.CurHzoom;
            objectDef.CurHzoomTime = kaitaiObject.CurHzoomTime;
            objectDef.FinalHzoomTime = kaitaiObject.FinalHzoomTime;
            objectDef.InitialHzoom = kaitaiObject.InitialHzoom;
            objectDef.FinalHzoom = kaitaiObject.FinalHzoom;
            objectDef.CurVzoom = kaitaiObject.CurVzoom;
            objectDef.CurVzoomTime = kaitaiObject.CurVzoomTime;
            objectDef.FinalVzoomTime = kaitaiObject.FinalVzoomTime;
            objectDef.InitialVzoom = kaitaiObject.InitialVzoom;
            objectDef.FinalVzoom = kaitaiObject.FinalVzoom;
            objectDef.CurHpos = kaitaiObject.CurHpos;
            objectDef.CurHscrollTime = kaitaiObject.CurHscrollTime;
            objectDef.FinalHscrollTime = kaitaiObject.FinalHscrollTime;
            objectDef.InitialHpos = kaitaiObject.InitialHpos;
            objectDef.FinalHpos = kaitaiObject.FinalHpos;
            objectDef.CurVpos = kaitaiObject.CurVpos;
            objectDef.CurVscrollTime = kaitaiObject.CurVscrollTime;
            objectDef.FinalVscrollTime = kaitaiObject.FinalVscrollTime;
            objectDef.InitialVpos = kaitaiObject.InitialVpos;
            objectDef.FinalVpos = kaitaiObject.FinalVpos;
            objectDef.CurRed = kaitaiObject.CurRed;
            objectDef.InitialRed = kaitaiObject.InitialRed;
            objectDef.FinalRed = kaitaiObject.FinalRed;
            objectDef.CurGreen = kaitaiObject.CurGreen;
            objectDef.InitialGreen = kaitaiObject.InitialGreen;
            objectDef.FinalGreen = kaitaiObject.FinalGreen;
            objectDef.CurBlue = kaitaiObject.CurBlue;
            objectDef.InitialBlue = kaitaiObject.InitialBlue;
            objectDef.FinalBlue = kaitaiObject.FinalBlue;
            objectDef.CurAlpha = kaitaiObject.CurAlpha;
            objectDef.InitialAlpha = kaitaiObject.InitialAlpha;
            objectDef.FinalAlpha = kaitaiObject.FinalAlpha;
            objectDef.CurColorTransitionTime = kaitaiObject.CurColorTransitionTime;
            objectDef.FinalColorTransitionTime = kaitaiObject.FinalColorTransitionTime;
            objectDef.BackwardMonLink = kaitaiObject.BackwardMonLink;
            objectDef.ForwardMonLink = kaitaiObject.ForwardMonLink;
            objectDef.AnimationNum = kaitaiObject.AnimationNum;

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectStandardBody kaitaiObject)
        {
            var objectDef = new SetupObjectStandard();

            CopyGenericObjectBaseProperties(objectDef, kaitaiObject.ObjectBase);

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectTagBody kaitaiObject)
        {
            var objectDef = new SetupObjectTag();

            objectDef.TagId = kaitaiObject.TagId;
            objectDef.Value = kaitaiObject.Value;
            objectDef.Unknown_04 = kaitaiObject.Unknown08;
            objectDef.Unknown_08 = kaitaiObject.Unknown0c;

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectTankBody kaitaiObject)
        {
            var objectDef = new SetupObjectTank();

            CopyGenericObjectBaseProperties(objectDef, kaitaiObject.ObjectBase);

            if (kaitaiObject.Bytes.Length != objectDef.Data.Length)
            {
                throw new InvalidOperationException($"Error parsing setup binary file when constructing \"{nameof(SetupObjectTank)}\". Parsed data length ({objectDef.Data.Length}) does not match expected value ({kaitaiObject.Bytes.Length})");
            }

            Array.Copy(kaitaiObject.Bytes, objectDef.Data, kaitaiObject.Bytes.Length);

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectWeaponBody kaitaiObject)
        {
            var objectDef = new SetupObjectWeapon();

            CopyGenericObjectBaseProperties(objectDef, kaitaiObject.ObjectBase);

            objectDef.GunPickup = kaitaiObject.GunPickup;
            objectDef.LinkedItem = kaitaiObject.LinkedItem;
            objectDef.Timer = kaitaiObject.Timer;
            objectDef.PointerLinkedItem = kaitaiObject.PointerLinkedItem;

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectCctvBody kaitaiObject)
        {
            var objectDef = new SetupObjectCctv();

            CopyGenericObjectBaseProperties(objectDef, kaitaiObject.ObjectBase);

            if (kaitaiObject.Bytes.Length != objectDef.Data.Length)
            {
                throw new InvalidOperationException($"Error parsing setup binary file when constructing \"{nameof(SetupObjectCctv)}\". Parsed data length ({objectDef.Data.Length}) does not match expected value ({kaitaiObject.Bytes.Length})");
            }

            Array.Copy(kaitaiObject.Bytes, objectDef.Data, kaitaiObject.Bytes.Length);

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectMultiMonitorBody kaitaiObject)
        {
            var objectDef = new SetupObjectMultiMonitor();

            CopyGenericObjectBaseProperties(objectDef, kaitaiObject.ObjectBase);

            if (kaitaiObject.Bytes.Length != objectDef.Data.Length)
            {
                throw new InvalidOperationException($"Error parsing setup binary file when constructing \"{nameof(SetupObjectMultiMonitor)}\". Parsed data length ({objectDef.Data.Length}) does not match expected value ({kaitaiObject.Bytes.Length})");
            }

            Array.Copy(kaitaiObject.Bytes, objectDef.Data, kaitaiObject.Bytes.Length);

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectBodyArmorBody kaitaiObject)
        {
            var objectDef = new SetupObjectBodyArmor();

            CopyGenericObjectBaseProperties(objectDef, kaitaiObject.ObjectBase);

            objectDef.ArmorStrength = kaitaiObject.ArmorStrength;
            objectDef.ArmorPercent = kaitaiObject.ArmorPercent;

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectGlassBody kaitaiObject)
        {
            var objectDef = new SetupObjectGlass();

            CopyGenericObjectBaseProperties(objectDef, kaitaiObject.ObjectBase);

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectHangingMonitorBody kaitaiObject)
        {
            var objectDef = new SetupObjectHangingMonitor();

            CopyGenericObjectBaseProperties(objectDef, kaitaiObject.ObjectBase);

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectivePhotographItemBody kaitaiObject)
        {
            var objectDef = new SetupObjectivePhotographItem();

            objectDef.TagId = kaitaiObject.ObjectTagId;
            objectDef.Unknown_04 = kaitaiObject.Unknown04;
            objectDef.Unknown_08 = kaitaiObject.Unknown08;

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectiveCopyItemBody kaitaiObject)
        {
            var objectDef = new SetupObjectiveCopyItem();

            objectDef.TagId = kaitaiObject.ObjectTagId;
            objectDef.Unknown_04 = kaitaiObject.Unknown04;
            objectDef.Unknown_06 = kaitaiObject.Unknown06;

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectSafeBody kaitaiObject)
        {
            var objectDef = new SetupObjectSafe();

            CopyGenericObjectBaseProperties(objectDef, kaitaiObject.ObjectBase);

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectSafeItemBody kaitaiObject)
        {
            var objectDef = new SetupObjectSafeItem();

            objectDef.Item = kaitaiObject.Item;
            objectDef.Safe = kaitaiObject.Safe;
            objectDef.Door = kaitaiObject.Door;
            objectDef.Empty = (int)kaitaiObject.Empty;

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectAmmoMagBody kaitaiObject)
        {
            var objectDef = new SetupObjectAmmoMag();

            CopyGenericObjectBaseProperties(objectDef, kaitaiObject.ObjectBase);

            objectDef.AmmoType = kaitaiObject.AmmoType;

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectVehicleBody kaitaiObject)
        {
            var objectDef = new SetupObjectVehicle();

            CopyGenericObjectBaseProperties(objectDef, kaitaiObject.ObjectBase);

            if (kaitaiObject.Bytes.Length != objectDef.Data.Length)
            {
                throw new InvalidOperationException($"Error parsing setup binary file when constructing \"{nameof(SetupObjectVehicle)}\". Parsed data length ({objectDef.Data.Length}) does not match expected value ({kaitaiObject.Bytes.Length})");
            }

            Array.Copy(kaitaiObject.Bytes, objectDef.Data, kaitaiObject.Bytes.Length);

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectGlassTintedBody kaitaiObject)
        {
            var objectDef = new SetupObjectGlassTinted();

            CopyGenericObjectBaseProperties(objectDef, kaitaiObject.ObjectBase);

            objectDef.Unknown04 = kaitaiObject.Unknown04;
            objectDef.Unknown08 = kaitaiObject.Unknown08;
            objectDef.Unknown0c = kaitaiObject.Unknown0c;
            objectDef.Unknown10 = kaitaiObject.Unknown10;
            objectDef.Unknown14 = kaitaiObject.Unknown14;

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectLockBody kaitaiObject)
        {
            var objectDef = new SetupObjectLock();

            objectDef.Door = kaitaiObject.Door;
            objectDef.Lock = kaitaiObject.Lock;
            objectDef.Empty = kaitaiObject.Empty;

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectiveEnterRoomBody kaitaiObject)
        {
            var objectDef = new SetupObjectObjectiveEnterRoom();

            objectDef.Room = kaitaiObject.Room;
            objectDef.Unknown04 = kaitaiObject.Unknown04;
            objectDef.Unknown08 = kaitaiObject.Unknown08;

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectLinkItemsBody kaitaiObject)
        {
            var objectDef = new SetupObjectLinkItems();

            objectDef.Offset1 = kaitaiObject.Offset1;
            objectDef.Offset2 = kaitaiObject.Offset2;

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectLinkPropsBody kaitaiObject)
        {
            var objectDef = new SetupObjectLinkProps();

            objectDef.Offset1 = kaitaiObject.Offset1;
            objectDef.Offset2 = kaitaiObject.Offset2;
            objectDef.Unknown08 = kaitaiObject.Unknown08;

            return objectDef;
        }

        private static ISetupObject Convert(Gen.Setup.SetupObjectGasPropBody kaitaiObject)
        {
            var objectDef = new SetupObjectGasProp();

            CopyGenericObjectBaseProperties(objectDef, kaitaiObject.ObjectBase);

            return objectDef;
        }

        private static void CopyGenericObjectBaseProperties(SetupObjectGenericBase dest, Gen.Setup.SetupGenericObject kaitaiObject)
        {
            dest.ObjectId = kaitaiObject.ObjectId;
            dest.Preset = kaitaiObject.Preset;
            dest.Flags1 = kaitaiObject.Flags1;
            dest.Flags2 = kaitaiObject.Flags2;
            dest.PointerPositionData = kaitaiObject.PointerPositionData;
            dest.PointerObjInstanceController = kaitaiObject.PointerObjInstanceController;
            dest.Unknown18 = kaitaiObject.Unknown18;
            dest.Unknown1c = kaitaiObject.Unknown1c;
            dest.Unknown20 = kaitaiObject.Unknown20;
            dest.Unknown24 = kaitaiObject.Unknown24;
            dest.Unknown28 = kaitaiObject.Unknown28;
            dest.Unknown2c = kaitaiObject.Unknown2c;
            dest.Unknown30 = kaitaiObject.Unknown30;
            dest.Unknown34 = kaitaiObject.Unknown34;
            dest.Unknown38 = kaitaiObject.Unknown38;
            dest.Unknown3c = kaitaiObject.Unknown3c;
            dest.Unknown40 = kaitaiObject.Unknown40;
            dest.Unknown44 = kaitaiObject.Unknown44;
            dest.Unknown48 = kaitaiObject.Unknown48;
            dest.Unknown4c = kaitaiObject.Unknown4c;
            dest.Unknown50 = kaitaiObject.Unknown50;
            dest.Unknown54 = kaitaiObject.Unknown54;
            dest.Xpos = kaitaiObject.Xpos;
            dest.Ypos = kaitaiObject.Ypos;
            dest.Zpos = kaitaiObject.Zpos;
            dest.Bitflags = kaitaiObject.Bitflags;
            dest.PointerCollisionBlock = kaitaiObject.PointerCollisionBlock;
            dest.Unknown6c = kaitaiObject.Unknown6c;
            dest.Unknown70 = kaitaiObject.Unknown70;
            dest.Health = kaitaiObject.Health;
            dest.MaxHealth = kaitaiObject.MaxHealth;
            dest.Unknown78 = kaitaiObject.Unknown78;
            dest.Unknown7c = kaitaiObject.Unknown7c;
        }

        private static SetupPathSetEntry Convert(Gen.Setup.PathSetEntry kaitaiObject)
        {
            var spse = new SetupPathSetEntry();

            spse.EntryPointer = kaitaiObject.Pointer;
            spse.Unknown_04 = kaitaiObject.Unknown04;

            if (!object.ReferenceEquals(null, kaitaiObject.Data))
            {
                spse.Entry = new PathSet(kaitaiObject.Data.Select(x => (int)x.Value));
            }

            return spse;
        }

        private static SetupAiListEntry Convert(Gen.Setup.SetupAiScript kaitaiObject)
        {
            var sae = new SetupAiListEntry();

            sae.EntryPointer = kaitaiObject.Pointer;
            sae.Id = kaitaiObject.Id;

            return sae;
        }

        private static Coord3df Convert(Gen.Setup.Coord3d kaitaiObject)
        {
            var c3 = new Coord3df();

            c3.X = kaitaiObject.X;
            c3.Y = kaitaiObject.Y;
            c3.Z = kaitaiObject.Z;

            return c3;
        }

        private static BoundingBoxf Convert(Gen.Setup.Bbox kaitaiObject)
        {
            var bbox = new BoundingBoxf();

            bbox.MinX = kaitaiObject.Xmin;
            bbox.MaxX = kaitaiObject.Xmax;
            bbox.MinY = kaitaiObject.Ymin;
            bbox.MaxY = kaitaiObject.Ymax;
            bbox.MinZ = kaitaiObject.Zmin;
            bbox.MaxZ = kaitaiObject.Zmax;

            return bbox;
        }

        private static StringPointer Convert(Gen.Setup.StringPointer kaitaiObject)
        {
            var sp = new StringPointer();

            if (kaitaiObject.Offset == 0)
            {
                sp.Offset = 0;
                sp.Value = null;
            }
            else
            {
                sp.Offset = (int)kaitaiObject.Offset;
                sp.Value = kaitaiObject.Deref;
            }

            return sp;
        }

        private static Pad Convert(Gen.Setup.Pad kaitaiObject)
        {
            var pad = new Pad();

            pad.Position = Convert(kaitaiObject.Pos);
            pad.Up = Convert(kaitaiObject.Up);
            pad.Look = Convert(kaitaiObject.Look);
            pad.Name = Convert(kaitaiObject.Plink);
            pad.Unknown = (int)kaitaiObject.Unknown;

            return pad;
        }

        private static Pad3d Convert(Gen.Setup.Pad3d kaitaiObject)
        {
            var pad = new Pad3d();

            pad.Position = Convert(kaitaiObject.Pos);
            pad.Up = Convert(kaitaiObject.Up);
            pad.Look = Convert(kaitaiObject.Look);
            pad.Name = Convert(kaitaiObject.Plink);
            pad.Unknown = (int)kaitaiObject.Unknown;

            pad.BoundingBox = Convert(kaitaiObject.Bbox);

            return pad;
        }
    }
}
