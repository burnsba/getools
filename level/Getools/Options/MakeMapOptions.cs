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
    [Verb("make_map", HelpText = "Generate a map from a stan and optional setup data.")]
    public class MakeMapOptions
    {
        /// <summary>
        /// Gets or sets stan input file name.
        /// </summary>
        [Option('s', "stan", Required = false, HelpText = "stan filename.")]
        public string StanFilename { get; set; }

        /// <summary>
        /// Gets or sets stan input file type. Will be resolved to <see cref="StanFileType"/>.
        /// </summary>
        [Option("stan-file-type", Required = false, HelpText = "Describes file type, such as whether this is a binary file or json. Attempts to guess the format based on file extension if not set.", MetaValue = "FTYPE")]
        public string StanFileTypeString { get; set; }

        /// <summary>
        /// Gets or sets output file name.
        /// </summary>
        [Option('o', "output-file", Required = false, HelpText = "Output filename (with extension).")]
        public string OutputFilename { get; set; }

        /// <summary>
        /// Gets or sets output file type. Will be resolved to <see cref="OutputFileType"/>.
        /// </summary>
        [Option("output-file-type", Required = false, HelpText = "Describes file type, such as whether this is a binary file or json. Attempts to guess the format based on file extension if not set.", MetaValue = "FTYPE")]
        public string OutputFileTypeString { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show guards on the output map.
        /// </summary>
        [Option('g', "guards", Required = false, HelpText = "Show guards.", Default = (bool)false)]
        public bool? GuardsVisible { get; set; }

        /// <summary>
        /// Capture any remaining command line arguments here.
        /// </summary>
        [Value(0, Hidden = true)]
        public IEnumerable<string> TypoCatch { get; set; }

        /// <summary>
        /// Gets or sets strongly typed stan input file type.
        /// </summary>
        public Getools.Lib.Game.FileType StanFileType { get; set; }

        /// <summary>
        /// Gets or sets strongly typed output file type.
        /// </summary>
        public Getools.Lib.Game.FileType OutputFileType { get; set; }
    }
}
