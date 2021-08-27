using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Getools.Lib.Game;
using Getools.Lib.Game.Asset.Intro;
using Getools.Lib.Game.Asset.Setup;
using Getools.Lib.Game.Asset.SetupObject;
using Getools.Lib.Game.Enums;

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
            var ssf = StageSetupFile.NewEmpty();

            //ssf.PathTablesOffset = (int)kaitaiObject.Pointers.PathTablesOffset;
            //ssf.PathLinksOffset = (int)kaitaiObject.Pointers.PathLinksOffset;
            //ssf.IntrosOffset = (int)kaitaiObject.Pointers.IntrosOffset;
            //ssf.ObjectsOffset = (int)kaitaiObject.Pointers.ObjectListOffset;
            //ssf.PathSetsOffset = (int)kaitaiObject.Pointers.PathSetsOffset;
            //ssf.AiListOffset = (int)kaitaiObject.Pointers.AiListOffset;
            //ssf.PadListOffset = (int)kaitaiObject.Pointers.PadListOffset;
            //ssf.Pad3dListOffset = (int)kaitaiObject.Pointers.Pad3dListOffset;
            //ssf.PadNamesOffset = (int)kaitaiObject.Pointers.PadNamesOffset;
            //ssf.Pad3dNamesOffset = (int)kaitaiObject.Pointers.Pad3dNamesOffset;

            //var fillerBlocks = new List<Gen.Setup.FillerBlock>();

            foreach (var block in kaitaiObject.Contents)
            {
                switch (block.Body)
                {
                    case Gen.Setup.PathTableSection kaitaiSection:
                        ssf.Sections.Add(ConvertSection(ssf, kaitaiSection));
                        break;

                    case Gen.Setup.PathLinksSection kaitaiSection:
                        ssf.Sections.Add(ConvertSection(ssf, kaitaiSection));
                        break;

                    case Gen.Setup.IntroList kaitaiSection:
                        ssf.Sections.Add(ConvertSection(ssf, kaitaiSection));
                        break;

                    case Gen.Setup.ObjectList kaitaiSection:
                        ssf.Sections.Add(ConvertSection(ssf, kaitaiSection));
                        break;

                    case Gen.Setup.PathSetsSection kaitaiSection:
                        ssf.Sections.Add(ConvertSection(ssf, kaitaiSection));
                        break;

                    case Gen.Setup.AiList kaitaiSection:
                        ssf.Sections.Add(ConvertSection(ssf, kaitaiSection));
                        break;

                    case Gen.Setup.PadList kaitaiSection:
                        ssf.Sections.Add(ConvertSection(ssf, kaitaiSection));
                        break;

                    case Gen.Setup.Pad3dList kaitaiSection:
                        ssf.Sections.Add(ConvertSection(ssf, kaitaiSection));
                        break;

                    case Gen.Setup.PadNamesList kaitaiSection:
                        ssf.Sections.Add(ConvertSection(ssf, kaitaiSection));
                        break;

                    case Gen.Setup.Pad3dNamesList kaitaiSection:
                        ssf.Sections.Add(ConvertSection(ssf, kaitaiSection));
                        break;

                    case Gen.Setup.FillerBlock filler:
                        // will be processed once known data is converted
                        ssf.Sections.Add(ToUnrefUnknown(filler));
                        break;

                    default:
                        throw new InvalidOperationException($"Error parsing setup binary file. Unknown section block of type \"{block.GetType().Name}\"");
                }
            }

            // rodata should happen first
            ParseRodataSection(ssf);

            ParseAiListData(ssf);
            //ParsePadNames(ssf, fillerBlocks, rodataBytes, rodataOffset, ref fillerBlockPathSetCount, ref potentialPathSetCount);
            //ParsePad3dNames(ssf, fillerBlocks, rodataBytes, rodataOffset, ref fillerBlockPathTableCount, ref potentialPathTableCount);
            //ParsePathLinkEntries(ssf, fillerBlocks);
            //ParsePathTablesEntries(ssf);
            //ParsePathSetsEntries(ssf);
            ParseUnrefAiListData(ssf);

            SearchPadNames(ssf);

            // mark entry data filler blocks as taken

            if (!object.ReferenceEquals(null, ssf.SectionIntros))
            {
                var creditsIntros = ssf.SectionIntros.Intros.Where(x => x.Type == IntroType.Credits).Cast<IntroCredits>();

                foreach (var credits in creditsIntros)
                {
                    var entrySize = credits.Credits.CreditsEntries.Count * IntroCreditEntry.SizeOf;

                    if (entrySize > 0)
                    {
                        if (ssf.FillerBlocks.Any(x => x.Offset == credits.DataOffset && x.Length == entrySize))
                        {
                            ssf.ClaimUnrefSectionBytes(credits.DataOffset, -1);
                        }
                    }
                }
            }

            if (!object.ReferenceEquals(null, ssf.SectionPathTables))
            {
                var entrySize = ssf.SectionPathTables.GetPrequelDataSize();
                var firstEntry = ssf.SectionPathTables.PathTables.Where(x => x.EntryPointer > 0).OrderBy(x => x.EntryPointer).FirstOrDefault();

                if (!object.ReferenceEquals(null, firstEntry) && entrySize > 0)
                {
                    if (ssf.FillerBlocks.Any(x => x.Offset == firstEntry.EntryPointer && x.Length == entrySize))
                    {
                        ssf.ClaimUnrefSectionBytes(firstEntry.EntryPointer, -1);
                    }
                }
            }

            if (!object.ReferenceEquals(null, ssf.SectionPathList))
            {
                var entrySize = ssf.SectionPathList.GetPrequelDataSize();

                var firstEntry = ssf.SectionPathList.PathLinkEntries.Where(x => x.NeighborsPointer > 0).OrderBy(x => x.NeighborsPointer).FirstOrDefault();

                if (!object.ReferenceEquals(null, firstEntry) && entrySize > 0)
                {
                    if (ssf.FillerBlocks.Any(x => x.Offset == firstEntry.NeighborsPointer && x.Length == entrySize))
                    {
                        ssf.ClaimUnrefSectionBytes(firstEntry.NeighborsPointer, -1);
                    }
                }
            }

            if (!object.ReferenceEquals(null, ssf.SectionPathList))
            {
                var entrySize = ssf.SectionPathList.GetPrequelDataSize();

                var firstEntry = ssf.SectionPathList.PathLinkEntries.Where(x => x.IndexPointer > 0).OrderBy(x => x.IndexPointer).FirstOrDefault();

                if (!object.ReferenceEquals(null, firstEntry) && entrySize > 0)
                {
                    if (ssf.FillerBlocks.Any(x => x.Offset == firstEntry.IndexPointer && x.Length == entrySize))
                    {
                        ssf.ClaimUnrefSectionBytes(firstEntry.IndexPointer, -1);
                    }
                }
            }

            if (!object.ReferenceEquals(null, ssf.SectionPathSets))
            {
                var entrySize = ssf.SectionPathSets.GetPrequelDataSize();
                var firstEntry = ssf.SectionPathSets.PathSets.Where(x => x.EntryPointer > 0).OrderBy(x => x.EntryPointer).FirstOrDefault();

                if (!object.ReferenceEquals(null, firstEntry) && entrySize > 0)
                {
                    if (ssf.FillerBlocks.Any(x => x.Offset == firstEntry.EntryPointer && x.Length == entrySize))
                    {
                        ssf.ClaimUnrefSectionBytes((int)firstEntry.EntryPointer, -1);
                    }
                }
            }

            if (!object.ReferenceEquals(null, ssf.SectionAiLists))
            {
                var entrySize = ssf.SectionAiLists.GetPrequelDataSize();
                var firstEntry = ssf.SectionAiLists.AiLists.Where(x => x.EntryPointer > 0).OrderBy(x => x.EntryPointer).FirstOrDefault();

                if (!object.ReferenceEquals(null, firstEntry) && entrySize > 0)
                {
                    if (ssf.FillerBlocks.Any(x => x.Offset == firstEntry.EntryPointer && x.Length == entrySize))
                    {
                        ssf.ClaimUnrefSectionBytes((int)firstEntry.EntryPointer, -1);
                    }
                }
            }

            ssf.SortSectionsByOffset();

            return ssf;
        }

        private static DataSectionPathTable ConvertSection(StageSetupFile parent, Gen.Setup.PathTableSection kaitaiObject)
        {
            var result = new DataSectionPathTable();

            result.Offset = (int)kaitaiObject.M_Parent.M_Parent.Pointers.PathTablesOffset;

            foreach (var entry in kaitaiObject.Data)
            {
                result.PathTables.Add(Convert(entry));
            }

            return result;
        }

        private static DataSectionPathList ConvertSection(StageSetupFile parent, Gen.Setup.PathLinksSection kaitaiObject)
        {
            var result = new DataSectionPathList();

            result.Offset = (int)kaitaiObject.M_Parent.M_Parent.Pointers.PathLinksOffset;

            foreach (var entry in kaitaiObject.Data)
            {
                result.PathLinkEntries.Add(Convert(entry));
            }

            return result;
        }

        private static DataSectionIntros ConvertSection(StageSetupFile parent, Gen.Setup.IntroList kaitaiObject)
        {
            var result = new DataSectionIntros();

            result.Offset = (int)kaitaiObject.M_Parent.M_Parent.Pointers.IntrosOffset;

            foreach (var entry in kaitaiObject.Data)
            {
                IIntro intro = Convert(entry);
                result.Intros.Add(intro);

                if (intro is IntroCredits credits)
                {
                    var section = RefSectionCredits.NewSection(credits);
                    section.Offset = credits.DataOffset;
                    parent.AddSectionBefore(section, SetupSectionId.SectionObjects);
                }
            }

            return result;
        }

        private static DataSectionObjects ConvertSection(StageSetupFile parent, Gen.Setup.ObjectList kaitaiObject)
        {
            var result = new DataSectionObjects();

            result.Offset = (int)kaitaiObject.M_Parent.M_Parent.Pointers.ObjectListOffset;

            foreach (var entry in kaitaiObject.Data)
            {
                result.Objects.Add(Convert(entry));
            }

            return result;
        }

        private static DataSectionPathSets ConvertSection(StageSetupFile parent, Gen.Setup.PathSetsSection kaitaiObject)
        {
            var result = new DataSectionPathSets();

            result.Offset = (int)kaitaiObject.M_Parent.M_Parent.Pointers.PathSetsOffset;

            foreach (var entry in kaitaiObject.Data)
            {
                result.PathSets.Add(Convert(entry));
            }

            return result;
        }

        private static DataSectionAiList ConvertSection(StageSetupFile parent, Gen.Setup.AiList kaitaiObject)
        {
            var result = new DataSectionAiList();

            result.Offset = (int)kaitaiObject.M_Parent.M_Parent.Pointers.AiListOffset;

            foreach (var entry in kaitaiObject.Data)
            {
                result.AiLists.Add(Convert(entry));
            }

            return result;
        }

        private static DataSectionPadList ConvertSection(StageSetupFile parent, Gen.Setup.PadList kaitaiObject)
        {
            var result = new DataSectionPadList();

            result.Offset = (int)kaitaiObject.M_Parent.M_Parent.Pointers.PadListOffset;

            foreach (var entry in kaitaiObject.Data)
            {
                result.PadList.Add(Convert(entry));
            }

            return result;
        }

        private static DataSectionPad3dList ConvertSection(StageSetupFile parent, Gen.Setup.Pad3dList kaitaiObject)
        {
            var result = new DataSectionPad3dList();

            result.Offset = (int)kaitaiObject.M_Parent.M_Parent.Pointers.Pad3dListOffset;

            foreach (var entry in kaitaiObject.Data)
            {
                result.Pad3dList.Add(Convert(entry));
            }

            return result;
        }

        private static DataSectionPadNames ConvertSection(StageSetupFile parent, Gen.Setup.PadNamesList kaitaiObject)
        {
            var result = new DataSectionPadNames();

            result.Offset = (int)kaitaiObject.M_Parent.M_Parent.Pointers.PadNamesOffset;

            foreach (var entry in kaitaiObject.Data)
            {
                result.PadNames.Add(Convert(entry));
            }

            return result;
        }

        private static DataSectionPad3dNames ConvertSection(StageSetupFile parent, Gen.Setup.Pad3dNamesList kaitaiObject)
        {
            var result = new DataSectionPad3dNames();

            result.Offset = (int)kaitaiObject.M_Parent.M_Parent.Pointers.Pad3dNamesOffset;

            foreach (var entry in kaitaiObject.Data)
            {
                result.Pad3dNames.Add(Convert(entry));
            }

            return result;
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
            var intro = new IntroCredits();

            intro.DataOffset = kaitaiObject.DataOffset;

            if (intro.DataOffset > 0 && kaitaiObject.CreditData.Any())
            {
                intro.Credits = new CreditsContainer();

                foreach (var entry in kaitaiObject.CreditData)
                {
                    intro.Credits.CreditsEntries.Add(Convert(entry));
                }
            }

            return intro;
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

        private static IntroCreditEntry Convert(Gen.Setup.IntroCreditEntry kaitaiObject)
        {
            var intro = new IntroCreditEntry();

            if (!Enum.IsDefined(typeof(CreditTextAlignment), (ushort)kaitaiObject.TextAlignment1))
            {
                throw new NotSupportedException($"Error parsing setup intro credits. Text alignment (1) \"{kaitaiObject.TextAlignment1}\" not defined.");
            }

            if (!Enum.IsDefined(typeof(CreditTextAlignment), (ushort)kaitaiObject.TextAlignment2))
            {
                throw new NotSupportedException($"Error parsing setup intro credits. Text alignment (2) \"{kaitaiObject.TextAlignment2}\" not defined.");
            }

            intro.TextId1 = kaitaiObject.TextId1;
            intro.TextId2 = kaitaiObject.TextId2;
            intro.Position1 = kaitaiObject.TextPosition1;
            intro.Alignment1 = (CreditTextAlignment)kaitaiObject.TextAlignment1;
            intro.Position2 = kaitaiObject.TextPosition2;
            intro.Alignment2 = (CreditTextAlignment)kaitaiObject.TextAlignment2;

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

                case Gen.Setup.SetupObjectDoorScaleBody kaitaiObjectDef:
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

        private static ISetupObject Convert(Gen.Setup.SetupObjectDoorScaleBody kaitaiObject)
        {
            var objectDef = new SetupObjectDoorScale();

            objectDef.Modifier = kaitaiObject.Modifier;

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

        // Parse AI List functions
        private static void ParseAiListData(StageSetupFile ssf)
        {
            if (!object.ReferenceEquals(null, ssf.SectionAiLists)
                && ssf.SectionAiLists.AiLists.Where(x => x.EntryPointer > 0).Any())
            {
                // Facility has a duplicate ailist entry, so note the .Distinct here.
                var sortedPointers = ssf.SectionAiLists.AiLists.Where(x => x.EntryPointer > 0).Select(x => x.EntryPointer).OrderBy(x => x).Distinct().ToList();
                var numberSortedPointers = sortedPointers.Count;
                int functionSize = 0;

                var aidataOffset = ssf.SectionAiLists.AiLists
                    .Where(x => x.EntryPointer > 0)
                    .OrderBy(x => x.EntryPointer)
                    .Select(x => x.EntryPointer)
                    .First();

                var aidataBlock = ssf.FillerBlocks.FirstOrDefault(x => x.Offset == aidataOffset);

                // There may be unreferenced AI functions at the start of the filler block.
                if (object.ReferenceEquals(null, aidataBlock))
                {
                    var previousSectionOffset = ssf.PreviousSectionOffset(ssf.SectionAiLists.Offset);

                    // looking for filler section immediately before AI List section.
                    aidataBlock = ssf.FillerBlocks
                        .Where(x => x.Offset > previousSectionOffset && x.Offset < ssf.SectionAiLists.Offset)
                        .OrderByDescending(x => x.Offset)
                        .FirstOrDefault();

                    functionSize = (int)aidataOffset - aidataBlock.Offset;
                    if (functionSize < 0)
                    {
                        throw new ArgumentException($"Calculated invalid AI funciton size: {functionSize}, function entry: 0x{aidataBlock.Offset:x4}");
                    }

                    var claimed = ssf.ClaimUnrefSectionBytes(aidataBlock.Offset, functionSize);
                    claimed.Offset = aidataBlock.Offset;

                    var f2 = new SetupAiListEntry()
                    {
                        Function = new AiFunction()
                        {
                            Data = claimed.GetDataBytes().Take(functionSize).ToArray(),
                            Offset = claimed.Offset,
                        },
                    };

                    var section = UnrefSectionAiFunction.NewUnreferencedSection();
                    section.Offset = f2.Function.Offset;
                    section.AiLists.Add(f2);
                    ssf.AddSectionBefore(section, SetupSectionId.SectionAiList, ssf.SectionAiLists.Offset);

                    // done with unclaimed AI List, this should fixup the aidatablock now
                    aidataBlock = ssf.FillerBlocks.FirstOrDefault(x => x.Offset == aidataOffset);
                }

                if (object.ReferenceEquals(null, aidataBlock))
                {
                    throw new InvalidOperationException("AI Functions were listed in setup binary, but could not resolve associated function data");
                }

                int blockIndex = 0;

                var aimap = new Dictionary<int, AiFunction>();
                var claimedDataSize = 0;

                foreach (var entry in ssf.SectionAiLists.AiLists.Where(x => x.EntryPointer > 0))
                {
                    // if this is a duplicate entry link the existing function and continue.
                    if (aimap.ContainsKey((int)entry.EntryPointer))
                    {
                        entry.Function = aimap[(int)entry.EntryPointer];
                        continue;
                    }

                    functionSize = 0;
                    int currentEntrySortedIndex = sortedPointers.IndexOf(entry.EntryPointer);

                    if (currentEntrySortedIndex < numberSortedPointers - 1)
                    {
                        functionSize = (int)sortedPointers[currentEntrySortedIndex + 1] - (int)entry.EntryPointer;
                    }
                    else
                    {
                        functionSize = ssf.SectionAiLists.Offset - (int)entry.EntryPointer;
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

                    claimedDataSize += functionSize;

                    aimap.Add((int)entry.EntryPointer, entry.Function);

                    Array.Copy(aidataBlock.GetDataBytes(), blockIndex, entry.Function.Data, 0, functionSize);
                }

                if (claimedDataSize > 0)
                {
                    ssf.ClaimUnrefSectionBytes(aidataBlock.Offset, claimedDataSize);
                }
            }
        }


        //private static void ParsePathLinkEntries(StageSetupFile ssf)
        //{
        //    // It's possible the path list and path table entries are empty (multiplayer maps).
        //    // But it seems there's still a "not-used" entry of sorts included in the bin file.
        //    // Check for "not used" struct s_pathLink pathlist[] (s32 path_neighbors... and s32 path_indeces) here.
        //    // The path table entries (s32 path_table...) will be checked next.
        //    if (!object.ReferenceEquals(null, ssf.SectionPathList) && ssf.SectionPathList.PathLinkEntries.Count <= 1)
        //    {
        //        var previousSectionOffset = ssf.PreviousSectionOffset(ssf.SectionPathList.Offset);

        //        // looking for filler section immediately before struct s_pathLink pathlist section.
        //        var pathListData = ssf.FillerBlocks
        //            .Where(x => x.Offset > previousSectionOffset && x.Offset < ssf.SectionPathList.Offset)
        //            .OrderByDescending(x => x.Offset)
        //            .FirstOrDefault();

        //        if (!object.ReferenceEquals(null, pathListData))
        //        {
        //            // Alright, there's a number of theoretical scenarios, but going to keep this practical for now.
        //            // It's known there aren't any ssf.PathLinkEntries (except the NULL entry), so assume
        //            // that the only item here is a { -1 } and { -1 } neighbors and indeces arrays. Throw an exception
        //            // if that's not the case and figure out what to do at that time.
        //            //
        //            // Scenario 2: there's just a NULL entry.
        //            if (pathListData.Length != 8 && pathListData.Length != 4)
        //            {
        //                // Will be claimed by the unreferenced filler block check.
        //                // throw new NotSupportedException($"Error parsing setup. The path list array doesn't reference any data points, but it appears there are entries included in the .bin file. It was assumed this is a multiplayer map and there are two [ -1 ] entries, but that assumption is not correct.");
        //                return;
        //            }

        //            var claimed = ssf.ClaimUnrefSectionBytes(pathListData.Offset, -1);

        //            UnrefSectionPathList section;

        //            if (claimed.Length == 8)
        //            {
        //                section = UnrefSectionPathList.NewUnreferencedPathLinkEntry();
        //                section.Offset = claimed.Offset;

        //                var neighborVal = BitUtility.Read32Big(claimed.GetDataBytes(), 0);
        //                var indecesVal = BitUtility.Read32Big(claimed.GetDataBytes(), 4);

        //                var entry = new SetupPathLinkEntry()
        //                {
        //                    // set order if there's more than one
        //                    Neighbors = new PathListing()
        //                    {
        //                        Ids = new List<int>() { neighborVal },
        //                    },
        //                    Indeces = new PathListing()
        //                    {
        //                        Ids = new List<int>() { indecesVal },
        //                    },
        //                };

        //                section.PathLinkEntries.Add(entry);

        //                ssf.AddSectionBefore(section, SetupSectionId.SectionPathLink);
        //            }
        //            else if (claimed.Length == 4)
        //            {
        //                section = UnrefSectionPathList.NewUnreferencedPathLinkPointer();
        //                section.Offset = claimed.Offset;

        //                var val = BitUtility.Read32Big(claimed.GetDataBytes(), 0);
        //                if (val != 0)
        //                {
        //                    throw new NotSupportedException($"Error parsing setup. The path list array doesn't reference any data points, but it appears there are entries included in the .bin file. It was assumed this was just a NULL entry, but that assumption is not correct.");
        //                }

        //                var entry = new SetupPathLinkEntry()
        //                {
        //                    IsNull = true,
        //                };

        //                section.PathLinkEntries.Add(entry);

        //                ssf.AddSectionBefore(section, SetupSectionId.SectionPathLink);
        //            }
        //        }
        //    }
        //}

        //private static void ParsePathTablesEntries(StageSetupFile ssf)
        //{
        //    // It's possible the path list and path table entries are empty (multiplayer maps).
        //    // But it seems there's still a "not-used" entry of sorts included in the bin file.
        //    // Check for "not used" entries
        //    if (!object.ReferenceEquals(null, ssf.SectionPathTables) && ssf.SectionPathTables.PathTables.Count <= 1)
        //    {
        //        var previousSectionOffset = ssf.PreviousSectionOffset(ssf.SectionPathTables.Offset);

        //        // looking for filler section immediately before struct s_pathTbl pathtbl section.
        //        var pathTableData = ssf.FillerBlocks
        //            .Where(x => x.Offset > previousSectionOffset && x.Offset < ssf.SectionPathTables.Offset)
        //            .OrderByDescending(x => x.Offset)
        //            .FirstOrDefault();

        //        if (!object.ReferenceEquals(null, pathTableData)
        //            && (pathTableData.Length != 0))
        //        {
        //            var expectedWords = 1;

        //            // See notes in the other section above.
        //            if (pathTableData.Length != (expectedWords * Config.TargetWordSize))
        //            {
        //                // Will be claimed by the unreferenced filler block check.
        //                // throw new NotSupportedException($"Error parsing setup. The path table array doesn't reference any data points, but it appears there are entries included in the .bin file. It was assumed this is a multiplayer map and there is only one [ -1 ] entry, but that assumption is not correct.");
        //                return;
        //            }

        //            var claimed = ssf.ClaimUnrefSectionBytes(pathTableData.Offset, -1);

        //            var section = UnrefSectionPathTable.NewUnreferencedSection();
        //            section.Offset = claimed.Offset;

        //            var pathTableVal = BitUtility.Read32Big(claimed.GetDataBytes(), 0);

        //            var entry = new SetupPathTableEntry()
        //            {
        //                // set order if there's more than one
        //                Entry = new PathTable()
        //                {
        //                    Ids = new List<int>() { pathTableVal },
        //                },
        //            };

        //            section.PathTables.Add(entry);

        //            ssf.AddSectionBefore(section, SetupSectionId.SectionPathTable);
        //        }
        //    }
        //}

        //private static void ParsePathSetsEntries(StageSetupFile ssf)
        //{
        //    // Check for "not used" entries
        //    if (ssf.SectionPathSets.PathSets.Count <= 1)
        //    {
        //        var previousSectionOffset = ssf.PreviousSectionOffset(ssf.SectionPathSets.Offset);

        //        // looking for filler section immediately before struct s_pathSet paths section.
        //        var pathSetsData = ssf.FillerBlocks
        //            .Where(x => x.Offset > previousSectionOffset && x.Offset < ssf.SectionPathSets.Offset)
        //            .OrderByDescending(x => x.Offset)
        //            .FirstOrDefault();

        //        if (!object.ReferenceEquals(null, pathSetsData)
        //            && (pathSetsData.Length != 0))
        //        {
        //            var expectedWords = 1;

        //            // See notes in the other section above.
        //            if (pathSetsData.Length != (expectedWords * Config.TargetWordSize))
        //            {
        //                // Will be claimed by the unreferenced filler block check.
        //                // throw new NotSupportedException($"Error parsing setup. The path sets array doesn't reference any data points, but it appears there are entries included in the .bin file. It was assumed this is a multiplayer map and there is only one [ -1 ] entry, but that assumption is not correct.");
        //                return;
        //            }

        //            var claimed = ssf.ClaimUnrefSectionBytes(pathSetsData.Offset, -1);

        //            var section = UnrefSectionPathSets.NewUnreferencedSection();
        //            section.Offset = claimed.Offset;

        //            var pathTableVal = BitUtility.Read32Big(claimed.GetDataBytes(), 0);

        //            var entry = new SetupPathSetEntry()
        //            {
        //                // set order if there's more than one
        //                Entry = new PathSet()
        //                {
        //                    Ids = new List<int>() { pathTableVal },
        //                },
        //            };

        //            section.PathSets.Add(entry);

        //            ssf.AddSectionBefore(section, SetupSectionId.SectionPathSets);
        //        }
        //    }
        //}

        private static void ParseUnrefAiListData(StageSetupFile ssf)
        {
            // "0x04" marks the end of an entry, and these are byte arrays, so "0x04..." (pad to 1 word) is the "not used" entry.
            if (ssf.SectionAiLists.AiLists.Count <= 1)
            {
                var previousSectionOffset = ssf.PreviousSectionOffset(ssf.SectionAiLists.Offset);

                // looking for filler section immediately before AI List section.
                var ailistData = ssf.FillerBlocks
                    .Where(x => x.Offset > previousSectionOffset && x.Offset < ssf.SectionAiLists.Offset)
                    .OrderByDescending(x => x.Offset)
                    .FirstOrDefault();

                if (!object.ReferenceEquals(null, ailistData))
                {
                    // See notes in the other section above.
                    // Note: AI entries are word aligned, but this should just be the byte "0x04" padded out to one word.
                    if (ailistData.Length != 4)
                    {
                        throw new NotSupportedException($"Error parsing setup. The AI List array doesn't reference any AI functions (byte arrays), but it appears there are entries included in the .bin file. It was assumed this is a multiplayer map and there is only one entry (of size 1 word), but that assumption is not correct.");
                    }

                    var claimed = ssf.ClaimUnrefSectionBytes(ailistData.Offset, -1);

                    var section = UnrefSectionAiFunction.NewUnreferencedSection();
                    section.Offset = claimed.Offset;

                    var entry = new SetupAiListEntry()
                    {
                        Function = new AiFunction()
                        {
                            // set order if there's more than one
                            Data = claimed.GetDataBytes(),
                        },
                    };

                    section.AiLists.Add(entry);

                    ssf.AddSectionBefore(section, SetupSectionId.SectionAiList);
                }
            }
        }

        // Needs to happen after looking for section data entries, but before claiming all unreferenced sections.
        private static void SearchPadNames(StageSetupFile ssf)
        {
            var knownSectionOffsets = ssf.Sections.Select(x => x.Offset).ToList();

            //var unreferenced = fillerBlocks.Where(x => !knownSectionOffsets.Contains((int)x.StartPos));

            // if we do find a section, that will permute the underlying list, so start with a list
            // of ids and looked the relevant section each iteration.
            var unreferencedOffsetIds = ssf.FillerBlocks.OrderBy(x => x.Offset).Select(x => x.Offset);

            int found = 0;
            int rodataOffset = -1;
            int binSize = -1;

            if (!object.ReferenceEquals(null, ssf.Rodata))
            {
                rodataOffset = ssf.Rodata.Offset;
                binSize = ssf.Rodata.Offset + ssf.Rodata.Length;
            }
            else
            {
                return;
            }

            //foreach (var fillerSection in )
            foreach (var sectionOffset in unreferencedOffsetIds)
            {
                var fillerSection = ssf.FillerBlocks.First(x => x.Offset == sectionOffset);

                if (fillerSection.Length <= 4)
                {
                    continue;
                }

                var firstWord = BitUtility.Read32Big(fillerSection.GetDataBytes().Take(4).ToArray(), 0);
                if (firstWord == 0 || firstWord == -1 || firstWord < rodataOffset || firstWord > binSize)
                {
                    continue;
                }

                var availableBytes = ssf.BytesToNextAnySection(fillerSection.Offset, fillerSection.Length);

                var pointerCount = fillerSection.Length / Config.TargetPointerSize;
                int fillerBlockIndex = 0;

                found++;
                if (found == 1)
                {
                    // pad3dnames
                    var section = new DataSectionPad3dNames();
                    section.Offset = fillerSection.Offset;
                    section.IsUnreferenced = true;

                    if ((fillerSection.Length % Config.TargetPointerSize) != 0)
                    {
                        throw new InvalidOperationException($"Error trying to read pad3dnames. Closest match length is not word aligned.");
                    }

                    for (int i = 0; i < pointerCount; i++)
                    {
                        int pointer = BitUtility.Read32Big(fillerSection.GetDataBytes(), fillerBlockIndex);

                        if (pointer > 0)
                        {
                            var stringValue = BitUtility.ReadString(ssf.Rodata.GetDataBytes(), pointer - rodataOffset, 50);
                            section.Pad3dNames.Add(new StringPointer(fillerSection.Offset + fillerBlockIndex, stringValue));
                            fillerBlockIndex += Config.TargetPointerSize;
                        }
                        else if (pointer < 0)
                        {
                            throw new NotSupportedException($"Error reading filler block names, expecting pointer to .rodata or NULL, got [{pointer}].");
                        }
                        else
                        {
                            section.Pad3dNames.Add(new StringPointer(null));
                            fillerBlockIndex += Config.TargetPointerSize;
                            break;
                        }
                    }

                    var nextSectionIndex = ssf.Sections.FindIndex(x => x.Offset > fillerSection.Offset);
                    if (nextSectionIndex < 0)
                    {
                        nextSectionIndex = 0;
                    }

                    ssf.Sections.Insert(nextSectionIndex, section);

                    // claim the data that was just taken
                    ssf.ClaimUnrefSectionBytes(fillerSection.Offset, fillerBlockIndex);
                }
                else if (found == 2)
                {
                    // padnames
                    var section = new DataSectionPadNames();
                    section.Offset = fillerSection.Offset;
                    section.IsUnreferenced = true;

                    if ((fillerSection.Length % Config.TargetPointerSize) != 0)
                    {
                        throw new InvalidOperationException($"Error trying to read pad3dnames. Closest match length is not word aligned.");
                    }

                    for (int i = 0; i < pointerCount; i++)
                    {
                        int pointer = BitUtility.Read32Big(fillerSection.GetDataBytes(), fillerBlockIndex);

                        if (pointer > 0)
                        {
                            var stringValue = BitUtility.ReadString(ssf.Rodata.GetDataBytes(), pointer - rodataOffset, 50);
                            section.PadNames.Add(new StringPointer(fillerSection.Offset + fillerBlockIndex, stringValue));
                            fillerBlockIndex += Config.TargetPointerSize;
                        }
                        else if (pointer < 0)
                        {
                            throw new NotSupportedException($"Error reading filler block names, expecting pointer to .rodata or NULL, got [{pointer}].");
                        }
                        else
                        {
                            section.PadNames.Add(new StringPointer(null));
                            fillerBlockIndex += Config.TargetPointerSize;
                            break;
                        }
                    }

                    var nextSectionIndex = ssf.Sections.FindIndex(x => x.Offset > fillerSection.Offset);
                    if (nextSectionIndex < 0)
                    {
                        nextSectionIndex = 0;
                    }

                    ssf.Sections.Insert(nextSectionIndex, section);

                    // claim the data that was just taken
                    ssf.ClaimUnrefSectionBytes(fillerSection.Offset, fillerBlockIndex);
                }
                else
                {
                    break;
                }
            }
        }

        private static void ParseRodataSection(StageSetupFile ssf)
        {
            var lastMainSectionOffset = ssf.Sections
                .Where(x => x.IsMainSection)
                .OrderByDescending(x => x.Offset)
                .Select(x => x.Offset)
                .First();

            var rodataFiller = ssf.Sections
                .Where(x => x.TypeId == SetupSectionId.UnreferencedUnknown && x.Offset > lastMainSectionOffset)
                .Cast<UnrefSectionUnknown>()
                .OrderByDescending(x => x.Offset)
                .FirstOrDefault();

            if (!object.ReferenceEquals(null, rodataFiller))
            {
                var unknownSectionIndex = ssf.Sections.FindIndex(x => x.MetaId == rodataFiller.MetaId);
                ssf.Sections.RemoveAt(unknownSectionIndex);

                var rodataSection = new RefSectionRodata(rodataFiller.GetDataBytes());
                rodataSection.Offset = rodataFiller.Offset;

                ssf.Sections.Add(rodataSection);
            }
        }

        //private static void MarkEntryFillerBlocksTaken<T>(
        //    StageSetupFile ssf,
        //    List<Gen.Setup.FillerBlock> fillerBlocks,
        //    SetupDataSection refSection,
        //    List<T> collection,
        //    Func<List<T>, IEnumerable<int>> getPossibleEntries)
        //{
        //    if (refSection != null)
        //    {
        //        if (collection.Any())
        //        {
        //            //var possibleEntries = ssf.SectionPathTables.PathTables.Where(x => x.EntryPointer > 0).Select(x => x.EntryPointer).Take(5);
        //            var possibleEntries = getPossibleEntries(collection);
        //            foreach (var entryAddress in possibleEntries)
        //            {
        //                if (fillerBlocks.Any(x => (int)x.StartPos == entryAddress))
        //                {
        //                    var section = new RefSectionIgnore();
        //                    section.Offset = entryAddress;
        //                    ssf.Sections.Add(section);
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //}

        private static UnrefSectionUnknown ToUnrefUnknown(Gen.Setup.FillerBlock fillerBlock)
        {
            var fillerBytes = fillerBlock.Data.SelectMany(x => x).ToArray();

            var section = new UnrefSectionUnknown(fillerBytes);
            section.Offset = (int)fillerBlock.StartPos;

            return section;
        }
    }
}
