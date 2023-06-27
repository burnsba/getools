using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace Getools.Options
{
    /// <summary>
    /// Verb to inflate gzip compressed data.
    /// </summary>
    [Verb("unzip", HelpText = "Unzip compress gzip or Rare 1172 file")]
    public class UnzipOptions : IOptionsBase, IOptionsInputFile, IOptionsOutputFile
    {
        /// <summary>
        /// Gets or sets input file name.
        /// </summary>
        [Option('i', "input-file", Required = true, HelpText = "Input filename.")]
        public string InputFilename { get; set; }

        /// <summary>
        /// Gets or sets output file name.
        /// </summary>
        [Option('o', "output-file", Required = true, HelpText = "Output filename.")]
        public string OutputFilename { get; set; }

        /// <inheritdoc />
        [Value(0, Hidden = true)]
        public IEnumerable<string> TypoCatch { get; set; }

        [Option('t', "trace", Required = false, HelpText = "Debug level TRACE.", Default = false)]
        public bool Trace { get; set; }
    }
}
