using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Getools.Lib.Converters;
using Getools.Lib.Game.Enums;
using Getools.Lib.Kaitai;
using Getools.Lib.Kaitai.Gen;
using Xunit;
using Xunit.Abstractions;

namespace Getools.Test.AssetTests
{
    public partial class Ramrom
    {
        private const string _filename_bin = "demo.bin";

        private const string _testFileDirectory = "../../../TestFiles/ramrom";

        private readonly ITestOutputHelper _testOutputHelper;

        public Ramrom(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void ParseBin()
        {
            var path = Path.Combine(_testFileDirectory, _filename_bin);
            var ramromFile = RamromParser.ParseBin(path);

            Assert.NotNull(ramromFile);

            Assert.Equal((ulong)0x0001020304050607, ramromFile.RandomSeed);
            Assert.Equal((ulong)0x1a2a3a4a5a6a7a8a, ramromFile.Randomizer);
            Assert.Equal(LevelId.Runway, ramromFile.LevelId);
            Assert.Equal(Difficulty.SecretAgent, ramromFile.Difficulty);
            Assert.Equal(2, ramromFile.SizeCommands);

            Assert.NotNull(ramromFile.SaveData);

            Assert.Equal((uint)0x01020304, ramromFile.SaveData.Checksum1);
            Assert.Equal((uint)0x1a2a3a4a, ramromFile.SaveData.Checksum2);
            Assert.Equal(0x01, ramromFile.SaveData.CompletionBitflags);
            Assert.Equal(0x02, ramromFile.SaveData.Flag007);
            Assert.Equal(0xFF, ramromFile.SaveData.MusicVolume);
            Assert.Equal(0xFE, ramromFile.SaveData.SfxVolume);
            Assert.Equal(0x0123, ramromFile.SaveData.Options);
            Assert.Equal(0x01, ramromFile.SaveData.UnlockedCheats1);
            Assert.Equal(0x02, ramromFile.SaveData.UnlockedCheats2);
            Assert.Equal(0x03, ramromFile.SaveData.UnlockedCheats3);
            Assert.Equal(0xee, ramromFile.SaveData.Padding);

            Assert.NotNull(ramromFile.SaveData.Times);
            Assert.Equal(19 * 4, ramromFile.SaveData.Times.Length);

            int index = 0;
            for (int i = 0; i < 4; i++)
            {
                int startVal = (i * 16) + 1;

                for (int j = 0; j < 19; j++)
                {
                    Assert.Equal(startVal, ramromFile.SaveData.Times[index]);

                    startVal++;
                    index++;
                }
            }

            Assert.Equal((ushort)0xffff, (ushort)ramromFile.Padding);
            Assert.Equal(0x01020304, ramromFile.TotalTimeMs);
            Assert.Equal(0x05667788, ramromFile.FileSize);
            Assert.Equal(GameMode.Multi, ramromFile.Mode);
            Assert.Equal((uint)1, ramromFile.SlotNumber);
            Assert.Equal((uint)1, ramromFile.NumberPlayers);
            Assert.Equal((uint)1, ramromFile.Scenario);
            Assert.Equal((uint)1, ramromFile.MultiplayerStageSel);
            Assert.Equal((uint)2, ramromFile.GameLength);
            Assert.Equal((uint)11, ramromFile.MultiplayerWeaponSet);

            Assert.NotNull(ramromFile.MultiplayerChar);
            Assert.Equal(4, ramromFile.MultiplayerChar.Length);

            for (int i = 0; i < 4; i++)
            {
                Assert.Equal((uint)0xffffffff, ramromFile.MultiplayerChar[i]);
            }

            Assert.NotNull(ramromFile.MultiplayerHandicap);
            Assert.Equal(4, ramromFile.MultiplayerHandicap.Length);

            for (int i = 0; i < 4; i++)
            {
                Assert.Equal((uint)1, ramromFile.MultiplayerHandicap[i]);
            }

            Assert.NotNull(ramromFile.MultiplayerControlStyle);
            Assert.Equal(4, ramromFile.MultiplayerControlStyle.Length);

            for (int i = 0; i < 4; i++)
            {
                Assert.Equal((uint)2, ramromFile.MultiplayerControlStyle[i]);
            }

            Assert.Equal((uint)3, ramromFile.AimOption);

            Assert.NotNull(ramromFile.MultiplayerFlags);
            Assert.Equal(4, ramromFile.MultiplayerFlags.Length);

            for (int i = 0; i < 4; i++)
            {
                Assert.Equal((uint)4, ramromFile.MultiplayerFlags[i]);
            }

            Assert.Equal((uint)0xffffffff, ramromFile.Padding2);

            Assert.NotNull(ramromFile.Iterations);
            Assert.True(1 == ramromFile.Iterations.Count);

            byte seedStart = 2;
            byte randomSeed = 0xf2;
            byte checksum = 0x27;
            for (int iterationIndex = 0; iterationIndex <  ramromFile.Iterations.Count; iterationIndex++)
            {
                var iter = ramromFile.Iterations[iterationIndex];

                Assert.Equal(seedStart, iter.Head.SpeedFrames);
                Assert.Equal(2, iter.Head.Count);
                Assert.Equal(randomSeed, iter.Head.RandomSeed);
                Assert.Equal(checksum, iter.Head.Check);

                Assert.NotNull(iter.Blocks);
                int expectedSize = ramromFile.SizeCommands * iter.Head.Count;
                Assert.Equal(expectedSize, iter.Blocks.Count);

                int stickStart = seedStart + 1;
                int buttonStart = 0xff;
                for (int i = 0; i < expectedSize; i++)
                {
                    Assert.Equal(stickStart, iter.Blocks[i].StickX);
                    Assert.Equal(stickStart, iter.Blocks[i].StickY);
                    Assert.Equal(buttonStart, iter.Blocks[i].ButtonLow);
                    Assert.Equal(buttonStart, iter.Blocks[i].ButtonHigh);

                    stickStart++;
                    buttonStart--;
                }
            }
        }
    }
}
