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

        [Option("input-format", Required = true, HelpText = "Input format.", MetaValue = "IN_FORMAT")]
        public string InputFormatString { get; set; }

        [Option('o', "output-file", Required = true, HelpText = "Output filename.")]
        public string OutputFilename { get; set; }

        [Option("output-format", Required = true, HelpText = "Output format.", MetaValue = "OUT_FORMAT")]
        public string OutputFormatString { get; set; }

        [Option('d', "dname", Required = false, HelpText = "Container object declaration name, used when converting to code/source. Defaults to input filename without extension if not set.")]
        public string DeclarationName { get; set; }
    }
}
