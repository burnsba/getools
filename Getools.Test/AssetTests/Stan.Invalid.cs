using System;
using System.IO;
using System.Linq;
using Getools.Lib.Converters;
using Getools.Lib.Error;
using Xunit;
using Xunit.Abstractions;

namespace Getools.Test.AssetTests
{
    public partial class Stan
    {
        private const string _filename_bin_bad_offset = "Tbg_bad_offset_stanZ.bin";
        private const string _filename_bin_no_footer = "Tbg_no_footer_stanZ.bin";
        private const string _filename_c_no_footer = "Tbg_no_footer_stanZ.c";
        private const string _filename_c_no_header = "Tbg_no_header_stanZ.c";
        private const string _filename_c_no_points = "Tbg_no_points_stanZ.c";
        private const string _filename_json_missing_format = "Tbg_missing_format_stanZ.json";
        private const string _filename_json_no_footer = "Tbg_no_footer_stanZ.json";
        private const string _filename_json_no_header = "Tbg_no_header_stanZ.json";
        private const string _filename_json_tile_without_points = "Tbg_tile_without_points_stanZ.json";

        [Fact]
        public void bin_bad_offset()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
            {
                var path = Path.Combine(_testFileDirectory, _filename_bin_bad_offset);
                var stan = StanConverters.ReadFromBinFile(path, Path.GetFileNameWithoutExtension(_filename_bin_bad_offset));
            });
        }

        [Fact]
        public void bin_no_footer()
        {
            Assert.Throws<EndOfStreamException>(() =>
            {
                var path = Path.Combine(_testFileDirectory, _filename_bin_no_footer);
                var stan = StanConverters.ReadFromBinFile(path, Path.GetFileNameWithoutExtension(_filename_bin_bad_offset));
            });
        }

        [Fact]
        public void c_no_footer()
        {
            Assert.Throws<BadFileFormatException>(() =>
            {
                var path = Path.Combine(_testFileDirectory, _filename_c_no_footer);
                var stan = StanConverters.ParseFromC(path);
            });
        }

        [Fact]
        public void c_no_header()
        {
            Assert.Throws<BadFileFormatException>(() =>
            {
                var path = Path.Combine(_testFileDirectory, _filename_c_no_header);
                var stan = StanConverters.ParseFromC(path);
            });
        }

        [Fact]
        public void c_no_points()
        {
            Assert.Throws<BadFileFormatException>(() =>
            {
                var path = Path.Combine(_testFileDirectory, _filename_c_no_points);
                var stan = StanConverters.ParseFromC(path);
            });
        }

        [Fact]
        public void json_missing_format()
        {
            Assert.Throws<BadFileFormatException>(() =>
            {
                var path = Path.Combine(_testFileDirectory, _filename_json_missing_format);
                var stan = StanConverters.ReadFromJson(path);
            });
        }

        [Fact]
        public void json_no_footer()
        {
            Assert.Throws<BadFileFormatException>(() =>
            {
                var path = Path.Combine(_testFileDirectory, _filename_json_no_footer);
                var stan = StanConverters.ReadFromJson(path);
            });
        }

        [Fact]
        public void json_no_header()
        {
            Assert.Throws<BadFileFormatException>(() =>
            {
                var path = Path.Combine(_testFileDirectory, _filename_json_no_header);
                var stan = StanConverters.ReadFromJson(path);
            });
        }

        [Fact]
        public void json_tile_without_points()
        {
            Assert.Throws<BadFileFormatException>(() =>
            {
                var path = Path.Combine(_testFileDirectory, _filename_json_tile_without_points);
                var stan = StanConverters.ReadFromJson(path);
            });
        }
    }
}
