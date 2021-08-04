using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace Getools.Options
{
    [Verb("make_map", HelpText = "Generate a map from a stan and optional setup data.")]
    public class MakeMapOptions
    {
        [Option('s', "stan", Required = false, HelpText = "stan filename.")]
        public string StanFilename { get; set; }

        [Option("stan-format", Required = false, HelpText = "stan file format.", MetaValue = "STAN_FORMAT")]
        public string StanFormat { get; set; }

        [Option('o', "output-file", Required = false, HelpText = "Output filename (with extension).")]
        public string OutputFilename { get; set; }

        [Option("output-format", Required = false, HelpText = "Output format.", MetaValue = "OUT_FORMAT")]
        public string OutputFormat { get; set; }

        [Option('g', "guards", Required = false, HelpText = "Show guards.", Default = (bool)false)]
        public bool? Vsible { get; set; }

        /// <summary>
        /// Capture any remaining command line arguments here.
        /// </summary>
        [Value(0, Hidden = true)]
        public IEnumerable<string> TypoCatch { get; set; }
    }
}
