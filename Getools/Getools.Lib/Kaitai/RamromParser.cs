using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Getools.Lib.Error;
using Getools.Lib.Game;
using Getools.Lib.Game.Asset.Intro;
using Getools.Lib.Game.Asset.Ramrom;
using Getools.Lib.Game.Asset.SetupObject;
using Getools.Lib.Game.Asset.Stan;
using Getools.Lib.Game.Enums;
using static Getools.Lib.Kaitai.Gen.RamromReplay;

namespace Getools.Lib.Kaitai
{
    /// <summary>
    /// Wrapper for Kaitai parser generated code.
    /// </summary>
    public static class RamromParser
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
        public static RamromFile ParseBin(string path)
        {
            var kaitaiObject = Gen.RamromReplay.FromFile(path);

            var demo = Convert(kaitaiObject);

            return demo;
        }

        /// <summary>
        /// Converts Kaitai struct ramrom object to library ramrom object.
        /// </summary>
        /// <param name="kaitaiObject">Kaitai struct object.</param>
        /// <returns>Library object.</returns>
        private static RamromFile Convert(Gen.RamromReplay kaitaiObject)
        {
            var result = new RamromFile();

            result.RandomSeed = kaitaiObject.RandomSeed;
            result.Randomizer = kaitaiObject.Randomizer;
            result.LevelId = (LevelId)kaitaiObject.StageNum;
            result.Difficulty = (Difficulty)kaitaiObject.Difficulty;
            result.SizeCommands = (int)kaitaiObject.SizeCmds;
            result.Padding = (short)kaitaiObject.Padding;
            result.TotalTimeMs = kaitaiObject.TotaltimeMs;
            result.FileSize = kaitaiObject.FileSize;
            result.Mode = (GameMode)kaitaiObject.Mode;
            result.SlotNumber = kaitaiObject.SlotNum;
            result.NumberPlayers = kaitaiObject.NumPlayers;
            result.Scenario = kaitaiObject.Scenario;
            result.MultiplayerStageSel = kaitaiObject.MpstageSel;
            result.GameLength = kaitaiObject.GameLength;
            result.MultiplayerWeaponSet = kaitaiObject.MpWeaponSet;
            result.AimOption = kaitaiObject.AimOption;
            result.Padding2 = kaitaiObject.Padding2;

            result.SaveData = Convert(kaitaiObject.SaveFile);

            for (int i = 0; i < 4; i++)
            {
                result.MultiplayerChar[i] = (uint)kaitaiObject.MpChar[i];
                result.MultiplayerHandicap[i] = (uint)kaitaiObject.MpHandi[i];
                result.MultiplayerControlStyle[i] = (uint)kaitaiObject.MpContstyle[i];
                result.MultiplayerFlags[i] = (uint)kaitaiObject.MpFlags[i];
            }

            int iterationIndex = 0;
            foreach (var iter in kaitaiObject.SeqData)
            {
                var item = Convert(iter);
                item.FrameIndex = iterationIndex;

                result.Iterations.Add(item);

                iterationIndex++;
            }

            return result;
        }

        private static Getools.Lib.Game.File.SaveData Convert(Gen.RamromReplay.SaveData kaitaiObject)
        {
            var result = new Game.File.SaveData();

            result.Checksum1 = kaitaiObject.Chksum1;
            result.Checksum2 = kaitaiObject.Chksum2;
            result.CompletionBitflags = kaitaiObject.CompletionBitflags;
            result.Flag007 = kaitaiObject.Flag007;
            result.MusicVolume = kaitaiObject.MusicVol;
            result.SfxVolume = kaitaiObject.SfxVol;
            result.Options = kaitaiObject.Options;
            result.UnlockedCheats1 = kaitaiObject.UnlockedCheats1;
            result.UnlockedCheats2 = kaitaiObject.UnlockedCheats2;
            result.UnlockedCheats3 = kaitaiObject.UnlockedCheats3;
            result.Padding = kaitaiObject.Unused;

            if (result.Times.Length != kaitaiObject.Times.Count)
            {
                throw new BadFileFormatException("Count mismatch between expected number of times and serialized count");
            }

            for (int i = 0; i < result.Times.Length; i++)
            {
                result.Times[i] = kaitaiObject.Times[i];
            }

            return result;
        }

        private static Seed Convert(Gen.RamromReplay.RamromSeed kaitaiObject)
        {
            var result = new Seed();

            result.SpeedFrames = kaitaiObject.SpeedFrames;
            result.Count = kaitaiObject.Count;
            result.RandomSeed = kaitaiObject.Randseed;
            result.Check = kaitaiObject.Check;

            return result;
        }

        private static Blockbuf Convert(Gen.RamromReplay.RamromBlockbuf kaitaiObject)
        {
            var result = new Blockbuf();

            result.StickX = kaitaiObject.StickX;
            result.StickY = kaitaiObject.StickY;
            result.ButtonLow = kaitaiObject.ButtonLow;
            result.ButtonHigh = kaitaiObject.ButtonHigh;

            return result;
        }

        private static CaptureIteration Convert(Gen.RamromReplay.RamromIter kaitaiObject)
        {
            var head = Convert(kaitaiObject.Head);

            var result = new CaptureIteration(head);

            foreach (var bb in kaitaiObject.Data)
            {
                var blockbuf = Convert(bb);
                result.Blocks.Add(blockbuf);
            }

            return result;
        }
    }
}
