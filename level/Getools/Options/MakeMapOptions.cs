using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace Getools.Options
{
    /// <summary>
    /// Verb to build a map from provided inputs.
    /// </summary>
    [Verb("make_map", HelpText = "Generate a map from stage data.")]
    public class MakeMapOptions : IOptionsBase, IOptionsOutputFile
    {
        /// <summary>
        /// Gets or sets stan input file name.
        /// </summary>
        [Option("stan", Required = false, HelpText = "stan filename.")]
        public string StanFilename { get; set; }

        [Option("setup", Required = false, HelpText = "setup filename.")]
        public string SetupFilename { get; set; }

        [Option("bg", Required = false, HelpText = "bg filename.")]
        public string BgFilename { get; set; }

        [Option("scale", Required = false, Default = 1.0, HelpText = "Level scale.")]
        public double LevelScale { get; set; }

        [Option("slice-z", Required = false, HelpText = "Slice stage at singular value (plane), perpendicular offset from ground (internal y value).")]
        public double? SlizeZ { get; set; }

        [Option("min-z", Required = false, HelpText = "Lower boundary of bounding box to determine points of interest (internal y value).")]
        public double? ZMin { get; set; }

        [Option("max-z", Required = false, HelpText = "Upper boundary of bounding box to determine points of interest (internal y value).")]
        public double? ZMax { get; set; }

        /// <summary>
        /// Gets or sets output file name.
        /// </summary>
        [Option('o', "output-file", Required = false, HelpText = "Output filename (with extension).")]
        public string OutputFilename { get; set; }

        /// <summary>
        /// Capture any remaining command line arguments here.
        /// </summary>
        [Value(0, Hidden = true)]
        public IEnumerable<string> TypoCatch { get; set; }

        /// <summary>
        /// Gets or sets strongly typed input file type.
        /// </summary>
        public Getools.Lib.Game.FileType SetupFileType { get; set; }

        /// <summary>
        /// Gets or sets strongly typed input struct format.
        /// </summary>
        public Getools.Lib.Game.TypeFormat SetupTypeFormat { get; set; }

        /// <summary>
        /// Gets or sets strongly typed combined input data format (file+struct).
        /// </summary>
        public Getools.Lib.Game.DataFormats SetupDataFormat { get; set; }

        /// <summary>
        /// Gets or sets strongly typed input file type.
        /// </summary>
        public Getools.Lib.Game.FileType StanFileType { get; set; }

        /// <summary>
        /// Gets or sets strongly typed input struct format.
        /// </summary>
        public Getools.Lib.Game.TypeFormat StanTypeFormat { get; set; }

        /// <summary>
        /// Gets or sets strongly typed combined input data format (file+struct).
        /// </summary>
        public Getools.Lib.Game.DataFormats StanDataFormat { get; set; }

        /// <summary>
        /// Gets or sets strongly typed input file type.
        /// </summary>
        public Getools.Lib.Game.FileType BgFileType { get; set; }

        /// <summary>
        /// Gets or sets strongly typed input struct format.
        /// </summary>
        public Getools.Lib.Game.TypeFormat BgTypeFormat { get; set; }

        /// <summary>
        /// Gets or sets strongly typed combined input data format (file+struct).
        /// </summary>
        public Getools.Lib.Game.DataFormats BgDataFormat { get; set; }
    }
}
