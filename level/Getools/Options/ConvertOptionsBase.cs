using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace Getools.Options
{
    /// <summary>
    /// Common base class for converting one data set to another.
    /// </summary>
    public abstract class ConvertOptionsBase
    {
        /// <summary>
        /// Gets or sets input file name.
        /// </summary>
        [Option('i', "input-file", Required = true, HelpText = "Input filename.")]
        public string InputFilename { get; set; }

        /// <summary>
        /// Gets or sets input file type. Will be resolved to <see cref="InputFileType"/>.
        /// </summary>
        [Option("input-file-type", Required = false, HelpText = "Describes file type, such as whether this is a binary file or json. Attempts to guess the format based on file extension if not set.", MetaValue = "FTYPE")]
        public string InputFileTypeString { get; set; }

        /// <summary>
        /// Gets or sets output file name.
        /// </summary>
        [Option('o', "output-file", Required = true, HelpText = "Output filename.")]
        public string OutputFilename { get; set; }

        /// <summary>
        /// Gets or sets output file type. Will be resolved to <see cref="OutputFileType"/>.
        /// </summary>
        [Option("output-file-type", Required = false, HelpText = "Describes file type, such as whether this is a binary file or json. Attempts to guess the format based on file extension if not set.", MetaValue = "FTYPE")]
        public string OutputFileTypeString { get; set; }

        /// <summary>
        /// Gets or sets stan header variable declaration name.
        /// </summary>
        [Option('d', "dname", Required = false, HelpText = "Container object declaration name, used when converting to code/source. Defaults to input filename without extension if not set.")]
        public string DeclarationName { get; set; }

        /// <summary>
        /// Capture any remaining command line arguments here.
        /// </summary>
        [Value(0, Hidden = true)]
        public IEnumerable<string> TypoCatch { get; set; }

        /// <summary>
        /// Gets or sets strongly typed input file type.
        /// </summary>
        public Getools.Lib.Game.FileType InputFileType { get; set; }

        /// <summary>
        /// Gets or sets strongly typed output file type.
        /// </summary>
        public Getools.Lib.Game.FileType OutputFileType { get; set; }

        /// <summary>
        /// Gets or sets strongly typed input struct format.
        /// </summary>
        public Getools.Lib.Game.TypeFormat InputTypeFormat { get; set; }

        /// <summary>
        /// Gets or sets strongly typed output struct format.
        /// </summary>
        public Getools.Lib.Game.TypeFormat OutputTypeFormat { get; set; }

        /// <summary>
        /// Gets or sets strongly typed combined input data format (file+struct).
        /// </summary>
        public Getools.Lib.Game.DataFormats InputDataFormat { get; set; }

        /// <summary>
        /// Gets or sets strongly typed combined output data format (file+struct).
        /// </summary>
        public Getools.Lib.Game.DataFormats OutputDataFormat { get; set; }
    }
}
