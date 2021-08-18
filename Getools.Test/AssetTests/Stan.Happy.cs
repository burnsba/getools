using System;
using System.IO;
using System.Linq;
using Getools.Lib.Converters;
using Xunit;
using Xunit.Abstractions;

namespace Getools.Test.AssetTests
{
    public partial class Stan
    {
        private const string _filename_c = "Tbg_test_all_p_stanZ.c";
        private const string _filename_json = "Tbg_test_all_p_stanZ.json";
        private const string _filename_bin = "Tbg_test_all_p_stanZ.bin";
        private const string _filename_betac = "Tbg_testbeta_all_p_stanZ.c";
        private const string _filename_betajson = "Tbg_testbeta_all_p_stanZ.json";
        private const string _filename_betabin = "Tbg_testbeta_all_p_stanZ.bin";

        private const string _testFileDirectory = "../../../TestFiles/stan";

        private const int _c_lineSkip = 8;

        private readonly ITestOutputHelper _testOutputHelper;

        public Stan(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Convert_c_to_bin()
        {
            var path = Path.Combine(_testFileDirectory, _filename_c);
            var referenceFilePath = Path.Combine(_testFileDirectory, _filename_bin);
            var stan = StanConverters.ParseFromC(path);
            string outfile = "z" + Guid.NewGuid().ToString("n");

            _testOutputHelper.WriteLine($"input: {path}");
            _testOutputHelper.WriteLine($"reference: {referenceFilePath}");
            _testOutputHelper.WriteLine($"output: {outfile}");

            StanConverters.WriteToBin(stan, outfile);

            string actualHash = Utility.SHA256CheckSum(outfile);
            string expectedHash = Utility.SHA256CheckSum(referenceFilePath);

            Assert.Equal(expectedHash, actualHash);

            File.Delete(outfile);
        }

        [Fact]
        public void Convert_c_to_json()
        {
            var path = Path.Combine(_testFileDirectory, _filename_c);
            var referenceFilePath = Path.Combine(_testFileDirectory, _filename_json);
            var stan = StanConverters.ParseFromC(path);
            string outfile = "z" + Guid.NewGuid().ToString("n");

            _testOutputHelper.WriteLine($"input: {path}");
            _testOutputHelper.WriteLine($"reference: {referenceFilePath}");
            _testOutputHelper.WriteLine($"output: {outfile}");

            StanConverters.WriteToJson(stan, outfile);

            string actualHash = Utility.SHA256CheckSum(outfile);
            string expectedHash = Utility.SHA256CheckSum(referenceFilePath);

            Assert.Equal(expectedHash, actualHash);

            File.Delete(outfile);
        }

        [Fact]
        public void Convert_c_to_c()
        {
            var path = Path.Combine(_testFileDirectory, _filename_c);
            var referenceFilePath = Path.Combine(_testFileDirectory, _filename_c);
            var stan = StanConverters.ParseFromC(path);
            string outfile = "z" + Guid.NewGuid().ToString("n");

            _testOutputHelper.WriteLine($"input: {path}");
            _testOutputHelper.WriteLine($"reference: {referenceFilePath}");
            _testOutputHelper.WriteLine($"output: {outfile}");

            StanConverters.WriteToC(stan, outfile);

            var expectedLines = File.ReadLines(referenceFilePath).ToList();
            var actualLines = File.ReadLines(outfile).ToList();

            // need to skip auto generated header section
            AssertHelpers.AssertStringListsEqual(expectedLines, actualLines, skip:_c_lineSkip);

            File.Delete(outfile);
        }

        [Fact]
        public void Convert_bin_to_bin()
        {
            var path = Path.Combine(_testFileDirectory, _filename_bin);
            var referenceFilePath = Path.Combine(_testFileDirectory, _filename_bin);
            var stan = StanConverters.ReadFromBinFile(path, Path.GetFileNameWithoutExtension(_filename_bin));
            string outfile = "z" + Guid.NewGuid().ToString("n");

            _testOutputHelper.WriteLine($"input: {path}");
            _testOutputHelper.WriteLine($"reference: {referenceFilePath}");
            _testOutputHelper.WriteLine($"output: {outfile}");

            StanConverters.WriteToBin(stan, outfile);

            string actualHash = Utility.SHA256CheckSum(outfile);
            string expectedHash = Utility.SHA256CheckSum(referenceFilePath);

            Assert.Equal(expectedHash, actualHash);

            File.Delete(outfile);
        }

        [Fact]
        public void Convert_bin_to_json()
        {
            var path = Path.Combine(_testFileDirectory, _filename_bin);
            var referenceFilePath = Path.Combine(_testFileDirectory, _filename_json);
            var stan = StanConverters.ReadFromBinFile(path, Path.GetFileNameWithoutExtension(_filename_bin));
            string outfile = "z" + Guid.NewGuid().ToString("n");

            _testOutputHelper.WriteLine($"input: {path}");
            _testOutputHelper.WriteLine($"reference: {referenceFilePath}");
            _testOutputHelper.WriteLine($"output: {outfile}");

            StanConverters.WriteToJson(stan, outfile);

            string actualHash = Utility.SHA256CheckSum(outfile);
            string expectedHash = Utility.SHA256CheckSum(referenceFilePath);

            Assert.Equal(expectedHash, actualHash);

            File.Delete(outfile);
        }

        [Fact]
        public void Convert_bin_to_c()
        {
            var path = Path.Combine(_testFileDirectory, _filename_bin);
            var referenceFilePath = Path.Combine(_testFileDirectory, _filename_c);
            var stan = StanConverters.ReadFromBinFile(path, Path.GetFileNameWithoutExtension(_filename_bin));
            string outfile = "z" + Guid.NewGuid().ToString("n");

            _testOutputHelper.WriteLine($"input: {path}");
            _testOutputHelper.WriteLine($"reference: {referenceFilePath}");
            _testOutputHelper.WriteLine($"output: {outfile}");

            StanConverters.WriteToC(stan, outfile);

            var expectedLines = File.ReadLines(referenceFilePath).ToList();
            var actualLines = File.ReadLines(outfile).ToList();

            // need to skip auto generated header section
            AssertHelpers.AssertStringListsEqual(expectedLines, actualLines, skip: 8);

            File.Delete(outfile);
        }

        [Fact]
        public void Convert_json_to_bin()
        {
            var path = Path.Combine(_testFileDirectory, _filename_json);
            var referenceFilePath = Path.Combine(_testFileDirectory, _filename_bin);
            var stan = StanConverters.ReadFromJson(path);
            string outfile = "z" + Guid.NewGuid().ToString("n");

            _testOutputHelper.WriteLine($"input: {path}");
            _testOutputHelper.WriteLine($"reference: {referenceFilePath}");
            _testOutputHelper.WriteLine($"output: {outfile}");

            StanConverters.WriteToBin(stan, outfile);

            string actualHash = Utility.SHA256CheckSum(outfile);
            string expectedHash = Utility.SHA256CheckSum(referenceFilePath);

            Assert.Equal(expectedHash, actualHash);

            File.Delete(outfile);
        }

        [Fact]
        public void Convert_json_to_json()
        {
            var path = Path.Combine(_testFileDirectory, _filename_json);
            var referenceFilePath = Path.Combine(_testFileDirectory, _filename_json);
            var stan = StanConverters.ReadFromJson(path);
            string outfile = "z" + Guid.NewGuid().ToString("n");

            _testOutputHelper.WriteLine($"input: {path}");
            _testOutputHelper.WriteLine($"reference: {referenceFilePath}");
            _testOutputHelper.WriteLine($"output: {outfile}");

            StanConverters.WriteToJson(stan, outfile);

            string actualHash = Utility.SHA256CheckSum(outfile);
            string expectedHash = Utility.SHA256CheckSum(referenceFilePath);

            Assert.Equal(expectedHash, actualHash);

            File.Delete(outfile);
        }

        [Fact]
        public void Convert_json_to_c()
        {
            var path = Path.Combine(_testFileDirectory, _filename_json);
            var referenceFilePath = Path.Combine(_testFileDirectory, _filename_c);
            var stan = StanConverters.ReadFromJson(path);
            string outfile = "z" + Guid.NewGuid().ToString("n");

            _testOutputHelper.WriteLine($"input: {path}");
            _testOutputHelper.WriteLine($"reference: {referenceFilePath}");
            _testOutputHelper.WriteLine($"output: {outfile}");

            StanConverters.WriteToC(stan, outfile);

            var expectedLines = File.ReadLines(referenceFilePath).ToList();
            var actualLines = File.ReadLines(outfile).ToList();

            // need to skip auto generated header section
            AssertHelpers.AssertStringListsEqual(expectedLines, actualLines, skip: 8);

            File.Delete(outfile);
        }

        [Fact]
        public void Convert_betac_to_betabin()
        {
            var path = Path.Combine(_testFileDirectory, _filename_betac);
            var referenceFilePath = Path.Combine(_testFileDirectory, _filename_betabin);
            var stan = StanConverters.ParseFromBetaC(path);
            string outfile = "z" + Guid.NewGuid().ToString("n");

            _testOutputHelper.WriteLine($"input: {path}");
            _testOutputHelper.WriteLine($"reference: {referenceFilePath}");
            _testOutputHelper.WriteLine($"output: {outfile}");

            StanConverters.WriteToBetaBin(stan, outfile);

            string actualHash = Utility.SHA256CheckSum(outfile);
            string expectedHash = Utility.SHA256CheckSum(referenceFilePath);

            Assert.Equal(expectedHash, actualHash);

            File.Delete(outfile);
        }

        [Fact]
        public void Convert_betac_to_betajson()
        {
            var path = Path.Combine(_testFileDirectory, _filename_betac);
            var referenceFilePath = Path.Combine(_testFileDirectory, _filename_betajson);
            var stan = StanConverters.ParseFromBetaC(path);
            string outfile = "z" + Guid.NewGuid().ToString("n");

            _testOutputHelper.WriteLine($"input: {path}");
            _testOutputHelper.WriteLine($"reference: {referenceFilePath}");
            _testOutputHelper.WriteLine($"output: {outfile}");

            StanConverters.WriteToJson(stan, outfile);

            string actualHash = Utility.SHA256CheckSum(outfile);
            string expectedHash = Utility.SHA256CheckSum(referenceFilePath);

            Assert.Equal(expectedHash, actualHash);

            File.Delete(outfile);
        }

        [Fact]
        public void Convert_betac_to_betac()
        {
            var path = Path.Combine(_testFileDirectory, _filename_betac);
            var referenceFilePath = Path.Combine(_testFileDirectory, _filename_betac);
            var stan = StanConverters.ParseFromBetaC(path);
            string outfile = "z" + Guid.NewGuid().ToString("n");

            _testOutputHelper.WriteLine($"input: {path}");
            _testOutputHelper.WriteLine($"reference: {referenceFilePath}");
            _testOutputHelper.WriteLine($"output: {outfile}");

            StanConverters.WriteToBetaC(stan, outfile);

            var expectedLines = File.ReadLines(referenceFilePath).ToList();
            var actualLines = File.ReadLines(outfile).ToList();

            // need to skip auto generated header section
            AssertHelpers.AssertStringListsEqual(expectedLines, actualLines, skip: _c_lineSkip);

            File.Delete(outfile);
        }

        [Fact]
        public void Convert_betabin_to_betabin()
        {
            var path = Path.Combine(_testFileDirectory, _filename_betabin);
            var referenceFilePath = Path.Combine(_testFileDirectory, _filename_betabin);
            var stan = StanConverters.ReadFromBetaBinFile(path, Path.GetFileNameWithoutExtension(_filename_betabin));
            string outfile = "z" + Guid.NewGuid().ToString("n");

            _testOutputHelper.WriteLine($"input: {path}");
            _testOutputHelper.WriteLine($"reference: {referenceFilePath}");
            _testOutputHelper.WriteLine($"output: {outfile}");

            StanConverters.WriteToBetaBin(stan, outfile);

            string actualHash = Utility.SHA256CheckSum(outfile);
            string expectedHash = Utility.SHA256CheckSum(referenceFilePath);

            Assert.Equal(expectedHash, actualHash);

            File.Delete(outfile);
        }

        [Fact]
        public void Convert_betabin_to_betajson()
        {
            var path = Path.Combine(_testFileDirectory, _filename_betabin);
            var referenceFilePath = Path.Combine(_testFileDirectory, _filename_betajson);
            var stan = StanConverters.ReadFromBetaBinFile(path, Path.GetFileNameWithoutExtension(_filename_betabin));
            string outfile = "z" + Guid.NewGuid().ToString("n");

            _testOutputHelper.WriteLine($"input: {path}");
            _testOutputHelper.WriteLine($"reference: {referenceFilePath}");
            _testOutputHelper.WriteLine($"output: {outfile}");

            StanConverters.WriteToJson(stan, outfile);

            string actualHash = Utility.SHA256CheckSum(outfile);
            string expectedHash = Utility.SHA256CheckSum(referenceFilePath);

            Assert.Equal(expectedHash, actualHash);

            File.Delete(outfile);
        }

        [Fact]
        public void Convert_betabin_to_betac()
        {
            var path = Path.Combine(_testFileDirectory, _filename_betabin);
            var referenceFilePath = Path.Combine(_testFileDirectory, _filename_betac);
            var stan = StanConverters.ReadFromBetaBinFile(path, Path.GetFileNameWithoutExtension(_filename_betabin));
            string outfile = "z" + Guid.NewGuid().ToString("n");

            _testOutputHelper.WriteLine($"input: {path}");
            _testOutputHelper.WriteLine($"reference: {referenceFilePath}");
            _testOutputHelper.WriteLine($"output: {outfile}");

            StanConverters.WriteToBetaC(stan, outfile);

            var expectedLines = File.ReadLines(referenceFilePath).ToList();
            var actualLines = File.ReadLines(outfile).ToList();

            // need to skip auto generated header section
            AssertHelpers.AssertStringListsEqual(expectedLines, actualLines, skip: 8);

            File.Delete(outfile);
        }

        [Fact]
        public void Convert_betajson_to_betabin()
        {
            var path = Path.Combine(_testFileDirectory, _filename_betajson);
            var referenceFilePath = Path.Combine(_testFileDirectory, _filename_betabin);
            var stan = StanConverters.ReadFromJson(path);
            string outfile = "z" + Guid.NewGuid().ToString("n");

            _testOutputHelper.WriteLine($"input: {path}");
            _testOutputHelper.WriteLine($"reference: {referenceFilePath}");
            _testOutputHelper.WriteLine($"output: {outfile}");

            StanConverters.WriteToBetaBin(stan, outfile);

            string actualHash = Utility.SHA256CheckSum(outfile);
            string expectedHash = Utility.SHA256CheckSum(referenceFilePath);

            Assert.Equal(expectedHash, actualHash);

            File.Delete(outfile);
        }

        [Fact]
        public void Convert_betajson_to_betajson()
        {
            var path = Path.Combine(_testFileDirectory, _filename_betajson);
            var referenceFilePath = Path.Combine(_testFileDirectory, _filename_betajson);
            var stan = StanConverters.ReadFromJson(path);
            string outfile = "z" + Guid.NewGuid().ToString("n");

            _testOutputHelper.WriteLine($"input: {path}");
            _testOutputHelper.WriteLine($"reference: {referenceFilePath}");
            _testOutputHelper.WriteLine($"output: {outfile}");

            StanConverters.WriteToJson(stan, outfile);

            string actualHash = Utility.SHA256CheckSum(outfile);
            string expectedHash = Utility.SHA256CheckSum(referenceFilePath);

            Assert.Equal(expectedHash, actualHash);

            File.Delete(outfile);
        }

        [Fact]
        public void Convert_betajson_to_betac()
        {
            var path = Path.Combine(_testFileDirectory, _filename_betajson);
            var referenceFilePath = Path.Combine(_testFileDirectory, _filename_betac);
            var stan = StanConverters.ReadFromJson(path);
            string outfile = "z" + Guid.NewGuid().ToString("n");

            _testOutputHelper.WriteLine($"input: {path}");
            _testOutputHelper.WriteLine($"reference: {referenceFilePath}");
            _testOutputHelper.WriteLine($"output: {outfile}");

            StanConverters.WriteToBetaC(stan, outfile);

            var expectedLines = File.ReadLines(referenceFilePath).ToList();
            var actualLines = File.ReadLines(outfile).ToList();

            // need to skip auto generated header section
            AssertHelpers.AssertStringListsEqual(expectedLines, actualLines, skip: 8);

            File.Delete(outfile);
        }

        [Fact]
        public void Convert_betac_to_c()
        {
            var path = Path.Combine(_testFileDirectory, _filename_betac);
            var referenceFilePath = Path.Combine(_testFileDirectory, _filename_c);
            var stan = StanConverters.ParseFromBetaC(path);
            string outfile = "z" + Guid.NewGuid().ToString("n");

            _testOutputHelper.WriteLine($"input: {path}");
            _testOutputHelper.WriteLine($"reference: {referenceFilePath}");
            _testOutputHelper.WriteLine($"output: {outfile}");

            StanConverters.WriteToC(stan, outfile);

            var expectedLines = File.ReadLines(referenceFilePath).ToList();

            // we want to use the input variable name in the header section,
            // so don't fail if that's different in reference file.
            for (int i=0; i<20; i++)
            {
                expectedLines[i] = expectedLines[i].Replace("Tbg_test_all_p_stanZ", "Tbg_testbeta_all_p_stanZ");
            }

            var actualLines = File.ReadLines(outfile).ToList();

            // need to skip auto generated header section
            AssertHelpers.AssertStringListsEqual(expectedLines, actualLines, skip: _c_lineSkip);

            File.Delete(outfile);
        }


        [Fact]
        public void Convert_betabin_to_bin()
        {
            var path = Path.Combine(_testFileDirectory, _filename_betabin);
            var referenceFilePath = Path.Combine(_testFileDirectory, _filename_bin);
            var stan = StanConverters.ReadFromBetaBinFile(path, Path.GetFileNameWithoutExtension(_filename_betabin));
            string outfile = "z" + Guid.NewGuid().ToString("n");

            _testOutputHelper.WriteLine($"input: {path}");
            _testOutputHelper.WriteLine($"reference: {referenceFilePath}");
            _testOutputHelper.WriteLine($"output: {outfile}");

            StanConverters.WriteToBin(stan, outfile);

            string actualHash = Utility.SHA256CheckSum(outfile);
            string expectedHash = Utility.SHA256CheckSum(referenceFilePath);

            Assert.Equal(expectedHash, actualHash);

            File.Delete(outfile);
        }

        [Fact]
        public void Convert_betajson_to_json()
        {
            var path = Path.Combine(_testFileDirectory, _filename_betajson);
            var referenceFilePath = Path.Combine(_testFileDirectory, _filename_json);
            var stan = StanConverters.ReadFromJson(path);
            string outfile = "z" + Guid.NewGuid().ToString("n");

            _testOutputHelper.WriteLine($"input: {path}");
            _testOutputHelper.WriteLine($"reference: {referenceFilePath}");
            _testOutputHelper.WriteLine($"output: {outfile}");

            Assert.Equal("Tbg_testbeta_all_p_stanZ", stan.Header.Name);

            // Change the name so it will match against the reference file.
            stan.Header.Name = "Tbg_test_all_p_stanZ";

            stan.SetFormat(Lib.Game.TypeFormat.Normal);
            stan.DeserializeFix();

            StanConverters.WriteToJson(stan, outfile);

            string actualHash = Utility.SHA256CheckSum(outfile);
            string expectedHash = Utility.SHA256CheckSum(referenceFilePath);

            Assert.Equal(expectedHash, actualHash);

            File.Delete(outfile);
        }

        [Fact]
        public void CheckFooterSize()
        {
            Assert.Equal(24, Getools.Lib.Game.Asset.Stan.StandFileFooter.GetDataSizeOf());
        }
    }
}
