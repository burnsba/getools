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
    public static class SetupParser
    {
        public static StageSetupFile ParseBin(string path)
        {
            var kaitaiObject = Gen.Setup.FromFile(path);

            var setup = Convert(kaitaiObject);

            return setup;
        }

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

            spte.Entry = new PathTable(kaitaiObject.Data.Select(x => x.Value));

            return spte;
        }

        private static SetupPathLinkEntry Convert(Gen.Setup.PathLinkEntry kaitaiObject)
        {
            var sple = new SetupPathLinkEntry();

            sple.NeighborsPointer = (int)kaitaiObject.PadNeighborOffset;
            sple.IndexPointer = (int)kaitaiObject.PadIndexOffset;

            sple.Neighbors = new PathListing(kaitaiObject.PadNeighborIds.Select(x => x.Value));
            sple.Indeces = new PathListing(kaitaiObject.PadIndexIds.Select(x => x.Value));

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
            intro.Unknown_0c = kaitaiObject.Unknown0c;
            intro.Unknown_10 = kaitaiObject.Unknown10;
            intro.Unknown_14 = kaitaiObject.Unknown14;
            intro.Unknown_18 = kaitaiObject.Unknown18;
            intro.Unknown_1c = kaitaiObject.Unknown1c;
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
            intro.Left = kaitaiObject.Left;
            intro.Right = kaitaiObject.Right;
            intro.Unknown_18 = kaitaiObject.Unknown18;

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
                case Gen.Setup.SetupObjectAircraftBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectAlarmBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectAmmoBoxBody kaitaiObjectDef:
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

                case Gen.Setup.SetupObjectEndProps kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectGuardBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectHatBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectKeyBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectObjectiveCompleteConditionBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectEndObjectiveBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectObjectiveFailConditionBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectMissionObjectiveBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectWatchMenuObjectiveBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                case Gen.Setup.SetupObjectRenameBody kaitaiObjectDef:
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

                case Gen.Setup.SetupObjectWeaponBody kaitaiObjectDef:
                    objectDef = Convert(kaitaiObjectDef);
                    break;

                default:
                    throw new InvalidOperationException($"Error parsing setup binary file. Unknown {nameof(Gen.Setup.SetupIntroRecord)} of type \"{kaitaiObject.Body.GetType().Name}\"");
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

            Array.Copy(objectDef.Data, kaitaiObject.Bytes, kaitaiObject.Bytes.Length);

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

            objectDef.Ammo9mm = kaitaiObject.Ammo9mm;
            objectDef.Ammo9mm2 = kaitaiObject.Ammo9mm2;
            objectDef.AmmoRifle = kaitaiObject.AmmoRifle;
            objectDef.AmmoShotgun = kaitaiObject.AmmoShotgun;
            objectDef.AmmoHgrenade = kaitaiObject.AmmoHgrenade;
            objectDef.AmmoRockets = kaitaiObject.AmmoRockets;
            objectDef.AmmoRemote = kaitaiObject.AmmoRemote;
            objectDef.AmmoProx = kaitaiObject.AmmoProx;
            objectDef.AmmoTimed = kaitaiObject.AmmoTimed;
            objectDef.AmmoThrowing = kaitaiObject.AmmoThrowing;
            objectDef.AmmoGlaunch = kaitaiObject.AmmoGlaunch;
            objectDef.AmmoMagnum = kaitaiObject.AmmoMagnum;
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

            Array.Copy(objectDef.Data, kaitaiObject.Bytes, kaitaiObject.Bytes.Length);

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
            objectDef.Unknown24 = kaitaiObject.Unknown24;

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
            var objectDef = new SetupObjectVehicle();

            CopyGenericObjectBaseProperties(objectDef, kaitaiObject.ObjectBase);

            if (kaitaiObject.Bytes.Length != objectDef.Data.Length)
            {
                throw new InvalidOperationException($"Error parsing setup binary file when constructing \"{nameof(SetupObjectVehicle)}\". Parsed data length ({objectDef.Data.Length}) does not match expected value ({kaitaiObject.Bytes.Length})");
            }

            Array.Copy(objectDef.Data, kaitaiObject.Bytes, kaitaiObject.Bytes.Length);

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

            spse.Entry = new PathSet(kaitaiObject.Data.Select(x => x.Value));

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
            var sp = new StringPointer()
            {
                Offset = (int)kaitaiObject.Offset,
                Value = kaitaiObject.Deref,
            };

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

            pad.NameRodataOffset = (int)pad.Name.Offset;

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

            pad.NameRodataOffset = (int)pad.Name.Offset;

            return pad;
        }
    }
}
