using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Getools.Lib.Converters;
using Getools.Lib.Game.Asset.Setup;
using Xunit;
using Xunit.Abstractions;

namespace Getools.Test.AssetTests
{
    public partial class Setup
    {
        private const string _testFileDirectory = "../../../TestFiles/setup";

        private const string _filename_bin = "Usetup_testZ.bin";

        private readonly ITestOutputHelper _testOutputHelper;

        public Setup(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Read_bin_simple()
        {
            var path = Path.Combine(_testFileDirectory, _filename_bin);
            var referenceFilePath = Path.Combine(_testFileDirectory, _filename_bin);
            var setup = SetupConverters.ReadFromBinFile(path);
            string outfile = "z" + Guid.NewGuid().ToString("n");

            _testOutputHelper.WriteLine($"input: {path}");

            Assert.NotNull(setup);

            Assert.NotNull(setup.SectionPadList);
            Assert.NotNull(setup.SectionPad3dList);
            Assert.NotNull(setup.SectionObjects);
            Assert.NotNull(setup.SectionIntros);
            Assert.NotNull(setup.SectionPathList);
            Assert.NotNull(setup.SectionPad3dNames);
            Assert.NotNull(setup.SectionPathTables);
            Assert.NotNull(setup.SectionPadNames);
            Assert.NotNull(setup.SectionPathSets);
            Assert.NotNull(setup.SectionAiLists);

            Assert.Equal(2, setup.SectionPadList.PadList.Count);
            Assert.Equal(setup.SectionPadList.GetEntriesCount(), setup.SectionPadList.PadList.Count);

            Assert.Equal(2, setup.SectionPad3dList.Pad3dList.Count);
            Assert.Equal(setup.SectionPad3dList.GetEntriesCount(), setup.SectionPad3dList.Pad3dList.Count);

            Assert.Equal(2, setup.SectionObjects.Objects.Count);
            Assert.Equal(setup.SectionObjects.GetEntriesCount(), setup.SectionObjects.Objects.Count);

            Assert.Equal(2, setup.SectionIntros.Intros.Count);
            Assert.Equal(setup.SectionIntros.GetEntriesCount(), setup.SectionIntros.Intros.Count);

            Assert.Equal(2, setup.SectionPathList.PathLinkEntries.Count);
            Assert.Equal(setup.SectionPathList.GetEntriesCount(), setup.SectionPathList.PathLinkEntries.Count);

            Assert.Equal(2, setup.SectionPad3dNames.Pad3dNames.Count);
            Assert.Equal(setup.SectionPad3dNames.GetEntriesCount(), setup.SectionPad3dNames.Pad3dNames.Count);

            Assert.Equal(2, setup.SectionPathTables.PathTables.Count);
            Assert.Equal(setup.SectionPathTables.GetEntriesCount(), setup.SectionPathTables.PathTables.Count);

            Assert.Equal(2, setup.SectionPadNames.PadNames.Count);
            Assert.Equal(setup.SectionPadNames.GetEntriesCount(), setup.SectionPadNames.PadNames.Count);

            Assert.Equal(2, setup.SectionPathSets.PathSets.Count);
            Assert.Equal(setup.SectionPathSets.GetEntriesCount(), setup.SectionPathSets.PathSets.Count);

            Assert.Equal(2, setup.SectionAiLists.AiLists.Count);
            Assert.Equal(setup.SectionAiLists.GetEntriesCount(), setup.SectionAiLists.AiLists.Count);

            Assert.Single(setup.Sections.OfType<UnrefSectionAiFunction>());

            var pathLink = setup.SectionPathList.PathLinkEntries.First();
            Assert.True(pathLink.NeighborsPointer > 0);
            Assert.True(pathLink.IndexPointer > 0);
            Assert.NotNull(pathLink.Neighbors);
            Assert.NotNull(pathLink.Indeces);
            Assert.Equal(2, pathLink.Neighbors.Ids.Count);
            Assert.Equal(4, pathLink.Indeces.Ids.Count);

            var pathTable = setup.SectionPathTables.PathTables.First();
            Assert.True(pathTable.EntryPointer > 0);
            Assert.NotNull(pathTable.Entry);
            Assert.Equal(5, pathTable.Entry.Ids.Count);

            var pathSet = setup.SectionPathSets.PathSets.First();
            Assert.True(pathSet.EntryPointer > 0);
            Assert.NotNull(pathSet.Entry);
            Assert.Equal(4, pathSet.Entry.Ids.Count);

            var aifunction = setup.SectionAiLists.AiLists.First();
            Assert.True(aifunction.EntryPointer > 0);
            Assert.NotNull(aifunction.Function);
            Assert.NotNull(aifunction.Function.Data);
        }

    }
}
