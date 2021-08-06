using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace Getools.Options
{
    public abstract class ConvertOptionsBase
    {
        [Option('i', "input-file", Required = true, HelpText = "Input filename.")]
        public string InputFilename { get; set; }

        [Option("input-file-type", Required = false, HelpText = "Describes file type, such as whether this is a binary file or json. Attempts to guess the format based on file extension if not set.", MetaValue = "FTYPE")]
        public string InputFileTypeString { get; set; }

        [Option('o', "output-file", Required = true, HelpText = "Output filename.")]
        public string OutputFilename { get; set; }

        [Option("output-file-type", Required = false, HelpText = "Describes file type, such as whether this is a binary file or json. Attempts to guess the format based on file extension if not set.", MetaValue = "FTYPE")]
        public string OutputFileTypeString { get; set; }

        [Option('d', "dname", Required = false, HelpText = "Container object declaration name, used when converting to code/source. Defaults to input filename without extension if not set.")]
        public string DeclarationName { get; set; }

        [Option("input-data-is-beta", Required = false, Default = false, HelpText = "Flag for input to use beta data structures/formats.")]
        public bool? InputDataFormatIsBeta { get; set; }

        [Option("output-data-is-beta", Required = false, Default = false, HelpText = "Flag for output to use beta data structures/formats.")]
        public bool? OutputDataFormatIsBeta { get; set; }

        /// <summary>
        /// Capture any remaining command line arguments here.
        /// </summary>
        [Value(0, Hidden = true)]
        public IEnumerable<string> TypoCatch { get; set; }

        public Getools.Lib.Game.FileType InputFileType { get; set; }
        public Getools.Lib.Game.FileType OutputFileType { get; set; }

        public Getools.Lib.Game.TypeFormat InputTypeFormat { get; set; }
        public Getools.Lib.Game.TypeFormat OutputTypeFormat { get; set; }

        public Getools.Lib.Game.DataFormats InputDataFormat { get; set; }
        public Getools.Lib.Game.DataFormats OutputDataFormat { get; set; }
    }
}
